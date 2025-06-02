using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcWeb.Migrations
{
    /// <inheritdoc />
    public partial class CommentsToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Piranha_Notifications",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Piranha_Notifications");
        }
    }
}
