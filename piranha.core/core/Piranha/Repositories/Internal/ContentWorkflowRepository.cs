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

namespace Piranha.Repositories.Internal;

/// <summary>
/// In-memory implementation of the content workflow repository.
/// </summary>
public class ContentWorkflowRepository : IContentWorkflowRepository
{
    private readonly Dictionary<Guid, ContentWorkflowState> _states = new Dictionary<Guid, ContentWorkflowState>();

    /// <summary>
    /// Gets the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The workflow state</returns>
    public Task<ContentWorkflowState> GetByContentIdAsync(Guid contentId)
    {
        if (_states.TryGetValue(contentId, out var state))
        {
            return Task.FromResult(state);
        }
        return Task.FromResult<ContentWorkflowState>(null);
    }

    /// <summary>
    /// Saves the workflow state.
    /// </summary>
    /// <param name="state">The workflow state</param>
    public Task SaveAsync(ContentWorkflowState state)
    {
        if (state != null)
        {
            _states[state.ContentId] = state;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes the workflow state for the specified content.
    /// </summary>
    /// <param name="contentId">The content id</param>
    public Task DeleteAsync(Guid contentId)
    {
        _states.Remove(contentId);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Gets all content items in the specified state.
    /// </summary>
    /// <param name="stateId">The state id</param>
    /// <returns>All content items</returns>
    public Task<IEnumerable<ContentWorkflowState>> GetByStateAsync(string stateId)
    {
        var result = _states.Values
            .Where(s => s.CurrentStateId == stateId)
            .ToList();
        
        return Task.FromResult<IEnumerable<ContentWorkflowState>>(result);
    }
    
    /// <summary>
    /// Gets all content workflow states.
    /// </summary>
    /// <returns>All content workflow states</returns>
    public Task<IEnumerable<ContentWorkflowState>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<ContentWorkflowState>>(_states.Values.ToList());
    }
}
