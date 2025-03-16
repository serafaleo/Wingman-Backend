using Wingman.Api.Core.Services;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Aircrafts.Repositories.Interfaces;
using Wingman.Api.Features.Aircrafts.Services.Interfaces;

namespace Wingman.Api.Features.Aircrafts.Services;

public class AircraftsService(IAircraftsRepository repo) : CommonService<Aircraft>(repo), IAircraftsService
{
}
