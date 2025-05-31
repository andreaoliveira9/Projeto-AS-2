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
using Piranha.Notifications.Events;

namespace Piranha.Notifications.Services;

/// <summary>
/// Default implementation of the audit service.
/// Focused on consuming messages and storing audit records.
/// </summary>
public sealed class NotificationsService : INotificationsService
{
    private readonly ILogger<NotificationsService> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">The logger</param>
    public NotificationsService(
        ILogger<NotificationsService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Log the event for now, actual processing logic can be added later
            _logger.LogInformation("Processing workflow state changed event: {Event}", stateChangedEvent);
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing workflow state changed event: {Event}", stateChangedEvent);
            throw; // Re-throw to allow further handling if needed
        }
    }
}
