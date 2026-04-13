using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class AppUser
{
    public int Id { get; set; }

    [MaxLength(256)]
    public string Email { get; set; } = "";

    [MaxLength(256)]
    public string PasswordHash { get; set; } = "";

    [MaxLength(256)]
    public string FullName { get; set; } = "";

    [MaxLength(64)]
    public string? Phone { get; set; }

    [MaxLength(512)]
    public string? ProfileImagePath { get; set; }

    [MaxLength(32)]
    public string Role { get; set; } = Roles.Coordinator;

    public bool IsActive { get; set; } = true;

    /// <summary>When false, the user must set email, password, and phone before other API access.</summary>
    public bool ProfileCompleted { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SessionLog> Sessions { get; set; } = new List<SessionLog>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<DeciTask> AssignedTasks { get; set; } = new List<DeciTask>();
    public ICollection<DeciTask> CreatedTasks { get; set; } = new List<DeciTask>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
