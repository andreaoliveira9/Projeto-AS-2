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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Piranha.Notifications.Configuration;
using RabbitMQ.Client;

namespace Piranha.Notifications.Services;

/// <summary>
/// Interface for RabbitMQ connection management.
/// </summary>
public interface IRabbitMQConnectionService
{
    /// <summary>
    /// Gets or creates a connection to RabbitMQ.
    /// </summary>
    /// <returns>The RabbitMQ connection</returns>
    IConnection GetConnection();

    /// <summary>
    /// Gets or creates a channel from the connection.
    /// </summary>
    /// <returns>The RabbitMQ channel</returns>
    IChannel GetChannel();

    /// <summary>
    /// Closes the connection and channel if they exist.
    /// </summary>
    void Close();
}

/// <summary>
/// Service for managing RabbitMQ connections and channels.
/// </summary>
public sealed class RabbitMQConnectionService : IRabbitMQConnectionService, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQConnectionService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly object _lock = new();
    private bool _disposed = false;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="options">RabbitMQ options</param>
    /// <param name="logger">Logger</param>
    public RabbitMQConnectionService(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQConnectionService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public IConnection GetConnection()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnectionService));

        if (_connection != null && _connection.IsOpen)
            return _connection;

        lock (_lock)
        {
            if (_connection != null && _connection.IsOpen)
                return _connection;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    RequestedHeartbeat = TimeSpan.FromSeconds(_options.HeartbeatInterval),
                    AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled,
                    NetworkRecoveryInterval = TimeSpan.FromMilliseconds(_options.NetworkRecoveryInterval),
                    RequestedConnectionTimeout = TimeSpan.FromMilliseconds(_options.ConnectionTimeout)
                };

                if (_options.UseSsl)
                {
                    factory.Ssl = new SslOption
                    {
                        Enabled = true,
                        ServerName = _options.HostName
                    };
                }

                _connection = factory.CreateConnectionAsync("Piranha.Notifications.Consumer").GetAwaiter().GetResult();

                _logger.LogInformation("Connected to RabbitMQ at {HostName}:{Port}", _options.HostName, _options.Port);
                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ at {HostName}:{Port}", _options.HostName, _options.Port);
                throw;
            }
        }
    }

    /// <inheritdoc />
    public IChannel GetChannel()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQConnectionService));

        if (_channel != null && _channel.IsOpen)
            return _channel;

        lock (_lock)
        {
            if (_channel != null && _channel.IsOpen)
                return _channel;

            try
            {
                var connection = GetConnection();
                _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

                if (_options.AutoDeclare)
                {
                    // Declare exchange
                    _channel.ExchangeDeclareAsync(
                        exchange: _options.ExchangeName,
                        type: _options.ExchangeType,
                        durable: _options.ExchangeDurable,
                        autoDelete: false,
                        arguments: null).GetAwaiter().GetResult();

                    // Declare queue
                    _channel.QueueDeclareAsync(
                        queue: _options.QueueName,
                        durable: _options.QueueDurable,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null).GetAwaiter().GetResult();

                    // Bind queue to exchange
                    _channel.QueueBindAsync(
                        queue: _options.QueueName,
                        exchange: _options.ExchangeName,
                        routingKey: _options.RoutingKey,
                        arguments: null).GetAwaiter().GetResult();
                }

                return _channel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ channel");
                throw;
            }
        }
    }

    /// <inheritdoc />
    public void Close()
    {
        lock (_lock)
        {
            try
            {
                _channel?.CloseAsync().GetAwaiter().GetResult();
                _channel?.Dispose();
                _channel = null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing RabbitMQ channel");
            }

            try
            {
                _connection?.CloseAsync().GetAwaiter().GetResult();
                _connection?.Dispose();
                _connection = null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing RabbitMQ connection");
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            Close();
            _disposed = true;
        }
    }
}
