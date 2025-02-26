namespace Wingman.Api.Features.Auth.DTOs;

public class TokenResponseDto
{
    public required Guid UserId { get; set; }
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}
