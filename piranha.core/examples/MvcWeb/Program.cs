using Microsoft.EntityFrameworkCore;
using MvcWeb.Data;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.EditorialWorkflow.Extensions;
using Piranha.Audit.Extensions;
using Piranha.Manager.Editor;
using Piranha.Data.EF.EditorialWorkflowAndAudit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
var serviceName = "MvcWeb";
var serviceVersion = "1.0.0";

// Add services to the container
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource(serviceName)
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter(serviceName)
        .AddPrometheusExporter());

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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
    
    // Editorial Workflow
    options.UseEditorialWorkflow();
    options.UseEditorialWorkflowEF();

    // Audit 
    options.UseAudit(rabbitMQOptions => {
        rabbitMQOptions.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "rabbitmq";
        rabbitMQOptions.UserName = builder.Configuration["RabbitMQ:UserName"] ?? "admin";
        rabbitMQOptions.Password = builder.Configuration["RabbitMQ:Password"] ?? "admin";
        rabbitMQOptions.QueueName = "audit.WorkflowStateChanged";
        rabbitMQOptions.MaxRetryAttempts = 5;
    });
    options.UseAuditEF();

    options.UseFileStorage(naming: Piranha.Local.FileStorageNaming.UniqueFolderNames);
    options.UseImageSharp();
    options.UseTinyMCE();
    options.UseMemoryCache();

    var connectionString = builder.Configuration.GetConnectionString("piranha");
    options.UseEF<SQLiteModulesDb>(db => {
        db.UseSqlite(connectionString);
        // Suppress the pending model changes warning
        db.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    });
    
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline
app.UseOpenTelemetryPrometheusScrapingEndpoint();

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
});

app.Run();
