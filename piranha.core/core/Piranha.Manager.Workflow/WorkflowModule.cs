using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Piranha.Manager;
using Piranha.Security;

namespace Piranha.Manager.Workflow
{
    public static class WorkflowModule
    {
        /// <summary>
        /// Adds the Workflow manager module.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The services</returns>
        public static IServiceCollection AddPiranhaManagerWorkflow(this IServiceCollection services)
        {
            // Add the workflow controller
            services.AddMvc()
                .AddApplicationPart(typeof(Controllers.WorkflowController).Assembly);

            // Add the workflow permissions
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.Workflows, policy =>
                {
                    policy.RequireClaim(Permission.Admin, Permission.Admin);
                });
            });

            // Extend the manager UI
            services.AddManagerExtensions();

            // Return the service collection
            return services;
        }

        /// <summary>
        /// Uses the Workflow manager module.
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UsePiranhaManagerWorkflow(this IApplicationBuilder builder)
        {
            // Add the embedded file provider for static resources
            return builder.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(typeof(WorkflowModule).Assembly, "Piranha.Manager.Workflow.assets.dist"),
                RequestPath = "/manager/workflow"
            });
        }

        /// <summary>
        /// Uses the Workflow manager module.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder</param>
        /// <returns>The endpoint route builder</returns>
        public static IEndpointRouteBuilder UsePiranhaManagerWorkflow(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllerRoute(
                name: "workflow-list",
                pattern: "manager/workflows",
                defaults: new { area = "Manager", controller = "Workflow", action = "List" }
            );

            endpoints.MapControllerRoute(
                name: "workflow-details",
                pattern: "manager/workflow/{id:Guid}",
                defaults: new { area = "Manager", controller = "Workflow", action = "Details" }
            );

            return endpoints;
        }
    }
} 