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

namespace Piranha.Data.Notifications;

/// <summary>
/// Entity Framework data model for notifications.
/// </summary>
[Serializable]
public class Notification
{
    /// <summary>
    /// Gets/sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the timestamp of the notification.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
