using Piranha.Workflow.Models;

namespace MvcWebWorkflow.Models
{
    public class WorkflowListViewModel
    {
        public List<WorkflowDefinition> Definitions { get; set; } = new List<WorkflowDefinition>();
    }

    public class WorkflowDetailViewModel
    {
        public WorkflowDefinition Definition { get; set; } = new WorkflowDefinition();
    }
} 