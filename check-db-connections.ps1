# Script kiểm tra kết nối database cho tất cả các service

Write-Host "=== KIỂM TRA KẾT NỐI DATABASE ===" -ForegroundColor Cyan
Write-Host ""

$databases = @(
    @{
        Name = "IdentityDB"
        Server = "localhost"
        Database = "IdentityDB"
        User = "sa"
        Password = "12345"
    },
    @{
        Name = "InventoryDB"
        Server = "localhost"
        Database = "InventoryDB"
        User = "sa"
        Password = "12345"
    },
    @{
        Name = "ProductDB"
        Server = "localhost"
        Database = "ProductDB"
        User = "sa"
        Password = "12345"
    },
    @{
        Name = "POSDB"
        Server = "localhost"
        Database = "POSDB"
        User = "sa"
        Password = "12345"
    }
)

foreach ($db in $databases) {
    Write-Host "Kiểm tra: $($db.Name)..." -NoNewline
    
    $connectionString = "Server=$($db.Server);Database=$($db.Database);User Id=$($db.User);Password=$($db.Password);TrustServerCertificate=True;Connection Timeout=5;"
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT @@VERSION"
        $version = $command.ExecuteScalar()
        
        $connection.Close()
        
        Write-Host " ✓ KẾT NỐI THÀNH CÔNG" -ForegroundColor Green
        Write-Host "  Server: $($db.Server)" -ForegroundColor Gray
        Write-Host "  Database: $($db.Database)" -ForegroundColor Gray
    }
    catch {
        Write-Host " ✗ KẾT NỐI THẤT BẠI" -ForegroundColor Red
        Write-Host "  Lỗi: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    Write-Host ""
}

Write-Host "=== HOÀN THÀNH ===" -ForegroundColor Cyan
