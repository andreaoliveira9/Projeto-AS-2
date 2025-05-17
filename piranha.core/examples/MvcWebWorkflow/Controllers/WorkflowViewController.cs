using Microsoft.AspNetCore.Mvc;
using Piranha.Workflow.Models;
using Piranha.Workflow.Services;
using MvcWebWorkflow.Models;

namespace MvcWebWorkflow.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WorkflowViewController : Controller
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowViewController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [Route("/workflows")]
        public async Task<IActionResult> Index()
        {
            var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
            return View(new WorkflowListViewModel
            {
                Definitions = definitions.ToList()
            });
        }

        [Route("/workflow/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
                return View(new WorkflowDetailViewModel
                {
                    Definition = definition
                });
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
} 