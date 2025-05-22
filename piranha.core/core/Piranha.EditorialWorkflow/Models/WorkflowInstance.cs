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
/// Represents an active instance of a workflow applied to specific content
/// </summary>
[Serializable]
public sealed class WorkflowInstance
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the reference to the content item.
    /// </summary>
    public string ContentId { get; set; }

    /// <summary>
    /// Gets/sets the type of content (Page, Post, etc.) for reference.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets/sets the content title at the time of workflow creation (for reference).
    /// </summary>
    public string ContentTitle { get; set; }

    /// <summary>
    /// Gets/sets the current status of the workflow instance.
    /// </summary>
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Active;

    /// <summary>
    /// Gets/sets the user who initiated this workflow instance.
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets/sets when the workflow instance was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets/sets when the workflow instance was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets/sets when the workflow instance was completed (if applicable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets/sets additional metadata as JSON.
    /// </summary>
    public string Metadata { get; set; }

    /// <summary>
    /// Gets/sets the workflow definition id.
    /// </summary>
    public Guid WorkflowDefinitionId { get; set; }

    /// <summary>
    /// Gets/sets the current state id.
    /// </summary>
    public Guid CurrentStateId { get; set; }

    /// <summary>
    /// Gets/sets the workflow definition.
    /// </summary>
    public WorkflowDefinition WorkflowDefinition { get; set; }

    /// <summary>
    /// Gets/sets the current state.
    /// </summary>
    public WorkflowState CurrentState { get; set; }
}

/// <summary>
/// Status of a workflow instance
/// </summary>
public enum WorkflowInstanceStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3,
    OnHold = 4
}
