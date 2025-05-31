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
using Microsoft.Extensions.Logging;
using Piranha;
using Piranha.Notifications.Configuration;
using Piranha.Notifications.Services;

namespace Piranha.Notifications.Extensions;


/// <summary>
/// Extension methods for setting up Audit services.
/// </summary>
public static class ServiceCollectionExtensions
{
    
    /// <summary>
    /// Adds the Audit services to the service collection with custom RabbitMQ options.
    /// Consumes messages from RabbitMQ message queue.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure RabbitMQ options</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNotifications(this IServiceCollection services, Action<RabbitMQOptions> configureOptions)
    {

        // Configure RabbitMQ options
        services.Configure(configureOptions);

        // Register core audit service - only for consuming messages
        services.AddScoped<INotificationsService, NotificationsService>();

        // Register RabbitMQ connection service
        services.AddSingleton<IRabbitMQConnectionService, RabbitMQConnectionService>();

        // Register background service for consuming audit messages from RabbitMQ
        services.AddHostedService<NotificationsMessageConsumerService>();

        /* services.AddControllers()
            .AddApplicationPart(typeof(AuditController).Assembly)
            .AddControllersAsServices(); */

        return services;
    }
}

/// <summary>
/// Extension methods for setting up Audit in PiranhaServiceBuilder.
/// </summary>
public static class PiranhaServiceBuilderExtensions
{
    /// <summary>
    /// Uses the Audit services for consuming messages from RabbitMQ with custom options.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <param name="configureOptions">Action to configure RabbitMQ options</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseNotifications(this PiranhaServiceBuilder serviceBuilder, Action<RabbitMQOptions> configureOptions)
    {
        serviceBuilder.Services.AddNotifications(configureOptions);

        return serviceBuilder;
    }
}
