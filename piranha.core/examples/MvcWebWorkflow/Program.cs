using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.Data.EF.SQLite;
using Piranha.Manager.Editor;
using Piranha.Manager.Workflow;
using Piranha.Workflow;
using Piranha.Workflow.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.AddPiranha(options =>
{
    /**
     * This will enable automatic reload of .cshtml
     * without restarting the application. However since
     * this adds a slight overhead it should not be
     * enabled in production.
     */
    options.AddRazorRuntimeCompilation = true;

    options.UseCms();
    options.UseManager();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();

    var connectionString = builder.Configuration.GetConnectionString("piranha");
    options.UseEF<SQLiteDb>(db => db.UseSqlite(connectionString));
    options.UseIdentityWithSeed<IdentitySQLiteDb>(db => db.UseSqlite(connectionString));

    /**
     * Here you can configure the different permissions
     * that you want to use for securing content in the
     * application.
    options.UseSecurity(o =>
    {
        o.UsePermission("WebUser", "Web User");
    });
     */

    /**
     * Here you can specify the login url for the front end
     * application. This does not affect the login url of
     * the manager interface.
    options.LoginUrl = "login";
     */
});

// Add the workflow modules
builder.Services.AddPiranhaWorkflow();
builder.Services.AddPiranhaManagerWorkflow();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use the Piranha Manager Workflow middleware for static files
// Explicitly specify that we want the IApplicationBuilder version
IApplicationBuilder appBuilder = app;
appBuilder.UsePiranhaManagerWorkflow();

app.UsePiranha(options =>
{
    // Initialize Piranha
    App.Init(options.Api);

    // Build content types
    new ContentTypeBuilder(options.Api)
        .AddAssembly(typeof(Program).Assembly)
        .Build()
        .DeleteOrphans();

    // Configure Tiny MCE
    EditorConfig.FromFile("editorconfig.json");

    options.UseManager();
    options.UseTinyMCE();
    options.UseIdentity();

    // Initialize the workflow module
    InitializeWorkflow(app.Services);
});

// Register the workflow controller routes
app.MapControllerRoute(
    name: "workflow-list",
    pattern: "manager/workflows",
    defaults: new { area = "Manager", controller = "Workflow", action = "List" }
);

app.MapControllerRoute(
    name: "workflow-details",
    pattern: "manager/workflow/{id:Guid}",
    defaults: new { area = "Manager", controller = "Workflow", action = "Details" }
);

app.Run();

// Helper method to initialize a default workflow
void InitializeWorkflow(IServiceProvider services)
{
    try
    {
        // Get the workflow service
        var workflowService = services.GetRequiredService<IWorkflowService>();

        // Create a standard editorial workflow
        var standardWorkflow = WorkflowDefaults.CreateStandardEditorialWorkflow();
        workflowService.SaveWorkflowDefinitionAsync(standardWorkflow).GetAwaiter().GetResult();
        Console.WriteLine($"Created standard editorial workflow: {standardWorkflow.Name}");

        // Create a simple workflow
        var simpleWorkflow = WorkflowDefaults.CreateSimpleWorkflow();
        workflowService.SaveWorkflowDefinitionAsync(simpleWorkflow).GetAwaiter().GetResult();
        Console.WriteLine($"Created simple workflow: {simpleWorkflow.Name}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing workflows: {ex.Message}");
    }
}