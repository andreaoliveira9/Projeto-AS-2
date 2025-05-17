/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using Microsoft.Extensions.DependencyInjection;
using Piranha.Workflow.Services;

namespace Piranha.Workflow
{
    public static class WorkflowModule
    {
        /// <summary>
        /// Register the workflow module with the specified service collection.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddPiranhaWorkflow(this IServiceCollection services)
        {
            // Register the workflow service
            services.AddSingleton<IWorkflowService, WorkflowService>();

            return services;
        }

        /// <summary>
        /// Register the workflow module with the application builder.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static PiranhaServiceBuilder UsePiranhaWorkflow(this PiranhaServiceBuilder builder)
        {
            // Here we can perform any initialization needed when the module is loaded

            return builder;
        }
    }
} 