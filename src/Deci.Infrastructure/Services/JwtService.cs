using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Deci.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Deci.Infrastructure.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    public const string ProfileCompleteClaimType = "profile_complete";

    public string GenerateToken(int userId, string email, string fullName, string role, bool profileCompleted)
    {
        var key = config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "DECI_DEV_ONLY_CHANGE_THIS_KEY_32_CHARS_MIN!!";
        var issuer = config["Jwt:Issuer"] ?? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "DeciApi";
        var audience = config["Jwt:Audience"] ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "DeciPortal";
        var expiryRaw = config["Jwt:ExpiryMinutes"] ?? "10080";
        var expiryMinutes = int.TryParse(expiryRaw, out var m) ? m : 10080;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Role, role),
            new(ProfileCompleteClaimType, profileCompleted ? "true" : "false"),
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
