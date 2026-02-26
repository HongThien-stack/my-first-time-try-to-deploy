# Product Service - API Documentation

## POST /api/product - Tạo sản phẩm mới

### Mô tả
Endpoint này cho phép tạo sản phẩm mới với đầy đủ thông tin và hình ảnh. Chỉ có **Admin**, **Manager**, và **Warehouse Staff** mới có quyền tạo sản phẩm.

### Authorization
- **Required**: Yes
- **Type**: Bearer Token (JWT)
- **Allowed Roles**: Admin, Manager, Warehouse Staff

### Request
- **Method**: POST
- **URL**: `/api/product`
- **Content-Type**: `multipart/form-data`

### Request Body (Form Data)

#### Thông tin cơ bản (Required)
- `sku` (string, required): Mã SKU sản phẩm (tối đa 50 ký tự, phải unique)
- `name` (string, required): Tên sản phẩm (3-255 ký tự)
- `categoryId` (guid, required): ID danh mục sản phẩm
- `price` (decimal, required): Giá bán (>= 0)
- `unit` (string, required): Đơn vị tính (Kg, Gram, Lít, Chai, Hộp, Túi, Cái)

#### Thông tin bổ sung (Optional)
- `barcode` (string): Mã vạch (tối đa 50 ký tự, phải unique nếu có)
- `description` (string): Mô tả chi tiết sản phẩm
- `brand` (string): Thương hiệu (tối đa 100 ký tự)
- `origin` (string): Xuất xứ (tối đa 100 ký tự)
- `originalPrice` (decimal): Giá gốc trước khuyến mãi (>= 0)
- `costPrice` (decimal): Giá vốn (>= 0)

#### Đơn vị và khối lượng (Optional)
- `weight` (decimal): Khối lượng tính bằng kg (>= 0)
- `volume` (decimal): Thể tích tính bằng lít (>= 0)
- `quantityPerUnit` (int): Số lượng mỗi đơn vị (>= 1, default: 1)
- `minOrderQuantity` (int): Số lượng đặt tối thiểu (>= 1, default: 1)
- `maxOrderQuantity` (int): Số lượng đặt tối đa (>= 1)

#### Hạn sử dụng và bảo quản (Optional)
- `expirationDate` (datetime): Ngày hết hạn
- `shelfLifeDays` (int): Hạn sử dụng (số ngày, >= 1)
- `storageInstructions` (string): Hướng dẫn bảo quản (tối đa 500 ký tự)
- `isPerishable` (bool): Có phải thực phẩm tươi sống (default: false)

#### Trạng thái (Optional)
- `isAvailable` (bool): Có sẵn để bán (default: true)
- `isFeatured` (bool): Sản phẩm nổi bật (default: false)
- `isNew` (bool): Sản phẩm mới (default: false)
- `isOnSale` (bool): Đang giảm giá (default: false)

#### SEO (Optional)
- `slug` (string): URL-friendly name (tối đa 255 ký tự, auto-generate nếu không cung cấp)
- `metaTitle` (string): SEO title (tối đa 255 ký tự)
- `metaDescription` (string): SEO description (tối đa 500 ký tự)
- `metaKeywords` (string): SEO keywords (tối đa 500 ký tự)

#### Hình ảnh (Required và Optional)
- `mainImage` (file, **required**): Ảnh chính của sản phẩm
  - Định dạng: .jpg, .jpeg, .png, .gif, .webp
  - Kích thước tối đa: 10MB
  
- `additionalImages` (file[], optional): Ảnh phụ (tối đa 10 ảnh)
  - Định dạng: .jpg, .jpeg, .png, .gif, .webp
  - Kích thước mỗi ảnh: 10MB

### Response

#### Success Response (201 Created)
```json
{
  "success": true,
  "message": "Tạo sản phẩm thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sku": "RAU-003",
    "name": "Rau Cải Bó Xôi",
    "barcode": "8934560003333",
    "categoryId": "a1b2c3d4-5678-90ab-cdef-123456789012",
    "categoryName": "Rau Ăn Lá",
    "price": 18000,
    "unit": "Kg",
    "brand": "Đà Lạt",
    "origin": "Việt Nam",
    "imageUrl": "https://res.cloudinary.com/dwuir1s4w/image/upload/v1234567890/products/rau-cai-bo-xoi.jpg",
    "images": [
      "https://res.cloudinary.com/dwuir1s4w/image/upload/v1234567890/products/rau-cai-bo-xoi-2.jpg",
      "https://res.cloudinary.com/dwuir1s4w/image/upload/v1234567890/products/rau-cai-bo-xoi-3.jpg"
    ],
    "slug": "rau-cai-bo-xoi",
    "createdAt": "2026-02-26T10:30:00Z",
    "createdBy": "123e4567-e89b-12d3-a456-426614174000",
    "createdByName": "Nguyễn Văn Admin"
  }
}
```

#### Error Responses

**400 Bad Request - Validation Error**
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "errors": [
    "SKU là bắt buộc",
    "Tên sản phẩm phải từ 3 đến 255 ký tự",
    "Ảnh chính là bắt buộc"
  ]
}
```

**400 Bad Request - Business Logic Error**
```json
{
  "success": false,
  "message": "SKU 'RAU-001' đã tồn tại"
}
```

**401 Unauthorized**
```json
{
  "success": false,
  "message": "User ID không hợp lệ trong token"
}
```

**403 Forbidden**
```json
{
  "success": false,
  "message": "You do not have permission to access this resource"
}
```

**500 Internal Server Error**
```json
{
  "success": false,
  "message": "Đã xảy ra lỗi khi tạo sản phẩm. Vui lòng thử lại sau."
}
```

### Features

#### 1. Authentication & Authorization
- Yêu cầu JWT token hợp lệ
- Chỉ Admin, Manager, Warehouse Staff có quyền
- Tự động lấy thông tin user từ token

#### 2. Validation
- Validate tất cả input (required, length, format, range)
- Kiểm tra SKU và Barcode unique
- Validate giá và số lượng hợp lý
- Validate định dạng và kích thước ảnh

#### 3. Cloudinary Integration
- Upload ảnh chính và ảnh phụ lên Cloudinary
- Tự động resize và optimize ảnh
- Hỗ trợ nhiều định dạng ảnh

#### 4. Transaction & Rollback
- Nếu có lỗi, tự động xóa các ảnh đã upload
- Không lưu dữ liệu không đầy đủ vào database

#### 5. Audit Logging
- Ghi lại thông tin người tạo (User ID, Name, IP)
- Lưu action CREATE vào product_audit_logs
- Ghi log chi tiết về sản phẩm được tạo

#### 6. Slug Generation
- Tự động tạo slug từ tên sản phẩm
- Hỗ trợ tiếng Việt có dấu
- URL-friendly format

### Example Request (cURL)

```bash
curl -X POST "http://localhost:5000/api/product" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: multipart/form-data" \
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
  -F "mainImage=@/path/to/main-image.jpg" \
  -F "additionalImages=@/path/to/image2.jpg" \
  -F "additionalImages=@/path/to/image3.jpg"
```

### Example Request (Postman)

1. Chọn method: **POST**
2. URL: `http://localhost:5000/api/product`
3. Headers:
   - Authorization: `Bearer YOUR_JWT_TOKEN`
4. Body → form-data:
   - Thêm các field text và file như mô tả ở trên

### Testing Steps

1. **Lấy JWT Token** từ Identity Service:
   ```bash
   POST http://localhost:5001/api/auth/login
   {
     "email": "admin@company.com",
     "password": "Password123!"
   }
   ```

2. **Tạo sản phẩm** với token nhận được

3. **Kiểm tra kết quả**:
   - Xem product được tạo trong database
   - Kiểm tra ảnh có upload lên Cloudinary
   - Xem audit log trong bảng product_audit_logs

### Database Schema

Sau khi tạo product thành công, dữ liệu sẽ được lưu vào:

1. **products table**: Thông tin sản phẩm đầy đủ
2. **product_audit_logs table**: Log hành động tạo sản phẩm

### Notes

- Đảm bảo đã chạy migration script để tạo bảng `product_audit_logs`
- Cloudinary credentials phải được cấu hình đúng trong appsettings.json
- JWT secret key phải khớp giữa Identity Service và Product Service
- Upload ảnh có thể mất vài giây tùy kích thước và tốc độ mạng
