#!/usr/bin/env bash
# ============================================================================
# Hi-Trade Database Asset Seeder (Linux / macOS)
# ============================================================================
set -euo pipefail

CONTAINER_NAME="${1:-hitrade-sqlserver}"
PASSWORD="${2:-HiTrade@2026Password!}"
RUN_MIGRATIONS="${3:-true}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "=========================================="
echo "  Hi-Trade Database Asset Seeder          "
echo "=========================================="

# 1. Check if container is running
echo "[1/4] Checking SQL Server container status..."
if ! docker ps --filter "name=${CONTAINER_NAME}" --format "{{.Names}}" | grep -q "${CONTAINER_NAME}"; then
    echo "Error: Container '${CONTAINER_NAME}' is not running."
    echo "Start it first with: docker compose up -d"
    exit 1
fi
echo "Container '${CONTAINER_NAME}' is running."

# 2. Wait for SQL Server
echo "[2/4] Verifying SQL Server availability..."
READY=false
for i in {1..30}; do
    if docker exec "${CONTAINER_NAME}" /bin/bash -c "
        if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
            /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}' -C -Q 'SELECT 1' > /dev/null 2>&1
        else
            /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}' -Q 'SELECT 1' > /dev/null 2>&1
        fi" 2>/dev/null; then
        READY=true
        break
    fi
    echo "Waiting for SQL Server to accept connections ($i/30)..."
    sleep 2
done

if [ "$READY" != "true" ]; then
    echo "Error: Timed out waiting for SQL Server."
    exit 1
fi
echo "SQL Server is ready."

# 3. Apply migrations if requested
if [ "$RUN_MIGRATIONS" = "true" ]; then
    echo "[3/4] Ensuring migrations are applied..."
    if command -v dotnet >/dev/null 2>&1; then
        (cd "${ROOT_DIR}" && dotnet ef database update --project Hi-Trade.DAL --startup-project Hi-Trade) || {
            echo "Falling back to init-migrations.sql..."
            docker exec -i "${CONTAINER_NAME}" /bin/bash -c "
                if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
                    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}' -C
                else
                    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}'
                fi" < "${SCRIPT_DIR}/init-migrations.sql"
        }
    else
        echo "dotnet CLI not found; running init-migrations.sql..."
        docker exec -i "${CONTAINER_NAME}" /bin/bash -c "
            if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
                /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}' -C
            else
                /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}'
            fi" < "${SCRIPT_DIR}/init-migrations.sql"
    fi
fi

# 4. Run seed script
echo "[4/4] Executing asset seed script..."
docker exec -i "${CONTAINER_NAME}" /bin/bash -c "
    if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}' -C
    else
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '${PASSWORD}'
    fi" < "${SCRIPT_DIR}/seed-assets.sql"

echo ""
echo "Asset seeding completed successfully!"
