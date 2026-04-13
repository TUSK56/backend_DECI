namespace Deci.Domain.Entities;

/// <summary>Singleton row (Id = 1) for global admin settings.</summary>
public class SystemSetting
{
    public int Id { get; set; } = 1;

    public TimeSpan ShiftStart { get; set; } = TimeSpan.FromHours(9);

    public TimeSpan ShiftEnd { get; set; } = TimeSpan.FromHours(17);

    public bool IpTrackingEnabled { get; set; } = true;

    public bool SessionApprovalRequired { get; set; } = true;
}
