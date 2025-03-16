using Wingman.Api.Core.Controllers;
using Wingman.Api.Features.Flights.Models;
using Wingman.Api.Features.Flights.Services.Interfaces;

namespace Wingman.Api.Features.Flights.Controllers;

public class FlightsController(IFlightsService service) : CommonController<Flight>(service)
{
}
