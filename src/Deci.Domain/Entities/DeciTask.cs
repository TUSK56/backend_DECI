using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class DeciTask
{
    public int Id { get; set; }

    [MaxLength(512)]
    public string Title { get; set; } = "";

    [MaxLength(4000)]
    public string? Description { get; set; }

    public int AssigneeId { get; set; }
    public AppUser Assignee { get; set; } = null!;

    public int CreatedById { get; set; }
    public AppUser CreatedBy { get; set; } = null!;

    [MaxLength(32)]
    public string Priority { get; set; } = "medium";

    [MaxLength(32)]
    public string? Deadline { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "todo";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
