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
using Piranha;
using Piranha.EditorialWorkflow.Repositories;
using Piranha.Repositories.EditorialWorkflow;
using Piranha.Audit.Repositories;
using Piranha.Repositories.Audit;
using Piranha.Notifications.Repositories;
using Piranha.Repositories.Notifications;

namespace Piranha.Data.EF.EditorialWorkflowAndAuditAndNotifications;

/// <summary>
/// Extension methods for setting up Editorial Workflow services.
/// </summary>
public static class PiranhaServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Editorial Workflow repositories to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddEditorialWorkflowRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
        services.AddScoped<IWorkflowStateRepository, WorkflowStateRepository>();
        services.AddScoped<ITransitionRuleRepository, TransitionRuleRepository>();
        services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
        services.AddScoped<IWorkflowContentExtensionRepository, WorkflowContentExtensionRepository>();

        return services;
    }

    /// <summary>
    /// Uses the Editorial Workflow EF repositories.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseEditorialWorkflowEF(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddEditorialWorkflowRepositories();

        return serviceBuilder;
    }
    
    /// <summary>
    /// Adds the Audit EF repositories to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddAuditRepositories(this IServiceCollection services)
    {
        services.AddScoped<IStateChangeRecordRepository, StateChangeRecordRepository>();

        return services;
    }

    /// <summary>
    /// Uses the Audit EF repositories.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseAuditEF(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddAuditRepositories();
        
        return serviceBuilder;
    }
    
    /// <summary>
    /// Adds the Notifications EF repositories to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNotificationsRepositories(this IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IStateChangedNotificationRepository, StateChangedNotificationRepository>();

        return services;
    }

    /// <summary>
    /// Uses the Notifications EF repositories.
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseNotificationsEF(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddNotificationsRepositories();
        
        return serviceBuilder;
    }

    /// <summary>
    /// Uses all EF repositories (Editorial Workflow, Audit and Notifications).
    /// </summary>
    /// <param name="serviceBuilder">The service builder</param>
    /// <returns>The updated builder</returns>
    public static PiranhaServiceBuilder UseEditorialWorkflowAndAuditAndNotificationsEF(this PiranhaServiceBuilder serviceBuilder)
    {
        serviceBuilder.Services.AddEditorialWorkflowRepositories();
        serviceBuilder.Services.AddAuditRepositories();
        serviceBuilder.Services.AddNotificationsRepositories();
        
        return serviceBuilder;
    }
}
