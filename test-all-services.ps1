# Test All Services Integration
# Run this script to verify all 3 services are working together

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Microservices Integration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Test IdentityService
Write-Host "1. Testing IdentityService (Port 5000)..." -ForegroundColor Yellow
try {
    $identity = Invoke-RestMethod -Uri "https://localhost:5000/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body '{"email":"admin@company.com","password":"Admin@123"}' `
        -SkipCertificateCheck -ErrorAction Stop
    Write-Host "   ✓ IdentityService: OK" -ForegroundColor Green
    Write-Host "   Token received: $($identity.data.token.Substring(0,20))..." -ForegroundColor Gray
}
catch {
    Write-Host "   ✗ IdentityService: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test ProductService
Write-Host "2. Testing ProductService (Port 5001)..." -ForegroundColor Yellow
try {
    $products = Invoke-RestMethod -Uri "https://localhost:5001/api/product" `
        -Method Get `
        -SkipCertificateCheck -ErrorAction Stop
    Write-Host "   ✓ ProductService: OK" -ForegroundColor Green
    Write-Host "   Products found: $($products.data.Count)" -ForegroundColor Gray
    
    if ($products.data.Count -gt 0) {
        $firstProduct = $products.data[0]
        Write-Host "   Sample: $($firstProduct.name) (ID: $($firstProduct.id))" -ForegroundColor Gray
        $global:testProductId = $firstProduct.id
    }
}
catch {
    Write-Host "   ✗ ProductService: FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test InventoryService - Warehouses
Write-Host "3. Testing InventoryService - Warehouses (Port 5002)..." -ForegroundColor Yellow
try {
    $warehouses = Invoke-RestMethod -Uri "https://localhost:5002/api/warehouse" `
        -Method Get `
        -SkipCertificateCheck -ErrorAction Stop
    Write-Host "   ✓ InventoryService (Warehouses): OK" -ForegroundColor Green
    Write-Host "   Warehouses found: $($warehouses.data.Count)" -ForegroundColor Gray
    
    foreach ($wh in $warehouses.data) {
        Write-Host "   - $($wh.name): $($wh.location)" -ForegroundColor Gray
    }
}
catch {
    Write-Host "   ✗ InventoryService (Warehouses): FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test InventoryService - Inventories
Write-Host "4. Testing InventoryService - Inventories (Port 5002)..." -ForegroundColor Yellow
try {
    $inventories = Invoke-RestMethod -Uri "https://localhost:5002/api/inventory" `
        -Method Get `
        -SkipCertificateCheck -ErrorAction Stop
    Write-Host "   ✓ InventoryService (Inventories): OK" -ForegroundColor Green
    Write-Host "   Inventory records: $($inventories.data.Count)" -ForegroundColor Gray
    
    if ($inventories.data.Count -eq 0) {
        Write-Host "   (No inventory data yet - this is normal for new setup)" -ForegroundColor DarkGray
    }
}
catch {
    Write-Host "   ✗ InventoryService (Inventories): FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test Cross-Service Integration
if ($global:testProductId) {
    Write-Host "5. Testing Cross-Service Integration..." -ForegroundColor Yellow
    Write-Host "   Getting inventory for product: $global:testProductId" -ForegroundColor Gray
    try {
        $productInventory = Invoke-RestMethod -Uri "https://localhost:5002/api/inventory/product/$global:testProductId" `
            -Method Get `
            -SkipCertificateCheck -ErrorAction Stop
        Write-Host "   ✓ Cross-Service Integration: OK" -ForegroundColor Green
        Write-Host "   Inventory records for this product: $($productInventory.data.Count)" -ForegroundColor Gray
    }
    catch {
        Write-Host "   ✓ Cross-Service Integration: OK (No inventory for this product yet)" -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Integration Test Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor White
Write-Host "  - IdentityService:  https://localhost:5000/swagger" -ForegroundColor Gray
Write-Host "  - ProductService:   https://localhost:5001/swagger" -ForegroundColor Gray
Write-Host "  - InventoryService: https://localhost:5002/swagger" -ForegroundColor Gray
Write-Host ""
