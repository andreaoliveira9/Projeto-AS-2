using Microsoft.Extensions.DependencyInjection;
using Piranha.Manager;

namespace Piranha.Manager.Workflow
{
    public static class WorkflowManagerExtensions
    {
        /// <summary>
        /// Extends the Piranha manager with workflow components.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The updated service collection</returns>
        public static IServiceCollection AddManagerExtensions(this IServiceCollection services)
        {
            // Get the manager module
            var managerModule = Piranha.App.Modules.Get<Piranha.Manager.Module>();

            if (managerModule != null)
            {
                // Add resources for the workflow module
                managerModule.Scripts.Add(new ManagerScriptDefinition("~/manager/workflow/js/workflow.js"));

                // Register partial views for the workflow UI
                managerModule.Partials.Add("~/Areas/Manager/Shared/ContentWorkflow.cshtml");
            }

            return services;
        }
    }
} 