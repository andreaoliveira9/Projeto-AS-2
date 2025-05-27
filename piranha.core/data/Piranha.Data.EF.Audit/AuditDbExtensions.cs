/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.EntityFrameworkCore;

namespace Piranha.Data.EF.Audit;

/// <summary>
/// Interface for the Audit database context.
/// </summary>
public interface IAuditDb
{
    /// <summary>
    /// Gets/sets the state change records set.
    /// </summary>
    DbSet<StateChangeRecord> StateChangeRecord { get; set; }
}

/// <summary>
/// Extensions for configuring Audit tables in an existing DbContext.
/// </summary>
public static class AuditDbExtensions
{
    /// <summary>
    /// Configures the Audit entities in the model builder.
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void ConfigureAudit(this ModelBuilder modelBuilder)
    {
        // Configure table name with prefix
        modelBuilder.Entity<StateChangeRecord>().ToTable("Piranha_StateChangeRecords");

        // StateChangeRecord configuration
        var stateChangeRecord = modelBuilder.Entity<StateChangeRecord>();
        stateChangeRecord.HasKey(s => s.Id);
        stateChangeRecord.Property(s => s.WorkflowInstanceId).IsRequired();
        stateChangeRecord.Property(s => s.ContentId).IsRequired();
        stateChangeRecord.Property(s => s.ContentType).IsRequired().HasMaxLength(50);
        stateChangeRecord.Property(s => s.FromState).HasMaxLength(100);
        stateChangeRecord.Property(s => s.ToState).IsRequired().HasMaxLength(100);
        stateChangeRecord.Property(s => s.UserId).IsRequired().HasMaxLength(450);
        stateChangeRecord.Property(s => s.Username).HasMaxLength(256);
        stateChangeRecord.Property(s => s.Timestamp).IsRequired();
        stateChangeRecord.Property(s => s.Comments).HasMaxLength(1000);
        stateChangeRecord.Property(s => s.TransitionRuleId);
        stateChangeRecord.Property(s => s.Metadata).HasColumnType("nvarchar(max)");
        stateChangeRecord.Property(s => s.Success).IsRequired().HasDefaultValue(true);
        stateChangeRecord.Property(s => s.ErrorMessage).HasMaxLength(2000);
        
        // Indexes for better query performance
        stateChangeRecord.HasIndex(s => s.WorkflowInstanceId);
        stateChangeRecord.HasIndex(s => s.ContentId);
        stateChangeRecord.HasIndex(s => new { s.ContentId, s.Timestamp });
        stateChangeRecord.HasIndex(s => s.UserId);
        stateChangeRecord.HasIndex(s => s.Timestamp);
        stateChangeRecord.HasIndex(s => new { s.FromState, s.ToState });
        stateChangeRecord.HasIndex(s => s.TransitionRuleId);
        stateChangeRecord.HasIndex(s => s.Success);
    }
}
