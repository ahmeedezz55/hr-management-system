#!/usr/bin/env bash
# =====================================================================
# Auto-start the HR Management System web app inside a Dev Container /
# GitHub Codespace. Invoked by postStartCommand in devcontainer.json.
#
# - Binds http://0.0.0.0:5188 so port forwarding always reaches the app
#   (the browser opens automatically via portsAttributes/onAutoForward).
# - Migrations + seed data are applied by the app itself at startup
#   (Program.cs -> DbSeeder), including the admin account
#   admin@hr.sa / Admin@123.
# - A missing GROQ_API_KEY only disables the AI assistant; the app runs.
# =====================================================================
set -u

cd "$(dirname "$0")/.."   # repo root (script lives in .devcontainer/)

# Stop any previous instance so container restarts never hit
# "address already in use".
pkill -f "HR.Web" 2>/dev/null || true
sleep 1

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://0.0.0.0:5188

echo ""
echo "=============================================="
echo " Starting HR Management System on port 5188..."
echo " Login:    admin@hr.sa"
echo " Password: Admin@123"
echo "=============================================="
echo ""

# --no-launch-profile: ignore local launchSettings ports and use
# ASPNETCORE_URLS from above instead.
exec dotnet run --project src/HR.Web --no-launch-profile
