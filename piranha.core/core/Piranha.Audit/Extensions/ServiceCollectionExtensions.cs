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
    /// Focused on consuming messages from message queue.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAudit(this IServiceCollection services)
    {
        // Register core audit service - only for consuming messages
        services.AddScoped<IAuditService, AuditService>();

        // Register message queue channel for consuming audit events
        services.AddSingleton(provider =>
        {
            var channelOptions = new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            };
            return Channel.CreateBounded<string>(channelOptions);
        });

        // Register background service for consuming audit messages
        services.AddHostedService<AuditMessageConsumerService>();

        return services;
    }
}

/// <summary>
/// Extension methods for setting up Audit in PiranhaServiceBuilder.
/// </summary>
public static class PiranhaServiceBuilderExtensions
{
    /// <summary>
    /// Uses the Audit services for consuming messages.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseAudit(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddAudit();
        
        return serviceBuilder;
    }
}
