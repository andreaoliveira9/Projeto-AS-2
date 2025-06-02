using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;

namespace MvcWeb.Controllers;

[ApiController]
[Route("api/test/workflow")]
[AllowAnonymous] // Allow anonymous access for load testing
public class TestWorkflowApiController : BaseApiController
{
    private readonly IEditorialWorkflowService _workflowService;
    private readonly ILogger<TestWorkflowApiController> _logger;

    public TestWorkflowApiController(
        IEditorialWorkflowService workflowService, 
        ILogger<TestWorkflowApiController> logger,
        IWebHostEnvironment environment) : base(environment)
    {
        _workflowService = workflowService;
        _logger = logger;
    }

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions()
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow definitions");
            return StatusCode(500, "Error getting workflow definitions");
        }
    }

    [HttpGet("definitions/with-stats")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitionsWithStats()
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var definitions = await _workflowService.GetAllWorkflowDefinitionsWithStatsAsync();
            return Ok(definitions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow definitions with stats");
            return StatusCode(500, "Error getting workflow definitions with stats");
        }
    }

    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> GetWorkflowDefinition(Guid id)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
            if (definition == null)
                return NotFound();

            return Ok(definition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow definition {DefinitionId}", id);
            return StatusCode(500, "Error getting workflow definition");
        }
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<WorkflowDefinition>> CreateWorkflowDefinition(WorkflowDefinition definition)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var result = await _workflowService.CreateWorkflowDefinitionAsync(definition);
            return CreatedAtAction(nameof(GetWorkflowDefinition), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow definition");
            return StatusCode(500, "Error creating workflow definition");
        }
    }

    [HttpGet("definitions/{definitionId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowState>>> GetWorkflowStates(Guid definitionId)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var states = await _workflowService.GetWorkflowStatesByDefinitionAsync(definitionId);
            return Ok(states);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow states for definition {DefinitionId}", definitionId);
            return StatusCode(500, "Error getting workflow states");
        }
    }

    [HttpPost("states")]
    public async Task<ActionResult<WorkflowState>> CreateWorkflowState(WorkflowState state)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var result = await _workflowService.CreateWorkflowStateAsync(state);
            return CreatedAtAction("GetWorkflowState", "workflow", new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow state");
            return StatusCode(500, "Error creating workflow state");
        }
    }

    [HttpPost("rules")]
    public async Task<ActionResult<TransitionRule>> CreateTransitionRule(TransitionRule rule)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var result = await _workflowService.CreateTransitionRuleAsync(rule);
            return CreatedAtAction("GetTransitionRule", "workflow", new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transition rule");
            return StatusCode(500, "Error creating transition rule");
        }
    }

    [HttpGet("instances")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetWorkflowInstances()
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            // Get all instances instead of user-specific ones for testing
            var instances = await _workflowService.GetAllWorkflowInstancesAsync();
            return Ok(instances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow instances");
            return StatusCode(500, "Error getting workflow instances");
        }
    }

    [HttpPost("instances")]
    public async Task<ActionResult<WorkflowInstance>> CreateWorkflowInstance(WorkflowInstance instance)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            // For testing, use system user
            instance.CreatedBy = "system";
            var result = await _workflowService.CreateWorkflowInstanceAsync(instance);
            return CreatedAtAction("GetWorkflowInstance", "workflow", new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow instance");
            return StatusCode(500, "Error creating workflow instance");
        }
    }

    [HttpPost("instances/{id}/transition")]
    public async Task<ActionResult> TransitionWorkflow(Guid id, [FromBody] string targetState)
    {
        try
        {
            if (!AllowAnonymousForTesting())
                return Unauthorized("This endpoint is only available in development/testing");

            var success = await _workflowService.TransitionWorkflowAsync(id, targetState);
            if (!success)
                return BadRequest("Invalid transition");

            return Ok(new { success = true, message = "Transition completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning workflow {InstanceId} to {TargetState}", id, targetState);
            return StatusCode(500, "Error transitioning workflow");
        }
    }

    [HttpGet("debug/database")]
    public async Task<ActionResult> DebugDatabase()
    {
        try
        {
            var canConnect = await _workflowService.TestDatabaseConnectionAsync();
            return Ok(new { 
                DatabaseConnected = canConnect,
                Message = canConnect ? "Database connection successful" : "Database connection failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing database connection");
            return StatusCode(500, "Error testing database connection");
        }
    }

    [HttpGet("debug/roles")]
    public async Task<ActionResult> GetSystemRoles()
    {
        try
        {
            var roles = await _workflowService.GetSystemRolesAsync();
            return Ok(roles.Select(r => new { 
                Id = r.Id,
                Name = r.Name,
                NormalizedName = r.NormalizedName
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system roles");
            return StatusCode(500, "Error getting system roles");
        }
    }
}