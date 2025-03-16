using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Aircrafts.Repositories.Interfaces;
using Wingman.Api.Features.Aircrafts.Services;

namespace Wingman.Tests.Unit;

public class AircraftsServiceTests
{
    private readonly Mock<IAircraftsRepository> _mockRepo;
    private readonly AircraftsService _service;

    public AircraftsServiceTests()
    {
        _mockRepo = new Mock<IAircraftsRepository>();
        _service = new AircraftsService(_mockRepo.Object);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnBadRequest_WhenPageIsLessThen1()
    {
        // Arrange
        int page = 0;
        int pageSize = 1;
        Guid contextUserId = Guid.NewGuid();

        // Act
        Either<ProblemDetails, List<Aircraft>> result = await _service.GetAllAsync(page, pageSize, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to get Aircrafts.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Invalid pagination parameters.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBadRequest_WhenPageSizeIsLessThen1()
    {
        // Arrange
        int page = 1;
        int pageSize = 0;
        Guid contextUserId = Guid.NewGuid();

        // Act
        Either<ProblemDetails, List<Aircraft>> result = await _service.GetAllAsync(page, pageSize, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to get Aircrafts.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Invalid pagination parameters.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAircraftList_WhenSuccessful()
    {
        // Arrange
        int page = 1;
        int pageSize = 20;
        Guid contextUserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetAllAsync(contextUserId, page, pageSize)).ReturnsAsync(new List<Aircraft>());

        // Act
        Either<ProblemDetails, List<Aircraft>> result = await _service.GetAllAsync(page, pageSize, contextUserId);

        // Assert
        Assert.True(result.IsRight);
        Assert.NotNull(result.RightAsEnumerable().First());
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenAircraftNotInDataBase()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync((Aircraft?)null);

        // Act
        Either<ProblemDetails, Aircraft> result = await _service.GetByIdAsync(id, userId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to get Aircraft ID {id}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The requested Aircraft was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnForbidden_WhenUserIDsDiffer()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = id,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = Guid.NewGuid()
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(aircraftInDataBase);

        // Act
        Either<ProblemDetails, Aircraft> result = await _service.GetByIdAsync(id, userId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status403Forbidden, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to get Aircraft ID {id}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The current user does not have permission to access this Aircraft.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAircraft_WhenSuccessful()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = id,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = userId
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(aircraftInDataBase);

        // Act
        Either<ProblemDetails, Aircraft> result = await _service.GetByIdAsync(id, userId);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(aircraftInDataBase, result.RightAsEnumerable().First());
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnGuid_WhenSuccessful()
    {
        // Arrange
        Guid createdId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        Aircraft aircraftToCreate = new()
        {
            Registration = "PP-PPP",
            TypeICAO = "C152",
        };

        _mockRepo.Setup(repo => repo.CreateAsync(aircraftToCreate)).ReturnsAsync(createdId);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.CreateAsync(aircraftToCreate, userId);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(userId, aircraftToCreate.UserId);
        Assert.Equal(createdId, aircraftToCreate.Id);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenRouteIdIsDifferentThanBodyId()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftToUpdate = new()
        {
            Id = Guid.NewGuid(),
            Registration = "PP-PPP",
            TypeICAO = "C152",
        };

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, aircraftToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update Aircraft ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Body object ID and route ID are different.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenAircraftNotInDataBase()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftToUpdate = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync((Aircraft?)null);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, aircraftToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update Aircraft ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The requested Aircraft was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnForbidden_WhenUserIDsDiffer()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = Guid.NewGuid()
        };

        Aircraft aircraftToUpdate = new()
        {
            Registration = "PP-PPP",
            TypeICAO = "C152",
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(aircraftInDataBase);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, aircraftToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status403Forbidden, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update Aircraft ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The current user does not have permission to access this Aircraft.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenBodyUserIdDiffersFromContext()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = contextUserId
        };

        Aircraft aircraftToUpdate = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = Guid.NewGuid()
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(aircraftInDataBase);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, aircraftToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update Aircraft ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Body object UserID was changed, which is not permitted.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUnit_WhenSuccessful()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = contextUserId
        };

        Aircraft aircraftToUpdate = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152"
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(aircraftInDataBase);
        _mockRepo.Setup(repo => repo.UpdateAsync(aircraftToUpdate)).ReturnsAsync(true);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, aircraftToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(aircraftToUpdate.UserId, contextUserId);
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnNotFound_WhenAircraftNotInDataBase()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync((Aircraft?)null);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.DeleteByIdAsync(routeId, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to delete Aircraft ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The requested Aircraft was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnForbidden_WhenUserIDsDiffer()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = Guid.NewGuid()
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(aircraftInDataBase);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.DeleteByIdAsync(routeId, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status403Forbidden, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to delete Aircraft ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The current user does not have permission to access this Aircraft.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnUnit_WhenSuccessful()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        Aircraft aircraftInDataBase = new()
        {
            Id = routeId,
            Registration = "PP-PPP",
            TypeICAO = "C152",
            UserId = contextUserId
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(aircraftInDataBase);
        _mockRepo.Setup(repo => repo.DeleteByIdAsync(routeId)).ReturnsAsync(true);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.DeleteByIdAsync(routeId, contextUserId);

        // Assert
        Assert.True(result.IsRight);
    }

    #endregion
}
