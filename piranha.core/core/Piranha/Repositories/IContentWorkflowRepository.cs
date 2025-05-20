/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Workflow.Models;

namespace Piranha.Repositories;

/// <summary>
/// Interface for the content workflow repository.
/// </summary>
public interface IContentWorkflowRepository
{
    /// <summary>
    /// Gets the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow state</returns>
    Task<ContentWorkflowState> GetByContentIdAsync(Guid contentId);

    /// <summary>
    /// Saves the workflow state.
    /// </summary>
    /// <param name="state">The workflow state</param>
    Task SaveAsync(ContentWorkflowState state);

    /// <summary>
    /// Deletes the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    Task DeleteAsync(Guid contentId);
    
    /// <summary>
    /// Gets all content items in the specified state.
    /// </summary>
    /// <param name="stateId">The state id</param>
    /// <returns>All content items</returns>
    Task<IEnumerable<ContentWorkflowState>> GetByStateAsync(string stateId);
    
    /// <summary>
    /// Gets all content workflow states.
    /// </summary>
    /// <returns>All content workflow states</returns>
    Task<IEnumerable<ContentWorkflowState>> GetAllAsync();
}
