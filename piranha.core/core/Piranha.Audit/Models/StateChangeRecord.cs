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

namespace Piranha.Audit.Models;

/// <summary>
/// Specialized audit record for workflow state transitions.
/// </summary>
[Serializable]
public sealed class StateChangeRecord
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content type (e.g., "Page", "Post").
    /// </summary>
    public string ContentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the previous state.
    /// </summary>
    public string FromState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the new state.
    /// </summary>
    public string ToState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the transition rule id that triggered this change.
    /// </summary>
    public string transitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username who reviewed/performed the action.
    /// </summary>
    public string reviewedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets whether the transition was approved (true) or rejected (false).
    /// </summary>
    public bool approved { get; set; }

    /// <summary>
    /// Gets/sets when the state change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets/sets optional comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets/sets whether the action was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets/sets the error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
