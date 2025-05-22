/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

namespace Piranha.EditorialWorkflow.Models;

/// <summary>
/// Defines a transition rule between two workflow states with role-based permissions
/// </summary>
[Serializable]
public sealed class TransitionRule
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the optional description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets/sets the comment or reason template for this transition.
    /// </summary>
    public string CommentTemplate { get; set; }

    /// <summary>
    /// Gets/sets if a comment is required when performing this transition.
    /// </summary>
    public bool RequiresComment { get; set; } = false;

    /// <summary>
    /// Gets/sets the JSON array of role names that can perform this transition.
    /// References existing Piranha roles.
    /// </summary>
    public string AllowedRoles { get; set; } = "[]";

    /// <summary>
    /// Gets/sets the display order for UI purposes.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets/sets if this transition is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets/sets when the transition rule was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets/sets the source state id.
    /// </summary>
    public Guid FromStateId { get; set; }

    /// <summary>
    /// Gets/sets the target state id.
    /// </summary>
    public Guid ToStateId { get; set; }

    /// <summary>
    /// Gets/sets the source state.
    /// </summary>
    public WorkflowState FromState { get; set; }

    /// <summary>
    /// Gets/sets the target state.
    /// </summary>
    public WorkflowState ToState { get; set; }
}
