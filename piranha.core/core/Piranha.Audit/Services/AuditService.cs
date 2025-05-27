/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.Logging;
using Piranha.Audit.Events;
using Piranha.Audit.Models;
using Piranha.Audit.Repositories;
using System.Text.Json;

namespace Piranha.Audit.Services;

/// <summary>
/// Default implementation of the audit service.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly IStateChangeRecordRepository _stateChangeRecordRepository;
    private readonly ILogger<AuditService> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="stateChangeRecordRepository">The state change record repository</param>
    /// <param name="logger">The logger</param>
    public AuditService(
        IStateChangeRecordRepository stateChangeRecordRepository,
        ILogger<AuditService> logger)
    {
        _stateChangeRecordRepository = stateChangeRecordRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await LogStateChangeAsync(
                stateChangedEvent.WorkflowInstanceId,
                stateChangedEvent.ContentId,
                stateChangedEvent.ContentType,
                stateChangedEvent.FromState,
                stateChangedEvent.ToState,
                stateChangedEvent.UserId,
                stateChangedEvent.Username,
                stateChangedEvent.Comments,
                stateChangedEvent.TransitionRuleId,
                stateChangedEvent.IsAutomaticTransition,
                JsonSerializer.Serialize(stateChangedEvent.Metadata),
                stateChangedEvent.Success,
                stateChangedEvent.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process workflow state changed event {EventId}",
                stateChangedEvent.EventId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task LogStateChangeAsync(
        Guid workflowInstanceId,
        Guid contentId,
        string contentType,
        string fromState,
        string toState,
        string userId,
        string username,
        string comments = null,
        Guid? transitionRuleId = null,
        bool isAutomaticTransition = false,
        string metadata = null,
        bool success = true,
        string errorMessage = null)
    {
        // Create state change record
        var stateChangeRecord = new StateChangeRecord
        {
            Id = Guid.NewGuid(),
            WorkflowInstanceId = workflowInstanceId,
            ContentId = contentId,
            ContentType = contentType,
            FromState = fromState,
            ToState = toState,
            UserId = userId,
            Username = username,
            Timestamp = DateTime.UtcNow,
            Comments = comments,
            TransitionRuleId = transitionRuleId,
            Metadata = metadata,
            Success = success,
            ErrorMessage = errorMessage
        };

        await _stateChangeRecordRepository.SaveAsync(stateChangeRecord);
        _logger.LogInformation("State change logged for content {ContentId} from {FromState} to {ToState} by user {UserId}",
            contentId, fromState, toState, userId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StateChangeRecord>> GetStateChangeHistoryAsync(Guid contentId)
    {
        return await _stateChangeRecordRepository.GetByContentAsync(contentId);
    }

    /// <inheritdoc />
    public async Task<int> CleanupOldRecordsAsync(int retentionDays = 365)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        
        var stateChangeDeleted = await _stateChangeRecordRepository.DeleteOlderThanAsync(cutoffDate);
        
        _logger.LogInformation("Cleanup completed. Deleted {TotalDeleted} state change records older than {CutoffDate}",
            stateChangeDeleted, cutoffDate);
        
        return stateChangeDeleted;
    }
}
