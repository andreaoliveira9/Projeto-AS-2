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
/// Data model for WorkflowDefinition
/// </summary>
[Serializable]
public sealed class WorkflowDefinition
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the workflow name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets/sets the optional description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets/sets if this workflow is currently active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets/sets the version of the workflow for tracking changes.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets/sets when the workflow was initially created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets/sets when the workflow was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets/sets the user id who created this workflow.
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets/sets the user id who last modified this workflow.
    /// </summary>
    public string LastModifiedBy { get; set; }

    /// <summary>
    /// Gets/sets the available states for this workflow.
    /// </summary>
    public IList<WorkflowState> States { get; set; } = new List<WorkflowState>();

    /// <summary>
    /// Gets/sets the workflow instances using this definition.
    /// </summary>
    public IList<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}
