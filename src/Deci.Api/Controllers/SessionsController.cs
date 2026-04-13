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
public class SessionsController(IAppDbContext db, IWebHostEnvironment env) : ControllerBase
{
    public record SessionFileDto(string Kind, string Url, string OriginalName);

    public record SessionDto(
        int Id,
        int CoordinatorUserId,
        string CoordinatorName,
        string GroupCode,
        string TrainerName,
        string SessionDate,
        string? SessionLink,
        string? RecordingLink,
        string? Notes,
        string Status,
        DateTime CreatedAt,
        IReadOnlyList<SessionFileDto> Files);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionDto>>> List([FromQuery] string? status, [FromQuery] string? search)
    {
        var uid = User.GetUserId();
        var isElevated = User.IsElevated();
        var q = db.SessionLogs.AsNoTracking().Include(s => s.Coordinator).Include(s => s.Files).AsQueryable();
        if (!isElevated) q = q.Where(s => s.CoordinatorUserId == uid);
        if (!string.IsNullOrWhiteSpace(status) && status != "all") q = q.Where(s => s.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var sLower = search.ToLowerInvariant();
            q = q.Where(s => s.GroupCode.ToLower().Contains(sLower) || s.TrainerName.ToLower().Contains(sLower));
        }
        var list = await q.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return Ok(list.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SessionDto>> Get(int id)
    {
        var s = await db.SessionLogs.Include(x => x.Coordinator).Include(x => x.Files).FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        if (!User.IsElevated() && s.CoordinatorUserId != User.GetUserId()) return Forbid();
        return Ok(Map(s));
    }

    [HttpPost]
    [RequestSizeLimit(1024L * 1024L * 100)]
    public async Task<ActionResult<SessionDto>> Create(
        [FromForm] string groupCode,
        [FromForm] string trainerName,
        [FromForm] string sessionDate,
        [FromForm] string? sessionLink,
        [FromForm] string? recordingLink,
        [FromForm] string? notes,
        [FromForm(Name = "attendanceFiles")] List<IFormFile>? attendanceFiles,
        [FromForm(Name = "screenshots")] List<IFormFile>? screenshots)
    {
        if (string.IsNullOrWhiteSpace(groupCode)) return BadRequest("Group code required");
        var settings = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync() ?? new SystemSetting();
        var status = settings.SessionApprovalRequired ? "pending" : "approved";

        var session = new SessionLog
        {
            CoordinatorUserId = User.GetUserId(),
            GroupCode = groupCode.Trim().ToUpperInvariant(),
            TrainerName = trainerName?.Trim() ?? "",
            SessionDate = sessionDate ?? "",
            SessionLink = sessionLink,
            RecordingLink = recordingLink,
            Notes = notes,
            Status = status,
        };
        db.SessionLogs.Add(session);
        await db.SaveChangesAsync();

        var uploadRoot = Path.Combine(env.WebRootPath, "uploads", "sessions", session.Id.ToString());
        Directory.CreateDirectory(uploadRoot);

        async Task SaveFiles(IEnumerable<IFormFile>? files, string kind)
        {
            if (files == null) return;
            foreach (var file in files.Where(f => f.Length > 0))
            {
                var safe = Path.GetFileName(file.FileName);
                var ext = Path.GetExtension(safe);
                var stored = $"{Guid.NewGuid():N}{ext}";
                var path = Path.Combine(uploadRoot, stored);
                await using (var fs = System.IO.File.Create(path))
                    await file.CopyToAsync(fs);
                var relative = $"/uploads/sessions/{session.Id}/{stored}";
                db.SessionFiles.Add(new SessionFile
                {
                    SessionLogId = session.Id,
                    Kind = kind,
                    StoredPath = relative,
                    OriginalName = safe,
                });
            }
        }

        await SaveFiles(attendanceFiles, "attendance");
        await SaveFiles(screenshots, "screenshot");
        await db.SaveChangesAsync();

        var full = await db.SessionLogs.Include(x => x.Coordinator).Include(x => x.Files).FirstAsync(x => x.Id == session.Id);
        return CreatedAtAction(nameof(Get), new { id = session.Id }, Map(full));
    }

    public record SessionDecideBody(string Status);

    [HttpPost("{id:int}/decide")]
    [Authorize(Roles = Roles.AdminOrManager)]
    public async Task<ActionResult<SessionDto>> Decide(int id, [FromBody] SessionDecideBody body)
    {
        var s = await db.SessionLogs.Include(x => x.Coordinator).Include(x => x.Files).FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();
        if (body.Status is not ("approved" or "rejected")) return BadRequest();
        s.Status = body.Status;
        await db.SaveChangesAsync();
        return Ok(Map(s));
    }

    private SessionDto Map(SessionLog s) =>
        new(
            s.Id,
            s.CoordinatorUserId,
            s.Coordinator.FullName,
            s.GroupCode,
            s.TrainerName,
            s.SessionDate,
            s.SessionLink,
            s.RecordingLink,
            s.Notes,
            s.Status,
            s.CreatedAt,
            s.Files.Select(f => new SessionFileDto(f.Kind, $"{Request.Scheme}://{Request.Host}{f.StoredPath}", f.OriginalName)).ToList());
}
