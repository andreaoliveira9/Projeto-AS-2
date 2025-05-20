/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Workflow.Models;

namespace Piranha.Services;

/// <summary>
/// Interface for the workflow service.
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>The workflows</returns>
    Task<IEnumerable<WorkflowDefinition>> GetWorkflowsAsync();

    /// <summary>
    /// Gets the workflow with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow</returns>
    Task<WorkflowDefinition> GetWorkflowAsync(string name);

    /// <summary>
    /// Gets the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow state</returns>
    Task<ContentWorkflowState> GetContentWorkflowStateAsync(Guid contentId);

    /// <summary>
    /// Initializes a new workflow for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="workflowName">The workflow name</param>
    /// <param name="username">The username</param>
    /// <returns>The workflow state</returns>
    Task<ContentWorkflowState> InitWorkflowAsync(Guid contentId, string workflowName, string username);

    /// <summary>
    /// Transitions the content to a new state.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="toStateId">The target state id</param>
    /// <param name="username">The username</param>
    /// <param name="comment">Optional comment</param>
    /// <returns>The updated workflow state</returns>
    Task<ContentWorkflowState> TransitionAsync(Guid contentId, string toStateId, string username, string comment = null);
}
