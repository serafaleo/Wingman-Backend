using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Npgsql;
using Wingman.Api.Features.Auth.DTOs;
using Wingman.Api.Features.Auth.Helpers.Objects;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Tests.Unit.Features;

public class UsersServiceTests
{
    private readonly Mock<IUsersRepository> _mockRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly UsersService _service;

    public UsersServiceTests()
    {
        _mockRepo = new Mock<IUsersRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _service = new UsersService(_mockRepo.Object, _mockTokenService.Object);
    }

    #region SignUp

    [Fact]
    public async Task SignUp_ShouldReturnUnit_WhenSuccessful()
    {
        // Arrange
        SignUpRequestDto signUpDto = new()
        {
            Name = "Test Name",
            Email = "test@test.com",
            Password = "Password_123",
            PasswordConfirmation = "Password_123"
        };

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<User>())).ReturnsAsync(Guid.NewGuid());

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.SignUp(signUpDto);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task SignUp_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        SignUpRequestDto signUpDto = new()
        {
            Name = "Test Name",
            Email = "test@test.com",
            Password = "Password_123",
            PasswordConfirmation = "Password_123"
        };

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
            .Throws(new PostgresException(string.Empty, string.Empty, string.Empty, PostgresErrorCodes.UniqueViolation));

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.SignUp(signUpDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status409Conflict, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to create new user.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Email address already used.", result.LeftAsEnumerable().First().Detail);
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_ShouldReturnToken_WhenSuccessful()
    {
        // Arrange
        LoginRequestDto loginDto = new()
        {
            Email = "test@test.com",
            Password = "Password_123"
        };

        User user = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Name",
            Email = loginDto.Email
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, loginDto.Password);

        const string accessToken = "Some Access Token";
        const string refreshToken = "Some Refresh Token";

        _mockRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockTokenService.Setup(ts => ts.GenerateAccessToken(user)).Returns(accessToken);
        _mockTokenService.Setup(ts => ts.GenerateRefreshToken()).Returns(new RefreshToken() { Token = refreshToken, Expiration = DateTime.UtcNow.AddDays(7) });

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Login(loginDto);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(user.Id, result.RightAsEnumerable().First().UserId);
        Assert.Equal(refreshToken, result.RightAsEnumerable().First().RefreshToken);
        Assert.Equal(accessToken, result.RightAsEnumerable().First().AccessToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExists()
    {
        // Arrange
        LoginRequestDto loginDto = new()
        {
            Email = "test@test.com",
            Password = "Password_123"
        };

        _mockRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Login(loginDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Login failed.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Email or password wrong.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        LoginRequestDto loginDto = new()
        {
            Email = "test@test.com",
            Password = "Wrong Password"
        };

        User user = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test Name",
            Email = loginDto.Email
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Correct Password");

        _mockRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Login(loginDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Login failed.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Email or password wrong.", result.LeftAsEnumerable().First().Detail);
    }

    #endregion

    #region Refresh

    [Fact]
    public async Task Refresh_ShouldReturnNotFound_WhenUserDoesNotExists()
    {
        // Arrange
        RefreshRequestDto refreshDto = new()
        {
            RefreshToken = "Some Refresh Token",
            UserId = Guid.NewGuid()
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync((User?)null);

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to refresh session.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The requested User was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequest_WhenRefreshTokenInDataBaseIsNull()
    {
        // Arrange
        RefreshRequestDto refreshDto = new()
        {
            RefreshToken = "Some Refresh Token",
            UserId = Guid.NewGuid()
        };

        User user = new()
        {
            Id = refreshDto.UserId,
            Name = "Test Name",
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = null,
            RefreshTokenExpirationDateTimeUTC = null
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to refresh session.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Invalid Refresh Token.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequest_WhenRefreshTokenIsDifferentFromDatabase()
    {
        // Arrange
        RefreshRequestDto refreshDto = new()
        {
            RefreshToken = "Wrong Refresh Token",
            UserId = Guid.NewGuid()
        };

        User user = new()
        {
            Id = refreshDto.UserId,
            Name = "Test Name",
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = "Right Refresh Token",
            RefreshTokenExpirationDateTimeUTC = DateTime.UtcNow.AddDays(7)
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status400BadRequest, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to refresh session.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Invalid Refresh Token.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenRefreshTokenIsExpired()
    {
        // Arrange
        RefreshRequestDto refreshDto = new()
        {
            RefreshToken = "Some Refresh Token",
            UserId = Guid.NewGuid()
        };

        User user = new()
        {
            Id = refreshDto.UserId,
            Name = "Test Name",
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = refreshDto.RefreshToken,
            RefreshTokenExpirationDateTimeUTC = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to refresh session.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("Refresh Token is expired. A new login is necessary.", result.LeftAsEnumerable().First().Detail);
    }

    [Fact]
    public async Task Refresh_ShouldReturnNewToken_WhenSuccessful()
    {
        // Arrange
        RefreshRequestDto refreshDto = new()
        {
            RefreshToken = "Some Refresh Token",
            UserId = Guid.NewGuid()
        };

        User user = new()
        {
            Id = refreshDto.UserId,
            Name = "Test Name",
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = refreshDto.RefreshToken,
            RefreshTokenExpirationDateTimeUTC = DateTime.UtcNow.AddDays(1)
        };

        const string accessToken = "New Access Token";
        const string refreshToken = "New Refresh Token";

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);
        _mockTokenService.Setup(ts => ts.GenerateAccessToken(user)).Returns(accessToken);
        _mockTokenService.Setup(ts => ts.GenerateRefreshToken()).Returns(new RefreshToken() { Token = refreshToken, Expiration = DateTime.UtcNow.AddDays(7) });

        // Act
        Either<ProblemDetails, TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.True(result.IsRight);
        Assert.Equal(user.Id, result.RightAsEnumerable().First().UserId);
        Assert.Equal(refreshToken, result.RightAsEnumerable().First().RefreshToken);
        Assert.Equal(accessToken, result.RightAsEnumerable().First().AccessToken);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task Logout_ShouldReturnUnit_WhenSuccessful()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        User existingUser = new()
        {
            Name = "Test Name",
            Email = "test@test.com"
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.Logout(userId);

        // Assert
        Assert.True(result.IsRight);
    }

    [Fact]
    public async Task Logout_ShouldReturnNotFound_WhenUserDoesNotExists()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        _mockRepo.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        Either<ProblemDetails, LanguageExt.Unit> result = await _service.Logout(userId);

        // Assert
        Assert.True(result.IsLeft);
        Assert.Equal(StatusCodes.Status404NotFound, result.LeftAsEnumerable().First().Status);
        Assert.Equal("Failed to logout.", result.LeftAsEnumerable().First().Title);
        Assert.Equal("The requested User was not found in the server.", result.LeftAsEnumerable().First().Detail);
    }

    #endregion
}
