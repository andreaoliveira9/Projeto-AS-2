/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

namespace Piranha.Data.EditorialWorkflow;

/// <summary>
/// Data model for WorkflowContentExtension
/// </summary>
[Serializable]
public sealed class WorkflowContentExtension
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the reference to the content item (matches Piranha content ID).
    /// </summary>
    public string ContentId { get; set; }

    /// <summary>
    /// Gets/sets the type of content for quick reference.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets/sets the current workflow instance ID (if any).
    /// </summary>
    public Guid? CurrentWorkflowInstanceId { get; set; }

    /// <summary>
    /// Gets/sets if content is currently in an active workflow.
    /// </summary>
    public bool IsInWorkflow { get; set; } = false;

    /// <summary>
    /// Gets/sets the last workflow state for quick access.
    /// </summary>
    public string LastWorkflowState { get; set; }

    /// <summary>
    /// Gets/sets when the extension was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets/sets when the extension was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets/sets the current workflow instance.
    /// </summary>
    public WorkflowInstance CurrentWorkflowInstance { get; set; }
}
