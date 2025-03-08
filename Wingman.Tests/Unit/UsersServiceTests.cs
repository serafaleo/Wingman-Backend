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

        const string accessToken = "AccessToken";
        const string refreshToken = "RefreshToken";

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
}
