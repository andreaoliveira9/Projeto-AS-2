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
/// Represents a state within a workflow definition
/// </summary>
[Serializable]
public sealed class WorkflowState
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the unique identifier within the workflow (e.g., "draft", "review").
    /// </summary>
    public string StateId { get; set; }

    /// <summary>
    /// Gets/sets the display name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets/sets the optional description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets/sets if this is the initial state when a workflow instance is created.
    /// </summary>
    public bool IsInitial { get; set; } = false;

    /// <summary>
    /// Gets/sets if content in this state is considered published/live.
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// Gets/sets if this is a final state (no outgoing transitions).
    /// </summary>
    public bool IsFinal { get; set; } = false;

    /// <summary>
    /// Gets/sets the display order for UI purposes.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets/sets the optional color code for UI visualization (hex format).
    /// </summary>
    public string ColorCode { get; set; }

    /// <summary>
    /// Gets/sets when the state was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets/sets the workflow definition id.
    /// </summary>
    public Guid WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Gets/sets the workflow definition.
    /// </summary>
    public WorkflowDefinition WorkflowDefinition { get; set; }

    /// <summary>
    /// Gets/sets the outgoing transitions from this state.
    /// </summary>
    public IList<TransitionRule> OutgoingTransitions { get; set; } = new List<TransitionRule>();

    /// <summary>
    /// Gets/sets the incoming transitions to this state.
    /// </summary>
    public IList<TransitionRule> IncomingTransitions { get; set; } = new List<TransitionRule>();

    /// <summary>
    /// Gets/sets the workflow instances currently in this state.
    /// </summary>
    public IList<WorkflowInstance> CurrentInstances { get; set; } = new List<WorkflowInstance>();
}
