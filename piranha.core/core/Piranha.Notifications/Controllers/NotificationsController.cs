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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piranha.Notifications.Models;
using Piranha.Notifications.Repositories;

namespace Piranha.Notifications.Controllers;

/// <summary>
/// API Controller for notifications functionality.
/// Provides endpoints to query and manage state change notifications.
/// </summary>
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IStateChangedNotificationRepository _notificationRepository;
    private readonly ILogger<NotificationsController> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="notificationRepository">The notification repository</param>
    /// <param name="logger">The logger</param>
    public NotificationsController(
        IStateChangedNotificationRepository notificationRepository, 
        ILogger<NotificationsController> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all state changed notifications.
    /// </summary>
    /// <returns>List of state changed notifications ordered by timestamp (newest first)</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StateChangedNotificationDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<StateChangedNotificationDto>>> GetAllNotifications()
    {
        try
        {
            _logger.LogInformation("Retrieving all state changed notifications");

            var notifications = await _notificationRepository.GetAllAsync();
            var dtos = notifications
                .OrderByDescending(n => n.Timestamp)
                .Select(MapToDto)
                .ToList();

            _logger.LogInformation("Retrieved {Count} state changed notifications", dtos.Count);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all state changed notifications");
            return StatusCode(500, "An error occurred while retrieving notifications");
        }
    }

    /// <summary>
    /// Gets all notifications for a specific content.
    /// </summary>
    /// <param name="contentId">The content ID</param>
    /// <returns>List of notifications for the specified content</returns>
    [HttpGet("content/{contentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<StateChangedNotificationDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<StateChangedNotificationDto>>> GetNotificationsByContentId(Guid contentId)
    {
        try
        {
            if (contentId == Guid.Empty)
            {
                return BadRequest("Content ID cannot be empty");
            }

            _logger.LogInformation("Retrieving notifications for content {ContentId}", contentId);

            var allNotifications = await _notificationRepository.GetAllAsync();
            var contentNotifications = allNotifications
                .Where(n => n.ContentId == contentId)
                .OrderByDescending(n => n.Timestamp)
                .Select(MapToDto)
                .ToList();

            if (!contentNotifications.Any())
            {
                _logger.LogInformation("No notifications found for content {ContentId}", contentId);
                return NotFound($"No notifications found for content {contentId}");
            }

            _logger.LogInformation("Retrieved {Count} notifications for content {ContentId}", 
                contentNotifications.Count, contentId);
            return Ok(contentNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for content {ContentId}", contentId);
            return StatusCode(500, "An error occurred while retrieving content notifications");
        }
    }

    /// <summary>
    /// Gets notifications summary information.
    /// </summary>
    /// <returns>Summary of notification statistics</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(NotificationsSummaryDto), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<NotificationsSummaryDto>> GetNotificationsSummary()
    {
        try
        {
            _logger.LogInformation("Retrieving notifications summary");

            var notifications = await _notificationRepository.GetAllAsync();
            var notificationsList = notifications.ToList();

            var summary = new NotificationsSummaryDto
            {
                TotalNotifications = notificationsList.Count,
                LastNotification = notificationsList.OrderByDescending(n => n.Timestamp).FirstOrDefault()?.Timestamp,
                UniqueContentCount = notificationsList.Select(n => n.ContentId).Distinct().Count(),
                StateTransitions = notificationsList
                    .GroupBy(n => new { n.FromState, n.ToState })
                    .Select(g => new StateTransitionSummary
                    {
                        FromState = g.Key.FromState,
                        ToState = g.Key.ToState,
                        Count = g.Count()
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList(),
                RecentNotifications = notificationsList
                    .OrderByDescending(n => n.Timestamp)
                    .Take(10)
                    .Select(MapToDto)
                    .ToList()
            };

            _logger.LogInformation("Generated notifications summary: {TotalNotifications} total notifications", 
                summary.TotalNotifications);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications summary");
            return StatusCode(500, "An error occurred while retrieving notifications summary");
        }
    }

    /// <summary>
    /// Maps a StateChangedNotification model to a DTO.
    /// </summary>
    /// <param name="notification">The notification model</param>
    /// <returns>The DTO representation</returns>
    private static StateChangedNotificationDto MapToDto(StateChangedNotification notification)
    {
        return new StateChangedNotificationDto
        {
            Id = notification.Id,
            Timestamp = notification.Timestamp,
            ContentId = notification.ContentId,
            ContentName = notification.ContentName,
            FromState = notification.FromState,
            Comments = notification.Comments,
            ToState = notification.ToState,
            TransitionDescription = notification.TransitionDescription,
            ReviewedBy = notification.ReviewedBy,
            ApprovedBy = notification.ReviewedBy, // For backward compatibility
            Approved = notification.Approved
        };
    }
}

/// <summary>
/// DTO for state changed notification data.
/// </summary>
public class StateChangedNotificationDto
{
    /// <summary>
    /// Gets/sets the notification ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the timestamp of the notification.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets/sets the content ID that underwent the state change.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content type (e.g., "Page", "Post").
    /// </summary>
    public string ContentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the previous state of the content.
    /// </summary>
    public string FromState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the new state of the content.
    /// </summary>
    public string ToState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the new state of the content.
    /// </summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the transition rule description that triggered this change.
    /// </summary>
    public string TransitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username who reviewed/performed the action.
    /// </summary>
    public string ReviewedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username for quick reference (backward compatibility).
    /// </summary>
    public string ApprovedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets whether the transition was approved (true) or rejected (false).
    /// </summary>
    public bool Approved { get; set; }
}

/// <summary>
/// DTO for creating a new state changed notification.
/// </summary>
public class CreateStateChangedNotificationDto
{
    /// <summary>
    /// Gets/sets the content ID that underwent the state change.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content type (e.g., "Page", "Post").
    /// </summary>
    public string ContentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the previous state of the content.
    /// </summary>
    public string? FromState { get; set; }

    /// <summary>
    /// Gets/sets the new state of the content.
    /// </summary>
    public string? ToState { get; set; }

    /// <summary>
    /// Gets/sets the transition rule description that triggered this change.
    /// </summary>
    public string? TransitionDescription { get; set; }

    /// <summary>
    /// Gets/sets the username who reviewed/performed the action.
    /// </summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Gets/sets the username for quick reference (backward compatibility).
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Gets/sets whether the transition was approved (true) or rejected (false).
    /// </summary>
    public bool Approved { get; set; }
}

/// <summary>
/// DTO for notifications summary information.
/// </summary>
public class NotificationsSummaryDto
{
    /// <summary>
    /// Gets/sets the total number of notifications.
    /// </summary>
    public int TotalNotifications { get; set; }

    /// <summary>
    /// Gets/sets the last notification timestamp.
    /// </summary>
    public DateTime? LastNotification { get; set; }

    /// <summary>
    /// Gets/sets the number of unique content items with notifications.
    /// </summary>
    public int UniqueContentCount { get; set; }

    /// <summary>
    /// Gets/sets state transition statistics.
    /// </summary>
    public List<StateTransitionSummary> StateTransitions { get; set; } = new();

    /// <summary>
    /// Gets/sets the most recent notifications (limited to 10).
    /// </summary>
    public List<StateChangedNotificationDto> RecentNotifications { get; set; } = new();
}

/// <summary>
/// DTO for state transition summary information.
/// </summary>
public class StateTransitionSummary
{
    /// <summary>
    /// Gets/sets the previous state.
    /// </summary>
    public string FromState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the new state.
    /// </summary>
    public string ToState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the count of this type of transition.
    /// </summary>
    public int Count { get; set; }
}
