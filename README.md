# Microservices Architecture - Bách Hóa Xanh System

## 🎯 Tổng quan hệ thống

Hệ thống quản lý siêu thị Bách Hóa Xanh với kiến trúc microservices, bao gồm 3 services chính:

| Service | Port | Database | Purpose |
|---------|------|----------|---------|
| **IdentityService** | 5000/5001 | IdentityDB | Xác thực, phân quyền, quản lý user |
| **ProductService** | 5001/5001 | ProductDB | Quản lý sản phẩm, danh mục |
| **InventoryService** | 5002/5003 | InventoryDB | Quản lý kho, tồn kho, xuất nhập |

---

## 🏗️ Kiến trúc hệ thống

```
┌─────────────────────┐
│   Client/Frontend   │
└──────────┬──────────┘
           │
    ┌──────┴──────┐
    │   API Gateway  │
    │  (Future)      │
    └──────┬──────┘
           │
    ┌──────┴────────────────────┐
    │                           │
┌───▼────────┐  ┌──────▼──────┐  ┌───────▼──────┐
│  Identity  │  │   Product   │  │  Inventory   │
│  Service   │  │   Service   │  │   Service    │
│  Port 5000 │  │  Port 5001  │  │  Port 5002   │
└─────┬──────┘  └──────┬──────┘  └──────┬───────┘
      │                │                 │
┌─────▼─────┐  ┌──────▼──────┐  ┌──────▼──────┐
│ IdentityDB│  │  ProductDB  │  │ InventoryDB │
└───────────┘  └─────────────┘  └─────────────┘
```

---

## 📦 Cấu trúc thư mục

```
fsa clean/
├── IdentityService/           # Service 1: Authentication & Authorization
│   ├── src/
│   │   ├── IdentityService.API/
│   │   ├── IdentityService.Application/
│   │   ├── IdentityService.Domain/
│   │   └── IdentityService.Infrastructure/
│   ├── database-schema.sql
│   └── README.md
│
├── ProductService/            # Service 2: Product Management
│   ├── src/
│   │   ├── ProductService.API/
│   │   ├── ProductService.Application/
│   │   ├── ProductService.Domain/
│   │   └── ProductService.Infrastructure/
│   ├── database-schema.sql
│   └── README.md
│
├── InventoryService/          # Service 3: Inventory & Warehouse Management
│   ├── src/
│   │   ├── InventoryService.API/
│   │   ├── InventoryService.Application/
│   │   ├── InventoryService.Domain/
│   │   └── InventoryService.Infrastructure/
│   ├── database-schema.sql
│   ├── README.md
│   └── TESTING_GUIDE.md
│
└── test-all-services.ps1      # Integration test script
```

---

## 🚀 Quick Start Guide

### Bước 1: Cài đặt Dependencies

```bash
# .NET 9 SDK
# SQL Server
# Visual Studio 2022 hoặc VS Code
```

### Bước 2: Tạo Databases

```powershell
# Tạo IdentityDB
sqlcmd -S . -i "IdentityService\database-schema.sql"

# Tạo ProductDB
sqlcmd -S . -i "ProductService\database-schema.sql"

# Tạo InventoryDB
sqlcmd -S . -i "InventoryService\database-schema.sql"
```

### Bước 3: Chạy các Services

**Terminal 1 - IdentityService:**
```powershell
cd "IdentityService\src\IdentityService.API"
dotnet run
```
→ https://localhost:5000/swagger

**Terminal 2 - ProductService:**
```powershell
cd "ProductService\src\ProductService.API"
dotnet run
```
→ https://localhost:5001/swagger

**Terminal 3 - InventoryService:**
```powershell
cd "InventoryService\src\InventoryService.API"
dotnet run
```
→ https://localhost:5002/swagger

### Bước 4: Test Integration

```powershell
.\test-all-services.ps1
```

---

## 📊 Database Schema Overview

### IdentityDB (Authentication & Users)
- **users**: Thông tin người dùng, password hash, refresh token, OTP
- **roles**: Vai trò hệ thống (Admin, Manager, Staff, Customer)
- **user_login_logs**: Lịch sử đăng nhập
- **user_audit_logs**: Audit trail cho user management

### ProductDB (Product Catalog)
- **categories**: Danh mục sản phẩm (26 categories)
- **products**: Thông tin sản phẩm (SKU, barcode, price, inventory)

### InventoryDB (Warehouse & Stock)
- **warehouses**: Kho hàng
- **warehouse_slots**: Vị trí trong kho
- **inventories**: Tồn kho theo cửa hàng/sản phẩm
- **product_batches**: Lô hàng (batch code, expiry date)
- **stock_movements**: Phiếu xuất nhập kho
- **stock_movement_items**: Chi tiết phiếu xuất nhập
- **inventory_history**: Lịch sử thay đổi
- **inventory_logs**: Log hoạt động

---

## 🔗 Cross-Service References

```
IdentityDB.users.id
    ↓
    ├─> ProductDB.products.created_by
    ├─> ProductDB.products.updated_by
    ├─> InventoryDB.warehouses.created_by
    ├─> InventoryDB.warehouses.updated_by
    └─> InventoryDB.stock_movements.created_by

ProductDB.products.id
    ↓
    ├─> InventoryDB.inventories.product_id
    ├─> InventoryDB.product_batches.product_id
    └─> InventoryDB.inventory_logs.product_id

OrderDB.stores.id (Coming Soon)
    ↓
    ├─> InventoryDB.inventories.store_id
    └─> InventoryDB.stock_movements.store_id
```

---

## 🧪 API Testing Examples

### 1. Authentication Flow

```http
POST https://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "admin@company.com",
  "password": "YourPassword"
}
```

### 2. Get Products

```http
GET https://localhost:5001/api/product
```

### 3. Get Warehouses

```http
GET https://localhost:5002/api/warehouse
```

### 4. Get Inventory by Product

```http
GET https://localhost:5002/api/inventory/product/{productId}
```

### 5. Cross-Service Integration

```powershell
# Step 1: Login and get token
$login = Invoke-RestMethod -Uri "https://localhost:5000/api/auth/login" `
  -Method Post -ContentType "application/json" `
  -Body '{"email":"warehouse1@company.com","password":"Warehouse@123"}' `
  -SkipCertificateCheck

$token = $login.data.token

# Step 2: Get products
$products = Invoke-RestMethod -Uri "https://localhost:5001/api/product" `
  -Method Get -SkipCertificateCheck

# Step 3: Check inventory for first product
$productId = $products.data[0].id
$inventory = Invoke-RestMethod -Uri "https://localhost:5002/api/inventory/product/$productId" `
  -Method Get -SkipCertificateCheck
```

---

## 📱 API Endpoints Summary

### IdentityService (Port 5000)
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - Đăng xuất

### ProductService (Port 5001)
- `GET /api/product` - Lấy tất cả sản phẩm
- `GET /api/product/{id}` - Lấy sản phẩm theo ID
- `GET /api/category` - Lấy tất cả danh mục

### InventoryService (Port 5002)
- `GET /api/warehouse` - Lấy tất cả kho hàng
- `GET /api/warehouse/{id}` - Lấy kho hàng theo ID
- `GET /api/inventory` - Lấy tất cả tồn kho
- `GET /api/inventory/{id}` - Lấy tồn kho theo ID
- `GET /api/inventory/store/{storeId}` - Lấy tồn kho theo cửa hàng
- `GET /api/inventory/product/{productId}` - Lấy tồn kho theo sản phẩm

---

## 🔐 Sample Users

| Email | Password | Role | Purpose |
|-------|----------|------|---------|
| admin@company.com | Admin@123 | Admin | System admin |
| manager1@company.com | Manager@123 | Manager | Store manager |
| warehouse1@company.com | Warehouse@123 | Warehouse Staff | Warehouse operations |
| customer1@gmail.com | Customer@123 | Customer | Online shopping |

*Note: After database creation, run `POST /api/utility/update-passwords` to properly hash passwords*

---

## 🛠️ Development Guidelines

### Clean Architecture
Mỗi service tuân theo Clean Architecture với 4 layers:
1. **Domain** - Entities, business logic
2. **Application** - DTOs, Interfaces, Services
3. **Infrastructure** - DbContext, Repositories
4. **API** - Controllers, Configuration

### Coding Standards
- ✅ Use async/await for all I/O operations
- ✅ Implement proper error handling
- ✅ Log important actions and errors
- ✅ Follow RESTful API conventions
- ✅ Use dependency injection
- ✅ Implement soft delete where appropriate

### Database Conventions
- ✅ Use UNIQUEIDENTIFIER for primary keys
- ✅ Snake_case for column names
- ✅ Include audit fields (created_at, updated_at, created_by, updated_by)
- ✅ Add indexes for foreign keys and frequently queried fields
- ✅ Use NVARCHAR for Unicode support

---

## 📝 TODO / Roadmap

### Phase 1: Core Services ✅ COMPLETED
- [x] IdentityService - Authentication & Authorization
- [x] ProductService - Product Catalog Management
- [x] InventoryService - Warehouse & Stock Management

### Phase 2: Integration Features 🚧 IN PROGRESS
- [ ] JWT Authentication middleware for all services
- [ ] API Gateway implementation
- [ ] Service-to-service communication
- [ ] Distributed logging (Serilog + ELK Stack)
- [ ] Health checks and monitoring

### Phase 3: Advanced Features 📋 PLANNED
- [ ] OrderService - Sales & Orders
- [ ] ShiftService - Staff shift management
- [ ] ReportingService - Analytics & Reports
- [ ] NotificationService - Email/SMS notifications
- [ ] Redis caching layer
- [ ] RabbitMQ message queue
- [ ] Docker containerization
- [ ] Kubernetes orchestration

### Phase 4: Frontend 🎨 PLANNED
- [ ] Admin dashboard (React/Next.js)
- [ ] Staff POS application
- [ ] Customer mobile app
- [ ] Warehouse management UI

---

## 🐛 Troubleshooting

### Port Already in Use
```powershell
# Check which process is using the port
netstat -ano | findstr "5000"

# Kill the process
taskkill /PID <process_id> /F
```

### Database Connection Failed
1. Verify SQL Server is running
2. Check connection strings in `appsettings.json`
3. Ensure databases exist:
   ```sql
   SELECT name FROM sys.databases 
   WHERE name IN ('IdentityDB', 'ProductDB', 'InventoryDB');
   ```

### Build Errors
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### HTTPS Certificate Issues
```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

---

## 📚 Documentation

- [IdentityService README](IdentityService/README.md)
- [ProductService README](ProductService/README.md)
- [InventoryService README](InventoryService/README.md)
- [InventoryService Testing Guide](InventoryService/TESTING_GUIDE.md)

---

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

---

## 📄 License

This project is private and proprietary.

---

## 👥 Team

- **Backend Development**: .NET 9 + EF Core 9
- **Database**: SQL Server
- **Architecture**: Clean Architecture + Microservices
- **API Documentation**: Swagger/OpenAPI

---

## 📞 Support

For issues or questions, please contact the development team.

---

**Built with ❤️ using .NET 9 and Clean Architecture**
