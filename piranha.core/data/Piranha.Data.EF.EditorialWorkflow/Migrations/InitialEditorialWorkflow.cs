/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.EntityFrameworkCore.Migrations;

namespace Piranha.Data.EF.EditorialWorkflow.Migrations;

/// <summary>
/// Initial migration for Editorial Workflow module.
/// This migration script can be used as a template.
/// 
/// To generate a new migration, use:
/// dotnet ef migrations add InitialEditorialWorkflow --context YourDbContext
/// </summary>
public partial class InitialEditorialWorkflow : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create WorkflowDefinitions table
        migrationBuilder.CreateTable(
            name: "Piranha_WorkflowDefinitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                LastModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_WorkflowDefinitions", x => x.Id);
            });

        // Create WorkflowStates table
        migrationBuilder.CreateTable(
            name: "Piranha_WorkflowStates",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsInitial = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsFinal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                ColorCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                WorkflowDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_WorkflowStates", x => x.Id);
                table.ForeignKey(
                    name: "FK_Piranha_WorkflowStates_Piranha_WorkflowDefinitions_WorkflowDefinitionId",
                    column: x => x.WorkflowDefinitionId,
                    principalTable: "Piranha_WorkflowDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Create TransitionRules table
        migrationBuilder.CreateTable(
            name: "Piranha_TransitionRules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CommentTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                RequiresComment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                AllowedRoles = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                FromStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ToStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_TransitionRules", x => x.Id);
                table.ForeignKey(
                    name: "FK_Piranha_TransitionRules_Piranha_WorkflowStates_FromStateId",
                    column: x => x.FromStateId,
                    principalTable: "Piranha_WorkflowStates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Piranha_TransitionRules_Piranha_WorkflowStates_ToStateId",
                    column: x => x.ToStateId,
                    principalTable: "Piranha_WorkflowStates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Create WorkflowInstances table
        migrationBuilder.CreateTable(
            name: "Piranha_WorkflowInstances",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ContentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                ContentTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                WorkflowDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CurrentStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_WorkflowInstances", x => x.Id);
                table.ForeignKey(
                    name: "FK_Piranha_WorkflowInstances_Piranha_WorkflowDefinitions_WorkflowDefinitionId",
                    column: x => x.WorkflowDefinitionId,
                    principalTable: "Piranha_WorkflowDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Piranha_WorkflowInstances_Piranha_WorkflowStates_CurrentStateId",
                    column: x => x.CurrentStateId,
                    principalTable: "Piranha_WorkflowStates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Create WorkflowContentExtensions table
        migrationBuilder.CreateTable(
            name: "Piranha_WorkflowContentExtensions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ContentId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CurrentWorkflowInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsInWorkflow = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                LastWorkflowState = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Piranha_WorkflowContentExtensions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Piranha_WorkflowContentExtensions_Piranha_WorkflowInstances_CurrentWorkflowInstanceId",
                    column: x => x.CurrentWorkflowInstanceId,
                    principalTable: "Piranha_WorkflowInstances",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        // Create indexes for WorkflowDefinitions
        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowDefinitions_Name",
            table: "Piranha_WorkflowDefinitions",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowDefinitions_IsActive",
            table: "Piranha_WorkflowDefinitions",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowDefinitions_Created",
            table: "Piranha_WorkflowDefinitions",
            column: "Created");

        // Create indexes for WorkflowStates
        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_StateId",
            table: "Piranha_WorkflowStates",
            columns: new[] { "WorkflowDefinitionId", "StateId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_IsInitial",
            table: "Piranha_WorkflowStates",
            columns: new[] { "WorkflowDefinitionId", "IsInitial" });

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_IsPublished",
            table: "Piranha_WorkflowStates",
            columns: new[] { "WorkflowDefinitionId", "IsPublished" });

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowStates_WorkflowDefinitionId_SortOrder",
            table: "Piranha_WorkflowStates",
            columns: new[] { "WorkflowDefinitionId", "SortOrder" });

        // Create indexes for TransitionRules
        migrationBuilder.CreateIndex(
            name: "IX_Piranha_TransitionRules_FromStateId_ToStateId",
            table: "Piranha_TransitionRules",
            columns: new[] { "FromStateId", "ToStateId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_TransitionRules_FromStateId",
            table: "Piranha_TransitionRules",
            column: "FromStateId");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_TransitionRules_ToStateId",
            table: "Piranha_TransitionRules",
            column: "ToStateId");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_TransitionRules_IsActive",
            table: "Piranha_TransitionRules",
            column: "IsActive");

        // Create indexes for WorkflowInstances
        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_ContentId",
            table: "Piranha_WorkflowInstances",
            column: "ContentId");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_ContentId_Status",
            table: "Piranha_WorkflowInstances",
            columns: new[] { "ContentId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_WorkflowDefinitionId",
            table: "Piranha_WorkflowInstances",
            column: "WorkflowDefinitionId");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_CurrentStateId",
            table: "Piranha_WorkflowInstances",
            column: "CurrentStateId");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_CreatedBy",
            table: "Piranha_WorkflowInstances",
            column: "CreatedBy");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_Status",
            table: "Piranha_WorkflowInstances",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowInstances_LastModified",
            table: "Piranha_WorkflowInstances",
            column: "LastModified");

        // Create indexes for WorkflowContentExtensions
        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowContentExtensions_ContentId",
            table: "Piranha_WorkflowContentExtensions",
            column: "ContentId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowContentExtensions_ContentType",
            table: "Piranha_WorkflowContentExtensions",
            column: "ContentType");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowContentExtensions_IsInWorkflow",
            table: "Piranha_WorkflowContentExtensions",
            column: "IsInWorkflow");

        migrationBuilder.CreateIndex(
            name: "IX_Piranha_WorkflowContentExtensions_CurrentWorkflowInstanceId",
            table: "Piranha_WorkflowContentExtensions",
            column: "CurrentWorkflowInstanceId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop tables in reverse order to respect foreign key constraints
        migrationBuilder.DropTable(name: "Piranha_WorkflowContentExtensions");
        migrationBuilder.DropTable(name: "Piranha_WorkflowInstances");
        migrationBuilder.DropTable(name: "Piranha_TransitionRules");
        migrationBuilder.DropTable(name: "Piranha_WorkflowStates");
        migrationBuilder.DropTable(name: "Piranha_WorkflowDefinitions");
    }
}
