# Insight Forge Deployment Scripts

One-click deployment and testing scripts for Insight Forge. These scripts automate the entire process of starting the backend, frontend, and running E2E tests.

## Scripts Available

### PowerShell (Windows - Recommended)
- **File**: `deploy.ps1`
- **Platform**: Windows 10/11 with PowerShell 5.1+
- **Recommended**: Yes (best error handling and control)

### Bash (Linux/macOS)
- **File**: `deploy.sh`
- **Platform**: Linux, macOS
- **Shell**: bash 4.0+

### Batch (Windows - Legacy)
- **File**: `deploy.bat`
- **Platform**: Windows Command Prompt
- **Recommended**: No (use PowerShell instead)

## Prerequisites

### All Platforms
- .NET SDK 8.0 or later
- Node.js 18+ with npm
- Port 5000 (backend) and 5173 (frontend) must be available
- Git (for version control)

### Linux/macOS Only
- `netcat` (nc) for port testing
- `curl` or similar HTTP client

### Windows Only
- PowerShell 5.1+ or 7+
- TCP port access for testing

## Quick Start

### Windows - PowerShell (Recommended)

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

### Linux/macOS - Bash

```bash
# Navigate to deploy folder
cd ./deploy

# Make script executable
chmod +x deploy.sh

# Run with default settings (headless tests)
./deploy.sh

# Run with visible browser
./deploy.sh --ui

# Run without tests
./deploy.sh --skip-tests
```

### Windows - Command Prompt (Legacy)

```cmd
cd deploy
deploy.bat
```

## Script Options

### PowerShell (`deploy.ps1`)

```powershell
-SkipTests           # Skip running tests after deployment
-Headless $true      # Run tests in headless mode (default: $true)
-ServerPort 5000     # Backend server port (default: 5000)
-ClientPort 5173     # Frontend server port (default: 5173)
-TimeoutSeconds 60   # Service startup timeout (default: 60)
```

**Examples:**

```powershell
# Run with UI visible
.\deploy.ps1 -Headless $false

# Skip tests
.\deploy.ps1 -SkipTests

# Custom ports
.\deploy.ps1 -ServerPort 3000 -ClientPort 3001

# Longer timeout for slow machines
.\deploy.ps1 -TimeoutSeconds 120
```

### Bash (`deploy.sh`)

```bash
--skip-tests         # Skip running tests after deployment
--headless          # Run tests in headless mode (default)
--ui                # Run tests with UI visible
--server-port 5000  # Backend server port (default: 5000)
--client-port 5173  # Frontend server port (default: 5173)
--timeout 60        # Service startup timeout (default: 60)
```

**Examples:**

```bash
# Run with UI visible
./deploy.sh --ui

# Skip tests
./deploy.sh --skip-tests

# Custom ports
./deploy.sh --server-port 3000 --client-port 3001

# Longer timeout for slow machines
./deploy.sh --timeout 120
```

## What These Scripts Do

1. **Backend Server Setup**
   - Navigates to `src/server/insight.webapi/insight.webapi`
   - Runs `dotnet build`
   - Starts the server with `dotnet run`
   - Waits for server to be ready on configured port

2. **Frontend Server Setup**
   - Navigates to `src/client`
   - Installs dependencies if needed (`npm install`)
   - Starts dev server with `npm run dev`
   - Waits for frontend to be ready on configured port

3. **E2E Testing**
   - Navigates to `src/testing`
   - Builds the test project (`dotnet build`)
   - Runs tests with `dotnet test`
   - Configures test environment variables:
     - `Playwright__HeadlessMode`
     - `Playwright__BaseUrl`

4. **Cleanup**
   - Automatically stops all processes on exit
   - Gracefully handles interrupts (Ctrl+C)

## Output Examples

### Successful Run
```
╔════════════════════════════════════════╗
║  Insight Forge Deployment & Test      ║
╚════════════════════════════════════════╝

ℹ️  Starting backend server...
ℹ️  Building server...
✅ Server build successful
ℹ️  Starting server process...
✅ Backend Server is ready on port 5000

ℹ️  Starting frontend development server...
ℹ️  Starting dev server...
✅ Frontend Server is ready on port 5173

✅ All services started successfully!

Services running:
  - Backend: http://localhost:5000
  - Frontend: http://localhost:5173

ℹ️  Waiting 5 seconds before running tests...
ℹ️  Running Playwright E2E tests...
✅ All tests passed!

✅ Deployment and testing completed successfully!
```

## Troubleshooting

### "Server failed to start"
- Check if port 5000 is already in use: `netstat -an | find ":5000"`
- Try a different port: `.\deploy.ps1 -ServerPort 3000`
- Check logs: `cat /tmp/insight-server.log` (Linux/macOS) or `type %TEMP%\insight-server.log` (Windows)

### "Client failed to start"
- Check if port 5173 is already in use: `netstat -an | find ":5173"`
- Try a different port: `.\deploy.ps1 -ClientPort 3001`
- Ensure Node.js is installed: `node --version`
- Check logs: `cat /tmp/insight-client.log` (Linux/macOS) or `type %TEMP%\insight-client.log` (Windows)

### "Tests failed"
- Check test configuration in `src/testing/appsettings.json`
- Run tests manually: `cd src/testing && dotnet test -v normal`
- Check Playwright browser installation: `.\playwright.ps1 install` (Windows)
- For Linux/macOS: May need to install browser dependencies

### "Permission denied" (Linux/macOS)
```bash
chmod +x deploy.sh
./deploy.sh
```

### Ports Already in Use

**PowerShell:**
```powershell
# Find what's using the port
Get-NetTCPConnection -LocalPort 5000

# Kill the process
Stop-Process -Id <PID>

# Or use different ports
.\deploy.ps1 -ServerPort 3000 -ClientPort 3001
```

**Bash:**
```bash
# Find what's using the port
lsof -i :5000

# Kill the process
kill -9 <PID>

# Or use different ports
./deploy.sh --server-port 3000 --client-port 3001
```

**Command Prompt:**
```cmd
netstat -ano | find ":5000"
taskkill /PID <PID> /F
```

## Environment Variables

These scripts set environment variables automatically, but you can override them:

### For Tests
```powershell
# PowerShell
$env:Playwright__HeadlessMode = "false"
$env:Playwright__BaseUrl = "http://localhost:3000"
.\deploy.ps1
```

```bash
# Bash
export Playwright__HeadlessMode=false
export Playwright__BaseUrl="http://localhost:3000"
./deploy.sh
```

## CI/CD Integration

### GitHub Actions

```yaml
- name: Deploy and Test
  run: |
    cd deploy
    pwsh deploy.ps1 -SkipTests # Build only
  # Or for tests
  - name: Run Tests
    run: |
      cd deploy
      pwsh deploy.ps1 -Headless $true # Full deployment + tests
```

### Azure Pipelines

```yaml
- script: |
    cd deploy
    pwsh deploy.ps1
  displayName: 'Deploy and Test Insight Forge'
```

## Performance Tips

1. **First Run Slow?**
   - NuGet and npm package downloads take time
   - Subsequent runs are much faster

2. **Optimize for CI/CD**
   ```powershell
   # Skip some steps
   .\deploy.ps1 -SkipTests  # Just deploy
   ```

3. **Parallel Development**
   ```powershell
   # Run in separate PowerShell windows
   cd src/server/insight.webapi/insight.webapi; dotnet run
   cd src/client; npm run dev
   cd src/testing; dotnet test
   ```

## Logs

Logs are written to temporary directories:

- **Windows**: 
  - `%TEMP%\insight-server.log`
  - `%TEMP%\insight-client.log`

- **Linux/macOS**:
  - `/tmp/insight-server.log`
  - `/tmp/insight-client.log`

Check these files if services fail to start.

## Known Issues

1. **Port conflicts on Windows**: Use `netstat -ano` and `taskkill` to free ports
2. **Node not found**: Ensure Node.js is in PATH - restart terminal after installation
3. **Tests timeout**: Increase `--timeout` or `–TimeoutSeconds` option
4. **Slow CI/CD**: First run with fresh dependencies takes longer

## Support

For issues:
1. Check logs in temp directory
2. Run individual components manually to isolate problems
3. Verify prerequisites are installed
4. Check port availability
5. Review test output for specific errors

## Script Comparison

| Feature | PowerShell | Bash | Batch |
|---------|-----------|------|-------|
| Error Handling | Excellent | Good | Fair |
| Colors/Formatting | Yes | Yes | Limited |
| Cross-platform | Windows 5.1+ | Linux/macOS | Windows only |
| Recommended | ✅ Yes | ✅ Yes | No |
| Maintenance | Active | Active | Legacy |

## Contributing

To improve these scripts:
1. Test on your platform
2. Report issues with platform/shell version
3. Suggest improvements
4. Keep scripts synchronized across platforms
