using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager.Controllers;
using Piranha.Manager.Models;
using Piranha.Workflow.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Piranha.Manager.Workflow.Controllers
{
    /// <summary>
    /// The workflow controller.
    /// </summary>
    [Area("Manager")]
    [Authorize(Policy = Permissions.Workflows)]
    public class WorkflowController : ManagerController
    {
        private readonly IWorkflowService _workflowService;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="workflowService">The workflow service</param>
        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        /// <summary>
        /// Gets the workflow list view.
        /// </summary>
        [Route("manager/workflows")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var model = new ListModel
            {
                Definitions = await _workflowService.GetAllWorkflowDefinitionsAsync()
            };
            return View(model);
        }

        /// <summary>
        /// Gets the workflow details view.
        /// </summary>
        /// <param name="id">The workflow id</param>
        [Route("manager/workflow/{id:Guid}")]
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
            
            var model = new WorkflowModel
            {
                Definition = definition
            };
            
            return View(model);
        }

        /// <summary>
        /// Gets available workflow definitions for dropdowns.
        /// </summary>
        [Route("manager/api/workflow/definitions")]
        [HttpGet]
        public async Task<IActionResult> GetWorkflowDefinitions()
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
            
            // Debug: Print definitions to console
            Console.WriteLine($"Found {definitions?.Count() ?? 0} workflow definitions");
            foreach (var def in definitions ?? Enumerable.Empty<Piranha.Workflow.Models.WorkflowDefinition>())
            {
                Console.WriteLine($" - {def.Id}: {def.Name}");
            }
            
            return Json(definitions);
        }

        /// <summary>
        /// Debug endpoint - Gets available workflow definitions without authorization.
        /// </summary>
        [Route("manager/api/workflow/test-definitions")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkflowDefinitionsNoAuth()
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
            
            // Debug: Print definitions to console
            Console.WriteLine($"[NO AUTH] Found {definitions?.Count() ?? 0} workflow definitions");
            foreach (var def in definitions ?? Enumerable.Empty<Piranha.Workflow.Models.WorkflowDefinition>())
            {
                Console.WriteLine($" - {def.Id}: {def.Name}");
            }
            
            return Json(new { 
                status = "success",
                message = $"Found {definitions?.Count() ?? 0} workflow definitions",
                definitions = definitions
            });
        }

        /// <summary>
        /// Gets the API endpoint for content workflow instances.
        /// </summary>
        /// <param name="contentId">The content id</param>
        [Route("manager/api/workflow/content/{contentId:Guid}")]
        [HttpGet]
        public async Task<IActionResult> GetContentWorkflow(Guid contentId)
        {
            try
            {
                var instance = await _workflowService.GetWorkflowInstanceForContentAsync(contentId);
                return Json(new
                {
                    status = "success",
                    instance
                });
            }
            catch
            {
                return Json(new
                {
                    status = "error",
                    message = "Content has no workflow instance attached"
                });
            }
        }

        /// <summary>
        /// Gets the API endpoint for available transitions.
        /// </summary>
        /// <param name="contentId">The content id</param>
        [Route("manager/api/workflow/content/{contentId:Guid}/transitions")]
        [HttpGet]
        public async Task<IActionResult> GetAvailableTransitions(Guid contentId)
        {
            var transitions = await _workflowService.GetAvailableTransitionsAsync(contentId);
            return Json(new
            {
                status = "success",
                transitions
            });
        }

        /// <summary>
        /// Performs a workflow transition.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="model">The transition model</param>
        [Route("manager/api/workflow/content/{contentId:Guid}/transition")]
        [HttpPost]
        public async Task<IActionResult> PerformTransition(Guid contentId, [FromBody] TransitionModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "Invalid transition data"
                    });
                }

                var userId = User.Identity.IsAuthenticated ? 
                    Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString()) : 
                    (Guid?)null;

                var instance = await _workflowService.PerformTransitionAsync(
                    contentId,
                    model.TransitionId,
                    userId,
                    User.Identity.Name,
                    model.Comment);

                return Json(new
                {
                    status = "success",
                    instance
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Creates a new workflow instance for content.
        /// </summary>
        /// <param name="contentId">The content id</param>
        /// <param name="model">The creation model</param>
        [Route("manager/api/workflow/content/{contentId:Guid}/create")]
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowInstance(Guid contentId, [FromBody] CreateWorkflowModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new
                    {
                        status = "error",
                        message = "Invalid workflow data"
                    });
                }

                var instance = await _workflowService.CreateWorkflowInstanceAsync(
                    contentId,
                    model.ContentType,
                    model.WorkflowDefinitionId);

                return Json(new
                {
                    status = "success",
                    instance
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }
    }
} 