using Wingman.Api.Core.Repositories;
using Wingman.Api.Core.Services.Interfaces;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Aircrafts.Repositories.Interfaces;

namespace Wingman.Api.Features.Aircrafts.Repositories;

public class AircraftsRepository(IDbConnectionService db) : CommonRepository<Aircraft>(db), IAircraftsRepository
{
}
