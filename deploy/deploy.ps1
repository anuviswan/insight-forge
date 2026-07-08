param(
    [switch]$SkipTests,
    [bool]$Headless = $false,
    [int]$ServerPort = 5000,
    [int]$ClientPort = 5173,
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"

function Write-Info {
    param([string]$Message)
    Write-Host "[*] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[+] $Message" -ForegroundColor Green
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "[-] $Message" -ForegroundColor Red
}

function Test-Port {
    param([int]$Port, [string]$Service)
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        try {
            $conn = New-Object System.Net.Sockets.TcpClient("127.0.0.1", $Port)
            if ($conn.Connected) {
                $conn.Close()
                Write-Success "$Service is ready on port $Port"
                return $true
            }
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }
    Write-ErrorMsg "$Service failed to start on port $Port"
    return $false
}

function Get-ProjectRoot {
    $scriptPath = $MyInvocation.PSScriptRoot
    if (-not $scriptPath) {
        $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    }
    if (-not $scriptPath) {
        $scriptPath = Get-Location
    }
    return Split-Path -Parent $scriptPath
}

function Start-Server {
    Write-Info "Starting backend server..."
    $projectRoot = Get-ProjectRoot
    $serverPath = Join-Path $projectRoot "src\server\insight.webapi\insight.webapi"

    if (-not (Test-Path $serverPath)) {
        Write-ErrorMsg "Server project not found"
        return $false
    }

    Write-Info "Building server..."
    Push-Location $serverPath
    try {
        & dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMsg "Server build failed"
            return $false
        }
        Write-Success "Server build successful"

        Write-Info "Starting server process..."
        $script:ServerProcessId = (Start-Process -FilePath "dotnet" -ArgumentList "run" -WindowStyle Hidden -PassThru -NoNewWindow).Id

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
        Write-ErrorMsg "Client project not found"
        return $false
    }

    Push-Location $clientPath
    try {
        if (-not (Test-Path "node_modules")) {
            Write-Info "Installing client dependencies..."
            & npm install
            if ($LASTEXITCODE -ne 0) {
                Write-ErrorMsg "npm install failed"
                return $false
            }
        }

        Write-Info "Starting dev server..."
        $script:ClientProcessId = (Start-Process -FilePath "npm" -ArgumentList "run", "dev" -WindowStyle Hidden -PassThru -NoNewWindow).Id

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
        Write-ErrorMsg "Test project not found"
        return $false
    }

    Push-Location $testPath
    try {
        Write-Info "Building test project..."
        & dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-ErrorMsg "Test build failed"
            return $false
        }

        Write-Success "Test build successful"

        Write-Info "Starting tests..."
        $env:Playwright__HeadlessMode = $Headless
        $env:Playwright__BaseUrl = "http://localhost:$ClientPort"

        & dotnet test -v normal
        $exitCode = $LASTEXITCODE

        if ($exitCode -ne 0) {
            Write-ErrorMsg "Tests failed"
            return $false
        }

        Write-Success "All tests passed"
        return $true
    }
    finally {
        Pop-Location
        $env:Playwright__HeadlessMode = $null
        $env:Playwright__BaseUrl = $null
    }
}

function Cleanup {
    if ($script:ServerProcessId) {
        Stop-Process -Id $script:ServerProcessId -ErrorAction SilentlyContinue
    }
    if ($script:ClientProcessId) {
        Stop-Process -Id $script:ClientProcessId -ErrorAction SilentlyContinue
    }
}

$null = Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action { Cleanup }

Write-Host ""
Write-Host "Starting Insight Forge deployment..."
Write-Host ""

try {
    if (-not (Start-Server)) {
        throw "Failed to start backend server"
    }

    if (-not (Start-Client)) {
        throw "Failed to start frontend server"
    }

    Write-Info "All services started successfully"
    Write-Host ""
    Write-Host "Services running:"
    Write-Host "  - Backend: http://localhost:$ServerPort"
    Write-Host "  - Frontend: http://localhost:$ClientPort"
    Write-Host ""

    if (-not $SkipTests) {
        Write-Info "Waiting 5 seconds before running tests..."
        Start-Sleep -Seconds 5

        if (-not (Run-Tests)) {
            Cleanup
            exit 1
        }
    }

    Write-Success "Deployment completed"

    if (-not $SkipTests) {
        Write-Host ""
        Write-Host "Press Ctrl+C to stop"
        Wait-Process -Id $script:ServerProcessId, $script:ClientProcessId
    }
}
catch {
    Write-ErrorMsg "Deployment failed"
    Write-Host $_.Exception.Message
    Cleanup
    exit 1
}
