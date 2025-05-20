/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Piranha.Extend;
using Piranha.Security;
using Piranha.Services;

namespace Piranha.Workflow;

/// <summary>
/// The workflow module.
/// </summary>
public class WorkflowModule : IModule
{
    /// <summary>
    /// Gets the module author.
    /// </summary>
    public string Author => "Piranha Team";

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public string Name => "Piranha.Workflow";

    /// <summary>
    /// Gets the module version.
    /// </summary>
    public string Version => Piranha.Utils.GetAssemblyVersion(GetType().Assembly);

    /// <summary>
    /// Gets the module description.
    /// </summary>
    public string Description => "Workflow module for Piranha CMS";

    /// <summary>
    /// Gets the module package URL.
    /// </summary>
    public string PackageUrl => "https://www.nuget.org/packages/Piranha.Workflow";
    
    /// <summary>
    /// Gets the icon URL for the module.
    /// </summary>
    public string IconUrl => "https://piranhacms.org/assets/twitter-shield.png";

    /// <summary>
    /// Gets or sets the workflow file path for the standard editorial workflow.
    /// </summary>
    public string EditorialWorkflowPath { get; set; } = "Workflow/editorial-workflow.json";

    /// <summary>
    /// Initializes the module.
    /// </summary>
    public void Init()
    {
        // Register permissions
        var permissionModule = "Workflow";
        var modulePermissions = App.Permissions.GetModules();
        
        if (!modulePermissions.Contains(permissionModule))
        {
            // Module doesn't exist yet, it will be created when we access it
        }
        
        foreach (var permission in WorkflowPermission.All())
        {
            App.Permissions["Workflow"].Add(new PermissionItem
            {
                Name = permission,
                Title = GetPermissionTitle(permission)
            });
        }
        
        // Add menu item
        Menu.AddWorkflowModule();
    }

    /// <summary>
    /// Gets the title for the given permission.
    /// </summary>
    /// <param name="permission">The permission</param>
    /// <returns>The title</returns>
    private string GetPermissionTitle(string permission)
    {
        return permission switch
        {
            WorkflowPermission.ManageWorkflows => "Manage Workflows",
            WorkflowPermission.EditWorkflow => "Edit Workflow",
            WorkflowPermission.DeleteWorkflow => "Delete Workflow",
            WorkflowPermission.ViewWorkflowState => "View Workflow State",
            WorkflowPermission.TransitionToDraft => "Transition to Draft",
            WorkflowPermission.TransitionToReview => "Transition to Review",
            WorkflowPermission.TransitionToLegalReview => "Transition to Legal Review",
            WorkflowPermission.TransitionToApproved => "Transition to Approved",
            WorkflowPermission.TransitionToPublished => "Transition to Published",
            WorkflowPermission.TransitionToArchived => "Transition to Archived",
            WorkflowPermission.AdvanceWorkflow => "Advance Workflow",
            WorkflowPermission.RevertWorkflow => "Revert Workflow",
            WorkflowPermission.SkipWorkflowSteps => "Skip Workflow Steps",
            _ => permission
        };
    }
}

/// <summary>
/// Extension methods for the workflow module.
/// </summary>
public static class WorkflowModuleExtensions
{
    /// <summary>
    /// Adds the workflow module to the application.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="editorialWorkflowPath">Optional path to the editorial workflow JSON file</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddPiranhaWorkflow(this IServiceCollection services, string editorialWorkflowPath = null)
    {
        // Register the module
        var module = new WorkflowModule();
        
        if (!string.IsNullOrEmpty(editorialWorkflowPath))
        {
            module.EditorialWorkflowPath = editorialWorkflowPath;
        }
        
        // Register the module
        App.Modules.Register<WorkflowModule>();

        // Register services
        services.AddSingleton<WorkflowManager>();
        services.AddScoped<Repositories.IContentWorkflowRepository, Repositories.Internal.ContentWorkflowRepository>();
        services.AddScoped<IWorkflowService, Services.Internal.WorkflowService>();
#if false
        // Manager service reference - temporarily disabled until Manager namespace is properly referenced
        services.AddScoped<global::Piranha.Manager.Services.WorkflowService>();
#endif

        return services;
    }

    /// <summary>
    /// Uses the workflow module in the application.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>The service provider</returns>
    public static IServiceProvider UsePiranhaWorkflow(this IServiceProvider serviceProvider)
    {
        // Register the module if it's not already registered
        if (App.Modules.GetByType(typeof(WorkflowModule)) == null)
        {
            App.Modules.Register<WorkflowModule>();
        }
        
        // Get the workflow manager
        var workflowManager = serviceProvider.GetRequiredService<WorkflowManager>();
        
        // Get the module
        var module = App.Modules.Get<WorkflowModule>();
        
        // Load the editorial workflow
        var path = Path.Combine(
            Path.GetDirectoryName(typeof(WorkflowModule).Assembly.Location), 
            module.EditorialWorkflowPath);
        
        if (File.Exists(path))
        {
            workflowManager.LoadFromFile(path);
        }
        else
        {
            // Try to load from app domain base directory
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, module.EditorialWorkflowPath);
            
            if (File.Exists(path))
            {
                workflowManager.LoadFromFile(path);
            }
        }

        return serviceProvider;
    }
}
