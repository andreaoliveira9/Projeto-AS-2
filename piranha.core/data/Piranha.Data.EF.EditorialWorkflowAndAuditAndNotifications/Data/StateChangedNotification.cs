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

namespace Piranha.Data.Notifications;

/// <summary>
/// Entity Framework data model for state change notifications.
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
    /// Gets/sets the previous state of the content.
    /// </summary>
    public string Comments { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the transition rule id that triggered this change.
    /// </summary>
    public string TransitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username who reviewed/performed the action.
    /// </summary>
    public string ReviewedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets whether the transition was approved (true) or rejected (false).
    /// </summary>
    public bool Approved { get; set; }
}
