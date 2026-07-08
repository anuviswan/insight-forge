#!/bin/bash

##
# Insight Forge One-Click Deployment and Testing Script
#
# This script:
# 1. Starts the backend server
# 2. Starts the frontend development server
# 3. Waits for services to be ready
# 4. Runs Playwright E2E tests
#
# Usage:
#   ./deploy.sh [options]
#
# Options:
#   --skip-tests      Skip running tests
#   --headless        Disable headless mode (run with UI visible)
#   --server-port     Backend server port (default: 5000)
#   --client-port     Frontend server port (default: 5173)
#   --timeout         Timeout for service startup in seconds (default: 60)
#

set -e

# Configuration
SKIP_TESTS=false
HEADLESS=true
SERVER_PORT=5000
CLIENT_PORT=5173
TIMEOUT_SECONDS=60
SERVER_PID=""
CLIENT_PID=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-tests)
            SKIP_TESTS=true
            shift
            ;;
        --headless)
            HEADLESS=false
            shift
            ;;
        --server-port)
            SERVER_PORT="$2"
            shift 2
            ;;
        --client-port)
            CLIENT_PORT="$2"
            shift 2
            ;;
        --timeout)
            TIMEOUT_SECONDS="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Utility functions
info() {
    echo -e "${CYAN}ℹ️  $1${NC}"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

error() {
    echo -e "${RED}❌ $1${NC}"
}

warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Get project root
get_project_root() {
    local script_dir
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    echo "$(dirname "$script_dir")"
}

# Test if port is open
test_port() {
    local port=$1
    local service=$2
    local start_time
    local current_time
    local elapsed

    start_time=$(date +%s)

    while true; do
        if nc -z localhost "$port" 2>/dev/null; then
            success "$service is ready on port $port"
            return 0
        fi

        current_time=$(date +%s)
        elapsed=$((current_time - start_time))

        if [ $elapsed -gt $TIMEOUT_SECONDS ]; then
            error "$service failed to start on port $port within $TIMEOUT_SECONDS seconds"
            return 1
        fi

        sleep 0.5
    done
}

# Cleanup on exit
cleanup() {
    info "Cleaning up processes..."

    if [ -n "$SERVER_PID" ]; then
        if kill -0 "$SERVER_PID" 2>/dev/null; then
            kill "$SERVER_PID" 2>/dev/null || true
            info "Stopped backend server (PID: $SERVER_PID)"
        fi
    fi

    if [ -n "$CLIENT_PID" ]; then
        if kill -0 "$CLIENT_PID" 2>/dev/null; then
            kill "$CLIENT_PID" 2>/dev/null || true
            info "Stopped frontend server (PID: $CLIENT_PID)"
        fi
    fi
}

# Set trap for cleanup
trap cleanup EXIT

# Start backend server
start_server() {
    info "Starting backend server..."

    local project_root
    project_root=$(get_project_root)
    local server_path="$project_root/src/server/insight.webapi/insight.webapi"

    if [ ! -d "$server_path" ]; then
        error "Server project not found at $server_path"
        return 1
    fi

    info "Building server..."
    pushd "$server_path" > /dev/null
    if ! dotnet build > /dev/null 2>&1; then
        error "Server build failed"
        popd > /dev/null
        return 1
    fi
    success "Server build successful"

    info "Starting server process..."
    dotnet run > /tmp/insight-server.log 2>&1 &
    SERVER_PID=$!

    popd > /dev/null

    if test_port "$SERVER_PORT" "Backend Server"; then
        return 0
    fi
    return 1
}

# Start frontend server
start_client() {
    info "Starting frontend development server..."

    local project_root
    project_root=$(get_project_root)
    local client_path="$project_root/src/client"

    if [ ! -d "$client_path" ]; then
        error "Client project not found at $client_path"
        return 1
    fi

    pushd "$client_path" > /dev/null

    # Check if node_modules exists
    if [ ! -d "node_modules" ]; then
        info "Installing client dependencies..."
        if ! npm install > /dev/null 2>&1; then
            error "npm install failed"
            popd > /dev/null
            return 1
        fi
    fi

    info "Starting dev server..."
    npm run dev > /tmp/insight-client.log 2>&1 &
    CLIENT_PID=$!

    popd > /dev/null

    if test_port "$CLIENT_PORT" "Frontend Server"; then
        return 0
    fi
    return 1
}

# Run tests
run_tests() {
    info "Running Playwright E2E tests..."

    local project_root
    project_root=$(get_project_root)
    local test_path="$project_root/src/testing"

    if [ ! -d "$test_path" ]; then
        error "Test project not found at $test_path"
        return 1
    fi

    pushd "$test_path" > /dev/null

    info "Building test project..."
    if ! dotnet build > /dev/null 2>&1; then
        error "Test build failed"
        popd > /dev/null
        return 1
    fi

    success "Test build successful"

    info "Starting tests..."
    export Playwright__HeadlessMode=$HEADLESS
    export Playwright__BaseUrl="http://localhost:$CLIENT_PORT"

    if ! dotnet test -v normal; then
        error "Tests failed"
        popd > /dev/null
        return 1
    fi

    success "All tests passed!"
    popd > /dev/null
    return 0
}

# Main execution
main() {
    echo ""
    echo "╔════════════════════════════════════════╗"
    echo "║  Insight Forge Deployment & Test      ║"
    echo "╚════════════════════════════════════════╝"
    echo ""

    if ! start_server; then
        error "Failed to start backend server"
        return 1
    fi

    if ! start_client; then
        error "Failed to start frontend server"
        return 1
    fi

    success "All services started successfully!"
    echo ""
    echo "Services running:"
    echo "  - Backend: http://localhost:$SERVER_PORT"
    echo "  - Frontend: http://localhost:$CLIENT_PORT"
    echo ""

    if [ "$SKIP_TESTS" = false ]; then
        info "Waiting 5 seconds before running tests..."
        sleep 5

        if ! run_tests; then
            return 1
        fi
    fi

    success "Deployment and testing completed successfully!"
    echo ""
    echo "Keep services running? Press Ctrl+C to stop"

    # Keep script running while processes are alive
    wait
}

# Run main function
main
