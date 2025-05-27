/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Piranha.Audit.Models;

namespace Piranha.Audit.Repositories;

/// <summary>
/// Repository for state change record operations.
/// Focused on saving audit records and retrieving by content.
/// </summary>
public interface IStateChangeRecordRepository
{
    /// <summary>
    /// Gets all state change records for a specific content item.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetByContentAsync(Guid contentId);

    /// <summary>
    /// Saves a state change record.
    /// </summary>
    /// <param name="stateChangeRecord">The state change record</param>
    Task SaveAsync(StateChangeRecord stateChangeRecord);
}
