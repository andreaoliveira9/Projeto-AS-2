using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedAndReviewedByColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "approvedBy",
                table: "Piranha_StateChangeRecords",
                newName: "reviewedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Piranha_StateChangeRecords_approvedBy",
                table: "Piranha_StateChangeRecords",
                newName: "IX_Piranha_StateChangeRecords_reviewedBy");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "Piranha_Notifications",
                newName: "ReviewedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Piranha_Notifications_ApprovedBy",
                table: "Piranha_Notifications",
                newName: "IX_Piranha_Notifications_ReviewedBy");

            migrationBuilder.AddColumn<bool>(
                name: "approved",
                table: "Piranha_StateChangeRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Piranha_Notifications",
                type: "INTEGER",
                nullable: true,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_StateChangeRecords_approved",
                table: "Piranha_StateChangeRecords",
                column: "approved");

            migrationBuilder.CreateIndex(
                name: "IX_Piranha_Notifications_Approved",
                table: "Piranha_Notifications",
                column: "Approved");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Piranha_StateChangeRecords_approved",
                table: "Piranha_StateChangeRecords");

            migrationBuilder.DropIndex(
                name: "IX_Piranha_Notifications_Approved",
                table: "Piranha_Notifications");

            migrationBuilder.DropColumn(
                name: "approved",
                table: "Piranha_StateChangeRecords");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Piranha_Notifications");

            migrationBuilder.RenameColumn(
                name: "reviewedBy",
                table: "Piranha_StateChangeRecords",
                newName: "approvedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Piranha_StateChangeRecords_reviewedBy",
                table: "Piranha_StateChangeRecords",
                newName: "IX_Piranha_StateChangeRecords_approvedBy");

            migrationBuilder.RenameColumn(
                name: "ReviewedBy",
                table: "Piranha_Notifications",
                newName: "ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Piranha_Notifications_ReviewedBy",
                table: "Piranha_Notifications",
                newName: "IX_Piranha_Notifications_ApprovedBy");
        }
    }
}
