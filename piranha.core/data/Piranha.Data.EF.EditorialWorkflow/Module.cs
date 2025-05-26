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
using Piranha.EditorialWorkflow.Repositories;
using Piranha.Repositories.EditorialWorkflow;

namespace Piranha.Data.EF.EditorialWorkflow;

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
}
