using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Deci.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProfileCompleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(
                """
                UPDATE [Users] SET [ProfileCompleted] = 0
                WHERE [Email] LIKE N'account%@deci.local'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileCompleted",
                table: "Users");
        }
    }
}
