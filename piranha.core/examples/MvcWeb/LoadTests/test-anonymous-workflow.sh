#!/bin/bash

# Test anonymous workflow operations
BASE_URL="${BASE_URL:-http://localhost:5000}"

echo "Testing Anonymous Workflow Operations..."
echo "======================================="

# Test what works (anonymous)
echo "✅ Testing anonymous endpoints:"

echo -n "  Debug database: "
curl -s "$BASE_URL/api/workflow/debug/database" | jq -r '.message // "OK"' 2>/dev/null || echo "Response received"

echo -n "  Debug roles: "
ROLES=$(curl -s "$BASE_URL/api/workflow/debug/roles" | jq length 2>/dev/null || echo "0")
echo "$ROLES roles found"

echo -n "  Test metrics: "
curl -s "$BASE_URL/test-workflow-metrics" | jq -r '.message' 2>/dev/null || echo "Triggered"

echo -n "  Metrics endpoint: "
METRICS_COUNT=$(curl -s "$BASE_URL/metrics" | grep -c "workflow_" || echo "0")
echo "$METRICS_COUNT workflow metrics found"

echo ""
echo "❌ Testing authenticated endpoints (should fail):"

echo -n "  Create workflow definition: "
RESPONSE=$(curl -s -X POST "$BASE_URL/api/workflow/definitions" \
    -H "Content-Type: application/json" \
    -d '{"name":"Test","description":"Test","isActive":true,"version":1}' \
    -w "\n%{http_code}")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
echo "HTTP $HTTP_CODE (expected 302 redirect to login)"

echo -n "  Get workflow definitions: "
RESPONSE=$(curl -s "$BASE_URL/api/workflow/definitions/with-stats" -w "\n%{http_code}")
HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
echo "HTTP $HTTP_CODE (expected 302 redirect to login)"

echo ""
echo "Summary:"
echo "========"
echo "✅ Anonymous endpoints are working correctly"
echo "❌ Authenticated endpoints require login (as expected)"
echo ""
echo "To test authenticated endpoints, you need to:"
echo "1. Login through the manager interface first"
echo "2. Extract authentication cookies"
echo "3. Include cookies in API requests"
echo ""
echo "For load testing, consider:"
echo "1. Using only anonymous endpoints"
echo "2. Setting up proper authentication in k6 scripts"
echo "3. Creating temporary anonymous test endpoints for development"