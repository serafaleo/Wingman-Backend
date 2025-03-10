namespace Wingman.Api.Features.Aircrafts.Models;

public class Aircraft
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Registration { get; set; }
    public required string TypeICAO { get; set; }
}
