using Deci.Api.Extensions;
using Deci.Application.DTOs;
using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(IAppDbContext db, IWebHostEnvironment env, IPasswordHasher hasher) : ControllerBase
{
    [HttpGet("all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<UserDto>>> ListAll()
    {
        var users = await db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync();
        return Ok(users.Select(u => AuthController.MapUser(u, Request)));
    }

    /// <summary>Active coordinators for task assignment (Admin or Manager).</summary>
    [HttpGet("assignable-coordinators")]
    [Authorize(Roles = Roles.AdminOrManager)]
    public async Task<ActionResult<IEnumerable<object>>> AssignableCoordinators() =>
        Ok(await db.Users.AsNoTracking()
            .Where(u => u.IsActive && u.Role == Roles.Coordinator)
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(int id)
    {
        var me = User.GetUserId();
        if (!User.IsElevated() && me != id) return Forbid();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        UserStatsDto? stats = null;
        if (user.Role == Roles.Coordinator)
        {
            var sessions = await db.SessionLogs.CountAsync(s => s.CoordinatorUserId == id);
            var tasks = await db.Tasks.CountAsync(t => t.AssigneeId == id && t.Status == "done");
            stats = new UserStatsDto(sessions, tasks);
        }

        return Ok(new UserProfileDto(AuthController.MapUser(user, Request), stats));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email.ToLower() == email))
            return Conflict("Email already exists");

        if (req.Role == Roles.Admin)
            return BadRequest("Invalid role");
        var role = req.Role is Roles.Manager or Roles.Coordinator ? req.Role : Roles.Coordinator;
        var user = new AppUser
        {
            Email = email,
            PasswordHash = hasher.Hash(req.Password),
            FullName = req.FullName.Trim(),
            Phone = req.Phone?.Trim(),
            Role = role,
            IsActive = true,
            ProfileCompleted = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProfile), new { id = user.Id }, AuthController.MapUser(user, Request));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        if (user.Role == Roles.Admin)
        {
            if (req.IsActive == false) return BadRequest("Cannot deactivate the primary administrator");
            if (!string.IsNullOrWhiteSpace(req.Role) && req.Role != Roles.Admin)
                return BadRequest("Cannot change the primary administrator role");
        }

        if (!string.IsNullOrWhiteSpace(req.Email))
        {
            var e = req.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == e))
                return Conflict("Email already exists");
            user.Email = e;
        }
        if (!string.IsNullOrWhiteSpace(req.FullName)) user.FullName = req.FullName.Trim();
        if (req.Phone != null) user.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        if (!string.IsNullOrWhiteSpace(req.Password)) user.PasswordHash = hasher.Hash(req.Password);
        if (req.Role == Roles.Admin)
            return BadRequest("Invalid role");
        if (!string.IsNullOrWhiteSpace(req.Role) && (req.Role == Roles.Manager || req.Role == Roles.Coordinator))
            user.Role = req.Role;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

        await db.SaveChangesAsync();
        return Ok(AuthController.MapUser(user, Request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Deactivate(int id)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        if (user.Role == Roles.Admin) return BadRequest("Cannot deactivate the primary administrator");
        if (user.Id == User.GetUserId()) return BadRequest("Cannot deactivate yourself");
        user.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("me/profile")]
    public async Task<ActionResult<UserDto>> UpdateMyProfile([FromBody] UpdateProfileRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId());
        if (user == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(req.FullName)) user.FullName = req.FullName.Trim();
        if (req.Phone != null) user.Phone = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        await db.SaveChangesAsync();
        return Ok(AuthController.MapUser(user, Request));
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId());
        if (user == null) return NotFound();
        if (!hasher.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest("Current password is incorrect");
        user.PasswordHash = hasher.Hash(req.NewPassword);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("me/avatar")]
    public async Task<ActionResult<UserDto>> UploadAvatar(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp" and not ".gif")
            return BadRequest("Invalid image type");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId());
        if (user == null) return NotFound();

        var uploads = Path.Combine(env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploads);
        var name = $"{Guid.NewGuid():N}{ext}";
        var full = Path.Combine(uploads, name);
        await using (var fs = System.IO.File.Create(full))
            await file.CopyToAsync(fs);

        user.ProfileImagePath = $"/uploads/avatars/{name}";
        await db.SaveChangesAsync();
        return Ok(AuthController.MapUser(user, Request));
    }
}
