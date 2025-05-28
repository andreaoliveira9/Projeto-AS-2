using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.Data.Audit;
using Piranha.Data.EditorialWorkflow;
using Piranha.Data.EditorialWorkflowAndAudit;

namespace MvcWeb.Data;

public class SQLiteModulesDb : Db<SQLiteModulesDb>, IEditorialWorkflowDb, IAuditDb
{
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowState> WorkflowStates { get; set; }
    public DbSet<TransitionRule> TransitionRules { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowContentExtension> WorkflowContentExtensions { get; set; }
    public DbSet<StateChangeRecord> StateChangeRecords { get; set; }

    public SQLiteModulesDb(DbContextOptions<SQLiteModulesDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Use the official Editorial Workflow configuration
        modelBuilder.ConfigureEditorialWorkflowAndAudit();
    }
}