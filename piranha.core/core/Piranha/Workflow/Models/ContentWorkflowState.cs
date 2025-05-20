/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

namespace Piranha.Workflow.Models;

/// <summary>
/// Represents the workflow state of a content item.
/// </summary>
[Serializable]
public class ContentWorkflowState
{
    /// <summary>
    /// Gets/sets the content ID.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the workflow name.
    /// </summary>
    public string WorkflowName { get; set; }

    /// <summary>
    /// Gets/sets the current state ID.
    /// </summary>
    public string CurrentStateId { get; set; }

    /// <summary>
    /// Gets/sets the current state name.
    /// </summary>
    public string CurrentStateName { get; set; }

    /// <summary>
    /// Gets/sets the date the state was last changed.
    /// </summary>
    public DateTime StateChangedAt { get; set; }

    /// <summary>
    /// Gets/sets the user who last changed the state.
    /// </summary>
    public string StateChangedBy { get; set; }

    /// <summary>
    /// Gets/sets the workflow history.
    /// </summary>
    public List<WorkflowHistoryEntry> History { get; set; } = new List<WorkflowHistoryEntry>();
}

/// <summary>
/// Represents an entry in the workflow history.
/// </summary>
[Serializable]
public class WorkflowHistoryEntry
{
    /// <summary>
    /// Gets/sets the from state ID.
    /// </summary>
    public string FromStateId { get; set; }

    /// <summary>
    /// Gets/sets the from state name.
    /// </summary>
    public string FromStateName { get; set; }

    /// <summary>
    /// Gets/sets the to state ID.
    /// </summary>
    public string ToStateId { get; set; }

    /// <summary>
    /// Gets/sets the to state name.
    /// </summary>
    public string ToStateName { get; set; }

    /// <summary>
    /// Gets/sets the transition date.
    /// </summary>
    public DateTime TransitionedAt { get; set; }

    /// <summary>
    /// Gets/sets the user who performed the transition.
    /// </summary>
    public string TransitionedBy { get; set; }

    /// <summary>
    /// Gets/sets optional comment for the transition.
    /// </summary>
    public string Comment { get; set; }
}
