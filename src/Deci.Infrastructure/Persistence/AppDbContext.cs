using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deci.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<MasterTrainer> MasterTrainers => Set<MasterTrainer>();
    public DbSet<MasterGroupCode> MasterGroupCodes => Set<MasterGroupCode>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<SessionLog> SessionLogs => Set<SessionLog>();
    public DbSet<SessionFile> SessionFiles => Set<SessionFile>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<DeciTask> Tasks => Set<DeciTask>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(e => e.HasIndex(x => x.Email).IsUnique());
        modelBuilder.Entity<MasterGroupCode>(e => e.HasIndex(x => x.Code).IsUnique());

        modelBuilder.Entity<SessionLog>(e =>
        {
            e.HasOne(x => x.Coordinator).WithMany(x => x.Sessions).HasForeignKey(x => x.CoordinatorUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SessionFile>(e =>
        {
            e.HasOne(x => x.SessionLog).WithMany(x => x.Files).HasForeignKey(x => x.SessionLogId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AttendanceRecord>(e =>
        {
            e.HasOne(x => x.User).WithMany(x => x.AttendanceRecords).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeaveRequest>(e =>
        {
            e.HasOne(x => x.User).WithMany(x => x.LeaveRequests).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeciTask>(e =>
        {
            e.HasOne(x => x.Assignee).WithMany(x => x.AssignedTasks).HasForeignKey(x => x.AssigneeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CreatedBy).WithMany(x => x.CreatedTasks).HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasOne(x => x.User).WithMany(x => x.ChatMessages).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
