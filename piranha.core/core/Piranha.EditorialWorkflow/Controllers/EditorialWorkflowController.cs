using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;

namespace Piranha.EditorialWorkflow.Controllers;

[ApiController]
[Route("api/workflow")]
[Authorize]
public class EditorialWorkflowController : ControllerBase
{
    private readonly IEditorialWorkflowService _workflowService;
    private readonly ILogger<EditorialWorkflowController> _logger;

    public EditorialWorkflowController(IEditorialWorkflowService workflowService, ILogger<EditorialWorkflowController> logger)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    #region Request Models
    
    public class CreateWorkflowInstanceWithContentRequest
    {
        public string ContentId { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string ContentType { get; set; }
        public string ContentTitle { get; set; }
    }
    
    #endregion

    #region Workflow Definitions

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions()
    {
        var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
        return Ok(definitions);
    }

    [HttpGet("definitions/with-stats")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitionsWithStats()
    {
        var definitions = await _workflowService.GetAllWorkflowDefinitionsWithStatsAsync();
        return Ok(definitions);
    }

    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> GetWorkflowDefinition(Guid id)
    {
        var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
        if (definition == null)
            return NotFound();

        return Ok(definition);
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<WorkflowDefinition>> CreateWorkflowDefinition(WorkflowDefinition definition)
    {
        var result = await _workflowService.CreateWorkflowDefinitionAsync(definition);
        return CreatedAtAction(nameof(GetWorkflowDefinition), new { id = result.Id }, result);
    }

    [HttpPut("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> UpdateWorkflowDefinition(Guid id, WorkflowDefinition definition)
    {
        if (id != definition.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowDefinitionAsync(definition);
        return Ok(result);
    }

    [HttpDelete("definitions/{id}")]
    public async Task<ActionResult> DeleteWorkflowDefinition(Guid id)
    {
        var canDelete = await _workflowService.CanDeleteWorkflowDefinitionAsync(id);
        if (!canDelete)
            return BadRequest("Cannot delete workflow definition that has active instances");

        await _workflowService.DeleteWorkflowDefinitionAsync(id);
        return NoContent();
    }

    #endregion

    #region Workflow Instances

    [HttpGet("instances")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetWorkflowInstances()
    {
        var instances = await _workflowService.GetWorkflowInstancesByUserAsync();
        return Ok(instances);
    }

    [HttpGet("instances/{id}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstance(Guid id)
    {
        var instance = await _workflowService.GetWorkflowInstanceByIdAsync(id);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    [HttpPost("instances")]
    public async Task<ActionResult<WorkflowInstance>> CreateWorkflowInstance(WorkflowInstance instance)
    {
        var result = await _workflowService.CreateWorkflowInstanceAsync(instance);
        return CreatedAtAction(nameof(GetWorkflowInstance), new { id = result.Id }, result);
    }

    [HttpPut("instances/{id}")]
    public async Task<ActionResult<WorkflowInstance>> UpdateWorkflowInstance(Guid id, WorkflowInstance instance)
    {
        if (id != instance.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowInstanceAsync(instance);
        return Ok(result);
    }

    [HttpPost("instances/{id}/transition")]
    public async Task<ActionResult> TransitionWorkflow(Guid id, [FromBody] string targetState)
    {
        var success = await _workflowService.TransitionWorkflowAsync(id, targetState);
        if (!success)
            return BadRequest("Invalid transition or insufficient permissions");

        return Ok();
    }

    [HttpGet("instances/state/{state}")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetWorkflowInstancesByState(string state)
    {
        var instances = await _workflowService.GetWorkflowInstancesByStateAsync(state);
        return Ok(instances);
    }

    [HttpPost("CreateWorkflowInstanceWithContent")]
    public async Task<IActionResult> CreateWorkflowInstanceWithContent([FromBody] CreateWorkflowInstanceWithContentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ContentId) || request.WorkflowDefinitionId == Guid.Empty)
            {
                return BadRequest("ContentId and WorkflowDefinitionId are required");
            }

            var workflowInstance = await _workflowService.CreateWorkflowInstanceWithContentAsync(
                request.ContentId, 
                request.WorkflowDefinitionId,
                request.ContentType, 
                request.ContentTitle);

            return Ok(new { 
                workflowInstanceId = workflowInstance.Id,
                contentId = workflowInstance.ContentId,
                workflowDefinitionId = workflowInstance.WorkflowDefinitionId,
                currentStateId = workflowInstance.CurrentStateId,
                status = workflowInstance.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while creating the workflow instance");
        }
    }

    #endregion

    #region Workflow Content Extensions

    [HttpGet("content-extensions/{contentId}/exists")]
    public async Task<ActionResult<bool>> CheckWorkflowContentExtensionExists(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            return BadRequest("ContentId is required");

        var exists = await _workflowService.WorkflowContentExtensionExistsAsync(contentId);
        return Ok(exists);
    }

    [HttpGet("content-extensions/{contentId}")]
    public async Task<ActionResult<WorkflowContentExtension>> GetWorkflowContentExtension(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            return BadRequest("ContentId is required");

        var extension = await _workflowService.GetWorkflowContentExtensionAsync(contentId);
        if (extension == null)
            return NotFound();
        
        return Ok(extension);
    }

    /// <summary>
    /// Given a WorkflowDefinitionId, retrieve all the WorkflowContentExtensions
    /// </summary>
    [HttpGet("definitions/{workflowDefinitionId}/content-extensions")]
    public async Task<ActionResult<IEnumerable<WorkflowContentExtension>>> GetWorkflowContentExtensionsByDefinition(Guid workflowDefinitionId)
    {
        if (workflowDefinitionId == Guid.Empty)
            return BadRequest("WorkflowDefinitionId is required");

        var extensions = await _workflowService.GetWorkflowContentExtensionsByDefinitionAsync(workflowDefinitionId);
        return Ok(extensions);
    }

    /// <summary>
    /// Given a WorkflowInstanceId, retrieve the WorkflowInstance
    /// </summary>
    [HttpGet("workflow-instances/{workflowInstanceId}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        if (workflowInstanceId == Guid.Empty)
            return BadRequest("WorkflowInstanceId is required");

        var instance = await _workflowService.GetWorkflowInstanceByIdAsync(workflowInstanceId);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    /// <summary>
    /// Given a WorkflowInstanceId, update the WorkflowInstance (supports partial updates)
    /// </summary>
    [HttpPut("workflow-instances/{workflowInstanceId}")]
    public async Task<ActionResult<WorkflowInstance>> UpdateWorkflowInstanceById(Guid workflowInstanceId, [FromBody] WorkflowInstance instance)
    {
        try
        {
            if (workflowInstanceId == Guid.Empty)
                return BadRequest("WorkflowInstanceId is required");

            // Allow null or empty body for partial updates
            var result = await _workflowService.PartialUpdateWorkflowInstanceAsync(workflowInstanceId, instance);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating the workflow instance");
        }
    }

    #endregion

    #region Workflow States

    [HttpGet("definitions/{definitionId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowState>>> GetWorkflowStates(Guid definitionId)
    {
        var states = await _workflowService.GetWorkflowStatesByDefinitionAsync(definitionId);
        return Ok(states);
    }

    [HttpGet("states/{id}")]
    public async Task<ActionResult<WorkflowState>> GetWorkflowState(Guid id)
    {
        var state = await _workflowService.GetWorkflowStateByIdAsync(id);
        if (state == null)
            return NotFound();

        return Ok(state);
    }

    [HttpPost("states")]
    public async Task<ActionResult<WorkflowState>> CreateWorkflowState(WorkflowState state)
    {
        var result = await _workflowService.CreateWorkflowStateAsync(state);
        return CreatedAtAction(nameof(GetWorkflowState), new { id = result.Id }, result);
    }

    [HttpPut("states/{id}")]
    public async Task<ActionResult<WorkflowState>> UpdateWorkflowState(Guid id, WorkflowState state)
    {
        if (id != state.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowStateAsync(state);
        return Ok(result);
    }

    [HttpDelete("states/{id}")]
    public async Task<ActionResult> DeleteWorkflowState(Guid id)
    {
        await _workflowService.DeleteWorkflowStateAsync(id);
        return NoContent();
    }

    #endregion

    #region Transition Rules

    [HttpGet("definitions/{definitionId}/rules")]
    public async Task<ActionResult<IEnumerable<TransitionRule>>> GetTransitionRules(Guid definitionId)
    {
        var rules = await _workflowService.GetTransitionRulesByDefinitionAsync(definitionId);
        return Ok(rules);
    }

    [HttpGet("rules/{id}")]
    public async Task<ActionResult<TransitionRule>> GetTransitionRule(Guid id)
    {
        var rule = await _workflowService.GetTransitionRuleByIdAsync(id);
        if (rule == null)
            return NotFound();

        return Ok(rule);
    }

    [HttpPost("rules")]
    public async Task<ActionResult<TransitionRule>> CreateTransitionRule(TransitionRule rule)
    {
        var result = await _workflowService.CreateTransitionRuleAsync(rule);
        return CreatedAtAction(nameof(GetTransitionRule), new { id = result.Id }, result);
    }

    [HttpPut("rules/{id}")]
    public async Task<ActionResult<TransitionRule>> UpdateTransitionRule(Guid id, TransitionRule rule)
    {
        if (id != rule.Id)
            return BadRequest();

        var result = await _workflowService.UpdateTransitionRuleAsync(rule);
        return Ok(result);
    }

    [HttpDelete("rules/{id}")]
    public async Task<ActionResult> DeleteTransitionRule(Guid id)
    {
        await _workflowService.DeleteTransitionRuleAsync(id);
        return NoContent();
    }

    #endregion

    #region Debug Endpoints

    [HttpGet("debug/database")]
    [AllowAnonymous]
    public async Task<ActionResult> DebugDatabase()
    {
        var canConnect = await _workflowService.TestDatabaseConnectionAsync();
        return Ok(new { 
            DatabaseConnected = canConnect,
            Message = canConnect ? "Database connection successful" : "Database connection failed"
        });
    }

    [HttpGet("debug/roles")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSystemRoles()
    {
        var roles = await _workflowService.GetSystemRolesAsync();
        return Ok(roles.Select(r => new { 
            Id = r.Id,
            Name = r.Name,
            NormalizedName = r.NormalizedName
        }));
    }

    #endregion
}
