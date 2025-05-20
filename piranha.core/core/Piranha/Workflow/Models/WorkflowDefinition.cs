/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System.Text.Json.Serialization;

namespace Piranha.Workflow.Models;

/// <summary>
/// Represents the definition of an editorial workflow.
/// </summary>
[Serializable]
public class WorkflowDefinition
{
    /// <summary>
    /// Gets/sets the unique name of the workflow.
    /// </summary>
    public string WorkflowName { get; set; }

    /// <summary>
    /// Gets/sets the description of the workflow.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets/sets the states available in this workflow.
    /// </summary>
    public List<WorkflowState> States { get; set; } = new List<WorkflowState>();

    /// <summary>
    /// Gets the initial state of the workflow.
    /// </summary>
    [JsonIgnore]
    public WorkflowState InitialState => States.FirstOrDefault(s => s.IsInitial);

    /// <summary>
    /// Gets the published state of the workflow.
    /// </summary>
    [JsonIgnore]
    public WorkflowState PublishedState => States.FirstOrDefault(s => s.IsPublished);
    
    /// <summary>
    /// Gets a specific state by its ID.
    /// </summary>
    /// <param name="stateId">The state ID</param>
    /// <returns>The workflow state</returns>
    public WorkflowState GetState(string stateId)
    {
        return States.FirstOrDefault(s => s.Id == stateId);
    }
}

/// <summary>
/// Represents a state in an editorial workflow.
/// </summary>
[Serializable]
public class WorkflowState
{
    /// <summary>
    /// Gets/sets the unique id of the state.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets/sets the display name of the state.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets/sets the description of the state.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets/sets if this is the initial state of the workflow.
    /// </summary>
    public bool IsInitial { get; set; }

    /// <summary>
    /// Gets/sets if content in this state is considered published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets/sets the available transitions from this state.
    /// </summary>
    public List<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
    
    /// <summary>
    /// Checks if the given role has permission to transition to the specified state.
    /// </summary>
    /// <param name="toStateId">The target state ID</param>
    /// <param name="role">The role name</param>
    /// <returns>If the transition is allowed</returns>
    public bool CanTransitionTo(string toStateId, string role)
    {
        var transition = Transitions.FirstOrDefault(t => t.ToState == toStateId);
        return transition != null && transition.Roles.Contains(role);
    }
}

/// <summary>
/// Represents a transition between workflow states.
/// </summary>
[Serializable]
public class WorkflowTransition
{
    /// <summary>
    /// Gets/sets the target state ID.
    /// </summary>
    public string ToState { get; set; }

    /// <summary>
    /// Gets/sets the roles that are allowed to perform this transition.
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Gets/sets an optional comment about this transition.
    /// </summary>
    public string Comment { get; set; }
}
