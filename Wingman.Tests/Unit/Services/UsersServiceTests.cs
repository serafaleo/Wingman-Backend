using Microsoft.AspNetCore.Http;
using Moq;
using Npgsql;
using Wingman.Api.Core.DTOs;
using Wingman.Api.Features.Auth.DTOs;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Tests.Unit.Services;

public class UsersServiceTests
{
    private readonly Mock<IUsersRepository> _mockRepo;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly UsersService _usersService;

    public UsersServiceTests()
    {
        _mockRepo = new Mock<IUsersRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _usersService = new UsersService(_mockRepo.Object, _mockTokenService.Object);
    }


    #region SignUp
    [Fact]
    public async Task SignUp_Should_Return_Created_Response_When_Successful()
    {
        // Arrange
        SignUpRequestDto signUpDto = new()
        {
            Email = "test@test.com",
            Password = "Password123",
            PasswordConfirmation = "Password123"
        };

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<User>())).ReturnsAsync(Guid.NewGuid());

        // Act
        ApiResponseDto<object> result = await _usersService.SignUp(signUpDto);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, result.StatusCode);
        Assert.True(result.Success);
        Assert.Equal("User successfully created.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }

    [Fact]
    public async Task SignUp_Should_Return_Conflict_Response_When_Email_Already_Exists()
    {
        // Arrange
        SignUpRequestDto signUpDto = new()
        {
            Email = "test@test.com",
            Password = "Password123",
            PasswordConfirmation = "Password123"
        };

        _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<User>()))
            .Throws(new PostgresException(string.Empty, string.Empty, string.Empty, PostgresErrorCodes.UniqueViolation));

        // Act
        ApiResponseDto<object> result = await _usersService.SignUp(signUpDto);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.False(result.Success);
        Assert.Equal("Email address already used.", result.Message);
        Assert.Null(result.Data);
        Assert.Null(result.Errors);
    }
    #endregion
}
