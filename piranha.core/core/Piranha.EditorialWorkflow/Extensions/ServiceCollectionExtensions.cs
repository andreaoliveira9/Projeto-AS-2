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
using Piranha.EditorialWorkflow.Services;

namespace Piranha.EditorialWorkflow.Extensions;

/// <summary>
/// Extension methods for setting up Editorial Workflow services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Editorial Workflow services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddEditorialWorkflow(this IServiceCollection services)
    {
        services.AddScoped<IEditorialWorkflowService, EditorialWorkflowService>();

        // Add RabbitMQ connection service (shared with Audit module)
        services.AddSingleton<IRabbitMQConnectionService, RabbitMQConnectionService>();
        
        // Add workflow message publisher
        services.AddScoped<IWorkflowMessagePublisher, WorkflowMessagePublisher>();
        
        return services;
    }
}

/// <summary>
/// Extension methods for setting up Editorial Workflow in PiranhaServiceBuilder.
/// </summary>
public static class PiranhaServiceBuilderExtensions
{
    /// <summary>
    /// Uses the Editorial Workflow services.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseEditorialWorkflow(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddEditorialWorkflow();
        
        return serviceBuilder;
    }
}
