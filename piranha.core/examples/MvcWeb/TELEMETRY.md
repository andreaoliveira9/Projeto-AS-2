# MvcWeb Application Telemetry Setup

This setup provides full observability for the MvcWeb application using:
- **Jaeger** for distributed tracing
- **Prometheus** for metrics collection
- **Grafana** for visualization
- **OpenTelemetry** for instrumentation

## Prerequisites

- Docker and Docker Compose installed
- .NET 9.0 SDK (for local development)

## Quick Start

1. **Start all services**:
```bash
docker-compose up -d
```

2. **Access the services**:
- **MvcWeb Application**: http://localhost:5001
- **Jaeger UI**: http://localhost:16686
- **Prometheus**: http://localhost:9090
- **Grafana**: http://localhost:3000 (admin/admin)
- **RabbitMQ Management**: http://localhost:15673 (admin/admin)

## Architecture

The application uses OpenTelemetry to export:
- **Traces** to Jaeger (via OTLP)
- **Metrics** to Prometheus (via scraping endpoint at `/metrics`)
- **Logs** to stdout (can be configured for other exporters)

## Telemetry Features

### Automatic Instrumentation
- ASP.NET Core requests
- HTTP client calls
- SQL client operations
- Runtime metrics (CPU, memory, GC)
- Process metrics

### Custom Metrics
The application exposes standard ASP.NET Core metrics including:
- Request rate
- Request duration
- Active requests
- Failed requests

### Distributed Tracing
All requests are automatically traced with:
- Request/response details
- Database queries
- External HTTP calls
- Custom spans

## Grafana Dashboards

A pre-configured dashboard is included with:
- Request rate graph
- 95th percentile response time
- CPU usage
- Memory usage

## Troubleshooting

1. **Check container status**:
```bash
docker-compose ps
```

2. **View logs**:
```bash
docker-compose logs -f mvcweb
```

3. **Verify metrics endpoint**:
```bash
curl http://localhost:5001/metrics
```

## Configuration

### Environment Variables
- `OTEL_EXPORTER_OTLP_ENDPOINT`: OpenTelemetry collector endpoint
- `OTEL_SERVICE_NAME`: Service name for telemetry
- `ASPNETCORE_ENVIRONMENT`: Application environment

### Adding Custom Metrics
Add custom metrics in your code:
```csharp
var meter = new Meter("MvcWeb");
var counter = meter.CreateCounter<long>("custom_operations_total");
counter.Add(1, new KeyValuePair<string, object?>("operation", "example"));
```

### Adding Custom Traces
Add custom spans:
```csharp
using var activity = Activity.StartActivity("CustomOperation");
activity?.SetTag("custom.tag", "value");
// Your operation here
```