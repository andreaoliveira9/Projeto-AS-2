using Piranha;
using Piranha.AspNetCore;
using Piranha.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace MvcWeb.Extensions;

/// <summary>
/// Extensions for the workflow module.
/// </summary>
public static class WorkflowExtensions
{
    /// <summary>
    /// Uses the workflow module.
    /// </summary>
    /// <param name="options">The builder options</param>
    /// <returns>The builder options</returns>
    public static PiranhaApplicationBuilder UseWorkflow(this PiranhaApplicationBuilder options)
    {
        // Get the service provider
        var serviceProvider = options.Builder.ApplicationServices;
        
        // Use the workflow module
        serviceProvider.UsePiranhaWorkflow();
        
        return options;
    }
}
