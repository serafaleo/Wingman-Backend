using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.DTOs;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Controllers;

[Route("api/[controller]/[action]")]
public abstract class BaseController<T>(IBaseService<T> service) : Controller
{
    private readonly IBaseService<T> _service = service;

    protected IActionResult CreateResponse<R>(ApiResponseDto<R> response)
    {
        return response.StatusCode switch
        {
            StatusCodes.Status200OK => Ok(response),
            StatusCodes.Status201Created => Created("", response),
            StatusCodes.Status400BadRequest => BadRequest(response),
            StatusCodes.Status401Unauthorized => Unauthorized(response),
            StatusCodes.Status404NotFound => NotFound(response),
            StatusCodes.Status409Conflict => Conflict(response),
            _ => throw new NotImplementedException()
        };
    }
}
