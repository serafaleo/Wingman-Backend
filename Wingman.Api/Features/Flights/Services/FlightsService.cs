using Wingman.Api.Core.Services;
using Wingman.Api.Features.Flights.Models;
using Wingman.Api.Features.Flights.Repositories.Interfaces;
using Wingman.Api.Features.Flights.Services.Interfaces;

namespace Wingman.Api.Features.Flights.Services;

public class FlightsService(IFlightsRepository repo) : BaseService<Flight>(repo), IFlightsService
{
    private readonly IFlightsRepository _repo = repo;
}
