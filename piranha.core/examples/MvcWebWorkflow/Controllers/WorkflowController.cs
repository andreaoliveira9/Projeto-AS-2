using Microsoft.AspNetCore.Mvc;
using Piranha.Workflow.Models;
using Piranha.Workflow.Services;

namespace MvcWebWorkflow.Controllers
{
    [ApiController]
    [Route("api/workflow")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpGet]
        [Route("definitions")]
        public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions()
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
            return Ok(definitions);
        }

        [HttpGet]
        [Route("definitions/{id}")]
        public async Task<ActionResult<WorkflowDefinition>> GetWorkflowDefinition(Guid id)
        {
            try
            {
                var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
                return Ok(definition);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("definitions/content-type/{contentType}")]
        public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitionsForContentType(string contentType)
        {
            var definitions = await _workflowService.GetWorkflowDefinitionsForContentTypeAsync(contentType);
            return Ok(definitions);
        }

        [HttpGet]
        [Route("instances/{id}")]
        public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstance(Guid id)
        {
            try
            {
                var instance = await _workflowService.GetWorkflowInstanceByIdAsync(id);
                return Ok(instance);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        [Route("content/{contentId}")]
        public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstanceForContent(Guid contentId)
        {
            try
            {
                var instance = await _workflowService.GetWorkflowInstanceForContentAsync(contentId);
                return Ok(instance);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        [Route("content/{contentId}/create")]
        public async Task<ActionResult<WorkflowInstance>> CreateWorkflowInstance(Guid contentId, [FromBody] CreateWorkflowInstanceRequest request)
        {
            try
            {
                var instance = await _workflowService.CreateWorkflowInstanceAsync(contentId, request.ContentType, request.WorkflowDefinitionId);
                return Ok(instance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("content/{contentId}/transitions")]
        public async Task<ActionResult<IEnumerable<TransitionRule>>> GetAvailableTransitions(Guid contentId, [FromQuery] Guid? userId = null)
        {
            var transitions = await _workflowService.GetAvailableTransitionsAsync(contentId, userId);
            return Ok(transitions);
        }

        [HttpPost]
        [Route("content/{contentId}/transition")]
        public async Task<ActionResult<WorkflowInstance>> PerformTransition(Guid contentId, [FromBody] PerformTransitionRequest request)
        {
            try
            {
                var instance = await _workflowService.PerformTransitionAsync(
                    contentId,
                    request.TransitionRuleId,
                    request.UserId,
                    request.Username,
                    request.Comment);
                return Ok(instance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("content/{contentId}/extension")]
        public async Task<ActionResult<WorkflowContentExtension>> GetContentExtension(Guid contentId)
        {
            try
            {
                var extension = await _workflowService.GetContentExtensionAsync(contentId);
                return Ok(extension);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

    public class CreateWorkflowInstanceRequest
    {
        public string ContentType { get; set; } = string.Empty;
        public Guid WorkflowDefinitionId { get; set; }
    }

    public class PerformTransitionRequest
    {
        public Guid TransitionRuleId { get; set; }
        public Guid? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
} 