#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Piranha.AspNetCore.Identity.Data;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Repositories;
using Piranha.Telemetry;
using System.Diagnostics;
using System.Diagnostics.Metrics;

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

    // NEW METHOD - Creates workflow instance with content extension
    public async Task<WorkflowInstance> CreateWorkflowInstanceWithContentAsync(string contentId, Guid workflowDefinitionId, string contentType = null, string contentTitle = null)
    {
        _logger.LogInformation("CreateWorkflowInstanceWithContentAsync: Creating workflow instance for ContentId: {ContentId}, WorkflowDefinitionId: {WorkflowDefinitionId}", contentId, workflowDefinitionId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContentAsync: ContentId is null or empty");
                throw new ArgumentException("ContentId cannot be null or empty", nameof(contentId));
            }

            if (workflowDefinitionId == Guid.Empty)
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContentAsync: WorkflowDefinitionId is empty");
                throw new ArgumentException("WorkflowDefinitionId cannot be empty", nameof(workflowDefinitionId));
            }

            // Get the workflow definition
            var workflowDefinition = await _workflowDefinitionRepository.GetById(workflowDefinitionId);
            if (workflowDefinition == null)
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContentAsync: Workflow definition not found with ID: {WorkflowDefinitionId}", workflowDefinitionId);
                throw new InvalidOperationException($"Workflow definition not found with ID: {workflowDefinitionId}");
            }

            // Get the workflow states to find the initial state
            var states = await _workflowStateRepository.GetByWorkflow(workflowDefinitionId);
            var initialState = states.FirstOrDefault(s => s.IsInitial);
            if (initialState == null)
            {
                _logger.LogWarning("CreateWorkflowInstanceWithContentAsync: No initial state found for workflow: {WorkflowDefinitionId}", workflowDefinitionId);
                throw new InvalidOperationException($"No initial state found for workflow: {workflowDefinitionId}");
            }

            // Get current user
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            string userId = user?.Id.ToString() ?? "system";

            // Create workflow instance
            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid(),
                ContentId = contentId,
                ContentType = contentType ?? "Unknown",
                ContentTitle = contentTitle ?? "Unknown",
                WorkflowDefinitionId = workflowDefinitionId,
                CurrentStateId = initialState.Id,
                Status = WorkflowInstanceStatus.Active,
                CreatedBy = userId,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            // Save workflow instance
            await _workflowInstanceRepository.Save(workflowInstance);
            _logger.LogInformation("CreateWorkflowInstanceWithContentAsync: Created workflow instance with ID: {WorkflowInstanceId}", workflowInstance.Id);

            // Create workflow content extension (should be new since we delete existing ones first)
            var workflowContentExtension = new WorkflowContentExtension
            {
                Id = Guid.NewGuid(),
                ContentId = contentId,
                CurrentWorkflowInstanceId = workflowInstance.Id
            };

            await _contentExtensionRepository.Save(workflowContentExtension);
            _logger.LogInformation("CreateWorkflowInstanceWithContentAsync: Created workflow content extension for ContentId: {ContentId}", contentId);

            _logger.LogInformation("CreateWorkflowInstanceWithContentAsync: Successfully created workflow instance and content extension for ContentId: {ContentId}", contentId);
            return workflowInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowInstanceWithContentAsync: Error creating workflow instance for ContentId: {ContentId}, WorkflowDefinitionId: {WorkflowDefinitionId}", contentId, workflowDefinitionId);
            throw;
        }
    }

    // NEW METHOD - Get WorkflowContentExtensions by WorkflowDefinitionId
    public async Task<IEnumerable<WorkflowContentExtension>> GetWorkflowContentExtensionsByDefinitionAsync(Guid workflowDefinitionId)
    {
        _logger.LogInformation("GetWorkflowContentExtensionsByDefinitionAsync: Retrieving content extensions for WorkflowDefinitionId: {WorkflowDefinitionId}", workflowDefinitionId);
        
        try
        {
            if (workflowDefinitionId == Guid.Empty)
            {
                _logger.LogWarning("GetWorkflowContentExtensionsByDefinitionAsync: WorkflowDefinitionId is empty");
                return new List<WorkflowContentExtension>();
            }

            // Get all workflow instances for this definition
            var workflowInstances = await _workflowInstanceRepository.GetByWorkflow(workflowDefinitionId);
            var instanceIds = workflowInstances.Select(wi => wi.Id).ToList();

            // Get all active workflow content extensions
            var allExtensions = await _contentExtensionRepository.GetActiveWorkflows();
            
            // Filter by workflow definition
            var filteredExtensions = allExtensions.Where(ext => 
                ext.CurrentWorkflowInstanceId.HasValue && 
                instanceIds.Contains(ext.CurrentWorkflowInstanceId.Value)).ToList();

            _logger.LogInformation("GetWorkflowContentExtensionsByDefinitionAsync: Found {Count} content extensions for WorkflowDefinitionId: {WorkflowDefinitionId}", 
                filteredExtensions.Count, workflowDefinitionId);
            
            return filteredExtensions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowContentExtensionsByDefinitionAsync: Error retrieving content extensions for WorkflowDefinitionId: {WorkflowDefinitionId}", workflowDefinitionId);
            throw;
        }
    }

    // NEW METHOD - Partial update for WorkflowInstance
    public async Task<WorkflowInstance> PartialUpdateWorkflowInstanceAsync(Guid workflowInstanceId, WorkflowInstance partialUpdate)
    {
        _logger.LogInformation("PartialUpdateWorkflowInstanceAsync: Starting partial update for WorkflowInstanceId: {WorkflowInstanceId}", workflowInstanceId);
        
        try
        {
            if (workflowInstanceId == Guid.Empty)
            {
                _logger.LogWarning("PartialUpdateWorkflowInstanceAsync: WorkflowInstanceId is empty");
                throw new ArgumentException("WorkflowInstanceId cannot be empty", nameof(workflowInstanceId));
            }

            // Get the existing workflow instance
            var existingInstance = await _workflowInstanceRepository.GetById(workflowInstanceId);
            if (existingInstance == null)
            {
                _logger.LogWarning("PartialUpdateWorkflowInstanceAsync: WorkflowInstance not found with ID: {WorkflowInstanceId}", workflowInstanceId);
                throw new InvalidOperationException($"WorkflowInstance not found with ID: {workflowInstanceId}");
            }

            // Only update fields that are provided (not null/empty)
            if (!string.IsNullOrWhiteSpace(partialUpdate?.ContentId))
            {
                existingInstance.ContentId = partialUpdate.ContentId;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated ContentId to {ContentId}", partialUpdate.ContentId);
            }

            if (!string.IsNullOrWhiteSpace(partialUpdate?.ContentType))
            {
                existingInstance.ContentType = partialUpdate.ContentType;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated ContentType to {ContentType}", partialUpdate.ContentType);
            }

            if (!string.IsNullOrWhiteSpace(partialUpdate?.ContentTitle))
            {
                existingInstance.ContentTitle = partialUpdate.ContentTitle;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated ContentTitle to {ContentTitle}", partialUpdate.ContentTitle);
            }

            if (partialUpdate?.WorkflowDefinitionId != null && partialUpdate.WorkflowDefinitionId != Guid.Empty)
            {
                existingInstance.WorkflowDefinitionId = partialUpdate.WorkflowDefinitionId;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated WorkflowDefinitionId to {WorkflowDefinitionId}", partialUpdate.WorkflowDefinitionId);
            }

            if (partialUpdate?.CurrentStateId != null && partialUpdate.CurrentStateId != Guid.Empty)
            {
                existingInstance.CurrentStateId = partialUpdate.CurrentStateId;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated CurrentStateId to {CurrentStateId}", partialUpdate.CurrentStateId);
            }

            if (partialUpdate?.Status != null)
            {
                existingInstance.Status = partialUpdate.Status;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated Status to {Status}", partialUpdate.Status);
            }

            if (!string.IsNullOrWhiteSpace(partialUpdate?.Metadata))
            {
                existingInstance.Metadata = partialUpdate.Metadata;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated Metadata");
            }

            if (partialUpdate?.CompletedAt != null)
            {
                existingInstance.CompletedAt = partialUpdate.CompletedAt;
                _logger.LogDebug("PartialUpdateWorkflowInstanceAsync: Updated CompletedAt to {CompletedAt}", partialUpdate.CompletedAt);
            }

            // Always update LastModified
            existingInstance.LastModified = DateTime.UtcNow;

            // Save the updated instance
            await _workflowInstanceRepository.Save(existingInstance);
            
            _logger.LogInformation("PartialUpdateWorkflowInstanceAsync: Successfully updated WorkflowInstance {WorkflowInstanceId}", workflowInstanceId);
            return existingInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PartialUpdateWorkflowInstanceAsync: Error updating WorkflowInstance {WorkflowInstanceId}", workflowInstanceId);
            throw;
        }
    }

    // NEW METHOD - Get all workflow instances
    public async Task<IEnumerable<WorkflowInstance>> GetAllWorkflowInstancesAsync()
    {
        _logger.LogInformation("GetAllWorkflowInstancesAsync: Retrieving all workflow instances");
        
        try
        {
            var instances = await _workflowInstanceRepository.GetAll();
            _logger.LogInformation("GetAllWorkflowInstancesAsync: Retrieved {Count} workflow instances", instances?.Count() ?? 0);
            
            return instances ?? new List<WorkflowInstance>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllWorkflowInstancesAsync: Error retrieving all workflow instances");
            throw;
        }
    }

    // Workflow Content Extensions
    public async Task<bool> WorkflowContentExtensionExistsAsync(string contentId)
    {
        _logger.LogInformation("WorkflowContentExtensionExistsAsync: Checking if workflow content extension exists for ContentId: {ContentId}", contentId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                _logger.LogWarning("WorkflowContentExtensionExistsAsync: ContentId is null or empty");
                return false;
            }

            var exists = await _contentExtensionRepository.Exists(contentId);
            _logger.LogInformation("WorkflowContentExtensionExistsAsync: ContentId {ContentId} exists: {Exists}", contentId, exists);
            
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WorkflowContentExtensionExistsAsync: Error checking if workflow content extension exists for ContentId: {ContentId}", contentId);
            throw;
        }
    }

    public async Task<Piranha.EditorialWorkflow.Models.WorkflowContentExtension> GetWorkflowContentExtensionAsync(string contentId)
    {
        _logger.LogInformation("GetWorkflowContentExtensionAsync: Retrieving workflow content extension for ContentId: {ContentId}", contentId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                _logger.LogWarning("GetWorkflowContentExtensionAsync: ContentId is null or empty");
                return null;
            }

            var extension = await _contentExtensionRepository.GetByContentId(contentId);
            
            if (extension != null)
            {
                _logger.LogInformation("GetWorkflowContentExtensionAsync: Found workflow content extension - Id: {Id}, ContentId: {ContentId}, CurrentWorkflowInstanceId: {CurrentWorkflowInstanceId}", 
                    extension.Id, extension.ContentId, extension.CurrentWorkflowInstanceId);
            }
            else
            {
                _logger.LogInformation("GetWorkflowContentExtensionAsync: No workflow content extension found for ContentId: {ContentId}", contentId);
            }
            
            return extension;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetWorkflowContentExtensionAsync: Error retrieving workflow content extension for ContentId: {ContentId}", contentId);
            throw;
        }
    }

    // NEW METHOD - Delete WorkflowInstance
    public async Task DeleteWorkflowInstanceAsync(Guid workflowInstanceId)
    {
        _logger.LogInformation("DeleteWorkflowInstanceAsync: Deleting workflow instance with ID: {WorkflowInstanceId}", workflowInstanceId);
        
        try
        {
            if (workflowInstanceId == Guid.Empty)
            {
                _logger.LogWarning("DeleteWorkflowInstanceAsync: WorkflowInstanceId is empty");
                throw new ArgumentException("WorkflowInstanceId cannot be empty", nameof(workflowInstanceId));
            }

            await _workflowInstanceRepository.Delete(workflowInstanceId);
            _logger.LogInformation("DeleteWorkflowInstanceAsync: Successfully deleted workflow instance {WorkflowInstanceId}", workflowInstanceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowInstanceAsync: Error deleting workflow instance {WorkflowInstanceId}", workflowInstanceId);
            throw;
        }
    }

    // NEW METHOD - Delete WorkflowContentExtension
    public async Task DeleteWorkflowContentExtensionAsync(string contentId)
    {
        _logger.LogInformation("DeleteWorkflowContentExtensionAsync: Deleting workflow content extension for ContentId: {ContentId}", contentId);
        
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
            {
                _logger.LogWarning("DeleteWorkflowContentExtensionAsync: ContentId is null or empty");
                throw new ArgumentException("ContentId cannot be null or empty", nameof(contentId));
            }

            await _contentExtensionRepository.Delete(contentId);
            _logger.LogInformation("DeleteWorkflowContentExtensionAsync: Successfully deleted workflow content extension for ContentId: {ContentId}", contentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowContentExtensionAsync: Error deleting workflow content extension for ContentId: {ContentId}", contentId);
            throw;
        }
    }

    public async Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition definition)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.WorkflowOperation, "CreateWorkflowDefinition");
        activity.DisplayName = $"WorkflowService: Create Definition '{definition?.Name}'";
        
        _logger.LogInformation("CreateWorkflowDefinitionAsync: Starting creation of workflow definition. Name: {Name}, IsActive: {IsActive}", 
            definition?.Name, definition?.IsActive);

        try
        {
            activity?.SetTag(PiranhaTelemetry.AttributeNames.OperationType, "workflow.create");
            activity?.SetTag("workflow.name", definition?.Name);
            activity?.SetTag("workflow.is_active", definition?.IsActive);
            
            if (definition == null)
            {
                _logger.LogError("CreateWorkflowDefinitionAsync: Definition is null");
                throw new ArgumentNullException(nameof(definition));
            }

            // If setting this workflow as active, ensure no other workflow is active
            if (definition.IsActive)
            {
                await EnsureOnlyOneActiveWorkflowAsync(definition.Id);
                _logger.LogInformation("CreateWorkflowDefinitionAsync: Setting workflow as active, deactivated other workflows");
            }

            // Get current user or set default
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            string userId = user?.Id.ToString() ?? "system";
            
            // Set audit fields
            definition.CreatedBy = definition.CreatedBy ?? userId;
            definition.LastModifiedBy = definition.LastModifiedBy ?? userId;
            definition.Created = definition.Created == default ? DateTime.UtcNow : definition.Created;
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

            activity?.SetTag("workflow.id", definition.Id.ToString());
            activity?.SetOperationStatus(true, "Workflow definition created successfully");
            
            return definition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowDefinitionAsync: Error creating workflow definition. Name: {Name}", definition?.Name);
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(WorkflowDefinition definition)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.WorkflowOperation, "UpdateWorkflowDefinition");
        
        _logger.LogInformation("UpdateWorkflowDefinitionAsync: Starting update of workflow definition. Name: {Name}, IsActive: {IsActive}", 
            definition?.Name, definition?.IsActive);

        try
        {
            activity?.SetTag(PiranhaTelemetry.AttributeNames.OperationType, "workflow.update");
            activity?.SetTag("workflow.id", definition?.Id.ToString());
            activity?.SetTag("workflow.name", definition?.Name);
            activity?.SetTag("workflow.is_active", definition?.IsActive);
            
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

            activity?.SetOperationStatus(true, "Workflow definition updated successfully");
            
            return definition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateWorkflowDefinitionAsync: Error updating workflow definition. Name: {Name}", definition?.Name);
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task DeleteWorkflowDefinitionAsync(Guid id)
    {
        _logger.LogInformation("DeleteWorkflowDefinitionAsync: Deleting workflow definition with ID: {Id}", id);
        
        try
        {
            await _workflowDefinitionRepository.Delete(id);
            _logger.LogInformation("DeleteWorkflowDefinitionAsync: Successfully deleted workflow definition with ID: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteWorkflowDefinitionAsync: Error deleting workflow definition with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> CanDeleteWorkflowDefinitionAsync(Guid id)
    {
        var instances = await _workflowInstanceRepository.GetByWorkflow(id);
        return instances?.Any() != true;
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync()
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.WorkflowOperation, "GetAllWorkflowDefinitions");
        activity?.SetTag(PiranhaTelemetry.AttributeNames.OperationType, "workflow.list");
        
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

            activity?.SetTag("workflow.definitions.count", definitionsList.Count);
            activity?.SetOperationStatus(true, $"Retrieved {definitionsList.Count} workflow definitions");
            
            return definitionsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllWorkflowDefinitionsAsync: Error retrieving workflow definitions");
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task<WorkflowDefinition> GetWorkflowDefinitionByIdAsync(Guid id)
    {
        return await _workflowDefinitionRepository.GetById(id);
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

    public async Task<WorkflowState> CreateWorkflowStateAsync(WorkflowState state)
    {
        _logger.LogInformation("CreateWorkflowStateAsync: Starting creation of workflow state. StateId: {StateId}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
            state?.StateId, state?.WorkflowDefinitionId);

        try
        {
            // Validate that the workflow definition exists
            var workflowDefinition = await _workflowDefinitionRepository.GetById(state.WorkflowDefinitionId);
            if (workflowDefinition == null)
            {
                _logger.LogError("CreateWorkflowStateAsync: Workflow definition not found with ID: {WorkflowDefinitionId}", 
                    state.WorkflowDefinitionId);
                throw new InvalidOperationException($"Workflow definition with ID {state.WorkflowDefinitionId} does not exist. Please create the workflow definition first.");
            }

            _logger.LogInformation("CreateWorkflowStateAsync: Found workflow definition - Name: {Name}, IsActive: {IsActive}", 
                workflowDefinition.Name, workflowDefinition.IsActive);

            // Check if a state with the same StateId already exists in this workflow
            var existingStates = await _workflowStateRepository.GetByWorkflow(state.WorkflowDefinitionId);
            var duplicateState = existingStates?.FirstOrDefault(s => s.StateId == state.StateId);
            if (duplicateState != null)
            {
                _logger.LogError("CreateWorkflowStateAsync: State with StateId '{StateId}' already exists in workflow {WorkflowDefinitionId}", 
                    state.StateId, state.WorkflowDefinitionId);
                throw new InvalidOperationException($"A state with StateId '{state.StateId}' already exists in this workflow.");
            }

            // Set creation timestamp
            state.Created = DateTime.UtcNow;

            await _workflowStateRepository.Save(state);
            
            _logger.LogInformation("CreateWorkflowStateAsync: Successfully created workflow state. ID: {Id}, StateId: {StateId}", 
                state.Id, state.StateId);
            
            return state;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowStateAsync: Error creating workflow state. StateId: {StateId}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
                state?.StateId, state?.WorkflowDefinitionId);
            throw;
        }
    }

    /// <summary>
    /// Creates a default workflow definition if none exists, then creates the workflow state
    /// </summary>
    public async Task<WorkflowState> CreateWorkflowStateWithValidationAsync(WorkflowState state)
    {
        _logger.LogInformation("CreateWorkflowStateWithValidationAsync: Starting creation with auto-workflow creation if needed. StateId: {StateId}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
            state?.StateId, state?.WorkflowDefinitionId);

        try
        {
            // Check if workflow definition exists
            var workflowDefinition = await _workflowDefinitionRepository.GetById(state.WorkflowDefinitionId);
            if (workflowDefinition == null)
            {
                _logger.LogWarning("CreateWorkflowStateWithValidationAsync: Workflow definition {WorkflowDefinitionId} not found, creating default workflow", 
                    state.WorkflowDefinitionId);
                
                // Create a default workflow definition
                var defaultWorkflow = new WorkflowDefinition
                {
                    Id = state.WorkflowDefinitionId,
                    Name = "Default Editorial Workflow",
                    Description = "Auto-created workflow for editorial process",
                    IsActive = true,
                    Version = 1,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    States = new List<WorkflowState>(),
                    Instances = new List<WorkflowInstance>()
                };

                await _workflowDefinitionRepository.Save(defaultWorkflow);
                
                _logger.LogInformation("CreateWorkflowStateWithValidationAsync: Created default workflow definition with ID: {WorkflowDefinitionId}", 
                    state.WorkflowDefinitionId);
            }

            // Now create the state using the existing method
            return await CreateWorkflowStateAsync(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateWorkflowStateWithValidationAsync: Error creating workflow state with validation. StateId: {StateId}, WorkflowDefinitionId: {WorkflowDefinitionId}", 
                state?.StateId, state?.WorkflowDefinitionId);
            throw;
        }
    }

    public async Task<WorkflowState> UpdateWorkflowStateAsync(WorkflowState state)
    {
        await _workflowStateRepository.Save(state);
        return state;
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

    public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.WorkflowOperation, "CreateWorkflowInstance");
        activity.DisplayName = "WorkflowService: Create Instance";
        activity?.SetTag(PiranhaTelemetry.AttributeNames.OperationType, "workflow.instance.create");
        activity?.SetTag("workflow.definition_id", instance?.WorkflowDefinitionId.ToString());
        
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
        
        // Record metrics for instance creation
        var tags = new KeyValuePair<string, object?>[] {
            new("workflow_id", instance.WorkflowDefinitionId.ToString()),
            new("content_type", "content"), // Default content type
            new("initial_state", initialState.StateId)
        };
        WorkflowMetricsProvider.InstancesCreated.Add(1, tags);
        
        activity?.SetTag("workflow.instance.id", instance.Id.ToString());
        activity?.SetTag(PiranhaTelemetry.AttributeNames.WorkflowState, initialState.StateId);
        activity?.SetTag(PiranhaTelemetry.AttributeNames.UserId, PiranhaTelemetry.MaskSensitiveData(user?.Id.ToString(), SensitiveDataType.UserId));
        activity?.SetOperationStatus(true, "Workflow instance created successfully");
        
        return instance;
    }

    public async Task<WorkflowInstance> UpdateWorkflowInstanceAsync(WorkflowInstance instance)
    {
        await _workflowInstanceRepository.Save(instance);
        return instance;
    }

    public async Task<bool> TransitionWorkflowAsync(Guid instanceId, string targetStateId)
    {
        using var activity = PiranhaTelemetry.StartActivity(PiranhaTelemetry.ActivityNames.WorkflowOperation, "TransitionWorkflow");
        activity.DisplayName = $"WorkflowService: Transition to {targetStateId}";
        activity?.SetTag(PiranhaTelemetry.AttributeNames.OperationType, "workflow.transition");
        activity?.SetTag("workflow.instance_id", instanceId.ToString());
        activity?.SetTag("workflow.target_state", targetStateId);
        
        var stopwatch = Stopwatch.StartNew();
        var instance = await _workflowInstanceRepository.GetById(instanceId);
        if (instance == null)
        {
            activity?.SetOperationStatus(false, "Workflow instance not found");
            return false;
        }

        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        var roles = await _userManager.GetRolesAsync(user);

        // Buscar o estado alvo
        var workflow = await _workflowDefinitionRepository.GetWithStates(instance.WorkflowDefinitionId);
        var targetState = workflow.States.FirstOrDefault(s => s.StateId == targetStateId);
        if (targetState == null)
        {
            // Record failed transition
            RecordTransitionMetrics(instance.WorkflowDefinitionId.ToString(), "unknown", targetStateId, 
                "content", roles?.FirstOrDefault(), false, stopwatch.ElapsedMilliseconds);
            return false;
        }

        // Get current state for metrics
        var currentState = workflow.States.FirstOrDefault(s => s.Id == instance.CurrentStateId);
        var fromStateId = currentState?.StateId ?? "unknown";

        // Verifica se existe uma regra de transição válida
        var transitionRule = await _transitionRuleRepository.GetTransition(instance.CurrentStateId, targetState.Id);
        if (transitionRule == null)
        {
            RecordTransitionMetrics(instance.WorkflowDefinitionId.ToString(), fromStateId, targetStateId, 
                "content", roles?.FirstOrDefault(), false, stopwatch.ElapsedMilliseconds);
            return false;
        }

        // Verifica se o usuário tem permissão para fazer a transição
        if (!roles.Any(r => transitionRule.AllowedRoles.Contains(r)))
        {
            activity?.SetTag("workflow.transition.denied", "User lacks required role");
            activity?.SetOperationStatus(false, "User lacks permission for transition");
            RecordTransitionMetrics(instance.WorkflowDefinitionId.ToString(), fromStateId, targetStateId, 
                "content", roles?.FirstOrDefault(), false, stopwatch.ElapsedMilliseconds);
            return false;
        }

        // Atualiza o estado
        var previousStateId = instance.CurrentStateId;
        instance.CurrentStateId = targetState.Id;
        instance.LastModified = DateTime.UtcNow;
        await _workflowInstanceRepository.Save(instance);
        
        stopwatch.Stop();
        
        // Record successful transition metrics
        RecordTransitionMetrics(instance.WorkflowDefinitionId.ToString(), fromStateId, targetStateId, 
            "content", roles?.FirstOrDefault(), true, stopwatch.ElapsedMilliseconds);
        
        activity?.SetTag(PiranhaTelemetry.AttributeNames.WorkflowState, targetState.StateId);
        activity?.SetTag(PiranhaTelemetry.AttributeNames.WorkflowTransition, $"{previousStateId} -> {targetState.Id}");
        activity?.SetTag(PiranhaTelemetry.AttributeNames.UserId, PiranhaTelemetry.MaskSensitiveData(user?.Id.ToString(), SensitiveDataType.UserId));
        activity?.SetOperationStatus(true, "Workflow transition completed successfully");
        
        return true;
    }

    public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByUserAsync()
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        return await _workflowInstanceRepository.GetByUser(user.Id.ToString());
    }

    public Task<IEnumerable<WorkflowInstance>> GetWorkflowInstancesByStateAsync(string state)
    {
        return Task.FromResult<IEnumerable<WorkflowInstance>>(new List<WorkflowInstance>()); // Simplified for now
    }

    public async Task<WorkflowInstance> GetWorkflowInstanceByIdAsync(Guid id)
    {
        return await _workflowInstanceRepository.GetById(id);
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

    /// <summary>
    /// Ensures only one workflow is active at a time
    /// </summary>
    private async Task EnsureOnlyOneActiveWorkflowAsync(Guid currentWorkflowId)
    {
        var allWorkflows = await _workflowDefinitionRepository.GetAll();
        foreach (var workflow in allWorkflows.Where(w => w.Id != currentWorkflowId && w.IsActive))
        {
            workflow.IsActive = false;
            await _workflowDefinitionRepository.Save(workflow);
        }
    }
    
    /// <summary>
    /// Helper method to record workflow transition metrics
    /// </summary>
    private static void RecordTransitionMetrics(string workflowId, string fromState, string toState, 
        string contentType, string? userRole, bool success, double durationMs)
    {
        var tags = new KeyValuePair<string, object?>[]
        {
            new("workflow_id", workflowId),
            new("from_state", fromState),
            new("to_state", toState),
            new("content_type", contentType),
            new("success", success.ToString())
        };

        if (!string.IsNullOrEmpty(userRole))
        {
            var extendedTags = tags.Concat(new[] { new KeyValuePair<string, object?>("user_role", userRole) }).ToArray();
            tags = extendedTags;
        }

        // Record transition count
        WorkflowMetricsProvider.TransitionCount.Add(1, tags);

        // Record failure if applicable
        if (!success)
        {
            WorkflowMetricsProvider.TransitionFailureCount.Add(1, tags);
        }

        // Record duration
        WorkflowMetricsProvider.TransitionDuration.Record(durationMs, tags);

        // Record transition by role
        if (!string.IsNullOrEmpty(userRole))
        {
            var roleTags = new KeyValuePair<string, object?>[] {
                new("role", userRole),
                new("workflow_id", workflowId)
            };
            WorkflowMetricsProvider.TransitionsByRole.Add(1, roleTags);
        }

        // Check for specific state transitions
        if (toState.ToLower() == "published" && success)
        {
            WorkflowMetricsProvider.ContentPublished.Add(1, new KeyValuePair<string, object?>[] {
                new("workflow_id", workflowId),
                new("content_type", contentType)
            });
        }
        else if (toState.ToLower() == "rejected" && success)
        {
            WorkflowMetricsProvider.ContentRejected.Add(1, new KeyValuePair<string, object?>[] {
                new("workflow_id", workflowId),
                new("content_type", contentType),
                new("from_state", fromState)
            });
        }
    }
}