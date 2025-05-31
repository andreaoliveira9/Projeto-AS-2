using Piranha.EditorialWorkflow.Models;

namespace Piranha.EditorialWorkflow.Services;

public interface IEditorialWorkflowService
{
    // Workflow Definitions
    Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition definition);
    Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(WorkflowDefinition definition);
    Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync();
    Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id);
    Task DeleteWorkflowDefinitionAsync(Guid id);
    Task<bool> CanDeleteWorkflowDefinitionAsync(Guid id);
    Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsWithStatsAsync();

    // Workflow States
    Task<WorkflowState> CreateWorkflowStateAsync(WorkflowState state);
    Task<WorkflowState> UpdateWorkflowStateAsync(WorkflowState state);
    Task<WorkflowState> GetWorkflowStateByIdAsync(Guid id);
    Task<IEnumerable<WorkflowState>> GetWorkflowStatesByDefinitionAsync(Guid definitionId);
    Task DeleteWorkflowStateAsync(Guid id);

    // Transition Rules
    Task<TransitionRule> CreateTransitionRuleAsync(TransitionRule rule);
    Task<TransitionRule> UpdateTransitionRuleAsync(TransitionRule rule);
    Task<TransitionRule> GetTransitionRuleByIdAsync(Guid id);
    Task<IEnumerable<TransitionRule>> GetTransitionRulesByDefinitionAsync(Guid definitionId);
    Task DeleteTransitionRuleAsync(Guid id);

    // Workflow Instances
    Task<WorkflowInstance> CreateWorkflowInstanceAsync(WorkflowInstance instance);
    Task<WorkflowInstance> UpdateWorkflowInstanceAsync(WorkflowInstance instance);
    Task<bool> TransitionWorkflowAsync(Guid instanceId, string targetState);
    Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByUserAsync();
    Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByStateAsync(string state);
    Task<WorkflowInstance> GetWorkflowInstanceByIdAsync(Guid id);

    // Workflow Content Extensions
    Task<bool> WorkflowContentExtensionExistsAsync(string contentId);
    Task<Piranha.EditorialWorkflow.Models.WorkflowContentExtension> GetWorkflowContentExtensionAsync(string contentId);
    Task<WorkflowInstance> CreateWorkflowInstanceWithContentAsync(string contentId, Guid workflowDefinitionId, string contentType = null, string contentTitle = null);

    // Debug methods
    Task<bool> TestDatabaseConnectionAsync();
    Task<IEnumerable<Piranha.AspNetCore.Identity.Data.Role>> GetSystemRolesAsync();
}
