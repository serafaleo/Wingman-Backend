using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Services;
using Wingman.Api.Features.Auth.DTOs;
using Wingman.Api.Features.Auth.Helpers.Objects;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Api.Features.Auth.Services;

public class UsersService(IUsersRepository repo, ITokenService tokenService) : BaseService<User>, IUsersService
{
    private readonly IUsersRepository _repo = repo;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<Either<ProblemDetails, Unit>> SignUp(SignUpRequestDto signUpDto)
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

            return new Unit();
        }
        catch (PostgresException ex)
        {
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return new ProblemDetails().Conflict("Failed to create new user.", "Email address already used.");
            }

            throw;
        }
    }

    public async Task<Either<ProblemDetails, TokenResponseDto>> Login(LoginRequestDto loginDto)
    {
        // NOTE(serafa.leo): Here we avoid information disclosure by returning Unauthorized if anything
        // wrong occurs, and by providing a generic message like "Email or password is wrong".

        const string defaultErrorTitle = "Login failed";
        const string defaultErrorMessage = "Email or password wrong.";

        User? user = await _repo.GetUserByEmailAsync(loginDto.Email);

        if (user is null)
        {
            return new ProblemDetails().Unauthorized(defaultErrorTitle, defaultErrorMessage);
        }

        PasswordVerificationResult passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash!, loginDto.Password);

        loginDto.Password = string.Empty;

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return new ProblemDetails().Unauthorized(defaultErrorTitle, defaultErrorMessage);
        }

        return await GenerateTokenDtoAndSaveAsync(user);
    }

    public async Task<Either<ProblemDetails, TokenResponseDto>> Refresh(RefreshRequestDto refreshDto)
    {
        // TODO(serafa.leo): Do we need to check if the JWT is still valid?

        const string defaultErrorTitle = "Failed to refresh session.";

        User? user = await _repo.GetByIdAsync(refreshDto.UserId);

        if (user is null)
        {
            return new ProblemDetails().DefaultNotFound(defaultErrorTitle, _modelName);
        }

        if (user.RefreshToken.IsNullOrEmpty() || user.RefreshToken != refreshDto.RefreshToken)
        {
            return new ProblemDetails().BadRequest(defaultErrorTitle, "Invalid Refresh Token.");
        }

        if (user.RefreshTokenExpirationDateTimeUTC <= DateTime.UtcNow)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpirationDateTimeUTC = null;

            await _repo.UpdateRefreshTokenAsync(user);

            return new ProblemDetails().Unauthorized(defaultErrorTitle, "Refresh Token is expired. A new login is necessary.");
        }

        return await GenerateTokenDtoAndSaveAsync(user);
    }

    public async Task<Either<ProblemDetails, Unit>> Logout(Guid userId, string userEmail)
    {
        User wildcardUser = new()
        {
            Id = userId,
            Email = userEmail
        };

        await _repo.UpdateRefreshTokenAsync(wildcardUser);

        return new Unit();
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
