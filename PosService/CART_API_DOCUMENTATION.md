# Cart Management API - POS Service

## Overview
Cart management endpoints for Point of Sale system. A cart is a sale with status = PENDING.

## 🔐 Authentication Required
All Cart APIs require **JWT Bearer Token** with **Store Staff** role.

### How to get JWT Token:

1. **Login as Store Staff** via IdentityService:
```bash
POST http://localhost:5001/api/auth/login
Body: {
  "email": "storestaff1@company.com",
  "password": "StoreStaff123!"
}
```

2. **Copy the token** from response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

3. **Use token in Swagger**:
   - Click "🔓 Authorize" button at top
   - Enter: `Bearer eyJhbGc...` (with "Bearer " prefix)
   - Click "Authorize"

4. **Use token in curl/Postman**:
```bash
curl -H "Authorization: Bearer eyJhbGc..." \
     http://localhost:5006/api/cart/...
```

## Base URL
```
http://localhost:5000/api/cart
```

## Endpoints

### 1. Create Cart (POST /api/cart/create)
Tạo giỏ hàng mới (bắt đầu giao dịch)

**Request Body:**
```json
{
  "storeId": "11111111-1111-1111-1111-111111111111",
  "cashierId": "33333333-3333-3333-3333-333333333333",
  "customerId": "55555555-5555-5555-5555-555555555555",  // Optional
  "notes": "Customer notes"  // Optional
}
```

**Response:**
```json
{
  "success": true,
  "message": "Cart created successfully",
  "data": {
    "id": "aa100001-0001-0001-0001-000000000001",
    "saleNumber": "SALE-2026-011",
    "storeId": "11111111-1111-1111-1111-111111111111",
    "cashierId": "33333333-3333-3333-3333-333333333333",
    "customerId": "55555555-5555-5555-5555-555555555555",
    "subtotal": 0,
    "taxAmount": 0,
    "discountAmount": 0,
    "totalAmount": 0,
    "notes": "Customer notes",
    "createdAt": "2026-03-11T10:30:00Z",
    "updatedAt": null,
    "items": []
  }
}
```

---

### 2. Get Cart (GET /api/cart/{cartId})
Xem chi tiết giỏ hàng + items

**Request:**
```
GET /api/cart/aa100001-0001-0001-0001-000000000001
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "aa100001-0001-0001-0001-000000000001",
    "saleNumber": "SALE-2026-011",
    "storeId": "11111111-1111-1111-1111-111111111111",
    "cashierId": "33333333-3333-3333-3333-333333333333",
    "customerId": "55555555-5555-5555-5555-555555555555",
    "subtotal": 76000,
    "taxAmount": 0,
    "discountAmount": 0,
    "totalAmount": 76000,
    "notes": null,
    "createdAt": "2026-03-11T10:30:00Z",
    "updatedAt": "2026-03-11T10:35:00Z",
    "items": [
      {
        "id": "item-id-1",
        "productId": "F0000001-0001-0001-0001-000000000005",
        "productName": "Sữa Tươi Vinamilk 100%",
        "productSku": "SUA-001",
        "quantity": 2,
        "unitPrice": 38000,
        "lineDiscount": 0,
        "lineTotal": 76000
      }
    ]
  }
}
```

---

### 3. Add Item to Cart (POST /api/cart/{cartId}/items)
Thêm sản phẩm (quét barcode)

**Request:**
```
POST /api/cart/aa100001-0001-0001-0001-000000000001/items
```

**Request Body:**
```json
{
  "barcode": "8934560003234",
  "quantity": 2
}
```

**Response:**
```json
{
  "success": true,
  "message": "Item added to cart successfully",
  "data": {
    "id": "aa100001-0001-0001-0001-000000000001",
    "saleNumber": "SALE-2026-011",
    "subtotal": 76000,
    "totalAmount": 76000,
    "items": [
      {
        "id": "item-id-1",
        "productId": "F0000001-0001-0001-0001-000000000005",
        "productName": "Sữa Tươi Vinamilk 100%",
        "productSku": "SUA-001",
        "quantity": 2,
        "unitPrice": 38000,
        "lineDiscount": 0,
        "lineTotal": 76000
      }
    ]
  }
}
```

**Notes:**
- If product already exists in cart, quantity will be increased
- If product not found by barcode, returns error

---

### 4. Update Cart Item (PUT /api/cart/{cartId}/items/{itemId})
Sửa số lượng

**Request:**
```
PUT /api/cart/aa100001-0001-0001-0001-000000000001/items/item-id-1
```

**Request Body:**
```json
{
  "quantity": 5
}
```

**Response:**
```json
{
  "success": true,
  "message": "Item quantity updated successfully",
  "data": {
    "id": "aa100001-0001-0001-0001-000000000001",
    "subtotal": 190000,
    "totalAmount": 190000,
    "items": [
      {
        "id": "item-id-1",
        "productName": "Sữa Tươi Vinamilk 100%",
        "quantity": 5,
        "unitPrice": 38000,
        "lineTotal": 190000
      }
    ]
  }
}
```

**Notes:**
- If quantity is 0 or negative, item will be removed from cart
- Cart totals are automatically recalculated

---

### 5. Remove Cart Item (DELETE /api/cart/{cartId}/items/{itemId})
Xóa sản phẩm

**Request:**
```
DELETE /api/cart/aa100001-0001-0001-0001-000000000001/items/item-id-1
```

**Response:**
```json
{
  "success": true,
  "message": "Item removed from cart successfully",
  "data": {
    "id": "aa100001-0001-0001-0001-000000000001",
    "subtotal": 0,
    "totalAmount": 0,
    "items": []
  }
}
```

---

## Mock Products Available (for testing barcode scanning)

| Barcode | Product Name | SKU | Price (VND) |
|---------|--------------|-----|-------------|
| 8934560001234 | Rau Muống | RAU-001 | 20,000 |
| 8934560002234 | Cam Sành | TC-001 | 35,000 |
| 8934560002241 | Táo Envy | TC-002 | 150,000 |
| 8934560003234 | Sữa Tươi Vinamilk 100% | SUA-001 | 38,000 |
| 8934560003241 | Sữa TH True Milk | SUA-002 | 42,000 |
| 8934560004234 | Gạo ST25 | GAO-001 | 180,000 |
| 8934560004241 | Gạo Jasmine | GAO-002 | 140,000 |

---

## Business Logic

### Cart = Sale with Status PENDING
- When cart is created, a sale with `status = "PENDING"` is created
- Cart items are stored in `sale_items` table
- Totals are automatically recalculated on every change

### Auto Calculation
On every add/update/delete operation:
```
subtotal = SUM(line_total of all items)
line_total = quantity × unit_price - line_discount
total_amount = subtotal - discount_amount + tax_amount
```

### Adding Items
- If barcode exists in cart → increase quantity
- If barcode new → add as new item
- Product info is fetched from mock product database

---

## Error Responses

**Cart Not Found:**
```json
{
  "success": false,
  "message": "Cart not found or already completed"
}
```

**Product Not Found:**
```json
{
  "success": false,
  "message": "Product with barcode 1234567890 not found"
}
```

**Item Not Found:**
```json
{
  "success": false,
  "message": "Cart or item not found"
}
```

---

## Testing with Swagger

Navigate to: `http://localhost:5000/swagger`

All endpoints are documented and can be tested directly from Swagger UI.

---

## Example Flow

1. **Create Cart**
   ```
   POST /api/cart/create
   { "storeId": "...", "cashierId": "..." }
   → Returns cartId
   ```

2. **Scan Product 1 (Sữa Vinamilk)**
   ```
   POST /api/cart/{cartId}/items
   { "barcode": "8934560003234", "quantity": 2 }
   → Subtotal: 76,000 VND
   ```

3. **Scan Product 2 (Gạo ST25)**
   ```
   POST /api/cart/{cartId}/items
   { "barcode": "8934560004234", "quantity": 1 }
   → Subtotal: 256,000 VND
   ```

4. **Update Quantity**
   ```
   PUT /api/cart/{cartId}/items/{itemId}
   { "quantity": 5 }
   ```

5. **View Cart**
   ```
   GET /api/cart/{cartId}
   → See all items and totals
   ```

6. **Remove Item**
   ```
   DELETE /api/cart/{cartId}/items/{itemId}
   ```
