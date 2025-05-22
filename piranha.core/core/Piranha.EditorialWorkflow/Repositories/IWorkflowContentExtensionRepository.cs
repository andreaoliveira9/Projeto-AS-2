/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.EditorialWorkflow.Models;

namespace Piranha.EditorialWorkflow.Repositories;

/// <summary>
/// Repository interface for WorkflowContentExtension operations
/// </summary>
public interface IWorkflowContentExtensionRepository
{
    /// <summary>
    /// Gets the workflow extension for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow content extension</returns>
    Task<WorkflowContentExtension> GetByContentId(string contentId);

    /// <summary>
    /// Gets all content currently in active workflows.
    /// </summary>
    /// <returns>The workflow content extensions</returns>
    Task<IEnumerable<WorkflowContentExtension>> GetActiveWorkflows();

    /// <summary>
    /// Gets all workflow extensions for the specified content type.
    /// </summary>
    /// <param name="contentType">The content type</param>
    /// <returns>The workflow content extensions</returns>
    Task<IEnumerable<WorkflowContentExtension>> GetByContentType(string contentType);

    /// <summary>
    /// Saves the given workflow content extension.
    /// </summary>
    /// <param name="extension">The workflow content extension</param>
    Task Save(WorkflowContentExtension extension);

    /// <summary>
    /// Deletes the workflow extension for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    Task Delete(string contentId);

    /// <summary>
    /// Checks if a workflow extension exists for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>True if an extension exists</returns>
    Task<bool> Exists(string contentId);
}
