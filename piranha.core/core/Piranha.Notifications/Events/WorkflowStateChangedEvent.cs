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

namespace Piranha.Notifications.Events;

/// <summary>
/// Event triggered when a workflow state transition occurs.
/// </summary>
[Serializable]
public sealed class WorkflowStateChangedEvent
{
    /// <summary>
    /// Gets/sets the event id.
    /// </summary>
    public Guid EventId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets/sets the workflow instance id.
    /// </summary>
    public Guid WorkflowInstanceId { get; set; }

    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the previous state.
    /// </summary>
    public string FromState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the new state.
    /// </summary>
    public string ToState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the user who triggered the state change.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username for quick reference.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets when the state change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets/sets optional comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets/sets the transition rule id.
    /// </summary>
    public Guid? TransitionRuleId { get; set; }

    /// <summary>
    /// Gets/sets whether the transition was automatic.
    /// </summary>
    public bool IsAutomaticTransition { get; set; }

    /// <summary>
    /// Gets/sets additional metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets/sets whether the action was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets/sets the error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
