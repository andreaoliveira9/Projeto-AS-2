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
        
        // Configure Editorial Workflow entities
        ConfigureEditorialWorkflow(modelBuilder);
    }

    private void ConfigureEditorialWorkflow(ModelBuilder modelBuilder)
    {
        // Configure WorkflowDefinition
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Created).IsRequired();
        });

        // Configure WorkflowState
        modelBuilder.Entity<WorkflowState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsInitial).HasDefaultValue(false);
            entity.Property(e => e.IsFinal).HasDefaultValue(false);
            
            entity.HasOne(e => e.WorkflowDefinition)
                  .WithMany(w => w.States)
                  .HasForeignKey(e => e.WorkflowDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TransitionRule
        modelBuilder.Entity<TransitionRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CommentTemplate).HasMaxLength(500);
            entity.Property(e => e.AllowedRoles).HasMaxLength(1000).HasDefaultValue("[]");
            entity.Property(e => e.RequiresComment).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Created).IsRequired();
            
            entity.HasOne(e => e.FromState)
                  .WithMany(s => s.OutgoingTransitions)
                  .HasForeignKey(e => e.FromStateId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.ToState)
                  .WithMany(s => s.IncomingTransitions)
                  .HasForeignKey(e => e.ToStateId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure WorkflowInstance - Fix foreign key conflicts
        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContentId).IsRequired();
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            
            // Fix enum configuration - store as integer without specifying default value
            entity.Property(e => e.Status)
                  .HasConversion<int>()
                  .IsRequired();
            
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.LastModified).IsRequired();
            
            // Explicitly configure relationships with proper foreign key names
            entity.HasOne(e => e.WorkflowDefinition)
                  .WithMany()
                  .HasForeignKey(e => e.WorkflowDefinitionId)
                  .HasConstraintName("FK_WorkflowInstance_WorkflowDefinition")
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.CurrentState)
                  .WithMany()
                  .HasForeignKey(e => e.CurrentStateId)
                  .HasConstraintName("FK_WorkflowInstance_CurrentState")
                  .OnDelete(DeleteBehavior.Restrict);
                  
            // Add indexes for better query performance
            entity.HasIndex(e => e.ContentId);
            entity.HasIndex(e => new { e.ContentId, e.Status });
        });

        // Configure WorkflowContentExtension
        modelBuilder.Entity<WorkflowContentExtension>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ContentId).IsRequired();
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastWorkflowState).HasMaxLength(100);
            entity.Property(e => e.IsInWorkflow).HasDefaultValue(false);
            entity.Property(e => e.Created).IsRequired();
            entity.Property(e => e.LastModified).IsRequired();
            
            entity.HasOne(e => e.CurrentWorkflowInstance)
                  .WithMany()
                  .HasForeignKey(e => e.CurrentWorkflowInstanceId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
