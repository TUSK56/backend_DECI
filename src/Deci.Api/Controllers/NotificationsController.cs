using Deci.Api.Extensions;
using Deci.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController(IAppDbContext db) : ControllerBase
{
    public record NotifDto(int Id, string Title, string Body, DateTime CreatedAt, bool Read);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotifDto>>> List()
    {
        var uid = User.GetUserId();
        var rows = await db.Notifications.AsNoTracking()
            .Where(n => n.ForUserId == null || n.ForUserId == uid)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(rows.Select(n => new NotifDto(n.Id, n.Title, n.Body, n.CreatedAt, n.Read)));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var uid = User.GetUserId();
        await db.Notifications.Where(n => n.ForUserId == null || n.ForUserId == uid).ExecuteUpdateAsync(s => s.SetProperty(n => n.Read, true));
        return NoContent();
    }
}
