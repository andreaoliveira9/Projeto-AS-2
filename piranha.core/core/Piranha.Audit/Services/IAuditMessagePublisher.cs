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
using Piranha.Audit.Events;
using System.Text.Json;
using System.Threading.Channels;

namespace Piranha.Audit.Services;

/// <summary>
/// Service for publishing audit events to the message queue.
/// </summary>
public interface IAuditMessagePublisher
{
    /// <summary>
    /// Publishes a workflow state changed event to the audit queue.
    /// </summary>
    /// <param name="stateChangedEvent">The state changed event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishWorkflowStateChangedAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a raw message to the audit queue.
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishMessageAsync(string message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of the audit message publisher.
/// </summary>
public sealed class AuditMessagePublisher : IAuditMessagePublisher
{
    private readonly Channel<string> _messageQueue;
    private readonly ILogger<AuditMessagePublisher> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="messageQueue">The message queue channel</param>
    /// <param name="logger">The logger</param>
    public AuditMessagePublisher(
        Channel<string> messageQueue,
        ILogger<AuditMessagePublisher> logger)
    {
        _messageQueue = messageQueue;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task PublishWorkflowStateChangedAsync(WorkflowStateChangedEvent stateChangedEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = JsonSerializer.Serialize(stateChangedEvent, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await PublishMessageAsync(message, cancellationToken);
            
            _logger.LogDebug("Published workflow state changed event {EventId} for content {ContentId}",
                stateChangedEvent.EventId, stateChangedEvent.ContentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish workflow state changed event {EventId}",
                stateChangedEvent.EventId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task PublishMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _messageQueue.Writer.WriteAsync(message, cancellationToken);
            _logger.LogDebug("Published message to audit queue: {MessagePreview}", 
                message.Length > 100 ? message[..100] + "..." : message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Message queue is closed. Cannot publish message");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to audit queue");
            throw;
        }
    }
}
