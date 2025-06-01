/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

#nullable enable

namespace Piranha.Notifications.Repositories;

/// <summary>
/// Repository interface for state changed notifications.
/// </summary>
public interface IStateChangedNotificationRepository
{
    /// <summary>
    /// Gets all state changed notifications.
    /// </summary>
    /// <returns>The available state changed notifications</returns>
    Task<IEnumerable<Models.StateChangedNotification>> GetAllAsync();

    /// <summary>
    /// Gets a state changed notification by id.
    /// </summary>
    /// <param name="id">The notification id</param>
    /// <returns>The notification if found</returns>
    Task<Models.StateChangedNotification?> GetByIdAsync(Guid id);

    /// <summary>
    /// Saves the given state changed notification.
    /// </summary>
    /// <param name="model">The notification model</param>
    Task SaveAsync(Models.StateChangedNotification model);

    /// <summary>
    /// Deletes the state changed notification with the given id.
    /// </summary>
    /// <param name="id">The notification id</param>
    Task DeleteAsync(Guid id);
}
