using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Features.Auth.DTOs;

namespace Wingman.Api.Features.Auth.Services.Interfaces;

public interface IUsersService
{
    public Task<Either<ProblemDetails, Unit>> SignUp(SignUpRequestDto signUpDto);
    public Task<Either<ProblemDetails, TokenResponseDto>> Login(LoginRequestDto loginDto);
    public Task<Either<ProblemDetails, TokenResponseDto>> Refresh(RefreshRequestDto refreshDto);
    public Task<Either<ProblemDetails, Unit>> Logout(Guid userId);
}
