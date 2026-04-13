using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class AppNotification
{
    public int Id { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = "";

    [MaxLength(2000)]
    public string Body { get; set; } = "";

    public int? ForUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Read { get; set; }
}
