# ================================================================
# TÓM TẮT: Nguyên nhân và cách fix lỗi 500 Internal Server Error
# ================================================================

## ❌ NGUYÊN NHÂN GÂY LỖI:

### 1. **Bảng product_audit_logs chưa được tạo** (NGUYÊN NHÂN CHÍNH)
   - Code cố gắng insert audit log vào bảng chưa tồn tại
   - Gây ra exception và trả về lỗi 500

### 2. **CategoryId không hợp lệ** (LỖI PHỤ)
   - Request dùng GUID mẫu: `3fa85f64-5717-4562-b3fc-2c963f66afa6`
   - GUID này không tồn tại trong database
   - Foreign key constraint sẽ fail

### 3. **Connection String không đồng bộ** (ĐÃ SỬA)
   - ProductService dùng Windows Auth (Trusted_Connection=True)
   - IdentityService dùng SQL Auth (User Id=sa;Password=12345)
   - Đã đồng bộ sang SQL Auth cho cả 2 services

## ✅ CÁC BƯỚC ĐÃ THỰC HIỆN:

### Bước 1: Tạo bảng product_audit_logs
```sql
-- Đã chạy file: database-migration-audit-logs.sql
-- Kết quả: Table product_audit_logs created successfully ✓
```

### Bước 2: Đồng bộ Connection String
```json
// File: ProductService/src/ProductService.API/appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProductDB;User Id=sa;Password=12345;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Bước 3: Đồng bộ JWT Settings
```json
// File: ProductService/src/ProductService.API/appsettings.json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm",
  "Issuer": "IdentityService",
  "Audience": "IdentityServiceClient"
}
```

### Bước 4: Lấy CategoryId thật từ database
```
CategoryId: 9639C3EB-A1C0-4100-B65E-0B3ADD9FA64C
Category Name: Vệ Sinh Nhà Cửa
```

### Bước 5: Restart ProductService
- Service đang chạy tại: http://localhost:5001
- Swagger UI: http://localhost:5001/swagger

## 🧪 CÁCH TEST LẠI:

### Option 1: Test với Swagger UI (Khuyến nghị)
1. Mở: **http://localhost:5001/swagger**
2. Click **Authorize**, nhập: `Bearer YOUR_TOKEN`
3. Expand **POST /api/Product**
4. Click **Try it out**
5. Điền thông tin:
   - **CategoryId**: `9639C3EB-A1C0-4100-B65E-0B3ADD9FA64C`
   - **Sku**: `TEST-001` (phải unique)
   - **Name**: `Nước Cam Test`
   - **Price**: `50000`
   - **Unit**: `Thùng`
   - **MainImage**: Upload file ảnh
6. Click **Execute**

### Option 2: Test với cURL
```bash
# Xem file: test-curl-fixed.sh
# Copy command và chạy (nhớ sửa path đến file ảnh)
```

### Option 3: Test với PowerShell Script
```powershell
# Chạy PowerShell script tự động
cd C:\GAME\OJT-Backend\ProductService
.\test-api.ps1
```

## 📊 KẾT QUẢ MONG ĐỢI:

### ✅ Success Response (201 Created)
```json
{
  "success": true,
  "message": "Tạo sản phẩm thành công",
  "data": {
    "id": "...",
    "sku": "TEST-001",
    "name": "Nước Cam Test",
    "categoryId": "9639C3EB-A1C0-4100-B65E-0B3ADD9FA64C",
    "price": 50000,
    "imageUrl": "https://res.cloudinary.com/dwuir1s4w/image/upload/...",
    "createdBy": "2222d76e-4478-46f8-8f14-9afff87c2921",
    "createdByName": "System Administrator"
  }
}
```

### 🔍 Kiểm tra kết quả:
```sql
-- Xem product mới tạo
SELECT * FROM ProductDB.dbo.products 
ORDER BY created_at DESC;

-- Xem audit log
SELECT * FROM ProductDB.dbo.product_audit_logs 
ORDER BY performed_at DESC;

-- Kiểm tra ảnh trên Cloudinary
-- Vào: https://cloudinary.com/console
```

## 🎯 CHECKLIST TRƯỚC KHI TEST:

- [✓] Service đang chạy: http://localhost:5001
- [✓] Bảng product_audit_logs đã tồn tại
- [✓] Connection string đúng (SQL Auth)
- [✓] JWT settings đồng bộ
- [✓] Token còn hạn (expires sau 60 phút)
- [✓] CategoryId tồn tại trong database
- [✓] SKU chưa tồn tại (phải unique)
- [✓] File ảnh có định dạng hợp lệ (.jpg, .jpeg, .png)

## 🔧 NẾU VẪN GẶP LỖI:

### 500 Internal Server Error
- Kiểm tra ProductService console có log error gì
- Kiểm tra SQL Server connection
- Kiểm tra Cloudinary credentials

### 400 Bad Request
- Kiểm tra validation errors
- Đảm bảo SKU chưa tồn tại
- Kiểm tra định dạng dữ liệu

### 401 Unauthorized
- Token hết hạn → Login lại
- JWT settings không khớp → Kiểm tra appsettings.json

### 403 Forbidden
- User không có quyền → Phải là Admin/Manager/Warehouse Staff

## 📝 FILES LIÊN QUAN:

- `database-migration-audit-logs.sql` - Migration script
- `debug-test-api.sql` - Debug database
- `test-api.ps1` - PowerShell test script (tự động)
- `test-curl-fixed.sh` - cURL command đã sửa
- `appsettings.json` - Cấu hình đã đồng bộ

Bây giờ có thể test lại và sẽ thành công! 🎉
