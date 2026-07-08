# Insight Forge Deployment Script

One-click deployment and testing for Insight Forge. This script automates the entire process of starting the backend, frontend, and running E2E tests.

## Script

- **File**: `deploy.ps1`
- **Platform**: Windows 10/11 with PowerShell 5.1+
- **Purpose**: Deploy backend, frontend, and run Playwright E2E tests

## Prerequisites

- .NET SDK 8.0 or later
- Node.js 18+ with npm
- PowerShell 5.1+ (built-in on Windows 10/11)
- Port 5000 (backend) and 5173 (frontend) must be available

## Quick Start

```powershell
# Navigate to deploy folder
cd .\deploy

# Run with default settings (headless tests)
.\deploy.ps1

# Run with visible browser
.\deploy.ps1 -Headless $false

# Run without tests
.\deploy.ps1 -SkipTests
```

## Script Options

```powershell
-SkipTests           # Skip running tests after deployment
-Headless $true      # Run tests in headless mode (default: $true)
-ServerPort 5000     # Backend server port (default: 5000)
-ClientPort 5173     # Frontend server port (default: 5173)
-TimeoutSeconds 60   # Service startup timeout (default: 60)
```

## Examples

```powershell
# Run with UI visible (disable headless mode)
.\deploy.ps1 -Headless $false

# Skip tests
.\deploy.ps1 -SkipTests

# Custom ports (if 5000/5173 are already in use)
.\deploy.ps1 -ServerPort 3000 -ClientPort 3001

# Longer timeout for slow machines
.\deploy.ps1 -TimeoutSeconds 120

# Everything combined
.\deploy.ps1 -Headless $false -ServerPort 3000 -ClientPort 3001 -TimeoutSeconds 120
```

## What The Script Does

1. **Backend Server Setup**
   - Navigates to `src/server/insight.webapi/insight.webapi`
   - Runs `dotnet build`
   - Starts the server with `dotnet run`
   - Waits for server to be ready on port 5000

2. **Frontend Server Setup**
   - Navigates to `src/client`
   - Installs dependencies if needed (`npm install`)
   - Starts dev server with `npm run dev`
   - Waits for frontend to be ready on port 5173

3. **E2E Testing**
   - Navigates to `src/testing`
   - Builds the test project (`dotnet build`)
   - Runs tests with `dotnet test`
   - Automatically configures test environment:
     - `Playwright__HeadlessMode`
     - `Playwright__BaseUrl`

4. **Cleanup**
   - Automatically stops all processes on exit
   - Gracefully handles Ctrl+C

## Output Example

```
╔════════════════════════════════════════╗
║  Insight Forge Deployment & Test      ║
╚════════════════════════════════════════╝

✅ Server build successful
✅ Backend Server is ready on port 5000
✅ Frontend Server is ready on port 5173

✅ All tests passed!
✅ Deployment and testing completed successfully!
```

## Troubleshooting

### Port Already in Use

```powershell
# Use different ports
.\deploy.ps1 -ServerPort 3000 -ClientPort 3001
```

### Services Failed to Start

Check logs:
```powershell
type $env:TEMP\insight-server.log
type $env:TEMP\insight-client.log
```

### Tests Failed

Run tests manually for more details:
```powershell
cd src/testing
dotnet test -v normal
```

### Permission Denied

If you get "cannot be loaded because running scripts is disabled":

```powershell
# Allow scripts for current user session
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## Manual Testing

After deployment, services stay running. You can:
- Visit `http://localhost:5173` to test the frontend
- Check `http://localhost:5000` for API endpoints
- Press Ctrl+C to stop all services

## CI/CD Integration

### GitHub Actions

```yaml
- name: Deploy and Test
  run: |
    cd deploy
    pwsh deploy.ps1
```

### Azure Pipelines

```yaml
- script: |
    cd deploy
    pwsh deploy.ps1
  displayName: 'Deploy and Test Insight Forge'
```

## Performance Tips

1. **First run slow?**
   - NuGet and npm package downloads take time
   - Subsequent runs are much faster

2. **Optimize for CI/CD**
   ```powershell
   .\deploy.ps1 -SkipTests  # Just deploy without testing
   ```

## Environment Variables

Set these to customize the deployment:

```powershell
$env:Playwright__HeadlessMode = "false"  # Show browser
$env:Playwright__BaseUrl = "http://localhost:3000"
.\deploy.ps1
```

## Known Issues

1. **Port conflicts**: Use `-ServerPort` and `-ClientPort` options to use different ports
2. **Node not found**: Restart PowerShell after installing Node.js
3. **Tests timeout**: Increase `-TimeoutSeconds` option if tests take longer

## Need Help?

1. Check logs in `$env:TEMP\insight-server.log` and `$env:TEMP\insight-client.log`
2. Verify ports 5000 and 5173 are available: `netstat -ano | find ":5000"`
3. Run individual components manually to isolate problems
4. Review test output for specific errors
