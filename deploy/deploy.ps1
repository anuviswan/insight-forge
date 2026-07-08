param(
    [switch]$SkipTests,
    [bool]$Headless = $false
)

Write-Host "Starting Insight Forge deployment..."
Write-Host ""

$projectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Write-Host "[1/4] Building backend server..."
Push-Location "$projectRoot\src\server\insight.webapi\insight.webapi"
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend build failed"
    exit 1
}
Pop-Location

Write-Host ""
Write-Host "[2/4] Starting backend server..."
Write-Host "Press Ctrl+C when ready to start frontend (or wait for ready message)"
Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "$projectRoot\src\server\insight.webapi\insight.webapi"
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "[3/4] Starting frontend server..."
Push-Location "$projectRoot\src\client"
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..."
    npm install
}
Start-Process -FilePath "npm" -ArgumentList "run", "dev" -WorkingDirectory "$projectRoot\src\client"
Start-Sleep -Seconds 5
Pop-Location

Write-Host ""
Write-Host "[4/4] Running tests..."

if (-not $SkipTests) {
    Push-Location "$projectRoot\src\testing"
    $env:Playwright__HeadlessMode = $Headless
    $env:Playwright__BaseUrl = "http://localhost:5173"
    dotnet test -v normal
    Pop-Location
}

Write-Host ""
Write-Host "Done!"
