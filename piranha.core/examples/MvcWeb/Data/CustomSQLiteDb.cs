using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.Data.EditorialWorkflow;

namespace MvcWeb.Data;

public class CustomSQLiteDb : Db<CustomSQLiteDb>, IEditorialWorkflowDb
{
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<WorkflowState> WorkflowStates { get; set; }
    public DbSet<TransitionRule> TransitionRules { get; set; }
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<WorkflowContentExtension> WorkflowContentExtensions { get; set; }

    public CustomSQLiteDb(DbContextOptions<CustomSQLiteDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Use the official Editorial Workflow configuration
        modelBuilder.ConfigureEditorialWorkflow();
    }
}
