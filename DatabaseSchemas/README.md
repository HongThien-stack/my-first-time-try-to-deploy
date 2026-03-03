# 🗄️ DATABASE SCHEMAS - COMPLETE DOCUMENTATION

## 📋 **OVERVIEW**

Hệ thống Internal SCM bao gồm **6 microservices** với **6 databases** riêng biệt, tổng cộng **45 tables**.

---

## **📂 FILE STRUCTURE**

```
DatabaseSchemas/
├── 1_IdentityDB.sql          (4 tables)  - User Management
├── 2_ProductDB.sql            (2 tables)  - Product Catalog
├── 3_InventoryDB.sql          (16 tables) - Warehouse & Inventory
├── 4_POSDB.sql                (3 tables)  - Point of Sale
├── 5_PaymentDB.sql            (5 tables)  - Payment Gateway
├── 6_PromotionLoyaltyDB.sql   (12 tables) - Promotions & Loyalty
└── README.md                  (this file)
```

---

## **🗃️ DATABASE DETAILS**

### **1. IdentityDB** (7 tables)

**Purpose:** Authentication, Authorization, User Management, Notifications, System Configuration

| Table | Purpose | Records |
|-------|---------|---------|
| `roles` | User roles (Admin, Manager, Staff, etc.) | 5 roles |
| `users` | User accounts with authentication | 16 users |
| `user_login_logs` | Login history tracking | Auto-logged |
| `user_audit_logs` | User management audit trail | Auto-logged |
| `user_locations` | **NEW** Staff assignment to warehouses/stores | 7 assignments |
| `notifications` | **NEW** System notifications and alerts | 3 samples |
| `system_settings` | **NEW** Application configuration | 16 settings |

**Key Users (Fixed UUIDs):**
- Admin: `11111111-1111-1111-1111-111111111111` (admin@company.com)
- Manager 1: `22222222-2222-2222-2222-222222222221` (manager1@company.com) → Kho HCM
- Manager 2: `22222222-2222-2222-2222-222222222222` (manager2@company.com) → Cửa Hàng Thủ Đức
- Cashier 1: `33333333-3333-3333-3333-333333333331` (cashier1@company.com) → Cửa Hàng Thủ Đức
- Warehouse Staff 1: `44444444-4444-4444-4444-444444444441` (warehouse1@company.com) → Kho HCM
- Customer 1: `55555555-5555-5555-5555-555555555551` (customer1@gmail.com)

**Default Password:** `Password123!`  
⚠️ After running SQL, call: `POST /api/utility/update-passwords` to hash properly.

---

### **2. ProductDB** (2 tables)

**Purpose:** Product Catalog, Categories Management

| Table | Purpose | Records |
|-------|---------|---------|
| `categories` | Product categories | 26 categories |
| `products` | Product information | 8 sample products |

**Sample Products (Fixed UUIDs):**
- `P0000001-0001-0001-0001-000000000001`: Rau Muống (RAU-001)
- `P0000001-0001-0001-0001-000000000005`: Sữa Vinamilk (SUA-001)
- `P0000001-0001-0001-0001-000000000007`: Gạo ST25 (GAO-001)

---

### **3. InventoryDB** (16 tables)

**Purpose:** Warehouse Management, Stock Control, Transfers, Restock Requests

**Core Tables (8):**
1. `warehouses` - Warehouse locations
2. `warehouse_slots` - Storage positions (A-01-01)
3. `inventories` - Current stock levels
4. `product_batches` - Product batches with expiry dates
5. `stock_movements` - Stock in/out movements
6. `stock_movement_items` - Movement line items
7. `inventory_history` - Historical snapshots
8. `inventory_logs` - Audit trail

**Transfer Tables (2):**
9. `transfers` - Warehouse → Store transfers
10. `transfer_items` - Transfer line items

**Restock Tables (2):**
11. `restock_requests` - Store → Warehouse requests
12. `restock_request_items` - Request line items

**Quality Control Tables (4):**
13. `damage_reports` - Damaged goods reporting
14. `inventory_checks` - Physical inventory checks
15. `inventory_check_items` - Check line items
16. `store_receiving_logs` - Store receiving records

**Sample Data:**
- 4 Locations (2 warehouses, 2 stores)
- 5 Warehouse slots
- 9 Inventory records
- 3 Product batches
- 1 Transfer with 2 items
- 2 Restock requests

---

### **4. POSDB** (3 tables)

**Purpose:** Point of Sale, Sales Transactions

| Table | Purpose | Records |
|-------|---------|---------|
| `sales` | Sale transactions | 5 sales |
| `sale_items` | Sale line items | 9 items |
| `payments` | Payment records (optional) | 5 payments |

**Sample Sales:**
- SALE-2024-001: Cash (76,000 VND)
- SALE-2024-002: VNPay with 10% discount (378,000 VND)
- SALE-2024-003: Momo (195,000 VND)
- SALE-2024-004: Cash with voucher (135,000 VND)
- SALE-2024-005: VNPay pending (280,000 VND)

**Total Sales Value:** 1,064,000 VND

---

### **5. PaymentDB** (5 tables)

**Purpose:** VNPay & Momo Integration, Payment Processing

| Table | Purpose | Records |
|-------|---------|---------|
| `payment_methods` | Available payment methods | 4 methods |
| `payment_transactions` | Gateway transactions | 4 transactions |
| `payment_callbacks` | IPN/webhook logs | 2 callbacks |
| `payment_refunds` | Refund processing | 1 refund |
| `payment_reconciliation` | Daily reconciliation | 2 records |

**Payment Methods:**
- CASH (offline)
- CARD (offline)
- VNPAY (online, QR code)
- MOMO (online, QR code)

**Sample Transactions:**
- PAY-2024-001: VNPay completed (378k)
- PAY-2024-002: Momo completed (195k)
- PAY-2024-003: VNPay pending (280k)
- PAY-2024-004: VNPay failed (150k)

---

### **6. PromotionLoyaltyDB** (12 tables)

**Purpose:** Promotions, Vouchers, Loyalty Program, Rewards

**Promotion Module (6 tables):**
1. `promotions` - Promotion campaigns
2. `promotion_rules` - Promotion conditions
3. `vouchers` - Individual voucher codes
4. `promotion_usages` - Usage tracking
5. `sale_promotions` - Sale-promotion links

**Loyalty Module (6 tables):**
6. `membership_tiers` - Loyalty tiers (Bronze/Silver/Gold/Platinum)
7. `customer_loyalty` - Customer loyalty accounts
8. `points_transactions` - Points earning/spending history
9. `rewards_catalog` - Available rewards
10. `reward_redemptions` - Reward redemption records
11. `tier_upgrades` - Tier change history

**Sample Data:**
- 4 Membership tiers
- 3 Promotions (FLASH10, SAVE50K, NEWCUST20)
- 4 Vouchers
- 4 Customer loyalty accounts
- 3 Rewards in catalog

**Loyalty Tiers:**
- **Bronze** (Level 1): 0% discount, 1.0x points
- **Silver** (Level 2): 3% discount, 1.2x points (1,000+ points)
- **Gold** (Level 3): 5% discount, 1.5x points (5,000+ points)
- **Platinum** (Level 4): 10% discount, 2.0x points (15,000+ points)

---

## **🔗 DATABASE RELATIONSHIPS**

### **Cross-Database Foreign Keys**

All cross-service references use `UNIQUEIDENTIFIER` (GUID) format.

#### **IdentityDB → Other Services**
```
users.id (UNIQUEIDENTIFIER) referenced by:
  - ProductDB.products.created_by
  - ProductDB.products.updated_by
  - InventoryDB.warehouses.created_by
  - InventoryDB.stock_movements.received_by
  - InventoryDB.transfers.shipped_by
  - InventoryDB.transfers.received_by
  - InventoryDB.restock_requests.requested_by
  - InventoryDB.restock_requests.approved_by
  - InventoryDB.damage_reports.reported_by
  - InventoryDB.inventory_checks.checked_by
  - InventoryDB.inventory_logs.performed_by
  - POSDB.sales.cashier_id
  - POSDB.sales.customer_id
  - PaymentDB.payment_transactions.customer_id
  - PaymentDB.payment_refunds.requested_by
  - PaymentDB.payment_refunds.approved_by
  - PromotionLoyaltyDB.promotions.created_by
  - PromotionLoyaltyDB.vouchers.customer_id
  - PromotionLoyaltyDB.customer_loyalty.customer_id
```

#### **ProductDB → Other Services**
```
products.id (UNIQUEIDENTIFIER) referenced by:
  - InventoryDB.inventories.product_id
  - InventoryDB.product_batches.product_id
  - InventoryDB.stock_movement_items.product_id
  - InventoryDB.transfer_items.product_id
  - InventoryDB.restock_request_items.product_id
  - InventoryDB.inventory_check_items.product_id
  - POSDB.sale_items.product_id
  - PromotionLoyaltyDB.rewards_catalog.product_id
```

#### **InventoryDB → Other Services**
```
warehouses.id (UNIQUEIDENTIFIER) referenced by:
  - POSDB.sales.store_id (for STORE type locations)
  - InventoryDB.transfers.from_location_id
  - InventoryDB.transfers.to_location_id
```

#### **POSDB → Other Services**
```
sales.id (UNIQUEIDENTIFIER) referenced by:
  - PaymentDB.payment_transactions.sale_id
  - PromotionLoyaltyDB.promotion_usages.sale_id
  - PromotionLoyaltyDB.points_transactions.sale_id
  - PromotionLoyaltyDB.sale_promotions.sale_id
```

#### **PaymentDB → POSDB**
```
payment_transactions.id (UNIQUEIDENTIFIER) referenced by:
  - POSDB.sales.payment_transaction_id
```

#### **PromotionLoyaltyDB → POSDB**
```
promotions.id (UNIQUEIDENTIFIER) referenced by:
  - POSDB.sales.promotion_id
  
vouchers.code (NVARCHAR) referenced by:
  - POSDB.sales.voucher_code (soft reference)
```

---

## **📊 TABLE COUNT SUMMARY**

| Database | Tables | Sample Records | Purpose |
|----------|--------|----------------|---------|
| **IdentityDB** | 7 | 16 users, 7 location assignments, 3 notifications, 16 settings | User Management + Permissions + Notifications |
| **ProductDB** | 2 | 8 products | Product Catalog |
| **InventoryDB** | 16 | 30+ records | Warehouse Management |
| **POSDB** | 3 | 5 sales | Point of Sale |
| **PaymentDB** | 5 | 4 transactions | Payment Gateway |
| **PromotionLoyaltyDB** | 12 | 20+ records | Promotions & Loyalty |
| **TOTAL** | **45** | **110+ records** | Complete System |

---

## **🚀 INSTALLATION INSTRUCTIONS**

### **Step 1: Run SQL Scripts in Order**

Execute in SQL Server Management Studio (SSMS) or Azure Data Studio:

```bash
1. Run: 1_IdentityDB.sql
2. Run: 2_ProductDB.sql
3. Run: 3_InventoryDB.sql
4. Run: 4_POSDB.sql
5. Run: 5_PaymentDB.sql
6. Run: 6_PromotionLoyaltyDB.sql
```

### **Step 2: Hash Passwords**

After running `1_IdentityDB.sql`, call API to hash passwords:

```bash
POST http://localhost:5000/api/utility/update-passwords
```

This will update all user passwords from temporary hash to proper BCrypt hash.

### **Step 3: Verify Installation**

Check each database:

```sql
-- Check IdentityDB
USE IdentityDB;
SELECT COUNT(*) FROM users; -- Should be 16

-- Check ProductDB
USE ProductDB;
SELECT COUNT(*) FROM products; -- Should be 8

-- Check InventoryDB
USE InventoryDB;
SELECT COUNT(*) FROM warehouses; -- Should be 4

-- Check POSDB
USE POSDB;
SELECT COUNT(*) FROM sales; -- Should be 5

-- Check PaymentDB
USE PaymentDB;
SELECT COUNT(*) FROM payment_transactions; -- Should be 4

-- Check PromotionLoyaltyDB
USE PromotionLoyaltyDB;
SELECT COUNT(*) FROM membership_tiers; -- Should be 4
```

---

## **🔑 IMPORTANT NOTES**

### **1. Fixed UUIDs for Cross-Reference**

Sample data uses **fixed UUIDs** to ensure consistency across databases:

- **Users:** `11111111-xxxx-xxxx-xxxx-xxxxxxxxxxxx` pattern
- **Products:** `P0000001-xxxx-xxxx-xxxx-xxxxxxxxxxxx` pattern
- **Categories:** `C0000001-xxxx-xxxx-xxxx-xxxxxxxxxxxx` pattern
- **Warehouses:** `W0000001-xxxx-xxxx-xxxx-xxxxxxxxxxxx` pattern
- **Stores:** `S0000001-xxxx-xxxx-xxxx-xxxxxxxxxxxx` pattern

### **2. Connection Strings**

Each microservice should use its own connection string:

```json
{
  "ConnectionStrings": {
    "IdentityDB": "Server=localhost;Database=IdentityDB;...",
    "ProductDB": "Server=localhost;Database=ProductDB;...",
    "InventoryDB": "Server=localhost;Database=InventoryDB;...",
    "POSDB": "Server=localhost;Database=POSDB;...",
    "PaymentDB": "Server=localhost;Database=PaymentDB;...",
    "PromotionLoyaltyDB": "Server=localhost;Database=PromotionLoyaltyDB;..."
  }
}
```

### **3. Soft Delete Pattern**

Most tables use `is_deleted BIT` for soft deletion:

```sql
WHERE is_deleted = 0  -- Always filter out deleted records
```

### **4. Audit Logging**

All critical operations are logged:

- **IdentityDB:** `user_audit_logs` tracks all user management actions
- **InventoryDB:** `inventory_logs` tracks all stock changes
- **PaymentDB:** `payment_callbacks` logs all gateway interactions

### **5. Data Consistency**

When querying across services, ensure:

1. **User IDs** exist in IdentityDB before using in other services
2. **Product IDs** exist in ProductDB before creating inventory/sales
3. **Location IDs** (warehouses/stores) exist before creating transfers
4. **Sale IDs** exist before creating payment transactions

---

## **📈 SAMPLE QUERIES**

### **Get User with Role and Location**
```sql
USE IdentityDB;
SELECT 
    u.id, u.email, u.full_name, r.name AS role_name,
    ul.location_type, ul.location_id, ul.role_at_location, ul.is_primary
FROM users u
JOIN roles r ON u.role_id = r.id
LEFT JOIN user_locations ul ON u.id = ul.user_id AND ul.is_active = 1
WHERE u.email = 'manager1@company.com';
```

### **Get Unread Notifications for User**
```sql
USE IdentityDB;
SELECT id, notification_type, title, message, priority, created_at
FROM notifications
WHERE user_id = '22222222-2222-2222-2222-222222222221'
  AND is_read = 0
ORDER BY priority DESC, created_at DESC;
```

### **Get System Settings by Category**
```sql
USE IdentityDB;
SELECT setting_key, setting_value, data_type, description
FROM system_settings
WHERE category = 'LOYALTY' AND is_editable = 1;
```

### **Get Product with Category**
```sql
USE ProductDB;
SELECT p.id, p.sku, p.name, c.name AS category_name, p.price
FROM products p
JOIN categories c ON p.category_id = c.id
WHERE p.is_deleted = 0 AND p.is_available = 1;
```

### **Get Inventory by Location**
```sql
USE InventoryDB;
SELECT 
    i.product_id,
    i.location_type,
    i.quantity,
    i.available_quantity,
    i.min_stock_level
FROM inventories i
WHERE i.location_type = 'WAREHOUSE' 
  AND i.location_id = 'W0000001-0001-0001-0001-000000000001';
```

### **Get Sale with Items**
```sql
USE POSDB;
SELECT 
    s.sale_number,
    s.total_amount,
    s.payment_method,
    si.product_name,
    si.quantity,
    si.unit_price
FROM sales s
JOIN sale_items si ON s.id = si.sale_id
WHERE s.sale_number = 'SALE-2024-001';
```

### **Get Customer Loyalty Info**
```sql
USE PromotionLoyaltyDB;
SELECT 
    cl.customer_id,
    mt.tier_name,
    cl.available_points,
    cl.total_purchases,
    cl.purchase_count
FROM customer_loyalty cl
JOIN membership_tiers mt ON cl.membership_tier_id = mt.id
WHERE cl.customer_id = '55555555-5555-5555-5555-555555555551';
```

---

## **🎯 NEXT STEPS**

1. ✅ **Databases Created** - All 6 databases with sample data
2. ⏳ **API Implementation** - Build REST APIs for each service
3. ⏳ **Frontend Integration** - Connect React frontend to APIs
4. ⏳ **Payment Gateway Setup** - Configure VNPay & Momo credentials
5. ⏳ **Testing** - Integration testing across all services

---

## **📞 SUPPORT**

For questions about database structure, refer to:
- **SYSTEM_OVERVIEW.md** - Complete system documentation
- **API_ENDPOINTS.md** - API specifications
- **Individual SQL files** - Each has detailed comments

---

**Document Version:** 1.0  
**Created:** March 3, 2026  
**Status:** Complete Database Schema Package 🗄️
