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

namespace Piranha.Audit.Configuration;

/// <summary>
/// RabbitMQ configuration options for the audit module.
/// </summary>
public sealed class RabbitMQOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Piranha:Audit:RabbitMQ";

    /// <summary>
    /// RabbitMQ connection hostname.
    /// Default is "localhost".
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ connection port.
    /// Default is 5672.
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ username.
    /// Default is "guest".
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ password.
    /// Default is "guest".
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ virtual host.
    /// Default is "/".
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// The queue name to consume audit events from.
    /// Default is "piranha.audit.events".
    /// </summary>
    public string QueueName { get; set; } = "piranha.audit.events";

    /// <summary>
    /// The exchange name for audit events.
    /// Default is "piranha.audit".
    /// </summary>
    public string ExchangeName { get; set; } = "piranha.broadcast";

    /// <summary>
    /// The routing key for audit events.
    /// Default is "state.changed".
    /// </summary>
    public string RoutingKey { get; set; } = "state.changed";

    /// <summary>
    /// Whether to enable SSL/TLS connection.
    /// Default is false.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Connection timeout in milliseconds.
    /// Default is 30000 (30 seconds).
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30000;

    /// <summary>
    /// Heartbeat interval in seconds.
    /// Default is 60 seconds.
    /// </summary>
    public ushort HeartbeatInterval { get; set; } = 60;

    /// <summary>
    /// Automatic recovery enabled.
    /// Default is true.
    /// </summary>
    public bool AutomaticRecoveryEnabled { get; set; } = true;

    /// <summary>
    /// Network recovery interval in milliseconds.
    /// Default is 5000 (5 seconds).
    /// </summary>
    public int NetworkRecoveryInterval { get; set; } = 5000;

    /// <summary>
    /// Maximum number of retry attempts for message processing.
    /// Default is 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds.
    /// Default is 1000 (1 second).
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Whether to declare the queue and exchange automatically.
    /// Default is true.
    /// </summary>
    public bool AutoDeclare { get; set; } = true;

    /// <summary>
    /// Whether the queue should be durable.
    /// Default is true.
    /// </summary>
    public bool QueueDurable { get; set; } = true;

    /// <summary>
    /// Whether the exchange should be durable.
    /// Default is true.
    /// </summary>
    public bool ExchangeDurable { get; set; } = true;

    /// <summary>
    /// Exchange type. Default is "direct".
    /// </summary>
    public string ExchangeType { get; set; } = "fanout";
}
