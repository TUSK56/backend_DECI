using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeSeededRolesAdminAndCoordinators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE [Users] SET [Role] = N'Coordinator', [ProfileCompleted] = 0
                WHERE [Email] IN (
                    N'account1@deci.local', N'account2@deci.local', N'account3@deci.local',
                    N'account4@deci.local', N'account5@deci.local', N'account6@deci.local',
                    N'account7@deci.local'
                );

                UPDATE [Users] SET [ProfileCompleted] = 0
                WHERE [Email] = N'admin@deci.local';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
