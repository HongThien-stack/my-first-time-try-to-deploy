# 🔌 API ENDPOINTS - COMPLETE DOCUMENTATION

## 📋 **OVERVIEW**

Hệ thống Internal SCM bao gồm **6 microservices** với tổng cộng **120+ API endpoints**.

---

## **🎯 PRIORITY LEVELS**

- **P0 (CRITICAL):** Must have for MVP - Week 1-6
- **P1 (HIGH):** Important features - Week 7-12  
- **P2 (MEDIUM):** Nice to have - Week 13-17
- **P3 (LOW):** Optional/Future - Week 18+

---

# **1. IDENTITYSERVICE** (Port 5000-5001)

**Base URL:** `http://localhost:5000/api`

## **Auth APIs**

### ✅ **POST /api/auth/register** [P0]
Đăng ký user mới
```json
Request:
{
  "email": "user@company.com",
  "password": "Password123!",
  "fullName": "Nguyen Van A",
  "phone": "0901234567",
  "roleId": 2
}

Response: 201 Created
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": "uuid",
    "email": "user@company.com",
    "fullName": "Nguyen Van A"
  }
}
```

### ✅ **POST /api/auth/login** [P0]
Đăng nhập
```json
Request:
{
  "email": "admin@company.com",
  "password": "Password123!"
}

Response: 200 OK
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "user": {
      "id": "uuid",
      "email": "admin@company.com",
      "fullName": "Admin User",
      "role": "Admin",
      "workplace": null
    }
  }
}

// For Manager/Staff with workplace:
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600,
    "user": {
      "id": "22222222-2222-2222-2222-222222222221",
      "email": "manager1@company.com",
      "fullName": "Trần Thị Manager",
      "role": "Manager",
      "workplace": {
        "type": "WAREHOUSE",
        "id": "W0000001-0001-0001-0001-000000000001",
        "name": "Kho Tổng HCM",
        "code": "W-HCM",
        "address": "123 Đường ABC, Q. Thủ Đức, TP.HCM"
      }
    }
  }
}
```

### ✅ **POST /api/auth/refresh-token** [P0]
Làm mới access token
```json
Request:
{
  "refreshToken": "refresh_token_here"
}

Response: 200 OK
{
  "accessToken": "new_access_token",
  "refreshToken": "new_refresh_token",
  "expiresIn": 3600
}
```

### ✅ **POST /api/auth/logout** [P1]
Đăng xuất

### ✅ **POST /api/auth/forgot-password** [P2]
Quên mật khẩu (Send OTP)

### ✅ **POST /api/auth/reset-password** [P2]
Reset mật khẩu với OTP

### **POST /api/auth/change-password** [P1]
Đổi mật khẩu

### ✅ **GET /api/auth/me** [P0]
Lấy thông tin user hiện tại (bao gồm workplace)
```json
Response: 200 OK
{
  "success": true,
  "data": {
    "id": "22222222-2222-2222-2222-222222222221",
    "email": "manager1@company.com",
    "fullName": "Trần Thị Manager",
    "role": "Manager",
    "workplace": {
      "type": "WAREHOUSE",
      "id": "W0000001-0001-0001-0001-000000000001",
      "name": "Kho Tổng HCM",
      "code": "W-HCM",
      "address": "123 Đường ABC, Q. Thủ Đức"
    },
    "permissions": {
      "can_manage_inventory": true,
      "can_approve_transfers": true,
      "can_approve_restocks": true
    }
  }
}
```

### ✅ **POST /api/auth/verify-email** [P2]
Xác thực email với OTP

### ✅ **POST /api/auth/resend-email-otp** [P2]
Gửi lại OTP email

---

## **User Management APIs**

### ✅ **GET /api/users** [P0]
Lấy danh sách users (phân trang, filter)
```
Query params:
- page=1
- pageSize=20
- roleId=2
- status=ACTIVE
- search=nguyen
```

### ✅ **GET /api/users/:id** [P0]
Lấy chi tiết user

### ✅ **POST /api/users** [P0]
Tạo user mới (Admin only)

### ✅ **PUT /api/users/:id** [P0]
Cập nhật user

### ✅ **DELETE /api/users/:id** [P1]
Xóa user (soft delete)

### **PUT /api/users/:id/status** [P1]
Thay đổi status (ACTIVE/INACTIVE/SUSPENDED)

### **GET /api/users/:id/login-logs** [P2]
Xem lịch sử đăng nhập của user

### **GET /api/users/:id/audit-logs** [P2]
Xem audit logs của user

---

## **Role APIs**

### ✅ **GET /api/roles** [P0]
Lấy danh sách roles

### **GET /api/roles/:id** [P1]
Lấy chi tiết role

### **POST /api/roles** [P2]
Tạo role mới (Admin only)

### **PUT /api/roles/:id** [P2]
Cập nhật role

---

## **Notification APIs** ⭐ NEW

### **GET /api/notifications/me** [P0]
Lấy notifications của user hiện tại
```
Query params:
- is_read=false (unread only)
- notification_type=LOW_STOCK
- priority=HIGH
- page=1
- pageSize=20
```

### **GET /api/notifications/:id** [P0]
Lấy chi tiết notification

### **PUT /api/notifications/:id/read** [P0]
Đánh dấu notification là đã đọc

### **PUT /api/notifications/mark-all-read** [P0]
Đánh dấu tất cả notifications là đã đọc

### **DELETE /api/notifications/:id** [P1]
Xóa notification

### **POST /api/notifications** [P1]
Tạo notification mới (System/Admin)
```json
Request:
{
  "user_id": "uuid",
  "notification_type": "LOW_STOCK",
  "title": "Sắp hết hàng",
  "message": "Sữa Vinamilk còn 15/30 units",
  "priority": "HIGH",
  "reference_type": "INVENTORY",
  "reference_id": "product-uuid"
}
```

### **GET /api/notifications/unread-count** [P0]
Đếm số notifications chưa đọc

---

## **System Settings APIs** ⭐ NEW

### **GET /api/system-settings** [P0]
Lấy tất cả system settings
```
Query params:
- category=INVENTORY|LOYALTY|PAYMENT|NOTIFICATION|SYSTEM
- is_public=true (public settings only)
```

### **GET /api/system-settings/:key** [P0]
Lấy setting theo key
```json
Response:
{
  "setting_key": "LOW_STOCK_THRESHOLD_PERCENTAGE",
  "setting_value": "20",
  "data_type": "INT",
  "category": "INVENTORY",
  "description": "Ngưỡng cảnh báo hàng sắp hết (%)"
}
```

### **PUT /api/system-settings/:key** [P0]
Cập nhật setting (Admin only)
```json
Request:
{
  "setting_value": "25",
  "updated_by": "admin-uuid"
}
```

### **GET /api/system-settings/category/:category** [P0]
Lấy settings theo category

### **POST /api/system-settings** [P2]
Tạo setting mới (Admin only)

### **DELETE /api/system-settings/:key** [P2]
Xóa setting (Admin only)

---

# **2. PRODUCTSERVICE** (Port 5002-5003)

**Base URL:** `http://localhost:5002/api`

## **Category APIs**

### ✅ **GET /api/categories** [P0]
Lấy danh sách categories
```json
Response: 200 OK
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "Sữa Các Loại",
      "status": "ACTIVE",
      "createdAt": "2024-03-03T10:00:00Z"
    }
  ]
}
```

### ✅ **GET /api/categories/:id** [P0]
Lấy chi tiết category

### ✅ **POST /api/categories** [P1]
Tạo category mới

### ✅ **PUT /api/categories/:id** [P1]
Cập nhật category

### ✅ **DELETE /api/categories/:id** [P1]
Xóa category (soft delete)

---

## **Product APIs**

### ✅ **GET /api/products** [P0]
Lấy danh sách sản phẩm (phân trang, filter, search)
```
Query params:
- page=1
- pageSize=20
- categoryId=uuid
- search=sua
- isAvailable=true
- sortBy=price
- sortOrder=asc
```

### ✅ **GET /api/products/:id** [P0]
Lấy chi tiết sản phẩm

### **GET /api/products/sku/:sku** [P0]
Tìm sản phẩm theo SKU

### **GET /api/products/barcode/:barcode** [P0]
Tìm sản phẩm theo barcode (cho POS scanner)

### ✅ **POST /api/products** [P0]
Tạo sản phẩm mới
```json
Request:
{
  "sku": "SUA-003",
  "barcode": "8934560003248",
  "name": "Sữa Đà Lạt Milk",
  "categoryId": "uuid",
  "brand": "Đà Lạt Milk",
  "origin": "Việt Nam",
  "price": 35000,
  "unit": "Hộp",
  "isPerishable": true,
  "shelfLifeDays": 30,
  "description": "Sữa tươi 100%"
}
```

### ✅ **PUT /api/products/:id** [P0]
Cập nhật sản phẩm

### ✅ **DELETE /api/products/:id** [P1]
Xóa sản phẩm (soft delete)

### **PUT /api/products/:id/image** [P2]
Upload ảnh sản phẩm

### **GET /api/products/:id/inventory** [P0]
Lấy tồn kho của sản phẩm (tích hợp với InventoryService)

---

# **3. INVENTORYSERVICE** (Port 5004-5005)

**Base URL:** `http://localhost:5004/api`

## **Warehouse APIs**

### **GET /api/warehouses** [P0]
Lấy danh sách warehouses
```json
Response: 200 OK
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "name": "Kho Tổng HCM",
      "location": "Quận Thủ Đức, TP.HCM",
      "totalSlots": 120,
      "usedSlots": 85,
      "availableSlots": 35
    }
  ]
}
```

### **GET /api/warehouses/:id** [P0]
Lấy chi tiết warehouse

### **POST /api/warehouses** [P1]
Tạo warehouse mới

### **PUT /api/warehouses/:id** [P1]
Cập nhật warehouse

### **DELETE /api/warehouses/:id** [P2]
Xóa warehouse

---

## **Warehouse Slot APIs**

### **GET /api/warehouses/:warehouseId/slots** [P0]
Lấy danh sách slots trong warehouse
```
Query params:
- zone=A
- status=OCCUPIED|EMPTY
- productId=uuid
```

### **GET /api/slots/:id** [P0]
Lấy chi tiết slot

### **POST /api/warehouses/:warehouseId/slots** [P1]
Tạo slot mới

### **PUT /api/slots/:id** [P1]
Cập nhật slot

### **DELETE /api/slots/:id** [P2]
Xóa slot

---

## **Inventory APIs**

### **GET /api/inventory** [P0]
Lấy tồn kho (tất cả locations)
```
Query params:
- warehouseId=uuid
- storeId=uuid
- productId=uuid
- locationId=uuid
- lowStock=true (< threshold)
```

### **GET /api/inventory/:id** [P0]
Lấy chi tiết inventory record

### **GET /api/inventory/product/:productId** [P0]
Lấy tồn kho theo sản phẩm (all locations)

### **GET /api/inventory/location/:locationId** [P0]
Lấy tồn kho theo location (warehouse/store)

### **POST /api/inventory/check-availability** [P0]
Kiểm tra sản phẩm có sẵn không (cho POS)
```json
Request:
{
  "items": [
    {"productId": "uuid", "quantity": 10},
    {"productId": "uuid", "quantity": 5}
  ],
  "locationId": "store-uuid"
}

Response:
{
  "available": true,
  "items": [
    {
      "productId": "uuid",
      "requested": 10,
      "available": 50,
      "sufficient": true
    }
  ]
}
```

---

## **Stock Movement APIs**

### **GET /api/stock-movements** [P0]
Lấy lịch sử di chuyển hàng
```
Query params:
- locationId=uuid
- productId=uuid
- movementType=INBOUND|OUTBOUND|TRANSFER
- dateFrom=2024-03-01
- dateTo=2024-03-31
```

### **GET /api/stock-movements/:id** [P0]
Lấy chi tiết stock movement

### **POST /api/stock-movements/receive** [P0]
Nhận hàng từ supplier vào warehouse
```json
Request:
{
  "warehouseId": "uuid",
  "supplier": "Vinamilk Co.",
  "poNumber": "PO-2024-001",
  "receivedDate": "2024-03-03",
  "items": [
    {
      "productId": "uuid",
      "quantity": 100,
      "slotId": "uuid",
      "batchNumber": "BATCH-2024-001",
      "manufacturingDate": "2024-02-01",
      "expiryDate": "2024-08-01"
    }
  ],
  "notes": "Received in good condition"
}
```

### **GET /api/stock-movements/:id/items** [P0]
Lấy chi tiết items trong movement

---

## **Product Batch APIs**

### **GET /api/batches** [P0]
Lấy danh sách batches
```
Query params:
- productId=uuid
- warehouseId=uuid
- expiringIn=30 (days)
```

### **GET /api/batches/:id** [P0]
Lấy chi tiết batch

### **GET /api/batches/expiring-soon** [P0]
Lấy batches sắp hết hạn (< 30 ngày)

---

## **Transfer APIs**

### **GET /api/transfers** [P0]
Lấy danh sách transfers
```
Query params:
- status=PENDING|IN_TRANSIT|DELIVERED|CANCELLED
- fromLocationId=uuid
- toLocationId=uuid
- dateFrom=2024-03-01
```

### **GET /api/transfers/:id** [P0]
Lấy chi tiết transfer

### **POST /api/transfers** [P0]
Tạo transfer mới (warehouse → store)
```json
Request:
{
  "fromWarehouseId": "uuid",
  "toStoreId": "uuid",
  "transferDate": "2024-03-03",
  "expectedDelivery": "2024-03-04",
  "items": [
    {
      "productId": "uuid",
      "batchId": "uuid",
      "quantity": 50
    }
  ],
  "notes": "Urgent restock"
}
```

### **PUT /api/transfers/:id/status** [P0]
Cập nhật status transfer (PENDING → IN_TRANSIT → DELIVERED)

### **POST /api/transfers/:id/ship** [P0]
Đánh dấu transfer đã xuất kho (IN_TRANSIT)

### **POST /api/transfers/:id/receive** [P0]
Store nhận hàng từ transfer
```json
Request:
{
  "receivedDate": "2024-03-04",
  "receivedBy": "uuid",
  "items": [
    {
      "productId": "uuid",
      "expectedQuantity": 50,
      "receivedQuantity": 48,
      "note": "2 units damaged in transit"
    }
  ]
}
```

### **PUT /api/transfers/:id/cancel** [P1]
Hủy transfer

---

## **Restock Request APIs**

### **GET /api/restock-requests** [P0]
Lấy danh sách restock requests
```
Query params:
- storeId=uuid
- status=PENDING|APPROVED|PROCESSING|COMPLETED|REJECTED
- priority=NORMAL|HIGH|URGENT
```

### **GET /api/restock-requests/:id** [P0]
Lấy chi tiết restock request

### **POST /api/restock-requests** [P0]
Tạo restock request (store → warehouse)
```json
Request:
{
  "storeId": "uuid",
  "requestedBy": "uuid",
  "priority": "NORMAL",
  "items": [
    {
      "productId": "uuid",
      "requestedQuantity": 50,
      "currentQuantity": 15,
      "reason": "Low stock"
    }
  ],
  "notes": "Please deliver by tomorrow"
}
```

### **PUT /api/restock-requests/:id/approve** [P0]
Duyệt restock request (warehouse manager)

### **PUT /api/restock-requests/:id/reject** [P0]
Từ chối restock request

### **PUT /api/restock-requests/:id/complete** [P0]
Đánh dấu hoàn thành (sau khi transfer delivered)

---

## **Damage Report APIs**

### **GET /api/damage-reports** [P1]
Lấy danh sách damage reports
```
Query params:
- locationId=uuid
- damageType=EXPIRED|PHYSICAL_DAMAGE|QUALITY_ISSUE
- dateFrom=2024-03-01
```

### **GET /api/damage-reports/:id** [P1]
Lấy chi tiết damage report

### **POST /api/damage-reports** [P1]
Tạo damage report
```json
Request:
{
  "locationId": "uuid",
  "reportedBy": "uuid",
  "damageType": "PHYSICAL_DAMAGE",
  "items": [
    {
      "productId": "uuid",
      "batchId": "uuid",
      "quantity": 2,
      "value": 70000
    }
  ],
  "description": "Damaged during transport",
  "photos": ["image1.jpg", "image2.jpg"]
}
```

### **PUT /api/damage-reports/:id/approve** [P1]
Duyệt damage report (manager)

---

## **Inventory Check APIs**

### **GET /api/inventory-checks** [P1]
Lấy danh sách inventory checks
```
Query params:
- locationId=uuid
- checkType=FULL|PARTIAL|SPOT
- status=PENDING|COMPLETED
```

### **GET /api/inventory-checks/:id** [P1]
Lấy chi tiết inventory check

### **POST /api/inventory-checks** [P1]
Tạo inventory check mới
```json
Request:
{
  "locationId": "uuid",
  "checkedBy": "uuid",
  "checkType": "PARTIAL",
  "checkDate": "2024-03-03",
  "items": [
    {
      "productId": "uuid",
      "systemQuantity": 500,
      "actualQuantity": 498,
      "note": "2 units missing"
    }
  ]
}
```

### **PUT /api/inventory-checks/:id/submit** [P1]
Submit inventory check (cập nhật tồn kho)

---

## **Inventory History & Logs APIs**

### **GET /api/inventory-history** [P2]
Lấy lịch sử snapshot tồn kho

### **GET /api/inventory-logs** [P2]
Lấy audit logs của inventory changes

---

# **4. POSSERVICE** (Port 5006-5007)

**Base URL:** `http://localhost:5006/api`

## **Sales APIs**

### **GET /api/sales** [P0]
Lấy danh sách sales
```
Query params:
- storeId=uuid
- cashierId=uuid
- paymentMethod=CASH|CARD|VNPAY|MOMO
- status=COMPLETED|PENDING|CANCELLED
- dateFrom=2024-03-03
- dateTo=2024-03-03
```

### **GET /api/sales/:id** [P0]
Lấy chi tiết sale

### **POST /api/sales** [P0]
Tạo sale mới (checkout)
```json
Request:
{
  "storeId": "uuid",
  "cashierId": "uuid",
  "customerId": "uuid", // Optional
  "items": [
    {
      "productId": "uuid",
      "quantity": 2,
      "unitPrice": 38000,
      "discountAmount": 0
    }
  ],
  "subtotal": 76000,
  "discountAmount": 7600,
  "totalAmount": 68400,
  "paymentMethod": "VNPAY",
  "promotionId": "uuid", // Optional
  "voucherCode": "SAVE10", // Optional
  "pointsUsed": 0
}

Response: 201 Created
{
  "success": true,
  "data": {
    "saleId": "uuid",
    "saleNumber": "SALE-2024-001",
    "totalAmount": 68400,
    "paymentRequired": true,
    "paymentUrl": "https://vnpay.vn/..." // If VNPay/Momo
  }
}
```

### **PUT /api/sales/:id/cancel** [P1]
Hủy sale (before payment)

### **GET /api/sales/:id/receipt** [P0]
Lấy thông tin in hóa đơn

### **GET /api/sales/summary** [P1]
Lấy tổng hợp bán hàng (dashboard)
```
Query params:
- storeId=uuid
- dateFrom=2024-03-01
- dateTo=2024-03-31
```

---

## **Sale Items APIs**

### **GET /api/sales/:saleId/items** [P0]
Lấy danh sách items trong sale

---

# **5. PAYMENTSERVICE** (Port 5008-5009)

**Base URL:** `http://localhost:5008/api`

## **Payment Method APIs**

### **GET /api/payment-methods** [P0]
Lấy danh sách payment methods available
```json
Response:
{
  "data": [
    {
      "code": "CASH",
      "name": "Tiền mặt",
      "isOnline": false,
      "isActive": true
    },
    {
      "code": "VNPAY",
      "name": "VNPay",
      "isOnline": true,
      "isActive": true
    }
  ]
}
```

### **GET /api/payment-methods/:code** [P0]
Lấy chi tiết payment method

---

## **Payment Transaction APIs**

### **GET /api/payments** [P0]
Lấy danh sách payment transactions
```
Query params:
- saleId=uuid
- paymentMethod=VNPAY|MOMO
- status=PENDING|COMPLETED|FAILED
- dateFrom=2024-03-01
```

### **GET /api/payments/:id** [P0]
Lấy chi tiết payment transaction

### **POST /api/payments/initiate** [P0]
Khởi tạo payment (VNPay/Momo)
```json
Request:
{
  "saleId": "uuid",
  "paymentMethod": "VNPAY",
  "amount": 68400,
  "returnUrl": "http://pos.company.com/payment/return",
  "notifyUrl": "http://api.company.com/payments/webhook/vnpay"
}

Response:
{
  "transactionId": "uuid",
  "transactionNumber": "PAY-2024-001",
  "paymentUrl": "https://vnpay.vn/...",
  "qrCode": "https://api.vietqr.io/...",
  "expiresIn": 900 // seconds
}
```

### **POST /api/payments/webhook/vnpay** [P0]
Webhook callback từ VNPay (IPN)

### **GET /api/payments/return/vnpay** [P0]
Return URL từ VNPay (sau khi customer pay)

### **POST /api/payments/webhook/momo** [P0]
Webhook callback từ Momo (IPN)

### **GET /api/payments/return/momo** [P0]
Return URL từ Momo

### **GET /api/payments/:id/status** [P0]
Check status của payment (polling)

---

## **Refund APIs**

### **GET /api/refunds** [P1]
Lấy danh sách refunds

### **GET /api/refunds/:id** [P1]
Lấy chi tiết refund

### **POST /api/refunds** [P1]
Tạo refund request
```json
Request:
{
  "transactionId": "uuid",
  "refundAmount": 68400,
  "reason": "Customer cancelled order"
}
```

### **GET /api/refunds/:id/status** [P1]
Check status refund

---

## **Reconciliation APIs**

### **GET /api/reconciliation** [P2]
Lấy danh sách reconciliation records

### **POST /api/reconciliation/daily** [P2]
Đối soát daily với payment gateway

---

# **6. PROMOTIONLOYALTYSERVICE** (Port 5010-5011)

**Base URL:** `http://localhost:5010/api`

## **Promotion APIs**

### **GET /api/promotions** [P1]
Lấy danh sách promotions
```
Query params:
- status=ACTIVE|INACTIVE
- promotionType=PERCENTAGE|FIXED|BUY_X_GET_Y
- isActive=true
```

### **GET /api/promotions/:id** [P1]
Lấy chi tiết promotion

### **GET /api/promotions/active** [P1]
Lấy promotions đang active

### **POST /api/promotions** [P1]
Tạo promotion mới
```json
Request:
{
  "promotionCode": "FLASH10",
  "name": "Flash Sale 10%",
  "promotionType": "PERCENTAGE",
  "discountPercentage": 10,
  "startDate": "2024-03-01",
  "endDate": "2024-03-31",
  "minPurchaseAmount": 100000,
  "maxDiscountAmount": 50000,
  "usageLimit": 1000,
  "isActive": true
}
```

### **PUT /api/promotions/:id** [P1]
Cập nhật promotion

### **DELETE /api/promotions/:id** [P1]
Xóa promotion

### **POST /api/promotions/validate** [P1]
Validate promotion có áp dụng được không
```json
Request:
{
  "promotionCode": "FLASH10",
  "customerId": "uuid",
  "items": [
    {"productId": "uuid", "quantity": 2, "price": 38000}
  ],
  "subtotal": 76000
}

Response:
{
  "applicable": true,
  "discountAmount": 7600,
  "finalAmount": 68400,
  "message": "Promotion applied successfully"
}
```

---

## **Promotion Rule APIs**

### **GET /api/promotions/:promotionId/rules** [P1]
Lấy rules của promotion

### **POST /api/promotions/:promotionId/rules** [P1]
Thêm rule cho promotion

---

## **Voucher APIs**

### **GET /api/vouchers** [P1]
Lấy danh sách vouchers
```
Query params:
- promotionId=uuid
- customerId=uuid
- isUsed=false
```

### **GET /api/vouchers/:code** [P1]
Lấy voucher theo code

### **POST /api/vouchers/generate** [P1]
Generate vouchers cho promotion
```json
Request:
{
  "promotionId": "uuid",
  "quantity": 100,
  "prefix": "SAVE50K",
  "expiresAt": "2024-03-31"
}
```

### **POST /api/vouchers/use** [P1]
Sử dụng voucher
```json
Request:
{
  "voucherCode": "SAVE50K-001",
  "saleId": "uuid",
  "customerId": "uuid"
}
```

---

## **Membership Tier APIs**

### **GET /api/membership-tiers** [P1]
Lấy danh sách membership tiers
```json
Response:
{
  "data": [
    {
      "id": "uuid",
      "tierName": "BRONZE",
      "tierLevel": 1,
      "minPoints": 0,
      "discountPercentage": 0,
      "pointsMultiplier": 1.0
    },
    {
      "id": "uuid",
      "tierName": "GOLD",
      "tierLevel": 3,
      "minPoints": 5000,
      "discountPercentage": 10,
      "pointsMultiplier": 2.0
    }
  ]
}
```

### **GET /api/membership-tiers/:id** [P1]
Lấy chi tiết tier

### **POST /api/membership-tiers** [P2]
Tạo tier mới (Admin)

### **PUT /api/membership-tiers/:id** [P2]
Cập nhật tier

---

## **Customer Loyalty APIs**

### **GET /api/loyalty/customers** [P1]
Lấy danh sách loyalty customers
```
Query params:
- tierId=uuid
- minPoints=1000
- search=nguyen
```

### **GET /api/loyalty/customers/:customerId** [P1]
Lấy loyalty account của customer
```json
Response:
{
  "customerId": "uuid",
  "tier": {
    "name": "GOLD",
    "level": 3
  },
  "totalPoints": 7500,
  "availablePoints": 6200,
  "lifetimePoints": 15000,
  "totalPurchases": 25000000,
  "purchaseCount": 45,
  "joinedAt": "2024-01-01"
}
```

### **POST /api/loyalty/customers** [P1]
Đăng ký loyalty cho customer mới

### **POST /api/loyalty/customers/:customerId/award-points** [P1]
Tặng points cho customer (sau khi mua hàng)
```json
Request:
{
  "saleId": "uuid",
  "amount": 68400,
  "promotionId": "uuid" // Optional
}

Response:
{
  "pointsAwarded": 130,
  "newBalance": 6330,
  "newTier": "GOLD"
}
```

### **POST /api/loyalty/customers/:customerId/deduct-points** [P1]
Trừ points (khi đổi rewards)

---

## **Points Transaction APIs**

### **GET /api/loyalty/customers/:customerId/points-history** [P1]
Lấy lịch sử points transactions
```
Query params:
- transactionType=EARNED|REDEEMED|EXPIRED
- dateFrom=2024-03-01
```

---

## **Rewards Catalog APIs**

### **GET /api/rewards** [P1]
Lấy danh sách rewards có thể đổi
```json
Response:
{
  "data": [
    {
      "id": "uuid",
      "rewardCode": "VOUCHER-50K",
      "rewardName": "Voucher Giảm 50K",
      "rewardType": "DISCOUNT",
      "pointsCost": 500,
      "discountAmount": 50000,
      "stockQuantity": 100,
      "isActive": true
    }
  ]
}
```

### **GET /api/rewards/:id** [P1]
Lấy chi tiết reward

### **POST /api/rewards** [P2]
Tạo reward mới (Admin)

### **PUT /api/rewards/:id** [P2]
Cập nhật reward

---

## **Reward Redemption APIs**

### **GET /api/redemptions** [P1]
Lấy danh sách redemptions
```
Query params:
- customerId=uuid
- status=PENDING|COMPLETED|CANCELLED
```

### **GET /api/redemptions/:id** [P1]
Lấy chi tiết redemption

### **POST /api/redemptions** [P1]
Đổi reward
```json
Request:
{
  "customerId": "uuid",
  "rewardId": "uuid",
  "pointsSpent": 500
}

Response:
{
  "redemptionId": "uuid",
  "voucherGenerated": "SAVE50K-123",
  "remainingPoints": 5700,
  "expiresAt": "2024-04-03"
}
```

### **PUT /api/redemptions/:id/complete** [P1]
Hoàn thành redemption (sau khi customer nhận)

---

## **Tier Upgrade APIs**

### **GET /api/loyalty/customers/:customerId/tier-history** [P2]
Lấy lịch sử thay đổi tier

---

# **7. REPORTSERVICE** (Port 5012-5013) [P2]

**Base URL:** `http://localhost:5012/api`

## **Inventory Reports**

### **GET /api/reports/inventory/summary** [P2]
Báo cáo tồn kho tổng hợp
```
Query params:
- locationId=uuid
- date=2024-03-03
```

### **GET /api/reports/inventory/low-stock** [P2]
Báo cáo hàng tồn kho thấp

### **GET /api/reports/inventory/expiring-soon** [P2]
Báo cáo hàng sắp hết hạn

### **GET /api/reports/inventory/valuation** [P2]
Báo cáo giá trị tồn kho

---

## **Sales Reports**

### **GET /api/reports/sales/summary** [P2]
Báo cáo doanh thu tổng hợp
```
Query params:
- storeId=uuid
- dateFrom=2024-03-01
- dateTo=2024-03-31
- groupBy=day|week|month
```

### **GET /api/reports/sales/by-product** [P2]
Báo cáo doanh thu theo sản phẩm

### **GET /api/reports/sales/by-category** [P2]
Báo cáo doanh thu theo category

### **GET /api/reports/sales/by-store** [P2]
Báo cáo doanh thu theo cửa hàng

---

## **Transfer Reports**

### **GET /api/reports/transfers/summary** [P2]
Báo cáo chuyển hàng

### **GET /api/reports/transfers/performance** [P2]
Báo cáo hiệu suất chuyển hàng (on-time %)

---

## **Payment Reports**

### **GET /api/reports/payments/summary** [P2]
Báo cáo thanh toán tổng hợp

### **GET /api/reports/payments/by-method** [P2]
Báo cáo thanh toán theo phương thức

---

## **Promotion Reports**

### **GET /api/reports/promotions/performance** [P2]
Báo cáo hiệu quả khuyến mãi

### **GET /api/reports/loyalty/members** [P2]
Báo cáo thành viên loyalty

---

# **📊 API SUMMARY**

## **Total APIs: 131**

### **Implementation Status:**
- ✅ **Implemented:** 20 APIs (IdentityService: 10, ProductService: 10)
- ⭐ **New (Need to implement):** 11 APIs (Notifications: 7, System Settings: 4)
- ⏳ **Pending:** 100 APIs

### **By Service:**
- IdentityService: 31 APIs (✅ 10 implemented, ⭐ 11 new)
- ProductService: 15 APIs (✅ 10 implemented)
- InventoryService: 45 APIs
- POSService: 10 APIs
- PaymentService: 15 APIs
- PromotionLoyaltyService: 25 APIs
- ReportService: 10 APIs

### **By Priority:**
- **P0 (Critical):** 60 APIs - Week 1-6
- **P1 (High):** 40 APIs - Week 7-12
- **P2 (Medium):** 20 APIs - Week 13-17
- **P3 (Low):** 5 APIs - Week 18+

---

# **🔐 AUTHENTICATION**

All APIs (except login/register) require JWT Bearer Token:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

# **📝 RESPONSE FORMAT**

## **Success Response:**
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

## **Error Response:**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_REQUEST",
    "message": "Product not found",
    "details": { ... }
  }
}
```

## **Pagination Response:**
```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 250,
    "totalPages": 13
  }
}
```

---

# **🚀 NEXT STEPS**

1. **Week 1-6:** Implement tất cả P0 APIs (60 APIs)
2. **Week 7-12:** Implement P1 APIs (40 APIs)
3. **Week 13-17:** Implement P2 APIs (20 APIs)
4. **Testing:** Integration testing cho tất cả APIs
5. **Documentation:** Swagger/OpenAPI cho mỗi service

---

**Document Version:** 1.0  
**Created:** March 3, 2026  
**Status:** Complete API Specification 🔌
