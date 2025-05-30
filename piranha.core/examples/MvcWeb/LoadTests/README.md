# Workflow Load Tests

This directory contains load testing scripts for the Piranha CMS Editorial Workflow system.

## Files

### Scripts
- `verify-workflow-endpoints.sh` - Full endpoint verification with authentication
- `test-workflow-endpoints-simple.sh` - Simple test using anonymous endpoints
- `test-with-auth.sh` - Authentication testing script

### Load Tests
- `workflow-load-test.js` - Original k6 load test (requires authentication)
- `workflow-load-test-v2.js` - Enhanced k6 load test with multiple scenarios

### Configuration
- `package.json` - npm scripts for running tests

## Quick Start

### 1. Simple Verification (No Auth Required)
```bash
./test-workflow-endpoints-simple.sh
```

### 2. Authentication Test
```bash
./test-with-auth.sh
```

### 3. Load Test with k6
```bash
# Install k6 first
curl https://github.com/grafana/k6/releases/download/v0.47.0/k6-v0.47.0-linux-amd64.tar.gz -L | tar xvz --strip-components 1

# Run load test
k6 run workflow-load-test-v2.js
```

## Test Results Summary

Based on the current testing:

### ✅ Working Components
- **Metrics Generation**: Custom workflow metrics are being created and exposed
- **Prometheus Integration**: Metrics are properly formatted and scrapable
- **Debug Endpoints**: Anonymous endpoints work correctly
- **Test Metrics Endpoint**: `/test-workflow-metrics` works and triggers metrics

### ⚠️ Issues Found
- **Authentication**: The manager authentication is complex (requires CSRF tokens)
- **Metric Naming**: OpenTelemetry is appending units to metric names, causing double naming:
  - `workflow_transitions_total_transitions_total` instead of `workflow_transitions_total`
  - `workflow_transition_duration_ms_milliseconds` instead of `workflow_transition_duration_ms`

### 📊 Current Metrics Being Generated
- `workflow_transitions_total_transitions_total` - Total transitions
- `workflow_instances_created_total_instances_total` - Instances created  
- `workflow_transition_duration_ms_milliseconds` - Transition duration
- `workflow_transitions_by_role_total_transitions_total` - Role-based transitions
- `workflow_content_published_total_items_total` - Content published

## Metrics Endpoints

### Trigger Test Metrics
```bash
curl http://localhost:5000/test-workflow-metrics
```

### View All Metrics
```bash
curl http://localhost:5000/metrics | grep workflow_
```

### Check Prometheus Targets
```bash
curl http://localhost:9090/api/v1/targets
```

## Workflow Operations That Generate Metrics

1. **Instance Creation** - When workflow instances are created via API
2. **State Transitions** - When content moves between workflow states
3. **Content Publishing** - When content reaches published state
4. **Content Rejection** - When content is rejected

## Load Test Scenarios

The v2 load test includes multiple scenarios:

1. **Complete Workflow Creation** - Creates full workflow with states and rules
2. **Existing Endpoints Testing** - Tests read operations
3. **Metrics Triggering** - Focuses on generating metrics data
4. **Stress Testing** - Rapid requests to debug endpoints

## Environment Variables

- `BASE_URL` - Application base URL (default: http://localhost:5000)
- `USERNAME` - Username for authentication (default: admin)
- `PASSWORD` - Password for authentication (default: password)

## Troubleshooting

### No Metrics Appearing
1. Check if application is running: `curl http://localhost:5000/test-workflow-metrics`
2. Verify OpenTelemetry configuration in Program.cs
3. Check if meter name "Piranha.Workflow" is registered

### Authentication Issues
- Use debug endpoints for testing without auth
- Check manager login page for CSRF token requirements
- Verify user credentials

### Prometheus Not Scraping
1. Check Prometheus targets: `curl http://localhost:9090/api/v1/targets`
2. Verify prometheus.yml configuration
3. Check if metrics endpoint is accessible: `curl http://localhost:5000/metrics`

## Example Successful Test Output

```bash
$ ./test-workflow-endpoints-simple.sh

Testing Workflow API Endpoints (Anonymous)...
=============================================

1. Testing Debug Endpoints...
   Database connection test... ✓ Database OK
   System roles test... ✓ Retrieved 0 roles

2. Testing Metrics Endpoints...
   Triggering test metrics... ✓ Metrics triggered
   Checking metrics endpoint... ✓ Found 42 workflow metric lines

3. Testing Application Health...
   Manager interface... ✓ Manager interface accessible

4. Testing Telemetry Stack...
   Prometheus targets... ✓ Prometheus active with 0 targets
   Grafana dashboard... ✓ Grafana accessible
```

This indicates the system is working correctly for metrics generation and exposure.