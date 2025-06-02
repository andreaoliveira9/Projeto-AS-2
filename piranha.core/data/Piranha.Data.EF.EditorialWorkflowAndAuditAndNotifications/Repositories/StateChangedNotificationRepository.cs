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
/// Entity Framework implementation of the state changed notification repository.
/// </summary>
public class StateChangedNotificationRepository : IStateChangedNotificationRepository
{
    private readonly IDb _db;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="db">The current db context</param>
    public StateChangedNotificationRepository(IDb db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets all state changed notifications.
    /// </summary>
    /// <returns>The available state changed notifications</returns>
    public async Task<IEnumerable<Piranha.Notifications.Models.StateChangedNotification>> GetAllAsync()
    {
        var notifications = await _db.Set<Data.Notifications.StateChangedNotification>()
            .OfType<StateChangedNotification>()
            .AsNoTracking()
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();

        return notifications.Select(n => new Piranha.Notifications.Models.StateChangedNotification
        {
            Id = n.Id,
            Timestamp = n.Timestamp,
            ContentId = n.ContentId,
            ContentName = n.ContentName,
            FromState = n.FromState,
            ToState = n.ToState,
            Comments = n.Comments,
            TransitionDescription = n.TransitionDescription,
            ReviewedBy = n.ReviewedBy,
            Approved = n.Approved
        });
    }

    /// <summary>
    /// Gets a state changed notification by id.
    /// </summary>
    /// <param name="id">The notification id</param>
    /// <returns>The notification if found</returns>
    public async Task<Piranha.Notifications.Models.StateChangedNotification?> GetByIdAsync(Guid id)
    {
        var notification = await _db.Set<Data.Notifications.StateChangedNotification>()
            .OfType<StateChangedNotification>()
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
            return null;

        return new Piranha.Notifications.Models.StateChangedNotification
        {
            Id = notification.Id,
            Timestamp = notification.Timestamp,
            ContentId = notification.ContentId,
            ContentName = notification.ContentName,
            FromState = notification.FromState,
            ToState = notification.ToState,
            Comments = notification.Comments,
            TransitionDescription = notification.TransitionDescription,
            ReviewedBy = notification.ReviewedBy,
            Approved = notification.Approved
        };
    }

    /// <summary>
    /// Saves the given state changed notification.
    /// </summary>
    /// <param name="model">The notification model</param>
    public async Task SaveAsync(Piranha.Notifications.Models.StateChangedNotification model)
    {
        var notification = await _db.Set<Data.Notifications.StateChangedNotification>()
            .OfType<StateChangedNotification>()
            .FirstOrDefaultAsync(n => n.Id == model.Id);

        if (notification == null)
        {
            notification = new StateChangedNotification
            {
                Id = model.Id != Guid.Empty ? model.Id : Guid.NewGuid(),
                Timestamp = model.Timestamp,
                ContentId = model.ContentId,
                ContentName = model.ContentName,
                FromState = model.FromState,
                ToState = model.ToState,
                Comments = model.Comments,
                TransitionDescription = model.TransitionDescription,
                ReviewedBy = model.ReviewedBy,
                Approved = model.Approved
            };
            await _db.Set<Data.Notifications.StateChangedNotification>().AddAsync(notification);
        }
        else
        {
            notification.Timestamp = model.Timestamp;
            notification.ContentId = model.ContentId;
            notification.ContentName = model.ContentName;
            notification.FromState = model.FromState;
            notification.ToState = model.ToState;
            notification.Comments = model.Comments;
            notification.TransitionDescription = model.TransitionDescription;
            notification.ReviewedBy = model.ReviewedBy;
            notification.Approved = model.Approved;
            _db.Set<Data.Notifications.StateChangedNotification>().Update(notification);
        }

        await ((DbContext)_db).SaveChangesAsync();
        model.Id = notification.Id;
    }

    /// <summary>
    /// Deletes the state changed notification with the given id.
    /// </summary>
    /// <param name="id">The notification id</param>
    public async Task DeleteAsync(Guid id)
    {
        var notification = await _db.Set<Data.Notifications.StateChangedNotification>()
            .OfType<StateChangedNotification>()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification != null)
        {
            _db.Set<Data.Notifications.StateChangedNotification>().Remove(notification);
            await ((DbContext)_db).SaveChangesAsync();
        }
    }
}
