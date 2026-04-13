using System.Text.RegularExpressions;
using Deci.Infrastructure.Services;

namespace Deci.Api.Middleware;

public sealed class ProfileCompletionMiddleware(RequestDelegate next)
{
    private static readonly Regex UserProfileGetPath = new(@"^/api/users/\d+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public Task Invoke(HttpContext ctx)
    {
        if (ctx.User.Identity?.IsAuthenticated == true)
        {
            var raw = ctx.User.FindFirst(JwtService.ProfileCompleteClaimType)?.Value;
            if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase) && !IsAllowedPath(ctx))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("""{"code":"PROFILE_INCOMPLETE"}""");
            }
        }

        return next(ctx);
    }

    private static bool IsAllowedPath(HttpContext ctx)
    {
        var path = ctx.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/Auth/login", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/Auth/me", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/auth/me", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/Auth/complete-profile", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.Equals("/api/auth/complete-profile", StringComparison.OrdinalIgnoreCase)) return true;
        if (HttpMethods.IsGet(ctx.Request.Method) && UserProfileGetPath.IsMatch(path)) return true;
        return false;
    }
}
