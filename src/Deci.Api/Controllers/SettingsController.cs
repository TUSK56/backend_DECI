using Deci.Application.DTOs;
using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController(IAppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<SystemSettingsDto>> Get()
    {
        var s = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SystemSetting();
        return Ok(ToDto(s));
    }

    [HttpPut]
    [Authorize(Roles = Roles.AdminOrManager)]
    public async Task<ActionResult<SystemSettingsDto>> Update([FromBody] UpdateSystemSettingsRequest req)
    {
        var s = await db.SystemSettings.FirstOrDefaultAsync();
        if (s == null)
        {
            s = new SystemSetting { Id = 1 };
            db.SystemSettings.Add(s);
        }

        if (TimeSpan.TryParse(req.ShiftStart, out var ss)) s.ShiftStart = ss;
        if (TimeSpan.TryParse(req.ShiftEnd, out var se)) s.ShiftEnd = se;
        if (req.IpTrackingEnabled.HasValue) s.IpTrackingEnabled = req.IpTrackingEnabled.Value;
        if (req.SessionApprovalRequired.HasValue) s.SessionApprovalRequired = req.SessionApprovalRequired.Value;

        await db.SaveChangesAsync();
        return Ok(ToDto(s));
    }

    private static SystemSettingsDto ToDto(SystemSetting s) =>
        new(s.ShiftStart.ToString(@"hh\:mm"), s.ShiftEnd.ToString(@"hh\:mm"), s.IpTrackingEnabled, s.SessionApprovalRequired);
}
