/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.ComponentModel.DataAnnotations;

namespace Piranha.Models;

/// <summary>
/// Represents the workflow state for a piece of content.
/// </summary>
[Serializable]
public class ContentWorkflowState
{
    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the workflow name.
    /// </summary>
    [StringLength(64)]
    public string WorkflowName { get; set; }

    /// <summary>
    /// Gets/sets the current state id.
    /// </summary>
    [StringLength(64)]
    public string CurrentStateId { get; set; }

    /// <summary>
    /// Gets/sets when the state was last changed.
    /// </summary>
    public DateTime StateChangedAt { get; set; }

    /// <summary>
    /// Gets/sets who last changed the state.
    /// </summary>
    [StringLength(128)]
    public string StateChangedBy { get; set; }

    /// <summary>
    /// Gets/sets any comment provided with the last state change.
    /// </summary>
    public string StateChangeComment { get; set; }

    /// <summary>
    /// Gets/sets the history of state transitions.
    /// </summary>
    public IList<WorkflowStateTransition> History { get; set; } = new List<WorkflowStateTransition>();
}

/// <summary>
/// Represents a historical state transition in a workflow.
/// </summary>
[Serializable]
public class WorkflowStateTransition
{
    /// <summary>
    /// Gets/sets the from state id.
    /// </summary>
    [StringLength(64)]
    public string FromStateId { get; set; }

    /// <summary>
    /// Gets/sets the to state id.
    /// </summary>
    [StringLength(64)]
    public string ToStateId { get; set; }

    /// <summary>
    /// Gets/sets when the transition occurred.
    /// </summary>
    public DateTime TransitionedAt { get; set; }

    /// <summary>
    /// Gets/sets who performed the transition.
    /// </summary>
    [StringLength(128)]
    public string TransitionedBy { get; set; }

    /// <summary>
    /// Gets/sets any comment provided with the transition.
    /// </summary>
    public string Comment { get; set; }
}
