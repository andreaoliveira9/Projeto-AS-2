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
/// Api controller for workflow content operations.
/// </summary>
[Area("Manager")]
[Route("manager/api/workflow/content")]
[Authorize(Policy = WorkflowPermission.ViewWorkflowState)]
[ApiController]
public class WorkflowContentApiController : Controller
{
    private readonly IApi _api;
    private readonly IWorkflowService _workflowService;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="api">The current api</param>
    /// <param name="workflowService">The workflow service</param>
    public WorkflowContentApiController(
        IApi api,
        IWorkflowService workflowService)
    {
        _api = api;
        _workflowService = workflowService;
    }

    /// <summary>
    /// Gets all content items with workflow state.
    /// </summary>
    /// <param name="stateId">Optional state filter</param>
    /// <returns>The content items</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContentWorkflowModel>>> GetAll([FromQuery] string stateId = null)
    {
        // In a real implementation, these would come from querying the database
        // For now, return some mock data
        var contentItems = new List<ContentWorkflowModel>
        {
            new ContentWorkflowModel
            {
                ContentId = Guid.Parse("3c8b9d0d-8e7f-4c5c-9e3d-86d3e3a95c9b"),
                ContentTitle = "Sample Page",
                ContentType = "Page",
                WorkflowState = new ContentWorkflowState
                {
                    ContentId = Guid.Parse("3c8b9d0d-8e7f-4c5c-9e3d-86d3e3a95c9b"),
                    WorkflowName = "Standard Editorial Workflow",
                    CurrentStateId = "draft",
                    CurrentStateName = "Draft",
                    StateChangedAt = DateTime.Now.AddDays(-2),
                    StateChangedBy = "admin"
                }
            },
            new ContentWorkflowModel
            {
                ContentId = Guid.Parse("4d9b8c7e-6a5f-4b3c-9d2e-8a1b7c6d5e4f"),
                ContentTitle = "Another Page",
                ContentType = "Page",
                WorkflowState = new ContentWorkflowState
                {
                    ContentId = Guid.Parse("4d9b8c7e-6a5f-4b3c-9d2e-8a1b7c6d5e4f"),
                    WorkflowName = "Standard Editorial Workflow",
                    CurrentStateId = "review",
                    CurrentStateName = "Review",
                    StateChangedAt = DateTime.Now.AddDays(-1),
                    StateChangedBy = "editor"
                }
            },
            new ContentWorkflowModel
            {
                ContentId = Guid.Parse("5e4d3c2b-1a9b-8c7d-6e5f-4a3b2c1d0e9f"),
                ContentTitle = "Example Post",
                ContentType = "Post",
                WorkflowState = new ContentWorkflowState
                {
                    ContentId = Guid.Parse("5e4d3c2b-1a9b-8c7d-6e5f-4a3b2c1d0e9f"),
                    WorkflowName = "Standard Editorial Workflow",
                    CurrentStateId = "published",
                    CurrentStateName = "Published",
                    StateChangedAt = DateTime.Now,
                    StateChangedBy = "admin"
                }
            }
        };

        // Filter by state if requested
        if (!string.IsNullOrEmpty(stateId))
        {
            contentItems = contentItems
                .Where(c => c.WorkflowState.CurrentStateId == stateId)
                .ToList();
        }

        return Ok(contentItems);
    }
}

/// <summary>
/// Model for content with workflow state.
/// </summary>
public class ContentWorkflowModel
{
    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content title.
    /// </summary>
    public string ContentTitle { get; set; }

    /// <summary>
    /// Gets/sets the content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets/sets the workflow state.
    /// </summary>
    public ContentWorkflowState WorkflowState { get; set; }
}