using System.Net.Mail;
using Deci.Api.Extensions;
using Deci.Application.DTOs;
using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAppDbContext db, IJwtService jwt, IPasswordHasher hasher) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        if (user == null || !user.IsActive)
            return Unauthorized("Invalid credentials");
        if (!hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var dto = MapUser(user, Request);
        return Ok(new LoginResponse(jwt.GenerateToken(user.Id, user.Email, user.FullName, user.Role, user.ProfileCompleted), dto));
    }

    [Authorize]
    [HttpPost("complete-profile")]
    public async Task<ActionResult<LoginResponse>> CompleteProfile([FromBody] CompleteProfileRequest req)
    {
        var id = User.GetUserId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        if (user.ProfileCompleted) return BadRequest("Profile is already complete");

        if (!hasher.Verify(req.CurrentPassword, user.PasswordHash))
            return Unauthorized("Current password is incorrect");

        if (req.NewPassword != req.ConfirmPassword)
            return BadRequest("New password and confirmation do not match");

        if (req.NewPassword.Length < 8)
            return BadRequest("New password must be at least 8 characters");

        if (hasher.Verify(req.NewPassword, user.PasswordHash))
            return BadRequest("New password must be different from your current password");

        var newEmail = req.NewEmail.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(newEmail) || newEmail.Length < 5)
            return BadRequest("Email is required");
        try
        {
            _ = new MailAddress(newEmail);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid email format");
        }

        if (string.Equals(newEmail, user.Email.Trim().ToLowerInvariant(), StringComparison.Ordinal))
            return BadRequest("Choose a different email address than your sign-in email");

        if (await db.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == newEmail))
            return Conflict("That email is already in use");

        var phone = req.Phone.Trim();
        if (phone.Length < 5)
            return BadRequest("Phone number is required (at least 5 characters)");

        user.Email = newEmail;
        user.PasswordHash = hasher.Hash(req.NewPassword);
        user.Phone = phone;
        user.ProfileCompleted = true;
        await db.SaveChangesAsync();

        var dto = MapUser(user, Request);
        return Ok(new LoginResponse(jwt.GenerateToken(user.Id, user.Email, user.FullName, user.Role, true), dto));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var id = User.GetUserId();
        var user = await db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(MapUser(user, Request));
    }

    internal static UserDto MapUser(AppUser u, HttpRequest req)
    {
        string? url = null;
        if (!string.IsNullOrEmpty(u.ProfileImagePath))
            url = $"{req.Scheme}://{req.Host}{u.ProfileImagePath}";
        return new UserDto(u.Id, u.Email, u.FullName, u.Phone, url, u.Role, u.IsActive, u.ProfileCompleted, u.CreatedAt);
    }
}
