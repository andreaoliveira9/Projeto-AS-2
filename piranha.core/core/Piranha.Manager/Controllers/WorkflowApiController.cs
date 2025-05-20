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
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager.Models;
using Piranha.Manager.Services;
using Piranha.Models;
using Piranha.Security;
using Piranha.Services;
using Piranha.Workflow.Models;

namespace Piranha.Manager.Controllers;

/// <summary>
/// Api controller for workflow operations.
/// </summary>
[Area("Manager")]
[Route("manager/api/workflow")]
[Authorize(Policy = Permission.Admin)]
[ApiController]
public class WorkflowApiController : Controller
{
    private readonly IApi _api;
    private readonly IWorkflowService _workflowService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ManagerLocalizer _localizer;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The current api</param>
    /// <param name="workflowService">The workflow service</param>
    /// <param name="authorizationService">The authorization service</param>
    /// <param name="localizer">The localizer</param>
    public WorkflowApiController(
        IApi api,
        IWorkflowService workflowService,
        IAuthorizationService authorizationService,
        ManagerLocalizer localizer)
    {
        _api = api;
        _workflowService = workflowService;
        _authorizationService = authorizationService;
        _localizer = localizer;
    }

    /// <summary>
    /// Gets all available workflow definitions.
    /// </summary>
    /// <returns>The workflows</returns>
    [HttpGet("definitions")]
    [Authorize(Policy = WorkflowPermission.ViewWorkflowState)]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflows()
    {
        return Ok(await _workflowService.GetWorkflowsAsync());
    }

    /// <summary>
    /// Gets the workflow definition with the specified name.
    /// </summary>
    /// <param name="name">The workflow name</param>
    /// <returns>The workflow</returns>
    [HttpGet("definition/{name}")]
    [Authorize(Policy = WorkflowPermission.ViewWorkflowState)]
    public async Task<ActionResult<WorkflowDefinition>> GetWorkflow(string name)
    {
        var workflow = await _workflowService.GetWorkflowAsync(name);

        if (workflow == null)
        {
            return NotFound();
        }

        return Ok(workflow);
    }

    /// <summary>
    /// Gets the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow state</returns>
    [HttpGet("state/{contentId:Guid}")]
    [Authorize(Policy = WorkflowPermission.ViewWorkflowState)]
    public async Task<ActionResult<ContentWorkflowState>> GetContentState(Guid contentId)
    {
        var state = await _workflowService.GetContentWorkflowStateAsync(contentId);

        if (state == null)
        {
            return NotFound();
        }

        return Ok(state);
    }

    /// <summary>
    /// Initializes a new workflow for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="workflowName">The workflow name</param>
    /// <returns>The workflow state</returns>
    [HttpPost("init/{contentId:Guid}/{workflowName}")]
    [Authorize(Policy = WorkflowPermission.ManageWorkflows)]
    public async Task<ActionResult<ContentWorkflowState>> InitWorkflow(Guid contentId, string workflowName)
    {
        var username = User.Identity.Name;
        var state = await _workflowService.InitWorkflowAsync(contentId, workflowName, username);

        if (state == null)
        {
            return BadRequest();
        }

        return Ok(state);
    }

    /// <summary>
    /// Transitions the content to a new state.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="toStateId">The target state id</param>
    /// <param name="model">The transition model</param>
    /// <returns>The updated workflow state</returns>
    [HttpPost("transition/{contentId:Guid}/{toStateId}")]
    public async Task<ActionResult<ContentWorkflowState>> Transition(Guid contentId, string toStateId, [FromBody] WorkflowTransitionModel model)
    {
        // Get the current state to check role-based permissions
        var currentState = await _workflowService.GetContentWorkflowStateAsync(contentId);
        if (currentState == null)
        {
            return NotFound();
        }

        // Get the workflow to check transition rules
        var workflow = await _workflowService.GetWorkflowAsync(currentState.WorkflowName);
        if (workflow == null)
        {
            return BadRequest("Workflow not found");
        }

        // Get the current state in the workflow definition
        var state = workflow.GetState(currentState.CurrentStateId);
        if (state == null)
        {
            return BadRequest("Current state not found in workflow definition");
        }

        // Get the transition to check permissions
        var transition = state.Transitions.FirstOrDefault(t => t.ToState == toStateId);
        if (transition == null)
        {
            return BadRequest("Invalid transition");
        }

        // Extract user roles
        var userRoles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Check if any of the user roles matches the allowed roles for this transition
        var hasRolePermission = transition.Roles.Any(r => userRoles.Contains(r));
        
        // Also check for specific transition permission
        var transitionPermission = WorkflowPermission.GetTransitionPermission(toStateId);
        var hasTransitionPermission = false;
        
        if (transitionPermission != null)
        {
            var permissionResult = await _authorizationService.AuthorizeAsync(User, transitionPermission);
            hasTransitionPermission = permissionResult.Succeeded;
        }

        // Check directional permissions
        bool hasDirectionalPermission = false;
        
        // Determine the direction of the transition
        var targetState = workflow.GetState(toStateId);
        if (targetState != null)
        {
            // Check if this is a forward transition (advancing the workflow)
            if (targetState.IsPublished || toStateId == "approved")
            {
                var advanceResult = await _authorizationService.AuthorizeAsync(User, WorkflowPermission.AdvanceWorkflow);
                hasDirectionalPermission = advanceResult.Succeeded;
            }
            // Check if this is a backward transition (reverting the workflow)
            else if (toStateId == "draft" || currentState.CurrentStateId == "published" || currentState.CurrentStateId == "approved")
            {
                var revertResult = await _authorizationService.AuthorizeAsync(User, WorkflowPermission.RevertWorkflow);
                hasDirectionalPermission = revertResult.Succeeded;
            }
            // Check if this is skipping steps
            else if ((currentState.CurrentStateId == "draft" && toStateId == "approved") ||
                     (currentState.CurrentStateId == "draft" && toStateId == "published") ||
                     (currentState.CurrentStateId == "review" && toStateId == "published"))
            {
                var skipResult = await _authorizationService.AuthorizeAsync(User, WorkflowPermission.SkipWorkflowSteps);
                hasDirectionalPermission = skipResult.Succeeded;
            }
        }

        // If user doesn't have the required permissions, return forbidden
        if (!hasRolePermission && !hasTransitionPermission && !hasDirectionalPermission)
        {
            return Forbid();
        }

        // Perform the transition
        var username = User.Identity.Name;
        var updatedState = await _workflowService.TransitionAsync(contentId, toStateId, username, model?.Comment);

        if (updatedState == null)
        {
            return BadRequest("Failed to transition content");
        }

        return Ok(updatedState);
    }

    /// <summary>
    /// Gets all possible transitions for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The available transitions</returns>
    [HttpGet("transitions/{contentId:Guid}")]
    [Authorize(Policy = WorkflowPermission.ViewWorkflowState)]
    public async Task<ActionResult<IEnumerable<WorkflowTransitionModel>>> GetAvailableTransitions(Guid contentId)
    {
        // Get the current state
        var currentState = await _workflowService.GetContentWorkflowStateAsync(contentId);
        if (currentState == null)
        {
            return NotFound();
        }

        // Get the workflow
        var workflow = await _workflowService.GetWorkflowAsync(currentState.WorkflowName);
        if (workflow == null)
        {
            return BadRequest("Workflow not found");
        }

        // Get the current state in the workflow definition
        var state = workflow.GetState(currentState.CurrentStateId);
        if (state == null)
        {
            return BadRequest("Current state not found in workflow definition");
        }

        // Extract user roles
        var userRoles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Get all possible transitions for this user
        var availableTransitions = new List<WorkflowTransitionModel>();

        foreach (var transition in state.Transitions)
        {
            // Check role-based permission
            var hasRolePermission = transition.Roles.Any(r => userRoles.Contains(r));
            
            // Check specific transition permission
            var transitionPermission = WorkflowPermission.GetTransitionPermission(transition.ToState);
            var hasTransitionPermission = false;
            
            if (transitionPermission != null)
            {
                var permissionResult = await _authorizationService.AuthorizeAsync(User, transitionPermission);
                hasTransitionPermission = permissionResult.Succeeded;
            }

            // If user has permission for this transition, add it to the available transitions
            if (hasRolePermission || hasTransitionPermission)
            {
                var targetState = workflow.GetState(transition.ToState);
                
                availableTransitions.Add(new WorkflowTransitionModel
                {
                    ToStateId = transition.ToState,
                    ToStateName = targetState?.Name,
                    Comment = transition.Comment
                });
            }
        }

        return Ok(availableTransitions);
    }
}

/// <summary>
/// Model for a workflow transition.
/// </summary>
public class WorkflowTransitionModel
{
    /// <summary>
    /// Gets/sets the target state id.
    /// </summary>
    public string ToStateId { get; set; }

    /// <summary>
    /// Gets/sets the target state name.
    /// </summary>
    public string ToStateName { get; set; }

    /// <summary>
    /// Gets/sets the comment for the transition.
    /// </summary>
    public string Comment { get; set; }
}
