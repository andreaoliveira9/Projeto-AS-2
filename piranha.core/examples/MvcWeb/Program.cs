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
using Piranha.Telemetry;
using System.Diagnostics;
using MvcWeb.Telemetry;
using Piranha.EditorialWorkflow.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry
var serviceName = "MvcWeb";
var serviceVersion = "1.0.0";

// Configure Piranha telemetry sources
var piranhaServiceName = PiranhaTelemetry.ServiceName;

// Add services to the container
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: serviceName,
            serviceVersion: serviceVersion))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            // Enrich spans with additional information
            options.RecordException = true;
            options.Filter = (httpContext) =>
            {
                // Don't trace health checks or metrics endpoints
                var path = httpContext.Request.Path.Value;
                return !path.StartsWith("/health") && !path.StartsWith("/metrics");
            };
            options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("http.request.method", httpRequest.Method);
                activity.SetTag("http.request.content_type", httpRequest.ContentType);
                
                // Set more intuitive operation names based on the route
                var path = httpRequest.Path.Value;
                if (path.Contains("/api/workflow/"))
                {
                    activity.DisplayName = $"WorkflowAPI: {httpRequest.Method} {path}";
                }
                else if (path.Contains("/api/page/"))
                {
                    activity.DisplayName = $"PageAPI: {httpRequest.Method} {path}";
                }
                else if (path.Contains("/api/post/"))
                {
                    activity.DisplayName = $"PostAPI: {httpRequest.Method} {path}";
                }
                else if (path.Contains("/manager/"))
                {
                    activity.DisplayName = $"Manager: {httpRequest.Method} {path}";
                }
            };
            options.EnrichWithHttpResponse = (activity, httpResponse) =>
            {
                activity.SetTag("http.response.content_type", httpResponse.ContentType);
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.FilterHttpRequestMessage = (httpRequestMessage) =>
            {
                // Don't trace requests to telemetry endpoints
                return !httpRequestMessage.RequestUri?.Host.Contains("localhost:4317") ?? true;
            };
        })
        .AddSqlClientInstrumentation(options =>
        {
            options.RecordException = true;
            options.SetDbStatementForText = true;
            options.EnableConnectionLevelAttributes = true;
        })
        .AddSource(serviceName)
        .AddSource(piranhaServiceName) // Add Piranha CMS telemetry source
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
        .AddMeter("Piranha.Workflow") // Add our workflow metrics meter
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:4317");
        })
        .AddPrometheusExporter()); // Keep both for direct access

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
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

// Add workflow metrics services
builder.Services.AddHostedService<WorkflowMetricsService>();
builder.Services.AddHostedService<WorkflowMetricsEventHandler>();
builder.Services.AddHostedService<TestMetricsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline
app.UseOpenTelemetryPrometheusScrapingEndpoint();

// Test endpoint to trigger workflow metrics
app.MapGet("/test-workflow-metrics", () =>
{
    // Test all workflow metrics
    var tags = new KeyValuePair<string, object?>[]
    {
        new("workflow_id", "test-workflow"),
        new("from_state", "draft"),
        new("to_state", "published"),
        new("content_type", "page"),
        new("user_role", "editor"),
        new("success", "true")
    };
    
    // Trigger all metrics
    WorkflowMetricsProvider.TransitionCount.Add(1, tags);
    WorkflowMetricsProvider.InstancesCreated.Add(1, tags);
    WorkflowMetricsProvider.TransitionDuration.Record(250.5, tags);
    WorkflowMetricsProvider.TransitionsByRole.Add(1, tags);
    WorkflowMetricsProvider.ContentPublished.Add(1, tags);
    
    return Results.Json(new { 
        message = "Workflow metrics triggered successfully",
        metrics = new[] {
            "workflow_transitions_total",
            "workflow_instances_created_total", 
            "workflow_transition_duration_ms",
            "workflow_transitions_by_role_total",
            "workflow_content_published_total"
        }
    });
});

// Add workflow metrics middleware
app.UseWorkflowMetrics();

// Add custom middleware to enrich traces with Piranha context
app.Use(async (context, next) =>
{
    var activity = Activity.Current;
    if (activity != null)
    {
        // Add custom tags for better trace identification
        activity.SetTag("service.layer", "web");
        activity.SetTag("app.name", "MvcWeb");
        
        // Add user context if authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.Identity.Name;
            activity.SetTag(PiranhaTelemetry.AttributeNames.UserId, 
                PiranhaTelemetry.MaskSensitiveData(userId, Piranha.Telemetry.SensitiveDataType.UserId));
        }
    }
    
    await next();
});

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
