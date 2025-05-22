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
/// Integration tests that test the complete workflow scenario
/// </summary>
public class WorkflowIntegrationTests : EditorialWorkflowTestBase
{
    private readonly IWorkflowDefinitionRepository _workflowRepo;
    private readonly IWorkflowStateRepository _stateRepo;
    private readonly ITransitionRuleRepository _transitionRepo;
    private readonly IWorkflowInstanceRepository _instanceRepo;

    public WorkflowIntegrationTests()
    {
        _workflowRepo = _serviceProvider.GetRequiredService<IWorkflowDefinitionRepository>();
        _stateRepo = _serviceProvider.GetRequiredService<IWorkflowStateRepository>();
        _transitionRepo = _serviceProvider.GetRequiredService<ITransitionRuleRepository>();
        _instanceRepo = _serviceProvider.GetRequiredService<IWorkflowInstanceRepository>();
    }

    [Fact]
    public async Task Can_Create_Complete_Workflow_From_Json_Example()
    {
        // Arrange - Create the workflow definition from your JSON example
        var workflowId = Guid.NewGuid();
        var workflow = new WorkflowDefinition
        {
            Id = workflowId,
            Name = "Standard Editorial Workflow",
            Description = "Standard editorial workflow with draft, review, approval, and publish stages",
            IsActive = true,
            CreatedBy = "admin",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _workflowRepo.Save(workflow);

        // Create states
        var draftStateId = Guid.NewGuid();
        var reviewStateId = Guid.NewGuid();
        var legalReviewStateId = Guid.NewGuid();
        var approvedStateId = Guid.NewGuid();
        var publishedStateId = Guid.NewGuid();
        var archivedStateId = Guid.NewGuid();

        var states = new[]
        {
            new WorkflowState
            {
                Id = draftStateId,
                StateId = "draft",
                Name = "Draft",
                Description = "Initial content creation phase",
                IsInitial = true,
                SortOrder = 1,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            },
            new WorkflowState
            {
                Id = reviewStateId,
                StateId = "review",
                Name = "Review",
                Description = "Content being reviewed by editors",
                SortOrder = 2,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            },
            new WorkflowState
            {
                Id = legalReviewStateId,
                StateId = "legal_review",
                Name = "Legal Review",
                Description = "Content being reviewed by legal team",
                SortOrder = 3,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            },
            new WorkflowState
            {
                Id = approvedStateId,
                StateId = "approved",
                Name = "Approved",
                Description = "Content approved and ready for publication",
                SortOrder = 4,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            },
            new WorkflowState
            {
                Id = publishedStateId,
                StateId = "published",
                Name = "Published",
                Description = "Content is live on the site",
                IsPublished = true,
                SortOrder = 5,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            },
            new WorkflowState
            {
                Id = archivedStateId,
                StateId = "archived",
                Name = "Archived",
                Description = "Content is no longer active",
                IsFinal = true,
                SortOrder = 6,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            }
        };

        foreach (var state in states)
        {
            await _stateRepo.Save(state);
        }

        // Create transition rules based on your JSON
        var transitions = new[]
        {
            // From draft to review
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = draftStateId,
                ToStateId = reviewStateId,
                AllowedRoles = "[\"Editor\", \"Admin\"]",
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            },
            // From review back to draft
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = reviewStateId,
                ToStateId = draftStateId,
                AllowedRoles = "[\"Editor\", \"Admin\"]",
                CommentTemplate = "Requires revision",
                RequiresComment = true,
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            },
            // From review to legal review
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = reviewStateId,
                ToStateId = legalReviewStateId,
                AllowedRoles = "[\"Editor\", \"Admin\"]",
                CommentTemplate = "Approved by editor, needs legal review",
                IsActive = true,
                SortOrder = 2,
                Created = DateTime.UtcNow
            },
            // From review directly to published (Admin only)
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = reviewStateId,
                ToStateId = publishedStateId,
                AllowedRoles = "[\"Admin\"]",
                CommentTemplate = "Skip legal review (Admin only)",
                IsActive = true,
                SortOrder = 3,
                Created = DateTime.UtcNow
            },
            // From legal review to approved
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = legalReviewStateId,
                ToStateId = approvedStateId,
                AllowedRoles = "[\"LegalReviewer\", \"Admin\"]",
                CommentTemplate = "Approved by legal",
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            },
            // From approved to published
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = approvedStateId,
                ToStateId = publishedStateId,
                AllowedRoles = "[\"Admin\"]",
                CommentTemplate = "Final publication (Admin only)",
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            },
            // From published to archived
            new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = publishedStateId,
                ToStateId = archivedStateId,
                AllowedRoles = "[\"Admin\"]",
                CommentTemplate = "Archive content",
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            }
        };

        foreach (var transition in transitions)
        {
            await _transitionRepo.Save(transition);
        }

        // Act - Test workflow retrieval with states and transitions
        var completeWorkflow = await _workflowRepo.GetWithStatesAndTransitions(workflowId);

        // Assert
        Assert.NotNull(completeWorkflow);
        Assert.Equal("Standard Editorial Workflow", completeWorkflow.Name);
        Assert.Equal(6, completeWorkflow.States.Count);
        
        // Check initial state
        var initialState = completeWorkflow.States.FirstOrDefault(s => s.IsInitial);
        Assert.NotNull(initialState);
        Assert.Equal("draft", initialState.StateId);
        
        // Check published state
        var publishedState = completeWorkflow.States.FirstOrDefault(s => s.IsPublished);
        Assert.NotNull(publishedState);
        Assert.Equal("published", publishedState.StateId);

        // Check transitions exist
        var draftState = completeWorkflow.States.FirstOrDefault(s => s.StateId == "draft");
        Assert.NotNull(draftState);
        // Note: The transitions would be loaded via the OutgoingTransitions property
        // but we need to test this through the transition repository
        
        var draftTransitions = await _transitionRepo.GetByFromState(draftStateId);
        Assert.Single(draftTransitions);
        Assert.Contains("Editor", draftTransitions.First().AllowedRoles);
        Assert.Contains("Admin", draftTransitions.First().AllowedRoles);
    }

    [Fact]
    public async Task Can_Create_Workflow_Instance_For_Content()
    {
        // Arrange - Create a simple workflow first
        var workflowId = Guid.NewGuid();
        var workflow = new WorkflowDefinition
        {
            Id = workflowId,
            Name = "Simple Workflow",
            CreatedBy = "admin",
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        await _workflowRepo.Save(workflow);

        var initialStateId = Guid.NewGuid();
        var initialState = new WorkflowState
        {
            Id = initialStateId,
            StateId = "draft",
            Name = "Draft",
            IsInitial = true,
            WorkflowDefinitionId = workflowId,
            Created = DateTime.UtcNow
        };
        await _stateRepo.Save(initialState);

        // Act - Create a workflow instance for some content
        var contentId = Guid.NewGuid().ToString();
        var workflowInstance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            ContentId = contentId,
            ContentType = "Page",
            ContentTitle = "Test Page",
            WorkflowDefinitionId = workflowId,
            CurrentStateId = initialStateId,
            CreatedBy = "test-user",
            Status = WorkflowInstanceStatus.Active,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _instanceRepo.Save(workflowInstance);

        // Assert
        var retrieved = await _instanceRepo.GetByContent(contentId);
        Assert.NotNull(retrieved);
        Assert.Equal(contentId, retrieved.ContentId);
        Assert.Equal("Page", retrieved.ContentType);
        Assert.Equal(WorkflowInstanceStatus.Active, retrieved.Status);
        Assert.Equal(workflowId, retrieved.WorkflowDefinitionId);
        Assert.Equal(initialStateId, retrieved.CurrentStateId);

        // Test getting instances by state
        var instancesByState = await _instanceRepo.GetByState(initialStateId);
        Assert.Single(instancesByState);
        Assert.Equal(workflowInstance.Id, instancesByState.First().Id);
    }
}
