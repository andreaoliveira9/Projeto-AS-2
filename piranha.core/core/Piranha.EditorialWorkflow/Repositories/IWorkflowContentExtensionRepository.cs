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
/// Repository interface for managing workflow content extensions.
/// </summary>
public interface IWorkflowContentExtensionRepository
{
    /// <summary>
    /// Gets the workflow extension for the specified content by ID, including the current instance,
    /// its definition, and current state.
    /// </summary>
    /// <param name="contentId">The unique content identifier.</param>
    /// <returns>The workflow content extension or null if not found.</returns>
    Task<WorkflowContentExtension> GetByContentId(string contentId);

    /// <summary>
    /// Gets all content items that currently have an active workflow instance,
    /// ordered by last modified descending.
    /// </summary>
    /// <returns>A collection of workflow content extensions with active workflows.</returns>
    Task<IEnumerable<WorkflowContentExtension>> GetActiveWorkflows();

    /// <summary>
    /// Gets all workflow content extensions for the specified content type
    /// with active workflow instances, ordered by last modified descending.
    /// </summary>
    /// <param name="contentType">The content type name.</param>
    /// <returns>A collection of matching workflow content extensions.</returns>
    Task<IEnumerable<WorkflowContentExtension>> GetByContentType(string contentType);

    /// <summary>
    /// Saves or updates the specified workflow content extension.
    /// If the content ID doesn't exist, a new record is created.
    /// </summary>
    /// <param name="extension">The workflow content extension to save.</param>
    Task Save(WorkflowContentExtension extension);

    /// <summary>
    /// Deletes the workflow extension associated with the given content ID, if it exists.
    /// </summary>
    /// <param name="contentId">The content ID of the extension to delete.</param>
    Task Delete(string contentId);

    /// <summary>
    /// Determines whether a workflow content extension exists for the given content ID.
    /// </summary>
    /// <param name="contentId">The content ID to check.</param>
    /// <returns>True if the extension exists; otherwise, false.</returns>
    Task<bool> Exists(string contentId);
}