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

namespace Piranha.EditorialWorkflow.Services;

/// <summary>
/// Interface for publishing workflow state change messages to message queue.
/// </summary>
public interface IWorkflowMessagePublisher
{
    /// <summary>
    /// Publishes a workflow state changed event to the message queue.
    /// </summary>
    /// <param name="stateChangedEvent">The state changed event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> PublishStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default);
}
