using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUserIfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Same BCrypt hash as DbSeeder / seed-seven-users.sql — password: Deci123!
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'admin@deci.local')
                BEGIN
                    INSERT INTO [Users] ([Email], [PasswordHash], [FullName], [Phone], [ProfileImagePath], [Role], [IsActive], [ProfileCompleted], [CreatedAt])
                    VALUES (
                        N'admin@deci.local',
                        N'$2a$11$kMqd9kE.yAPwkjMIXk3KS.IH7WSpJJgOnLPmXETU1iSkbucmidOvO',
                        N'Administrator',
                        N'',
                        NULL,
                        N'Admin',
                        1,
                        0,
                        SYSUTCDATETIME()
                    );
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
