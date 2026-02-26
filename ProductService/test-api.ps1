# Test API POST /products - PowerShell Script
# Chạy script này để test API với CategoryId thật từ database

# Lưu ý: Thay YOUR_JWT_TOKEN bằng token thực tế

$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMjIyZDc2ZS00NDc4LTQ2ZjgtOGYxNC05YWZmZjg3YzI5MjEiLCJlbWFpbCI6ImFkbWluQGNvbXBhbnkuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJyb2xlX2lkIjoiMSIsInN0YXR1cyI6IkFDVElWRSIsImp0aSI6IjBjMjAxZDE5LTI3NDUtNDhmZC05MmNiLWY4MWY1MGM1MDk0NyIsImV4cCI6MTc3MjEzMDUwOCwiaXNzIjoiSWRlbnRpdHlTZXJ2aWNlIiwiYXVkIjoiSWRlbnRpdHlTZXJ2aWNlQ2xpZW50In0.wPKvxDVBS0q9iRDZxZRahdcf8rbg5GM2nyCa30eO7pY"

# ======================================
# BƯỚC 1: Lấy CategoryId từ database
# ======================================
Write-Host "=== BƯỚC 1: Truy vấn CategoryId từ database ===" -ForegroundColor Cyan

$query = @"
SELECT TOP 1 CAST(id AS VARCHAR(36)) as CategoryId, name 
FROM ProductDB.dbo.categories 
WHERE status = 'ACTIVE'
"@

try {
    $result = Invoke-Sqlcmd -Query $query -ServerInstance "localhost" -Username "sa" -Password "12345" -TrustServerCertificate
    $categoryId = $result.CategoryId
    $categoryName = $result.name
    
    Write-Host "✅ Tìm thấy CategoryId: $categoryId ($categoryName)" -ForegroundColor Green
}
catch {
    Write-Host "❌ Lỗi khi truy vấn database: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Vui lòng chạy SQL script 'debug-test-api.sql' để kiểm tra database" -ForegroundColor Yellow
    exit
}

# ======================================
# BƯỚC 2: Chuẩn bị test data
# ======================================
Write-Host "`n=== BƯỚC 2: Chuẩn bị test data ===" -ForegroundColor Cyan

$imagePath = "test-image.jpg"

# Tạo ảnh test nếu chưa có
if (-not (Test-Path $imagePath)) {
    Write-Host "⚠️  Không tìm thấy file ảnh test. Tạo ảnh mẫu..." -ForegroundColor Yellow
    
    # Tạo ảnh 1x1 pixel đơn giản (JPEG header + minimal data)
    $jpegBytes = [byte[]](0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9)
    [System.IO.File]::WriteAllBytes($imagePath, $jpegBytes)
    
    Write-Host "✅ Đã tạo file ảnh test: $imagePath" -ForegroundColor Green
}

# ======================================
# BƯỚC 3: Test API
# ======================================
Write-Host "`n=== BƯỚC 3: Gọi API POST /api/product ===" -ForegroundColor Cyan

$uri = "http://localhost:5001/api/Product"
$headers = @{
    "Authorization" = "Bearer $token"
    "accept" = "*/*"
}

# Tạo form data
$form = @{
    Sku = "TEST-$(Get-Random -Minimum 1000 -Maximum 9999)"
    Name = "Nước Cam Test $(Get-Date -Format 'HHmmss')"
    CategoryId = $categoryId
    Price = 50000
    Unit = "Thùng"
    Brand = "Việt Nam Juice"
    Origin = "Việt Nam"
    Description = "Sản phẩm test tự động"
    IsPerishable = $true
    ShelfLifeDays = 180
    IsAvailable = $true
    IsNew = $true
    IsFeatured = $true
    IsOnSale = $true
    OriginalPrice = 60000
    CostPrice = 20000
    MinOrderQuantity = 1
    MaxOrderQuantity = 100
    QuantityPerUnit = 24
    MainImage = Get-Item -Path $imagePath
}

Write-Host "Request URL: $uri" -ForegroundColor Gray
Write-Host "Product SKU: $($form.Sku)" -ForegroundColor Gray
Write-Host "Category: $categoryId ($categoryName)" -ForegroundColor Gray
Write-Host ""

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Form $form -ContentType "multipart/form-data"
    
    Write-Host "✅ SUCCESS!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Response:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 5 | Write-Host
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    $statusDescription = $_.Exception.Response.StatusDescription
    
    Write-Host "❌ ERROR: $statusCode $statusDescription" -ForegroundColor Red
    Write-Host ""
    
    if ($_.ErrorDetails.Message) {
        Write-Host "Error Details:" -ForegroundColor Yellow
        $_.ErrorDetails.Message | ConvertFrom-Json | ConvertTo-Json -Depth 5 | Write-Host
    }
    else {
        Write-Host "Error Message:" -ForegroundColor Yellow
        Write-Host $_.Exception.Message
    }
}

Write-Host "`n=== Test hoàn tất ===" -ForegroundColor Cyan
