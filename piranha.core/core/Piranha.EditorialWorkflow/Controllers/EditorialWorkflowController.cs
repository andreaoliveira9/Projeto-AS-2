using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;

namespace Piranha.EditorialWorkflow.Controllers;

[ApiController]
[Route("api/workflow")]
//[Authorize]
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
        _logger.LogInformation("GetWorkflowDefinitions: Starting to retrieve all workflow definitions");
        try
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
            _logger.LogInformation("GetWorkflowDefinitions: Retrieved {Count} workflow definitions", definitions?.Count() ?? 0);
            
            if (definitions?.Any() == true)
            {
                foreach (var def in definitions)
                {
                    _logger.LogDebug("GetWorkflowDefinitions: Found workflow - ID: {Id}, Name: {Name}, IsActive: {IsActive}", 
                        def.Id, def.Name, def.IsActive);
                }
            }
            
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowDefinitions: Error retrieving workflow definitions");
            return StatusCode(500, "An error occurred while retrieving workflow definitions");
        }
    }

    [HttpGet("definitions/with-stats")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitionsWithStats()
    {
        _logger.LogInformation("GetWorkflowDefinitionsWithStats: Starting to retrieve all workflow definitions with statistics");
        try
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsWithStatsAsync();
            _logger.LogInformation("GetWorkflowDefinitionsWithStats: Retrieved {Count} workflow definitions with stats", definitions?.Count() ?? 0);
            
            if (definitions?.Any() == true)
            {
                foreach (var def in definitions)
                {
                    _logger.LogDebug("GetWorkflowDefinitionsWithStats: Found workflow - ID: {Id}, Name: {Name}, States: {StateCount}, Instances: {InstanceCount}", 
                        def.Id, def.Name, def.States?.Count ?? 0, def.Instances?.Count ?? 0);
                }
            }
            
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowDefinitionsWithStats: Error retrieving workflow definitions with statistics");
            return StatusCode(500, "An error occurred while retrieving workflow definitions with statistics");
        }
    }

    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> GetWorkflowDefinition(Guid id)
    {
        _logger.LogInformation("GetWorkflowDefinition: Retrieving workflow definition with ID: {Id}", id);
        try
        {
            var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
            if (definition == null)
            {
                _logger.LogWarning("GetWorkflowDefinition: Workflow definition not found with ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("GetWorkflowDefinition: Found workflow definition - Name: {Name}, IsActive: {IsActive}", 
                definition.Name, definition.IsActive);
            return Ok(definition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowDefinition: Error retrieving workflow definition with ID: {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the workflow definition");
        }
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<WorkflowDefinition>> CreateWorkflowDefinition(WorkflowDefinition definition)
    {
        _logger.LogInformation("CreateWorkflowDefinition: Starting creation of new workflow definition");
        _logger.LogInformation("CreateWorkflowDefinition: Received definition - Name: {Name}, Description: {Description}, IsActive: {IsActive}, ID: {Id}", 
            definition?.Name, definition?.Description, definition?.IsActive, definition?.Id);

        try
        {
            if (definition == null)
            {
                _logger.LogWarning("CreateWorkflowDefinition: Received null workflow definition");
                return BadRequest(new { message = "Workflow definition cannot be null" });
            }

            if (string.IsNullOrWhiteSpace(definition.Name))
            {
                _logger.LogWarning("CreateWorkflowDefinition: Workflow name is null or empty");
                return BadRequest(new { message = "Workflow name is required" });
            }

            _logger.LogDebug("CreateWorkflowDefinition: Before calling service - Definition ID: {Id}", definition.Id);
            
            var result = await _workflowService.CreateWorkflowDefinitionAsync(definition);
            
            _logger.LogInformation("CreateWorkflowDefinition: Successfully created workflow definition. ID: {Id}, Name: {Name}", 
                result.Id, result.Name);
                
            return CreatedAtAction(nameof(GetWorkflowDefinition), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowDefinition: Error creating workflow definition. Name: {Name}, Exception: {Exception}", 
                definition?.Name, ex.ToString());
            return StatusCode(500, new { message = "An error occurred while creating the workflow definition", error = ex.Message });
        }
    }

    [HttpPut("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> UpdateWorkflowDefinition(Guid id, WorkflowDefinition definition)
    {
        _logger.LogInformation("UpdateWorkflowDefinition: Starting update of workflow definition. ID: {Id}, Name: {Name}", 
            id, definition?.Name);

        try
        {
            if (id != definition.Id)
            {
                _logger.LogWarning("UpdateWorkflowDefinition: URL ID {UrlId} does not match definition ID {DefinitionId}", 
                    id, definition.Id);
                return BadRequest("ID mismatch");
            }

            var result = await _workflowService.UpdateWorkflowDefinitionAsync(definition);
            
            _logger.LogInformation("UpdateWorkflowDefinition: Successfully updated workflow definition. ID: {Id}, Name: {Name}", 
                result.Id, result.Name);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateWorkflowDefinition: Error updating workflow definition. ID: {Id}", id);
            return StatusCode(500, "An error occurred while updating the workflow definition");
        }
    }

    [HttpDelete("definitions/{id}")]
    public async Task<ActionResult> DeleteWorkflowDefinition(Guid id)
    {
        _logger.LogInformation("DeleteWorkflowDefinition: Starting deletion of workflow definition with ID: {Id}", id);

        try
        {
            var existingDefinition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
            if (existingDefinition == null)
            {
                _logger.LogWarning("DeleteWorkflowDefinition: Workflow definition not found with ID: {Id}", id);
                return NotFound(new { message = "Workflow definition not found" });
            }

            _logger.LogInformation("DeleteWorkflowDefinition: Found definition to delete - Name: {Name}", 
                existingDefinition.Name);

            var canDelete = await _workflowService.CanDeleteWorkflowDefinitionAsync(id);
            if (!canDelete)
            {
                _logger.LogWarning("DeleteWorkflowDefinition: Cannot delete workflow definition {Id} - has active instances", id);
                return BadRequest(new { message = "Cannot delete workflow definition that has active instances" });
            }

            await _workflowService.DeleteWorkflowDefinitionAsync(id);
            
            _logger.LogInformation("DeleteWorkflowDefinition: Successfully deleted workflow definition with ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowDefinition: Error deleting workflow definition with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the workflow definition", error = ex.Message });
        }
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
        _logger.LogInformation("CreateWorkflowInstanceWithContent: Starting creation for ContentId: {ContentId}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
            request?.ContentId, request?.WorkflowDefinitionId);

        try
        {
            // Validate the request
            if (request == null)
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContent: Received null request");
                return BadRequest(new { message = "Request cannot be null" });
            }

            if (string.IsNullOrWhiteSpace(request.ContentId))
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContent: ContentId is null or empty");
                return BadRequest(new { message = "ContentId is required" });
            }

            if (request.WorkflowDefinitionId == Guid.Empty)
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContent: WorkflowDefinitionId is empty");
                return BadRequest(new { message = "WorkflowDefinitionId is required" });
            }

            // Use the service method to create both the workflow instance and content extension
            var workflowInstance = await _workflowService.CreateWorkflowInstanceWithContentAsync(
                request.ContentId, 
                request.WorkflowDefinitionId,
                request.ContentType, 
                request.ContentTitle);

            _logger.LogInformation("CreateWorkflowInstanceWithContent: Successfully created WorkflowInstance {WorkflowInstanceId} for ContentId {ContentId}",
                workflowInstance.Id, request.ContentId);

            return Ok(new { 
                workflowInstanceId = workflowInstance.Id,
                contentId = workflowInstance.ContentId,
                workflowDefinitionId = workflowInstance.WorkflowDefinitionId,
                currentStateId = workflowInstance.CurrentStateId,
                status = workflowInstance.Status.ToString()
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "CreateWorkflowInstanceWithContent: Invalid argument - {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CreateWorkflowInstanceWithContent: Invalid operation - {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowInstanceWithContent: Error creating WorkflowInstance with ContentId {ContentId} and WorkflowDefinitionId {WorkflowDefinitionId}.", 
                request?.ContentId, request?.WorkflowDefinitionId);
            return StatusCode(500, new { message = "An error occurred while creating the workflow instance" });
        }
    }

    #endregion

    #region Workflow Content Extensions

    [HttpGet("content-extensions/{contentId}/exists")]
    public async Task<ActionResult<bool>> CheckWorkflowContentExtensionExists(string contentId)
    {
        _logger.LogInformation("CheckWorkflowContentExtensionExists: Checking existence for ContentId: {ContentId}", contentId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                _logger.LogWarning("CheckWorkflowContentExtensionExists: ContentId is null or empty");
                return BadRequest(new { message = "ContentId is required" });
            }

            var exists = await _workflowService.WorkflowContentExtensionExistsAsync(contentId);
            
            _logger.LogInformation("CheckWorkflowContentExtensionExists: ContentId {ContentId} exists: {Exists}", contentId, exists);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckWorkflowContentExtensionExists: Error checking existence for ContentId: {ContentId}", contentId);
            return StatusCode(500, new { message = "An error occurred while checking workflow content extension existence" });
        }
    }

    [HttpGet("content-extensions/{contentId}")]
    public async Task<ActionResult<WorkflowContentExtension>> GetWorkflowContentExtension(string contentId)
    {
        _logger.LogInformation("GetWorkflowContentExtension: Retrieving for ContentId: {ContentId}", contentId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                _logger.LogWarning("GetWorkflowContentExtension: ContentId is null or empty");
                return BadRequest(new { message = "ContentId is required" });
            }

            var extension = await _workflowService.GetWorkflowContentExtensionAsync(contentId);
            
            if (extension == null)
            {
                _logger.LogInformation("GetWorkflowContentExtension: No extension found for ContentId: {ContentId}", contentId);
                return NotFound(new { message = "Workflow content extension not found" });
            }
            
            _logger.LogInformation("GetWorkflowContentExtension: Found extension for ContentId {ContentId}, WorkflowInstanceId: {WorkflowInstanceId}", 
                contentId, extension.CurrentWorkflowInstanceId);
            return Ok(extension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowContentExtension: Error retrieving extension for ContentId: {ContentId}", contentId);
            return StatusCode(500, new { message = "An error occurred while retrieving the workflow content extension" });
        }
    }

    #endregion

    #region Workflow States

    [HttpGet("definitions/{definitionId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowState>>> GetWorkflowStates(Guid definitionId)
    {
        _logger.LogInformation("GetWorkflowStates: Starting to retrieve workflow states for definition ID: {DefinitionId}", definitionId);
        
        try
        {
            var states = await _workflowService.GetWorkflowStatesByDefinitionAsync(definitionId);
            _logger.LogInformation("GetWorkflowStates: Retrieved {Count} workflow states for definition {DefinitionId}", 
                states?.Count() ?? 0, definitionId);
            
            if (states?.Any() == true)
            {
                foreach (var state in states)
                {
                    _logger.LogDebug("GetWorkflowStates: Found state - ID: {Id}, StateId: {StateId}, Name: {Name}", 
                        state.Id, state.StateId, state.Name);
                }
            }
            
            return Ok(states);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowStates: Error retrieving workflow states for definition ID: {DefinitionId}", definitionId);
            return StatusCode(500, new { message = "An error occurred while retrieving workflow states", error = ex.Message });
        }
    }

    [HttpGet("states/{id}")]
    public async Task<ActionResult<WorkflowState>> GetWorkflowState(Guid id)
    {
        _logger.LogInformation("GetWorkflowState: Retrieving workflow state with ID: {Id}", id);
        
        try
        {
            var state = await _workflowService.GetWorkflowStateByIdAsync(id);
            if (state == null)
            {
                _logger.LogWarning("GetWorkflowState: Workflow state not found with ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("GetWorkflowState: Found workflow state - Name: {Name}, StateId: {StateId}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
                state.Name, state.StateId, state.WorkflowDefinitionId);
            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowState: Error retrieving workflow state with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the workflow state", error = ex.Message });
        }
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
