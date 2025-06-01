/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Piranha.Audit.Configuration;
using Piranha.Audit.Events;
using Piranha.Audit.Services;
using System.Text;
using System.Text.Json;

namespace Piranha.EditorialWorkflow.Services;

/// <summary>
/// Service for publishing workflow state change messages to RabbitMQ.
/// </summary>
public sealed class WorkflowMessagePublisher : IWorkflowMessagePublisher
{
    private readonly IRabbitMQConnectionService _rabbitMQConnectionService;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<WorkflowMessagePublisher> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="rabbitMQConnectionService">The RabbitMQ connection service</param>
    /// <param name="options">RabbitMQ options</param>
    /// <param name="logger">The logger</param>
    public WorkflowMessagePublisher(
        IRabbitMQConnectionService rabbitMQConnectionService,
        IOptions<RabbitMQOptions> options,
        ILogger<WorkflowMessagePublisher> logger)
    {
        _rabbitMQConnectionService = rabbitMQConnectionService;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> PublishStateChangedEventAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (stateChangedEvent == null)
            {
                _logger.LogWarning("Cannot publish null state changed event");
                return false;
            }

            // Serialize the event to JSON
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var messageJson = JsonSerializer.Serialize(stateChangedEvent, options);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            // Get channel and publish message
            var channel = _rabbitMQConnectionService.GetChannel();
            
            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey,
                body: messageBytes,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully published workflow state changed event {EventId} for content {ContentId}",
                stateChangedEvent.EventId, stateChangedEvent.ContentId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish workflow state changed event {EventId}",
                stateChangedEvent?.EventId);
            return false;
        }
    }
}
