# Inventory Management Service

## Overview
Microservice quản lý kho hàng, theo dõi tồn kho, lô hàng, và xuất nhập kho.

## Technology Stack
- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core 9.0
- SQL Server
- Clean Architecture

## Project Structure
```
InventoryService/
├── src/
│   ├── InventoryService.API/          # Web API Layer
│   ├── InventoryService.Application/  # Business Logic Layer
│   ├── InventoryService.Domain/       # Domain Models
│   └── InventoryService.Infrastructure/ # Data Access Layer
├── database-schema.sql
└── README.md
```

## Database Tables
- **warehouses**: Quản lý kho hàng
- **warehouse_slots**: Vị trí lưu trữ trong kho
- **inventories**: Tồn kho theo cửa hàng và sản phẩm
- **product_batches**: Lô hàng sản phẩm
- **stock_movements**: Phiếu xuất nhập kho
- **stock_movement_items**: Chi tiết phiếu xuất nhập
- **inventory_history**: Lịch sử thay đổi tồn kho
- **inventory_logs**: Log hoạt động kho

## API Endpoints

### Warehouse Management
- `GET /api/warehouse` - Lấy danh sách tất cả kho hàng
- `GET /api/warehouse/{id}` - Lấy chi tiết 1 kho hàng

### Inventory Management
- `GET /api/inventory` - Lấy danh sách tồn kho
- `GET /api/inventory/store/{storeId}` - Lấy tồn kho theo cửa hàng
- `GET /api/inventory/product/{productId}` - Lấy tồn kho theo sản phẩm

## Configuration

### Database Connection
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=InventoryDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Port Configuration
- Development: https://localhost:5002, http://localhost:5003

## Setup Instructions

1. **Create Database**
```bash
sqlcmd -S . -i database-schema.sql
```

2. **Restore Dependencies**
```bash
dotnet restore
```

3. **Run Service**
```bash
cd src/InventoryService.API
dotnet run
```

4. **Access Swagger**
```
https://localhost:5002/swagger
```

## Integration with Other Services

### Dependencies
- **IdentityDB**: User authentication (created_by, updated_by)
- **ProductDB**: Product information (product_id)
- **OrderDB**: Store information (store_id) - Coming soon

### Cross-Service References
```
IdentityDB.users.id → created_by, updated_by
ProductDB.products.id → product_id
OrderDB.stores.id → store_id (planned)
```

## Development Notes

- Sử dụng Soft Delete cho warehouses, warehouse_slots, product_batches
- Audit trail đầy đủ với created_by, updated_by
- Index optimization cho các query thường dùng
- UNIQUE constraints đảm bảo tính toàn vẹn dữ liệu
