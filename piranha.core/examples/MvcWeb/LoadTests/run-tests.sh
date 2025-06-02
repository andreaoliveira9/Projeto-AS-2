#!/bin/bash

# Load Testing Script for Piranha CMS
# This script runs k6 load tests with different scenarios

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Default values
BASE_URL="${BASE_URL:-http://localhost:8080}"
ENVIRONMENT="${ENVIRONMENT:-Testing}"
K6_PROMETHEUS_RW_SERVER_URL="${K6_PROMETHEUS_RW_SERVER_URL:-http://localhost:9090/api/v1/write}"
OUTPUT_DIR="./results"

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if k6 is installed
check_k6() {
    if ! command -v k6 &> /dev/null; then
        print_error "k6 is not installed. Please install k6 first."
        echo "Installation instructions: https://k6.io/docs/getting-started/installation/"
        exit 1
    fi
    print_info "k6 is installed: $(k6 version)"
}

# Function to create output directory
setup_output_dir() {
    mkdir -p "$OUTPUT_DIR"
    print_info "Output directory created: $OUTPUT_DIR"
}

# Function to run a specific test
run_test() {
    local test_name=$1
    local script_path=$2
    local additional_args=$3
    
    print_info "Running test: $test_name"
    echo "Script: $script_path"
    echo "Environment: $ENVIRONMENT"
    echo "Base URL: $BASE_URL"
    echo "----------------------------------------"
    
    # Build k6 command
    local cmd="k6 run"
    cmd="$cmd -e BASE_URL=$BASE_URL"
    cmd="$cmd -e ENVIRONMENT=$ENVIRONMENT"
    cmd="$cmd --out json=$OUTPUT_DIR/${test_name}_$(date +%Y%m%d_%H%M%S).json"
    
    # Add Prometheus output if configured
    if [ ! -z "$K6_PROMETHEUS_RW_SERVER_URL" ]; then
        cmd="$cmd --out experimental-prometheus-rw"
    fi
    
    # Add additional arguments if provided
    if [ ! -z "$additional_args" ]; then
        cmd="$cmd $additional_args"
    fi
    
    # Add script path
    cmd="$cmd $script_path"
    
    # Execute the test
    eval $cmd
    
    if [ $? -eq 0 ]; then
        print_info "Test $test_name completed successfully"
    else
        print_error "Test $test_name failed"
        return 1
    fi
}

# Function to show usage
usage() {
    echo "Usage: $0 [OPTIONS] [TEST_TYPE]"
    echo ""
    echo "TEST_TYPE:"
    echo "  workflow    - Run workflow load tests"
    echo "  content     - Run content (pages/posts) load tests"
    echo "  page-workflow - Run page creation with workflow tests"
    echo "  full        - Run comprehensive load tests (default)"
    echo "  all         - Run all test types sequentially"
    echo ""
    echo "OPTIONS:"
    echo "  -u, --url URL              Base URL (default: http://localhost:8080)"
    echo "  -e, --env ENVIRONMENT      Environment (default: Testing)"
    echo "  -d, --duration DURATION    Test duration (default: varies by test)"
    echo "  -v, --vus VUS             Number of virtual users"
    echo "  -h, --help                Show this help message"
    echo ""
    echo "EXAMPLES:"
    echo "  $0 workflow"
    echo "  $0 -u http://mvcweb-app:8080 -e Testing full"
    echo "  $0 --vus 50 --duration 5m content"
}

# Parse command line arguments
TEST_TYPE="full"
EXTRA_ARGS=""

while [[ $# -gt 0 ]]; do
    case $1 in
        -u|--url)
            BASE_URL="$2"
            shift 2
            ;;
        -e|--env)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -d|--duration)
            EXTRA_ARGS="$EXTRA_ARGS --duration $2"
            shift 2
            ;;
        -v|--vus)
            EXTRA_ARGS="$EXTRA_ARGS --vus $2"
            shift 2
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        workflow|content|page-workflow|full|all)
            TEST_TYPE="$1"
            shift
            ;;
        *)
            print_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Main execution
print_info "Starting Piranha CMS Load Tests"
check_k6
setup_output_dir

# Export environment variables for k6
export K6_PROMETHEUS_RW_SERVER_URL
export K6_PROMETHEUS_RW_TREND_AS_NATIVE_HISTOGRAM=true

case $TEST_TYPE in
    workflow)
        run_test "workflow" "./scripts/workflow-tests.js" "$EXTRA_ARGS"
        ;;
    content)
        run_test "content" "./scripts/content-tests.js" "$EXTRA_ARGS"
        ;;
    page-workflow)
        run_test "page-workflow" "./scripts/page-workflow-focused-test.js" "$EXTRA_ARGS"
        ;;
    full)
        run_test "full" "./scripts/full-load-test.js" "$EXTRA_ARGS"
        ;;
    all)
        run_test "workflow" "./scripts/workflow-tests.js" "$EXTRA_ARGS"
        echo ""
        run_test "content" "./scripts/content-tests.js" "$EXTRA_ARGS"
        echo ""
        run_test "page-workflow" "./scripts/page-workflow-focused-test.js" "$EXTRA_ARGS"
        echo ""
        run_test "full" "./scripts/full-load-test.js" "$EXTRA_ARGS"
        ;;
    *)
        print_error "Invalid test type: $TEST_TYPE"
        usage
        exit 1
        ;;
esac

print_info "All tests completed. Results saved in: $OUTPUT_DIR"

# Generate summary report
if [ -f "$OUTPUT_DIR/summary.html" ]; then
    print_info "HTML report generated: $OUTPUT_DIR/summary.html"
fi

# Show quick stats if jq is available
if command -v jq &> /dev/null && [ -f "$OUTPUT_DIR/summary.json" ]; then
    echo ""
    print_info "Quick Summary:"
    jq '.metrics.http_req_duration.values | {p95, p99, avg}' "$OUTPUT_DIR/summary.json" 2>/dev/null || true
fi