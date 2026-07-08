# Quick Start Guide

## 🚀 One-Click Deploy & Test

### Windows (PowerShell) - Recommended

```powershell
cd deploy
.\deploy.ps1
```

That's it! The script will:
1. ✅ Start the backend server (port 5000)
2. ✅ Start the frontend dev server (port 5173)
3. ✅ Run all E2E tests with Playwright
4. ✅ Keep services running for manual testing

### macOS / Linux

```bash
cd deploy
chmod +x deploy.sh
./deploy.sh
```

## ⚙️ Common Options

### Run without tests (just deploy)
```powershell
# Windows
.\deploy.ps1 -SkipTests

# Linux/macOS
./deploy.sh --skip-tests
```

### See the browser while tests run
```powershell
# Windows
.\deploy.ps1 -Headless $false

# Linux/macOS
./deploy.sh --headless
```

### Use custom ports
```powershell
# Windows (if 5000/5173 are already in use)
.\deploy.ps1 -ServerPort 3000 -ClientPort 3001

# Linux/macOS
./deploy.sh --server-port 3000 --client-port 3001
```

## 🧪 What Gets Tested?

The automation project tests:
- ✅ User signup with valid credentials
- ✅ Password matching validation
- ✅ Email validation
- ✅ Terms & conditions requirement
- ✅ Error messages display
- ✅ Form state management
- ✅ Navigation flows
- And 6 more scenarios...

See `src/testing/Tests/Authentication/SignupTests.cs` for full details.

## 📊 Example Output

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

## ❌ Troubleshooting

### "Port already in use"
```powershell
# Use a different port
.\deploy.ps1 -ServerPort 3000 -ClientPort 3001
```

### "Services failed to start"
Check logs:
- Windows: `type %TEMP%\insight-server.log`
- Linux/macOS: `cat /tmp/insight-server.log`

### "Tests failed"
```bash
# Run tests manually for more details
cd src/testing
dotnet test -v normal
```

## 🔍 Manual Testing

After deployment, services stay running. You can:
- Visit `http://localhost:5173` to test the frontend
- Check `http://localhost:5000` for API endpoints
- Press Ctrl+C to stop all services

## 📚 More Information

See `README.md` for:
- Detailed options
- Environment variables
- CI/CD integration
- Performance tips
- Known issues

## 🎯 Next Steps

1. Run: `.\deploy.ps1`
2. Tests should pass ✅
3. Browse to `http://localhost:5173`
4. Try creating an account to test signup flow
5. Check test results in console

Happy testing! 🎉
