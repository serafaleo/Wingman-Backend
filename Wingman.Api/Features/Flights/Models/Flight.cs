using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Models;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Flights.Enums;

namespace Wingman.Api.Features.Flights.Models;

public class Flight : CommonModel
{
    public Guid AircraftId { get; set; }
    public EFlightStatus Status { get; set; }
    public DateTime DepartureDateTimeUTC { get; set; }
    public required string DepartureICAO { get; set; }
    public required string ArrivalICAO { get; set; }
    public required string AlternateICAO { get; set; }
    public TimeSpan Duration { get; set; }
}

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.CommonConfiguration();

        builder.HasOne<Aircraft>()
            .WithMany()
            .HasForeignKey(flight => flight.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasEnumCheckConstraint(flight => flight.Status).IsRequired();
    }
}

// TODO(serafa.leo): FluentValidation