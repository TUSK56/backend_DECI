using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class AttendanceRecord
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public DateTime ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }

    [MaxLength(64)]
    public string? Ip { get; set; }

    [MaxLength(64)]
    public string DateLabel { get; set; } = "";
}
