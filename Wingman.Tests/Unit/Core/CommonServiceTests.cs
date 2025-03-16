using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Wingman.Api.Core.Models;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services;

namespace Wingman.Tests.Unit.Core;

public abstract class CommonServiceTests<T> where T : CommonModel
{
    private readonly Mock<ICommonRepository<T>> _mockRepo;
    private readonly CommonService<T> _service;

    protected CommonServiceTests(CommonService<T> service, Mock<ICommonRepository<T>> mockRepo)
    {
        _mockRepo = mockRepo;
        _service = service;
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
        Either<ProblemDetails, List<T>> result = await _service.GetAllAsync(page, pageSize, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to get {_service.modelName}s.", result.LeftAsEnumerable().First().Title);
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
        Either<ProblemDetails, List<T>> result = await _service.GetAllAsync(page, pageSize, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to get {_service.modelName}s.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Invalid pagination parameters.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAircraftList_WhenSuccessful()
    {
        // Arrange
        int page = 1;
        int pageSize = 20;
        Guid contextUserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetAllAsync(contextUserId, page, pageSize)).ReturnsAsync(new List<T>());

        // Act
        Either<ProblemDetails, List<T>> result = await _service.GetAllAsync(page, pageSize, contextUserId);

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

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync((T?)null);

        // Act
        Either<ProblemDetails, T> result = await _service.GetByIdAsync(id, userId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to get {_service.modelName} ID {id}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal($"The requested {_service.modelName} was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnForbidden_WhenUserIDsDiffer()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = id;
        modelInDataBase.UserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(modelInDataBase);

        // Act
        Either<ProblemDetails, T> result = await _service.GetByIdAsync(id, userId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status403Forbidden, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to get {_service.modelName} ID {id}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal($"The current user does not have permission to access this {_service.modelName}.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAircraft_WhenSuccessful()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = id;
        modelInDataBase.UserId = userId;

        _mockRepo.Setup(repo => repo.GetByIdAsync(id)).ReturnsAsync(modelInDataBase);

        // Act
        Either<ProblemDetails, T> result = await _service.GetByIdAsync(id, userId);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(modelInDataBase, result.RightAsEnumerable().First());
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnGuid_WhenSuccessful()
    {
        // Arrange
        Guid createdId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        T modelToCreate = Activator.CreateInstance<T>();

        _mockRepo.Setup(repo => repo.CreateAsync(modelToCreate)).ReturnsAsync(createdId);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.CreateAsync(modelToCreate, userId);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(userId, modelToCreate.UserId);
        Assert.Equal(createdId, modelToCreate.Id);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenRouteIdIsDifferentThanBodyId()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelToUpdate = Activator.CreateInstance<T>();

        modelToUpdate.Id = Guid.NewGuid();

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, modelToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update {_service.modelName} ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Body object ID and route ID are different.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenAircraftNotInDataBase()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelToUpdate = Activator.CreateInstance<T>();

        modelToUpdate.Id = routeId;

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync((T?)null);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, modelToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update {_service.modelName} ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal($"The requested {_service.modelName} was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnForbidden_WhenUserIDsDiffer()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = routeId;
        modelInDataBase.UserId = Guid.NewGuid();

        T modelToUpdate = Activator.CreateInstance<T>();

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(modelInDataBase);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, modelToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status403Forbidden, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update {_service.modelName} ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal($"The current user does not have permission to access this {_service.modelName}.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequest_WhenBodyUserIdDiffersFromContext()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = routeId;
        modelInDataBase.UserId = contextUserId;

        T modelToUpdate = Activator.CreateInstance<T>();

        modelToUpdate.Id = routeId;
        modelToUpdate.UserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(modelInDataBase);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, modelToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to update {_service.modelName} ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Body object UserID was changed, which is not permitted.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUnit_WhenSuccessful()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = routeId;
        modelInDataBase.UserId = contextUserId;

        T modelToUpdate = Activator.CreateInstance<T>();

        modelToUpdate.Id = routeId;

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(modelInDataBase);
        _mockRepo.Setup(repo => repo.UpdateAsync(modelToUpdate)).ReturnsAsync(true);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.UpdateAsync(routeId, modelToUpdate, contextUserId);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(modelToUpdate.UserId, contextUserId);
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnNotFound_WhenAircraftNotInDataBase()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync((T?)null);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.DeleteByIdAsync(routeId, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to delete {_service.modelName} ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal($"The requested {_service.modelName} was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnForbidden_WhenUserIDsDiffer()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = routeId;
        modelInDataBase.UserId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(modelInDataBase);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.DeleteByIdAsync(routeId, contextUserId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status403Forbidden, result.LeftAsEnumerable().First().Status);
        Assert.Equal($"Failed to delete {_service.modelName} ID {routeId}.", result.LeftAsEnumerable().First().Title);
        Assert.Equal($"The current user does not have permission to access this {_service.modelName}.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnUnit_WhenSuccessful()
    {
        // Arrange
        Guid routeId = Guid.NewGuid();
        Guid contextUserId = Guid.NewGuid();

        T modelInDataBase = Activator.CreateInstance<T>();

        modelInDataBase.Id = routeId;
        modelInDataBase.UserId = contextUserId;

        _mockRepo.Setup(repo => repo.GetByIdAsync(routeId)).ReturnsAsync(modelInDataBase);
        _mockRepo.Setup(repo => repo.DeleteByIdAsync(routeId)).ReturnsAsync(true);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.DeleteByIdAsync(routeId, contextUserId);

        // Assert
        Assert.True(result.IsRight);
    }

    #endregion
}
