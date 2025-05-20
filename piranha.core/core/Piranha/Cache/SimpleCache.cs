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
using System.Threading;
using System.Threading.Tasks;

namespace Piranha.Cache;

/// <summary>
/// Simple in-memory cache implementation.
/// </summary>
public class SimpleCache : ICache
{
    private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

    /// <summary>
    /// Gets the specified object from the cache.
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="key">The unique key</param>
    /// <param name="cancellationToken">An optional cancelation token</param>
    /// <returns>The cached object, null if not found</returns>
    public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return Task.FromResult((T)value);
        }
        return Task.FromResult<T>(default);
    }

    /// <summary>
    /// Sets the specified object in the cache.
    /// </summary>
    /// <typeparam name="T">The type of object</typeparam>
    /// <param name="key">The unique key</param>
    /// <param name="value">The object to cache</param>
    /// <param name="cancellationToken">An optional cancelation token</param>
    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        _cache[key] = value;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the specified object from the cache.
    /// </summary>
    /// <param name="key">The unique key</param>
    /// <param name="cancellationToken">An optional cancelation token</param>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.ContainsKey(key))
        {
            _cache.Remove(key);
        }
        return Task.CompletedTask;
    }
}
