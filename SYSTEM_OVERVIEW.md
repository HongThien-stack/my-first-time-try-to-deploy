# 🏪 INTERNAL SCM SYSTEM - HỆ THỐNG QUẢN LÝ CHUỖI CUNG ỨNG NỘI BỘ

## 📋 **TỔNG QUAN DỰ ÁN**

### **Tên Dự Án:** 
Internal Supply Chain Management System với E-wallet Payments

### **Loại Hệ Thống:**
- **Primary (90%):** Internal Supply Chain Management (Warehouse-to-Store Distribution)
- **Secondary (10%):** Point of Sale với Modern Payment Methods

### **Phạm Vi:**
- ✅ Quản lý kho hàng (warehouses)
- ✅ Quản lý tồn kho (inventory tracking)
- ✅ Chuyển hàng (warehouse → stores)
- ✅ Bán hàng tại cửa hàng (POS)
- ✅ Thanh toán hiện đại (VNPay, Momo QR)
- ✅ Chương trình khuyến mãi & tích điểm
- ❌ KHÔNG có đặt hàng online
- ❌ KHÔNG có giao hàng tận nơi

---

## 🏗️ **KIẾN TRÚC HỆ THỐNG**

### **Microservices (6 Services):**

```
1. IdentityService (Port 5000-5001)
   - Authentication & Authorization
   - User management
   
2. ProductService (Port 5002-5003)
   - Product catalog
   - Category management
   
3. InventoryService (Port 5004-5005)
   - Warehouse management (slots, batches)
   - Store inventory tracking
   - Transfers (warehouse → store)
   - Restock requests
   - Damage reports
   - Inventory checks
   
4. POSService (Port 5006-5007)
   - Sales transactions at stores
   - Payment recording
   
5. PaymentService (Port 5008-5009)
   - VNPay integration (QR code)
   - Momo integration (QR code)
   - Transaction tracking
   
6. PromotionLoyaltyService (Port 5010-5011)
   - Promotion campaigns
   - Discount vouchers
   - Loyalty points system
   - Membership tiers
   - Rewards catalog
```

### **Technology Stack:**
- **Backend:** .NET 9.0, ASP.NET Core Web API
- **ORM:** Entity Framework Core 9.0.1
- **Database:** SQL Server
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API)
- **Payment Gateways:** VNPay, Momo

---

## 👥 **NGƯỜI DÙNG HỆ THỐNG**

### **1. Admin (System Administrator)**
- Quản lý toàn bộ hệ thống
- Quản lý users
- Cấu hình hệ thống

### **2. Warehouse Manager**
- Quản lý kho tổng
- Duyệt transfers
- Quản lý nhận hàng từ suppliers

### **3. Warehouse Staff**
- Nhập hàng vào kho
- Quản lý slots, batches
- Kiểm kê kho
- Xử lý chuyển hàng

### **4. Store Manager**
- Quản lý tồn kho cửa hàng
- Tạo restock requests
- Xem báo cáo cửa hàng

### **5. Store Staff / Cashier**
- Bán hàng tại POS
- Xử lý thanh toán
- In hóa đơn

### **6. Customer (Optional)**
- Tích điểm loyalty
- Xem rewards
- Đổi điểm lấy voucher

---

## 🖥️ **CÁC MÀN HÌNH CẦN CÓ**

---

## **I. MODULE QUẢN TRỊ (ADMIN)**

### **1. Dashboard - Tổng Quan**
**Người dùng:** Admin, Manager  
**Mục đích:** Hiển thị tổng quan toàn hệ thống

**Nội dung màn hình:**
```
┌─────────────────────────────────────────────────┐
│  📊 DASHBOARD - TỔNG QUAN HỆ THỐNG             │
├─────────────────────────────────────────────────┤
│                                                  │
│  Cards (Top):                                    │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌────────┐│
│  │ 1.2M VNĐ│ │ 350 SKU │ │  15 Low │ │ 25 POs ││
│  │ Revenue │ │Products │ │  Stock  │ │Pending ││
│  │  Today  │ │ Active  │ │ Items   │ │Orders  ││
│  └─────────┘ └─────────┘ └─────────┘ └────────┘│
│                                                  │
│  Charts:                                         │
│  ├─ Revenue Chart (Last 30 days)                │
│  ├─ Top Selling Products (Bar chart)            │
│  ├─ Inventory Status (Pie chart)                │
│  └─ Low Stock Alerts (List)                     │
│                                                  │
│  Quick Actions:                                  │
│  [+ New Transfer] [+ Restock Request]           │
│  [Inventory Check] [View Reports]               │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Dữ liệu hiển thị:**
- Tổng doanh thu hôm nay
- Số lượng sản phẩm active
- Số items low stock
- Số transfer/restock pending
- Biểu đồ doanh thu
- Top sản phẩm bán chạy
- Cảnh báo tồn kho thấp
- **NEW:** Notifications (low stock, expiring, approval needed)

**Chức năng Thông Báo:**
```
┌─────────────────────────────────────────────────┐
│  🔔 NOTIFICATIONS (Badge: 3)                    │
├─────────────────────────────────────────────────┤
│  🔴 [URGENT] Yêu cầu duyệt: RST-2024-002       │
│     Manager1 cần duyệt restock 150 units       │
│     2 hours ago                      [View]     │
│  ─────────────────────────────────────────────  │
│  🟠 [HIGH] Sắp hết hàng: Sữa Vinamilk          │
│     Còn 15/30 units tại Cửa Hàng Thủ Đức       │
│     5 hours ago                      [View]     │
│  ─────────────────────────────────────────────  │
│  🟡 [NORMAL] Hàng sắp hết hạn                  │
│     Batch VNM-2024-001, còn 30 ngày            │
│     1 day ago                        [View]     │
│                                                  │
│  [Mark All Read] [View All Notifications]       │
└─────────────────────────────────────────────────┘
```

**Notification Types:**
- LOW_STOCK: Hàng sắp hết
- EXPIRING_SOON: Hàng gần hết hạn
- APPROVAL_REQUIRED: Cần duyệt (transfer, restock)
- TRANSFER_COMPLETED: Chuyển hàng hoàn thành
- RESTOCK_APPROVED/REJECTED: Kết quả yêu cầu nhập hàng

---

### **2. Quản Lý Người Dùng**
**Người dùng:** Admin  
**Mục đích:** CRUD users, phân quyền, gán workplace

**Màn hình danh sách:**
```
┌─────────────────────────────────────────────────┐
│  👥 QUẢN LÝ NGƯỜI DÙNG                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  Filters:                                        │
│  [Role: All ▼] [Workplace: All ▼] [Status: All ▼] │
│  [Search: ___]  [+ Add User]           [Export]  │
│                                                  │
│  Table:                                          │
│  ┌──┬──────────┬────────────┬────────┬─────────┬──────┐  │
│  │ID│Email     │Full Name   │Role    │Workplace│Status│  │
│  ├──┼──────────┼────────────┼────────┼─────────┼──────┤  │
│  │1 │admin@... │Admin User  │Admin   │-        │Active│  │
│  │2 │manager...│Tran Manager│Manager │Kho HCM  │Active│  │
│  │3 │cashier...│Pham Cashier│Staff   │CH Thủ Đ │Active│  │
│  │  │          │            │        │         │[Edit]│  │
│  └──┴──────────┴────────────┴────────┴─────────┴──────┘  │
│                                                  │
│  Pagination: [<] 1 2 3 ... 10 [>]               │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Form thêm/sửa user:**
```
┌─────────────────────────────────────────────────┐
│  ✏️ THÊM/SỬA NGƯỜI DÙNG                        │
├─────────────────────────────────────────────────┤
│                                                  │
│  Email: [_____________________] *               │
│  Full Name: [_____________________] *           │
│  Phone: [_____________________]                 │
│                                                  │
│  Password: [_____________________] *            │
│  Confirm Password: [_____________________] *    │
│                                                  │
│  Role: [Manager ▼] *                             │
│  Status: [Active ▼] *                           │
│                                                  │
│  Workplace: (for Manager/Staff only)            │
│    Type: [Warehouse ▼]                          │
│    Location: [Kho Tổng HCM ▼]                  │
│                                                  │
│  [Cancel] [Save]                                │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Chức năng:**
- ✅ **Simple Workplace Assignment**: Mỗi user chỉ thuộc về 1 warehouse/store
- ✅ **Admin/Customer**: Không có workplace (NULL)
- ✅ **Direct Assignment**: Không cần bảng phức tạp

---

### **3. Quản Lý Sản Phẩm**
**Người dùng:** Admin, Manager  
**Mục đích:** CRUD products

**Màn hình danh sách:**
```
┌─────────────────────────────────────────────────┐
│  📦 QUẢN LÝ SẢN PHẨM                            │
├─────────────────────────────────────────────────┤
│                                                  │
│  Filters:                                        │
│  [Category: All ▼] [Status: All ▼]             │
│  [Search: ___________] [+ Add Product]          │
│                                                  │
│  Table:                                          │
│  ┌─────┬─────────┬──────────┬────────┬────────┐ │
│  │Image│ SKU     │ Name     │Category│ Price  │ │
│  ├─────┼─────────┼──────────┼────────┼────────┤ │
│  │ 🖼️ │SUA-001  │Sua TH    │Dairy   │38,000đ││
│  │ 🖼️ │GAO-001  │Gao ST25  │Rice    │180K   ││
│  │     │         │          │        │[Edit] │ │
│  └─────┴─────────┴──────────┴────────┴────────┘ │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Form thêm/sửa sản phẩm:**
```
┌─────────────────────────────────────────────────┐
│  ✏️ THÊM/SỬA SẢN PHẨM                          │
├─────────────────────────────────────────────────┤
│  Basic Info:                                     │
│    Name: [_____________________] *              │
│    SKU: [_____________________] *               │
│    Barcode: [_____________________]             │
│    Category: [Select... ▼] *                    │
│    Brand: [_____________________]               │
│                                                  │
│  Pricing:                                        │
│    Price: [_____________________] * VNĐ         │
│    Original Price: [___________] VNĐ            │
│    Cost Price: [___________] VNĐ                │
│                                                  │
│  Inventory:                                      │
│    Unit: [Kg ▼] *                               │
│    Weight: [___________] kg                     │
│    Is Perishable: [ ] Yes                       │
│    Shelf Life: [___________] days               │
│                                                  │
│  Images:                                         │
│    [Upload Image] [+ Add More]                  │
│    🖼️ image1.jpg [x]                           │
│                                                  │
│  Description:                                    │
│    [________________________________]            │
│    [________________________________]            │
│                                                  │
│  [Cancel] [Save Product]                        │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## **II. MODULE KHO - WAREHOUSE**

### **4. Danh Sách Kho**
**Người dùng:** Admin, Warehouse Manager  
**Mục đích:** Xem danh sách warehouses

```
┌─────────────────────────────────────────────────┐
│  🏭 DANH SÁCH KHO                               │
├─────────────────────────────────────────────────┤
│                                                  │
│  [+ Add Warehouse]                               │
│                                                  │
│  Cards:                                          │
│  ┌──────────────────────┐ ┌──────────────────┐  │
│  │ 🏭 Kho Tổng HCM      │ │ 🏭 Kho Miền Bắc  │  │
│  │ Location: Q. Thủ Đức │ │ Location: Hà Nội │  │
│  │ Total Slots: 120     │ │ Total Slots: 80  │  │
│  │ Used: 85 (71%)       │ │ Used: 45 (56%)   │  │
│  │ [View Details]       │ │ [View Details]   │  │
│  └──────────────────────┘ └──────────────────┘  │
│                                                  │
│  ┌──────────────────────┐                       │
│  │ 🏭 Kho Miền Trung    │                       │
│  │ Location: Đà Nẵng    │                       │
│  │ Total Slots: 60      │                       │
│  │ Used: 30 (50%)       │                       │
│  │ [View Details]       │                       │
│  └──────────────────────┘                       │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **5. Chi Tiết Kho & Slots**
**Người dùng:** Warehouse Manager, Warehouse Staff  
**Mục đích:** Xem chi tiết slots và inventory

```
┌─────────────────────────────────────────────────┐
│  🏭 KHO TỔNG HCM - CHI TIẾT                     │
├─────────────────────────────────────────────────┤
│                                                  │
│  Info:                                           │
│  Name: Kho Tổng HCM                             │
│  Location: Quận Thủ Đức, TP.HCM                 │
│  Total Slots: 120 | Used: 85 | Available: 35   │
│                                                  │
│  Tabs: [Slots] [Inventory] [Movements]         │
│                                                  │
│  ─────── SLOTS TAB ───────                      │
│                                                  │
│  Filters:                                        │
│  [Zone: All ▼] [Status: All ▼] [Search: ___]   │
│  [+ Add Slot]                                    │
│                                                  │
│  Table:                                          │
│  ┌──────┬──────┬────────┬─────────┬──────────┐  │
│  │Slot  │ Zone │Product │Quantity │ Expiry   │  │
│  ├──────┼──────┼────────┼─────────┼──────────┤  │
│  │A-01-1│Zone A│Sua TH  │ 500     │2024-06-01││
│  │A-01-2│Zone A│Gao ST25│ 1,200kg │2025-12-31││
│  │B-02-1│Zone B│Empty   │   -     │    -     ││
│  │      │      │        │         │ [View]   │  │
│  └──────┴──────┴────────┴─────────┴──────────┘  │
│                                                  │
│  ─────── INVENTORY TAB ───────                  │
│                                                  │
│  Summary by Product:                             │
│  ┌─────────┬────────┬──────────┬──────────────┐ │
│  │Product  │SKU     │ Quantity │ Value        │ │
│  ├─────────┼────────┼──────────┼──────────────┤ │
│  │Sua TH   │SUA-002 │ 500 hộp │19,000,000 đ ││
│  │Gao ST25 │GAO-001 │ 1,200kg │216,000,000 đ││
│  │         │        │          │   [Details]  │ │
│  └─────────┴────────┴──────────┴──────────────┘ │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **6. Nhận Hàng Từ Supplier**
**Người dùng:** Warehouse Staff  
**Mục đích:** Nhập hàng vào kho

```
┌─────────────────────────────────────────────────┐
│  📥 NHẬN HÀNG TỪ NHÀ CUNG CẤP                   │
├─────────────────────────────────────────────────┤
│                                                  │
│  Warehouse: [Kho Tổng HCM ▼] *                 │
│  Supplier: [_____________________]              │
│  Receipt Date: [2024-03-03] *                   │
│  PO Number: [_____________________]             │
│                                                  │
│  Products:                                       │
│  ┌─────────┬────────┬────────┬────────────────┐ │
│  │Product  │Quantity│Unit    │ Batch Info     │ │
│  ├─────────┼────────┼────────┼────────────────┤ │
│  │Sua TH   │ [100] │hộp     │Batch: [______]│││
│  │         │        │        │Mfg: [date]    │││
│  │         │        │        │Exp: [date]    │││
│  │         │        │        │Slot: [A-01-1▼]│││
│  ├─────────┼────────┼────────┼────────────────┤ │
│  │[+ Add Product]                              │ │
│  └─────────┴────────┴────────┴────────────────┘ │
│                                                  │
│  Notes: [_________________________________]      │
│                                                  │
│  [Cancel] [Save & Receive]                      │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **7. Tạo Transfer (Chuyển Hàng)**
**Người dùng:** Warehouse Manager, Warehouse Staff  
**Mục đích:** Chuyển hàng từ warehouse → store

```
┌─────────────────────────────────────────────────┐
│  🚚 TẠO PHIẾU CHUYỂN HÀNG                       │
├─────────────────────────────────────────────────┤
│                                                  │
│  Transfer Info:                                  │
│    From: [Kho Tổng HCM ▼] *                    │
│    To: [Cửa Hàng Quận 1 ▼] *                   │
│    Transfer Date: [2024-03-03] *                │
│    Expected Delivery: [2024-03-04]              │
│                                                  │
│  Products:                                       │
│  [Search product...] [Scan Barcode]             │
│                                                  │
│  ┌─────────┬────────┬────────┬─────────┬──────┐ │
│  │Product  │SKU     │Avail.  │Transfer │Batch │ │
│  ├─────────┼────────┼────────┼─────────┼──────┤ │
│  │Sua TH   │SUA-002 │ 500    │[_50_]  │Batch1││
│  │Gao ST25 │GAO-001 │1,200kg │[_100_] │Batch2││
│  │         │        │        │        │[x]   │ │
│  └─────────┴────────┴────────┴─────────┴──────┘ │
│                                                  │
│  [+ Add Product]                                 │
│                                                  │
│  Notes: [_________________________________]      │
│                                                  │
│  [Cancel] [Create Transfer]                     │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **8. Danh Sách Transfers**
**Người dùng:** Warehouse Manager, Store Manager  
**Mục đích:** Theo dõi chuyển hàng

```
┌─────────────────────────────────────────────────┐
│  🚚 DANH SÁCH CHUYỂN HÀNG                       │
├─────────────────────────────────────────────────┤
│                                                  │
│  Filters:                                        │
│  [Status: All ▼] [From: All ▼] [To: All ▼]    │
│  [Date Range: Last 30 days ▼]                   │
│  [+ New Transfer]                                │
│                                                  │
│  Table:                                          │
│  ┌──────────┬─────────┬─────────┬────────┬────┐ │
│  │Transfer# │ From    │ To      │ Date   │Stat││
│  ├──────────┼─────────┼─────────┼────────┼────┤ │
│  │TRF-001   │Kho HCM  │CH Q1    │03/03   │✓Done│
│  │TRF-002   │Kho HCM  │CH Q2    │03/03   │⏳Pen││
│  │TRF-003   │Kho HCM  │CH Q3    │03/02   │🚚Tran││
│  │          │         │         │        │[View││
│  └──────────┴─────────┴─────────┴────────┴────┘ │
│                                                  │
│  Legend:                                         │
│  ✓ DELIVERED | 🚚 IN_TRANSIT | ⏳ PENDING      │
│  ❌ CANCELLED                                   │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **9. Kiểm Kê Kho (Inventory Check)**
**Người dùng:** Warehouse Staff  
**Mục đích:** Kiểm tra và cập nhật tồn kho thực tế

```
┌─────────────────────────────────────────────────┐
│  📋 KIỂM KÊ KHO                                 │
├─────────────────────────────────────────────────┤
│                                                  │
│  Check Info:                                     │
│    Location: [Kho Tổng HCM ▼] *                │
│    Check Date: [2024-03-03] *                   │
│    Checker: [Warehouse Staff 1]                 │
│    Type: [ ] Full Check  [x] Partial           │
│                                                  │
│  Products to Check:                              │
│  [Search or scan barcode...]                     │
│                                                  │
│  ┌─────────┬────────┬────────┬────────┬───────┐ │
│  │Product  │System  │Actual  │Diff    │Action │ │
│  ├─────────┼────────┼────────┼────────┼───────┤ │
│  │Sua TH   │ 500    │[_498_] │  -2    │Note  ││
│  │Gao ST25 │1,200kg │[1200]  │   0    │✓OK   ││
│  │Cam Sanh │  80kg  │[_85_]  │  +5    │Note  ││
│  │         │        │        │        │[x]   │ │
│  └─────────┴────────┴────────┴────────┴───────┘ │
│                                                  │
│  Note for differences: [_____________________]  │
│                                                  │
│  [Cancel] [Submit Check]                        │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## **III. MODULE CỬA HÀNG - STORE**

### **10. Tồn Kho Cửa Hàng**
**Người dùng:** Store Manager, Store Staff  
**Mục đích:** Xem tồn kho tại store

```
┌─────────────────────────────────────────────────┐
│  🏪 TỒN KHO CỬA HÀNG QUẬN 1                     │
├─────────────────────────────────────────────────┤
│                                                  │
│  Summary:                                        │
│  Total SKUs: 250 | Total Value: 150M VNĐ       │
│  Low Stock Items: 15 ⚠️                        │
│                                                  │
│  Quick Actions:                                  │
│  [+ Restock Request] [Inventory Check]          │
│                                                  │
│  Filters:                                        │
│  [Category: All ▼] [Stock Status: All ▼]       │
│  [Search: ___________]                           │
│                                                  │
│  Table:                                          │
│  ┌─────────┬────────┬────────┬────────┬───────┐ │
│  │Product  │SKU     │Qty     │Status  │Action │ │
│  ├─────────┼────────┼────────┼────────┼───────┤ │
│  │Sua TH   │SUA-002 │ 50     │✓ OK    │Details││
│  │Gao ST25 │GAO-001 │ 15kg   │⚠️ Low  │Restock││
│  │Cam Sanh │TC-001  │  5kg   │❌ Critical│Restock│
│  │         │        │        │        │       │ │
│  └─────────┴────────┴────────┴────────┴───────┘ │
│                                                  │
│  Legend:                                         │
│  ✓ OK: > 20 units | ⚠️ Low: 5-20 | ❌ Critical: < 5│
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **11. Tạo Restock Request**
**Người dùng:** Store Manager, Store Staff  
**Mục đích:** Yêu cầu nhập thêm hàng từ warehouse

```
┌─────────────────────────────────────────────────┐
│  📦 TẠO YÊU CẦU NHẬP HÀNG                       │
├─────────────────────────────────────────────────┤
│                                                  │
│  Store: Cửa Hàng Quận 1                         │
│  Request Date: 2024-03-03                        │
│  Requested By: Store Manager 1                   │
│  Priority: [Normal ▼]                           │
│                                                  │
│  Products Needed:                                │
│  [Search or select from low stock items]        │
│                                                  │
│  ┌─────────┬────────┬────────┬────────┬───────┐ │
│  │Product  │Current │Request │Reason  │Action │ │
│  ├─────────┼────────┼────────┼────────┼───────┤ │
│  │Gao ST25 │  15kg  │[_50kg_]│Low stock│[x]  ││
│  │Cam Sanh │   5kg  │[_30kg_]│Critical│[x]  ││
│  │         │        │        │        │       │ │
│  └─────────┴────────┴────────┴────────┴───────┘ │
│                                                  │
│  [+ Add More Products]                           │
│                                                  │
│  Notes: [_________________________________]      │
│                                                  │
│  [Cancel] [Submit Request]                      │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **12. Nhận Hàng Tại Store**
**Người dùng:** Store Staff  
**Mục đích:** Xác nhận nhận hàng từ transfer

```
┌─────────────────────────────────────────────────┐
│  📥 NHẬN HÀNG TỪ KHO                            │
├─────────────────────────────────────────────────┤
│                                                  │
│  Transfer: TRF-002                               │
│  From: Kho Tổng HCM                             │
│  To: Cửa Hàng Quận 1                            │
│  Shipped Date: 2024-03-03                        │
│                                                  │
│  Products in Transfer:                           │
│                                                  │
│  ┌─────────┬────────┬────────┬────────┬───────┐ │
│  │Product  │Expected│Received│Status  │Note   │ │
│  ├─────────┼────────┼────────┼────────┼───────┤ │
│  │Gao ST25 │  50kg  │[_50kg_]│✓ Match │      ││
│  │Cam Sanh │  30kg  │[_28kg_]│⚠️ Diff │Damaged││
│  │         │        │        │        │       │ │
│  └─────────┴────────┴────────┴────────┴───────┘ │
│                                                  │
│  Receiver: [Store Staff 1]                       │
│  Received Date: [2024-03-04]                     │
│                                                  │
│  Notes: [2kg cam bị dập trong quá trình vận    │
│          chuyển]                                 │
│                                                  │
│  [Cancel] [Confirm Received]                    │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **13. Báo Cáo Hàng Hỏng**
**Người dùng:** Store Staff  
**Mục đích:** Báo cáo sản phẩm bị hỏng/hết hạn

```
┌─────────────────────────────────────────────────┐
│  ⚠️ BÁO CÁO HÀNG HỎ NG                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  Store: Cửa Hàng Quận 1                         │
│  Report Date: [2024-03-03] *                    │
│  Reported By: [Store Staff 1]                    │
│                                                  │
│  Damage Type:                                    │
│  [ ] Expired                                     │
│  [x] Physical Damage                             │
│  [ ] Quality Issue                               │
│  [ ] Other: [___________]                       │
│                                                  │
│  Products:                                       │
│  [Search or scan barcode...]                     │
│                                                  │
│  ┌─────────┬────────┬────────┬────────────────┐ │
│  │Product  │Batch   │Quantity│Value           │ │
│  ├─────────┼────────┼────────┼────────────────┤ │
│  │Cam Sanh │B-001   │[_2kg_] │70,000 VNĐ     ││
│  │         │        │        │       [x]      │ │
│  └─────────┴────────┴────────┴────────────────┘ │
│                                                  │
│  [+ Add Product]                                 │
│                                                  │
│  Description: [Cam bị dập trong quá trình vận  │
│                chuyển, không thể bán]            │
│                                                  │
│  Photos: [Upload photos...]                      │
│  🖼️ image1.jpg [x]                             │
│                                                  │
│  [Cancel] [Submit Report]                       │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## **IV. MODULE BÁN HÀNG - POS**

### **14. Màn Hình Bán Hàng (POS)**
**Người dùng:** Cashier  
**Mục đích:** Bán hàng tại quầy

```
┌─────────────────────────────────────────────────┐
│  💰 ĐIỂM BÁN HÀNG - POS                         │
├─────────────────────────────────────────────────┤
│                                                  │
│  Left Panel - Product Search:                    │
│  ┌──────────────────────────────────────────┐   │
│  │ [Search or Scan Barcode...] 🔍          │   │
│  │                                           │   │
│  │ Categories: [All] [Dairy] [Rice] [Fruit]│   │
│  │                                           │   │
│  │ Products Grid:                            │   │
│  │ ┌────────┐ ┌────────┐ ┌────────┐        │   │
│  │ │Sua TH  │ │Gao ST25│ │Cam Sanh│        │   │
│  │ │🖼️     │ │🖼️     │ │🖼️     │        │   │
│  │ │38,000đ│ │180,000│ │35,000đ│        │   │
│  │ └────────┘ └────────┘ └────────┘        │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  Right Panel - Cart:                             │
│  ┌──────────────────────────────────────────┐   │
│  │ Customer: [Search phone...] 🔍           │   │
│  │ Points Available: 1,500 pts              │   │
│  │                                           │   │
│  │ Cart Items:                               │   │
│  │ ┌──────────┬────┬────────┬─────────────┐│   │
│  │ │Product   │Qty │Price   │Total        ││   │
│  │ ├──────────┼────┼────────┼─────────────┤│   │
│  │ │Sua TH    │ 2  │38,000  │76,000      ││   │
│  │ │Gao ST25  │0.5 │180,000 │90,000      ││   │
│  │ │          │    │        │  [x]       ││   │
│  │ └──────────┴────┴────────┴─────────────┘│   │
│  │                                           │   │
│  │ Subtotal:          166,000 VNĐ          │   │
│  │ Discount (10%):     -16,600 VNĐ         │   │
│  │ Points Used (500): -10,000 VNĐ          │   │
│  │ ================================          │   │
│  │ TOTAL:             139,400 VNĐ          │   │
│  │                                           │   │
│  │ Payment Method:                           │   │
│  │ [💵 Cash] [💳 Card] [📱 VNPay] [📱 Momo]│   │
│  │                                           │   │
│  │ [Clear Cart] [Hold] [Checkout]           │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **15. Thanh Toán VNPay/Momo (QR Code)**
**Người dùng:** Cashier, Customer  
**Mục đích:** Thanh toán bằng QR code

```
┌─────────────────────────────────────────────────┐
│  📱 THANH TOÁN QR CODE                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  Payment Method Selected: 📱 VNPay              │
│                                                  │
│  Amount to Pay: 139,400 VNĐ                    │
│                                                  │
│  Please scan QR code with VNPay app:            │
│                                                  │
│          ┌───────────────────┐                  │
│          │                   │                  │
│          │   █████████████   │                  │
│          │   ██ ▄▄▄▄▄ ██ █   │                  │
│          │   ██ █   █ █ ██   │                  │
│          │   ██ █▄▄▄█ █ ██   │                  │
│          │   ██▄▄▄▄▄▄▄▄▄██   │                  │
│          │   █ ▄ █▀█▄  ███   │                  │
│          │   █████████████   │                  │
│          │                   │                  │
│          └───────────────────┘                  │
│                                                  │
│  Status: ⏳ Waiting for payment...              │
│                                                  │
│  Transaction ID: PAY-2024-001234                 │
│                                                  │
│  [Cancel Payment] [Check Status]                │
│                                                  │
│  ─────────────────────────────────               │
│  After successful payment:                       │
│  ✓ Payment Received!                            │
│  [Print Receipt] [New Sale]                     │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **16. Lịch Sử Bán Hàng**
**Người dùng:** Cashier, Store Manager  
**Mục đích:** Xem lịch sử giao dịch

```
┌─────────────────────────────────────────────────┐
│  📊 LỊCH SỬ BÁN HÀNG                            │
├─────────────────────────────────────────────────┤
│                                                  │
│  Filters:                                        │
│  [Date: Today ▼] [Cashier: All ▼]              │
│  [Payment: All ▼] [Search: ___]                 │
│                                                  │
│  Today Summary:                                  │
│  Total Sales: 1,250,000 VNĐ | Transactions: 45 │
│  Cash: 800K | Card: 200K | VNPay: 150K | Momo: 100K│
│                                                  │
│  Table:                                          │
│  ┌────────┬──────────┬────────┬────────┬──────┐ │
│  │Sale#   │Time      │Amount  │Payment │Status││
│  ├────────┼──────────┼────────┼────────┼──────┤ │
│  │S-001   │10:30 AM  │139,400 │VNPay   │✓Paid││
│  │S-002   │10:45 AM  │250,000 │Cash    │✓Paid││
│  │S-003   │11:00 AM  │180,000 │Momo    │✓Paid││
│  │        │          │        │        │[View]││
│  └────────┴──────────┴────────┴────────┴──────┘ │
│                                                  │
│  [Export to Excel]                               │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## **V. MODULE KHUYẾN MÃI & TÍCH ĐIỂM**

### **17. Quản Lý Khuyến Mãi**
**Người dùng:** Admin, Manager  
**Mục đích:** Tạo và quản lý promotions

```
┌─────────────────────────────────────────────────┐
│  🎁 QUẢN LÝ KHUYẾN MÃI                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  [+ Create Promotion]                            │
│                                                  │
│  Active Promotions (3):                          │
│  ┌──────────────────────────────────────────┐   │
│  │ 🎉 FLASH SALE 10%                        │   │
│  │ Code: FLASH10                             │   │
│  │ Type: Percentage Discount                 │   │
│  │ Value: 10%                                │   │
│  │ Period: 01/03 - 07/03/2024               │   │
│  │ Used: 125 / 1000                          │   │
│  │ [Edit] [Deactivate] [View Details]       │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 🎁 MUA 2 TẶNG 1                          │   │
│  │ Code: BUY2GET1                            │   │
│  │ Type: Buy X Get Y                         │   │
│  │ Rule: Buy 2 Sua TH, Get 1 Free          │   │
│  │ Period: 01/03 - 31/03/2024               │   │
│  │ Used: 45 / Unlimited                      │   │
│  │ [Edit] [Deactivate] [View Details]       │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Form tạo promotion:**
```
┌─────────────────────────────────────────────────┐
│  ✏️ TẠO KHUYẾN MÃI MỚI                         │
├─────────────────────────────────────────────────┤
│                                                  │
│  Basic Info:                                     │
│    Promotion Name: [_____________________] *    │
│    Code: [_____________________] *              │
│    Description: [_____________________]         │
│                                                  │
│  Type: [Percentage Discount ▼] *               │
│    ├─ Percentage Discount                       │
│    ├─ Fixed Discount                            │
│    ├─ Buy X Get Y                               │
│    └─ Bundle Deal                               │
│                                                  │
│  Discount Value:                                 │
│    Percentage: [___10___] %                     │
│    OR Fixed Amount: [_________] VNĐ             │
│                                                  │
│  Rules:                                          │
│    Min Purchase: [_________] VNĐ                │
│    Max Discount: [_________] VNĐ                │
│    Usage Limit: [_________] times               │
│    Per Customer Limit: [___] times              │
│                                                  │
│  Applicable To:                                  │
│    [x] All Products                             │
│    [ ] Specific Products: [Select...▼]         │
│    [ ] Specific Categories: [Select...▼]       │
│                                                  │
│  Period:                                         │
│    Start Date: [2024-03-01] *                   │
│    End Date: [2024-03-31] *                     │
│                                                  │
│  Loyalty Integration:                            │
│    Points Required: [________] points           │
│    Points Awarded: [________] points            │
│                                                  │
│  [Cancel] [Save Promotion]                      │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **18. Quản Lý Voucher**
**Người dùng:** Admin, Manager  
**Mục đích:** Tạo và phát voucher

```
┌─────────────────────────────────────────────────┐
│  🎫 QUẢN LÝ VOUCHER                             │
├─────────────────────────────────────────────────┤
│                                                  │
│  [+ Generate Vouchers]                           │
│                                                  │
│  Filters:                                        │
│  [Promotion: All ▼] [Status: All ▼] [Search]   │
│                                                  │
│  Table:                                          │
│  ┌────────────┬───────────┬────────┬──────────┐ │
│  │Voucher Code│Promotion  │Customer│Status    │ │
│  ├────────────┼───────────┼────────┼──────────┤ │
│  │SAVE50K-001 │Spring Sale│Public  │Available ││
│  │SAVE50K-002 │Spring Sale│Public  │Used      ││
│  │VIP100K-001 │VIP Member │Cust-123│Available ││
│  │            │           │        │  [View]  │ │
│  └────────────┴───────────┴────────┴──────────┘ │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **19. Chương Trình Tích Điểm**
**Người dùng:** Admin  
**Mục đích:** Cấu hình loyalty program

```
┌─────────────────────────────────────────────────┐
│  ⭐ CHƯƠNG TRÌNH TÍCH ĐIỂM                      │
├─────────────────────────────────────────────────┤
│                                                  │
│  Membership Tiers:                               │
│  ┌──────────────────────────────────────────┐   │
│  │ 🥉 BRONZE (0 - 999 points)               │   │
│  │ Benefits:                                 │   │
│  │ - Earn 1 point per 10,000 VNĐ           │   │
│  │ - No discount                             │   │
│  │ [Edit]                                    │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 🥈 SILVER (1,000 - 4,999 points)         │   │
│  │ Benefits:                                 │   │
│  │ - Earn 1.5 points per 10,000 VNĐ        │   │
│  │ - 5% extra discount                       │   │
│  │ [Edit]                                    │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 🥇 GOLD (5,000 - 9,999 points)           │   │
│  │ Benefits:                                 │   │
│  │ - Earn 2 points per 10,000 VNĐ          │   │
│  │ - 10% extra discount                      │   │
│  │ - Priority restock                        │   │
│  │ [Edit]                                    │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 💎 PLATINUM (10,000+ points)             │   │
│  │ Benefits:                                 │   │
│  │ - Earn 3 points per 10,000 VNĐ          │   │
│  │ - 15% extra discount                      │   │
│  │ - Exclusive rewards                       │   │
│  │ [Edit]                                    │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **20. Danh Sách Khách Hàng**
**Người dùng:** Admin, Manager  
**Mục đích:** Quản lý loyalty members

```
┌─────────────────────────────────────────────────┐
│  👥 DANH SÁCH KHÁCH HÀNG                        │
├─────────────────────────────────────────────────┤
│                                                  │
│  Summary:                                        │
│  Total Members: 1,250                            │
│  Bronze: 850 | Silver: 250 | Gold: 120 | Plat: 30│
│                                                  │
│  Filters:                                        │
│  [Tier: All ▼] [Status: All ▼] [Search: ___]   │
│                                                  │
│  Table:                                          │
│  ┌──────────┬────────┬──────────┬─────────┬────┐│
│  │Customer  │Phone   │ Tier     │ Points  │Pur ││
│  ├──────────┼────────┼──────────┼─────────┼────┤│
│  │Nguyen VK │0901... │🥈 Silver │ 2,500   │15M ││
│  │Tran TH   │0902... │🥇 Gold   │ 7,800   │45M ││
│  │Le VM     │0903... │💎 Plat   │ 15,000  │120M││
│  │          │        │          │         │[View│
│  └──────────┴────────┴──────────┴─────────┴────┘│
│                                                  │
│  [Export] [Send Promotion]                       │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **21. Catalog Phần Thưởng**
**Người dùng:** Admin  
**Mục đích:** Quản lý rewards có thể đổi

```
┌─────────────────────────────────────────────────┐
│  🎁 CATALOG PHẦN THƯỞNG                         │
├─────────────────────────────────────────────────┤
│                                                  │
│  [+ Add Reward]                                  │
│                                                  │
│  Available Rewards:                              │
│  ┌──────────────────────────────────────────┐   │
│  │ 🎫 Voucher Giảm 50K                      │   │
│  │ Points Cost: 500 points                   │   │
│  │ Type: Discount Voucher                    │   │
│  │ Stock: 100 remaining                      │   │
│  │ [Edit] [Deactivate]                       │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 🎁 Gạo ST25 Miễn Phí (1kg)              │   │
│  │ Points Cost: 1,800 points                 │   │
│  │ Type: Free Product                        │   │
│  │ Stock: 50 remaining                       │   │
│  │ [Edit] [Deactivate]                       │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  ┌──────────────────────────────────────────┐   │
│  │ 💳 Thẻ Quà Tặng 200K                     │   │
│  │ Points Cost: 2,000 points                 │   │
│  │ Type: Gift Card                           │   │
│  │ Stock: Unlimited                          │   │
│  │ [Edit] [Deactivate]                       │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## **VI. MODULE BÁO CÁO - REPORTS**

### **22. Báo Cáo Tổng Quan**
**Người dùng:** Admin, Manager  
**Mục đích:** Xem báo cáo tổng hợp

```
┌─────────────────────────────────────────────────┐
│  📊 BÁO CÁO TỔNG QUAN                           │
├─────────────────────────────────────────────────┤
│                                                  │
│  Period: [Last 30 days ▼] [From: ___] [To: ___]│
│                                                  │
│  Summary Cards:                                  │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐        │
│  │Total Sales│ │Profit    │ │Customers│        │
│  │45.2M VNĐ │ │12.5M    │ │1,250    │        │
│  │+15% ↑   │ │+8% ↑    │ │+120 ↑  │        │
│  └──────────┘ └──────────┘ └──────────┘        │
│                                                  │
│  Revenue Chart:                                  │
│  ┌──────────────────────────────────────────┐   │
│  │     📈 Daily Revenue                     │   │
│  │ 2M│     ▄▄▄                             │   │
│  │   │   ▄█  █▄                            │   │
│  │ 1M│ ▄█      ██▄                         │   │
│  │   │█          ██▄                       │   │
│  │ 0 └──────────────────────────────       │   │
│  │   1   7  14  21  28 (days)              │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  Top Products:                                   │
│  1. Gạo ST25 - 2,500kg sold - 450M VNĐ        │
│  2. Sữa TH - 1,200 boxes - 45.6M VNĐ          │
│  3. Cam Sành - 850kg - 29.75M VNĐ             │
│                                                  │
│  [Export Report] [Print]                        │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **23. Báo Cáo Tồn Kho**
**Người dùng:** Manager  
**Mục đích:** Báo cáo inventory

```
┌─────────────────────────────────────────────────┐
│  📦 BÁO CÁO TỒN KHO                             │
├─────────────────────────────────────────────────┤
│                                                  │
│  Location: [All ▼] Date: [2024-03-03]          │
│                                                  │
│  Summary:                                        │
│  Total Value: 1.2B VNĐ                          │
│  Total Items: 350 SKUs                           │
│  Low Stock: 25 items ⚠️                         │
│  Expiring Soon: 12 items ⚠️                     │
│                                                  │
│  Stock Status:                                   │
│  ┌────────────┬─────────┬──────────┬─────────┐  │
│  │Location    │ SKUs    │ Value    │ Status  │  │
│  ├────────────┼─────────┼──────────┼─────────┤  │
│  │Kho HCM     │ 250     │ 800M     │✓ Good  │  │
│  │CH Quận 1   │  80     │ 150M     │⚠️ Low  │  │
│  │CH Quận 2   │  65     │ 120M     │✓ Good  │  │
│  └────────────┴─────────┴──────────┴─────────┘  │
│                                                  │
│  Low Stock Items:                                │
│  ┌─────────┬────────┬────────┬────────────────┐ │
│  │Product  │Location│Current │Reorder Point   │ │
│  ├─────────┼────────┼────────┼────────────────┤ │
│  │Gao ST25 │CH Q1   │  15kg  │Target: 50kg   ││
│  │Cam Sanh │CH Q2   │   5kg  │Target: 30kg   ││
│  └─────────┴────────┴────────┴────────────────┘ │
│                                                  │
│  [Export] [Print]                                │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **24. Báo Cáo Chuyển Hàng**
**Người dùng:** Manager  
**Mục đích:** Báo cáo transfers

```
┌─────────────────────────────────────────────────┐
│  🚚 BÁO CÁO CHUYỂN HÀNG                         │
├─────────────────────────────────────────────────┤
│                                                  │
│  Period: [Last 30 days ▼]                       │
│                                                  │
│  Summary:                                        │
│  Total Transfers: 125                            │
│  Completed: 110 (88%)                            │
│  In Transit: 10 (8%)                             │
│  Pending: 5 (4%)                                 │
│                                                  │
│  Chart:                                          │
│  ┌──────────────────────────────────────────┐   │
│  │  Transfers by Status                     │   │
│  │  ████████████████████████████ 88% Done   │   │
│  │  ████ 8% In Transit                      │   │
│  │  ██ 4% Pending                           │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  By Route:                                       │
│  ┌────────────┬───────┬─────────┬───────────┐   │
│  │Route       │Count  │Value    │On-Time %  │   │
│  ├────────────┼───────┼─────────┼───────────┤   │
│  │Kho→CH Q1   │  45   │ 120M    │   95%     │   │
│  │Kho→CH Q2   │  38   │  95M    │   92%     │   │
│  │Kho→CH Q3   │  30   │  80M    │   90%     │   │
│  └────────────┴───────┴─────────┴───────────┘   │
│                                                  │
│  [Export] [Print]                                │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

### **25. Báo Cáo Thanh Toán**
**Người dùng:** Admin, Manager  
**Mục đích:** Báo cáo payment transactions

```
┌─────────────────────────────────────────────────┐
│  💳 BÁO CÁO THANH TOÁN                          │
├─────────────────────────────────────────────────┤
│                                                  │
│  Period: [Today ▼]                               │
│                                                  │
│  Summary:                                        │
│  Total Revenue: 1,250,000 VNĐ                   │
│  Total Transactions: 45                          │
│  Average Transaction: 27,778 VNĐ                │
│                                                  │
│  By Payment Method:                              │
│  ┌────────────┬───────┬─────────┬──────────┐    │
│  │Method      │Count  │Amount   │ %        │    │
│  ├────────────┼───────┼─────────┼──────────┤    │
│  │💵 Cash     │  25   │ 800,000 │  64%     │    │
│  │💳 Card     │  10   │ 200,000 │  16%     │    │
│  │📱 VNPay    │   8   │ 150,000 │  12%     │    │
│  │📱 Momo     │   5   │ 100,000 │   8%     │    │
│  └────────────┴───────┴─────────┴──────────┘    │
│                                                  │
│  Chart:                                          │
│  ┌──────────────────────────────────────────┐   │
│  │  █████████████████████ 64% Cash          │   │
│  │  ███████ 16% Card                        │   │
│  │  █████ 12% VNPay                         │   │
│  │  ███ 8% Momo                             │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  Failed Transactions: 2 (check details)  [View] │
│                                                  │
│  [Export] [Print] [Reconciliation Report]       │
│                                                  │
└─────────────────────────────────────────────────┘
```

---

## **VII. MODULE CÀI ĐẶT - SETTINGS**

### **26. Cài Đặt Hệ Thống**
**Người dùng:** Admin  
**Mục đích:** Cấu hình hệ thống (lưu trong `system_settings` table)

```
┌─────────────────────────────────────────────────┐
│  ⚙️ CÀI ĐẶT HỆ THỐNG                           │
├─────────────────────────────────────────────────┤
│                                                  │
│  Tabs: [General] [Inventory] [Loyalty]         │
│        [Payment] [Notification]                 │
│                                                  │
│  ─────── GENERAL TAB ───────                    │
│                                                  │
│  System Info:                                    │
│    System Name: [Hệ Thống Quản Lý Kho]         │
│    Timezone: [Asia/Ho_Chi_Minh ▼]              │
│    Currency: [VND ▼]                            │
│                                                  │
│  ─────── INVENTORY TAB ───────                  │
│                                                  │
│  Stock Alerts:                                   │
│    Low Stock %: [___20___] %                    │
│    Expiry Alert: [___30___] days before         │
│    Auto Approve Restock Under: [1,000,000] VNĐ │
│                                                  │
│  ─────── LOYALTY TAB ───────                    │
│                                                  │
│  Points Configuration:                           │
│    Points per 1000 VNĐ: [___1___] point        │
│    Points Expiry: [___12___] months             │
│    Min Points for Redemption: [___100___]       │
│                                                  │
│  ─────── PAYMENT TAB ───────                    │
│                                                  │
│  VNPay Settings:                                 │
│    Merchant ID: [DEMO________________]          │
│    Secret Key: [SECRETKEY123_________]          │
│    [ ] Enabled                                   │
│                                                  │
│  Momo Settings:                                  │
│    Partner Code: [MOMO_______________]          │
│    Access Key: [ACCESSKEY123_________]          │
│    Secret Key: [SECRETKEY789_________]          │
│    [ ] Enabled                                   │
│                                                  │
│  Payment Timeout: [___15___] minutes            │
│                                                  │
│  ─────── NOTIFICATION TAB ───────               │
│                                                  │
│  Notification Settings:                          │
│    Retention Period: [___30___] days            │
│    Email Notifications: [ ] Enabled             │
│    SMS Notifications: [ ] Enabled               │
│                                                  │
│  [Cancel] [Save All Settings]                   │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Chức năng:**
- ✅ **Database-driven Config**: Tất cả settings lưu trong DB (không hard-code)
- ✅ **Category Organization**: Inventory, Loyalty, Payment, Notification, System
- ✅ **Data Type Validation**: STRING, INT, DECIMAL, BOOLEAN, JSON
- ✅ **Hot Reload**: Thay đổi config không cần restart app

---

---

### **27. Quản Lý Thông Báo**
**Người dùng:** All Users  
**Mục đích:** Xem, quản lý notifications

```
┌─────────────────────────────────────────────────┐
│  🔔 THÔNG BÁO CỦA TÔI                           │
├─────────────────────────────────────────────────┤
│                                                  │
│  Filters:                                        │
│  [Type: All ▼] [Priority: All ▼] [Unread Only] │
│  [Mark All as Read]                              │
│                                                  │
│  List:                                           │
│  ┌──────────────────────────────────────────┐   │
│  │ 🔴 URGENT │ 2h ago                       │   │
│  │ Yêu cầu duyệt: RST-2024-002              │   │
│  │ Manager1 cần duyệt restock 150 units    │   │
│  │                                [View]    │   │
│  ├──────────────────────────────────────────┤   │
│  │ 🟠 HIGH │ 5h ago                         │   │
│  │ Sắp hết hàng: Sữa Vinamilk               │   │
│  │ Còn 15/30 units tại CH Thủ Đức          │   │
│  │                                [View]    │   │
│  ├──────────────────────────────────────────┤   │
│  │ 🟡 NORMAL │ 1d ago                       │   │
│  │ Hàng sắp hết hạn: Batch VNM-2024-001    │   │
│  │ Còn 30 ngày                              │   │
│  │                                [View]    │   │
│  └──────────────────────────────────────────┘   │
│                                                  │
│  Pagination: [<] 1 2 3 [>]                      │
│                                                  │
└─────────────────────────────────────────────────┘
```

**Actions:**
- Mark as read/unread
- Delete notification
- Jump to referenced entity (restock request, transfer, etc.)
- Auto-delete after 30 days (configurable)

---

## **📱 MOBILE APP (Optional - Future)**

### **28. Employee Mobile App**

**For Warehouse/Store Staff:**
- Quick barcode scanning
- Mobile inventory check
- Receive/transfer confirmation
- Damage reporting with photo upload

### **29. Customer Mobile App**

**For Loyalty Members:**
- View loyalty points & tier
- Browse rewards catalog
- Redeem points for rewards
- View transaction history
- Store locator

---

## **🎯 PRIORITY IMPLEMENTATION**

### **Phase 1 (Critical - Week 1-6):**
1. Dashboard
2. User Management
3. Product Management
4. Warehouse Management
5. Inventory Tracking
6. Transfer Management

### **Phase 2 (High - Week 7-12):**
7. Store Inventory
8. Restock Requests
9. Inventory Checks
10. Damage Reports
11. POS System
12. Payment Integration (VNPay/Momo)

### **Phase 3 (Medium - Week 13-17):**
13. Promotion Management
14. Voucher System
15. Loyalty Program
16. Rewards Catalog
17. Reports

### **Phase 4 (Optional):**
18. Advanced Analytics
19. Mobile Apps
20. Customer Portal

---

## **📊 TỔNG KẾT**

### **Tổng số màn hình:** 27 màn hình chính
### **Modules:** 7 modules
### **Timeline:** 17-18 tuần
### **Priority Screens:** 12 màn hình critical trong 6 tuần đầu

### **New Features Added:**
- ✅ **Workplace Assignment (Simple)**: Staff/Manager được gán vào 1 warehouse/store cố định
- ✅ **Notifications System**: Hệ thống thông báo real-time (low stock, expiring, approval)
- ✅ **System Settings**: Cấu hình database-driven (không hard-code), hot reload

### **Database:**
- **6 Databases:** IdentityDB (6 tables), ProductDB (2), InventoryDB (16), POSDB (3), PaymentDB (5), PromotionLoyaltyDB (12)
- **Total Tables:** 44 tables
- **Sample Data:** 100+ records with fixed UUIDs

**Hệ thống đầy đủ tính năng quản lý chuỗi cung ứng nội bộ với thanh toán hiện đại!**
