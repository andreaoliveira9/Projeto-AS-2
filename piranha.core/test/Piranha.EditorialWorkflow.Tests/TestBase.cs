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
using Microsoft.Extensions.DependencyInjection;
using Piranha.Data.EditorialWorkflow;
using Piranha.Data.EF.EditorialWorkflow;

namespace Piranha.EditorialWorkflow.Tests;

/// <summary>
/// Test DbContext that extends Piranha's Db with Editorial Workflow support
/// </summary>
public class TestEditorialWorkflowDb : Db<TestEditorialWorkflowDb>, IEditorialWorkflowDb
{
    // Editorial Workflow DbSets
    public DbSet<Data.EditorialWorkflow.WorkflowDefinition> WorkflowDefinitions { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowState> WorkflowStates { get; set; }
    public DbSet<Data.EditorialWorkflow.TransitionRule> TransitionRules { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowInstance> WorkflowInstances { get; set; }
    public DbSet<Data.EditorialWorkflow.WorkflowContentExtension> WorkflowContentExtensions { get; set; }

    public TestEditorialWorkflowDb(DbContextOptions<TestEditorialWorkflowDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Editorial Workflow entities
        modelBuilder.ConfigureEditorialWorkflow();
    }
}

/// <summary>
/// Base class for Editorial Workflow tests
/// </summary>
public abstract class EditorialWorkflowTestBase : IDisposable
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly TestEditorialWorkflowDb _db;

    protected EditorialWorkflowTestBase()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database
        services.AddDbContext<TestEditorialWorkflowDb>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add Editorial Workflow repositories
        services.AddEditorialWorkflowRepositories();
        
        _serviceProvider = services.BuildServiceProvider();
        _db = _serviceProvider.GetRequiredService<TestEditorialWorkflowDb>();
        
        // Ensure database is created
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db?.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
