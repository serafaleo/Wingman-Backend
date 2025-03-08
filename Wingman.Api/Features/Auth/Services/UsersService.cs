using Microsoft.AspNetCore.Identity;
using Npgsql;
using Wingman.Api.Core.DTOs;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Services;
using Wingman.Api.Features.Auth.DTOs;
using Wingman.Api.Features.Auth.Helpers.Objects;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Api.Features.Auth.Services;

public class UsersService(IUsersRepository repo, ITokenService tokenService) : BaseService<User>(repo), IUsersService
{
    private readonly IUsersRepository _repo = repo;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<ApiResponseDto<object>> SignUp(SignUpRequestDto signUpDto)
    {
        User newUser = new()
        {
            Email = signUpDto.Email
        };

        newUser.PasswordHash = new PasswordHasher<User>().HashPassword(newUser, signUpDto.Password);

        signUpDto.Password = string.Empty;
        signUpDto.PasswordConfirmation = string.Empty;

        try
        {
            await _repo.CreateAsync(newUser);

            // TODO(serafa.leo): Send email to verify account

            return new()
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "User successfully created."
            };
        }
        catch (PostgresException ex)
        {
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return new()
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = "Email address already used."
                };
            }

            throw;
        }
    }

    public async Task<ApiResponseDto<TokenResponseDto>> Login(LoginRequestDto loginDto)
    {
        // NOTE(serafa.leo): Here we avoid information disclosure by returning Unauthorizedif anything
        // wrong occurs, and by providing a generic message like "Email or password is wrong".

        ApiResponseDto<TokenResponseDto> commonFailedResponse = new()
        {
            StatusCode = StatusCodes.Status401Unauthorized,
            Message = "Email or password wrong."
        };

        User? user = await _repo.GetUserByEmailAsync(loginDto.Email);

        if (user is null)
        {
            return commonFailedResponse;
        }

        PasswordVerificationResult passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, loginDto.Password);

        loginDto.Password = string.Empty;

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return commonFailedResponse;
        }

        TokenResponseDto tokenDto = await GenerateTokenDtoAndSaveAsync(user);

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Data = tokenDto,
            Message = "Login successful."
        };
    }

    public async Task<ApiResponseDto<TokenResponseDto>> Refresh(RefreshRequestDto refreshDto)
    {
        // TODO(serafa.leo): Do we need to check if the JWT is still valid?

        User? user = await _repo.GetByIdAsync(refreshDto.UserId);

        if (user is null)
        {
            return new()
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User not found."
            };
        }

        if (user.RefreshToken.IsNullOrEmpty() || user.RefreshToken != refreshDto.RefreshToken)
        {
            return new()
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid Refresh Token."
            };
        }

        if (user.RefreshTokenExpirationDateTimeUTC <= DateTime.UtcNow)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTimeUTC = null;

            await _repo.UpdateRefreshTokenAsync(user);

            return new()
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Refresh Token is expired. A new login is necessary."
            };
        }

        TokenResponseDto tokenDto = await GenerateTokenDtoAndSaveAsync(user);

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Data = tokenDto,
            Message = "Refresh successful."
        };
    }

    public async Task<ApiResponseDto<object>> Logout(Guid userId, string userEmail)
    {
        User wildcardUser = new()
        {
            Id = userId,
            Email = userEmail
        };

        await _repo.UpdateRefreshTokenAsync(wildcardUser);

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Logout successful."
        };
    }

    private async Task<TokenResponseDto> GenerateTokenDtoAndSaveAsync(User user)
    {
        string accessToken = _tokenService.GenerateAccessToken(user);
        RefreshToken refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenExpirationDateTimeUTC = refreshToken.Expiration;

        await _repo.UpdateRefreshTokenAsync(user);

        return new TokenResponseDto()
        {
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }
}
