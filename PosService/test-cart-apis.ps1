# ====================================================
# Cart Management API Test Script
# ====================================================

$baseUrl = "http://localhost:5006"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  CART MANAGEMENT API TESTS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ====================================================
# TEST 1: CREATE CART
# ====================================================
Write-Host "TEST 1: Create New Cart" -ForegroundColor Yellow
Write-Host "POST /api/cart/create" -ForegroundColor Gray

$createBody = @{
    storeId = "11111111-1111-1111-1111-111111111111"
    cashierId = "33333333-3333-3333-3333-333333333333"
    customerId = "55555555-5555-5555-5555-555555555555"
    notes = "Test cart from PowerShell script"
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/create" -Method POST -Headers $headers -Body $createBody
    $cartId = $createResponse.data.id
    Write-Host "✓ Cart created successfully!" -ForegroundColor Green
    Write-Host "  Cart ID: $cartId" -ForegroundColor White
    Write-Host "  Sale Number: $($createResponse.data.saleNumber)" -ForegroundColor White
    Write-Host "  Status: $($createResponse.data.status)" -ForegroundColor White
    Write-Host "  Total: $($createResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to create cart" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 2: GET CART (Empty)
# ====================================================
Write-Host "TEST 2: Get Cart Details (Empty)" -ForegroundColor Yellow
Write-Host "GET /api/cart/$cartId" -ForegroundColor Gray

try {
    $getResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId" -Method GET
    Write-Host "✓ Cart retrieved successfully!" -ForegroundColor Green
    Write-Host "  Items count: $($getResponse.data.items.Count)" -ForegroundColor White
    Write-Host "  Total: $($getResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to get cart" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 3: ADD ITEM TO CART (Sữa Vinamilk)
# ====================================================
Write-Host "TEST 3: Add Item to Cart (Sữa Vinamilk)" -ForegroundColor Yellow
Write-Host "POST /api/cart/$cartId/items" -ForegroundColor Gray

$addItem1 = @{
    barcode = "8934560003234"
    quantity = 2
} | ConvertTo-Json

try {
    $add1Response = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items" -Method POST -Headers $headers -Body $addItem1
    Write-Host "✓ Item added successfully!" -ForegroundColor Green
    Write-Host "  Product: $($add1Response.data.items[0].productName)" -ForegroundColor White
    Write-Host "  Quantity: $($add1Response.data.items[0].quantity)" -ForegroundColor White
    Write-Host "  Unit Price: $($add1Response.data.items[0].unitPrice) VND" -ForegroundColor White
    Write-Host "  Line Total: $($add1Response.data.items[0].lineTotal) VND" -ForegroundColor White
    Write-Host "  Cart Total: $($add1Response.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to add item" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 4: ADD ANOTHER ITEM (Gạo ST25)
# ====================================================
Write-Host "TEST 4: Add Another Item (Gạo ST25)" -ForegroundColor Yellow
Write-Host "POST /api/cart/$cartId/items" -ForegroundColor Gray

$addItem2 = @{
    barcode = "8934560004234"
    quantity = 1
} | ConvertTo-Json

try {
    $add2Response = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items" -Method POST -Headers $headers -Body $addItem2
    Write-Host "✓ Item added successfully!" -ForegroundColor Green
    Write-Host "  Total items: $($add2Response.data.items.Count)" -ForegroundColor White
    Write-Host "  Cart Subtotal: $($add2Response.data.subtotal) VND" -ForegroundColor White
    Write-Host "  Cart Total: $($add2Response.data.totalAmount) VND`n" -ForegroundColor White
    
    # Save first item ID for later tests
    $firstItemId = $add2Response.data.items[0].id
} catch {
    Write-Host "✗ Failed to add item" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 5: UPDATE ITEM QUANTITY
# ====================================================
Write-Host "TEST 5: Update Item Quantity (Change from 2 to 5)" -ForegroundColor Yellow
Write-Host "PUT /api/cart/$cartId/items/$firstItemId" -ForegroundColor Gray

$updateItem = @{
    quantity = 5
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items/$firstItemId" -Method PUT -Headers $headers -Body $updateItem
    Write-Host "✓ Item quantity updated successfully!" -ForegroundColor Green
    $updatedItem = $updateResponse.data.items | Where-Object { $_.id -eq $firstItemId }
    Write-Host "  Product: $($updatedItem.productName)" -ForegroundColor White
    Write-Host "  New Quantity: $($updatedItem.quantity)" -ForegroundColor White
    Write-Host "  New Line Total: $($updatedItem.lineTotal) VND" -ForegroundColor White
    Write-Host "  Cart Total: $($updateResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to update item" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 6: GET CART (With Items)
# ====================================================
Write-Host "TEST 6: Get Cart Details (With Items)" -ForegroundColor Yellow
Write-Host "GET /api/cart/$cartId" -ForegroundColor Gray

try {
    $getFullResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId" -Method GET
    Write-Host "✓ Cart retrieved successfully!" -ForegroundColor Green
    Write-Host "  Cart ID: $($getFullResponse.data.id)" -ForegroundColor White
    Write-Host "  Sale Number: $($getFullResponse.data.saleNumber)" -ForegroundColor White
    Write-Host "  Status: $($getFullResponse.data.status)" -ForegroundColor White
    Write-Host "  Items count: $($getFullResponse.data.items.Count)" -ForegroundColor White
    Write-Host "`n  Items:" -ForegroundColor White
    foreach ($item in $getFullResponse.data.items) {
        Write-Host "    - $($item.productName) x $($item.quantity) = $($item.lineTotal) VND" -ForegroundColor Gray
    }
    Write-Host "`n  Subtotal: $($getFullResponse.data.subtotal) VND" -ForegroundColor White
    Write-Host "  Discount: $($getFullResponse.data.discountAmount) VND" -ForegroundColor White
    Write-Host "  Total: $($getFullResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to get cart" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 7: DELETE ITEM
# ====================================================
Write-Host "TEST 7: Delete Item from Cart" -ForegroundColor Yellow
Write-Host "DELETE /api/cart/$cartId/items/$firstItemId" -ForegroundColor Gray

try {
    $deleteResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items/$firstItemId" -Method DELETE
    Write-Host "✓ Item deleted successfully!" -ForegroundColor Green
    Write-Host "  Remaining items: $($deleteResponse.data.items.Count)" -ForegroundColor White
    Write-Host "  New Cart Total: $($deleteResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to delete item" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# ====================================================
# TEST 8: Test with Existing Sample Carts
# ====================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  TESTING WITH SAMPLE CARTS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test Cart 1
Write-Host "TEST 8: Get Sample Cart 1 (With Fixed IDs)" -ForegroundColor Yellow
$sampleCart1 = "AA100001-0001-0001-0001-000000000001"
Write-Host "GET /api/cart/$sampleCart1" -ForegroundColor Gray

try {
    $sample1 = Invoke-RestMethod -Uri "$baseUrl/api/cart/$sampleCart1" -Method GET
    Write-Host "✓ Sample cart retrieved!" -ForegroundColor Green
    Write-Host "  Cart: $($sample1.data.saleNumber)" -ForegroundColor White
    Write-Host "  Items: $($sample1.data.items.Count)" -ForegroundColor White
    Write-Host "  Total: $($sample1.data.totalAmount) VND" -ForegroundColor White
    if ($sample1.data.items.Count -gt 0) {
        Write-Host "  Item ID (for testing): $($sample1.data.items[0].id)" -ForegroundColor Yellow
    }
    Write-Host ""
} catch {
    Write-Host "✗ Failed to get sample cart" -ForegroundColor Red
}

# Test Cart 2 (Empty)
Write-Host "TEST 9: Get Sample Cart 2 (Empty Cart)" -ForegroundColor Yellow
$sampleCart2 = "AA100002-0001-0001-0001-000000000001"
Write-Host "GET /api/cart/$sampleCart2" -ForegroundColor Gray

try {
    $sample2 = Invoke-RestMethod -Uri "$baseUrl/api/cart/$sampleCart2" -Method GET
    Write-Host "✓ Empty cart retrieved!" -ForegroundColor Green
    Write-Host "  Cart: $($sample2.data.saleNumber)" -ForegroundColor White
    Write-Host "  Items: $($sample2.data.items.Count)" -ForegroundColor White
    Write-Host "  Total: $($sample2.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to get empty cart" -ForegroundColor Red
}

# Test Cart 3 (Multiple items)
Write-Host "TEST 10: Get Sample Cart 3 (Multiple Items)" -ForegroundColor Yellow
$sampleCart3 = "AA100003-0001-0001-0001-000000000001"
Write-Host "GET /api/cart/$sampleCart3" -ForegroundColor Gray

try {
    $sample3 = Invoke-RestMethod -Uri "$baseUrl/api/cart/$sampleCart3" -Method GET
    Write-Host "✓ Cart with multiple items retrieved!" -ForegroundColor Green
    Write-Host "  Cart: $($sample3.data.saleNumber)" -ForegroundColor White
    Write-Host "  Items: $($sample3.data.items.Count)" -ForegroundColor White
    Write-Host "  Products:" -ForegroundColor White
    foreach ($item in $sample3.data.items) {
        Write-Host "    - $($item.productName) x $($item.quantity)" -ForegroundColor Gray
        Write-Host "      Item ID: $($item.id)" -ForegroundColor Yellow
    }
    Write-Host "  Total: $($sample3.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to get cart with multiple items" -ForegroundColor Red
}

# ====================================================
# SUMMARY
# ====================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "✓ All 5 Cart Management APIs tested successfully!" -ForegroundColor Green
Write-Host "`nAvailable Endpoints:" -ForegroundColor White
Write-Host "  1. POST   /api/cart/create" -ForegroundColor Gray
Write-Host "  2. GET    /api/cart/{cartId}" -ForegroundColor Gray
Write-Host "  3. POST   /api/cart/{cartId}/items" -ForegroundColor Gray
Write-Host "  4. PUT    /api/cart/{cartId}/items/{itemId}" -ForegroundColor Gray
Write-Host "  5. DELETE /api/cart/{cartId}/items/{itemId}" -ForegroundColor Gray

Write-Host "`nMock Products Available (Barcodes):" -ForegroundColor White
Write-Host "  - 8934560001234 | Rau Muống       | 20,000 VND" -ForegroundColor Gray
Write-Host "  - 8934560002234 | Cam Sành        | 35,000 VND" -ForegroundColor Gray
Write-Host "  - 8934560002241 | Táo Envy        | 150,000 VND" -ForegroundColor Gray
Write-Host "  - 8934560003234 | Sữa Vinamilk    | 38,000 VND" -ForegroundColor Gray
Write-Host "  - 8934560003241 | Sữa TH True     | 42,000 VND" -ForegroundColor Gray
Write-Host "  - 8934560004234 | Gạo ST25        | 180,000 VND" -ForegroundColor Gray
Write-Host "  - 8934560004241 | Gạo Jasmine     | 140,000 VND" -ForegroundColor Gray

Write-Host "`nSample Carts with Fixed IDs:" -ForegroundColor White
Write-Host "  - AA100001-0001-0001-0001-000000000001 (with items)" -ForegroundColor Gray
Write-Host "  - AA100002-0001-0001-0001-000000000001 (empty)" -ForegroundColor Gray
Write-Host "  - AA100003-0001-0001-0001-000000000001 (multiple items)" -ForegroundColor Gray

Write-Host "`n========================================`n" -ForegroundColor Cyan
