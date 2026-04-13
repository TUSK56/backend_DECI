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
public class LeavesController(IAppDbContext db) : ControllerBase
{
    public record LeaveDto(int Id, int UserId, string UserName, string Type, string Start, string End, string Reason, string Status, DateTime CreatedAt);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveDto>>> List()
    {
        var uid = User.GetUserId();
        var isElevated = User.IsElevated();
        var q = db.LeaveRequests.AsNoTracking().Include(l => l.User).OrderByDescending(l => l.CreatedAt).AsQueryable();
        if (!isElevated) q = q.Where(l => l.UserId == uid);
        var rows = await q.ToListAsync();
        return Ok(rows.Select(l => new LeaveDto(l.Id, l.UserId, l.User.FullName, l.Type, l.Start, l.End, l.Reason, l.Status, l.CreatedAt)));
    }

    public record CreateLeave(string Type, string Start, string End, string Reason);

    [HttpPost]
    public async Task<ActionResult<LeaveDto>> Create([FromBody] CreateLeave body)
    {
        var uid = User.GetUserId();
        var user = await db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();
        var leave = new LeaveRequest
        {
            UserId = uid,
            Type = body.Type,
            Start = body.Start,
            End = body.End,
            Reason = body.Reason,
            Status = "pending",
        };
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        var loaded = await db.LeaveRequests.Include(l => l.User).FirstAsync(l => l.Id == leave.Id);
        return Ok(new LeaveDto(loaded.Id, loaded.UserId, loaded.User.FullName, loaded.Type, loaded.Start, loaded.End, loaded.Reason, loaded.Status, loaded.CreatedAt));
    }

    public record LeaveDecideBody(string Status);

    [HttpPost("{id:int}/decide")]
    [Authorize(Roles = Roles.AdminOrManager)]
    public async Task<ActionResult<LeaveDto>> Decide(int id, [FromBody] LeaveDecideBody body)
    {
        var l = await db.LeaveRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (l == null) return NotFound();
        if (body.Status is not ("approved" or "rejected")) return BadRequest();
        l.Status = body.Status;
        await db.SaveChangesAsync();
        return Ok(new LeaveDto(l.Id, l.UserId, l.User.FullName, l.Type, l.Start, l.End, l.Reason, l.Status, l.CreatedAt));
    }
}
