using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Npgsql;
using Wingman.Api.Core.DTOs;
using Wingman.Api.Features.Auth.DTOs;
using Wingman.Api.Features.Auth.Helpers.Objects;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Tests.Unit;

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
    public async Task SignUp_ShouldReturnCreated_WhenSuccessful()
    {
        // Arrange
        SignUpRequestDto signUpDto = new()
        {
            Email = "test@test.com",
            Password = "Password_123",
            PasswordConfirmation = "Password_123"
        };

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<User>())).ReturnsAsync(Guid.NewGuid());

        // Act
        ApiResponseDto<object> result = await _service.SignUp(signUpDto);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.True(result.Success);
        Assert.Equal("User successfully created.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task SignUp_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        SignUpRequestDto signUpDto = new()
        {
            Email = "test@test.com",
            Password = "Password_123",
            PasswordConfirmation = "Password_123"
        };

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
            .Throws(new PostgresException(string.Empty, string.Empty, string.Empty, PostgresErrorCodes.UniqueViolation));

        // Act
        ApiResponseDto<object> result = await _service.SignUp(signUpDto);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Email address already used.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }
    #endregion

    #region Login
    [Fact]
    public async Task Login_ShouldReturnOkAndToken_WhenSuccessful()
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
            Email = loginDto.Email
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, loginDto.Password);

        const string accessToken = "Some Access Token";
        const string refreshToken = "Some Refresh Token";

        _mockRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _mockTokenService.Setup(ts => ts.GenerateAccessToken(user)).Returns(accessToken);
        _mockTokenService.Setup(ts => ts.GenerateRefreshToken()).Returns(new RefreshToken() { Token = refreshToken, Expiration = DateTime.UtcNow.AddDays(7) });

        // Act
        ApiResponseDto<TokenResponseDto> result = await _service.Login(loginDto);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.Equal("Login successful.", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data.UserId);
        Assert.Equal(refreshToken, result.Data.RefreshToken);
        Assert.Equal(accessToken, result.Data.AccessToken);
        Assert.Null(result.Errors);
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
        ApiResponseDto<TokenResponseDto> result = await _service.Login(loginDto);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Email or password wrong.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
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
            Email = loginDto.Email
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Correct Password");

        _mockRepo.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        ApiResponseDto<TokenResponseDto> result = await _service.Login(loginDto);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Email or password wrong.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
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
        ApiResponseDto<TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Refresh_ShouldReturnBadRequest_WhenRefreshTokenInDatabaseIsNull()
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
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = null,
            RefreshTokenExpirationDateTimeUTC = null
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);

        // Act
        ApiResponseDto<TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Invalid Refresh Token.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
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
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = "Right Refresh Token",
            RefreshTokenExpirationDateTimeUTC = DateTime.UtcNow.AddDays(7)
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);

        // Act
        ApiResponseDto<TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Invalid Refresh Token.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
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
            Email = "test@test.com",
            PasswordHash = "Some Hash",
            RefreshToken = refreshDto.RefreshToken,
            RefreshTokenExpirationDateTimeUTC = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepo.Setup(repo => repo.GetByIdAsync(refreshDto.UserId)).ReturnsAsync(user);

        // Act
        ApiResponseDto<TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Refresh Token is expired. A new login is necessary.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task Refresh_ShouldReturnOkAndNewToken_WhenSuccessful()
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
        ApiResponseDto<TokenResponseDto> result = await _service.Refresh(refreshDto);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.Equal("Refresh successful.", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data.UserId);
        Assert.Equal(refreshToken, result.Data.RefreshToken);
        Assert.Equal(accessToken, result.Data.AccessToken);
        Assert.Null(result.Errors);
    }

    #endregion
}
