#!/usr/bin/env pwsh
<#
.SYNOPSIS
    One-click deployment and testing script for Insight Forge

.DESCRIPTION
    This script:
    1. Starts the backend server
    2. Starts the frontend development server
    3. Waits for services to be ready
    4. Runs Playwright E2E tests

.EXAMPLE
    .\deploy.ps1

.NOTES
    Requires:
    - .NET SDK
    - Node.js and npm
    - PowerShell 7+
#>

param(
    [switch]$SkipTests = $false,
    [switch]$Headless = $true,
    [int]$ServerPort = 5000,
    [int]$ClientPort = 5173,
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"
$WarningPreference = "Continue"

# Colors for output
$Colors = @{
    Success = 'Green'
    Error = 'Red'
    Warning = 'Yellow'
    Info = 'Cyan'
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Colors.Info
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Colors.Success
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Colors.Error
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Colors.Warning
}

function Test-Port {
    param(
        [int]$Port,
        [string]$Service
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        try {
            $connection = New-Object System.Net.Sockets.TcpClient("127.0.0.1", $Port)
            if ($connection.Connected) {
                $connection.Close()
                Write-Success "$Service is ready on port $Port"
                return $true
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    Write-Error-Custom "$Service failed to start on port $Port within $TimeoutSeconds seconds"
    return $false
}

function Get-ProjectRoot {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    return Split-Path -Parent $scriptDir
}

function Start-Server {
    Write-Info "Starting backend server..."

    $projectRoot = Get-ProjectRoot
    $serverPath = Join-Path $projectRoot "src\server\insight.webapi\insight.webapi"

    if (-not (Test-Path $serverPath)) {
        Write-Error-Custom "Server project not found at $serverPath"
        return $false
    }

    Write-Info "Building server..."
    Push-Location $serverPath
    try {
        $buildOutput = & dotnet build 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Server build failed"
            Write-Host $buildOutput
            return $false
        }
        Write-Success "Server build successful"

        Write-Info "Starting server process..."
        $serverProcess = Start-Process `
            -FilePath "dotnet" `
            -ArgumentList "run" `
            -WindowStyle Hidden `
            -PassThru `
            -NoNewWindow

        # Store process ID for cleanup
        Set-Variable -Name "ServerProcessId" -Value $serverProcess.Id -Scope Script

        # Wait for server to be ready
        if (Test-Port -Port $ServerPort -Service "Backend Server") {
            return $true
        }
        return $false
    }
    finally {
        Pop-Location
    }
}

function Start-Client {
    Write-Info "Starting frontend development server..."

    $projectRoot = Get-ProjectRoot
    $clientPath = Join-Path $projectRoot "src\client"

    if (-not (Test-Path $clientPath)) {
        Write-Error-Custom "Client project not found at $clientPath"
        return $false
    }

    Push-Location $clientPath
    try {
        # Check if node_modules exists
        if (-not (Test-Path "node_modules")) {
            Write-Info "Installing client dependencies..."
            & npm install
            if ($LASTEXITCODE -ne 0) {
                Write-Error-Custom "npm install failed"
                return $false
            }
        }

        Write-Info "Starting dev server..."
        $clientProcess = Start-Process `
            -FilePath "npm" `
            -ArgumentList "run", "dev" `
            -WindowStyle Hidden `
            -PassThru `
            -NoNewWindow

        Set-Variable -Name "ClientProcessId" -Value $clientProcess.Id -Scope Script

        # Wait for client to be ready
        if (Test-Port -Port $ClientPort -Service "Frontend Server") {
            return $true
        }
        return $false
    }
    finally {
        Pop-Location
    }
}

function Run-Tests {
    Write-Info "Running Playwright E2E tests..."

    $projectRoot = Get-ProjectRoot
    $testPath = Join-Path $projectRoot "src\testing"

    if (-not (Test-Path $testPath)) {
        Write-Error-Custom "Test project not found at $testPath"
        return $false
    }

    Push-Location $testPath
    try {
        Write-Info "Building test project..."
        $buildOutput = & dotnet build 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Test build failed"
            Write-Host $buildOutput
            return $false
        }

        Write-Success "Test build successful"

        Write-Info "Starting tests..."
        $env:Playwright__HeadlessMode = $Headless
        $env:Playwright__BaseUrl = "http://localhost:$ClientPort"

        $testOutput = & dotnet test -v normal 2>&1
        $testExitCode = $LASTEXITCODE

        Write-Host $testOutput

        if ($testExitCode -ne 0) {
            Write-Error-Custom "Tests failed with exit code $testExitCode"
            return $false
        }

        Write-Success "All tests passed!"
        return $true
    }
    finally {
        Pop-Location
        # Clean up environment variables
        $env:Playwright__HeadlessMode = $null
        $env:Playwright__BaseUrl = $null
    }
}

function Cleanup {
    Write-Info "Cleaning up processes..."

    if (Get-Variable -Name "ServerProcessId" -ErrorAction SilentlyContinue) {
        try {
            Stop-Process -Id $ServerProcessId -ErrorAction SilentlyContinue
            Write-Info "Stopped backend server"
        }
        catch {
            Write-Warning-Custom "Failed to stop backend server"
        }
    }

    if (Get-Variable -Name "ClientProcessId" -ErrorAction SilentlyContinue) {
        try {
            Stop-Process -Id $ClientProcessId -ErrorAction SilentlyContinue
            Write-Info "Stopped frontend server"
        }
        catch {
            Write-Warning-Custom "Failed to stop frontend server"
        }
    }
}

# Register cleanup on exit
$null = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { Cleanup }

# Trap errors to ensure cleanup
trap {
    Write-Error-Custom $_.Exception.Message
    Cleanup
    exit 1
}

# Main execution
Write-Host ""
Write-Host "╔════════════════════════════════════════╗"
Write-Host "║  Insight Forge Deployment & Test      ║"
Write-Host "╚════════════════════════════════════════╝"
Write-Host ""

try {
    # Start services
    if (-not (Start-Server)) {
        throw "Failed to start backend server"
    }

    if (-not (Start-Client)) {
        throw "Failed to start frontend server"
    }

    Write-Info "All services started successfully!"
    Write-Host ""
    Write-Host "Services running:"
    Write-Host "  - Backend: http://localhost:$ServerPort"
    Write-Host "  - Frontend: http://localhost:$ClientPort"
    Write-Host ""

    # Run tests
    if (-not $SkipTests) {
        Write-Info "Waiting 5 seconds before running tests..."
        Start-Sleep -Seconds 5

        if (-not (Run-Tests)) {
            Cleanup
            exit 1
        }
    }

    Write-Success "Deployment and testing completed successfully!"

    if (-not $SkipTests) {
        Write-Host ""
        Write-Host "Keep services running? Press Ctrl+C to stop"
        Wait-Process -Id $ServerProcessId, $ClientProcessId
    }
}
catch {
    Write-Error-Custom "Deployment failed: $_"
    Cleanup
    exit 1
}
