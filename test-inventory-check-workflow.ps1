# Script test InventoryCheck APIs
# Workflow: Create -> Submit -> Reconcile -> Approve -> Adjust

Write-Host "`n=== TEST INVENTORY CHECK APIS ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5003/api/inventory-checks"
$identityUrl = "http://localhost:5000/api/auth/login"

# Step 0: Login to get JWT token
Write-Host "Step 0: Đăng nhập để lấy JWT token..." -ForegroundColor Yellow
try {
    $loginBody = @{
        email = "admin@company.com"
        password = "Password123!"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri $identityUrl -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.accessToken
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    Write-Host "✓ Login thành công!" -ForegroundColor Green
    Write-Host "  Token: $($token.Substring(0,60))..." -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Login thất bại: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Đảm bảo IdentityService đang chạy trên port 5000" -ForegroundColor Yellow
    exit
}

# Step 1: Create Inventory Check
Write-Host "Step 1: Tạo phiên kiểm kê mới..." -ForegroundColor Yellow
try {
    $createBody = @{
        locationType = "STORE"
        locationId = "B0000001-0001-0001-0001-000000000001"
        checkType = "FULL"
        notes = "Monthly inventory check for Store Thu Duc"
    } | ConvertTo-Json

    $createResponse = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $createBody -Headers $headers
    $checkId = $createResponse.data.id
    $checkNumber = $createResponse.data.checkNumber
    
    Write-Host "✓ Tạo phiên kiểm kê thành công!" -ForegroundColor Green
    Write-Host "  Check ID: $checkId" -ForegroundColor Gray
    Write-Host "  Check Number: $checkNumber" -ForegroundColor Gray
    Write-Host "  Status: $($createResponse.data.status)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Tạo phiên kiểm kê thất bại: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "  Response: $responseBody" -ForegroundColor Yellow
    }
    exit
}

# Step 2: Submit Inventory Check Results
Write-Host "Step 2: Gửi kết quả kiểm kê (actual counts)..." -ForegroundColor Yellow
try {
    $submitBody = @{
        items = @(
            @{
                productId = "F0000001-0001-0001-0001-000000000001"
                actualQuantity = 45  # System: 50, Actual: 45, Difference: -5
                note = "5 units missing"
            },
            @{
                productId = "F0000001-0001-0001-0001-000000000003"
                actualQuantity = 82  # System: 80, Actual: 82, Difference: +2
                note = "2 units extra found"
            },
            @{
                productId = "F0000001-0001-0001-0001-000000000005"
                actualQuantity = 100  # System: 100, Actual: 100, Difference: 0
                note = "Correct count"
            }
        )
    } | ConvertTo-Json -Depth 10

    $submitResponse = Invoke-RestMethod -Uri "$baseUrl/$checkId/submit" -Method Put -Body $submitBody -Headers $headers
    
    Write-Host "✓ Gửi kết quả kiểm kê thành công!" -ForegroundColor Green
    Write-Host "  Status: $($submitResponse.data.status)" -ForegroundColor Gray
    Write-Host "  Total Discrepancies: $($submitResponse.data.totalDiscrepancies)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Gửi kết quả thất bại: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "  Response: $responseBody" -ForegroundColor Yellow
    }
}

# Step 3: Reconcile (View Discrepancies)
Write-Host "Step 3: Đối soát chênh lệch..." -ForegroundColor Yellow
try {
    $reconcileResponse = Invoke-RestMethod -Uri "$baseUrl/$checkId/reconcile" -Method Post -Headers $headers
    
    Write-Host "✓ Đối soát thành công!" -ForegroundColor Green
    Write-Host "  Số lượng chênh lệch: $($reconcileResponse.data.Count)" -ForegroundColor Gray
    
    foreach ($item in $reconcileResponse.data) {
        $diffColor = if ($item.difference -eq 0) { "Green" } elseif ($item.difference -lt 0) { "Red" } else { "Yellow" }
        Write-Host "  - Product: $($item.productId)" -ForegroundColor Gray
        Write-Host "    System: $($item.systemQuantity), Actual: $($item.actualQuantity), Diff: $($item.difference)" -ForegroundColor $diffColor
    }
    Write-Host ""
}
catch {
    Write-Host "✗ Đối soát thất bại: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Approve Inventory Check
Write-Host "Step 4: Duyệt kết quả kiểm kê..." -ForegroundColor Yellow
try {
    $approveBody = @{
        notes = "Approved by manager. Discrepancies noted and will be adjusted."
    } | ConvertTo-Json

    $approveResponse = Invoke-RestMethod -Uri "$baseUrl/$checkId/approve" -Method Put -Body $approveBody -Headers $headers
    
    Write-Host "✓ Duyệt kết quả thành công!" -ForegroundColor Green
    Write-Host "  Status: $($approveResponse.data.status)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Duyệt kết quả thất bại: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Adjust Inventory
Write-Host "Step 5: Cập nhật tồn kho theo kết quả kiểm kê..." -ForegroundColor Yellow
try {
    $adjustBody = @{
        reason = "Inventory adjustment based on physical count. Approved by manager."
    } | ConvertTo-Json

    $adjustResponse = Invoke-RestMethod -Uri "$baseUrl/$checkId/adjust" -Method Put -Body $adjustBody -Headers $headers
    
    Write-Host "✓ Cập nhật tồn kho thành công!" -ForegroundColor Green
    Write-Host "  Status: $($adjustResponse.data.status)" -ForegroundColor Gray
    Write-Host "  Check Number: $($adjustResponse.data.checkNumber)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Cập nhật tồn kho thất bại: $($_.Exception.Message)" -ForegroundColor Red
}

# Final: Get updated inventory check details
Write-Host "Final: Kiểm tra kết quả cuối cùng..." -ForegroundColor Yellow
try {
    $finalResponse = Invoke-RestMethod -Uri "$baseUrl/$checkId" -Method Get -Headers $headers
    
    Write-Host "✓ Thông tin phiên kiểm kê:" -ForegroundColor Green
    Write-Host "  Check Number: $($finalResponse.data.checkNumber)" -ForegroundColor Gray
    Write-Host "  Status: $($finalResponse.data.status)" -ForegroundColor Gray
    Write-Host "  Total Discrepancies: $($finalResponse.data.totalDiscrepancies)" -ForegroundColor Gray
    Write-Host "  Items Count: $($finalResponse.data.items.Count)" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host "✗ Lấy thông tin thất bại: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "=== TEST HOÀN TẤT ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Workflow đã test:" -ForegroundColor White
Write-Host "  1. ✓ Create inventory check session" -ForegroundColor Gray
Write-Host "  2. ✓ Submit actual counts" -ForegroundColor Gray
Write-Host "  3. ✓ Reconcile discrepancies" -ForegroundColor Gray
Write-Host "  4. ✓ Approve results" -ForegroundColor Gray
Write-Host "  5. ✓ Adjust inventory" -ForegroundColor Gray
Write-Host ""
