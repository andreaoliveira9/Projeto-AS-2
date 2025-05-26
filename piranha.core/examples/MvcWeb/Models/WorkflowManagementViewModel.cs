using Piranha.EditorialWorkflow.Models;

namespace MvcWeb.Models;

public class WorkflowManagementViewModel
{
    public List<WorkflowDefinition> Workflows { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}

public class CreateWorkflowViewModel
{
    public string WorkflowJson { get; set; } = "";
    public string? ErrorMessage { get; set; }
} 