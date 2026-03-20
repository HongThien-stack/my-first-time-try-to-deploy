#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
for i in {1..30}; do
    /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD -Q "SELECT 1" -No -h -1 -w 200 2>/dev/null
    if [ $? -eq 0 ]; then
        echo "SQL Server is ready!"
        break
    fi
    echo "Attempt $i/30 - SQL Server not ready yet..."
    sleep 2
done

# Create IdentityDB if it doesn't exist
echo "Creating IdentityDB..."
/opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P $SA_PASSWORD -Q "
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IdentityDB')
BEGIN
    CREATE DATABASE [IdentityDB];
END
" -No -h -1 -w 200

echo "SQL Server setup complete!"
echo "Starting .NET application..."

# Start the .NET application
exec dotnet IdentityService.API.dll
