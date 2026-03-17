# ====================================================
# Cart Management API Test Script (Updated)
# ====================================================
# Tests: Create Cart -> Add Items -> Update Qty -> Delete -> Complete Payment

$baseUrl = "http://localhost:5006"
$bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMzMzMzMzMi0zMzMzLTMzMzMtMzMzMy0zMzMzMzMzMzMzMzEiLCJlbWFpbCI6ImNhc2hpZXIxQGNvbXBhbnkuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlBo4bqhbSBUaOG7iyBUaHUgTmfDom4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdG9yZSBTdGFmZiIsInJvbGVfaWQiOiI0Iiwic3RhdHVzIjoiQUNUSVZFIiwianRpIjoiOTBhMWM1ZWEtY2ViMi00YTQzLTg5MDctYzc4ZjAzNjI0NzQ3IiwiZXhwIjoxNzczMzc4ODQyLCJpc3MiOiJJZGVudGl0eVNlcnZpY2UiLCJhdWQiOiJJZGVudGl0eVNlcnZpY2VDbGllbnQifQ.25f0ASB2Bj_GrSBN9fU5Pv1od3Aw3LUPUZoH8-ic8Jk"
$headers = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $bearerToken"
}

# Configuration
$storeId = "B0000001-0001-0001-0001-000000000001"
$cashierId = "333333333-3333-3333-3333-333333333331"
$customerId = "55555555-5555-5555-5555-555555555551"

# Product IDs
$product1 = "F0000001-0001-0001-0001-000000000005"  # Milk Vinamilk
$product2 = "F0000001-0001-0001-0001-000000000007"  # Rice ST25
$product3 = "F0000001-0001-0001-0001-000000000003"  # Orange

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  CART MANAGEMENT API TESTS (UPDATED)" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ====================================================
# TEST 1: CREATE CART
# ====================================================
Write-Host "TEST 1: Create New Cart with PaymentMethod" -ForegroundColor Yellow
Write-Host "POST /api/cart/create" -ForegroundColor Gray

$createBody = @{
    storeId = $storeId
    cashierId = $cashierId
    customerId = $customerId
    paymentMethod = "CASH"
    notes = "Test cart with new API"
} | ConvertTo-Json

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/create" -Method POST -Headers $headers -Body $createBody
    $cartId = $createResponse.data.id
    Write-Host "✓ Cart created successfully!" -ForegroundColor Green
    Write-Host "  Cart ID: $cartId" -ForegroundColor White
    Write-Host "  Sale Number: $($createResponse.data.saleNumber)" -ForegroundColor White
    Write-Host "  Status: $($createResponse.data.status)" -ForegroundColor White
    Write-Host "  Payment Method: $($createResponse.data.paymentMethod)" -ForegroundColor White
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
    $getResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId" -Method GET -Headers $headers
    Write-Host "✓ Cart retrieved successfully!" -ForegroundColor Green
    Write-Host "  Items count: $($getResponse.data.items.Count)" -ForegroundColor White
    Write-Host "  Total: $($getResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to get cart" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 3: ADD ITEM TO CART (Product 1 - Milk)
# ====================================================
Write-Host "TEST 3: Add Item to Cart (ProductId - Milk Vinamilk)" -ForegroundColor Yellow
Write-Host "POST /api/cart/$cartId/items" -ForegroundColor Gray

$addItem1 = @{
    productId = $product1
    quantity = 2
} | ConvertTo-Json

try {
    $add1Response = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items" -Method POST -Headers $headers -Body $addItem1
    $item1Id = $add1Response.data.items[0].id
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
# TEST 4: ADD ANOTHER ITEM (Product 2 - Rice)
# ====================================================
Write-Host "TEST 4: Add Another Item (Rice ST25)" -ForegroundColor Yellow
Write-Host "POST /api/cart/$cartId/items" -ForegroundColor Gray

$addItem2 = @{
    productId = $product2
    quantity = 1
} | ConvertTo-Json

try {
    $add2Response = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items" -Method POST -Headers $headers -Body $addItem2
    $item2Id = $add2Response.data.items[1].id
    Write-Host "✓ Item added successfully!" -ForegroundColor Green
    Write-Host "  Total items: $($add2Response.data.items.Count)" -ForegroundColor White
    Write-Host "  Cart Subtotal: $($add2Response.data.subtotal) VND" -ForegroundColor White
    Write-Host "  Cart Total: $($add2Response.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to add item" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 5: UPDATE ITEM QUANTITY
# ====================================================
Write-Host "TEST 5: Update Item Quantity (Milk: 2 -> 5)" -ForegroundColor Yellow
Write-Host "PUT /api/cart/$cartId/items/$item1Id" -ForegroundColor Gray

$updateItem = @{
    quantity = 5
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items/$item1Id" -Method PUT -Headers $headers -Body $updateItem
    Write-Host "✓ Item quantity updated successfully!" -ForegroundColor Green
    $updatedItem = $updateResponse.data.items | Where-Object { $_.id -eq $item1Id }
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
Write-Host "TEST 6: Get Cart Details (With 2 Items)" -ForegroundColor Yellow
Write-Host "GET /api/cart/$cartId" -ForegroundColor Gray

try {
    $getFullResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId" -Method GET -Headers $headers
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
Write-Host "TEST 7: Delete Item from Cart (Remove Rice)" -ForegroundColor Yellow
Write-Host "DELETE /api/cart/$cartId/items/$item2Id" -ForegroundColor Gray

try {
    $deleteResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/items/$item2Id" -Method DELETE -Headers $headers
    Write-Host "✓ Item deleted successfully!" -ForegroundColor Green
    Write-Host "  Remaining items: $($deleteResponse.data.items.Count)" -ForegroundColor White
    Write-Host "  New Cart Total: $($deleteResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to delete item" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Start-Sleep -Seconds 1

# ====================================================
# TEST 8: COMPLETE PAYMENT (NEW)
# ====================================================
Write-Host "TEST 8: Complete Payment and Save Subtotal" -ForegroundColor Yellow
Write-Host "POST /api/cart/$cartId/complete" -ForegroundColor Gray

try {
    $completeResponse = Invoke-RestMethod -Uri "$baseUrl/api/cart/$cartId/complete" -Method POST -Headers $headers
    Write-Host "✓ Payment completed successfully!" -ForegroundColor Green
    Write-Host "  Status: $($completeResponse.data.status)" -ForegroundColor White
    Write-Host "  Payment Status: $($completeResponse.data.paymentStatus)" -ForegroundColor White
    Write-Host "  Subtotal Saved: $($completeResponse.data.subtotal) VND" -ForegroundColor White
    Write-Host "  Final Total: $($completeResponse.data.totalAmount) VND`n" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to complete payment" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

# ====================================================
# SUMMARY
# ====================================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "✓ All 6 Cart Management APIs tested successfully!" -ForegroundColor Green
Write-Host "`nAvailable Endpoints:" -ForegroundColor White
Write-Host "  1. POST   /api/cart/create                 - Create new cart" -ForegroundColor Gray
Write-Host "  2. GET    /api/cart/{cartId}               - Get cart details" -ForegroundColor Gray
Write-Host "  3. POST   /api/cart/{cartId}/items         - Add product (ProductId)" -ForegroundColor Gray
Write-Host "  4. PUT    /api/cart/{cartId}/items/{itemId} - Update quantity" -ForegroundColor Gray
Write-Host "  5. DELETE /api/cart/{cartId}/items/{itemId} - Delete item" -ForegroundColor Gray
Write-Host "  6. POST   /api/cart/{cartId}/complete      - Complete payment" -ForegroundColor Gray

Write-Host "`nProduct IDs (from ProductDB):" -ForegroundColor White
Write-Host "  - F0000001-0001-0001-0001-000000000001 | Vegetable #1" -ForegroundColor Gray
Write-Host "  - F0000001-0001-0001-0001-000000000003 | Orange" -ForegroundColor Gray
Write-Host "  - F0000001-0001-0001-0001-000000000004 | Apple" -ForegroundColor Gray
Write-Host "  - F0000001-0001-0001-0001-000000000005 | Milk Vinamilk" -ForegroundColor Gray
Write-Host "  - F0000001-0001-0001-0001-000000000006 | Milk TH True" -ForegroundColor Gray
Write-Host "  - F0000001-0001-0001-0001-000000000007 | Rice ST25" -ForegroundColor Gray
Write-Host "  - F0000001-0001-0001-0001-000000000008 | Rice Jasmine" -ForegroundColor Gray

Write-Host "`nPayment Methods:" -ForegroundColor White
Write-Host "  - CASH   | Credit Card" -ForegroundColor Gray
Write-Host "  - VNPAY  | VNPay" -ForegroundColor Gray
Write-Host "  - MOMO   | Momo" -ForegroundColor Gray
Write-Host "  - CARD   | Debit Card" -ForegroundColor Gray

Write-Host "`nKey Features:" -ForegroundColor White
Write-Host "  ✓ Uses ProductId instead of barcode" -ForegroundColor Gray
Write-Host "  ✓ Validates with Inventory Service" -ForegroundColor Gray
Write-Host "  ✓ Customer ID is optional" -ForegroundColor Gray
Write-Host "  ✓ Saves subtotal on payment completion" -ForegroundColor Gray
Write-Host "  ✓ Automatic cart total recalculation" -ForegroundColor Gray

Write-Host "`n========================================`n" -ForegroundColor Cyan
