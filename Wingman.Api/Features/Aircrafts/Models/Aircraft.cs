using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Models;
using Wingman.Api.Features.Aircrafts.Helpers.Constants;

namespace Wingman.Api.Features.Aircrafts.Models;

public class Aircraft : CommonModel
{
    public required string Registration { get; set; }
    public required string TypeICAO { get; set; }
}

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.CommonConfiguration();

        builder.HasIndex(aircraft => new { aircraft.UserId, aircraft.Registration }).IsUnique();

        builder.Property(aircraft => aircraft.Registration)
            .HasMaxLength(LengthConstants.REGISTRATION_MAX_LENGTH)
            .IsRequired();

        builder.Property(aircraft => aircraft.TypeICAO)
            .HasMaxLength(LengthConstants.TYPE_ICAO_LENGTH)
            .IsRequired();
    }
}

public class AircraftValidator : AbstractValidator<Aircraft>
{
    public AircraftValidator()
    {
        RuleFor(aircraft => aircraft.Registration)
            .NotEmpty()
                .WithMessage("Aircraft registration must be provided.")
            .MaximumLength(LengthConstants.REGISTRATION_MAX_LENGTH)
                .WithMessage($"Registration must be up to {LengthConstants.REGISTRATION_MAX_LENGTH} characters long.");

        RuleFor(aircraft => aircraft.TypeICAO)
            .NotEmpty()
                .WithMessage("Aircraft ICAO type must be provided.")
            .Length(LengthConstants.TYPE_ICAO_LENGTH)
                .WithMessage($"ICAO type must be {LengthConstants.TYPE_ICAO_LENGTH} characters long.");
    }
}