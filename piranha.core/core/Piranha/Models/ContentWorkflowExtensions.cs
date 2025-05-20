/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Services;

namespace Piranha.Models;

/// <summary>
/// Extension methods for content with workflow support.
/// </summary>
public static class ContentWorkflowExtensions
{
    /// <summary>
    /// Gets the current workflow state for the content.
    /// </summary>
    /// <param name="content">The content</param>
    /// <param name="workflowService">The workflow service</param>
    /// <returns>The workflow state</returns>
    public static async Task<ContentWorkflowState> GetWorkflowStateAsync(this ContentBase content, IWorkflowService workflowService)
    {
        if (content == null || workflowService == null)
        {
            return null;
        }
        
        return await workflowService.GetContentWorkflowStateAsync(content.Id);
    }

    /// <summary>
    /// Initializes a new workflow for the content.
    /// </summary>
    /// <param name="content">The content</param>
    /// <param name="workflowService">The workflow service</param>
    /// <param name="workflowName">The workflow name</param>
    /// <param name="username">The current user</param>
    /// <returns>The workflow state</returns>
    public static async Task<ContentWorkflowState> InitWorkflowAsync(this ContentBase content, IWorkflowService workflowService, string workflowName, string username)
    {
        if (content == null || workflowService == null)
        {
            return null;
        }
        
        return await workflowService.InitWorkflowAsync(content.Id, workflowName, username);
    }

    /// <summary>
    /// Transitions the content to a new workflow state.
    /// </summary>
    /// <param name="content">The content</param>
    /// <param name="workflowService">The workflow service</param>
    /// <param name="toStateId">The new state id</param>
    /// <param name="username">The current user</param>
    /// <param name="comment">Optional comment</param>
    /// <returns>The workflow state</returns>
    public static async Task<ContentWorkflowState> TransitionAsync(this ContentBase content, IWorkflowService workflowService, string toStateId, string username, string comment = null)
    {
        if (content == null || workflowService == null)
        {
            return null;
        }
        
        return await workflowService.TransitionAsync(content.Id, toStateId, username, comment);
    }

    /// <summary>
    /// Checks if the current user can transition the content to the specified state.
    /// </summary>
    /// <param name="content">The content</param>
    /// <param name="workflowService">The workflow service</param>
    /// <param name="toStateId">The new state id</param>
    /// <param name="roles">The current user roles</param>
    /// <returns>If the transition is allowed</returns>
    public static async Task<bool> CanTransitionToAsync(this ContentBase content, IWorkflowService workflowService, string toStateId, IEnumerable<string> roles)
    {
        if (content == null || workflowService == null)
        {
            return false;
        }
        
        return await workflowService.CanTransitionAsync(content.Id, toStateId, roles);
    }
}
