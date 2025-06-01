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

namespace Piranha.Data.Audit;

/// <summary>
/// Entity Framework data model for state change records.
/// </summary>
[Serializable]
public sealed class StateChangeRecord
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

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
    public string transitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username for quick reference.
    /// </summary>
    public string approvedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets when the state change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets/sets optional comments or reasoning for the state change.
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
