# Script kiểm tra tất cả API endpoints trong InventoryService

Write-Host "`n=== KIỂM TRA TẤT CẢ API ENDPOINTS ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5003/api"
$endpoints = @(
    @{ Name = "Inventory (GET All)"; Url = "$baseUrl/Inventory"; RequiresAuth = $false },
    @{ Name = "Inventory (GET by location)"; Url = "$baseUrl/Inventory/location/WAREHOUSE/a0000001-0001-0001-0001-000000000001"; RequiresAuth = $false },
    @{ Name = "Inventory (GET by product)"; Url = "$baseUrl/Inventory/product/f0000001-0001-0001-0001-000000000001"; RequiresAuth = $false },
    @{ Name = "Warehouse (GET All)"; Url = "$baseUrl/Warehouse"; RequiresAuth = $false },
    @{ Name = "Slots (SlotController không có GetAll)"; Url = "N/A"; RequiresAuth = $false; Skip = $true },
    @{ Name = "Batches (GET All)"; Url = "$baseUrl/batches"; RequiresAuth = $true },
    @{ Name = "Inventory Checks"; Url = "$baseUrl/inventory-checks"; RequiresAuth = $true },
    @{ Name = "Damage Reports (GET All)"; Url = "$baseUrl/damage-reports/Get-All-Damage-Reports"; RequiresAuth = $true },
    @{ Name = "Stock Movements (GET All)"; Url = "$baseUrl/stock-movements/get-all"; RequiresAuth = $true },
    @{ Name = "Inventory History"; Url = "$baseUrl/inventory-history"; RequiresAuth = $true }
)

foreach ($endpoint in $endpoints) {
    if ($endpoint.Skip) {
        Write-Host "Skipping: $($endpoint.Name)" -ForegroundColor Gray
        Write-Host ""
        continue
    }
    
    Write-Host "Testing: $($endpoint.Name)..." -NoNewline
    
    try {
        $response = Invoke-RestMethod -Uri $endpoint.Url -Method Get -ErrorAction Stop
        Write-Host " ✓ SUCCESS" -ForegroundColor Green
        if ($response.data) {
            Write-Host "  Data count: $($response.data.Count)" -ForegroundColor Gray
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 401 -and $endpoint.RequiresAuth) {
            Write-Host " ⚠ REQUIRES AUTH (401)" -ForegroundColor Yellow
        }
        elseif ($statusCode -eq 500) {
            Write-Host " ✗ ERROR 500" -ForegroundColor Red
            Write-Host "  Message: $($_.Exception.Message)" -ForegroundColor Red
        }
        else {
            Write-Host " ✗ FAILED ($statusCode)" -ForegroundColor Red
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    Write-Host ""
}

Write-Host "=== HOÀN THÀNH ===" -ForegroundColor Cyan
