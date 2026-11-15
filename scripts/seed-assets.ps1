<#
.SYNOPSIS
    Seeds initial assets into the Hi-Trade SQL Server database container.
.DESCRIPTION
    Waits for the SQL Server container to become healthy, ensures migrations are
    applied, and executes scripts/seed-assets.sql.
.PARAMETER ContainerName
    Name of the Docker container running SQL Server (default: hitrade-sqlserver).
.PARAMETER Password
    SA password for the SQL Server instance (default: HiTrade@2026Password!).
.PARAMETER RunMigrations
    Whether to run dotnet ef database update before seeding (default: $true).
#>
[CmdletBinding()]
param(
    [string]$ContainerName = "hitrade-sqlserver",
    [string]$Password = "HiTrade@2026Password!",
    [switch]$RunMigrations = $true
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Hi-Trade Database Asset Seeder          " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Check if Docker container is running
Write-Host "[1/4] Checking SQL Server container status..." -ForegroundColor Yellow
$containerStatus = docker ps --filter "name=$ContainerName" --format "{{.Status}}"
if (-not $containerStatus) {
    Write-Host "Error: Container '$ContainerName' is not running." -ForegroundColor Red
    Write-Host "Start the Docker environment first with: docker compose up -d" -ForegroundColor Yellow
    exit 1
}
Write-Host "Container '$ContainerName' is running ($containerStatus)." -ForegroundColor Green

# 2. Wait for SQL Server to accept connections
Write-Host "[2/4] Verifying SQL Server availability..." -ForegroundColor Yellow
$retries = 30
$ready = $false
while ($retries -gt 0) {
    $testResult = docker exec $ContainerName /bin/bash -c @"
if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$Password' -C -Q 'SELECT 1' > /dev/null 2>&1
else
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '$Password' -Q 'SELECT 1' > /dev/null 2>&1
fi
"@
    if ($LASTEXITCODE -eq 0) {
        $ready = $true
        break
    }
    Write-Host "Waiting for SQL Server to be ready ($retries retries left)..."
    Start-Sleep -Seconds 2
    $retries--
}

if (-not $ready) {
    Write-Host "Error: Timed out waiting for SQL Server inside container '$ContainerName'." -ForegroundColor Red
    exit 1
}
Write-Host "SQL Server is ready and accepting queries." -ForegroundColor Green

# 3. Apply EF Core migrations if requested and dotnet is available
if ($RunMigrations) {
    Write-Host "[3/4] Ensuring EF Core migrations are applied..." -ForegroundColor Yellow
    $dotnetExists = Get-Command "dotnet" -ErrorAction SilentlyContinue
    if ($dotnetExists) {
        try {
            $rootPath = Resolve-Path (Join-Path $PSScriptRoot "..")
            Push-Location $rootPath
            Write-Host "Running: dotnet ef database update --project Hi-Trade.DAL --startup-project Hi-Trade"
            dotnet ef database update --project Hi-Trade.DAL --startup-project Hi-Trade
            Write-Host "Migrations applied successfully." -ForegroundColor Green
        }
        catch {
            Write-Host "Warning: dotnet ef update encountered an issue. Falling back to init-migrations.sql..." -ForegroundColor Yellow
            $initSql = Join-Path $PSScriptRoot "init-migrations.sql"
            if (Test-Path $initSql) {
                Get-Content $initSql -Raw | docker exec -i $ContainerName /bin/bash -c "
                if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
                    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$Password' -C
                else
                    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '$Password'
                fi"
            }
        }
        finally {
            Pop-Location
        }
    } else {
        Write-Host "dotnet CLI not found. Applying migrations via init-migrations.sql..." -ForegroundColor Yellow
        $initSql = Join-Path $PSScriptRoot "init-migrations.sql"
        if (Test-Path $initSql) {
            Get-Content $initSql -Raw | docker exec -i $ContainerName /bin/bash -c "
            if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
                /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$Password' -C
            else
                /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '$Password'
            fi"
        }
    }
} else {
    Write-Host "[3/4] Skipping migrations step (-RunMigrations:`$false)." -ForegroundColor DarkGray
}

# 4. Execute seed script
Write-Host "[4/4] Executing asset seed script..." -ForegroundColor Yellow
$seedSql = Join-Path $PSScriptRoot "seed-assets.sql"
if (-not (Test-Path $seedSql)) {
    Write-Host "Error: '$seedSql' not found." -ForegroundColor Red
    exit 1
}

Get-Content $seedSql -Raw | docker exec -i $ContainerName /bin/bash -c "
if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '$Password' -C
else
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '$Password'
fi"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nAll current assets seeded successfully!" -ForegroundColor Green
} else {
    Write-Host "`nFailed to seed assets." -ForegroundColor Red
    exit $LASTEXITCODE
}
