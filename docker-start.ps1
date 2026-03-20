# Docker Quick Start Script for Identity Service
# Usage: .\docker-start.ps1 [command]

param(
    [string]$Command = "help"
)

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$composePath = Join-Path $scriptPath ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Identity Service - Docker Management" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

function Show-Help {
    Write-Host "Available Commands:" -ForegroundColor Green
    Write-Host ""
    Write-Host "  up              - Start all containers (build if needed)" -ForegroundColor Yellow
    Write-Host "  down            - Stop all containers" -ForegroundColor Yellow
    Write-Host "  restart         - Restart all containers" -ForegroundColor Yellow
    Write-Host "  clean           - Stop and remove all containers/volumes" -ForegroundColor Yellow
    Write-Host "  build           - Build Docker images" -ForegroundColor Yellow
    Write-Host "  logs            - View logs from all containers" -ForegroundColor Yellow
    Write-Host "  logs-api        - View logs from Identity API" -ForegroundColor Yellow
    Write-Host "  logs-db         - View logs from SQL Server" -ForegroundColor Yellow
    Write-Host "  ps              - Show running containers" -ForegroundColor Yellow
    Write-Host "  shell-api       - Open shell in Identity API container" -ForegroundColor Yellow
    Write-Host "  shell-db        - Open shell in SQL Server container" -ForegroundColor Yellow
    Write-Host "  health          - Check health status" -ForegroundColor Yellow
    Write-Host "  stop            - Stop containers without removing" -ForegroundColor Yellow
    Write-Host "  migrate         - Run database migrations" -ForegroundColor Yellow
    Write-Host ""
}

function Start-Containers {
    Write-Host "Building Docker images..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose build
    
    Write-Host "Starting containers..." -ForegroundColor Green
    docker-compose up -d
    
    Write-Host ""
    Write-Host "Waiting for services to be ready..." -ForegroundColor Cyan
    Start-Sleep -Seconds 10
    
    Write-Host "Checking container status..." -ForegroundColor Green
    docker-compose ps
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Services Started Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Access the services:" -ForegroundColor Yellow
    Write-Host "  - Swagger UI:    http://localhost:8080/" -ForegroundColor Cyan
    Write-Host "  - Health Check:  http://localhost:8080/health" -ForegroundColor Cyan
    Write-Host "  - SQL Server:    localhost,1433 (sa user)" -ForegroundColor Cyan
    Write-Host ""
}

function Stop-Containers {
    Write-Host "Stopping containers..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose stop
    Write-Host "Containers stopped." -ForegroundColor Green
}

function Stop-AndRemove-Containers {
    Write-Host "Stopping and removing containers/volumes..." -ForegroundColor Red
    Set-Location $composePath
    docker-compose down -v
    Write-Host "Containers and volumes removed." -ForegroundColor Green
}

function Restart-Containers {
    Write-Host "Restarting containers..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose restart
    Write-Host "Containers restarted." -ForegroundColor Green
}

function Build-Images {
    Write-Host "Building Docker images..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose build --no-cache
    Write-Host "Build complete." -ForegroundColor Green
}

function View-Logs {
    Write-Host "Viewing logs (Press Ctrl+C to exit)..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose logs -f
}

function View-ApiLogs {
    Write-Host "Viewing Identity API logs (Press Ctrl+C to exit)..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose logs -f identity-api
}

function View-DbLogs {
    Write-Host "Viewing SQL Server logs (Press Ctrl+C to exit)..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose logs -f sqlserver
}

function Show-ProcessStatus {
    Write-Host "Running containers:" -ForegroundColor Green
    Set-Location $composePath
    docker-compose ps
}

function Open-ApiShell {
    Write-Host "Opening shell in Identity API container..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose exec identity-api sh
}

function Open-DbShell {
    Write-Host "Opening shell in SQL Server container..." -ForegroundColor Green
    Set-Location $composePath
    docker-compose exec sqlserver bash
}

function Check-Health {
    Write-Host "Checking health status..." -ForegroundColor Green
    Write-Host ""
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -UseBasicParsing
        $content = $response.Content | ConvertFrom-Json
        
        Write-Host "API Health Status:" -ForegroundColor Green
        Write-Host "  Status: $($content.status)" -ForegroundColor Cyan
        Write-Host "  Database: $($content.database)" -ForegroundColor Cyan
    }
    catch {
        Write-Host "API Health Status: UNAVAILABLE" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "Docker Containers Status:" -ForegroundColor Green
    Set-Location $composePath
    docker-compose ps
}

function Run-Migrations {
    Write-Host "Running database migrations..." -ForegroundColor Green
    Set-Location "$composePath\IdentityService"
    
    Write-Host "Applying migrations..." -ForegroundColor Yellow
    docker-compose -f "$composePath\docker-compose.yml" exec identity-api dotnet ef database update `
        -p src/IdentityService.Infrastructure `
        -s src/IdentityService.API
    
    Write-Host "Migrations complete." -ForegroundColor Green
}

# Execute command
switch ($Command.ToLower()) {
    "up" { Start-Containers }
    "down" { Stop-Containers }
    "clean" { Stop-AndRemove-Containers }
    "restart" { Restart-Containers }
    "build" { Build-Images }
    "logs" { View-Logs }
    "logs-api" { View-ApiLogs }
    "logs-db" { View-DbLogs }
    "ps" { Show-ProcessStatus }
    "shell-api" { Open-ApiShell }
    "shell-db" { Open-DbShell }
    "health" { Check-Health }
    "stop" { Stop-Containers }
    "migrate" { Run-Migrations }
    "help" { Show-Help }
    default {
        Write-Host "Unknown command: $Command" -ForegroundColor Red
        Write-Host ""
        Show-Help
    }
}
