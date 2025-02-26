using Wingman.Api.Features.Auth.Helpers.Objects;
using Wingman.Api.Features.Auth.Models;

namespace Wingman.Api.Features.Auth.Services.Interfaces;

public interface ITokenService
{
    public string GenerateAccessToken(User user);
    public RefreshToken GenerateRefreshToken();
}
