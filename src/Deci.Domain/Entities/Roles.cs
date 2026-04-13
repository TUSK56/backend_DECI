namespace Deci.Domain.Entities;

public static class Roles
{
    /// <summary>Single tenant administrator: user management and full operational access.</summary>
    public const string Admin = "Admin";

    public const string Manager = "Manager";
    public const string Coordinator = "Coordinator";

    /// <summary>For [Authorize(Roles = Roles.AdminOrManager)] — operational “manager view” (Admin or deputy Manager).</summary>
    public const string AdminOrManager = "Admin,Manager";
}
