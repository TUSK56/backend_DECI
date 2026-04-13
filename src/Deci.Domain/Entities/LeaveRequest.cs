using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class LeaveRequest
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    [MaxLength(64)]
    public string Type { get; set; } = "vacation";

    [MaxLength(32)]
    public string Start { get; set; } = "";

    [MaxLength(32)]
    public string End { get; set; } = "";

    [MaxLength(2000)]
    public string Reason { get; set; } = "";

    [MaxLength(32)]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
