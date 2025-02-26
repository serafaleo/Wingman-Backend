namespace Wingman.Api.Features.Auth.Models;

public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpirationDateTimeUTC { get; set; }
}
