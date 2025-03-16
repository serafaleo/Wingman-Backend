using Wingman.Api.Core.Repositories;
using Wingman.Api.Core.Services.Interfaces;
using Wingman.Api.Features.Flights.Models;
using Wingman.Api.Features.Flights.Repositories.Interfaces;

namespace Wingman.Api.Features.Flights.Repositories;

public class FlightsRepository(IDbConnectionService db) : CommonRepository<Flight>(db), IFlightsRepository
{
}
