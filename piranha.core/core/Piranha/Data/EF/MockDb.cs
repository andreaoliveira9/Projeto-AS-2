/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Piranha.Data.EF;

/// <summary>
/// Mock implementation of IDb for development purposes.
/// </summary>
public class MockDb : IDb
{
    private readonly Dictionary<Type, object> _collections = new Dictionary<Type, object>();

    /// <summary>
    /// Gets/sets the content workflow states.
    /// </summary>
    public IEnumerable<ContentWorkflowState> ContentWorkflowStates
    {
        get => GetCollection<ContentWorkflowState>();
        set => _collections[typeof(ContentWorkflowState)] = value;
    }

    /// <summary>
    /// Gets/sets the content workflow state transitions.
    /// </summary>
    public IEnumerable<ContentWorkflowStateTransition> ContentWorkflowStateTransitions
    {
        get => GetCollection<ContentWorkflowStateTransition>();
        set => _collections[typeof(ContentWorkflowStateTransition)] = value;
    }

    /// <summary>
    /// Gets a collection of the given type.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>The collection</returns>
    private IEnumerable<T> GetCollection<T>()
    {
        if (_collections.TryGetValue(typeof(T), out var collection))
        {
            return (IEnumerable<T>)collection;
        }
        
        var newCollection = new List<T>();
        _collections[typeof(T)] = newCollection;
        return newCollection;
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <returns>The number of rows affected</returns>
    public Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }
}
