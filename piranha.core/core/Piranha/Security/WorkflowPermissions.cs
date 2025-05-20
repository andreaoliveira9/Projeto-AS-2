/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

namespace Piranha.Security;

/// <summary>
/// The available workflow permissions.
/// </summary>
public static class WorkflowPermission
{
    // Workflow management permissions
    public const string ManageWorkflows = "PiranhaWorkflowManage";
    public const string EditWorkflow = "PiranhaWorkflowEdit";
    public const string DeleteWorkflow = "PiranhaWorkflowDelete";
    
    // Content workflow permissions - general
    public const string ViewWorkflowState = "PiranhaWorkflowStateView";
    
    // Specific state transition permissions
    public const string TransitionToDraft = "PiranhaWorkflowTransitionToDraft";
    public const string TransitionToReview = "PiranhaWorkflowTransitionToReview";
    public const string TransitionToLegalReview = "PiranhaWorkflowTransitionToLegalReview";
    public const string TransitionToApproved = "PiranhaWorkflowTransitionToApproved";
    public const string TransitionToPublished = "PiranhaWorkflowTransitionToPublished";
    public const string TransitionToArchived = "PiranhaWorkflowTransitionToArchived";
    
    // Directional transition permissions
    public const string AdvanceWorkflow = "PiranhaWorkflowAdvance"; // Move forward in the workflow
    public const string RevertWorkflow = "PiranhaWorkflowRevert";   // Send back to previous state
    public const string SkipWorkflowSteps = "PiranhaWorkflowSkip";  // Skip steps in the workflow
    
    /// <summary>
    /// Gets all workflow management permissions.
    /// </summary>
    /// <returns>All management permissions</returns>
    public static string[] AllManagement()
    {
        return new []
        {
            ManageWorkflows,
            EditWorkflow,
            DeleteWorkflow
        };
    }
    
    /// <summary>
    /// Gets all workflow state view permissions.
    /// </summary>
    /// <returns>All view permissions</returns>
    public static string[] AllView()
    {
        return new []
        {
            ViewWorkflowState
        };
    }
    
    /// <summary>
    /// Gets all workflow state transition permissions.
    /// </summary>
    /// <returns>All transition permissions</returns>
    public static string[] AllTransitions()
    {
        return new []
        {
            TransitionToDraft,
            TransitionToReview,
            TransitionToLegalReview,
            TransitionToApproved,
            TransitionToPublished,
            TransitionToArchived,
            AdvanceWorkflow,
            RevertWorkflow,
            SkipWorkflowSteps
        };
    }
    
    /// <summary>
    /// Gets all workflow permissions.
    /// </summary>
    /// <returns>All permissions</returns>
    public static string[] All()
    {
        return AllManagement()
            .Concat(AllView())
            .Concat(AllTransitions())
            .ToArray();
    }

    /// <summary>
    /// Gets the transition permission for the specified state.
    /// </summary>
    /// <param name="stateId">The state id</param>
    /// <returns>The permission name</returns>
    public static string GetTransitionPermission(string stateId)
    {
        return stateId switch
        {
            "draft" => TransitionToDraft,
            "review" => TransitionToReview,
            "legal_review" => TransitionToLegalReview,
            "approved" => TransitionToApproved,
            "published" => TransitionToPublished,
            "archived" => TransitionToArchived,
            _ => null
        };
    }
}
