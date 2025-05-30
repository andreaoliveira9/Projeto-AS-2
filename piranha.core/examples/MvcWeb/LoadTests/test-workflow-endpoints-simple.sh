#!/bin/bash

# Simple Workflow Endpoints Test (using anonymous debug endpoints)
BASE_URL="${BASE_URL:-http://localhost:5000}"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "Testing Workflow API Endpoints (Anonymous)..."
echo "============================================="
echo "Base URL: $BASE_URL"
echo ""

# Function to print test results
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
    fi
}

# Test debug endpoints (these should work without auth)
echo "1. Testing Debug Endpoints..."

echo -n "   Database connection test... "
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/workflow/debug/database")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Database OK"
else
    print_result 1 "Database test failed (HTTP $HTTP_CODE)"
fi

echo -n "   System roles test... "
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/api/workflow/debug/roles")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ]; then
    ROLES_COUNT=$(echo "$RESPONSE" | sed '$d' | jq length 2>/dev/null || echo "0")
    print_result 0 "Retrieved $ROLES_COUNT roles"
else
    print_result 1 "Roles test failed (HTTP $HTTP_CODE)"
fi

echo ""
echo "2. Testing Metrics Endpoints..."

echo -n "   Triggering test metrics... "
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/test-workflow-metrics")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Metrics triggered"
else
    print_result 1 "Failed to trigger metrics (HTTP $HTTP_CODE)"
fi

echo -n "   Checking metrics endpoint... "
METRICS=$(curl -s "$BASE_URL/metrics" | grep -c "workflow_")
if [ $METRICS -gt 0 ]; then
    print_result 0 "Found $METRICS workflow metric lines"
    echo ""
    echo "   Current workflow metrics:"
    curl -s "$BASE_URL/metrics" | grep "workflow_" | grep -v "_bucket" | grep -v "_sum" | grep -v "_count" | while read line; do
        echo "     $line"
    done
else
    print_result 1 "No workflow metrics found"
fi

echo ""
echo "3. Testing Application Health..."

echo -n "   Root endpoint... "
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "302" ]; then
    print_result 0 "Application responsive"
else
    print_result 1 "Application not responding (HTTP $HTTP_CODE)"
fi

echo -n "   Manager interface... "
RESPONSE=$(curl -s -w "\n%{http_code}" "$BASE_URL/manager")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "302" ]; then
    print_result 0 "Manager interface accessible"
else
    print_result 1 "Manager interface issues (HTTP $HTTP_CODE)"
fi

echo ""
echo "4. Testing Telemetry Stack..."

echo -n "   Prometheus targets... "
PROM_RESPONSE=$(curl -s "http://localhost:9090/api/v1/targets" 2>/dev/null)
if [ $? -eq 0 ]; then
    ACTIVE_TARGETS=$(echo "$PROM_RESPONSE" | jq -r '.data.activeTargets | length' 2>/dev/null || echo "0")
    print_result 0 "Prometheus active with $ACTIVE_TARGETS targets"
else
    print_result 1 "Prometheus not accessible"
fi

echo -n "   Grafana dashboard... "
GRAFANA_RESPONSE=$(curl -s "http://localhost:3000/api/health" 2>/dev/null)
if [ $? -eq 0 ]; then
    print_result 0 "Grafana accessible"
else
    print_result 1 "Grafana not accessible"
fi

echo ""
echo "============================================="
echo "Simple test completed!"

# Show summary of metrics
echo ""
echo "Current Workflow Metrics Summary:"
echo "================================="
curl -s "$BASE_URL/metrics" | grep "workflow_" | grep -v "_bucket" | grep -v "_sum" | grep -v "_count" | wc -l | xargs echo "Total metric lines:"
echo ""
echo "Metric types found:"
curl -s "$BASE_URL/metrics" | grep "^# TYPE.*workflow_" | sed 's/# TYPE /  - /' | sed 's/ .*//'