#!/usr/bin/env bash
# =====================================================================
# HR Management System - Linux/Mac setup script
#
# Checks prerequisites (.NET 10 SDK + SQL Server), prepares the local
# config, applies EF Core migrations and runs the application.
#
# Usage:  chmod +x setup.sh && ./setup.sh   (from the repository root)
# =====================================================================
set -u

BOLD="${BOLD:-$(tput bold 2>/dev/null || printf '')}"
NC="${NC:-$(tput sgr0 2>/dev/null || printf '')}"

say()    { printf '  %s\n' "$1"; }
say_ok() { printf ' [OK] %s\n' "$1"; }
say_warn(){ printf ' [!!] %s\n' "$1"; }
say_bad(){ printf ' [X]  %s\n' "$1"; }

echo ""
echo "================================================"
echo "  HR Management System - Setup (Linux/Mac)"
echo "================================================"
echo ""

# ---------- 1. Check .NET SDK ----------
if ! command -v dotnet >/dev/null 2>&1; then
    say_bad ".NET SDK was NOT found."
    say "    Install it from https://dotnet.microsoft.com/download/dotnet/10.0"
    exit 1
fi

if ! dotnet --list-sdks 2>/dev/null | grep -q '^10\.'; then
    say_bad ".NET 10 SDK was not found. Current: $(dotnet --version)"
    say "    Install .NET 10 SDK from https://dotnet.microsoft.com/download/dotnet/10.0"
    exit 1
fi
say_ok ".NET SDK found: $(dotnet --version)"
echo ""

# ---------- 2. Prepare local development config ----------
CONFIG="src/HR.Web/appsettings.Development.json"
if [ -f "$CONFIG" ]; then
    say_ok "appsettings.Development.json already exists - keeping it."
else
    say "Creating appsettings.Development.json ..."
    cat > "$CONFIG" <<'JSON'
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "Groq": { "ApiKey": "" }
}
JSON
    say_ok "Created. Note: this file is git-ignored."
    say "    To enable the AI Assistant, put your Groq key inside it:"
    say '      "Groq": { "ApiKey": "gsk_your_key_here" }'
    say "    Or use the safer user-secrets tool:"
    say '      dotnet user-secrets set "Groq:ApiKey" "gsk_..." --project src/HR.Web'
fi
echo ""

# ---------- 3. Check SQL Server ----------
SQL_CMD=""
for candidate in /opt/mssql-tools18/bin/sqlcmd /opt/mssql-tools/bin/sqlcmd; do
    if [ -x "$candidate" ]; then SQL_CMD="$candidate"; break; fi
done
if [ -z "$SQL_CMD" ]; then
    if command -v sqlcmd >/dev/null 2>&1; then SQL_CMD="$(command -v sqlcmd)"; fi
fi

if [ -z "$SQL_CMD" ]; then
    say_warn "sqlcmd was not found - skipping the connectivity check."
    say "    Make sure SQL Server is running, or use Docker instead: docker compose up"
elif "$SQL_CMD" -S localhost -C -Q "SELECT 1" -b -o /dev/null >/dev/null 2>&1; then
    say_ok "Connected to SQL Server (localhost)."
else
    say_warn "Could NOT connect to SQL Server on localhost:1433."
    say "    - Set a custom connection string in src/HR.Web/appsettings.json, or"
    say "    - use the zero-config Docker route: docker compose up"
    say "    The app also applies migrations automatically on first start."
fi
echo ""

# ---------- 4. Restore + Build ----------
say "Restoring NuGet packages and building the solution ..."
if ! dotnet build HR.System.sln; then
    say_bad "Build failed."
    exit 1
fi
say_ok "Build succeeded."
echo ""

# ---------- 5. Apply EF Core migrations ----------
say "Applying EF Core migrations ..."
if ! command -v dotnet-ef >/dev/null 2>&1; then
    say "dotnet-ef tool is not installed - installing it locally ..."
    dotnet tool install --local dotnet-ef >/dev/null 2>&1 || {
        say_warn "Could not install dotnet-ef - skipping."
        say "    Pending migrations are applied automatically at startup anyway."
    }
fi
if ! dotnet ef database update --project src/HR.Infrastructure --startup-project src/HR.Web; then
    say_warn "Failed to apply migrations via CLI - they will be applied at startup."
else
    say_ok "Database is up to date."
fi
echo ""

# ---------- 6. Run ----------
echo "================================================"
echo "  Starting HR.Web ..."
echo "  Login: admin@hr.sa / Admin@123"
echo "  Press Ctrl+C to stop."
echo "================================================"
echo ""
exec dotnet run --project src/HR.Web
