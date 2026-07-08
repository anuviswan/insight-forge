@echo off
REM Insight Forge One-Click Deployment and Testing Script
REM
REM This script:
REM 1. Starts the backend server
REM 2. Starts the frontend development server
REM 3. Waits for services to be ready
REM 4. Runs Playwright E2E tests
REM
REM Usage:
REM   deploy.bat [options]
REM
REM Options:
REM   --skip-tests      Skip running tests
REM   --headless        Run tests in headless mode (default)
REM   --ui              Run tests with UI visible

setlocal enabledelayedexpansion

REM Configuration
set SKIP_TESTS=0
set HEADLESS=1
set SERVER_PORT=5000
set CLIENT_PORT=5173
set TIMEOUT_SECONDS=60
set SERVER_PID=
set CLIENT_PID=

REM Parse arguments
:parse_args
if "%1"=="" goto args_done
if "%1"=="--skip-tests" (
    set SKIP_TESTS=1
    shift
    goto parse_args
)
if "%1"=="--headless" (
    set HEADLESS=1
    shift
    goto parse_args
)
if "%1"=="--ui" (
    set HEADLESS=0
    shift
    goto parse_args
)
shift
goto parse_args

:args_done

echo.
echo ╔════════════════════════════════════════╗
echo ║  Insight Forge Deployment ^& Test      ║
echo ╚════════════════════════════════════════╝
echo.

REM Get script directory
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR:~0,-7%

REM Start server
echo [*] Starting backend server...
set SERVER_PATH=%PROJECT_ROOT%src\server\insight.webapi\insight.webapi
if not exist "%SERVER_PATH%" (
    echo [!] Server project not found at %SERVER_PATH%
    exit /b 1
)

echo [*] Building server...
pushd "%SERVER_PATH%"
dotnet build > nul 2>&1
if errorlevel 1 (
    echo [!] Server build failed
    popd
    exit /b 1
)
echo [+] Server build successful
echo [*] Starting server process...
start /B dotnet run > "%TEMP%\insight-server.log" 2>&1
popd

REM Wait for server
set /a count=0
:wait_server
if %count% geq %TIMEOUT_SECONDS% (
    echo [!] Backend server failed to start on port %SERVER_PORT% within %TIMEOUT_SECONDS% seconds
    exit /b 1
)

netstat -an | find ":%SERVER_PORT%" > nul
if errorlevel 1 (
    set /a count=count+1
    timeout /t 1 /nobreak > nul
    goto wait_server
)
echo [+] Backend Server is ready on port %SERVER_PORT%

REM Start client
echo [*] Starting frontend development server...
set CLIENT_PATH=%PROJECT_ROOT%src\client
if not exist "%CLIENT_PATH%" (
    echo [!] Client project not found at %CLIENT_PATH%
    exit /b 1
)

pushd "%CLIENT_PATH%"
if not exist "node_modules" (
    echo [*] Installing client dependencies...
    call npm install > nul 2>&1
    if errorlevel 1 (
        echo [!] npm install failed
        popd
        exit /b 1
    )
)

echo [*] Starting dev server...
start /B npm run dev > "%TEMP%\insight-client.log" 2>&1
popd

REM Wait for client
set /a count=0
:wait_client
if %count% geq %TIMEOUT_SECONDS% (
    echo [!] Frontend server failed to start on port %CLIENT_PORT% within %TIMEOUT_SECONDS% seconds
    exit /b 1
)

netstat -an | find ":%CLIENT_PORT%" > nul
if errorlevel 1 (
    set /a count=count+1
    timeout /t 1 /nobreak > nul
    goto wait_client
)
echo [+] Frontend Server is ready on port %CLIENT_PORT%

echo.
echo [+] All services started successfully!
echo.
echo Services running:
echo   - Backend: http://localhost:%SERVER_PORT%
echo   - Frontend: http://localhost:%CLIENT_PORT%
echo.

REM Run tests
if "%SKIP_TESTS%"=="0" (
    echo [*] Waiting 5 seconds before running tests...
    timeout /t 5 /nobreak > nul

    echo [*] Running Playwright E2E tests...
    set TEST_PATH=%PROJECT_ROOT%src\testing
    if not exist "%TEST_PATH%" (
        echo [!] Test project not found at %TEST_PATH%
        exit /b 1
    )

    pushd "%TEST_PATH%"
    echo [*] Building test project...
    dotnet build > nul 2>&1
    if errorlevel 1 (
        echo [!] Test build failed
        popd
        exit /b 1
    )
    echo [+] Test build successful

    echo [*] Starting tests...
    set Playwright__HeadlessMode=%HEADLESS%
    set Playwright__BaseUrl=http://localhost:%CLIENT_PORT%

    dotnet test -v normal
    if errorlevel 1 (
        echo [!] Tests failed
        popd
        exit /b 1
    )

    echo [+] All tests passed!
    popd
)

echo.
echo [+] Deployment and testing completed successfully!
echo.
echo Keep services running? Press Ctrl+C to stop
pause

endlocal
