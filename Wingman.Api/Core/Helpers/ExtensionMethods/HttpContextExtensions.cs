﻿using System.Security.Claims;

namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext context)
    {
        return Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
