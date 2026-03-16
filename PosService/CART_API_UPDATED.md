# POS Service - Cart Management API (Updated)

## Overview
Updated Cart Management APIs for POS Service with the following improvements:
- Removed barcode-based product selection, now using ProductId from frontend
- Integrated with Inventory Service to validate product availability
- Added payment method selection (CASH, VNPAY, MOMO, CARD)
- Customer ID is optional when creating cart
- Simplified API to 4 endpoints + 1 completion endpoint

## API Endpoints

### 1. Create Cart (Start Transaction)
```
POST /api/cart/create
```

**Request Body:**
```json
{
  "storeId": "guid",
  "cashierId": "guid",
  "customerId": "guid or null",
  "paymentMethod": "CASH|VNPAY|MOMO|CARD",
  "notes": "string or null"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Cart created successfully",
  "data": {
    "id": "guid",
    "saleNumber": "SALE-2026-XXX",
    "storeId": "guid",
    "cashierId": "guid",
    "customerId": "guid or null",
    "subtotal": 0,
    "taxAmount": 0,
    "discountAmount": 0,
    "totalAmount": 0,
    "status": "PENDING",
    "paymentStatus": "PENDING",
    "paymentMethod": "CASH|VNPAY|MOMO|CARD",
    "notes": "string",
    "createdAt": "datetime",
    "updatedAt": "datetime",
    "items": []
  }
}
```

---

### 2. Get Cart Details
```
GET /api/cart/{cartId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "saleNumber": "SALE-2026-XXX",
    "storeId": "guid",
    "cashierId": "guid",
    "customerId": "guid or null",
    "subtotal": 1000,
    "taxAmount": 0,
    "discountAmount": 0,
    "totalAmount": 1000,
    "status": "PENDING",
    "paymentStatus": "PENDING",
    "paymentMethod": "CASH",
    "notes": "string",
    "createdAt": "datetime",
    "updatedAt": "datetime",
    "items": [
      {
        "id": "guid",
        "productId": "guid",
        "productName": "Product Name",
        "productSku": "SKU-001",
        "quantity": 2,
        "unitPrice": 500,
        "lineDiscount": 0,
        "lineTotal": 1000
      }
    ]
  }
}
```

---

### 3. Add Product to Cart
```
POST /api/cart/{cartId}/items
```

**Request Body:**
```json
{
  "productId": "guid",
  "quantity": 1
}
```

**Notes:**
- ProductId must exist in Inventory Service (is_sale = true)
- Quantity is validated against available inventory
- If product already exists in cart, quantity is incremented
- Cart totals are recalculated automatically

**Response:** Full cart details (same as GET /api/cart/{cartId})

---

### 4. Update Product Quantity
```
PUT /api/cart/{cartId}/items/{itemId}
```

**Request Body:**
```json
{
  "quantity": 3
}
```

**Notes:**
- Set quantity to 0 to remove the item
- Quantity > 0 updates the item, quantity <= 0 removes it
- Cart totals are recalculated automatically

**Response:** Full cart details (same as GET /api/cart/{cartId})

---

### 5. Remove Product from Cart
```
DELETE /api/cart/{cartId}/items/{itemId}
```

**Response:**
```json
{
  "success": true,
  "message": "Item removed from cart successfully",
  "data": { ... }
}
```

---

### 6. Complete Payment (NEW)
```
POST /api/cart/{cartId}/complete
```

**Response:**
```json
{
  "success": true,
  "message": "Cart completed successfully. Payment saved.",
  "data": {
    "id": "guid",
    "saleNumber": "SALE-2026-XXX",
    "status": "COMPLETED",
    "paymentStatus": "PAID",
    "subtotal": 1000,
    "totalAmount": 1000,
    ...
  }
}
```

**Notes:**
- Changes cart status from PENDING to COMPLETED
- Sets paymentStatus to PAID
- Subtotal is saved to database
- This endpoint is called after payment is processed successfully

---

## Key Changes

### DTOs Updated
1. **AddCartItemDto**: Changed from `Barcode` to `ProductId`
2. **CreateCartDto**: Added `PaymentMethod` parameter

### Architecture Changes
1. Integrated InventoryServiceClient in CartService
2. Product validation happens before adding to cart
3. Inventory availability check is performed
4. PaymentMethod is stored during cart creation

### Database
- Subtotal is now saved when cart is completed
- PaymentStatus and Status fields are updated appropriately
- All cart operations use existing Sales table

### Error Handling
- Returns 400 BadRequest for validation errors
- Returns 404 NotFound when cart doesn't exist
- Returns detailed error messages for inventory issues

---

## Sample Flow

```
1. POST /api/cart/create
   → Creates new PENDING cart

2. POST /api/cart/{cartId}/items
   → Adds product (validates with Inventory Service)

3. PUT /api/cart/{cartId}/items/{itemId}
   → Updates product quantity

4. DELETE /api/cart/{cartId}/items/{itemId}
   → Removes product

5. GET /api/cart/{cartId}
   → Checks final cart contents and total

6. POST /api/cart/{cartId}/complete
   → Completes transaction, saves payment info
```

---

## Configuration

In `appsettings.json`:
```json
{
  "Services": {
    "InventoryService": {
      "Url": "http://localhost:5002"
    }
  }
}
```

Make sure InventoryService is running on the configured URL.

---

## Status Values

- **Status**: PENDING | COMPLETED | CANCELLED
- **PaymentStatus**: PENDING | PAID | FAILED
- **PaymentMethod**: CASH | VNPAY | MOMO | CARD

---

## Tests Needed

- [x] Build succeeded
- [ ] Create cart with optional customerId
- [ ] Add item with ProductId (not barcode)
- [ ] Inventory validation
- [ ] Update quantity
- [ ] Remove item
- [ ] Complete payment
- [ ] Verify subtotal is saved to database
- [ ] Test with Inventory Service integration
