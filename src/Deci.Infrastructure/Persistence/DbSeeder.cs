using Deci.Application.Interfaces;
using Deci.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Deci.Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>Default password for seeded accounts (change after first login in production).</summary>
    public const string SeedPeerPassword = "Deci123!";

    public static async Task SeedAsync(IAppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        if (!await db.SystemSettings.AnyAsync(ct))
            db.SystemSettings.Add(new SystemSetting { Id = 1 });

        const string pwd = SeedPeerPassword;

        const string adminEmail = "admin@deci.local";
        if (!await db.Users.AnyAsync(u => u.Email.ToLower() == adminEmail, ct))
        {
            db.Users.Add(new AppUser
            {
                Email = adminEmail,
                PasswordHash = hasher.Hash(pwd),
                FullName = "Administrator",
                Phone = "",
                Role = Roles.Admin,
                IsActive = true,
                ProfileCompleted = false,
            });
        }

        var coordinators = new (string Email, string FullName)[]
        {
            ("account1@deci.local", "Account One"),
            ("account2@deci.local", "Account Two"),
            ("account3@deci.local", "Account Three"),
            ("account4@deci.local", "Account Four"),
            ("account5@deci.local", "Account Five"),
            ("account6@deci.local", "Account Six"),
            ("account7@deci.local", "Account Seven"),
        };

        foreach (var (email, fullName) in coordinators)
        {
            var e = email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(u => u.Email.ToLower() == e, ct)) continue;

            db.Users.Add(new AppUser
            {
                Email = email,
                PasswordHash = hasher.Hash(pwd),
                FullName = fullName,
                Phone = "",
                Role = Roles.Coordinator,
                IsActive = true,
                ProfileCompleted = false,
            });
        }

        var legacyPeerEmails = coordinators.Select(x => x.Email.Trim().ToLowerInvariant()).ToHashSet();
        var legacyManagers = await db.Users
            .Where(u => u.Role == Roles.Manager && legacyPeerEmails.Contains(u.Email.ToLower()))
            .ToListAsync(ct);
        foreach (var u in legacyManagers)
            u.Role = Roles.Coordinator;

        if (!await db.MasterTrainers.AnyAsync(ct))
        {
            foreach (var n in new[] { "Ahmed Hassan", "Fatima Ali", "Omar Khalid", "Layla Ibrahim", "Yusuf Nasser", "Nour Samir", "External Trainer" })
                db.MasterTrainers.Add(new MasterTrainer { Name = n });
        }

        if (!await db.MasterGroupCodes.AnyAsync(ct))
        {
            foreach (var c in new[] { "ON-CA-L1-G0001", "ON-CA-L1-G0002", "DEMO-GROUP-01" })
                db.MasterGroupCodes.Add(new MasterGroupCode { Code = c });
        }

        await db.SaveChangesAsync(ct);
    }
}
