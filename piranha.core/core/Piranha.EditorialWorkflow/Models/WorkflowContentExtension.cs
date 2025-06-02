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
/// Extension model to link existing Piranha content with workflow instances.
/// This maintains separation from core CMS models.
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
    /// Gets/sets the current workflow instance ID (if any).
    /// </summary>
    public Guid? CurrentWorkflowInstanceId { get; set; }

    /// <summary>
    /// Gets/sets the current workflow instance.
    /// </summary>
    public WorkflowInstance CurrentWorkflowInstance { get; set; }
}