using Deci.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deci.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<AppUser> Users { get; }
    DbSet<MasterTrainer> MasterTrainers { get; }
    DbSet<MasterGroupCode> MasterGroupCodes { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<SessionLog> SessionLogs { get; }
    DbSet<SessionFile> SessionFiles { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<DeciTask> Tasks { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<AppNotification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
