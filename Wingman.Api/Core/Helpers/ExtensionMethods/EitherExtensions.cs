using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class EitherExtensions
{
    public static IActionResult ToActionResult<T>(this Either<ProblemDetails, T> result)
    {
        return result.Match(
            Left: (problem) => problem.Status switch
            {
                // NOTE(serafa.leo): Possible return status from services.
                StatusCodes.Status400BadRequest => new BadRequestObjectResult(problem),
                StatusCodes.Status401Unauthorized => new UnauthorizedObjectResult(problem),
                StatusCodes.Status403Forbidden => new ObjectResult(problem) { StatusCode = problem.Status },
                StatusCodes.Status404NotFound => new NotFoundObjectResult(problem),
                StatusCodes.Status409Conflict => new ConflictObjectResult(problem),
                _ => throw new NotImplementedException()
            },
            Right: (obj) => new OkObjectResult(obj is Unit ? null : obj));
    }
}
