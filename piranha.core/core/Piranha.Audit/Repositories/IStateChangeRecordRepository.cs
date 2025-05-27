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
/// </summary>
public interface IStateChangeRecordRepository
{
    /// <summary>
    /// Gets a state change record by id.
    /// </summary>
    /// <param name="id">The unique id</param>
    /// <returns>The state change record</returns>
    Task<StateChangeRecord> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all state change records for a specific workflow instance.
    /// </summary>
    /// <param name="workflowInstanceId">The workflow instance id</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetByWorkflowInstanceAsync(Guid workflowInstanceId);

    /// <summary>
    /// Gets all state change records for a specific content item.
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetByContentAsync(Guid contentId);

    /// <summary>
    /// Gets state change records for a specific user.
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="take">Number of records to take</param>
    /// <param name="skip">Number of records to skip</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetByUserAsync(string userId, int take = 50, int skip = 0);

    /// <summary>
    /// Gets state change records within a date range.
    /// </summary>
    /// <param name="from">Start date</param>
    /// <param name="to">End date</param>
    /// <param name="take">Number of records to take</param>
    /// <param name="skip">Number of records to skip</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetByDateRangeAsync(DateTime from, DateTime to, int take = 50, int skip = 0);

    /// <summary>
    /// Gets state change records by transition states.
    /// </summary>
    /// <param name="fromState">The from state</param>
    /// <param name="toState">The to state</param>
    /// <param name="take">Number of records to take</param>
    /// <param name="skip">Number of records to skip</param>
    /// <returns>The state change records</returns>
    Task<IEnumerable<StateChangeRecord>> GetByTransitionAsync(string fromState, string toState, int take = 50, int skip = 0);

    /// <summary>
    /// Saves a state change record.
    /// </summary>
    /// <param name="stateChangeRecord">The state change record</param>
    Task SaveAsync(StateChangeRecord stateChangeRecord);

    /// <summary>
    /// Deletes a state change record.
    /// </summary>
    /// <param name="id">The unique id</param>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Deletes state change records older than the specified date.
    /// </summary>
    /// <param name="cutoffDate">The cutoff date</param>
    /// <returns>Number of deleted entries</returns>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
}
