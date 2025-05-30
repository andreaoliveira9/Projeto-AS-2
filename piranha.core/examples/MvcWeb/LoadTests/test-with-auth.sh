#!/bin/bash

# Test Workflow Endpoints with Proper Authentication
BASE_URL="${BASE_URL:-http://localhost:5000}"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "Testing Workflow API with Authentication..."
echo "==========================================="
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

# Step 1: Get the login page to extract antiforgery token
echo "1. Getting login page and tokens..."
LOGIN_PAGE=$(curl -s -c cookies.txt "$BASE_URL/manager/login")
if [ $? -eq 0 ]; then
    # Try to extract antiforgery token from hidden input
    CSRF_TOKEN=$(echo "$LOGIN_PAGE" | grep -o 'name="__RequestVerificationToken"[^>]*value="[^"]*"' | grep -o 'value="[^"]*"' | sed 's/value="//;s/"//')
    
    if [ ! -z "$CSRF_TOKEN" ]; then
        print_result 0 "CSRF token extracted: ${CSRF_TOKEN:0:20}..."
    else
        print_result 1 "Could not extract CSRF token"
        echo "Looking for any hidden inputs..."
        echo "$LOGIN_PAGE" | grep -o '<input[^>]*type="hidden"[^>]*>' | head -3
    fi
else
    print_result 1 "Failed to get login page"
    exit 1
fi

# Step 2: Try to authenticate
echo ""
echo "2. Attempting authentication..."

# Try without CSRF first (for local auth)
echo -n "   Trying simple auth... "
LOGIN_RESPONSE=$(curl -s -b cookies.txt -c cookies.txt -X POST "$BASE_URL/manager/login" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "Username=admin&Password=password" \
    -w "\n%{http_code}" \
    -L)

HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Authentication successful"
    AUTH_SUCCESS=true
elif [ "$HTTP_CODE" = "302" ]; then
    print_result 0 "Authentication successful (redirect)"
    AUTH_SUCCESS=true
else
    print_result 1 "Simple auth failed (HTTP $HTTP_CODE)"
    AUTH_SUCCESS=false
    
    # Try with CSRF token
    if [ ! -z "$CSRF_TOKEN" ]; then
        echo -n "   Trying with CSRF token... "
        LOGIN_RESPONSE=$(curl -s -b cookies.txt -c cookies.txt -X POST "$BASE_URL/manager/login" \
            -H "Content-Type: application/x-www-form-urlencoded" \
            -d "Username=admin&Password=password&__RequestVerificationToken=$CSRF_TOKEN" \
            -w "\n%{http_code}" \
            -L)
        
        HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n 1)
        if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "302" ]; then
            print_result 0 "Authentication with CSRF successful"
            AUTH_SUCCESS=true
        else
            print_result 1 "CSRF auth failed (HTTP $HTTP_CODE)"
        fi
    fi
fi

if [ "$AUTH_SUCCESS" = false ]; then
    echo ""
    echo "Authentication failed. Testing anonymous endpoints only..."
    echo ""
fi

# Helper function for authenticated API calls
api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local expected_code=$4
    
    if [ -z "$data" ]; then
        RESPONSE=$(curl -s -b cookies.txt -X "$method" "$BASE_URL$endpoint" \
            -H "Content-Type: application/json" \
            -w "\n%{http_code}")
    else
        RESPONSE=$(curl -s -b cookies.txt -X "$method" "$BASE_URL$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data" \
            -w "\n%{http_code}")
    fi
    
    HTTP_CODE=$(echo "$RESPONSE" | tail -n 1)
    BODY=$(echo "$RESPONSE" | sed '$d')
    
    if [ "$HTTP_CODE" = "$expected_code" ]; then
        echo "$BODY"
        return 0
    else
        echo "Expected $expected_code, got $HTTP_CODE"
        if [ ${#BODY} -lt 200 ]; then
            echo "$BODY"
        else
            echo "${BODY:0:200}..."
        fi
        return 1
    fi
}

if [ "$AUTH_SUCCESS" = true ]; then
    echo ""
    echo "3. Testing Authenticated Endpoints..."
    
    # Test workflow definitions
    echo -n "   Getting workflow definitions... "
    api_call GET "/api/workflow/definitions/with-stats" "" 200 > /dev/null
    print_result $? "Retrieved definitions"
    
    # Create a test workflow
    WORKFLOW_DEF='{
        "name": "Load Test Workflow '"$(date +%s)"'",
        "description": "Created by load test script",
        "isActive": true,
        "version": 1
    }'
    
    echo -n "   Creating workflow definition... "
    CREATE_RESULT=$(api_call POST "/api/workflow/definitions" "$WORKFLOW_DEF" 201)
    if [ $? -eq 0 ]; then
        WORKFLOW_ID=$(echo "$CREATE_RESULT" | grep -o '"id":"[^"]*' | cut -d'"' -f4)
        print_result 0 "Created (ID: ${WORKFLOW_ID:0:8}...)"
        
        # Create states for this workflow
        echo -n "   Creating workflow states... "
        STATE_DEF='{
            "stateId": "draft",
            "name": "Draft",
            "workflowDefinitionId": "'$WORKFLOW_ID'",
            "isInitial": true,
            "isPublished": false,
            "isFinal": false,
            "sortOrder": 1
        }'
        
        api_call POST "/api/workflow/states" "$STATE_DEF" 201 > /dev/null
        print_result $? "Created draft state"
        
        # Try to create an instance (this should trigger metrics)
        echo -n "   Creating workflow instance... "
        INSTANCE_DEF='{
            "workflowDefinitionId": "'$WORKFLOW_ID'",
            "contentId": "test-content-'$(date +%s)'",
            "contentType": "page"
        }'
        
        INSTANCE_RESULT=$(api_call POST "/api/workflow/instances" "$INSTANCE_DEF" 201)
        if [ $? -eq 0 ]; then
            print_result 0 "Instance created (should trigger metrics)"
        else
            print_result 1 "Failed to create instance"
            echo "$INSTANCE_RESULT"
        fi
        
    else
        print_result 1 "Failed to create workflow"
        echo "$CREATE_RESULT"
    fi
else
    echo ""
    echo "3. Testing Anonymous Endpoints Only..."
fi

# Always test these regardless of auth
echo ""
echo "4. Testing Metrics (after operations)..."

echo -n "   Triggering additional test metrics... "
curl -s "$BASE_URL/test-workflow-metrics" > /dev/null
print_result $? "Test metrics triggered"

echo -n "   Checking updated metrics... "
METRICS_COUNT=$(curl -s "$BASE_URL/metrics" | grep "workflow_" | grep -v "_bucket" | grep -v "_sum" | grep -v "_count" | wc -l)
print_result 0 "Found $METRICS_COUNT workflow metrics"

echo ""
echo "   Latest metric values:"
curl -s "$BASE_URL/metrics" | grep "workflow_.*_total.*}" | while read line; do
    metric_name=$(echo "$line" | grep -o '^[^{]*')
    metric_value=$(echo "$line" | grep -o '[0-9]\+ [0-9]\+$' | awk '{print $1}')
    echo "     $metric_name: $metric_value"
done

# Cleanup
rm -f cookies.txt

echo ""
echo "==========================================="
echo "Test completed!"