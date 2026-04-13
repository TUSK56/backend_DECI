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
public class TasksController(IAppDbContext db) : ControllerBase
{
    public record TaskDto(int Id, string Title, string? Description, int AssigneeId, string AssigneeName, int CreatedById, string Priority, string? Deadline, string Status, DateTime CreatedAt);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> List()
    {
        var uid = User.GetUserId();
        var isElevated = User.IsElevated();
        var q = db.Tasks.AsNoTracking().Include(t => t.Assignee).OrderByDescending(t => t.CreatedAt).AsQueryable();
        if (!isElevated) q = q.Where(t => t.AssigneeId == uid);
        var rows = await q.ToListAsync();
        return Ok(rows.Select(Map));
    }

    public record CreateTask(string Title, string? Description, int AssigneeId, string Priority, string? Deadline);

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrManager)]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTask body)
    {
        var assignee = await db.Users.FindAsync(body.AssigneeId);
        if (assignee == null || !assignee.IsActive) return BadRequest("Invalid assignee");
        var me = User.GetUserId();
        var t = new DeciTask
        {
            Title = body.Title,
            Description = body.Description,
            AssigneeId = body.AssigneeId,
            CreatedById = me,
            Priority = string.IsNullOrWhiteSpace(body.Priority) ? "medium" : body.Priority,
            Deadline = body.Deadline,
            Status = "todo",
        };
        db.Tasks.Add(t);
        await db.SaveChangesAsync();
        var loaded = await db.Tasks.Include(x => x.Assignee).FirstAsync(x => x.Id == t.Id);
        return Ok(Map(loaded));
    }

    public record TaskStatusBody(string Status);

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskDto>> UpdateStatus(int id, [FromBody] TaskStatusBody body)
    {
        var t = await db.Tasks.Include(x => x.Assignee).FirstOrDefaultAsync(x => x.Id == id);
        if (t == null) return NotFound();
        var uid = User.GetUserId();
        if (!User.IsElevated() && t.AssigneeId != uid) return Forbid();
        t.Status = body.Status;
        await db.SaveChangesAsync();
        return Ok(Map(t));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrManager)]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await db.Tasks.FindAsync(id);
        if (t == null) return NotFound();
        db.Tasks.Remove(t);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static TaskDto Map(DeciTask t) =>
        new(t.Id, t.Title, t.Description, t.AssigneeId, t.Assignee.FullName, t.CreatedById, t.Priority, t.Deadline, t.Status, t.CreatedAt);
}
