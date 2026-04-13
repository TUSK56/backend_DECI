using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.AdminOrManager)]
[Route("api/admin/trainers")]
public class TrainersController(IAppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List() =>
        Ok(await db.MasterTrainers.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.Name).Select(t => new { t.Id, t.Name }).ToListAsync());

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<object>>> ListIncludingInactive() =>
        Ok(await db.MasterTrainers.AsNoTracking().OrderBy(t => t.Name).Select(t => new { t.Id, t.Name, t.IsActive }).ToListAsync());

    public record TrainerBody(string Name);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TrainerBody body)
    {
        if (string.IsNullOrWhiteSpace(body.Name)) return BadRequest();
        db.MasterTrainers.Add(new MasterTrainer { Name = body.Name.Trim() });
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Rename(int id, [FromBody] TrainerBody body)
    {
        var t = await db.MasterTrainers.FindAsync(id);
        if (t == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(body.Name)) t.Name = body.Name.Trim();
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var t = await db.MasterTrainers.FindAsync(id);
        if (t == null) return NotFound();
        t.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
