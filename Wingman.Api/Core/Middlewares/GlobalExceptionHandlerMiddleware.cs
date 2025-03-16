using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.Helpers.ExtensionMethods;

namespace Wingman.Api.Core.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Exception innerException = ex.GetInnermostException();

            _logger.LogError(ex, innerException.Message); // TODO(serafa.leo): Learn about logging. Should I use ex or inner?

#if DEBUG
            ProblemDetails problem = new ProblemDetails().InternalServerError(innerException);
#else
            ProblemDetails problem = new ProblemDetails().InternalServerError();
#endif

            context.Response.StatusCode = problem.Status!.Value;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
