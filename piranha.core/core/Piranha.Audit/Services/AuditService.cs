/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

#nullable enable

using Microsoft.Extensions.Logging;
using Piranha.Audit.Events;
using Piranha.Audit.Models;
using Piranha.Audit.Repositories;
using System.Text.Json;

namespace Piranha.Audit.Services;

/// <summary>
/// Default implementation of the audit service.
/// Focused on consuming messages and storing audit records.
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
            // Create state change record from event
            var stateChangeRecord = new StateChangeRecord
            {
                Id = Guid.NewGuid(),
                ContentId = stateChangedEvent.ContentId,
                ContentName = stateChangedEvent.ContentName,
                FromState = stateChangedEvent.FromState,
                ToState = stateChangedEvent.ToState,
                transitionDescription = stateChangedEvent.transitionDescription,
                reviewedBy = stateChangedEvent.reviewedBy,
                approved = stateChangedEvent.approved,
                Timestamp = stateChangedEvent.Timestamp,
                Comments = stateChangedEvent.Comments,
                Success = stateChangedEvent.Success,
                ErrorMessage = stateChangedEvent.ErrorMessage
            };

            await _stateChangeRecordRepository.SaveAsync(stateChangeRecord);
            
            _logger.LogInformation("State change logged for content {ContentId} from {FromState} to {ToState} by user {Username}, approved: {Approved}",
                stateChangedEvent.ContentId, stateChangedEvent.FromState, stateChangedEvent.ToState, stateChangedEvent.reviewedBy, stateChangedEvent.approved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process workflow state change event for content {ContentId}", stateChangedEvent.ContentId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StateChangeRecord>> GetStateChangeHistoryAsync(Guid contentId)
    {
        return await _stateChangeRecordRepository.GetByContentAsync(contentId);
    }
}
