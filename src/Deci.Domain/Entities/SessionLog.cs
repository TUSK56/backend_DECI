using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class SessionLog
{
    public int Id { get; set; }

    public int CoordinatorUserId { get; set; }
    public AppUser Coordinator { get; set; } = null!;

    [MaxLength(128)]
    public string GroupCode { get; set; } = "";

    [MaxLength(256)]
    public string TrainerName { get; set; } = "";

    [MaxLength(32)]
    public string SessionDate { get; set; } = "";

    [MaxLength(2048)]
    public string? SessionLink { get; set; }

    [MaxLength(2048)]
    public string? RecordingLink { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SessionFile> Files { get; set; } = new List<SessionFile>();
}
