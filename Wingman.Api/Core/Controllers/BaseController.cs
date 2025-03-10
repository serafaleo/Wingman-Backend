using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.DTOs;

namespace Wingman.Api.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : Controller
{
    protected IActionResult CreateResponse<T>(ApiResponseDto<T> response)
    {
        return response.StatusCode switch
        {
            StatusCodes.Status200OK => Ok(response),
            StatusCodes.Status201Created => Created("", response),
            StatusCodes.Status400BadRequest => BadRequest(response),
            StatusCodes.Status401Unauthorized => Unauthorized(response),
            StatusCodes.Status403Forbidden => Forbid(), // TODO(serafa.leo): See how we can send the ApiResponseDto object with the Forbid. Maybe use ProblemDetails???
            StatusCodes.Status404NotFound => NotFound(response),
            StatusCodes.Status409Conflict => Conflict(response),
            _ => throw new NotImplementedException()
        };
    }
}
