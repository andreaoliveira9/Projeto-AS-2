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
        modelBuilder.Entity<StateChangeRecord>(entity =>
        {
            entity.ToTable("Piranha_StateChangeRecords");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .IsRequired();
            
            entity.Property(e => e.WorkflowInstanceId)
                .IsRequired();
            
            entity.Property(e => e.ContentId)
                .IsRequired();
            
            entity.Property(e => e.ContentType)
                .HasMaxLength(255)
                .IsRequired();
            
            entity.Property(e => e.FromState)
                .HasMaxLength(255);
            
            entity.Property(e => e.ToState)
                .HasMaxLength(255)
                .IsRequired();
            
            entity.Property(e => e.UserId)
                .HasMaxLength(128);
            
            entity.Property(e => e.Username)
                .HasMaxLength(255);
            
            entity.Property(e => e.Timestamp)
                .IsRequired();
            
            entity.Property(e => e.Comments)
                .HasMaxLength(1000);
            
            entity.Property(e => e.Metadata)
                .HasColumnType("TEXT");
            
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.ContentId);
            entity.HasIndex(e => e.WorkflowInstanceId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.FromState, e.ToState });
        });
    }
}
