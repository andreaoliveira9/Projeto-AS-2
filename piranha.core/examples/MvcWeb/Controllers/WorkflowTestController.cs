using Microsoft.AspNetCore.Mvc;
using Piranha.EditorialWorkflow.Services;
using Piranha.EditorialWorkflow.Models;
using System.Text.Json;

namespace MvcWeb.Controllers;

[ApiController]
[Route("api/test")]
public class WorkflowTestController : ControllerBase
{
    private readonly IEditorialWorkflowService _workflowService;

    public WorkflowTestController(IEditorialWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            message = "EditorialWorkflow service is registered and working in MvcWeb!"
        });
    }

    [HttpPost("create-sample-workflow")]
    public async Task<IActionResult> CreateSampleWorkflow()
    {
        try
        {
            // Create workflow definition
            var workflowId = Guid.NewGuid();
            var workflow = new WorkflowDefinition
            {
                Id = workflowId,
                Name = "MvcWeb Test Workflow",
                Description = "A test workflow created via MvcWeb API",
                IsActive = true,
                CreatedBy = "mvcweb-test",
                LastModifiedBy = "mvcweb-test",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await _workflowService.CreateWorkflowDefinitionAsync(workflow);

            // Create states
            var draftState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                StateId = "draft",
                Name = "Draft",
                Description = "Content in draft state",
                IsInitial = true,
                SortOrder = 1,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            };

            var reviewState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                StateId = "review",
                Name = "Under Review",
                Description = "Content under review",
                SortOrder = 2,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            };

            var publishedState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                StateId = "published",
                Name = "Published",
                Description = "Content is live",
                IsPublished = true,
                IsFinal = true,
                SortOrder = 3,
                WorkflowDefinitionId = workflowId,
                Created = DateTime.UtcNow
            };

            await _workflowService.CreateWorkflowStateAsync(draftState);
            await _workflowService.CreateWorkflowStateAsync(reviewState);
            await _workflowService.CreateWorkflowStateAsync(publishedState);

            // Create transitions
            var draftToReview = new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = draftState.Id,
                ToStateId = reviewState.Id,
                AllowedRoles = JsonSerializer.Serialize(new[] { "Admin", "Editor" }),
                Description = "Submit for review",
                CommentTemplate = "Ready for review",
                RequiresComment = false,
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            };

            var reviewToPublished = new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = reviewState.Id,
                ToStateId = publishedState.Id,
                AllowedRoles = JsonSerializer.Serialize(new[] { "Admin" }),
                Description = "Publish content",
                CommentTemplate = "Approved for publication",
                RequiresComment = true,
                IsActive = true,
                SortOrder = 1,
                Created = DateTime.UtcNow
            };

            var reviewToDraft = new TransitionRule
            {
                Id = Guid.NewGuid(),
                FromStateId = reviewState.Id,
                ToStateId = draftState.Id,
                AllowedRoles = JsonSerializer.Serialize(new[] { "Admin", "Editor" }),
                Description = "Send back to draft",
                CommentTemplate = "Needs revision",
                RequiresComment = true,
                IsActive = true,
                SortOrder = 2,
                Created = DateTime.UtcNow
            };

            await _workflowService.CreateTransitionRuleAsync(draftToReview);
            await _workflowService.CreateTransitionRuleAsync(reviewToPublished);
            await _workflowService.CreateTransitionRuleAsync(reviewToDraft);

            return Ok(new
            {
                success = true,
                workflowId = workflowId,
                message = "MvcWeb sample workflow created successfully!",
                workflow = new
                {
                    id = workflow.Id,
                    name = workflow.Name,
                    description = workflow.Description,
                    states = new[] 
                    { 
                        new { id = "draft", name = "Draft", isInitial = true, isPublished = false },
                        new { id = "review", name = "Under Review", isInitial = false, isPublished = false },
                        new { id = "published", name = "Published", isInitial = false, isPublished = true }
                    },
                    transitions = new[] 
                    { 
                        "draft → review", 
                        "review → published", 
                        "review → draft" 
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("workflows")]
    public async Task<IActionResult> GetWorkflows()
    {
        try
        {
            var workflows = await _workflowService.GetAllWorkflowDefinitionsAsync();
            return Ok(new
            {
                success = true,
                count = workflows.Count(),
                workflows = workflows.Select(w => new
                {
                    id = w.Id,
                    name = w.Name,
                    description = w.Description,
                    isActive = w.IsActive,
                    version = w.Version,
                    created = w.Created,
                    createdBy = w.CreatedBy
                })
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("create-test-instance")]
    public async Task<IActionResult> CreateTestInstance()
    {
        try
        {
            // Get the first workflow
            var workflows = await _workflowService.GetAllWorkflowDefinitionsAsync();
            var workflow = workflows.FirstOrDefault();
            
            if (workflow == null)
            {
                return BadRequest(new { success = false, error = "No workflows found. Create a workflow first." });
            }

            // Create a test instance
            var instance = new WorkflowInstance
            {
                Id = Guid.NewGuid(),
                ContentId = Guid.NewGuid().ToString(),
                ContentType = "Page",
                ContentTitle = "Test Page for MvcWeb",
                WorkflowDefinitionId = workflow.Id,
                Status = WorkflowInstanceStatus.Active,
                CreatedBy = "mvcweb-test",
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await _workflowService.CreateWorkflowInstanceAsync(instance);

            return Ok(new
            {
                success = true,
                instanceId = instance.Id,
                contentTitle = instance.ContentTitle,
                workflowName = workflow.Name,
                message = "Test workflow instance created!"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}