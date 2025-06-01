/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Notifications.Events;

namespace Piranha.Notifications.Services;

/// <summary>
/// Service for handling audit and history operations.
/// Focused on consuming messages and storing audit records.
/// </summary>
public interface INotificationsService
{
    /// <summary>
    /// Processes a workflow state changed event received from message queue.
    /// </summary>
    /// <param name="stateChangedEvent">The state changed event to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default);
}
