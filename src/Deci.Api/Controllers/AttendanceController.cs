using Deci.Api.Extensions;
using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AttendanceController(IAppDbContext db) : ControllerBase
{
    public record AttendanceDto(int Id, int UserId, string UserName, DateTime ClockIn, DateTime? ClockOut, string? Ip, string DateLabel);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> List()
    {
        var uid = User.GetUserId();
        var isElevated = User.IsElevated();
        var q = db.AttendanceRecords.AsNoTracking().Include(a => a.User).OrderByDescending(a => a.ClockIn).AsQueryable();
        if (!isElevated) q = q.Where(a => a.UserId == uid);
        var rows = await q.Take(500).ToListAsync();
        return Ok(rows.Select(a => new AttendanceDto(a.Id, a.UserId, a.User.FullName, a.ClockIn, a.ClockOut, a.Ip, a.DateLabel)));
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<AttendanceDto>> ClockIn([FromServices] IHttpContextAccessor http)
    {
        var settings = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SystemSetting();
        var uid = User.GetUserId();
        var user = await db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        var today = DateTime.UtcNow.Date;
        var exists = await db.AttendanceRecords.AnyAsync(a => a.UserId == uid && a.ClockIn.Date == today && a.ClockOut == null);
        if (exists) return BadRequest("Already clocked in");

        string? ip = null;
        if (settings.IpTrackingEnabled)
            ip = http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        var rec = new AttendanceRecord
        {
            UserId = uid,
            ClockIn = DateTime.UtcNow,
            Ip = ip,
            DateLabel = DateTime.UtcNow.ToString("d"),
        };
        db.AttendanceRecords.Add(rec);
        await db.SaveChangesAsync();
        var loaded = await db.AttendanceRecords.Include(r => r.User).FirstAsync(r => r.Id == rec.Id);
        return Ok(new AttendanceDto(loaded.Id, loaded.UserId, loaded.User.FullName, loaded.ClockIn, loaded.ClockOut, loaded.Ip, loaded.DateLabel));
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<AttendanceDto>> ClockOut()
    {
        var uid = User.GetUserId();
        var today = DateTime.UtcNow.Date;
        var rec = await db.AttendanceRecords.Include(r => r.User)
            .FirstOrDefaultAsync(a => a.UserId == uid && a.ClockIn.Date == today && a.ClockOut == null);
        if (rec == null) return NotFound();
        rec.ClockOut = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(new AttendanceDto(rec.Id, rec.UserId, rec.User.FullName, rec.ClockIn, rec.ClockOut, rec.Ip, rec.DateLabel));
    }
}
