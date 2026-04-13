using System.Security.Claims;
using Deci.Domain.Entities;

namespace Deci.Api.Extensions;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    public static bool IsElevated(this ClaimsPrincipal user) =>
        user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Manager);

    public static bool IsAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(Roles.Admin);
}
