# Local Development Script for Identity Service
# Usage: .\local-start.ps1 [command]

param(
    [string]$Command = "help"
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$identityPath = Join-Path $scriptPath "IdentityService"
$apiPath = Join-Path $identityPath "src\IdentityService.API"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Identity Service - Local Development" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

function Show-Help {
    Write-Host "Available Commands:" -ForegroundColor Green
    Write-Host ""
    Write-Host "  setup           - Setup project (restore, build, migrate)" -ForegroundColor Yellow
    Write-Host "  run             - Run Identity Service locally" -ForegroundColor Yellow
    Write-Host "  build           - Build the project" -ForegroundColor Yellow
    Write-Host "  restore         - Restore NuGet packages" -ForegroundColor Yellow
    Write-Host "  clean           - Clean build artifacts" -ForegroundColor Yellow
    Write-Host "  migrate         - Apply database migrations" -ForegroundColor Yellow
    Write-Host "  add-migration   - Create a new migration (requires -Name parameter)" -ForegroundColor Yellow
    Write-Host "  open-swagger    - Open Swagger UI in browser" -ForegroundColor Yellow
    Write-Host "  test-api        - Test API health endpoint" -ForegroundColor Yellow
    Write-Host "  db-update       - Update database with latest migrations" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\local-start.ps1 setup" -ForegroundColor Gray
    Write-Host "  .\local-start.ps1 run" -ForegroundColor Gray
    Write-Host "  .\local-start.ps1 add-migration -Name AddNewColumn" -ForegroundColor Gray
}

function Setup-Project {
    Write-Host "Setting up Identity Service project..." -ForegroundColor Green
    
    Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
    Set-Location $identityPath
    dotnet restore
    
    Write-Host "Building project..." -ForegroundColor Yellow
    dotnet build
    
    Write-Host "Applying database migrations..." -ForegroundColor Yellow
    Apply-Migrations
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Setup Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next, run: .\local-start.ps1 run" -ForegroundColor Yellow
}

function Run-Service {
    Write-Host "Starting Identity Service..." -ForegroundColor Green
    Set-Location $apiPath
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Identity Service is running!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Access the service:" -ForegroundColor Yellow
    Write-Host "  - Swagger UI:    http://localhost:5000/" -ForegroundColor Cyan
    Write-Host "  - API Base:      http://localhost:5000/api" -ForegroundColor Cyan
    Write-Host "  - Health Check:  http://localhost:5000/health" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
    Write-Host ""
    
    dotnet run
}

function Build-Project {
    Write-Host "Building project..." -ForegroundColor Green
    Set-Location $identityPath
    dotnet build
    Write-Host "Build complete." -ForegroundColor Green
}

function Restore-Packages {
    Write-Host "Restoring NuGet packages..." -ForegroundColor Green
    Set-Location $identityPath
    dotnet restore
    Write-Host "Restore complete." -ForegroundColor Green
}

function Clean-Project {
    Write-Host "Cleaning build artifacts..." -ForegroundColor Green
    Set-Location $identityPath
    dotnet clean
    Write-Host "Clean complete." -ForegroundColor Green
}

function Apply-Migrations {
    Write-Host "Applying database migrations..." -ForegroundColor Yellow
    Set-Location $identityPath
    
    try {
        dotnet ef database update `
            -p src/IdentityService.Infrastructure `
            -s src/IdentityService.API
        Write-Host "Migrations applied successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "Error applying migrations: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Make sure:" -ForegroundColor Yellow
        Write-Host "  1. SQL Server is running and accessible" -ForegroundColor Gray
        Write-Host "  2. Update connection string in appsettings.json if needed" -ForegroundColor Gray
        Write-Host "  3. Database 'IdentityDB' exists or will be created" -ForegroundColor Gray
    }
}

function Add-Migration {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )
    
    Write-Host "Creating migration: $Name" -ForegroundColor Green
    Set-Location $identityPath
    
    dotnet ef migrations add $Name `
        -p src/IdentityService.Infrastructure `
        -s src/IdentityService.API
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration created successfully." -ForegroundColor Green
        Write-Host "Migration file: src/IdentityService.Infrastructure/Migrations/$Name.cs" -ForegroundColor Yellow
    }
    else {
        Write-Host "Error creating migration." -ForegroundColor Red
    }
}

function Open-Swagger {
    Write-Host "Opening Swagger UI..." -ForegroundColor Green
    Start-Process "http://localhost:5000"
}

function Test-ApiHealth {
    Write-Host "Testing API health endpoint..." -ForegroundColor Green
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing
        $content = $response.Content | ConvertFrom-Json
        
        Write-Host ""
        Write-Host "Health Status:" -ForegroundColor Green
        Write-Host "  Status: $($content.status)" -ForegroundColor Cyan
        Write-Host "  Database: $($content.database)" -ForegroundColor Cyan
        Write-Host ""
    }
    catch {
        Write-Host "Error: Cannot connect to API at http://localhost:5000" -ForegroundColor Red
        Write-Host "Make sure the service is running: .\local-start.ps1 run" -ForegroundColor Yellow
    }
}

function Update-Database {
    Write-Host "Updating database..." -ForegroundColor Green
    Set-Location $identityPath
    
    try {
        dotnet ef database update `
            -p src/IdentityService.Infrastructure `
            -s src/IdentityService.API
        Write-Host "Database updated successfully." -ForegroundColor Green
    }
    catch {
        Write-Host "Error updating database: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Parse and execute command
switch ($Command.ToLower()) {
    "setup" { Setup-Project }
    "run" { Run-Service }
    "build" { Build-Project }
    "restore" { Restore-Packages }
    "clean" { Clean-Project }
    "migrate" { Apply-Migrations }
    "open-swagger" { Open-Swagger }
    "test-api" { Test-ApiHealth }
    "db-update" { Update-Database }
    "add-migration" { 
        if ($args.Count -gt 0) {
            Add-Migration -Name $args[0] 
        }
        else {
            Write-Host "Error: Migration name required" -ForegroundColor Red
            Write-Host "Usage: .\local-start.ps1 add-migration -Name YourMigrationName" -ForegroundColor Yellow
        }
    }
    "help" { Show-Help }
    default {
        Write-Host "Unknown command: $Command" -ForegroundColor Red
        Write-Host ""
        Show-Help
    }
}

Write-Host ""
