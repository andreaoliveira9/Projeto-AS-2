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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Piranha.Notifications.Configuration;
using Piranha.Notifications.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Piranha.Notifications.Services;

/// <summary>
/// Background service that consumes audit events from RabbitMQ.
/// Focused solely on consuming messages and storing audit records.
/// </summary>
public sealed class NotificationsMessageConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationsMessageConsumerService> _logger;
    private readonly IRabbitMQConnectionService _rabbitMQConnectionService;
    private readonly RabbitMQOptions _options;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="logger">The logger</param>
    /// <param name="rabbitMQConnectionService">The RabbitMQ connection service</param>
    /// <param name="options">RabbitMQ options</param>
    public NotificationsMessageConsumerService(
        IServiceProvider serviceProvider,
        ILogger<NotificationsMessageConsumerService> logger,
        IRabbitMQConnectionService rabbitMQConnectionService,
        IOptions<RabbitMQOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQConnectionService = rabbitMQConnectionService;
        _options = options.Value;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit message consumer service started");

        try
        {
            await SetupRabbitMQConsumerAsync(stoppingToken);

            // Keep the service running until cancellation is requested
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Audit message consumer service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit message consumer service encountered an unexpected error");
            throw;
        }
        finally
        {
            await CleanupAsync();
        }

        _logger.LogInformation("Audit message consumer service stopped");
    }

    private async Task SetupRabbitMQConsumerAsync(CancellationToken cancellationToken)
    {
        try
        {
            _channel = _rabbitMQConnectionService.GetChannel();
            _consumer = new AsyncEventingBasicConsumer(_channel);

            _consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var deliveryTag = eventArgs.DeliveryTag;

                try
                {
                    await ProcessMessageWithRetryAsync(message, cancellationToken);
                    
                    // Acknowledge the message only after successful processing
                    await _channel.BasicAckAsync(deliveryTag, false, cancellationToken);
                    
                    _logger.LogDebug("Successfully processed and acknowledged message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process audit message after retries: {Message}", 
                        message?.Length > 200 ? message[..200] + "..." : message);
                    
                    // Reject the message and don't requeue it to avoid infinite loops
                    await _channel.BasicNackAsync(deliveryTag, false, false, cancellationToken);
                }
            };

            // Start consuming messages
            var consumerTag = await _channel.BasicConsumeAsync(
                queue: _options.QueueName,
                autoAck: false, // We'll manually acknowledge messages
                consumer: _consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Started consuming messages from queue '{QueueName}' with consumer tag '{ConsumerTag}'", 
                _options.QueueName, consumerTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to setup RabbitMQ consumer");
            throw;
        }
    }

    private async Task ProcessMessageWithRetryAsync(string message, CancellationToken cancellationToken)
    {
        var attempt = 0;
        var maxAttempts = _options.MaxRetryAttempts;
        
        while (attempt < maxAttempts)
        {
            try
            {
                await ProcessMessageAsync(message, cancellationToken);
                return; // Success, exit retry loop
            }
            catch (Exception ex) when (attempt < maxAttempts - 1)
            {
                attempt++;
                _logger.LogWarning(ex, "Failed to process message on attempt {Attempt}/{MaxAttempts}. Retrying in {DelayMs}ms", 
                    attempt, maxAttempts, _options.RetryDelayMs);
                
                await Task.Delay(_options.RetryDelayMs, cancellationToken);
            }
            catch (Exception ex)
            {
                // Final attempt failed
                attempt++;
                _logger.LogError(ex, "Failed to process message after {MaxAttempts} attempts", maxAttempts);
                throw;
            }
        }
    }

    private async Task CleanupAsync()
    {
        try
        {
            if (_consumer != null)
            {
                _consumer = null;
            }

            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
                _channel = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup");
        }
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationsService = scope.ServiceProvider.GetRequiredService<INotificationsService>();

            var stateChangedEvent = DeserializeStateChangedEvent(message);
            if (stateChangedEvent != null)
            {
                await notificationsService.ProcessWorkflowStateChangedEventAsync(stateChangedEvent, cancellationToken);
                _logger.LogDebug("Successfully processed workflow state changed event {ContentId}",
                    stateChangedEvent.ContentId);
            }
            else
            {
                _logger.LogWarning("Received message could not be deserialized as WorkflowStateChangedEvent");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification message: {Message}", 
                message?.Length > 200 ? message[..200] + "..." : message);
            throw;
        }
    }

    private WorkflowStateChangedEvent? DeserializeStateChangedEvent(string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("Received null or empty message");
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Deserialize<WorkflowStateChangedEvent>(message, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize workflow state changed event from message: {MessagePreview}", 
                message?.Length > 200 ? message[..200] + "..." : message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deserializing message: {MessagePreview}", 
                message?.Length > 200 ? message[..200] + "..." : message);
            return null;
        }
    }
}
