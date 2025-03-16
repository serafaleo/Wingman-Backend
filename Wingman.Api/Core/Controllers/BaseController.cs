using Microsoft.AspNetCore.Mvc;

namespace Wingman.Api.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase { }
