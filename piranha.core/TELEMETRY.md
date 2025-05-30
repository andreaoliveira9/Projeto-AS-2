# Piranha CMS OpenTelemetry Instrumentation

This document describes the comprehensive OpenTelemetry instrumentation implemented in Piranha CMS for distributed tracing and observability.

## Overview

Piranha CMS now includes built-in OpenTelemetry instrumentation that provides detailed insights into:
- API operations (Pages, Posts, Media, Content)
- Database queries and performance
- Cache operations and hit rates
- Background services (Workflow, Audit)
- HTTP request/response cycles
- Service-to-service communication

## Key Features

### 1. Sensitive Data Masking
All traces automatically mask sensitive information including:
- Email addresses (shows first 2 chars + domain)
- User IDs (shows first 8 chars of GUID)
- IP addresses (shows first 2 octets)
- Tokens and passwords (fully masked)
- Custom sensitive data

### 2. HTTP Method Labeling
All HTTP operations are properly labeled with semantic conventions:
- `http.method`: GET, POST, PUT, DELETE
- `http.route`: The route template
- `http.status_code`: Response status
- `http.url`: Request URL (with sensitive parts masked)

### 3. Service-Specific Tracing

#### Page Service
- Operations: Create, GetAll, GetById, GetStartpage
- Tags: page.count, content.type, content.id, site.id

#### Post Service  
- Operations: Create, GetAll, GetById, GetBySlug
- Tags: post.count, post.archive_id, post.slug

#### Media Service
- Operations: GetAllByFolderId, Upload, Delete
- Tags: media.count, media.folder_id, media.type, media.size

#### Workflow Service
- Operations: Create, Update, Transition, GetAll
- Tags: workflow.id, workflow.state, workflow.transition, user.id (masked)

#### Audit Service
- Operations: ConsumeMessages, ProcessMessage, StoreRecord
- Tags: audit.event_type, audit.entity_id

### 4. Database Tracing
Repository operations include:
- Query type (SELECT, INSERT, UPDATE, DELETE)
- Table name
- Operation context
- Execution time

### 5. Cache Tracing
Cache operations include:
- Operation type (get, set, remove)
- Cache key
- Hit/miss status
- Cache level

## Configuration

### Basic Setup (MvcWeb example)

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "MvcWeb", serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (httpContext) =>
            {
                // Don't trace health/metrics endpoints
                var path = httpContext.Request.Path.Value;
                return !path.StartsWith("/health") && !path.StartsWith("/metrics");
            };
        })
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation()
        .AddSource("MvcWeb")
        .AddSource(PiranhaTelemetry.ServiceName) // Piranha CMS traces
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));
```

### Custom Enrichment

Add Piranha-specific context to all traces:

```csharp
app.Use(async (context, next) =>
{
    var activity = Activity.Current;
    if (activity != null)
    {
        activity.SetTag("service.layer", "web");
        
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            activity.SetTag(PiranhaTelemetry.AttributeNames.UserId, 
                PiranhaTelemetry.MaskSensitiveData(userId, SensitiveDataType.UserId));
        }
    }
    
    await next();
});
```

## Activity Sources

Piranha CMS uses the following activity sources:
- `Piranha.CMS` (version 11.0.0) - Main service operations

## Activity Names

Standard activity naming convention: `{operation_type}.{operation_name}`

Examples:
- `piranha.page.operation.Create`
- `piranha.post.operation.GetById`
- `piranha.workflow.operation.TransitionWorkflow`
- `piranha.cache.operation.Get`
- `piranha.db.operation.SELECT`

## Semantic Conventions

Piranha follows OpenTelemetry semantic conventions with custom attributes:

### Standard Attributes
- `http.method`, `http.status_code`, `http.route`
- `db.system`, `db.operation`, `db.statement`
- `cache.operation`, `cache.key`, `cache.hit`

### Piranha-Specific Attributes
- `piranha.content.type`: Page/Post type ID
- `piranha.content.id`: Content GUID
- `piranha.site.id`: Site GUID
- `piranha.user.id`: User ID (masked)
- `piranha.operation.type`: Detailed operation type
- `piranha.workflow.state`: Current workflow state
- `piranha.workflow.transition`: State transition

## Viewing Traces

Traces can be viewed in:
- **Jaeger UI**: http://localhost:16686
- **Grafana**: http://localhost:3000 (with Tempo data source)
- **Application Insights** (with appropriate exporter)
- **Any OpenTelemetry-compatible backend**

## Performance Considerations

- Tracing adds minimal overhead (typically <1ms per operation)
- Sensitive data masking is performed efficiently in-memory
- Database query tracing can be disabled if needed
- Cache operations are only traced at appropriate cache levels

## Security

- All user IDs and personal information are masked
- Passwords and tokens are never logged
- SQL statements can be optionally included/excluded
- IP addresses are partially masked

## Troubleshooting

### No traces appearing
1. Ensure OpenTelemetry collector is running
2. Check the OTLP endpoint configuration
3. Verify activity sources are added to tracer provider

### Missing service traces
Add the Piranha activity source:
```csharp
.AddSource(PiranhaTelemetry.ServiceName)
```

### Performance impact
Reduce tracing overhead by:
- Filtering out health check endpoints
- Disabling SQL statement capture
- Using sampling for high-traffic services