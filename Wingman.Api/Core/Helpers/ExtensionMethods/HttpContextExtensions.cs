using System.Security.Claims;

namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext context)
    {
        string? userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdString.IsNullOrEmpty())
            return null;

        return Guid.Parse(userIdString!);
    }

    public static string? GetUserEmail(this HttpContext context)
    {
        return context.User.FindFirstValue(ClaimTypes.Email);
    }
}
