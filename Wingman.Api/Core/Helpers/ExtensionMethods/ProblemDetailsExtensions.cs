using Microsoft.AspNetCore.Mvc;

namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails BadRequest(this ProblemDetails problem, string title, string message)
    {
        problem.Status = StatusCodes.Status400BadRequest;
        FillRemainingFields(problem, title, message);
        return problem;
    }

    public static ProblemDetails DefaultNotFound(this ProblemDetails problem, string title, string modelName)
    {
        problem.Status = StatusCodes.Status404NotFound;
        FillRemainingFields(problem, title, $"The requested {modelName} was not found in the server.");
        return problem;
    }

    public static ProblemDetails DefaultForbidden(this ProblemDetails problem, string title, string modelName)
    {
        problem.Status = StatusCodes.Status403Forbidden;
        FillRemainingFields(problem, title, $"The current user does not have permission to access this {modelName}.");
        return problem;
    }

    public static ProblemDetails Unauthorized(this ProblemDetails problem, string title, string message)
    {
        problem.Status = StatusCodes.Status401Unauthorized;
        FillRemainingFields(problem, title, message);
        return problem;
    }

    public static ProblemDetails Conflict(this ProblemDetails problem, string title, string message)
    {
        problem.Status = StatusCodes.Status409Conflict;
        FillRemainingFields(problem, title, message);
        return problem;
    }

    public static ProblemDetails InternalServerError(this ProblemDetails problem, Exception? exception = null)
    {
        problem.Status = StatusCodes.Status500InternalServerError;

        string errorMessage = exception?.Message ?? "An unexpected error has occured.";

        if (exception is not null && exception.StackTrace is not null)
        {
            problem.Extensions.Add("stackTrace", exception.StackTrace);
        }

        FillRemainingFields(problem, "Internal Server Error", errorMessage);
        return problem;
    }

    private static void FillRemainingFields(ProblemDetails problem, string title, string message)
    {
        problem.Type = GetIetfDataTrackerUrl(problem.Status!.Value);
        problem.Title = title;
        problem.Detail = message;
    }

    private static string GetIetfDataTrackerUrl(int statusCode)
    {
        return $"https://tools.ietf.org/html/rfc9110#section-{GetSectionForStatusCode(statusCode)}";
    }

    private static string GetSectionForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            // Client error responses (4xx)
            StatusCodes.Status400BadRequest => "15.5.1",
            StatusCodes.Status401Unauthorized => "15.5.2",
            StatusCodes.Status402PaymentRequired => "15.5.3",
            StatusCodes.Status403Forbidden => "15.5.4",
            StatusCodes.Status404NotFound => "15.5.5",
            StatusCodes.Status405MethodNotAllowed => "15.5.6",
            StatusCodes.Status406NotAcceptable => "15.5.7",
            StatusCodes.Status407ProxyAuthenticationRequired => "15.5.8",
            StatusCodes.Status408RequestTimeout => "15.5.9",
            StatusCodes.Status409Conflict => "15.5.10",
            StatusCodes.Status410Gone => "15.5.11",
            StatusCodes.Status411LengthRequired => "15.5.12",
            StatusCodes.Status412PreconditionFailed => "15.5.13",
            StatusCodes.Status413PayloadTooLarge => "15.5.14",
            StatusCodes.Status414UriTooLong => "15.5.15",
            StatusCodes.Status415UnsupportedMediaType => "15.5.16",
            StatusCodes.Status416RangeNotSatisfiable => "15.5.17",
            StatusCodes.Status417ExpectationFailed => "15.5.18",
            StatusCodes.Status418ImATeapot => "15.5.19",
            StatusCodes.Status421MisdirectedRequest => "15.5.20",
            StatusCodes.Status422UnprocessableEntity => "15.5.21",
            StatusCodes.Status423Locked => "15.5.22",
            StatusCodes.Status424FailedDependency => "15.5.23",
            StatusCodes.Status426UpgradeRequired => "15.5.25",
            StatusCodes.Status428PreconditionRequired => "15.5.26",
            StatusCodes.Status429TooManyRequests => "15.5.27",
            StatusCodes.Status431RequestHeaderFieldsTooLarge => "15.5.28",
            StatusCodes.Status451UnavailableForLegalReasons => "15.5.29",

            // Server error responses (5xx)
            StatusCodes.Status500InternalServerError => "15.6.1",
            StatusCodes.Status501NotImplemented => "15.6.2",
            StatusCodes.Status502BadGateway => "15.6.3",
            StatusCodes.Status503ServiceUnavailable => "15.6.4",
            StatusCodes.Status504GatewayTimeout => "15.6.5",
            StatusCodes.Status505HttpVersionNotsupported => "15.6.6",
            StatusCodes.Status506VariantAlsoNegotiates => "15.6.7",
            StatusCodes.Status507InsufficientStorage => "15.6.8",
            StatusCodes.Status508LoopDetected => "15.6.9",
            StatusCodes.Status510NotExtended => "15.6.10",
            StatusCodes.Status511NetworkAuthenticationRequired => "15.6.11",

            // Default case
            _ => "15"
        };
    }
}
