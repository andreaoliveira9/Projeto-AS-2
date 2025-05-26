using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Piranha.AspNetCore.Identity.Data;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Repositories;

namespace Piranha.EditorialWorkflow.Services;

public class EditorialWorkflowService : IEditorialWorkflowService
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IWorkflowStateRepository _workflowStateRepository;
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly ITransitionRuleRepository _transitionRuleRepository;
    private readonly IWorkflowContentExtensionRepository _contentExtensionRepository;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Piranha.AspNetCore.Identity.Data.Role> _roleManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<EditorialWorkflowService> _logger;

    public EditorialWorkflowService(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IWorkflowStateRepository workflowStateRepository,
        IWorkflowInstanceRepository workflowInstanceRepository,
        ITransitionRuleRepository transitionRuleRepository,
        IWorkflowContentExtensionRepository contentExtensionRepository,
        UserManager<User> userManager,
        RoleManager<Piranha.AspNetCore.Identity.Data.Role> roleManager,
        IHttpContextAccessor httpContextAccessor,
        ILogger<EditorialWorkflowService> logger)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _workflowStateRepository = workflowStateRepository;
        _workflowInstanceRepository = workflowInstanceRepository;
        _transitionRuleRepository = transitionRuleRepository;
        _contentExtensionRepository = contentExtensionRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition definition)
    {
        _logger.LogInformation("CreateWorkflowDefinitionAsync: Starting creation of workflow definition. Name: {Name}, IsActive: {IsActive}", 
            definition?.Name, definition?.IsActive);

        try
        {
            if (definition == null)
            {
                _logger.LogError("CreateWorkflowDefinitionAsync: Definition is null");
                throw new ArgumentNullException(nameof(definition));
            }

            // Get current user information
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            string userId = user?.Id.ToString() ?? "system";
            
            if (user != null)
            {
                _logger.LogDebug("CreateWorkflowDefinitionAsync: Current user ID: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("CreateWorkflowDefinitionAsync: Could not get current user from context, using fallback 'system'");
            }

            // Ensure ID is set
            if (definition.Id == Guid.Empty)
            {
                definition.Id = Guid.NewGuid();
                _logger.LogDebug("CreateWorkflowDefinitionAsync: Generated new ID: {Id}", definition.Id);
            }

            // Check if this is the first workflow being created
            var existingWorkflows = await _workflowDefinitionRepository.GetAll();
            var existingWorkflowsList = existingWorkflows?.ToList() ?? new List<WorkflowDefinition>();
            
            if (!existingWorkflowsList.Any())
            {
                // If this is the first workflow, it must be active
                definition.IsActive = true;
                _logger.LogInformation("CreateWorkflowDefinitionAsync: First workflow being created, setting as active");
            }
            else if (definition.IsActive)
            {
                // If setting this workflow as active, ensure no other workflow is active
                await EnsureOnlyOneActiveWorkflowAsync();
                _logger.LogInformation("CreateWorkflowDefinitionAsync: Setting workflow as active, deactivated other workflows");
            }

            // Set audit fields
            definition.CreatedBy = userId;
            definition.LastModifiedBy = userId;
            definition.Created = DateTime.UtcNow;
            definition.LastModified = DateTime.UtcNow;

            _logger.LogDebug("CreateWorkflowDefinitionAsync: Before repository save - ID: {Id}, Name: {Name}, CreatedBy: {CreatedBy}, IsActive: {IsActive}", 
                definition.Id, definition.Name, definition.CreatedBy, definition.IsActive);

            await _workflowDefinitionRepository.Save(definition);
            
            _logger.LogInformation("CreateWorkflowDefinitionAsync: Successfully saved workflow definition. ID: {Id}, Name: {Name}, IsActive: {IsActive}", 
                definition.Id, definition.Name, definition.IsActive);

            // Verify the save by attempting to retrieve the workflow
            var savedDefinition = await _workflowDefinitionRepository.GetById(definition.Id);
            if (savedDefinition != null)
            {
                _logger.LogInformation("CreateWorkflowDefinitionAsync: Verification successful - workflow found after save. ID: {Id}", definition.Id);
            }
            else
            {
                _logger.LogWarning("CreateWorkflowDefinitionAsync: Verification failed - workflow not found after save. ID: {Id}", definition.Id);
            }

            return definition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowDefinitionAsync: Error creating workflow definition. Name: {Name}", definition?.Name);
            throw;
        }
    }

    public async Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(WorkflowDefinition definition)
    {
        _logger.LogInformation("UpdateWorkflowDefinitionAsync: Starting update of workflow definition. Name: {Name}, IsActive: {IsActive}", 
            definition?.Name, definition?.IsActive);

        try
        {
            // If setting this workflow as active, ensure no other workflow is active
            if (definition.IsActive)
            {
                await EnsureOnlyOneActiveWorkflowAsync(definition.Id);
                _logger.LogInformation("UpdateWorkflowDefinitionAsync: Setting workflow as active, deactivated other workflows");
            }
            else
            {
                // If deactivating this workflow, ensure at least one workflow remains active
                var allWorkflows = await _workflowDefinitionRepository.GetAll();
                var otherActiveWorkflows = allWorkflows?.Where(w => w.IsActive && w.Id != definition.Id).ToList();
                
                if (otherActiveWorkflows?.Any() != true)
                {
                    _logger.LogWarning("UpdateWorkflowDefinitionAsync: Cannot deactivate workflow {Id} as it would leave no active workflows", definition.Id);
                    throw new InvalidOperationException("Cannot deactivate the last active workflow. At least one workflow must remain active.");
                }
            }

            // Set audit fields
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            string userId = user?.Id.ToString() ?? "system";
            
            definition.LastModifiedBy = userId;
            definition.LastModified = DateTime.UtcNow;

            await _workflowDefinitionRepository.Save(definition);
            
            _logger.LogInformation("UpdateWorkflowDefinitionAsync: Successfully updated workflow definition. ID: {Id}, Name: {Name}, IsActive: {IsActive}", 
                definition.Id, definition.Name, definition.IsActive);

            return definition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateWorkflowDefinitionAsync: Error updating workflow definition. Name: {Name}", definition?.Name);
            throw;
        }
    }

    public async Task DeleteWorkflowDefinitionAsync(Guid id)
    {
        _logger.LogInformation("DeleteWorkflowDefinitionAsync: Deleting workflow definition with ID: {Id}", id);
        
        try
        {
            // Check if the workflow being deleted is active
            var workflowToDelete = await _workflowDefinitionRepository.GetById(id);
            if (workflowToDelete == null)
            {
                _logger.LogWarning("DeleteWorkflowDefinitionAsync: Workflow {Id} not found", id);
                throw new InvalidOperationException("Workflow not found");
            }

            bool wasActive = workflowToDelete.IsActive;
            
            // Delete the workflow
            await _workflowDefinitionRepository.Delete(id);
            _logger.LogInformation("DeleteWorkflowDefinitionAsync: Successfully deleted workflow definition with ID: {Id}", id);
            
            // If we deleted an active workflow, ensure at least one remaining workflow is active
            if (wasActive)
            {
                await EnsureAtLeastOneActiveWorkflowAsync();
                _logger.LogInformation("DeleteWorkflowDefinitionAsync: Ensured at least one workflow remains active after deletion");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowDefinitionAsync: Error deleting workflow definition with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> CanDeleteWorkflowDefinitionAsync(Guid id)
    {
        _logger.LogInformation("CanDeleteWorkflowDefinitionAsync: Checking if workflow definition {Id} can be deleted", id);
        
        try
        {
            // Check if there are any workflow instances using this definition
            var instances = await _workflowInstanceRepository.GetByWorkflow(id);
            var hasInstances = instances?.Any() == true;
            
            _logger.LogInformation("CanDeleteWorkflowDefinitionAsync: Workflow {Id} has instances: {HasInstances}", 
                id, hasInstances);
            
            return !hasInstances;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CanDeleteWorkflowDefinitionAsync: Error checking if workflow definition {Id} can be deleted", id);
            throw;
        }
    }

    private async Task EnsureOnlyOneActiveWorkflowAsync(Guid? excludeId = null)
    {
        _logger.LogInformation("EnsureOnlyOneActiveWorkflowAsync: Ensuring only one active workflow exists. ExcludeId: {ExcludeId}", excludeId);
        
        try
        {
            var allWorkflows = await _workflowDefinitionRepository.GetAll();
            var activeWorkflows = allWorkflows?.Where(w => w.IsActive && w.Id != excludeId).ToList();
            
            if (activeWorkflows?.Any() == true)
            {
                _logger.LogInformation("EnsureOnlyOneActiveWorkflowAsync: Found {Count} other active workflows, deactivating them", 
                    activeWorkflows.Count);
                
                foreach (var workflow in activeWorkflows)
                {
                    workflow.IsActive = false;
                    workflow.LastModified = DateTime.UtcNow;
                    // Get current user for audit trail
                    var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                    workflow.LastModifiedBy = user?.Id.ToString() ?? "system";
                    
                    await _workflowDefinitionRepository.Save(workflow);
                    
                    _logger.LogDebug("EnsureOnlyOneActiveWorkflowAsync: Deactivated workflow {Id} ({Name})", 
                        workflow.Id, workflow.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EnsureOnlyOneActiveWorkflowAsync: Error ensuring only one active workflow");
            throw;
        }
    }

    private async Task EnsureAtLeastOneActiveWorkflowAsync()
    {
        _logger.LogInformation("EnsureAtLeastOneActiveWorkflowAsync: Ensuring at least one workflow is active");
        
        try
        {
            var allWorkflows = await _workflowDefinitionRepository.GetAll();
            var workflowsList = allWorkflows?.ToList() ?? new List<WorkflowDefinition>();
            
            if (!workflowsList.Any())
            {
                _logger.LogInformation("EnsureAtLeastOneActiveWorkflowAsync: No workflows exist");
                return;
            }

            var activeWorkflows = workflowsList.Where(w => w.IsActive).ToList();
            
            if (!activeWorkflows.Any())
            {
                _logger.LogInformation("EnsureAtLeastOneActiveWorkflowAsync: No active workflows found, activating the first one");
                
                // Activate the first workflow (by creation date)
                var firstWorkflow = workflowsList.OrderBy(w => w.Created).First();
                firstWorkflow.IsActive = true;
                firstWorkflow.LastModified = DateTime.UtcNow;
                
                // Get current user for audit trail
                var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
                firstWorkflow.LastModifiedBy = user?.Id.ToString() ?? "system";
                
                await _workflowDefinitionRepository.Save(firstWorkflow);
                
                _logger.LogInformation("EnsureAtLeastOneActiveWorkflowAsync: Activated workflow {Id} ({Name})", 
                    firstWorkflow.Id, firstWorkflow.Name);
            }
            else
            {
                _logger.LogInformation("EnsureAtLeastOneActiveWorkflowAsync: Found {Count} active workflows", activeWorkflows.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EnsureAtLeastOneActiveWorkflowAsync: Error ensuring at least one active workflow");
            throw;
        }
    }

    public async Task<WorkflowState> CreateWorkflowStateAsync(WorkflowState state)
    {
        await _workflowStateRepository.Save(state);
        return state;
    }

    public async Task<WorkflowState> UpdateWorkflowStateAsync(WorkflowState state)
    {
        await _workflowStateRepository.Save(state);
        return state;
    }

    public async Task<TransitionRule> CreateTransitionRuleAsync(TransitionRule rule)
    {
        await _transitionRuleRepository.Save(rule);
        return rule;
    }

    public async Task<TransitionRule> UpdateTransitionRuleAsync(TransitionRule rule)
    {
        await _transitionRuleRepository.Save(rule);
        return rule;
    }

    public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        var roles = await _userManager.GetRolesAsync(user);

        instance.CreatedBy = user.Id.ToString();
        instance.Created = DateTime.UtcNow;
        // Buscar o estado inicial do workflow
        var workflow = await _workflowDefinitionRepository.GetWithStates(instance.WorkflowDefinitionId);
        var initialState = workflow.States.FirstOrDefault(s => s.IsInitial);
        if (initialState == null)
            throw new Exception("Estado inicial não encontrado para o workflow.");
        instance.CurrentStateId = initialState.Id;
        await _workflowInstanceRepository.Save(instance);
        return instance;
    }

    public async Task<WorkflowInstance> UpdateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        await _workflowInstanceRepository.Save(instance);
        return instance;
    }

    public async Task<bool> TransitionWorkflowAsync(Guid instanceId, string targetStateId)
    {
        var instance = await _workflowInstanceRepository.GetById(instanceId);
        if (instance == null)
            return false;

        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        var roles = await _userManager.GetRolesAsync(user);

        // Buscar o estado alvo
        var workflow = await _workflowDefinitionRepository.GetWithStates(instance.WorkflowDefinitionId);
        var targetState = workflow.States.FirstOrDefault(s => s.StateId == targetStateId);
        if (targetState == null)
            return false;

        // Verifica se existe uma regra de transição válida
        var transitionRule = await _transitionRuleRepository.GetTransition(instance.CurrentStateId, targetState.Id);
        if (transitionRule == null)
            return false;

        // Verifica se o usuário tem permissão para fazer a transição
        if (!roles.Any(r => transitionRule.AllowedRoles.Contains(r)))
            return false;

        // Atualiza o estado
        instance.CurrentStateId = targetState.Id;
        instance.LastModified = DateTime.UtcNow;
        await _workflowInstanceRepository.Save(instance);
        return true;
    }

    public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByUserAsync()
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        return await _workflowInstanceRepository.GetByUser(user.Id.ToString());
    }

    public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByStateAsync(string stateId)
    {
        // Buscar o estado pelo StateId
        // Aqui, para simplificar, vamos buscar todos os workflows e filtrar os estados
        var allWorkflows = await _workflowDefinitionRepository.GetAll();
        var stateGuids = allWorkflows.SelectMany(w => w.States).Where(s => s.StateId == stateId).Select(s => s.Id).ToList();
        var result = new List<WorkflowInstance>();
        foreach (var guid in stateGuids)
        {
            var instances = await _workflowInstanceRepository.GetByState(guid);
            result.AddRange(instances);
        }
        return result;
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceByIdAsync(Guid id)
    {
        return await _workflowInstanceRepository.GetById(id);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync()
    {
        _logger.LogInformation("GetAllWorkflowDefinitionsAsync: Starting retrieval of all workflow definitions");

        try
        {
            var definitions = await _workflowDefinitionRepository.GetAll();
            var definitionsList = definitions?.ToList() ?? new List<WorkflowDefinition>();
            
            _logger.LogInformation("GetAllWorkflowDefinitionsAsync: Retrieved {Count} workflow definitions", definitionsList.Count);
            
            if (definitionsList.Any())
            {
                foreach (var def in definitionsList)
                {
                    _logger.LogDebug("GetAllWorkflowDefinitionsAsync: Found workflow - ID: {Id}, Name: {Name}, IsActive: {IsActive}, Created: {Created}", 
                        def.Id, def.Name, def.IsActive, def.Created);
                }
            }
            else
            {
                _logger.LogInformation("GetAllWorkflowDefinitionsAsync: No workflow definitions found in database");
            }

            return definitionsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllWorkflowDefinitionsAsync: Error retrieving workflow definitions");
            throw;
        }
    }

    public async Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id)
    {
        return await _workflowDefinitionRepository.GetById(id);
    }

    public async Task<IEnumerable<WorkflowState>> GetWorkflowStatesByDefinitionAsync(Guid definitionId)
    {
        _logger.LogInformation("GetWorkflowStatesByDefinitionAsync: Starting retrieval of workflow states for definition ID: {DefinitionId}", definitionId);

        try
        {
            var states = await _workflowStateRepository.GetByWorkflow(definitionId);
            var statesList = states?.ToList() ?? new List<WorkflowState>();
            
            _logger.LogInformation("GetWorkflowStatesByDefinitionAsync: Retrieved {Count} workflow states for definition {DefinitionId}", 
                statesList.Count, definitionId);
            
            if (statesList.Any())
            {
                foreach (var state in statesList)
                {
                    _logger.LogDebug("GetWorkflowStatesByDefinitionAsync: Found state - ID: {Id}, StateId: {StateId}, Name: {Name}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
                        state.Id, state.StateId, state.Name, state.WorkflowDefinitionId);
                }
            }
            else
            {
                _logger.LogInformation("GetWorkflowStatesByDefinitionAsync: No workflow states found for definition {DefinitionId}", definitionId);
            }

            return statesList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowStatesByDefinitionAsync: Error retrieving workflow states for definition {DefinitionId}", definitionId);
            throw;
        }
    }

    public async Task<IEnumerable<TransitionRule>> GetTransitionRulesByDefinitionAsync(Guid definitionId)
    {
        _logger.LogInformation("GetTransitionRulesByDefinitionAsync: Starting retrieval of transition rules for definition ID: {DefinitionId}", definitionId);

        try
        {
            // Buscar todos os estados do workflow
            var states = await _workflowStateRepository.GetByWorkflow(definitionId);
            var statesList = states?.ToList() ?? new List<WorkflowState>();
            
            _logger.LogDebug("GetTransitionRulesByDefinitionAsync: Found {Count} states for definition {DefinitionId}", 
                statesList.Count, definitionId);
            
            var rules = new List<TransitionRule>();
            foreach (var state in statesList)
            {
                _logger.LogDebug("GetTransitionRulesByDefinitionAsync: Getting transitions for state {StateId} (ID: {Id})", 
                    state.StateId, state.Id);
                
                var transitions = await _transitionRuleRepository.GetByFromState(state.Id);
                var transitionsList = transitions?.ToList() ?? new List<TransitionRule>();
                
                _logger.LogDebug("GetTransitionRulesByDefinitionAsync: Found {Count} transitions for state {StateId}", 
                    transitionsList.Count, state.StateId);
                
                rules.AddRange(transitionsList);
            }
            
            _logger.LogInformation("GetTransitionRulesByDefinitionAsync: Retrieved {Count} total transition rules for definition {DefinitionId}", 
                rules.Count, definitionId);

            return rules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTransitionRulesByDefinitionAsync: Error retrieving transition rules for definition {DefinitionId}", definitionId);
            throw;
        }
    }

    public async Task<WorkflowState> GetWorkflowStateByIdAsync(Guid id)
    {
        _logger.LogInformation("GetWorkflowStateByIdAsync: Retrieving workflow state with ID: {Id}", id);
        
        try
        {
            var state = await _workflowStateRepository.GetById(id);
            if (state != null)
            {
                _logger.LogInformation("GetWorkflowStateByIdAsync: Found workflow state - StateId: {StateId}, Name: {Name}", 
                    state.StateId, state.Name);
            }
            else
            {
                _logger.LogInformation("GetWorkflowStateByIdAsync: No workflow state found with ID: {Id}", id);
            }
            
            return state;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowStateByIdAsync: Error retrieving workflow state with ID: {Id}", id);
            throw;
        }
    }

    public async Task DeleteWorkflowStateAsync(Guid id)
    {
        _logger.LogInformation("DeleteWorkflowStateAsync: Deleting workflow state with ID: {Id}", id);
        
        try
        {
            await _workflowStateRepository.Delete(id);
            _logger.LogInformation("DeleteWorkflowStateAsync: Successfully deleted workflow state with ID: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowStateAsync: Error deleting workflow state with ID: {Id}", id);
            throw;
        }
    }

    public async Task<TransitionRule> GetTransitionRuleByIdAsync(Guid id)
    {
        _logger.LogInformation("GetTransitionRuleByIdAsync: Retrieving transition rule with ID: {Id}", id);
        
        try
        {
            var rule = await _transitionRuleRepository.GetById(id);
            if (rule != null)
            {
                _logger.LogInformation("GetTransitionRuleByIdAsync: Found transition rule - FromStateId: {FromStateId}, ToStateId: {ToStateId}", 
                    rule.FromStateId, rule.ToStateId);
            }
            else
            {
                _logger.LogInformation("GetTransitionRuleByIdAsync: No transition rule found with ID: {Id}", id);
            }
            
            return rule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTransitionRuleByIdAsync: Error retrieving transition rule with ID: {Id}", id);
            throw;
        }
    }

    public async Task DeleteTransitionRuleAsync(Guid id)
    {
        _logger.LogInformation("DeleteTransitionRuleAsync: Deleting transition rule with ID: {Id}", id);
        
        try
        {
            await _transitionRuleRepository.Delete(id);
            _logger.LogInformation("DeleteTransitionRuleAsync: Successfully deleted transition rule with ID: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteTransitionRuleAsync: Error deleting transition rule with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> TestDatabaseConnectionAsync()
    {
        _logger.LogInformation("TestDatabaseConnectionAsync: Starting database connection test");

        try
        {
            // Try to get count of workflow definitions (simple database operation)
            var count = await _workflowDefinitionRepository.CountAsync();
            _logger.LogInformation("TestDatabaseConnectionAsync: Successfully retrieved count: {Count}", count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TestDatabaseConnectionAsync: Database connection test failed");
            return false;
        }
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsWithStatsAsync()
    {
        _logger.LogInformation("GetAllWorkflowDefinitionsWithStatsAsync: Starting retrieval of all workflow definitions with statistics");

        try
        {
            var definitions = await _workflowDefinitionRepository.GetAll();
            var definitionsList = definitions?.ToList() ?? new List<WorkflowDefinition>();
            
            _logger.LogInformation("GetAllWorkflowDefinitionsWithStatsAsync: Retrieved {Count} workflow definitions", definitionsList.Count);
            
            // Get states and instances for each workflow
            foreach (var definition in definitionsList)
            {
                try
                {
                    // Load states
                    var states = await _workflowStateRepository.GetByWorkflow(definition.Id);
                    definition.States = states?.ToList() ?? new List<WorkflowState>();
                    
                    // Load instances for counting
                    var instances = await _workflowInstanceRepository.GetByWorkflow(definition.Id);
                    definition.Instances = instances?.ToList() ?? new List<WorkflowInstance>();
                    
                    _logger.LogDebug("GetAllWorkflowDefinitionsWithStatsAsync: Workflow {Id} ({Name}) - States: {StateCount}, Instances: {InstanceCount}", 
                        definition.Id, definition.Name, definition.States.Count, definition.Instances.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "GetAllWorkflowDefinitionsWithStatsAsync: Error loading stats for workflow {Id} ({Name})", 
                        definition.Id, definition.Name);
                    
                    // Ensure collections are initialized even if loading fails
                    definition.States = definition.States ?? new List<WorkflowState>();
                    definition.Instances = definition.Instances ?? new List<WorkflowInstance>();
                }
            }

            return definitionsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllWorkflowDefinitionsWithStatsAsync: Error retrieving workflow definitions with statistics");
            throw;
        }
    }

    public async Task<IEnumerable<Piranha.AspNetCore.Identity.Data.Role>> GetSystemRolesAsync()
    {
        _logger.LogInformation("GetSystemRolesAsync: Starting retrieval of system roles");

        try
        {
            var roles = await _roleManager.Roles.ToListAsync();
            _logger.LogInformation("GetSystemRolesAsync: Retrieved {Count} roles", roles.Count);
            
            foreach (var role in roles)
            {
                _logger.LogDebug("GetSystemRolesAsync: Found role - Name: {Name}, NormalizedName: {NormalizedName}", 
                    role.Name, role.NormalizedName);
            }
            
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSystemRolesAsync: Error retrieving system roles");
            throw;
        }
    }
} 