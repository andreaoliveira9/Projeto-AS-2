/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.AspNetCore.Authorization;
using Piranha.Models;
using Piranha.Security;
using Piranha.Services;
using Piranha.Workflow.Models;

namespace Piranha.Manager.Services;

public class WorkflowService
{
    private readonly IApi _api;
    private readonly IWorkflowService _workflowService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The api</param>
    /// <param name="workflowService">The workflow service</param>
    /// <param name="authorizationService">The authorization service</param>
    public WorkflowService(
        IApi api,
        IWorkflowService workflowService,
        IAuthorizationService authorizationService)
    {
        _api = api;
        _workflowService = workflowService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets all available workflows.
    /// </summary>
    /// <returns>The workflows</returns>
    public async Task<IEnumerable<WorkflowDefinition>> GetWorkflowsAsync()
    {
        return await _workflowService.GetWorkflowsAsync();
    }

    /// <summary>
    /// Gets the workflow with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow</returns>
    public async Task<WorkflowDefinition> GetWorkflowAsync(string name)
    {
        return await _workflowService.GetWorkflowAsync(name);
    }

    /// <summary>
    /// Gets the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow state</returns>
    public async Task<ContentWorkflowState> GetContentWorkflowStateAsync(Guid contentId)
    {
        return await _workflowService.GetContentWorkflowStateAsync(contentId);
    }
}
