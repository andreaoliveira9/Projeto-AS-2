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

using Microsoft.EntityFrameworkCore;
using Piranha.Data.Notifications;
using Piranha.Notifications.Repositories;
using Piranha.Data.EditorialWorkflowAndAuditAndNotifications;

namespace Piranha.Repositories.Notifications;

/// <summary>
/// Entity Framework implementation of the notification repository.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly IDb _db;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="db">The current db context</param>
    public NotificationRepository(IDb db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets all notifications.
    /// </summary>
    /// <returns>The available notifications</returns>
    public async Task<IEnumerable<Piranha.Notifications.Models.Notification>> GetAllAsync()
    {
        var notifications = await _db.Set<Data.Notifications.Notification>()
            .AsNoTracking()
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();

        return notifications.Select(n => new Piranha.Notifications.Models.Notification
        {
            Id = n.Id,
            Timestamp = n.Timestamp
        });
    }

    /// <summary>
    /// Gets a notification by id.
    /// </summary>
    /// <param name="id">The notification id</param>
    /// <returns>The notification if found</returns>
    public async Task<Piranha.Notifications.Models.Notification?> GetByIdAsync(Guid id)
    {
        var notification = await _db.Set<Data.Notifications.Notification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
            return null;

        return new Piranha.Notifications.Models.Notification
        {
            Id = notification.Id,
            Timestamp = notification.Timestamp
        };
    }

    /// <summary>
    /// Saves the given notification.
    /// </summary>
    /// <param name="model">The notification model</param>
    public async Task SaveAsync(Piranha.Notifications.Models.Notification model)
    {
        var notification = await _db.Set<Data.Notifications.Notification>()
            .FirstOrDefaultAsync(n => n.Id == model.Id);

        if (notification == null)
        {
            notification = new Notification
            {
                Id = model.Id != Guid.Empty ? model.Id : Guid.NewGuid(),
                Timestamp = model.Timestamp
            };
            await _db.Set<Data.Notifications.Notification>().AddAsync(notification);
        }
        else
        {
            notification.Timestamp = model.Timestamp;
            _db.Set<Data.Notifications.Notification>().Update(notification);
        }

        await ((DbContext)_db).SaveChangesAsync();
        model.Id = notification.Id;
    }

    /// <summary>
    /// Deletes the notification with the given id.
    /// </summary>
    /// <param name="id">The notification id</param>
    public async Task DeleteAsync(Guid id)
    {
        var notification = await _db.Set<Data.Notifications.Notification>()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification != null)
        {
            _db.Set<Data.Notifications.Notification>().Remove(notification);
            await ((DbContext)_db).SaveChangesAsync();
        }
    }
}
