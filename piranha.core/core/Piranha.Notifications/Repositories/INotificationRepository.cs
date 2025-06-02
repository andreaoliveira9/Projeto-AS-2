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
/// Repository interface for notifications.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Gets all notifications.
    /// </summary>
    /// <returns>The available notifications</returns>
    Task<IEnumerable<Models.Notification>> GetAllAsync();

    /// <summary>
    /// Gets a notification by id.
    /// </summary>
    /// <param name="id">The notification id</param>
    /// <returns>The notification if found</returns>
    Task<Models.Notification?> GetByIdAsync(Guid id);

    /// <summary>
    /// Saves the given notification.
    /// </summary>
    /// <param name="model">The notification model</param>
    Task SaveAsync(Models.Notification model);

    /// <summary>
    /// Deletes the notification with the given id.
    /// </summary>
    /// <param name="id">The notification id</param>
    Task DeleteAsync(Guid id);
}
