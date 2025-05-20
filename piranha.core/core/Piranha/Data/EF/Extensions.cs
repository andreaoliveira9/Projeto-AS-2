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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Piranha.Data.EF;

/// <summary>
/// Extension methods for EF Core.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Temporary extension method for FirstOrDefaultAsync until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>The matching item</returns>
    public static Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return Task.FromResult(source.FirstOrDefault(predicate));
    }

    /// <summary>
    /// Temporary extension method for FirstOrDefaultAsync without predicate until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <param name="source">The source collection</param>
    /// <returns>The first item</returns>
    public static Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> source)
    {
        return Task.FromResult(source.FirstOrDefault());
    }

    /// <summary>
    /// Temporary extension method for ToListAsync until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <param name="source">The source collection</param>
    /// <returns>The list</returns>
    public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> source)
    {
        return Task.FromResult(source.ToList());
    }

    /// <summary>
    /// Temporary extension method for Where until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The source type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="predicate">The predicate</param>
    /// <returns>The filtered collection</returns>
    public static Task<IEnumerable<T>> WhereAsync<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return Task.FromResult(source.Where(predicate));
    }

    /// <summary>
    /// Temporary extension method for AddAsync until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="entity">The entity</param>
    /// <returns>A task</returns>
    public static Task AddAsync<T>(this ICollection<T> source, T entity)
    {
        source.Add(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Temporary extension method for AddRangeAsync until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="entities">The entities</param>
    /// <returns>A task</returns>
    public static Task AddRangeAsync<T>(this ICollection<T> source, IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            source.Add(entity);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Temporary extension method for RemoveRange until EF Core is properly referenced.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="source">The source collection</param>
    /// <param name="entities">The entities</param>
    public static void RemoveRange<T>(this ICollection<T> source, IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            source.Remove(entity);
        }
    }
}
