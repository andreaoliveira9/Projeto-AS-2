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
using Piranha;
using Piranha.Data.EditorialWorkflow;

namespace MvcWeb.Data;

/// <summary>
/// SQLite database context with Editorial Workflow support.
/// </summary>
public sealed class SQLiteWorkflowDb : Db<SQLiteWorkflowDb>, IEditorialWorkflowDb
{
    /// <summary>
    /// Gets/sets the workflow definitions set.
    /// </summary>
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }

    /// <summary>
    /// Gets/sets the workflow states set.
    /// </summary>
    public DbSet<WorkflowState> WorkflowStates { get; set; }

    /// <summary>
    /// Gets/sets the transition rules set.
    /// </summary>
    public DbSet<TransitionRule> TransitionRules { get; set; }

    /// <summary>
    /// Gets/sets the workflow instances set.
    /// </summary>
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; }

    /// <summary>
    /// Gets/sets the workflow content extensions set.
    /// </summary>
    public DbSet<WorkflowContentExtension> WorkflowContentExtensions { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="options">Configuration options</param>
    public SQLiteWorkflowDb(DbContextOptions<SQLiteWorkflowDb> options) : base(options)
    {
    }

    /// <summary>
    /// Creates and configures the data model.
    /// </summary>
    /// <param name="mb">The current model builder</param>
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        
        // Configure Editorial Workflow entities
        mb.ConfigureEditorialWorkflow();
    }
}
