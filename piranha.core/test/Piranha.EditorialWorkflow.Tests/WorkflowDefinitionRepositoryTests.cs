/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Repositories;
using Xunit;

namespace Piranha.EditorialWorkflow.Tests;

/// <summary>
/// Basic tests for WorkflowDefinition repository
/// </summary>
public class WorkflowDefinitionRepositoryTests : EditorialWorkflowTestBase
{
    private readonly IWorkflowDefinitionRepository _repository;

    public WorkflowDefinitionRepositoryTests()
    {
        _repository = _serviceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
    }

    [Fact]
    public async Task Can_Create_And_Retrieve_WorkflowDefinition()
    {
        // Arrange
        var workflowDefinition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Test Workflow",
            Description = "A test workflow for unit testing",
            IsActive = true,
            CreatedBy = "test-user",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        // Act
        await _repository.Save(workflowDefinition);
        var retrieved = await _repository.GetById(workflowDefinition.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(workflowDefinition.Name, retrieved.Name);
        Assert.Equal(workflowDefinition.Description, retrieved.Description);
        Assert.Equal(workflowDefinition.IsActive, retrieved.IsActive);
        Assert.Equal(workflowDefinition.CreatedBy, retrieved.CreatedBy);
    }

    [Fact]
    public async Task Can_Get_Active_Workflows()
    {
        // Arrange
        var activeWorkflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Active Workflow",
            IsActive = true,
            CreatedBy = "test-user",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var inactiveWorkflow = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Workflow",
            IsActive = false,
            CreatedBy = "test-user",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _repository.Save(activeWorkflow);
        await _repository.Save(inactiveWorkflow);

        // Act
        var activeWorkflows = await _repository.GetActive();

        // Assert
        Assert.Single(activeWorkflows);
        Assert.Equal(activeWorkflow.Name, activeWorkflows.First().Name);
    }

    [Fact]
    public async Task Can_Check_If_Workflow_Exists_By_Name()
    {
        // Arrange
        var workflowDefinition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Unique Workflow Name",
            CreatedBy = "test-user",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _repository.Save(workflowDefinition);

        // Act & Assert
        Assert.True(await _repository.ExistsByName("Unique Workflow Name"));
        Assert.False(await _repository.ExistsByName("Non-existent Workflow"));
    }

    [Fact]
    public async Task Can_Delete_Workflow()
    {
        // Arrange
        var workflowDefinition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Workflow To Delete",
            CreatedBy = "test-user",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _repository.Save(workflowDefinition);

        // Act
        await _repository.Delete(workflowDefinition.Id);
        var retrieved = await _repository.GetById(workflowDefinition.Id);

        // Assert
        Assert.Null(retrieved);
    }
}
