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

    [HttpPost("states")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkflowState>> CreateWorkflowState(WorkflowState state)
    {
        var result = await _workflowService.CreateWorkflowStateAsync(state);
        return Ok(result);
    }

    [HttpPut("states/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkflowState>> UpdateWorkflowState(Guid id, WorkflowState state)
    {
        if (id != state.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowStateAsync(state);
        return Ok(result);
    }

    #endregion

    #region Transition Rules

    [HttpGet("definitions/{definitionId}/rules")]
    public async Task<ActionResult<IEnumerable<TransitionRule>>> GetTransitionRules(Guid definitionId)
    {
        _logger.LogInformation("GetTransitionRules: Starting to retrieve transition rules for definition ID: {DefinitionId}", definitionId);
        
        try
        {
            var rules = await _workflowService.GetTransitionRulesByDefinitionAsync(definitionId);
            _logger.LogInformation("GetTransitionRules: Retrieved {Count} transition rules for definition {DefinitionId}", 
                rules?.Count() ?? 0, definitionId);
            
            if (rules?.Any() == true)
            {
                foreach (var rule in rules)
                {
                    _logger.LogDebug("GetTransitionRules: Found rule - ID: {Id}, FromStateId: {FromStateId}, ToStateId: {ToStateId}", 
                        rule.Id, rule.FromStateId, rule.ToStateId);
                }
            }
            
            return Ok(rules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTransitionRules: Error retrieving transition rules for definition ID: {DefinitionId}", definitionId);
            return StatusCode(500, new { message = "An error occurred while retrieving transition rules", error = ex.Message });
        }
    }

    [HttpPost("rules")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TransitionRule>> CreateTransitionRule(TransitionRule rule)
    {
        var result = await _workflowService.CreateTransitionRuleAsync(rule);
        return Ok(result);
    }

    [HttpPut("rules/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TransitionRule>> UpdateTransitionRule(Guid id, TransitionRule rule)
    {
        if (id != rule.Id)
            return BadRequest();

        var result = await _workflowService.UpdateTransitionRuleAsync(rule);
        return Ok(result);
    }

    #endregion

    #region Debug Endpoints

    [HttpGet("debug/database")]
    [AllowAnonymous]
    public async Task<ActionResult> DebugDatabase()
    {
        _logger.LogInformation("DebugDatabase: Starting database debug check");

        try
        {
            // Check if we can connect to the database
            var canConnect = await _workflowService.TestDatabaseConnectionAsync();
            _logger.LogInformation("DebugDatabase: Database connection test result: {CanConnect}", canConnect);

            return Ok(new { 
                DatabaseConnected = canConnect,
                Message = canConnect ? "Database connection successful" : "Database connection failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DebugDatabase: Error testing database connection");
            return StatusCode(500, new { 
                Error = "Database connection test failed", 
                Message = ex.Message 
            });
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

    #endregion
} 