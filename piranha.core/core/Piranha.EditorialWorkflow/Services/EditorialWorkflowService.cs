using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
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

    public EditorialWorkflowService(
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IWorkflowStateRepository workflowStateRepository,
        IWorkflowInstanceRepository workflowInstanceRepository,
        ITransitionRuleRepository transitionRuleRepository,
        IWorkflowContentExtensionRepository contentExtensionRepository,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _workflowStateRepository = workflowStateRepository;
        _workflowInstanceRepository = workflowInstanceRepository;
        _transitionRuleRepository = transitionRuleRepository;
        _contentExtensionRepository = contentExtensionRepository;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition definition)
    {
        await _workflowDefinitionRepository.Save(definition);
        return definition;
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
        return await _workflowDefinitionRepository.GetAll();
    }

    public async Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id)
    {
        return await _workflowDefinitionRepository.GetById(id);
    }

    public async Task<IEnumerable<WorkflowState>> GetWorkflowStatesByDefinitionAsync(Guid definitionId)
    {
        return await _workflowStateRepository.GetByWorkflow(definitionId);
    }

    public async Task<IEnumerable<TransitionRule>> GetTransitionRulesByDefinitionAsync(Guid definitionId)
    {
        // Buscar todos os estados do workflow
        var states = await _workflowStateRepository.GetByWorkflow(definitionId);
        var rules = new List<TransitionRule>();
        foreach (var state in states)
        {
            var transitions = await _transitionRuleRepository.GetByFromState(state.Id);
            rules.AddRange(transitions);
        }
        return rules;
    }
} 