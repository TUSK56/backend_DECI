using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    [MaxLength(4000)]
    public string Text { get; set; } = "";

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
