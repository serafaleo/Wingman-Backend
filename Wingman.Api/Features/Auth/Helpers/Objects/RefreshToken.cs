namespace Wingman.Api.Features.Auth.Helpers.Objects;

public class RefreshToken
{
    public required string Token;
    public DateTime? Expiration;
}
