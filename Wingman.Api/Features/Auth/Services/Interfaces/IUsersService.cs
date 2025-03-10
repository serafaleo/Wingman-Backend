using Wingman.Api.Core.DTOs;
using Wingman.Api.Features.Auth.DTOs;

namespace Wingman.Api.Features.Auth.Services.Interfaces;

public interface IUsersService
{
    public Task<ApiResponseDto<object>> SignUp(SignUpRequestDto signUpDto);
    public Task<ApiResponseDto<TokenResponseDto>> Login(LoginRequestDto loginDto);
    public Task<ApiResponseDto<TokenResponseDto>> Refresh(RefreshRequestDto refreshDto);
    public Task<ApiResponseDto<object>> Logout(Guid userId, string userEmail);
}
