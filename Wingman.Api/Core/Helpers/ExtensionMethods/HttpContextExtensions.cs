using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class HttpContextExtensions
{
    public static Guid GetUserIdFromHeader(this HttpContext context)
    {
        // NOTE(serafa.leo): This First will break if the request does not have a Authentication token. Let it just breaks.
        return Guid.Parse(GetJwtClaims(context).First(claim => claim.Type == ClaimTypes.NameIdentifier).Value);
    }

    public static string GetUserEmailFromHeader(this HttpContext context)
    {
        return GetJwtClaims(context).First(claim => claim.Type == ClaimTypes.Email).Value;
    }

    private static IEnumerable<Claim> GetJwtClaims(HttpContext context)
    {
        string jwt = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        return new JwtSecurityTokenHandler().ReadJwtToken(jwt).Claims;
    }
}
