using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

[ApiController]
[Authorize(Roles = Roles.AdminOrManager)]
[Route("api/admin/group-codes")]
public class GroupCodesController(IAppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List() =>
        Ok(await db.MasterGroupCodes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).Select(x => new { x.Id, x.Code }).ToListAsync());

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<object>>> ListIncludingInactive() =>
        Ok(await db.MasterGroupCodes.AsNoTracking().OrderBy(x => x.Code).Select(x => new { x.Id, x.Code, x.IsActive }).ToListAsync());

    public record CodeBody(string Code);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CodeBody body)
    {
        if (string.IsNullOrWhiteSpace(body.Code)) return BadRequest();
        var code = body.Code.Trim().ToUpperInvariant();
        if (await db.MasterGroupCodes.AnyAsync(x => x.Code == code)) return Conflict();
        db.MasterGroupCodes.Add(new MasterGroupCode { Code = code });
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CodeBody body)
    {
        var g = await db.MasterGroupCodes.FindAsync(id);
        if (g == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(body.Code)) g.Code = body.Code.Trim().ToUpperInvariant();
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var g = await db.MasterGroupCodes.FindAsync(id);
        if (g == null) return NotFound();
        g.IsActive = false;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
