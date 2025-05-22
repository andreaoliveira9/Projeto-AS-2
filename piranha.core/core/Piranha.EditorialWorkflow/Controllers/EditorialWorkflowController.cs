using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;

namespace Piranha.EditorialWorkflow.Controllers;

[ApiController]
[Route("api/workflow")]
[Authorize]
public class EditorialWorkflowController : ControllerBase
{
    private readonly IEditorialWorkflowService _workflowService;

    public EditorialWorkflowController(IEditorialWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    #region Workflow Definitions

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions()
    {
        var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkflowDefinition>> CreateWorkflowDefinition(WorkflowDefinition definition)
    {
        var result = await _workflowService.CreateWorkflowDefinitionAsync(definition);
        return CreatedAtAction(nameof(GetWorkflowDefinition), new { id = result.Id }, result);
    }

    [HttpPut("definitions/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WorkflowDefinition>> UpdateWorkflowDefinition(Guid id, WorkflowDefinition definition)
    {
        if (id != definition.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowDefinitionAsync(definition);
        return Ok(result);
    }

    #endregion

    #region Workflow States

    [HttpGet("definitions/{definitionId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowState>>> GetWorkflowStates(Guid definitionId)
    {
        var states = await _workflowService.GetWorkflowStatesByDefinitionAsync(definitionId);
        return Ok(states);
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
        var rules = await _workflowService.GetTransitionRulesByDefinitionAsync(definitionId);
        return Ok(rules);
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