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
using Piranha;
using Piranha.Audit.Services;
using System.Threading.Channels;

namespace Piranha.Audit.Extensions;

/// <summary>
/// Extension methods for setting up Audit services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Audit services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration for audit options</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAudit(this IServiceCollection services, Action<AuditOptions> configureOptions = null)
    {
        var options = new AuditOptions();
        configureOptions?.Invoke(options);

        // Register options
        services.AddSingleton(options);

        // Register core audit service
        services.AddScoped<IAuditService, AuditService>();

        // Register audit message publisher
        services.AddScoped<IAuditMessagePublisher, AuditMessagePublisher>();

        // Register message queue channel for audit events
        services.AddSingleton(provider =>
        {
            var channelOptions = new BoundedChannelOptions(options.MessageQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };
            return Channel.CreateBounded<string>(channelOptions);
        });

        // Register background service for consuming audit messages
        if (options.EnableMessageConsumer)
        {
            services.AddHostedService<AuditMessageConsumerService>();
        }

        // Register background service for automatic cleanup
        if (options.EnableAutomaticCleanup)
        {
            services.AddHostedService<AuditCleanupService>();
        }

        return services;
    }
}

/// <summary>
/// Extension methods for setting up Audit in PiranhaServiceBuilder.
/// </summary>
public static class PiranhaServiceBuilderExtensions
{
    /// <summary>
    /// Uses the Audit services.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <param name="configureOptions">Optional configuration for audit options</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseAudit(this PiranhaServiceBuilder serviceBuilder, Action<AuditOptions> configureOptions = null)
    {
        serviceBuilder.Services.AddAudit(configureOptions);
        
        return serviceBuilder;
    }
}

/// <summary>
/// Configuration options for the Audit module.
/// </summary>
public sealed class AuditOptions
{
    /// <summary>
    /// Gets/sets whether to enable the message consumer background service.
    /// Default is true.
    /// </summary>
    public bool EnableMessageConsumer { get; set; } = true;

    /// <summary>
    /// Gets/sets the message queue capacity.
    /// Default is 1000.
    /// </summary>
    public int MessageQueueCapacity { get; set; } = 1000;

    /// <summary>
    /// Gets/sets the default retention period for audit records in days.
    /// Default is 365 days (1 year).
    /// </summary>
    public int DefaultRetentionDays { get; set; } = 365;

    /// <summary>
    /// Gets/sets whether to enable automatic cleanup of old records.
    /// Default is false.
    /// </summary>
    public bool EnableAutomaticCleanup { get; set; } = false;

    /// <summary>
    /// Gets/sets how often to run automatic cleanup (in hours).
    /// Default is 24 hours.
    /// </summary>
    public int CleanupIntervalHours { get; set; } = 24;
}
