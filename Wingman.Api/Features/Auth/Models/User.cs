using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Models;
using Wingman.Api.Features.Auth.Helpers.Constants;

namespace Wingman.Api.Features.Auth.Models;

public class User : BaseModel
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpirationDateTimeUTC { get; set; }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.BaseConfiguration();

        builder.Property(user => user.Name)
            .HasMaxLength(LengthConstants.USER_NAME_MAX_LENGTH)
            .IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.Email)
            .HasMaxLength(LengthConstants.EMAIL_MAX_LENGTH)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(84) // Current length of hash output
            .IsRequired();

        builder.Property(user => user.RefreshToken)
            .HasMaxLength(LengthConstants.REFRESH_TOKEN_LENGTH);
    }
}