/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Piranha.Audit.Events;
using Piranha.Audit.Services;
using System.Text.Json;
using System.Threading.Channels;

namespace Piranha.Audit.Services;

/// <summary>
/// Background service that consumes audit events from a message queue.
/// </summary>
public sealed class AuditMessageConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditMessageConsumerService> _logger;
    private readonly Channel<string> _messageQueue;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="logger">The logger</param>
    /// <param name="messageQueue">The message queue channel</param>
    public AuditMessageConsumerService(
        IServiceProvider serviceProvider,
        ILogger<AuditMessageConsumerService> logger,
        Channel<string> messageQueue)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _messageQueue = messageQueue;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit message consumer service started");

        await foreach (var message in _messageQueue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessMessageAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process audit message: {Message}", message);
            }
        }

        _logger.LogInformation("Audit message consumer service stopped");
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

            var stateChangedEvent = DeserializeStateChangedEvent(message);
            if (stateChangedEvent != null)
            {
                await auditService.ProcessWorkflowStateChangedEventAsync(stateChangedEvent, cancellationToken);
                _logger.LogDebug("Successfully processed workflow state changed event {EventId}",
                    stateChangedEvent.EventId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit message: {Message}", message);
            throw;
        }
    }

    private WorkflowStateChangedEvent DeserializeStateChangedEvent(string message)
    {
        try
        {
            return JsonSerializer.Deserialize<WorkflowStateChangedEvent>(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize workflow state changed event from message: {Message}", message);
            return null;
        }
    }
}
