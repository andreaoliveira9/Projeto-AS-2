/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Models;

namespace Piranha.Manager.Models;

/// <summary>
/// View model for the workflow state information.
/// </summary>
public class WorkflowStateViewModel
{
    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the workflow name.
    /// </summary>
    public string WorkflowName { get; set; }

    /// <summary>
    /// Gets/sets the current state id.
    /// </summary>
    public string CurrentStateId { get; set; }

    /// <summary>
    /// Gets/sets the current state name.
    /// </summary>
    public string CurrentStateName { get; set; }

    /// <summary>
    /// Gets/sets if the current state is published.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets/sets when the state was last changed.
    /// </summary>
    public DateTime StateChangedAt { get; set; }

    /// <summary>
    /// Gets/sets who last changed the state.
    /// </summary>
    public string StateChangedBy { get; set; }

    /// <summary>
    /// Gets/sets any comment provided with the last state change.
    /// </summary>
    public string StateChangeComment { get; set; }

    /// <summary>
    /// Gets/sets the available transitions.
    /// </summary>
    public IList<WorkflowTransitionModel> AvailableTransitions { get; set; } = new List<WorkflowTransitionModel>();

    /// <summary>
    /// Gets/sets the state history.
    /// </summary>
    public IList<WorkflowStateHistoryItem> History { get; set; } = new List<WorkflowStateHistoryItem>();

    /// <summary>
    /// Creates a new workflow state view model from the given state and workflow.
    /// </summary>
    /// <param name="state">The workflow state</param>
    /// <param name="workflow">The workflow definition</param>
    /// <returns>The view model</returns>
    public static WorkflowStateViewModel Create(ContentWorkflowState state, Workflow.Models.WorkflowDefinition workflow)
    {
        if (state == null || workflow == null)
        {
            return null;
        }

        var currentState = workflow.GetState(state.CurrentStateId);
        
        var model = new WorkflowStateViewModel
        {
            ContentId = state.ContentId,
            WorkflowName = state.WorkflowName,
            CurrentStateId = state.CurrentStateId,
            CurrentStateName = currentState?.Name ?? state.CurrentStateId,
            IsPublished = currentState?.IsPublished ?? false,
            StateChangedAt = state.StateChangedAt,
            StateChangedBy = state.StateChangedBy,
            StateChangeComment = state.StateChangeComment
        };

        // Add history items
        foreach (var transition in state.History.OrderByDescending(h => h.TransitionedAt))
        {
            var fromState = transition.FromStateId != null ? workflow.GetState(transition.FromStateId) : null;
            var toState = workflow.GetState(transition.ToStateId);
            
            model.History.Add(new WorkflowStateHistoryItem
            {
                FromStateId = transition.FromStateId,
                FromStateName = fromState?.Name ?? transition.FromStateId,
                ToStateId = transition.ToStateId,
                ToStateName = toState?.Name ?? transition.ToStateId,
                TransitionedAt = transition.TransitionedAt,
                TransitionedBy = transition.TransitionedBy,
                Comment = transition.Comment
            });
        }

        return model;
    }
}

/// <summary>
/// View model for a workflow state history item.
/// </summary>
public class WorkflowStateHistoryItem
{
    /// <summary>
    /// Gets/sets the from state id.
    /// </summary>
    public string FromStateId { get; set; }

    /// <summary>
    /// Gets/sets the from state name.
    /// </summary>
    public string FromStateName { get; set; }

    /// <summary>
    /// Gets/sets the to state id.
    /// </summary>
    public string ToStateId { get; set; }

    /// <summary>
    /// Gets/sets the to state name.
    /// </summary>
    public string ToStateName { get; set; }

    /// <summary>
    /// Gets/sets when the transition occurred.
    /// </summary>
    public DateTime TransitionedAt { get; set; }

    /// <summary>
    /// Gets/sets who performed the transition.
    /// </summary>
    public string TransitionedBy { get; set; }

    /// <summary>
    /// Gets/sets any comment provided with the transition.
    /// </summary>
    public string Comment { get; set; }
}

/// <summary>
/// View model for a workflow definition.
/// </summary>
public class WorkflowDefinitionViewModel
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
    public List<WorkflowStateViewModel> States { get; set; } = new List<WorkflowStateViewModel>();

    /// <summary>
    /// Creates a new workflow definition view model from the given workflow.
    /// </summary>
    /// <param name="workflow">The workflow definition</param>
    /// <returns>The view model</returns>
    public static WorkflowDefinitionViewModel Create(Workflow.Models.WorkflowDefinition workflow)
    {
        if (workflow == null)
        {
            return null;
        }

        var model = new WorkflowDefinitionViewModel
        {
            WorkflowName = workflow.WorkflowName,
            Description = workflow.Description
        };

        // Add state items
        foreach (var state in workflow.States)
        {
            var stateModel = new WorkflowStateViewModel
            {
                CurrentStateId = state.Id,
                CurrentStateName = state.Name,
                IsPublished = state.IsPublished
            };

            // Add available transitions
            foreach (var transition in state.Transitions)
            {
                var targetState = workflow.GetState(transition.ToState);

                stateModel.AvailableTransitions.Add(new WorkflowTransitionModel
                {
                    ToStateId = transition.ToState,
                    ToStateName = targetState?.Name,
                    Comment = transition.Comment,
                    Roles = transition.Roles.ToList()
                });
            }

            model.States.Add(stateModel);
        }

        return model;
    }
}

/// <summary>
/// View model for a workflow transition.
/// </summary>
public class WorkflowTransitionModel
{
    /// <summary>
    /// Gets/sets the target state id.
    /// </summary>
    public string ToStateId { get; set; }

    /// <summary>
    /// Gets/sets the target state name.
    /// </summary>
    public string ToStateName { get; set; }

    /// <summary>
    /// Gets/sets the comment for the transition.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Gets/sets the roles allowed to perform this transition.
    /// </summary>
    public List<string> Roles { get; set; } = new List<string>();
}

/// <summary>
/// Model for a content item with workflow information.
/// </summary>
public class ContentWorkflowViewModel
{
    /// <summary>
    /// Gets/sets the content id.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content title.
    /// </summary>
    public string ContentTitle { get; set; }

    /// <summary>
    /// Gets/sets the content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets/sets the current workflow state.
    /// </summary>
    public WorkflowStateViewModel WorkflowState { get; set; }
}
