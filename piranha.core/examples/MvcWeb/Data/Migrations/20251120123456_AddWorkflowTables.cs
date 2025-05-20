using Microsoft.EntityFrameworkCore.Migrations;

namespace MvcWeb.Data.Migrations;

/// <summary>
/// Migration for adding workflow state tables to the database.
/// </summary>
public partial class AddWorkflowTables : Migration
{
    /// <summary>
    /// Upgrades the schema.
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ContentWorkflowStates table
        migrationBuilder.CreateTable(
            name: "Piranha_ContentWorkflowStates",
            columns: table => new
            {
                ContentId = table.Column<Guid>(nullable: false),
                WorkflowName = table.Column<string>(maxLength: 64, nullable: false),
                CurrentStateId = table.Column<string>(maxLength: 64, nullable: false),
                StateChangedAt = table.Column<DateTime>(nullable: false),
                StateChangedBy = table.Column<string>(maxLength: 128, nullable: true),
                StateChangeComment = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_ContentWorkflowStates", x => x.ContentId);
            });

        // ContentWorkflowStateTransitions table
        migrationBuilder.CreateTable(
            name: "Piranha_ContentWorkflowStateTransitions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                ContentId = table.Column<Guid>(nullable: false),
                FromStateId = table.Column<string>(maxLength: 64, nullable: true),
                ToStateId = table.Column<string>(maxLength: 64, nullable: false),
                TransitionedAt = table.Column<DateTime>(nullable: false),
                TransitionedBy = table.Column<string>(maxLength: 128, nullable: true),
                Comment = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_ContentWorkflowStateTransitions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Piranha_ContentWorkflowStateTransitions_Piranha_ContentWorkflowStates_ContentId",
                    column: x => x.ContentId,
                    principalTable: "Piranha_ContentWorkflowStates",
                    principalColumn: "ContentId",
                    onDelete: ReferentialAction.Cascade);
            });

        // Create indexes
        migrationBuilder.CreateIndex(
            name: "IX_Piranha_ContentWorkflowStates_WorkflowName_CurrentStateId",
            table: "Piranha_ContentWorkflowStates",
            columns: new[] { "WorkflowName", "CurrentStateId" });

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_ContentWorkflowStateTransitions_ContentId",
            table: "Piranha_ContentWorkflowStateTransitions",
            column: "ContentId");
    }

    /// <summary>
    /// Downgrades the schema.
    /// </summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Piranha_ContentWorkflowStateTransitions");

        migrationBuilder.DropTable(
            name: "Piranha_ContentWorkflowStates");
    }
}
