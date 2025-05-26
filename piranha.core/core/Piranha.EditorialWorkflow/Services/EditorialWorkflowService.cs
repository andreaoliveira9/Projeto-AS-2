using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<EditorialWorkflowService> _logger;

    public EditorialWorkflowService(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IWorkflowStateRepository workflowStateRepository,
        IWorkflowInstanceRepository workflowInstanceRepository,
        ITransitionRuleRepository transitionRuleRepository,
        IWorkflowContentExtensionRepository contentExtensionRepository,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        ILogger<EditorialWorkflowService> logger)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _workflowStateRepository = workflowStateRepository;
        _workflowInstanceRepository = workflowInstanceRepository;
        _transitionRuleRepository = transitionRuleRepository;
        _contentExtensionRepository = contentExtensionRepository;
        _userManager = userManager;
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
            string userId = "system"; // fallback value
            
            if (user != null)
            {
                userId = user.Id.ToString();
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

            // Set audit fields
            definition.CreatedBy = userId;
            definition.LastModifiedBy = userId;
            definition.Created = DateTime.UtcNow;
            definition.LastModified = DateTime.UtcNow;

            _logger.LogDebug("CreateWorkflowDefinitionAsync: Before repository save - ID: {Id}, Name: {Name}, CreatedBy: {CreatedBy}", 
                definition.Id, definition.Name, definition.CreatedBy);

            await _workflowDefinitionRepository.Save(definition);
            
            _logger.LogInformation("CreateWorkflowDefinitionAsync: Successfully saved workflow definition. ID: {Id}, Name: {Name}", 
                definition.Id, definition.Name);

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
        await _workflowDefinitionRepository.Save(definition);
        return definition;
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
} 