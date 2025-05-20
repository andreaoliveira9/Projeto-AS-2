/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Workflow;
using Piranha.Workflow.Models;
using Piranha.Repositories;

namespace Piranha.Services.Internal;

/// <summary>
/// The workflow service.
/// </summary>
public class WorkflowService : IWorkflowService
{
    private readonly IApi _api;
    private readonly IContentWorkflowRepository _repo;
    private readonly WorkflowManager _workflowManager;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The api</param>
    /// <param name="repo">The repository</param>
    /// <param name="workflowManager">The workflow manager</param>
    public WorkflowService(
        IApi api,
        IContentWorkflowRepository repo,
        WorkflowManager workflowManager)
    {
        _api = api;
        _repo = repo;
        _workflowManager = workflowManager;
    }

    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>The workflows</returns>
    public Task<IEnumerable<WorkflowDefinition>> GetWorkflowsAsync()
    {
        return Task.FromResult(_workflowManager.GetWorkflows());
    }

    /// <summary>
    /// Gets the workflow with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow</returns>
    public Task<WorkflowDefinition> GetWorkflowAsync(string name)
    {
        return Task.FromResult(_workflowManager.GetWorkflow(name));
    }

    /// <summary>
    /// Gets the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow state</returns>
    public Task<ContentWorkflowState> GetContentWorkflowStateAsync(Guid contentId)
    {
        // This should come from the repository in a real implementation
        // For now, return a mock state for testing
        var state = new ContentWorkflowState
        {
            ContentId = contentId,
            WorkflowName = "Standard Editorial Workflow",
            CurrentStateId = "draft",
            CurrentStateName = "Draft",
            StateChangedAt = DateTime.Now,
            StateChangedBy = "admin"
        };

        return Task.FromResult(state);
    }

    /// <summary>
    /// Initializes a new workflow for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="workflowName">The workflow name</param>
    /// <param name="username">The username</param>
    /// <returns>The workflow state</returns>
    public Task<ContentWorkflowState> InitWorkflowAsync(Guid contentId, string workflowName, string username)
    {
        var workflow = _workflowManager.GetWorkflow(workflowName);
        if (workflow == null || workflow.InitialState == null)
        {
            return Task.FromResult<ContentWorkflowState>(null);
        }

        var state = new ContentWorkflowState
        {
            ContentId = contentId,
            WorkflowName = workflowName,
            CurrentStateId = workflow.InitialState.Id,
            CurrentStateName = workflow.InitialState.Name,
            StateChangedAt = DateTime.Now,
            StateChangedBy = username
        };

        state.History.Add(new WorkflowHistoryEntry
        {
            FromStateId = null,
            FromStateName = null,
            ToStateId = workflow.InitialState.Id,
            ToStateName = workflow.InitialState.Name,
            TransitionedAt = DateTime.Now,
            TransitionedBy = username,
            Comment = "Workflow initialized"
        });

        // Save state to repository in a real implementation
        return Task.FromResult(state);
    }

    /// <summary>
    /// Transitions the content to a new state.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="toStateId">The target state id</param>
    /// <param name="username">The username</param>
    /// <param name="comment">Optional comment</param>
    /// <returns>The updated workflow state</returns>
    public async Task<ContentWorkflowState> TransitionAsync(Guid contentId, string toStateId, string username, string comment = null)
    {
        var state = await GetContentWorkflowStateAsync(contentId);
        if (state == null)
        {
            return null;
        }

        var workflow = _workflowManager.GetWorkflow(state.WorkflowName);
        if (workflow == null)
        {
            return null;
        }

        var currentState = workflow.GetState(state.CurrentStateId);
        var targetState = workflow.GetState(toStateId);

        if (currentState == null || targetState == null)
        {
            return null;
        }

        // Check if transition is allowed - in real implementation this would check roles too
        var transition = currentState.Transitions.FirstOrDefault(t => t.ToState == toStateId);
        if (transition == null)
        {
            return null;
        }

        // Update state
        state.CurrentStateId = toStateId;
        state.CurrentStateName = targetState.Name;
        state.StateChangedAt = DateTime.Now;
        state.StateChangedBy = username;

        // Add to history
        state.History.Add(new WorkflowHistoryEntry
        {
            FromStateId = currentState.Id,
            FromStateName = currentState.Name,
            ToStateId = toStateId,
            ToStateName = targetState.Name,
            TransitionedAt = DateTime.Now,
            TransitionedBy = username,
            Comment = comment ?? transition.Comment
        });

        // Check if this is a publish transition
        if (targetState.IsPublished)
        {
            // In a real implementation, this would update the status of the content to Published
            // For now, just note it in the history
            state.History.Add(new WorkflowHistoryEntry
            {
                FromStateId = toStateId,
                FromStateName = targetState.Name,
                ToStateId = toStateId,
                ToStateName = targetState.Name,
                TransitionedAt = DateTime.Now,
                TransitionedBy = username,
                Comment = "Content was published"
            });
        }

        // Save state to repository in a real implementation
        return state;
    }
}
