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
using Piranha.Notifications.Models;
using Piranha.Notifications.Repositories;

namespace Piranha.Notifications.Services;

/// <summary>
/// Default implementation of the audit service.
/// Focused on consuming messages and storing audit records.
/// </summary>
public sealed class NotificationsService : INotificationsService
{
    private readonly ILogger<NotificationsService> _logger;
    private readonly IStateChangedNotificationRepository _stateChangedNotificationRepository;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="stateChangedNotificationRepository">The state changed notification repository</param>
    public NotificationsService(
        ILogger<NotificationsService> logger,
        IStateChangedNotificationRepository stateChangedNotificationRepository)
    {
        _logger = logger;
        _stateChangedNotificationRepository = stateChangedNotificationRepository;
    }

    /// <inheritdoc />
    public async Task ProcessWorkflowStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing workflow state changed event for ContentId: {ContentId}, From: {FromState}, To: {ToState}", 
                stateChangedEvent.ContentId, stateChangedEvent.FromState, stateChangedEvent.ToState);

            // Create a new StateChangedNotification from the event
            var notification = new StateChangedNotification
            {
                Id = Guid.NewGuid(),
                Timestamp = stateChangedEvent.Timestamp != default ? stateChangedEvent.Timestamp : DateTime.UtcNow,
                ContentId = stateChangedEvent.ContentId,
                ContentName = stateChangedEvent.ContentName,
                FromState = stateChangedEvent.FromState,
                ToState = stateChangedEvent.ToState,
                TransitionDescription = stateChangedEvent.transitionDescription,
                ReviewedBy = stateChangedEvent.reviewedBy,
                Approved = stateChangedEvent.approved
            };

            // Save the notification to the database
            await _stateChangedNotificationRepository.SaveAsync(notification);

            _logger.LogInformation("Successfully saved workflow state changed notification with Id: {NotificationId}, approved: {Approved}", 
                notification.Id, notification.Approved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing workflow state changed event for ContentId: {ContentId}", stateChangedEvent.ContentId);
            throw; // Re-throw to allow further handling if needed
        }
    }
}
