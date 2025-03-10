using Wingman.Api.Features.Flights.Enums;

namespace Wingman.Api.Features.Flights.Models;

public class Flight
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AircraftId { get; set; }
    public EFlightStatus Status { get; set; }
    public DateTime DepartureDateTimeUTC { get; set; }
    public required string DepartureICAO { get; set; }
    public required string ArrivalICAO { get; set; }
    public required string AlternateICAO { get; set; }
    public TimeSpan Duration { get; set; }
}
