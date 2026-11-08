@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM =====================================================================
REM HR Management System - Windows setup script
REM
REM Checks prerequisites (.NET 10 SDK + SQL Server), prepares the local
REM config, applies EF Core migrations and runs the application.
REM
REM Usage:  setup.bat   (run from the repository root)
REM =====================================================================

echo.
echo ================================================
echo   HR Management System - Setup (Windows)
echo ================================================
echo.

REM ---------- 1. Check .NET SDK ----------
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [X] .NET SDK was NOT found.
    echo     Install it from https://dotnet.microsoft.com/download/dotnet/10.0
    goto :fail
)

for /f "tokens=*" %%i in ('dotnet --version') do set dotnetver=%%i
dotnet --list-sdks 2>nul | findstr /b "10." >nul
if errorlevel 1 (
    echo [X] .NET 10 SDK was not found. Current: dotnet %dotnetver%
    echo     Install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0
    goto :fail
)
echo [OK] .NET SDK found: %dotnetver%
echo.

REM ---------- 2. Prepare local development config ----------
if exist "src\HR.Web\appsettings.Development.json" (
    echo [OK] appsettings.Development.json already exists - keeping it.
) else (
    echo [i] Creating appsettings.Development.json ...
    (
        echo {
        echo   "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
        echo   "Groq": { "ApiKey": "" }
        echo }
    ) > "src\HR.Web\appsettings.Development.json"
    echo [OK] Created. ^ Note: this file is git-ignored.
    echo      To enable the AI Assistant, put your Groq key inside it:
    echo        "Groq": { "ApiKey": "gsk_your_key_here" }
    echo      Best practice: use the user-secrets tool instead:
    echo        dotnet user-secrets set "Groq:ApiKey" "gsk_..." --project src\HR.Web
)
echo.

REM ---------- 3. Check SQL Server ----------
echo [i] Checking SQL Server ^(localhost\SQLEXPRESS or a local instance^) ...
where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo [!] sqlcmd was not found - skipping the connectivity check.
    echo     Make sure SQL Server is running, or simply use Docker instead:
    echo     docker compose up
) else (
    sqlcmd -S "localhost\SQLEXPRESS" -E -Q "SELECT 1" >nul 2>&1
    if errorlevel 1 (
        echo [!] Could NOT connect to localhost\SQLEXPRESS.
        echo     - If you run a different SQL Server instance, edit the connection
        echo       string in src\HR.Web\appsettings.json
        echo     - Or start SQL Server ^(SQL Server Configuration Manager^)
        echo     - Or use the zero-config Docker route: docker compose up
        goto :fail
    ) else (
        echo [OK] Connected to SQL Server ^(localhost\SQLEXPRESS^).
    )
)
echo.

REM ---------- 4. Restore + Build ----------
echo [i] Restoring NuGet packages and building the solution ...
dotnet build HR.System.sln
if errorlevel 1 (
    echo [X] Build failed.
    goto :fail
)
echo [OK] Build succeeded.
echo.

REM ---------- 5. Apply EF Core migrations ----------
echo [i] Applying EF Core migrations ...
where dotnet-ef >nul 2>&1
if errorlevel 1 (
    echo [!] dotnet-ef tool is not installed - installing it globally ...
    dotnet tool install --global dotnet-ef
    if errorlevel 1 (
        echo [!] Could not install dotnet-ef. Skipped.
        echo     Note: pending migrations are still applied automatically
        echo     on the first application start.
        goto :run
    )
)
dotnet tool update --global dotnet-ef >nul 2>&1
dotnet ef database update --project src/HR.Infrastructure --startup-project src/HR.Web
if errorlevel 1 (
    echo [!] Failed to apply migrations via CLI - they will be applied
    echo     automatically at application startup anyway.
    goto :run
)
echo [OK] Database is up to date.
echo.

REM ---------- 6. Run ----------
:run
echo ================================================
echo   Starting HR.Web ...
echo   Login: admin@hr.sa / Admin@123
echo   Press Ctrl+C to stop.
echo ================================================
echo.
dotnet run --project src/HR.Web
goto :end

:fail
echo.
echo [X] Setup failed. Fix the errors above and run setup.bat again.
exit /b 1

:end
endlocal
