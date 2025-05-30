#!/bin/bash

# Verify Workflow Endpoints Script
# This script tests all workflow endpoints to ensure they're working correctly

BASE_URL="${BASE_URL:-http://localhost:5000}"
USERNAME="${USERNAME:-admin}"
PASSWORD="${PASSWORD:-password}"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "Testing Workflow API Endpoints..."
echo "================================"
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

# Step 1: Login and get cookie
echo "1. Authenticating..."
LOGIN_RESPONSE=$(curl -s -c cookies.txt -X POST "$BASE_URL/manager/login" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "username=$USERNAME&password=$PASSWORD" \
    -w "\n%{http_code}")

HTTP_CODE=$(echo "$LOGIN_RESPONSE" | tail -n 1)
if [ "$HTTP_CODE" = "302" ] || [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Authentication successful"
else
    print_result 1 "Authentication failed (HTTP $HTTP_CODE)"
    exit 1
fi

# Helper function for API calls
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
        echo "$BODY"
        return 1
    fi
}

echo ""
echo "2. Testing Workflow Definition Endpoints..."

# Create workflow definition
WORKFLOW_DEF='{
    "name": "Test Workflow '"$(date +%s)"'",
    "description": "Created by endpoint verification script",
    "isActive": true,
    "version": 1
}'

echo -n "   Creating workflow definition... "
CREATE_RESULT=$(api_call POST "/api/workflow/definitions" "$WORKFLOW_DEF" 201)
if [ $? -eq 0 ]; then
    WORKFLOW_ID=$(echo "$CREATE_RESULT" | grep -o '"id":"[^"]*' | cut -d'"' -f4)
    print_result 0 "Created (ID: $WORKFLOW_ID)"
else
    print_result 1 "Failed to create workflow definition"
    echo "$CREATE_RESULT"
    exit 1
fi

# Get workflow definition
echo -n "   Getting workflow definition... "
api_call GET "/api/workflow/definitions/$WORKFLOW_ID" "" 200 > /dev/null
print_result $? "Retrieved workflow definition"

# Get all definitions with stats
echo -n "   Getting definitions with stats... "
api_call GET "/api/workflow/definitions/with-stats" "" 200 > /dev/null
print_result $? "Retrieved definitions with stats"

echo ""
echo "3. Testing Workflow State Endpoints..."

# Create states
declare -A STATES
STATES=(
    ["draft"]='{"stateId":"draft","name":"Draft","workflowDefinitionId":"'$WORKFLOW_ID'","isInitial":true,"isPublished":false,"isFinal":false,"sortOrder":1}'
    ["review"]='{"stateId":"review","name":"Review","workflowDefinitionId":"'$WORKFLOW_ID'","isInitial":false,"isPublished":false,"isFinal":false,"sortOrder":2}'
    ["published"]='{"stateId":"published","name":"Published","workflowDefinitionId":"'$WORKFLOW_ID'","isInitial":false,"isPublished":true,"isFinal":true,"sortOrder":3}'
)

declare -A STATE_IDS

for state_key in "${!STATES[@]}"; do
    echo -n "   Creating state '$state_key'... "
    STATE_RESULT=$(api_call POST "/api/workflow/states" "${STATES[$state_key]}" 201)
    if [ $? -eq 0 ]; then
        STATE_ID=$(echo "$STATE_RESULT" | grep -o '"id":"[^"]*' | cut -d'"' -f4)
        STATE_IDS[$state_key]=$STATE_ID
        print_result 0 "Created (ID: $STATE_ID)"
    else
        print_result 1 "Failed to create state"
        echo "$STATE_RESULT"
    fi
done

# Get workflow states
echo -n "   Getting workflow states... "
api_call GET "/api/workflow/definitions/$WORKFLOW_ID/states" "" 200 > /dev/null
print_result $? "Retrieved workflow states"

echo ""
echo "4. Testing Transition Rule Endpoints..."

# Create transition rules
TRANSITIONS=(
    "draft:review"
    "review:published"
)

for transition in "${TRANSITIONS[@]}"; do
    IFS=':' read -r from to <<< "$transition"
    FROM_ID=${STATE_IDS[$from]}
    TO_ID=${STATE_IDS[$to]}
    
    if [ ! -z "$FROM_ID" ] && [ ! -z "$TO_ID" ]; then
        RULE='{
            "fromStateId":"'$FROM_ID'",
            "toStateId":"'$TO_ID'",
            "allowedRoles":"[\"Admin\",\"Editor\"]",
            "description":"Transition from '$from' to '$to'",
            "isActive":true
        }'
        
        echo -n "   Creating rule $from -> $to... "
        api_call POST "/api/workflow/rules" "$RULE" 201 > /dev/null
        print_result $? "Created transition rule"
    fi
done

# Get workflow rules
echo -n "   Getting workflow rules... "
api_call GET "/api/workflow/definitions/$WORKFLOW_ID/rules" "" 200 > /dev/null
print_result $? "Retrieved workflow rules"

echo ""
echo "5. Testing Workflow Instance Endpoints..."

# Create workflow instance
INSTANCE='{
    "workflowDefinitionId":"'$WORKFLOW_ID'",
    "contentId":"test-content-'$(date +%s)'",
    "contentType":"page"
}'

echo -n "   Creating workflow instance... "
INSTANCE_RESULT=$(api_call POST "/api/workflow/instances" "$INSTANCE" 201)
if [ $? -eq 0 ]; then
    INSTANCE_ID=$(echo "$INSTANCE_RESULT" | grep -o '"id":"[^"]*' | cut -d'"' -f4)
    print_result 0 "Created instance (ID: $INSTANCE_ID)"
    
    # Try to transition
    echo -n "   Performing workflow transition... "
    api_call POST "/api/workflow/instances/$INSTANCE_ID/transition" '"review"' 200 > /dev/null
    print_result $? "Transition successful"
else
    print_result 1 "Failed to create instance"
    echo "$INSTANCE_RESULT"
fi

echo ""
echo "6. Testing Debug Endpoints..."

echo -n "   Getting system roles... "
api_call GET "/api/workflow/debug/roles" "" 200 > /dev/null
print_result $? "Retrieved system roles"

echo -n "   Testing database connection... "
api_call GET "/api/workflow/debug/database" "" 200 > /dev/null
print_result $? "Database connection OK"

echo ""
echo "7. Triggering Test Metrics..."
echo -n "   Triggering workflow metrics... "
curl -s "$BASE_URL/test-workflow-metrics" > /dev/null
print_result $? "Metrics triggered"

echo ""
echo "8. Checking Metrics Endpoint..."
echo -n "   Checking for workflow metrics... "
METRICS=$(curl -s "$BASE_URL/metrics" | grep -c "workflow_")
if [ $METRICS -gt 0 ]; then
    print_result 0 "Found $METRICS workflow metric lines"
    echo ""
    echo "   Sample metrics:"
    curl -s "$BASE_URL/metrics" | grep "workflow_" | grep -v "bucket" | head -5 | while read line; do
        echo "     $line"
    done
else
    print_result 1 "No workflow metrics found"
fi

# Cleanup
rm -f cookies.txt

echo ""
echo "================================"
echo "Test completed!"