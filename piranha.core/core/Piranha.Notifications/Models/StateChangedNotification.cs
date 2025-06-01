/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

namespace Piranha.Notifications.Models;

/// <summary>
/// Entity Framework data model for state change records.
/// </summary>
[Serializable]
public sealed class StateChangedNotification : Notification
{
    /// <summary>
    /// Gets/sets the content id that underwent the state change.
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
    /// Gets/sets the transition rule id that triggered this change.
    /// </summary>
    public string TransitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username for quick reference.
    /// </summary>
    public string ApprovedBy { get; set; } = string.Empty;
}