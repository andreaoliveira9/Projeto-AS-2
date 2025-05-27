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
/// Focused on consuming messages and storing audit records.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Processes a workflow state changed event received from message queue.
    /// </summary>
    /// <param name="stateChangedEvent">The state changed event to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets state change history for content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetStateChangeHistoryAsync(Guid contentId);
}
