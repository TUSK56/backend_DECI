using Deci.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Deci.Api.Controllers;

/// <summary>Master lists for session forms (all authenticated users).</summary>
[ApiController]
[Authorize]
[Route("api/catalog")]
public class CatalogController(IAppDbContext db) : ControllerBase
{
    [HttpGet("trainers")]
    public async Task<ActionResult<IEnumerable<object>>> Trainers() =>
        Ok(await db.MasterTrainers.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.Name).Select(t => new { t.Id, t.Name }).ToListAsync());

    [HttpGet("group-codes")]
    public async Task<ActionResult<IEnumerable<object>>> GroupCodes() =>
        Ok(await db.MasterGroupCodes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).Select(x => new { x.Id, x.Code }).ToListAsync());
}
