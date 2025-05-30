using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Piranha.AspNetCore.Telemetry;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;
using Piranha.Telemetry;

namespace Piranha.EditorialWorkflow.Controllers;

[ApiController]
[Route("api/workflow")]
public class EditorialWorkflowController : ControllerBase
{
    private readonly IEditorialWorkflowService _workflowService;
    private readonly ILogger<EditorialWorkflowController> _logger;
    private readonly IHostEnvironment _environment;

    public EditorialWorkflowController(IEditorialWorkflowService workflowService, ILogger<EditorialWorkflowController> logger, IHostEnvironment environment)
    {
        _workflowService = workflowService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Helper method to check if anonymous access should be allowed for load testing
    /// </summary>
    private bool AllowAnonymousForTesting()
    {
        return _environment.IsDevelopment() || _environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);
    }

    #region Workflow Definitions

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions()
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.ApiOperation, "GetWorkflowDefinitions");
        activity?.EnrichWithHttpContext(HttpContext);
        activity?.SetTag("operation.name", AspNetCoreTracingExtensions.CreateOperationName("WorkflowAPI", "GetAll", "WorkflowDefinitions"));
        
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
            
            activity?.SetTag("workflow.definitions.count", definitions?.Count() ?? 0);
            activity?.SetOperationStatus(true, $"Retrieved {definitions?.Count() ?? 0} workflow definitions");
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowDefinitions: Error retrieving workflow definitions");
            activity?.RecordException(ex);
            return StatusCode(500, "An error occurred while retrieving workflow definitions");
        }
    }

    [HttpGet("definitions/with-stats")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitionsWithStats()
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
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
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.ApiOperation, "GetWorkflowDefinition");
        activity?.EnrichWithHttpContext(HttpContext);
        activity?.SetTag("workflow.definition.id", id.ToString());
        activity?.SetTag("operation.name", AspNetCoreTracingExtensions.CreateOperationName("WorkflowAPI", "GetById", "WorkflowDefinition"));
        
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
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
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
    // [Authorize(Roles = "Admin")] // Commented out for testing
    public async Task<ActionResult<WorkflowState>> CreateWorkflowState(WorkflowState state)
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        _logger.LogInformation("CreateWorkflowState: Starting creation of new workflow state");
        _logger.LogInformation("CreateWorkflowState: Received state - StateId: {StateId}, Name: {Name}, WorkflowDefinitionId: {WorkflowDefinitionId}, IsInitial: {IsInitial}, IsPublished: {IsPublished}, IsFinal: {IsFinal}", 
            state?.StateId, state?.Name, state?.WorkflowDefinitionId, state?.IsInitial, state?.IsPublished, state?.IsFinal);

        try
        {
            if (state == null)
            {
                _logger.LogWarning("CreateWorkflowState: Received null workflow state");
                return BadRequest(new { message = "Workflow state cannot be null" });
            }

            if (string.IsNullOrWhiteSpace(state.StateId))
            {
                _logger.LogWarning("CreateWorkflowState: StateId is null or empty");
                return BadRequest(new { message = "State ID is required" });
            }

            if (string.IsNullOrWhiteSpace(state.Name))
            {
                _logger.LogWarning("CreateWorkflowState: State name is null or empty");
                return BadRequest(new { message = "State name is required" });
            }

            if (state.WorkflowDefinitionId == Guid.Empty)
            {
                _logger.LogWarning("CreateWorkflowState: WorkflowDefinitionId is empty");
                return BadRequest(new { message = "Workflow definition ID is required" });
            }

            _logger.LogDebug("CreateWorkflowState: Before calling service - State ID: {Id}, StateId: {StateId}, SortOrder: {SortOrder}, ColorCode: {ColorCode}", 
                state.Id, state.StateId, state.SortOrder, state.ColorCode);
            
            var result = await _workflowService.CreateWorkflowStateWithValidationAsync(state);
            
            _logger.LogInformation("CreateWorkflowState: Successfully created workflow state. ID: {Id}, StateId: {StateId}, Name: {Name}", 
                result.Id, result.StateId, result.Name);
                
            return CreatedAtAction(nameof(GetWorkflowState), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "CreateWorkflowState: Validation error creating workflow state. StateId: {StateId}, Name: {Name}", 
                state?.StateId, state?.Name);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowState: Error creating workflow state. StateId: {StateId}, Name: {Name}, Exception: {Exception}", 
                state?.StateId, state?.Name, ex.ToString());
            return StatusCode(500, new { message = "An error occurred while creating the workflow state", error = ex.Message });
        }
    }

    [HttpPut("states/{id}")]
    // [Authorize(Roles = "Admin")] // Commented out for testing
    public async Task<ActionResult<WorkflowState>> UpdateWorkflowState(Guid id, WorkflowState state)
    {
        _logger.LogInformation("UpdateWorkflowState: Starting update of workflow state. ID: {Id}, StateId: {StateId}, Name: {Name}", 
            id, state?.StateId, state?.Name);

        try
        {
            if (state == null)
            {
                _logger.LogWarning("UpdateWorkflowState: Received null workflow state");
                return BadRequest(new { message = "Workflow state cannot be null" });
            }

            if (id != state.Id)
            {
                _logger.LogWarning("UpdateWorkflowState: URL ID {UrlId} does not match state ID {StateId}", 
                    id, state.Id);
                return BadRequest(new { message = "ID mismatch" });
            }

            if (string.IsNullOrWhiteSpace(state.StateId))
            {
                _logger.LogWarning("UpdateWorkflowState: StateId is null or empty");
                return BadRequest(new { message = "State ID is required" });
            }

            if (string.IsNullOrWhiteSpace(state.Name))
            {
                _logger.LogWarning("UpdateWorkflowState: State name is null or empty");
                return BadRequest(new { message = "State name is required" });
            }

            _logger.LogDebug("UpdateWorkflowState: Before calling service - State details: StateId: {StateId}, Name: {Name}, WorkflowDefinitionId: {WorkflowDefinitionId}, IsInitial: {IsInitial}, IsPublished: {IsPublished}, IsFinal: {IsFinal}, SortOrder: {SortOrder}", 
                state.StateId, state.Name, state.WorkflowDefinitionId, state.IsInitial, state.IsPublished, state.IsFinal, state.SortOrder);

            var result = await _workflowService.UpdateWorkflowStateAsync(state);
            
            _logger.LogInformation("UpdateWorkflowState: Successfully updated workflow state. ID: {Id}, StateId: {StateId}, Name: {Name}", 
                result.Id, result.StateId, result.Name);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateWorkflowState: Error updating workflow state. ID: {Id}, StateId: {StateId}, Exception: {Exception}", 
                id, state?.StateId, ex.ToString());
            return StatusCode(500, new { message = "An error occurred while updating the workflow state", error = ex.Message });
        }
    }

    [HttpDelete("states/{id}")]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteWorkflowState(Guid id)
    {
        _logger.LogInformation("DeleteWorkflowState: Starting deletion of workflow state with ID: {Id}", id);

        try
        {
            var existingState = await _workflowService.GetWorkflowStateByIdAsync(id);
            if (existingState == null)
            {
                _logger.LogWarning("DeleteWorkflowState: Workflow state not found with ID: {Id}", id);
                return NotFound(new { message = "Workflow state not found" });
            }

            _logger.LogInformation("DeleteWorkflowState: Found state to delete - StateId: {StateId}, Name: {Name}", 
                existingState.StateId, existingState.Name);

            await _workflowService.DeleteWorkflowStateAsync(id);
            
            _logger.LogInformation("DeleteWorkflowState: Successfully deleted workflow state with ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowState: Error deleting workflow state with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the workflow state", error = ex.Message });
        }
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

    [HttpGet("rules/{id}")]
    public async Task<ActionResult<TransitionRule>> GetTransitionRule(Guid id)
    {
        _logger.LogInformation("GetTransitionRule: Retrieving transition rule with ID: {Id}", id);
        
        try
        {
            var rule = await _workflowService.GetTransitionRuleByIdAsync(id);
            if (rule == null)
            {
                _logger.LogWarning("GetTransitionRule: Transition rule not found with ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("GetTransitionRule: Found transition rule - FromStateId: {FromStateId}, ToStateId: {ToStateId}, AllowedRoles: {AllowedRoles}", 
                rule.FromStateId, rule.ToStateId, rule.AllowedRoles);
            return Ok(rule);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTransitionRule: Error retrieving transition rule with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the transition rule", error = ex.Message });
        }
    }

    [HttpPost("rules")]
    // [Authorize(Roles = "Admin")] // Commented out for testing
    public async Task<ActionResult<TransitionRule>> CreateTransitionRule(TransitionRule rule)
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        _logger.LogInformation("CreateTransitionRule: Starting creation of new transition rule");
        _logger.LogInformation("CreateTransitionRule: Received rule - FromStateId: {FromStateId}, ToStateId: {ToStateId}, AllowedRoles: {AllowedRoles}, Description: {Description}", 
            rule?.FromStateId, rule?.ToStateId, rule?.AllowedRoles, rule?.Description);

        try
        {
            if (rule == null)
            {
                _logger.LogWarning("CreateTransitionRule: Received null transition rule");
                return BadRequest(new { message = "Transition rule cannot be null" });
            }

            if (rule.FromStateId == Guid.Empty)
            {
                _logger.LogWarning("CreateTransitionRule: FromStateId is empty");
                return BadRequest(new { message = "Source state ID is required" });
            }

            if (rule.ToStateId == Guid.Empty)
            {
                _logger.LogWarning("CreateTransitionRule: ToStateId is empty");
                return BadRequest(new { message = "Target state ID is required" });
            }

            if (rule.FromStateId == rule.ToStateId)
            {
                _logger.LogWarning("CreateTransitionRule: FromStateId and ToStateId are the same: {StateId}", rule.FromStateId);
                return BadRequest(new { message = "Source and target states must be different" });
            }

            if (string.IsNullOrWhiteSpace(rule.AllowedRoles))
            {
                _logger.LogDebug("CreateTransitionRule: AllowedRoles is empty, setting to empty array");
                rule.AllowedRoles = "[]";
            }

            _logger.LogDebug("CreateTransitionRule: Before calling service - Rule ID: {Id}, RequiresComment: {RequiresComment}, IsActive: {IsActive}, SortOrder: {SortOrder}", 
                rule.Id, rule.RequiresComment, rule.IsActive, rule.SortOrder);
            
            var result = await _workflowService.CreateTransitionRuleAsync(rule);
            
            _logger.LogInformation("CreateTransitionRule: Successfully created transition rule. ID: {Id}, FromStateId: {FromStateId}, ToStateId: {ToStateId}", 
                result.Id, result.FromStateId, result.ToStateId);
                
            return CreatedAtAction(nameof(GetTransitionRule), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTransitionRule: Error creating transition rule. FromStateId: {FromStateId}, ToStateId: {ToStateId}, Exception: {Exception}", 
                rule?.FromStateId, rule?.ToStateId, ex.ToString());
            return StatusCode(500, new { message = "An error occurred while creating the transition rule", error = ex.Message });
        }
    }

    [HttpPut("rules/{id}")]
    // [Authorize(Roles = "Admin")] // Commented out for testing
    public async Task<ActionResult<TransitionRule>> UpdateTransitionRule(Guid id, TransitionRule rule)
    {
        _logger.LogInformation("UpdateTransitionRule: Starting update of transition rule. ID: {Id}, FromStateId: {FromStateId}, ToStateId: {ToStateId}", 
            id, rule?.FromStateId, rule?.ToStateId);

        try
        {
            if (rule == null)
            {
                _logger.LogWarning("UpdateTransitionRule: Received null transition rule");
                return BadRequest(new { message = "Transition rule cannot be null" });
            }

            if (id != rule.Id)
            {
                _logger.LogWarning("UpdateTransitionRule: URL ID {UrlId} does not match rule ID {RuleId}", 
                    id, rule.Id);
                return BadRequest(new { message = "ID mismatch" });
            }

            if (rule.FromStateId == Guid.Empty)
            {
                _logger.LogWarning("UpdateTransitionRule: FromStateId is empty");
                return BadRequest(new { message = "Source state ID is required" });
            }

            if (rule.ToStateId == Guid.Empty)
            {
                _logger.LogWarning("UpdateTransitionRule: ToStateId is empty");
                return BadRequest(new { message = "Target state ID is required" });
            }

            if (rule.FromStateId == rule.ToStateId)
            {
                _logger.LogWarning("UpdateTransitionRule: FromStateId and ToStateId are the same: {StateId}", rule.FromStateId);
                return BadRequest(new { message = "Source and target states must be different" });
            }

            _logger.LogDebug("UpdateTransitionRule: Before calling service - Rule details: AllowedRoles: {AllowedRoles}, Description: {Description}, CommentTemplate: {CommentTemplate}, RequiresComment: {RequiresComment}, IsActive: {IsActive}, SortOrder: {SortOrder}", 
                rule.AllowedRoles, rule.Description, rule.CommentTemplate, rule.RequiresComment, rule.IsActive, rule.SortOrder);

            var result = await _workflowService.UpdateTransitionRuleAsync(rule);
            
            _logger.LogInformation("UpdateTransitionRule: Successfully updated transition rule. ID: {Id}, FromStateId: {FromStateId}, ToStateId: {ToStateId}", 
                result.Id, result.FromStateId, result.ToStateId);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateTransitionRule: Error updating transition rule. ID: {Id}, FromStateId: {FromStateId}, ToStateId: {ToStateId}, Exception: {Exception}", 
                id, rule?.FromStateId, rule?.ToStateId, ex.ToString());
            return StatusCode(500, new { message = "An error occurred while updating the transition rule", error = ex.Message });
        }
    }

    [HttpDelete("rules/{id}")]
    // [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteTransitionRule(Guid id)
    {
        _logger.LogInformation("DeleteTransitionRule: Starting deletion of transition rule with ID: {Id}", id);

        try
        {
            var existingRule = await _workflowService.GetTransitionRuleByIdAsync(id);
            if (existingRule == null)
            {
                _logger.LogWarning("DeleteTransitionRule: Transition rule not found with ID: {Id}", id);
                return NotFound(new { message = "Transition rule not found" });
            }

            _logger.LogInformation("DeleteTransitionRule: Found rule to delete - FromStateId: {FromStateId}, ToStateId: {ToStateId}", 
                existingRule.FromStateId, existingRule.ToStateId);

            await _workflowService.DeleteTransitionRuleAsync(id);
            
            _logger.LogInformation("DeleteTransitionRule: Successfully deleted transition rule with ID: {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteTransitionRule: Error deleting transition rule with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the transition rule", error = ex.Message });
        }
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

    [HttpGet("debug/roles")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSystemRoles()
    {
        _logger.LogInformation("GetSystemRoles: Starting retrieval of system roles");

        try
        {
            var roles = await _workflowService.GetSystemRolesAsync();
            _logger.LogInformation("GetSystemRoles: Retrieved {Count} roles", roles?.Count() ?? 0);

            return Ok(roles.Select(r => new { 
                Id = r.Id,
                Name = r.Name,
                NormalizedName = r.NormalizedName
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSystemRoles: Error retrieving system roles");
            return StatusCode(500, new { 
                Error = "Failed to retrieve system roles", 
                Message = ex.Message 
            });
        }
    }

    #endregion

    #region Workflow Instances

    [HttpGet("instances")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetWorkflowInstances()
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        var instances = await _workflowService.GetWorkflowInstancesByUserAsync();
        return Ok(instances);
    }

    [HttpGet("instances/{id}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstance(Guid id)
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
        var instance = await _workflowService.GetWorkflowInstanceByIdAsync(id);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    [HttpPost("instances")]
    public async Task<ActionResult<WorkflowInstance>> CreateWorkflowInstance(WorkflowInstance instance)
    {
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
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
        // Allow anonymous access in development/testing environments for load testing
        if (!AllowAnonymousForTesting() && !User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }
        
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