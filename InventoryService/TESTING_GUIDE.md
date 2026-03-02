# InventoryService API Testing Guide

## Service Ports
- **IdentityService**: https://localhost:5000 (HTTP: 5001)
- **ProductService**: https://localhost:5001 (HTTP: 5001) 
- **InventoryService**: https://localhost:5002 (HTTP: 5003)

## Quick Start

### 1. Start IdentityService
```bash
cd "c:\Users\win\Downloads\fsa clean\IdentityService\src\IdentityService.API"
dotnet run
```
Access: https://localhost:5000/swagger

### 2. Start ProductService
```bash
cd "c:\Users\win\Downloads\fsa clean\ProductService\src\ProductService.API"
dotnet run
```
Access: https://localhost:5001/swagger

### 3. Start InventoryService
```bash
cd "c:\Users\win\Downloads\fsa clean\InventoryService\src\InventoryService.API"
dotnet run
```
Access: https://localhost:5002/swagger

---

## API Endpoints Testing

### InventoryService Endpoints

#### 1. Get All Warehouses
```http
GET https://localhost:5002/api/warehouse
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Warehouses retrieved successfully",
  "data": [
    {
      "id": "guid",
      "name": "Central Warehouse",
      "location": "District 1, HCMC",
      "isDeleted": false,
      "createdAt": "2026-03-02T...",
      "updatedAt": null
    },
    {
      "id": "guid",
      "name": "North Warehouse",
      "location": "Cau Giay, Hanoi",
      "isDeleted": false,
      "createdAt": "2026-03-02T...",
      "updatedAt": null
    }
  ]
}
```

#### 2. Get Warehouse by ID
```http
GET https://localhost:5002/api/warehouse/{id}
```

#### 3. Get All Inventories
```http
GET https://localhost:5002/api/inventory
```

**Expected Response:**
```json
{
  "success": true,
  "message": "Inventories retrieved successfully",
  "data": []
}
```

#### 4. Get Inventory by ID
```http
GET https://localhost:5002/api/inventory/{id}
```

#### 5. Get Inventories by Store
```http
GET https://localhost:5002/api/inventory/store/{storeId}
```

#### 6. Get Inventories by Product
```http
GET https://localhost:5002/api/inventory/product/{productId}
```

---

## Cross-Service Integration Testing

### Scenario 1: Check Product Inventory

**Step 1:** Get product from ProductService
```http
GET https://localhost:5001/api/product
```

**Step 2:** Use product ID to check inventory
```http
GET https://localhost:5002/api/inventory/product/{productId}
```

### Scenario 2: Warehouse Staff Login and Check Warehouses

**Step 1:** Login as warehouse staff via IdentityService
```http
POST https://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "warehouse1@company.com",
  "password": "YourPassword"
}
```

**Step 2:** Get all warehouses
```http
GET https://localhost:5002/api/warehouse
Authorization: Bearer {token}
```

---

## Testing with PowerShell

### Test Warehouse API
```powershell
# Get all warehouses
Invoke-RestMethod -Uri "https://localhost:5002/api/warehouse" -Method Get -SkipCertificateCheck

# Get specific warehouse (replace with actual ID)
$warehouseId = "your-warehouse-guid"
Invoke-RestMethod -Uri "https://localhost:5002/api/warehouse/$warehouseId" -Method Get -SkipCertificateCheck
```

### Test Inventory API
```powershell
# Get all inventories
Invoke-RestMethod -Uri "https://localhost:5002/api/inventory" -Method Get -SkipCertificateCheck

# Get inventories by product
$productId = "your-product-guid"
Invoke-RestMethod -Uri "https://localhost:5002/api/inventory/product/$productId" -Method Get -SkipCertificateCheck
```

---

## Testing with cURL

### Test Warehouse API
```bash
# Get all warehouses
curl -k https://localhost:5002/api/warehouse

# Get specific warehouse
curl -k https://localhost:5002/api/warehouse/{id}
```

### Test Inventory API
```bash
# Get all inventories
curl -k https://localhost:5002/api/inventory

# Get inventories by store
curl -k https://localhost:5002/api/inventory/store/{storeId}

# Get inventories by product
curl -k https://localhost:5002/api/inventory/product/{productId}
```

---

## Database Sample Data

### Warehouses (Already Created)
- Central Warehouse (District 1, HCMC)
- North Warehouse (Cau Giay, Hanoi)

### To Add Sample Inventory Data
Run this SQL to create test inventory records:

```sql
USE InventoryDB;
GO

-- Get warehouse and product IDs (replace with actual IDs from your databases)
DECLARE @warehouseId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM warehouses);
DECLARE @storeId UNIQUEIDENTIFIER = NEWID(); -- Temporary, will use OrderDB later
DECLARE @productId UNIQUEIDENTIFIER = NEWID(); -- Replace with actual ProductDB product ID

-- Insert sample inventory
INSERT INTO inventories (id, store_id, product_id, quantity, alert_threshold)
VALUES 
(NEWID(), @storeId, @productId, 100, 20);

SELECT * FROM inventories;
```

---

## Troubleshooting

### Service Not Starting
1. Check if ports are already in use:
   ```powershell
   netstat -ano | findstr "5002"
   netstat -ano | findstr "5003"
   ```

2. Kill process if needed:
   ```powershell
   taskkill /PID <process_id> /F
   ```

### Database Connection Issues
1. Verify SQL Server is running
2. Check connection string in `appsettings.json`
3. Ensure InventoryDB exists:
   ```sql
   SELECT name FROM sys.databases WHERE name = 'InventoryDB';
   ```

### HTTPS Certificate Issues
If you encounter certificate errors:
```bash
dotnet dev-certs https --trust
```

---

## Data Flow Between Services

```
User Request
    ↓
IdentityService (Port 5000)
    → Authenticate user
    → Return JWT token
    ↓
ProductService (Port 5001)
    → Get product information
    → Return product details (id, name, sku, etc.)
    ↓
InventoryService (Port 5002)
    → Query inventory by product_id
    → Check stock levels across warehouses
    → Return inventory data
```

---

## Next Steps

1. ✅ All 3 services are running independently
2. ✅ Each service has its own database
3. ⏳ Add JWT authentication to InventoryService endpoints
4. ⏳ Create OrderDB and integrate with InventoryService
5. ⏳ Implement stock movement tracking
6. ⏳ Add real-time inventory updates

---

## Sample Test Workflow

1. **Start all services** (3 terminal windows)
2. **Login** to get JWT token
3. **Get products** from ProductService
4. **Check inventory** for those products
5. **View warehouses** to see storage locations
6. **Monitor stock levels** and low-stock alerts

All services are now ready for integration testing! 🚀
