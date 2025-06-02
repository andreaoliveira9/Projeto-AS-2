using Microsoft.EntityFrameworkCore;
using MvcWeb.Data;
using MvcWeb.Services;
using Piranha;
using Piranha.AspNetCore.Identity.SQLite;
using Piranha.AttributeBuilder;
using Piranha.EditorialWorkflow.Extensions;
using Piranha.Notifications.Extensions;
using Piranha.Audit.Extensions;
using Piranha.Manager.Editor;
using Piranha.Data.EF.EditorialWorkflowAndAuditAndNotifications;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
var serviceName = "mvcweb-app";
var serviceVersion = "1.0.0";

builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddScoped<TelemetryHookService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.namespace"] = "piranha"
        }))
    .WithTracing(tracing => tracing
        .AddSource(TelemetryService.WorkflowActivitySource.Name)
        .AddSource(TelemetryService.PageActivitySource.Name)
        .AddSource(TelemetryService.PostActivitySource.Name)
        .AddSource(TelemetryService.MediaActivitySource.Name)
        .AddSource(TelemetryService.ContentActivitySource.Name)
        .AddAspNetCoreInstrumentation(options =>
        {
            options.Filter = (httpContext) =>
            {
                // Filter out health checks and static files
                var path = httpContext.Request.Path.Value?.ToLower() ?? "";
                return !path.Contains("/health") && !path.Contains("/metrics") && !path.StartsWith("/manager/assets");
            };
            options.RecordException = true;
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.FilterHttpRequestMessage = (httpRequestMessage) =>
            {
                // Don't trace calls to telemetry backends
                var host = httpRequestMessage.RequestUri?.Host ?? "";
                return !host.Contains("otel-collector") && !host.Contains("jaeger");
            };
        })
        .AddSqlClientInstrumentation(options =>
        {
            options.SetDbStatementForText = true;
            options.RecordException = true;
        })
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddMeter(TelemetryService.WorkflowMeter.Name)
        .AddMeter(TelemetryService.PageMeter.Name)
        .AddMeter(TelemetryService.PostMeter.Name)
        .AddMeter(TelemetryService.MediaMeter.Name)
        .AddMeter(TelemetryService.SystemMeter.Name)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
        }));

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
    options.AddOtlpExporter(otlpOptions =>
    {
        otlpOptions.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
    });
});

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
        rabbitMQOptions.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        rabbitMQOptions.UserName = builder.Configuration["RabbitMQ:UserName"] ?? "admin";
        rabbitMQOptions.Password = builder.Configuration["RabbitMQ:Password"] ?? "admin";
        rabbitMQOptions.QueueName = "audit.WorkflowStateChanged";
        rabbitMQOptions.MaxRetryAttempts = 5;
    });
    options.UseAuditEF();

    // Notifications 
    options.UseNotifications(rabbitMQOptions => {
        rabbitMQOptions.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        rabbitMQOptions.UserName = builder.Configuration["RabbitMQ:UserName"] ?? "admin";
        rabbitMQOptions.Password = builder.Configuration["RabbitMQ:Password"] ?? "admin";
        rabbitMQOptions.QueueName = "notifications.WorkflowStateChanged";
        rabbitMQOptions.MaxRetryAttempts = 5;
    });
    options.UseNotificationsEF();

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

// Add Prometheus scraping endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Add telemetry middleware
app.UseMiddleware<MvcWeb.Middleware.TelemetryMiddleware>();

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
    
    // Register telemetry hooks
    var hookService = app.Services.GetRequiredService<TelemetryHookService>();
    hookService.RegisterHooks();
});

app.Run();
