# Development Setup Guide

## Prerequisites

### Required Software
- **.NET 10.0 SDK** or higher
- **Node.js** (v18+ recommended) and npm
- **Git**

### Optional but Recommended
- Visual Studio 2022 or VS Code with C# extension
- Azure Storage Explorer (for debugging storage data)

## Getting Started

### 1. Install Dependencies

#### Backend Dependencies
```bash
cd src/server/insight.webapi
dotnet restore
```

#### Frontend Dependencies
```bash
cd src/client
npm install
```

### 2. Configure Local Environment

#### Install Azurite (Azure Storage Emulator)
Azurite is automatically started when running the Web API in development mode. To ensure it's available:

```bash
npm install -g azurite
```

Verify installation:
```bash
azurite --version
```

#### Set Up Environment Variables
Create `appsettings.Development.json` in `src/server/insight.webapi/insight.webapi/` (already configured with defaults):

- **ASPNETCORE_ENVIRONMENT**: Set to `Development` (auto-detected by ASP.NET)
- **AZURE_STORAGE_CONNECTION_STRING**: Not needed in development (uses default emulator connection)
- **GEMINI_API_KEY**: Add your Gemini API key for AI features
- **JWT_SECRET_KEY**: Development key provided in appsettings.Development.json

### 3. Running the Application

#### Option A: Using the Deployment Script (Recommended)
```powershell
.\deploy\deploy.ps1 -SkipTests
```

This will:
- Build the backend
- Start the Web API (which auto-starts Azurite)
- Start the frontend dev server (Vite)

#### Option B: Running Manually

**Terminal 1 - Start the Web API:**
```bash
cd src/server/insight.webapi/insight.webapi
dotnet run
```

Azurite will automatically start and log:
```
[info] Azurite started successfully (PID: 12345)
```

**Terminal 2 - Start the Frontend:**
```bash
cd src/client
npm run dev
```

Access the app at: `http://localhost:5173`

### 4. Verify Everything Works

1. **Backend API**: Browse to `http://localhost:5001/openapi` (or `https://localhost:5001` if HTTPS)
2. **Frontend**: `http://localhost:5173`
3. **Azurite**: Runs on `http://127.0.0.1:10002` (internal, no browser access needed)

## Azure Storage Emulator (Azurite)

### Automatic Startup
- **Development Only**: Azurite auto-starts when running the Web API with `ASPNETCORE_ENVIRONMENT=Development`
- **Production**: Azurite is NOT started (uses real Azure Table Storage)

### Storage Location
- Azurite data files: `./azurite-data/` (in the Web API directory)
- Automatically cleaned up on API shutdown

### Manual Azurite Management

If you need to run Azurite independently:
```bash
azurite --silent --location ./azurite-data
```

Kill Azurite process:
```bash
pkill -f azurite        # macOS/Linux
Stop-Process -Name node -Force  # Windows (if running via npm)
```

### Debugging Storage Data

Use **Azure Storage Explorer** to inspect tables and entities:
1. Install from [Microsoft Store](https://apps.microsoft.com/store/detail/azure-storage-explorer/9NBLGGGNTJT9) or [GitHub](https://github.com/microsoft/AzureStorageExplorer)
2. Connect to: `http://127.0.0.1:10002/devstoreaccount1`
3. Browse `users` and `emailverifications` tables

## CORS Configuration

### Development
- Allows: `http://localhost:5173`, `http://localhost:3000`
- Credentials: Enabled
- Methods: All HTTP methods

### Production
- Only origins specified in configuration
- Default: Empty (no CORS enabled)
- Override in `appsettings.Production.json`:
  ```json
  {
    "Cors": {
      "AllowedOrigins": ["https://your-domain.com"]
    }
  }
  ```

## Database & Tables

### Azure Table Storage Tables (Auto-Created)
- **users**: Stores user registration data
- **emailverifications**: Stores email verification tokens

Tables are automatically created on first run if they don't exist.

### Reset Storage
To reset all data during development:
```bash
# Stop the Web API
rm -r ./src/server/insight.webapi/insight.webapi/azurite-data
# Restart the Web API (tables will be recreated)
```

## Troubleshooting

### Azurite Won't Start
```
Failed to start Azurite. Make sure Node.js and npm are installed. 
Install with: npm install -g azurite
```

**Solution:**
```bash
npm install -g azurite
# Then restart the Web API
```

### Port Already in Use
If `localhost:5001` or Azurite port `127.0.0.1:10002` is in use:

**Kill existing processes:**
```powershell
# Windows - Kill Web API
Stop-Process -Name dotnet -Force

# Kill Azurite
Stop-Process -Name node -Force
```

### CORS Errors in Browser Console
- **Development**: Check that frontend runs on `http://localhost:5173` or `http://localhost:3000`
- **Production**: Verify `Cors:AllowedOrigins` in appsettings.json

### Connection String Errors
If you see connection errors at startup:
- Verify Azurite is running (check logs in Web API console)
- Ensure `appsettings.Development.json` has the correct connection string
- Check that Azure Table Storage tables were created

## Building for Production

### Backend
```bash
cd src/server/insight.webapi/insight.webapi
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd src/client
npm run build
```

Output goes to `dist/` folder. Serve with any static file server.

## Code Style & Standards

See [CLAUDE.md](./CLAUDE.md) for architecture guidelines and coding standards.

## Support

For issues or questions:
1. Check logs in terminal where Web API is running
2. Verify Azurite is running (log output should show "Azurite started successfully")
3. Ensure all prerequisites are installed (`node --version`, `dotnet --version`)
