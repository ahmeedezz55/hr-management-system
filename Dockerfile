# =====================================================================
# HR Management System - Multi-stage Docker build
# Build:  docker build -t hr-system .
# Run:    docker-compose up   (recommended - includes SQL Server)
# =====================================================================

# ---------- Stage 1: Restore + Build + Publish ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching for restore
COPY ["HR.System.sln", "./"]
COPY ["src/HR.Domain/HR.Domain.csproj", "src/HR.Domain/"]
COPY ["src/HR.Infrastructure/HR.Infrastructure.csproj", "src/HR.Infrastructure/"]
COPY ["src/HR.Web/HR.Web.csproj", "src/HR.Web/"]

RUN dotnet restore "src/HR.Web/HR.Web.csproj"

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish "src/HR.Web/HR.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Stage 2: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

# Expose plain HTTP inside the container (TLS is terminated by the host/proxy)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# The application applies pending EF Core migrations and seeds default
# data automatically at startup (Program.cs -> DbSeeder.SeedAsync).
ENTRYPOINT ["dotnet", "HR.Web.dll"]
