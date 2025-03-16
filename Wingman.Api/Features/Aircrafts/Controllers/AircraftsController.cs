using Wingman.Api.Core.Controllers;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Aircrafts.Services.Interfaces;

namespace Wingman.Api.Features.Aircrafts.Controllers;

public class AircraftsController(IAircraftsService service) : CommonController<Aircraft>(service)
{
}
