using Wingman.Api.Core.DTOs;
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
            Exception inner = ex.GetInnermostException();

            _logger.LogError(ex, inner.Message); // TODO(serafa.leo): Learn about logging. Should I use ex or inner?

            ApiResponseDto<object> response = new()
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
#if DEBUG
            response.Message = inner.Message;
            response.Errors = [inner.StackTrace];
#else
            apiResponse.Message = "An unexpected error has occured.";
#endif
            context.Response.StatusCode = response.StatusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
