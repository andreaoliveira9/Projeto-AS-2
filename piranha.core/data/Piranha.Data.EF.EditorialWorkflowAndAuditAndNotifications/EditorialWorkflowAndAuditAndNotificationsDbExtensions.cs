/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

#nullable enable

using Microsoft.EntityFrameworkCore;

namespace Piranha.Data.EditorialWorkflowAndAuditAndNotifications;

using Piranha.Data.EditorialWorkflow;
using Piranha.Data.Audit;
using Piranha.Data.Notifications;

/// <summary>
/// Interface for the Editorial Workflow database context.
/// </summary>
public interface IEditorialWorkflowDb
{
    /// <summary>
    /// Gets/sets the workflow definitions set.
    /// </summary>
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }

    /// <summary>
    /// Gets/sets the workflow states set.
    /// </summary>
    DbSet<WorkflowState> WorkflowStates { get; set; }

    /// <summary>
    /// Gets/sets the transition rules set.
    /// </summary>
    DbSet<TransitionRule> TransitionRules { get; set; }

    /// <summary>
    /// Gets/sets the workflow instances set.
    /// </summary>
    DbSet<WorkflowInstance> WorkflowInstances { get; set; }

    /// <summary>
    /// Gets/sets the workflow content extensions set.
    /// </summary>
    DbSet<WorkflowContentExtension> WorkflowContentExtensions { get; set; }
}

public interface IAuditDb
{
    /// <summary>
    /// Gets/sets the state change records set.
    /// </summary>
    DbSet<StateChangeRecord> StateChangeRecords { get; set; }
}

public interface INotificationsDb
{
    /// <summary>
    /// Gets/sets the notifications set.
    /// </summary>
    DbSet<Notification> Notifications { get; set; }
}

/// <summary>
/// Extensions for configuring Editorial Workflow, Audit and Notifications tables in an existing DbContext.
/// </summary>
public static class EditorialWorkflowAndAuditAndNotificationsDbExtensions
{
    /// <summary>
    /// Configures the Editorial Workflow, Audit and Notifications entities in the model builder.
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void ConfigureEditorialWorkflowAndAuditAndNotifications(this ModelBuilder modelBuilder)
    {
        // Configure table names with prefix
        modelBuilder.Entity<WorkflowDefinition>().ToTable("Piranha_WorkflowDefinitions");
        modelBuilder.Entity<WorkflowState>().ToTable("Piranha_WorkflowStates");
        modelBuilder.Entity<TransitionRule>().ToTable("Piranha_TransitionRules");
        modelBuilder.Entity<WorkflowInstance>().ToTable("Piranha_WorkflowInstances");
        modelBuilder.Entity<WorkflowContentExtension>().ToTable("Piranha_WorkflowContentExtensions");

        // WorkflowDefinition configuration
        var workflowDef = modelBuilder.Entity<WorkflowDefinition>();
        workflowDef.HasKey(w => w.Id);
        workflowDef.Property(w => w.Name).IsRequired().HasMaxLength(100);
        workflowDef.Property(w => w.Description).HasMaxLength(500);
        workflowDef.Property(w => w.CreatedBy).IsRequired().HasMaxLength(450);
        workflowDef.Property(w => w.LastModifiedBy).HasMaxLength(450);
        workflowDef.HasIndex(w => w.Name).IsUnique();
        workflowDef.HasIndex(w => w.IsActive);
        workflowDef.HasIndex(w => w.Created);
        workflowDef.HasMany(w => w.States)
            .WithOne(s => s.WorkflowDefinition)
            .HasForeignKey(s => s.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        workflowDef.HasMany(w => w.Instances)
            .WithOne(i => i.WorkflowDefinition)
            .HasForeignKey(i => i.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkflowState configuration
        var workflowState = modelBuilder.Entity<WorkflowState>();
        workflowState.HasKey(s => s.Id);
        workflowState.Property(s => s.StateId).IsRequired().HasMaxLength(50);
        workflowState.Property(s => s.Name).IsRequired().HasMaxLength(100);
        workflowState.Property(s => s.Description).HasMaxLength(500);
        workflowState.Property(s => s.ColorCode).HasMaxLength(7);
        workflowState.HasIndex(s => new { s.WorkflowDefinitionId, s.StateId }).IsUnique();
        workflowState.HasIndex(s => new { s.WorkflowDefinitionId, s.IsInitial });
        workflowState.HasIndex(s => new { s.WorkflowDefinitionId, s.IsPublished });
        workflowState.HasIndex(s => new { s.WorkflowDefinitionId, s.SortOrder });
        workflowState.HasOne(s => s.WorkflowDefinition)
            .WithMany(w => w.States)
            .HasForeignKey(s => s.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        workflowState.HasMany(s => s.OutgoingTransitions)
            .WithOne(t => t.FromState)
            .HasForeignKey(t => t.FromStateId)
            .OnDelete(DeleteBehavior.Cascade);
        workflowState.HasMany(s => s.IncomingTransitions)
            .WithOne(t => t.ToState)
            .HasForeignKey(t => t.ToStateId)
            .OnDelete(DeleteBehavior.Restrict);
        workflowState.HasMany(s => s.CurrentInstances)
            .WithOne(i => i.CurrentState)
            .HasForeignKey(i => i.CurrentStateId)
            .OnDelete(DeleteBehavior.Restrict);

        // TransitionRule configuration
        var transitionRule = modelBuilder.Entity<TransitionRule>();
        transitionRule.HasKey(t => t.Id);
        transitionRule.Property(t => t.Description).HasMaxLength(500);
        transitionRule.Property(t => t.CommentTemplate).HasMaxLength(200);
        transitionRule.Property(t => t.AllowedRoles).IsRequired().HasDefaultValue("[]");
        transitionRule.HasIndex(t => new { t.FromStateId, t.ToStateId }).IsUnique();
        transitionRule.HasIndex(t => t.FromStateId);
        transitionRule.HasIndex(t => t.ToStateId);
        transitionRule.HasIndex(t => t.IsActive);
        transitionRule.HasOne(t => t.FromState)
            .WithMany(s => s.OutgoingTransitions)
            .HasForeignKey(t => t.FromStateId)
            .OnDelete(DeleteBehavior.Cascade);
        transitionRule.HasOne(t => t.ToState)
            .WithMany(s => s.IncomingTransitions)
            .HasForeignKey(t => t.ToStateId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkflowInstance configuration
        var workflowInstance = modelBuilder.Entity<WorkflowInstance>();
        workflowInstance.HasKey(i => i.Id);
        workflowInstance.Property(i => i.ContentId).IsRequired().HasMaxLength(450);
        workflowInstance.Property(i => i.ContentType).IsRequired().HasMaxLength(50);
        workflowInstance.Property(i => i.ContentTitle).HasMaxLength(200);
        workflowInstance.Property(i => i.CreatedBy).IsRequired().HasMaxLength(450);
        workflowInstance.Property(i => i.Status).HasConversion<int>().HasDefaultValue(WorkflowInstanceStatus.Active);
        workflowInstance.HasIndex(i => i.ContentId);
        workflowInstance.HasIndex(i => new { i.ContentId, i.Status });
        workflowInstance.HasIndex(i => i.WorkflowDefinitionId);
        workflowInstance.HasIndex(i => i.CurrentStateId);
        workflowInstance.HasIndex(i => i.CreatedBy);
        workflowInstance.HasIndex(i => i.Status);
        workflowInstance.HasIndex(i => i.LastModified);
        workflowInstance.HasOne(i => i.WorkflowDefinition)
            .WithMany(w => w.Instances)
            .HasForeignKey(i => i.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
        workflowInstance.HasOne(i => i.CurrentState)
            .WithMany(s => s.CurrentInstances)
            .HasForeignKey(i => i.CurrentStateId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkflowContentExtension configuration
        var workflowContentExt = modelBuilder.Entity<WorkflowContentExtension>();
        workflowContentExt.HasKey(e => e.Id);
        workflowContentExt.Property(e => e.ContentId).IsRequired().HasMaxLength(450);
        workflowContentExt.HasIndex(e => e.ContentId).IsUnique();
        workflowContentExt.HasIndex(e => e.CurrentWorkflowInstanceId);
        workflowContentExt.HasOne(e => e.CurrentWorkflowInstance)
            .WithMany()
            .HasForeignKey(e => e.CurrentWorkflowInstanceId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure table name with prefix
        modelBuilder.Entity<StateChangeRecord>().ToTable("Piranha_StateChangeRecords");

        // StateChangeRecord configuration
        var stateChangeRecord = modelBuilder.Entity<StateChangeRecord>();
        stateChangeRecord.HasKey(s => s.Id);
        stateChangeRecord.Property(s => s.ContentId).IsRequired();
        stateChangeRecord.Property(s => s.ContentName).IsRequired().HasMaxLength(100);
        stateChangeRecord.Property(s => s.FromState).IsRequired().HasMaxLength(100);
        stateChangeRecord.Property(s => s.ToState).IsRequired().HasMaxLength(100);
        stateChangeRecord.Property(s => s.transitionDescription).IsRequired().HasMaxLength(500);
        stateChangeRecord.Property(s => s.reviewedBy).IsRequired().HasMaxLength(256);
        stateChangeRecord.Property(s => s.approved).IsRequired().HasDefaultValue(true);
        stateChangeRecord.Property(s => s.Timestamp).IsRequired();
        stateChangeRecord.Property(s => s.Comments).HasMaxLength(1000);
        stateChangeRecord.Property(s => s.Success).IsRequired().HasDefaultValue(true);
        stateChangeRecord.Property(s => s.ErrorMessage).HasMaxLength(2000);

        // Indexes for better query performance
        stateChangeRecord.HasIndex(s => s.ContentId);
        stateChangeRecord.HasIndex(s => new { s.ContentId, s.Timestamp });
        stateChangeRecord.HasIndex(s => s.reviewedBy);
        stateChangeRecord.HasIndex(s => s.approved);
        stateChangeRecord.HasIndex(s => s.Timestamp);
        stateChangeRecord.HasIndex(s => new { s.FromState, s.ToState });
        stateChangeRecord.HasIndex(s => s.Success);

        // Configure table names with prefix for notifications (TPH - Single table)
        modelBuilder.Entity<Notification>().ToTable("Piranha_Notifications");

        // Notification configuration (base class)
        var notification = modelBuilder.Entity<Notification>();
        notification.HasKey(n => n.Id);
        notification.Property(n => n.Timestamp).IsRequired();
        notification.HasIndex(n => n.Timestamp);
        
        // Configure TPH inheritance with discriminator
        notification.HasDiscriminator<string>("NotificationType")
            .HasValue<Notification>("Base")
            .HasValue<StateChangedNotification>("StateChanged");

        // StateChangedNotification configuration
        var stateChangedNotification = modelBuilder.Entity<StateChangedNotification>();
        stateChangedNotification.Property(n => n.ContentId).IsRequired();
        stateChangedNotification.Property(n => n.ContentName).IsRequired().HasMaxLength(100);
        stateChangedNotification.Property(n => n.FromState).IsRequired().HasMaxLength(100);
        stateChangedNotification.Property(n => n.ToState).IsRequired().HasMaxLength(100);
        stateChangedNotification.Property(n => n.TransitionDescription).IsRequired().HasMaxLength(500);
        stateChangedNotification.Property(n => n.ReviewedBy).IsRequired().HasMaxLength(256);
        stateChangedNotification.Property(n => n.Approved).IsRequired().HasDefaultValue(true);
        
        // Indexes for StateChangedNotification
        stateChangedNotification.HasIndex(n => n.ContentId);
        stateChangedNotification.HasIndex(n => new { n.ContentId, n.Timestamp });
        stateChangedNotification.HasIndex(n => n.ReviewedBy);
        stateChangedNotification.HasIndex(n => n.Approved);
        stateChangedNotification.HasIndex(n => new { n.FromState, n.ToState });
    }
}
