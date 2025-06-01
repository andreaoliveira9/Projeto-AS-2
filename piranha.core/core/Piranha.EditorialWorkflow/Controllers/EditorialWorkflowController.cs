using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piranha.AspNetCore.Identity.Data;
using Piranha.EditorialWorkflow.Models;
using Piranha.EditorialWorkflow.Services;
using Piranha.Audit.Events;

namespace Piranha.EditorialWorkflow.Controllers;

[ApiController]
[Route("api/workflow")]
//[Authorize]
public class EditorialWorkflowController : ControllerBase
{
    private readonly IEditorialWorkflowService _workflowService;
    private readonly ILogger<EditorialWorkflowController> _logger;
    private readonly IApi _api;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly IWorkflowMessagePublisher _messagePublisher;

    public EditorialWorkflowController(
        IEditorialWorkflowService workflowService,
        ILogger<EditorialWorkflowController> logger,
        IApi api,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IWorkflowMessagePublisher messagePublisher)
    {
        _workflowService = workflowService;
        _logger = logger;
        _api = api;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _messagePublisher = messagePublisher;
    }

    #region Request Models

    public class CreateWorkflowInstanceWithContentRequest
    {
        public string ContentId { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string ContentType { get; set; }
        public string ContentTitle { get; set; }
    }

    public class WorkflowTransitionRequest
    {
        public Guid TransitionRuleId { get; set; }
        public string Comment { get; set; }
    }

    public class WorkflowInstanceTransitionsResponse
    {
        public Guid WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; }
        public WorkflowState CurrentState { get; set; }
        public IList<TransitionRule> AvailableTransitions { get; set; }
    }

    #endregion

    #region Workflow Definitions

    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions()
    {
        var definitions = await _workflowService.GetAllWorkflowDefinitionsAsync();
        return Ok(definitions);
    }

    [HttpGet("definitions/with-stats")]
    public async Task<ActionResult<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitionsWithStats()
    {
        var definitions = await _workflowService.GetAllWorkflowDefinitionsWithStatsAsync();
        return Ok(definitions);
    }

    [HttpGet("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> GetWorkflowDefinition(Guid id)
    {
        var definition = await _workflowService.GetWorkflowDefinitionByIdAsync(id);
        if (definition == null)
            return NotFound();

        return Ok(definition);
    }

    [HttpPost("definitions")]
    public async Task<ActionResult<WorkflowDefinition>> CreateWorkflowDefinition(WorkflowDefinition definition)
    {
        var result = await _workflowService.CreateWorkflowDefinitionAsync(definition);
        return CreatedAtAction(nameof(GetWorkflowDefinition), new { id = result.Id }, result);
    }

    [HttpPut("definitions/{id}")]
    public async Task<ActionResult<WorkflowDefinition>> UpdateWorkflowDefinition(Guid id, WorkflowDefinition definition)
    {
        if (id != definition.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowDefinitionAsync(definition);
        return Ok(result);
    }

    [HttpDelete("definitions/{id}")]
    public async Task<ActionResult> DeleteWorkflowDefinition(Guid id)
    {
        var canDelete = await _workflowService.CanDeleteWorkflowDefinitionAsync(id);
        if (!canDelete)
            return BadRequest("Cannot delete workflow definition that has active instances");

        await _workflowService.DeleteWorkflowDefinitionAsync(id);
        return NoContent();
    }

    #endregion

    #region Workflow Instances

    [HttpGet("instances")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetWorkflowInstances()
    {
        var instances = await _workflowService.GetWorkflowInstancesByUserAsync();
        return Ok(instances);
    }

    [HttpDelete("content-extensions/{contentId}")]
    public async Task<ActionResult> DeleteWorkflowContentExtension(string contentId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest("ContentId is required");

            // Check if the workflow content extension exists
            var extension = await _workflowService.GetWorkflowContentExtensionAsync(contentId);
            if (extension == null)
                return NotFound("Workflow content extension not found");

            // If there's an active workflow instance, inform the caller
            WorkflowInstance associatedInstance = null;
            if (extension.CurrentWorkflowInstanceId.HasValue)
            {
                associatedInstance = await _workflowService.GetWorkflowInstanceByIdAsync(extension.CurrentWorkflowInstanceId.Value);
            }

            // Delete the workflow content extension
            await _workflowService.DeleteWorkflowContentExtensionAsync(contentId);

            return Ok(new
            {
                success = true,
                message = "Workflow content extension deleted successfully",
                contentId = contentId,
                hadAssociatedInstance = associatedInstance != null,
                associatedInstanceId = associatedInstance?.Id,
                warning = associatedInstance != null ? "Note: Associated WorkflowInstance still exists and may need to be deleted separately" : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow content extension for ContentId {ContentId}", contentId);
            return StatusCode(500, "An error occurred while deleting the workflow content extension");
        }
    }

    /// <summary>
    /// Deletes all workflow entries (WorkflowInstance and WorkflowContentExtension) for a given content ID
    /// </summary>
    [HttpDelete("content/{contentId}/cleanup")]
    public async Task<ActionResult> CleanupWorkflowEntriesForContent(string contentId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentId))
                return BadRequest("ContentId is required");

            // Check if there are workflow entries for this content
            var existingContentExtension = await _workflowService.GetWorkflowContentExtensionAsync(contentId);
            if (existingContentExtension == null)
            {
                return NotFound("No workflow entries found for the specified content");
            }

            var deletedInstanceId = (Guid?)null;
            var deletedExtensionId = existingContentExtension.Id;

            // If there's an active workflow instance, delete it first
            if (existingContentExtension.CurrentWorkflowInstanceId.HasValue)
            {
                var existingInstance = await _workflowService.GetWorkflowInstanceByIdAsync(existingContentExtension.CurrentWorkflowInstanceId.Value);
                if (existingInstance != null)
                {
                    deletedInstanceId = existingInstance.Id;
                    await _workflowService.DeleteWorkflowInstanceAsync(existingInstance.Id);
                    _logger.LogInformation("Deleted WorkflowInstance {InstanceId} for content {ContentId}", existingInstance.Id, contentId);
                }
            }

            // Delete the WorkflowContentExtension
            await _workflowService.DeleteWorkflowContentExtensionAsync(contentId);
            _logger.LogInformation("Deleted WorkflowContentExtension {ExtensionId} for content {ContentId}", deletedExtensionId, contentId);

            return Ok(new
            {
                success = true,
                message = "All workflow entries cleaned up successfully",
                contentId = contentId,
                deletedWorkflowInstanceId = deletedInstanceId,
                deletedWorkflowContentExtensionId = deletedExtensionId,
                summary = deletedInstanceId.HasValue 
                    ? "Deleted both WorkflowInstance and WorkflowContentExtension" 
                    : "Deleted WorkflowContentExtension only (no active instance found)"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up workflow entries for ContentId {ContentId}", contentId);
            return StatusCode(500, "An error occurred while cleaning up workflow entries");
        }
    }

    /// <summary>
    /// Gets all workflow instances regardless of user (admin access) with user information
    /// </summary>
    [HttpGet("workflow-instances")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllWorkflowInstances()
    {
        var instances = await _workflowService.GetAllWorkflowInstancesAsync();

        // Get all unique user IDs from the instances
        var userIds = instances
            .Where(i => !string.IsNullOrEmpty(i.CreatedBy) && i.CreatedBy.ToLower() != "system")
            .Select(i => i.CreatedBy)
            .Distinct()
            .ToList();

        // Fetch user information for all user IDs
        var userDict = new Dictionary<string, object>();

        foreach (var userId in userIds)
        {
            try
            {
                // Try to get user by ID first
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    userDict[userId] = new
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        DisplayName = !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email
                    };
                }
                else
                {
                    // Try to get user by username/email if not found by ID
                    user = await _userManager.FindByNameAsync(userId) ?? await _userManager.FindByEmailAsync(userId);
                    if (user != null)
                    {
                        userDict[userId] = new
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            DisplayName = !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email
                        };
                    }
                    else
                    {
                        // User not found, create a fallback entry
                        userDict[userId] = new
                        {
                            Id = userId,
                            UserName = userId,
                            Email = userId,
                            DisplayName = userId
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load user information for user ID: {UserId}", userId);
                // Create a fallback entry for failed lookups
                userDict[userId] = new
                {
                    Id = userId,
                    UserName = userId,
                    Email = userId,
                    DisplayName = userId
                };
            }
        }
        
        // Create the response with enhanced instance data
        var enhancedInstances = instances.Select(instance => new
        {
            // All original instance properties
            Id = instance.Id,
            ContentId = instance.ContentId,
            ContentType = instance.ContentType,
            ContentTitle = instance.ContentTitle,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            CurrentStateId = instance.CurrentStateId,
            Status = instance.Status,
            Metadata = instance.Metadata,
            Created = instance.Created,
            LastModified = instance.LastModified,
            CreatedBy = instance.CreatedBy,

            // Enhanced user information
            CreatedByUser = userDict.ContainsKey(instance.CreatedBy ?? "")
                ? userDict[instance.CreatedBy]
                : new
                {
                    Id = instance.CreatedBy ?? "system",
                    UserName = instance.CreatedBy ?? "System",
                    Email = instance.CreatedBy ?? "system",
                    DisplayName = string.IsNullOrEmpty(instance.CreatedBy) || instance.CreatedBy.ToLower() == "system"
                        ? "System"
                        : instance.CreatedBy
                }
        });

        return Ok(enhancedInstances);
    }

    [HttpGet("instances/{id}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstance(Guid id)
    {
        var instance = await _workflowService.GetWorkflowInstanceByIdAsync(id);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    /// <summary>
    /// Gets a workflow instance with its current state and available transitions
    /// </summary>
    [HttpGet("workflow-instances/{workflowInstanceId}/transitions")]
    public async Task<ActionResult<WorkflowInstanceTransitionsResponse>> GetWorkflowInstanceTransitions(Guid workflowInstanceId)
    {
        try
        {
            if (workflowInstanceId == Guid.Empty)
                return BadRequest("WorkflowInstanceId is required");

            // Get the workflow instance
            var workflowInstance = await _workflowService.GetWorkflowInstanceByIdAsync(workflowInstanceId);
            if (workflowInstance == null)
                return NotFound("Workflow instance not found");

            // Get the current state with outgoing transitions
            var currentState = await _workflowService.GetWorkflowStateByIdAsync(workflowInstance.CurrentStateId);
            if (currentState == null)
                return NotFound("Current state not found");

            // Get all outgoing transitions for the current state
            var outgoingTransitions = await _workflowService.GetTransitionRulesByDefinitionAsync(workflowInstance.WorkflowDefinitionId);
            var availableTransitions = outgoingTransitions
                .Where(t => t.FromStateId == currentState.Id && t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ToList();

            var response = new WorkflowInstanceTransitionsResponse
            {
                WorkflowInstanceId = workflowInstanceId,
                WorkflowInstance = workflowInstance,
                CurrentState = currentState,
                AvailableTransitions = availableTransitions
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow instance transitions for {WorkflowInstanceId}", workflowInstanceId);
            return StatusCode(500, "An error occurred while getting workflow instance transitions");
        }
    }

    /// <summary>
    /// Performs a state transition on a workflow instance and publishes content if transitioning to a published or final state
    /// </summary>
    [HttpPost("workflow-instances/{workflowInstanceId}/transition")]
    public async Task<ActionResult<WorkflowInstance>> PerformWorkflowTransition(Guid workflowInstanceId, [FromBody] WorkflowTransitionRequest request)
    {
        try
        {
            if (workflowInstanceId == Guid.Empty)
                return BadRequest("WorkflowInstanceId is required");

            if (request == null || request.TransitionRuleId == Guid.Empty)
                return BadRequest("TransitionRuleId is required");

            // Get the workflow instance
            var workflowInstance = await _workflowService.GetWorkflowInstanceByIdAsync(workflowInstanceId);
            if (workflowInstance == null)
                return NotFound("Workflow instance not found");

            // Get the transition rule
            var transitionRule = await _workflowService.GetTransitionRuleByIdAsync(request.TransitionRuleId);
            if (transitionRule == null)
                return NotFound("Transition rule not found");

            // Validate that the transition is valid from the current state
            if (transitionRule.FromStateId != workflowInstance.CurrentStateId)
                return BadRequest("Invalid transition: the transition rule does not start from the current state");

            // Check if the transition is active
            if (!transitionRule.IsActive)
                return BadRequest("Transition rule is not active");

            // Validate comment requirement
            if (transitionRule.RequiresComment && string.IsNullOrWhiteSpace(request.Comment))
                return BadRequest("This transition requires a comment");

            // Get the current and target states for message creation
            var fromState = await _workflowService.GetWorkflowStateByIdAsync(transitionRule.FromStateId);
            var targetState = await _workflowService.GetWorkflowStateByIdAsync(transitionRule.ToStateId);
            if (fromState == null || targetState == null)
                return BadRequest("From state or target state not found");

            // Perform the transition - update the current state
            workflowInstance.CurrentStateId = transitionRule.ToStateId;
            workflowInstance.LastModified = DateTime.UtcNow;

            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

            if (user == null)
            {
                _logger.LogWarning("User not found for workflow transition");
                return Unauthorized("User not found");
            }

            // Get user roles and validate against transition rule allowed roles
            var userRoles = await _userManager.GetRolesAsync(user);
            _logger.LogInformation("User {UserId} ({UserName}) has roles: [{UserRoles}]",
                user.Id, user.UserName ?? user.Email, string.Join(", ", userRoles));
            _logger.LogInformation("Transition rule {TransitionRuleId} requires roles: {AllowedRoles}",
                transitionRule.Id, transitionRule.AllowedRoles ?? "NULL (no restrictions)");
            _logger.LogInformation("Raw AllowedRoles value: '{RawValue}'", transitionRule.AllowedRoles);

            // Check if user has required roles for this transition
            if (!string.IsNullOrWhiteSpace(transitionRule.AllowedRoles))
            {
                List<string> allowedRoles;

                // Check if AllowedRoles is in JSON array format
                if (transitionRule.AllowedRoles.TrimStart().StartsWith("["))
                {
                    try
                    {
                        // Parse as JSON array
                        var jsonRoles = System.Text.Json.JsonSerializer.Deserialize<string[]>(transitionRule.AllowedRoles);
                        allowedRoles = jsonRoles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).ToList() ?? new List<string>();
                        _logger.LogInformation("Parsed AllowedRoles from JSON array format");
                    }
                    catch (System.Text.Json.JsonException)
                    {
                        _logger.LogWarning("Failed to parse AllowedRoles as JSON, falling back to comma-separated parsing");
                        // Fallback to comma-separated parsing
                        allowedRoles = transitionRule.AllowedRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(r => r.Trim())
                            .Where(r => !string.IsNullOrEmpty(r))
                            .ToList();
                    }
                }
                else
                {
                    // Parse as comma-separated string
                    allowedRoles = transitionRule.AllowedRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim())
                        .Where(r => !string.IsNullOrEmpty(r))
                        .ToList();
                    _logger.LogInformation("Parsed AllowedRoles from comma-separated format");
                }

                _logger.LogInformation("Final parsed allowed roles: [{ParsedAllowedRoles}]", string.Join(", ", allowedRoles));

                if (allowedRoles.Any())
                {
                    // Check if user has at least one of the required roles (case-insensitive)
                    var hasRequiredRole = false;
                    string matchingRole = null;

                    foreach (var allowedRole in allowedRoles)
                    {
                        var userRole = userRoles.FirstOrDefault(ur => ur.Equals(allowedRole, StringComparison.OrdinalIgnoreCase));
                        if (userRole != null)
                        {
                            hasRequiredRole = true;
                            matchingRole = userRole;
                            _logger.LogInformation("User has matching role: '{UserRole}' matches required '{AllowedRole}'", userRole, allowedRole);
                            break;
                        }
                    }

                    if (!hasRequiredRole)
                    {
                        _logger.LogWarning("AUTHORIZATION FAILED: User {UserId} with roles [{UserRoles}] does not have any of the required roles [{RequiredRoles}] for transition {TransitionRuleId}",
                            user.Id, string.Join(", ", userRoles), string.Join(", ", allowedRoles), transitionRule.Id);

                        return Unauthorized(
                            $"You do not have permission to perform this transition. Required roles: {string.Join(", ", allowedRoles)}");
                    }

                    _logger.LogInformation("AUTHORIZATION SUCCESS: User {UserId} has required role '{MatchingRole}' for transition {TransitionRuleId}",
                        user.Id, matchingRole, transitionRule.Id);
                }
                else
                {
                    _logger.LogInformation("AllowedRoles contains only empty values - treating as no restrictions for transition {TransitionRuleId}",
                        transitionRule.Id);
                }
            }
            else
            {
                _logger.LogInformation("No role restrictions for transition rule {TransitionRuleId} - allowing all authenticated users",
                    transitionRule.Id);
            }

            _logger.LogInformation("Transition will be performed by user: {UserName} ({UserId})",
                user.UserName ?? user.Email, user.Id);

            
            // Add transition comment to metadata if provided
            if (!string.IsNullOrWhiteSpace(request.Comment))
            {
                var transitionLog = new
                {
                    timestamp = DateTime.UtcNow,
                    fromStateId = transitionRule.FromStateId,
                    toStateId = transitionRule.ToStateId,
                    transitionRuleId = transitionRule.Id,
                    comment = request.Comment,
                    performedBy = user.UserName
                };

                // Update metadata with transition log
                var metadata = string.IsNullOrWhiteSpace(workflowInstance.Metadata) ? "{}" : workflowInstance.Metadata;
                // Simple append - in production you might want to parse JSON and add to an array
                workflowInstance.Metadata = metadata.TrimEnd('}') +
                    (metadata == "{}" ? "" : ",") +
                    $"\"lastTransition\":{System.Text.Json.JsonSerializer.Serialize(transitionLog)}}}";
            }

            // Update the workflow instance
            var updatedInstance = await _workflowService.UpdateWorkflowInstanceAsync(workflowInstance);

            // Check if we need to publish the content
            bool contentPublished = false;
            if (targetState.IsPublished || targetState.IsFinal)
            {
                contentPublished = await PublishContentAsync(workflowInstance.ContentId, workflowInstance.ContentType);
            }

            // If current state is published or final, and we're going back to initial state, unpublish the content
            bool contentUnpublished = false;
            if (fromState != null && (fromState.IsPublished || fromState.IsFinal) && targetState != null && !targetState.IsPublished)
            {
              contentUnpublished = await UnpublishContentAsync(workflowInstance.ContentId, workflowInstance.ContentType);
            }

            // Send audit message to RabbitMQ on successful transition
            bool messagePublished = false;
            try
            {
                // Parse contentId to Guid for the message
                if (Guid.TryParse(workflowInstance.ContentId, out var contentGuid))
                {
                    // Create the audit event message using the updated structure
                    var stateChangedEvent = new WorkflowStateChangedEvent
                    {
                        ContentId = contentGuid,
                        ContentName = workflowInstance.ContentTitle ?? "Unknown Content",
                        FromState = fromState.Name,
                        ToState = targetState.Name,
                        transitionDescription = transitionRule.Description ?? "Content approved for next state",
                        reviewedBy = user.UserName ?? user.Email ?? "Unknown",
                        approved = true, // Normal transitions are considered approvals
                        Timestamp = DateTime.UtcNow.AddHours(1),
                        Comments = request.Comment,
                        Success = true,
                        ErrorMessage = null
                    };

                    // Publish the message to RabbitMQ
                    messagePublished = await _messagePublisher.PublishStateChangedEventAsync(stateChangedEvent);

                    if (messagePublished)
                    {
                        _logger.LogInformation("Successfully published workflow state change message for transition {TransitionRuleId}", transitionRule.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to publish workflow state change message for transition {TransitionRuleId}", transitionRule.Id);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not parse ContentId {ContentId} as Guid for message publishing", workflowInstance.ContentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing workflow state change message for transition {TransitionRuleId}", transitionRule.Id);
                // Don't fail the transition if message publishing fails
            }

            _logger.LogInformation("Workflow transition performed: Instance {InstanceId} moved from state {FromStateId} to {ToStateId}. Content published: {ContentPublished}, Message published: {MessagePublished}",
                workflowInstanceId, transitionRule.FromStateId, transitionRule.ToStateId, contentPublished, messagePublished);

            return Ok(new
            {
                workflowInstance = updatedInstance,
                contentPublished = contentPublished,
                messagePublished = messagePublished,
                targetState = new
                {
                    targetState.Name,
                    targetState.IsPublished,
                    targetState.IsFinal
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing workflow transition for instance {WorkflowInstanceId}", workflowInstanceId);
            return StatusCode(500, "An error occurred while performing the workflow transition");
        }
    }

    /// <summary>
    /// Publishes content based on content type
    /// </summary>
    private async Task<bool> PublishContentAsync(string contentId, string contentType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentId) || string.IsNullOrWhiteSpace(contentType))
            {
                _logger.LogWarning("Cannot publish content: ContentId or ContentType is empty");
                return false;
            }

            // Parse contentId to Guid
            if (!Guid.TryParse(contentId, out var contentGuid))
            {
                _logger.LogWarning("Cannot publish content: Invalid ContentId format {ContentId}", contentId);
                return false;
            }

            switch (contentType.ToLowerInvariant())
            {
                case "page":
                    return await PublishPageAsync(contentGuid);

                case "post":
                    return await PublishPostAsync(contentGuid);

                default:
                    _logger.LogWarning("Content publishing not supported for content type: {ContentType}", contentType);
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing content {ContentId} of type {ContentType}", contentId, contentType);
            return false;
        }
    }

    /// <summary>
    /// Publishes a page
    /// </summary>
    private async Task<bool> PublishPageAsync(Guid pageId)
    {
        try
        {
            // Get the page
            var page = await _api.Pages.GetByIdAsync(pageId);
            if (page == null)
            {
                _logger.LogWarning("Page not found for publishing: {PageId}", pageId);
                return false;
            }

            // Check if already published
            if (page.Published.HasValue)
            {
                _logger.LogInformation("Page {PageId} is already published", pageId);
                return true;
            }

            // Set published date and save
            page.Published = DateTime.UtcNow;
            
            await _api.Pages.SaveAsync(page);
            
            _logger.LogInformation("Successfully published page {PageId}", pageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing page {PageId}", pageId);
            return false;
        }
    }

    /// <summary>
    /// Publishes a post
    /// </summary>
    private async Task<bool> PublishPostAsync(Guid postId)
    {
        try
        {
            // Get the post
            var post = await _api.Posts.GetByIdAsync(postId);
            if (post == null)
            {
                _logger.LogWarning("Post not found for publishing: {PostId}", postId);
                return false;
            }

            // Check if already published
            if (post.Published.HasValue)
            {
                _logger.LogInformation("Post {PostId} is already published", postId);
                return true;
            }

            // Set published date and save
            post.Published = DateTime.UtcNow;
            
            await _api.Posts.SaveAsync(post);
            
            _logger.LogInformation("Successfully published post {PostId}", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing post {PostId}", postId);
            return false;
        }
    }

    /// <summary>
    /// Unpublishes content based on content type
    /// </summary>
    private async Task<bool> UnpublishContentAsync(string contentId, string contentType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(contentId) || string.IsNullOrWhiteSpace(contentType))
            {
                _logger.LogWarning("Cannot unpublish content: ContentId or ContentType is empty");
                return false;
            }

            // Parse contentId to Guid
            if (!Guid.TryParse(contentId, out var contentGuid))
            {
                _logger.LogWarning("Cannot unpublish content: Invalid ContentId format {ContentId}", contentId);
                return false;
            }

            switch (contentType.ToLowerInvariant())
            {
                case "page":
                    return await UnpublishPageAsync(contentGuid);

                case "post":
                    return await UnpublishPostAsync(contentGuid);

                default:
                    _logger.LogWarning("Content unpublishing not supported for content type: {ContentType}", contentType);
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing content {ContentId} of type {ContentType}", contentId, contentType);
            return false;
        }
    }

    /// <summary>
    /// Unpublishes a page
    /// </summary>
    private async Task<bool> UnpublishPageAsync(Guid pageId)
    {
        try
        {
            // Get the page
            var page = await _api.Pages.GetByIdAsync(pageId);
            if (page == null)
            {
                _logger.LogWarning("Page not found for unpublishing: {PageId}", pageId);
                return false;
            }

            // Check if already unpublished
            if (!page.Published.HasValue)
            {
                _logger.LogInformation("Page {PageId} is already unpublished", pageId);
                return true;
            }

            // Set published date to null and save
            page.Published = null;

            await _api.Pages.SaveAsync(page);

            _logger.LogInformation("Successfully unpublished page {PageId}", pageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing page {PageId}", pageId);
            return false;
        }
    }

    /// <summary>
    /// Unpublishes a post
    /// </summary>
    private async Task<bool> UnpublishPostAsync(Guid postId)
    {
        try
        {
            // Get the post
            var post = await _api.Posts.GetByIdAsync(postId);
            if (post == null)
            {
                _logger.LogWarning("Post not found for unpublishing: {PostId}", postId);
                return false;
            }

            // Check if already unpublished
            if (!post.Published.HasValue)
            {
                _logger.LogInformation("Post {PostId} is already unpublished", postId);
                return true;
            }

            // Set published date to null and save
            post.Published = null;

            await _api.Posts.SaveAsync(post);

            _logger.LogInformation("Successfully unpublished post {PostId}", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing post {PostId}", postId);
            return false;
        }
    }

    [HttpPost("instances")]
    public async Task<ActionResult<WorkflowInstance>> CreateWorkflowInstance(WorkflowInstance instance)
    {
        var result = await _workflowService.CreateWorkflowInstanceAsync(instance);
        return CreatedAtAction(nameof(GetWorkflowInstance), new { id = result.Id }, result);
    }

    /// <summary>
    /// Rejects a workflow instance by resetting it to the initial state, unpublishing content if needed, and sending MQ message
    /// </summary>
    [HttpPost("instances/{instanceId}/reject")]
    public async Task<ActionResult> RejectWorkflowInstance(Guid instanceId, [FromBody] WorkflowTransitionRequest request = null)
    {
        try
        {
            if (instanceId == Guid.Empty)
                return BadRequest("WorkflowInstance ID is required");

            // Get the workflow instance
            var workflowInstance = await _workflowService.GetWorkflowInstanceByIdAsync(instanceId);
            if (workflowInstance == null)
                return NotFound("Workflow instance not found");

            // Get the workflow definition to find the initial state
            var workflowDefinition = await _workflowService.GetWorkflowDefinitionByIdAsync(workflowInstance.WorkflowDefinitionId);
            if (workflowDefinition == null)
                return BadRequest("Workflow definition not found");

            // Get all states for this workflow definition
            var workflowStates = await _workflowService.GetWorkflowStatesByDefinitionAsync(workflowInstance.WorkflowDefinitionId);
            var initialState = workflowStates.FirstOrDefault(s => s.IsInitial);

            if (initialState == null)
                return BadRequest("No initial state found for this workflow");

            // Get the current state to check if content needs to be unpublished and for messaging
            var currentState = await _workflowService.GetWorkflowStateByIdAsync(workflowInstance.CurrentStateId);
            bool contentUnpublished = false;

            // If current state is published or final, and we're going back to initial state, unpublish the content
            if (currentState != null && (currentState.IsPublished || currentState.IsFinal))
            {
                contentUnpublished = await UnpublishContentAsync(workflowInstance.ContentId, workflowInstance.ContentType);
            }

            // Get current user for logging and messaging
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            string userName = user?.UserName ?? user?.Email ?? "System";

            // Reset the workflow instance to the initial state
            workflowInstance.CurrentStateId = initialState.Id;
            workflowInstance.LastModified = DateTime.UtcNow;

            // Add rejection metadata
            var rejectionLog = new
            {
                timestamp = DateTime.UtcNow,
                action = "rejected",
                previousStateId = currentState?.Id,
                resetToStateId = initialState.Id,
                performedBy = userName,
                contentUnpublished = contentUnpublished,
                comment = request?.Comment
            };

            // Update metadata with rejection log
            var metadata = string.IsNullOrWhiteSpace(workflowInstance.Metadata) ? "{}" : workflowInstance.Metadata;
            workflowInstance.Metadata = metadata.TrimEnd('}') +
                (metadata == "{}" ? "" : ",") +
                $"\"lastRejection\":{System.Text.Json.JsonSerializer.Serialize(rejectionLog)}}}";

            // Update the workflow instance
            var updatedInstance = await _workflowService.UpdateWorkflowInstanceAsync(workflowInstance);

            // Send rejection message to RabbitMQ
            bool messagePublished = false;
            try
            {
                // Parse contentId to Guid for the message
                if (Guid.TryParse(workflowInstance.ContentId, out var contentGuid))
                {
                    // Create the audit event message for rejection
                    var stateChangedEvent = new WorkflowStateChangedEvent
                    {
                        ContentId = contentGuid,
                        ContentName = workflowInstance.ContentTitle ?? "Unknown Content",
                        FromState = currentState?.Name ?? "Unknown",
                        ToState = initialState.Name,
                        transitionDescription = "Content rejected and reset to initial state",
                        reviewedBy = userName,
                        approved = false, // Rejection is not an approval
                        Timestamp = DateTime.UtcNow.AddHours(1),
                        Comments = request?.Comment ?? "Content rejected",
                        Success = true,
                        ErrorMessage = null
                    };

                    // Publish the message to RabbitMQ
                    messagePublished = await _messagePublisher.PublishStateChangedEventAsync(stateChangedEvent);

                    if (messagePublished)
                    {
                        _logger.LogInformation("Successfully published workflow rejection message for instance {InstanceId}", instanceId);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to publish workflow rejection message for instance {InstanceId}", instanceId);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not parse ContentId {ContentId} as Guid for rejection message publishing", workflowInstance.ContentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing workflow rejection message for instance {InstanceId}", instanceId);
                // Don't fail the rejection if message publishing fails
            }

            _logger.LogInformation("Workflow instance {InstanceId} rejected and reset to initial state {StateId}. Content unpublished: {ContentUnpublished}, Message published: {MessagePublished}",
                instanceId, initialState.Id, contentUnpublished, messagePublished);

            return Ok(new
            {
                success = true,
                message = "Workflow instance rejected and reset to initial state",
                workflowInstanceId = instanceId,
                resetToState = new
                {
                    id = initialState.Id,
                    name = initialState.Name
                },
                updatedInstance = updatedInstance,
                contentUnpublished = contentUnpublished,
                messagePublished = messagePublished
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting workflow instance {InstanceId}", instanceId);
            return StatusCode(500, "An error occurred while rejecting the workflow instance");
        }
    }

    [HttpDelete("instances/{id}")]
    public async Task<ActionResult> DeleteWorkflowInstance(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return BadRequest("WorkflowInstance ID is required");

            // Check if the workflow instance exists
            var instance = await _workflowService.GetWorkflowInstanceByIdAsync(id);
            if (instance == null)
                return NotFound("Workflow instance not found");

            // Delete the workflow instance
            await _workflowService.DeleteWorkflowInstanceAsync(id);

            return Ok(new
            {
                success = true,
                message = "Workflow instance deleted successfully",
                workflowInstanceId = id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow instance {WorkflowInstanceId}", id);
            return StatusCode(500, "An error occurred while deleting the workflow instance");
        }
    }

    [HttpPut("instances/{id}")]
    public async Task<ActionResult<WorkflowInstance>> UpdateWorkflowInstance(Guid id, WorkflowInstance instance)
    {
        if (id != instance.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowInstanceAsync(instance);
        return Ok(result);
    }

    [HttpPost("instances/{id}/transition")]
    public async Task<ActionResult> TransitionWorkflow(Guid id, [FromBody] string targetState)
    {
        var success = await _workflowService.TransitionWorkflowAsync(id, targetState);
        if (!success)
            return BadRequest("Invalid transition or insufficient permissions");

        return Ok();
    }

    [HttpGet("instances/state/{state}")]
    public async Task<ActionResult<IEnumerable<WorkflowInstance>>> GetWorkflowInstancesByState(string state)
    {
        var instances = await _workflowService.GetWorkflowInstancesByStateAsync(state);
        return Ok(instances);
    }

    [HttpPost("CreateWorkflowInstanceWithContent")]
    public async Task<IActionResult> CreateWorkflowInstanceWithContent([FromBody] CreateWorkflowInstanceWithContentRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ContentId) || request.WorkflowDefinitionId == Guid.Empty)
            {
                return BadRequest("ContentId and WorkflowDefinitionId are required");
            }

            var workflowInstance = await _workflowService.CreateWorkflowInstanceWithContentAsync(
                request.ContentId, 
                request.WorkflowDefinitionId,
                request.ContentType, 
                request.ContentTitle);

            return Ok(new { 
                workflowInstanceId = workflowInstance.Id,
                contentId = workflowInstance.ContentId,
                workflowDefinitionId = workflowInstance.WorkflowDefinitionId,
                currentStateId = workflowInstance.CurrentStateId,
                status = workflowInstance.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow instance with content");
            return StatusCode(500, "An error occurred while creating the workflow instance");
        }
    }

    /// <summary>
    /// Assigns a workflow to a page - Simple endpoint for testing
    /// </summary>
    [HttpPost("assign-workflow-to-page/{pageId}")]
    public async Task<IActionResult> AssignWorkflowToPage(Guid pageId, [FromBody] AssignWorkflowRequest request)
    {
        try
        {
            if (pageId == Guid.Empty)
                return BadRequest("PageId is required");

            if (request == null || request.WorkflowDefinitionId == Guid.Empty)
                return BadRequest("WorkflowDefinitionId is required");

            // Get page information
            var page = await _api.Pages.GetByIdAsync(pageId);
            if (page == null)
                return NotFound("Page not found");

            string contentId = pageId.ToString();

            // Check for existing workflow entries and delete them if found
            var existingContentExtension = await _workflowService.GetWorkflowContentExtensionAsync(contentId);
            if (existingContentExtension != null)
            {
                _logger.LogInformation("Found existing WorkflowContentExtension for page {PageId}, deleting it", pageId);
                
                // If there's an active workflow instance, delete it first
                if (existingContentExtension.CurrentWorkflowInstanceId.HasValue)
                {
                    var existingInstance = await _workflowService.GetWorkflowInstanceByIdAsync(existingContentExtension.CurrentWorkflowInstanceId.Value);
                    if (existingInstance != null)
                    {
                        _logger.LogInformation("Found existing WorkflowInstance {InstanceId} for page {PageId}, deleting it", existingInstance.Id, pageId);
                        await _workflowService.DeleteWorkflowInstanceAsync(existingInstance.Id);
                    }
                }
                
                // Delete the WorkflowContentExtension
                await _workflowService.DeleteWorkflowContentExtensionAsync(contentId);
            }

            // Create workflow instance for the page
            var workflowInstance = await _workflowService.CreateWorkflowInstanceWithContentAsync(
                contentId,
                request.WorkflowDefinitionId,
                "page",
                page.Title);

            return Ok(new
            {
                success = true,
                message = "Workflow assigned to page successfully",
                workflowInstanceId = workflowInstance.Id,
                pageId = pageId,
                pageTitle = page.Title,
                workflowDefinitionId = request.WorkflowDefinitionId,
                replacedExisting = existingContentExtension != null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning workflow to page {PageId}", pageId);
            return StatusCode(500, "An error occurred while assigning workflow to page");
        }
    }

    public class AssignWorkflowRequest
    {
        public Guid WorkflowDefinitionId { get; set; }
    }

    #endregion

    #region Workflow Content Extensions

    [HttpGet("content-extensions/{contentId}/exists")]
    public async Task<ActionResult<bool>> CheckWorkflowContentExtensionExists(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            return BadRequest("ContentId is required");
        var exists = await _workflowService.WorkflowContentExtensionExistsAsync(contentId);
        return Ok(exists);
    }

    [HttpGet("content-extensions/{contentId}")]
    public async Task<ActionResult<WorkflowContentExtension>> GetWorkflowContentExtension(string contentId)
    {
        if (string.IsNullOrWhiteSpace(contentId))
            return BadRequest("ContentId is required");

        var extension = await _workflowService.GetWorkflowContentExtensionAsync(contentId);
        if (extension == null)
            return NotFound();
        
        return Ok(extension);
    }

    /// <summary>
    /// Given a WorkflowDefinitionId, retrieve all the WorkflowContentExtensions
    /// </summary>
    [HttpGet("definitions/{workflowDefinitionId}/content-extensions")]
    public async Task<ActionResult<IEnumerable<WorkflowContentExtension>>> GetWorkflowContentExtensionsByDefinition(Guid workflowDefinitionId)
    {
        if (workflowDefinitionId == Guid.Empty)
            return BadRequest("WorkflowDefinitionId is required");

        var extensions = await _workflowService.GetWorkflowContentExtensionsByDefinitionAsync(workflowDefinitionId);
        return Ok(extensions);
    }

    /// <summary>
    /// Given a WorkflowInstanceId, retrieve the WorkflowInstance
    /// </summary>
    [HttpGet("workflow-instances/{workflowInstanceId}")]
    public async Task<ActionResult<WorkflowInstance>> GetWorkflowInstanceById(Guid workflowInstanceId)
    {
        if (workflowInstanceId == Guid.Empty)
            return BadRequest("WorkflowInstanceId is required");

        var instance = await _workflowService.GetWorkflowInstanceByIdAsync(workflowInstanceId);
        if (instance == null)
            return NotFound();

        return Ok(instance);
    }

    /// <summary>
    /// Given a WorkflowInstanceId, update the WorkflowInstance (supports partial updates)
    /// </summary>
    [HttpPut("workflow-instances/{workflowInstanceId}")]
    public async Task<ActionResult<WorkflowInstance>> UpdateWorkflowInstanceById(Guid workflowInstanceId, [FromBody] WorkflowInstance instance)
    {
        try
        {
            if (workflowInstanceId == Guid.Empty)
                return BadRequest("WorkflowInstanceId is required");

            // Allow null or empty body for partial updates
            var result = await _workflowService.PartialUpdateWorkflowInstanceAsync(workflowInstanceId, instance);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while updating the workflow instance");
        }
    }

    #endregion

    #region Workflow States

    [HttpGet("definitions/{definitionId}/states")]
    public async Task<ActionResult<IEnumerable<WorkflowState>>> GetWorkflowStates(Guid definitionId)
    {
        var states = await _workflowService.GetWorkflowStatesByDefinitionAsync(definitionId);
        return Ok(states);
    }

    [HttpGet("states/{id}")]
    public async Task<ActionResult<WorkflowState>> GetWorkflowState(Guid id)
    {
        var state = await _workflowService.GetWorkflowStateByIdAsync(id);
        if (state == null)
            return NotFound();

        return Ok(state);
    }

    [HttpPost("states")]
    public async Task<ActionResult<WorkflowState>> CreateWorkflowState(WorkflowState state)
    {
        var result = await _workflowService.CreateWorkflowStateAsync(state);
        return CreatedAtAction(nameof(GetWorkflowState), new { id = result.Id }, result);
    }

    [HttpPut("states/{id}")]
    public async Task<ActionResult<WorkflowState>> UpdateWorkflowState(Guid id, WorkflowState state)
    {
        if (id != state.Id)
            return BadRequest();

        var result = await _workflowService.UpdateWorkflowStateAsync(state);
        return Ok(result);
    }

    [HttpDelete("states/{id}")]
    public async Task<ActionResult> DeleteWorkflowState(Guid id)
    {
        await _workflowService.DeleteWorkflowStateAsync(id);
        return NoContent();
    }

    #endregion

    #region Transition Rules

    [HttpGet("definitions/{definitionId}/rules")]
    public async Task<ActionResult<IEnumerable<TransitionRule>>> GetTransitionRules(Guid definitionId)
    {
        var rules = await _workflowService.GetTransitionRulesByDefinitionAsync(definitionId);
        return Ok(rules);
    }

    [HttpGet("rules/{id}")]
    public async Task<ActionResult<TransitionRule>> GetTransitionRule(Guid id)
    {
        var rule = await _workflowService.GetTransitionRuleByIdAsync(id);
        if (rule == null)
            return NotFound();

        return Ok(rule);
    }

    [HttpPost("rules")]
    public async Task<ActionResult<TransitionRule>> CreateTransitionRule(TransitionRule rule)
    {
        var result = await _workflowService.CreateTransitionRuleAsync(rule);
        return CreatedAtAction(nameof(GetTransitionRule), new { id = result.Id }, result);
    }

    [HttpPut("rules/{id}")]
    public async Task<ActionResult<TransitionRule>> UpdateTransitionRule(Guid id, TransitionRule rule)
    {
        if (id != rule.Id)
            return BadRequest();

        var result = await _workflowService.UpdateTransitionRuleAsync(rule);
        return Ok(result);
    }

    [HttpDelete("rules/{id}")]
    public async Task<ActionResult> DeleteTransitionRule(Guid id)
    {
        await _workflowService.DeleteTransitionRuleAsync(id);
        return NoContent();
    }

    #endregion

    #region Debug Endpoints

    [HttpGet("debug/database")]
    [AllowAnonymous]
    public async Task<ActionResult> DebugDatabase()
    {
        var canConnect = await _workflowService.TestDatabaseConnectionAsync();
        return Ok(new { 
            DatabaseConnected = canConnect,
            Message = canConnect ? "Database connection successful" : "Database connection failed"
        });
    }

    [HttpGet("debug/roles")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSystemRoles()
    {
        var roles = await _workflowService.GetSystemRolesAsync();
        return Ok(roles.Select(r => new { 
            Id = r.Id,
            Name = r.Name,
            NormalizedName = r.NormalizedName
        }));
    }

    #endregion
}
