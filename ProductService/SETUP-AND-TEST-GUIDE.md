# Hướng dẫn Setup và Test API POST /products

## 🎯 Tổng quan
Chức năng này cho phép tạo sản phẩm mới với đầy đủ thông tin và upload ảnh lên Cloudinary. Chỉ có **Admin**, **Manager**, và **Warehouse Staff** mới có quyền tạo sản phẩm.

## ✅ Các tính năng đã được implement

### 1. **JWT Authentication & Authorization**
- ✅ Tích hợp JWT Bearer Token
- ✅ Kiểm tra role: Admin, Manager, Warehouse Staff
- ✅ Tự động lấy thông tin user từ token

### 2. **Cloudinary Integration**
- ✅ Upload ảnh chính (bắt buộc)
- ✅ Upload nhiều ảnh phụ (tối đa 10 ảnh)
- ✅ Validate định dạng ảnh (.jpg, .jpeg, .png, .gif, .webp)
- ✅ Validate kích thước ảnh (max 10MB)
- ✅ Auto resize và optimize ảnh

### 3. **Validation**
- ✅ Validate tất cả field bắt buộc
- ✅ Validate range (giá, số lượng phải >= 0)
- ✅ Validate length (string length limits)
- ✅ Validate unique (SKU, Barcode)
- ✅ Validate business logic (OriginalPrice >= Price, MaxOrderQuantity >= MinOrderQuantity)

### 4. **Audit Logging**
- ✅ Ghi log người tạo (User ID, User Name, IP Address)
- ✅ Ghi log action CREATE
- ✅ Lưu thông tin product vào audit log (JSON format)

### 5. **Transaction & Rollback**
- ✅ Nếu có lỗi khi lưu DB → xóa các ảnh đã upload
- ✅ Nếu có lỗi khi upload ảnh → rollback tất cả ảnh đã upload trước đó
- ✅ Đảm bảo data consistency

### 6. **Slug Generation**
- ✅ Tự động tạo slug từ tên sản phẩm
- ✅ Hỗ trợ tiếng Việt có dấu
- ✅ URL-friendly format

## 📋 Các bước Setup

### Bước 1: Chạy Migration Script

```bash
# Chạy script để tạo bảng product_audit_logs
# Mở SQL Server Management Studio và chạy file:
ProductService/database-migration-audit-logs.sql
```

### Bước 2: Cấu hình appsettings.json

File đã được cấu hình sẵn tại `ProductService/src/ProductService.API/appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWT_MustBeAtLeast32Characters!",
    "Issuer": "IdentityService",
    "Audience": "ProductService"
  },
  "CloudinarySettings": {
    "CloudName": "dwuir1s4w",
    "ApiKey": "291637823979115",
    "ApiSecret": "PgyV3HTMWTsz194iicucLbgpsIs"
  }
}
```

⚠️ **Lưu ý**: JWT SecretKey phải giống với IdentityService để token có thể validate được.

### Bước 3: Build và Run ProductService

```bash
cd C:\GAME\OJT-Backend\ProductService\src\ProductService.API
dotnet build
dotnet run
```

Mặc định service sẽ chạy tại: `http://localhost:5000`

## 🧪 Cách Test API

### Test 1: Lấy JWT Token từ Identity Service

Trước tiên, bạn cần login để lấy JWT token:

```bash
POST http://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@company.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "admin@company.com",
      "fullName": "System Administrator",
      "role": "Admin"
    }
  }
}
```

Copy `accessToken` để dùng cho các request tiếp theo.

### Test 2: Tạo Product với Postman

1. **Method**: POST
2. **URL**: `http://localhost:5000/api/product`
3. **Headers**:
   - `Authorization`: `Bearer YOUR_ACCESS_TOKEN`
4. **Body** (form-data):

   | Key | Type | Value | Required |
   |-----|------|-------|----------|
   | sku | Text | `RAU-003` | ✅ |
   | name | Text | `Rau Cải Bó Xôi` | ✅ |
   | categoryId | Text | `{guid-của-category-rau-ăn-lá}` | ✅ |
   | price | Text | `18000` | ✅ |
   | unit | Text | `Kg` | ✅ |
   | brand | Text | `Đà Lạt` | ❌ |
   | origin | Text | `Việt Nam` | ❌ |
   | description | Text | `Rau cải bó xôi tươi sạch từ Đà Lạt` | ❌ |
   | isPerishable | Text | `true` | ❌ |
   | shelfLifeDays | Text | `3` | ❌ |
   | mainImage | File | Chọn file ảnh | ✅ |
   | additionalImages | File | Chọn file ảnh 1 | ❌ |
   | additionalImages | File | Chọn file ảnh 2 | ❌ |

5. **Click Send**

### Test 3: Tạo Product với cURL

```bash
curl -X POST "http://localhost:5000/api/product" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -F "sku=RAU-003" \
  -F "name=Rau Cải Bó Xôi" \
  -F "categoryId=a1b2c3d4-5678-90ab-cdef-123456789012" \
  -F "price=18000" \
  -F "unit=Kg" \
  -F "brand=Đà Lạt" \
  -F "origin=Việt Nam" \
  -F "description=Rau cải bó xôi tươi sạch từ Đà Lạt" \
  -F "isPerishable=true" \
  -F "shelfLifeDays=3" \
  -F "mainImage=@C:/path/to/main-image.jpg" \
  -F "additionalImages=@C:/path/to/image2.jpg" \
  -F "additionalImages=@C:/path/to/image3.jpg"
```

### Test 4: Kiểm tra kết quả

#### 4.1. Kiểm tra Response
```json
{
  "success": true,
  "message": "Tạo sản phẩm thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sku": "RAU-003",
    "name": "Rau Cải Bó Xôi",
    "imageUrl": "https://res.cloudinary.com/dwuir1s4w/image/upload/...",
    "images": [
      "https://res.cloudinary.com/dwuir1s4w/image/upload/...",
      "https://res.cloudinary.com/dwuir1s4w/image/upload/..."
    ],
    "createdBy": "123e4567-e89b-12d3-a456-426614174000",
    "createdByName": "System Administrator"
  }
}
```

#### 4.2. Kiểm tra Database
```sql
-- Kiểm tra product đã được tạo
SELECT * FROM ProductDB.dbo.products 
WHERE sku = 'RAU-003';

-- Kiểm tra audit log
SELECT * FROM ProductDB.dbo.product_audit_logs 
WHERE action = 'CREATE'
ORDER BY performed_at DESC;
```

#### 4.3. Kiểm tra Cloudinary
- Truy cập: https://cloudinary.com/console
- Login với account: dwuir1s4w
- Vào Media Library → products folder
- Kiểm tra ảnh đã upload

## 🔍 Test Cases

### Test Case 1: Tạo product thành công với đầy đủ thông tin
- ✅ Có token hợp lệ
- ✅ Role = Admin/Manager/Warehouse Staff
- ✅ Tất cả field required hợp lệ
- ✅ Upload 1 main image + 2 additional images

**Expected**: HTTP 201, product được tạo, ảnh upload lên Cloudinary, có audit log

### Test Case 2: Tạo product không có token
- ❌ Không gửi Authorization header

**Expected**: HTTP 401 Unauthorized

### Test Case 3: Tạo product với role không hợp lệ
- ✅ Token hợp lệ nhưng role = Customer/Store Staff

**Expected**: HTTP 403 Forbidden

### Test Case 4: Tạo product thiếu field bắt buộc
- ❌ Không có SKU hoặc Name hoặc MainImage

**Expected**: HTTP 400 Bad Request với thông báo lỗi cụ thể

### Test Case 5: Tạo product với SKU đã tồn tại
- ❌ SKU đã có trong database

**Expected**: HTTP 400 Bad Request: "SKU 'XXX' đã tồn tại"

### Test Case 6: Tạo product với ảnh quá lớn
- ❌ Upload ảnh > 10MB

**Expected**: HTTP 400 Bad Request: "Kích thước file không được vượt quá 10MB"

### Test Case 7: Tạo product với định dạng ảnh không hợp lệ
- ❌ Upload file .pdf, .doc, .txt

**Expected**: HTTP 400 Bad Request: "Định dạng file không được hỗ trợ"

### Test Case 8: Test rollback khi lỗi database
- ✅ Upload ảnh thành công
- ❌ Database connection bị lỗi

**Expected**: Ảnh đã upload sẽ bị xóa khỏi Cloudinary

## 📊 Monitoring & Logging

### Application Logs
Logs sẽ được ghi ra console khi chạy service:

```
[INF] User System Administrator (ID: 123e4567, Role: Admin) is creating product RAU-003
[INF] Uploading main image for product RAU-003
[INF] Uploading 2 additional images
[INF] Image uploaded successfully: https://res.cloudinary.com/...
[INF] Product 3fa85f64-5717-4562-b3fc-2c963f66afa6 - Rau Cải Bó Xôi created successfully by System Administrator
```

### Audit Logs trong Database
Mọi thao tác tạo product đều được ghi vào bảng `product_audit_logs`:

```sql
SELECT 
    pal.id,
    pal.product_id,
    pal.performed_by_name,
    pal.action,
    pal.new_values,
    pal.description,
    pal.ip_address,
    pal.performed_at,
    p.name as product_name,
    p.sku
FROM product_audit_logs pal
LEFT JOIN products p ON pal.product_id = p.id
ORDER BY pal.performed_at DESC;
```

## 🛠️ Troubleshooting

### Lỗi: JWT Token không hợp lệ
**Nguyên nhân**: JWT SecretKey không khớp giữa Identity Service và Product Service
**Giải pháp**: Kiểm tra và đồng bộ SecretKey trong appsettings.json của cả 2 services

### Lỗi: Cannot upload image to Cloudinary
**Nguyên nhân**: Cloudinary credentials không đúng
**Giải pháp**: Kiểm tra CloudinarySettings trong appsettings.json

### Lỗi: Category not found
**Nguyên nhân**: CategoryId không tồn tại trong database
**Giải pháp**: Query database để lấy CategoryId hợp lệ:
```sql
SELECT id, name FROM ProductDB.dbo.categories WHERE status = 'ACTIVE';
```

### Lỗi: Database connection failed
**Nguyên nhân**: SQL Server không chạy hoặc connection string sai
**Giải pháp**: 
1. Kiểm tra SQL Server đang chạy
2. Kiểm tra connection string trong appsettings.json

## 📚 Tài liệu bổ sung

- [API-CREATE-PRODUCT.md](./API-CREATE-PRODUCT.md) - Chi tiết API documentation
- [database-migration-audit-logs.sql](./database-migration-audit-logs.sql) - Migration script

## 🎉 Tổng kết

Chức năng POST /products đã được implement đầy đủ với:
- ✅ JWT Authentication & Authorization
- ✅ Cloudinary Integration (upload ảnh)
- ✅ Validation đầy đủ
- ✅ Audit Logging
- ✅ Transaction Rollback
- ✅ Error Handling
- ✅ Slug Generation (tiếng Việt)

Bạn có thể bắt đầu test ngay bây giờ! 🚀
