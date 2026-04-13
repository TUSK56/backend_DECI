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
public class ChatController(IAppDbContext db) : ControllerBase
{
    public record ChatDto(int Id, int UserId, string UserName, string Initials, string Text, DateTime SentAt);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatDto>>> List([FromQuery] int take = 200)
    {
        var rows = await db.ChatMessages.AsNoTracking().Include(m => m.User).OrderByDescending(m => m.SentAt).Take(take).ToListAsync();
        rows.Reverse();
        return Ok(rows.Select(m => new ChatDto(m.Id, m.UserId, m.User.FullName, Initials(m.User.FullName), m.Text, m.SentAt)));
    }

    public record PostMessage(string Text);

    [HttpPost]
    public async Task<ActionResult<ChatDto>> Post([FromBody] PostMessage body)
    {
        if (string.IsNullOrWhiteSpace(body.Text)) return BadRequest();
        var uid = User.GetUserId();
        var user = await db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();
        var msg = new ChatMessage { UserId = uid, Text = body.Text.Trim() };
        db.ChatMessages.Add(msg);
        await db.SaveChangesAsync();
        var loaded = await db.ChatMessages.Include(m => m.User).FirstAsync(m => m.Id == msg.Id);
        return Ok(new ChatDto(loaded.Id, loaded.UserId, loaded.User.FullName, Initials(loaded.User.FullName), loaded.Text, loaded.SentAt));
    }

    private static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return "?";
        if (parts.Length == 1) return parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }
}
