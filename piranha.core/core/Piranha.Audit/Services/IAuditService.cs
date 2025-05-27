/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Audit.Events;
using Piranha.Audit.Models;

namespace Piranha.Audit.Services;

/// <summary>
/// Service for handling audit and history operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Processes a workflow state changed event.
    /// </summary>
    /// <param name="stateChangedEvent">The state changed event to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a workflow state change.
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance id</param>
    /// <param name="contentId">The content id</param>
    /// <param name="contentType">The content type</param>
    /// <param name="fromState">The previous state</param>
    /// <param name="toState">The new state</param>
    /// <param name="userId">The user id</param>
    /// <param name="username">The username</param>
    /// <param name="comments">Optional comments</param>
    /// <param name="transitionRuleId">The transition rule id</param>
    /// <param name="isAutomaticTransition">Whether the transition was automatic</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="success">Whether the action was successful</param>
    /// <param name="errorMessage">Error message if failed</param>
    Task LogStateChangeAsync(
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
        string errorMessage = null);

    /// <summary>
    /// Gets state change history for content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetStateChangeHistoryAsync(Guid contentId);

    /// <summary>
    /// Performs cleanup of old audit records.
    /// </summary>
    /// <param name="retentionDays">Number of days to retain</param>
    /// <returns>Number of deleted records</returns>
    Task<int> CleanupOldRecordsAsync(int retentionDays = 365);
}
