using Piranha.EditorialWorkflow.Models;

namespace Piranha.EditorialWorkflow.Services;

public interface IEditorialWorkflowService
{
    // Workflow Definitions
    Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition definition);
    Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(WorkflowDefinition definition);
    Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync();
    Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id);

    // Workflow States
    Task<WorkflowState> CreateWorkflowStateAsync(WorkflowState state);
    Task<WorkflowState> UpdateWorkflowStateAsync(WorkflowState state);
    Task<IEnumerable<WorkflowState>> GetWorkflowStatesByDefinitionAsync(Guid definitionId);

    // Transition Rules
    Task<TransitionRule> CreateTransitionRuleAsync(TransitionRule rule);
    Task<TransitionRule> UpdateTransitionRuleAsync(TransitionRule rule);
    Task<IEnumerable<TransitionRule>> GetTransitionRulesByDefinitionAsync(Guid definitionId);

    // Workflow Instances
    Task<WorkflowInstance> CreateWorkflowInstanceAsync(WorkflowInstance instance);
    Task<WorkflowInstance> UpdateWorkflowInstanceAsync(WorkflowInstance instance);
    Task<bool> TransitionWorkflowAsync(Guid instanceId, string targetState);
    Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByUserAsync();
    Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByStateAsync(string state);
    Task<WorkflowInstance> GetWorkflowInstanceByIdAsync(Guid id);

    // Debug methods
    Task<bool> TestDatabaseConnectionAsync();
} 