# Test POST /api/inventory-checks

Write-Host "`n=== TEST CREATE INVENTORY CHECK ===" -ForegroundColor Cyan

# Get fresh token
$loginBody = @{
    email = "admin@company.com"
    password = "Password123!"
} | ConvertTo-Json

Write-Host "`nStep 1: Login to get token..." -ForegroundColor Yellow
$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.accessToken
Write-Host "✓ Token obtained (Role: Admin)" -ForegroundColor Green

# Test POST
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$createBody = @{
    locationType = "STORE"
    locationId = "B0000001-0001-0001-0001-000000000001"
    checkType = "FULL"
    notes = "Test from PowerShell - Store Thu Duc monthly check"
} | ConvertTo-Json

Write-Host "`nStep 2: POST /api/inventory-checks..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5003/api/inventory-checks" -Method Post -Body $createBody -Headers $headers
    
    Write-Host "✓ API SUCCESS!" -ForegroundColor Green
    Write-Host "`nInventory Check Created:" -ForegroundColor Cyan
    Write-Host "  Check ID: $($response.data.id)" -ForegroundColor White
    Write-Host "  Check Number: $($response.data.checkNumber)" -ForegroundColor White
    Write-Host "  Location: $($response.data.locationType) - $($response.data.locationId)" -ForegroundColor White
    Write-Host "  Status: $($response.data.status)" -ForegroundColor White
    Write-Host "  Check Type: $($response.data.checkType)" -ForegroundColor White
    
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "✗ API ERROR: Status Code $statusCode" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorBody = $reader.ReadToEnd()
        Write-Host "`nError Details:" -ForegroundColor Yellow
        Write-Host $errorBody
    }
}

Write-Host "`n=== HƯỚNG DẪN TEST TRÊN SWAGGER ===" -ForegroundColor Cyan
Write-Host "1. Mở: http://localhost:5003/swagger" -ForegroundColor White
Write-Host "2. Click nút [Authorize] (biểu tượng khóa ở góc trên)" -ForegroundColor White
Write-Host "3. Nhập token này vào ô Value:" -ForegroundColor White
Write-Host "   $token" -ForegroundColor Yellow
Write-Host "4. Click [Authorize] -> [Close]" -ForegroundColor White
Write-Host "5. Mở endpoint POST /api/inventory-checks -> [Try it out]" -ForegroundColor White
Write-Host "6. Nhập request body:" -ForegroundColor White
Write-Host @"
   {
     "locationType": "STORE",
     "locationId": "B0000001-0001-0001-0001-000000000001",
     "checkType": "FULL",
     "notes": "Test from Swagger UI"
   }
"@ -ForegroundColor Gray
Write-Host "7. Click [Execute]" -ForegroundColor White
Write-Host "`nToken này có hiệu lực trong 60 phút" -ForegroundColor Yellow
