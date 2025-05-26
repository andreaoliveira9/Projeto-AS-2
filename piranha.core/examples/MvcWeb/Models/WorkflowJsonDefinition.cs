namespace MvcWeb.Models;

public class WorkflowJsonDefinition
{
    public string WorkflowName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<StateDefinition> States { get; set; } = new();
}

public class StateDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsInitial { get; set; }
    public bool IsPublished { get; set; }
    public List<TransitionDefinition> Transitions { get; set; } = new();
}

public class TransitionDefinition
{
    public string ToState { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public string Comment { get; set; } = "";
} 