-- =====================================================
-- Inventory Management Service - COMPLETE
-- =====================================================
-- Database: InventoryDB
-- Purpose: Warehouse Management, Stock Control, Transfers
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InventoryDB')
BEGIN
    CREATE DATABASE InventoryDB;
END
GO

USE InventoryDB;
GO

-- =====================================================
-- Table: warehouses
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'warehouses')
BEGIN
    CREATE TABLE warehouses (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(255) NOT NULL,
        location NVARCHAR(500),
        capacity INT NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE
        parent_id UNIQUEIDENTIFIER NULL, -- Self-reference: sub-warehouse points to parent (kho tổng)
        is_deleted BIT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        created_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        CONSTRAINT FK_warehouses_parent FOREIGN KEY (parent_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_warehouses_name ON warehouses(name);
    CREATE INDEX IX_warehouses_status ON warehouses(status);
END
GO

-- =====================================================
-- Table: inventories
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventories')
BEGIN
    CREATE TABLE inventories (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        location_type NVARCHAR(50) NOT NULL, -- WAREHOUSE | STORE
        location_id UNIQUEIDENTIFIER NOT NULL, -- warehouse_id or store_id
        quantity INT NOT NULL DEFAULT 0,
        reserved_quantity INT NOT NULL DEFAULT 0, -- Đang được reserve
        available_quantity AS (quantity - reserved_quantity) PERSISTED,
        min_stock_level INT DEFAULT 10, -- Threshold cảnh báo hết hàng
        max_stock_level INT DEFAULT 1000,
        last_stock_check DATETIME2,
        updated_at DATETIME2 DEFAULT GETUTCDATE(),
        CONSTRAINT CHK_inventory_quantity CHECK (quantity >= 0),
        CONSTRAINT CHK_inventory_reserved CHECK (reserved_quantity >= 0),
        CONSTRAINT UQ_inventory_product_location UNIQUE (product_id, location_type, location_id)
    );
    
    CREATE INDEX IX_inventories_product_id ON inventories(product_id);
    CREATE INDEX IX_inventories_location ON inventories(location_type, location_id);
    -- Removed filtered index: Cannot compare columns in WHERE clause
    -- CREATE INDEX IX_inventories_low_stock ON inventories(product_id) WHERE available_quantity <= min_stock_level;
END
GO

-- =====================================================
-- Table: product_batches
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'product_batches')
BEGIN
    CREATE TABLE product_batches (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        batch_number NVARCHAR(100) NOT NULL,
        quantity INT NOT NULL,
        manufacturing_date DATE,
        expiry_date DATE,
        supplier NVARCHAR(255),
        supplier_id UNIQUEIDENTIFIER, -- ProductDB.suppliers.id (logical ref)
        received_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        status NVARCHAR(50) NOT NULL DEFAULT 'AVAILABLE', -- AVAILABLE | SOLD | EXPIRED | DAMAGED
        CONSTRAINT FK_batches_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id),
        CONSTRAINT UQ_batch_number UNIQUE (batch_number)
    );
    
    CREATE INDEX IX_batches_product_id ON product_batches(product_id);
    CREATE INDEX IX_batches_warehouse_id ON product_batches(warehouse_id);
    CREATE INDEX IX_batches_expiry_date ON product_batches(expiry_date);
    CREATE INDEX IX_batches_status ON product_batches(status);
END
GO

-- Handle existing product_batches table missing columns (migration)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'product_batches')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('product_batches') AND name = 'supplier_id')
        ALTER TABLE product_batches ADD supplier_id UNIQUEIDENTIFIER;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('product_batches') AND name = 'restock_request_id')
        ALTER TABLE product_batches DROP COLUMN restock_request_id;
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('product_batches') AND name = 'purchase_order_id')
    BEGIN
        -- Rename old purchase_order_id to restock_request_id if it exists and restock_request_id doesn't
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('product_batches') AND name = 'restock_request_id')
            EXEC sp_rename 'product_batches.purchase_order_id', 'restock_request_id', 'COLUMN';
    END
END
GO

-- =====================================================
-- Table: stock_movements
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'stock_movements')
BEGIN
    CREATE TABLE stock_movements (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        movement_number NVARCHAR(50) NOT NULL UNIQUE, -- SM-2024-001
        movement_type NVARCHAR(50) NOT NULL, -- INBOUND | OUTBOUND | TRANSFER | ADJUSTMENT
        location_id UNIQUEIDENTIFIER NOT NULL, -- warehouse_id or store_id
        location_type NVARCHAR(50) NOT NULL, -- WAREHOUSE | STORE
        movement_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        restock_request_id UNIQUEIDENTIFIER, -- restock_requests.id
        supplier_name NVARCHAR(255), -- For INBOUND
        transfer_id UNIQUEIDENTIFIER, -- transfers.id (logical ref, no FK to avoid ordering issue)
        received_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        status NVARCHAR(50) NOT NULL DEFAULT 'COMPLETED', -- PENDING | COMPLETED | CANCELLED
        notes NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_movements_movement_number ON stock_movements(movement_number);
    CREATE INDEX IX_movements_type ON stock_movements(movement_type);
    CREATE INDEX IX_movements_location ON stock_movements(location_type, location_id);
    CREATE INDEX IX_movements_date ON stock_movements(movement_date);
END
GO

-- Handle existing stock_movements table missing columns (migration)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'stock_movements')
BEGIN
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('stock_movements') AND name = 'supplier')
        EXEC sp_rename 'stock_movements.supplier', 'supplier_name', 'COLUMN';
    -- Drop purchase_order_id if it exists (no longer used)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('stock_movements') AND name = 'purchase_order_id')
        ALTER TABLE stock_movements DROP COLUMN purchase_order_id;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('stock_movements') AND name = 'transfer_id')
        ALTER TABLE stock_movements ADD transfer_id UNIQUEIDENTIFIER;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('stock_movements') AND name = 'restock_request_id')
        ALTER TABLE stock_movements ADD restock_request_id UNIQUEIDENTIFIER;
END
GO

-- Migration: add restock_request_id to transfers if missing
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'transfers')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('transfers') AND name = 'restock_request_id')
        ALTER TABLE transfers ADD restock_request_id UNIQUEIDENTIFIER;
    -- Drop batch_id from stock_movement_items if exists (no longer used)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('stock_movement_items') AND name = 'batch_id')
    BEGIN
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_movement_items_batches')
            ALTER TABLE stock_movement_items DROP CONSTRAINT FK_movement_items_batches;
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_movement_items_batch_id' AND object_id = OBJECT_ID('stock_movement_items'))
            DROP INDEX IX_movement_items_batch_id ON stock_movement_items;
        ALTER TABLE stock_movement_items DROP COLUMN batch_id;
    END
END
GO

-- =====================================================
-- Table: stock_movement_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'stock_movement_items')
BEGIN
    CREATE TABLE stock_movement_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        movement_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        quantity INT NOT NULL,
        unit_price DECIMAL(18,2), -- For valuation
        CONSTRAINT FK_movement_items_movements FOREIGN KEY (movement_id) REFERENCES stock_movements(id)
    );
    
    CREATE INDEX IX_movement_items_movement_id ON stock_movement_items(movement_id);
    CREATE INDEX IX_movement_items_product_id ON stock_movement_items(product_id);
END
GO

-- =====================================================
-- Table: inventory_history
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_history')
BEGIN
    CREATE TABLE inventory_history (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        inventory_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL,
        location_type NVARCHAR(50) NOT NULL,
        location_id UNIQUEIDENTIFIER NOT NULL,
        snapshot_date DATE NOT NULL,
        quantity INT NOT NULL,
        reserved_quantity INT NOT NULL,
        available_quantity INT NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_history_inventory_id ON inventory_history(inventory_id);
    CREATE INDEX IX_history_snapshot_date ON inventory_history(snapshot_date);
END
GO

-- =====================================================
-- Table: inventory_logs
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_logs')
BEGIN
    CREATE TABLE inventory_logs (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        inventory_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL,
        action NVARCHAR(100) NOT NULL, -- ADJUST | RECEIVE | TRANSFER | SALE | DAMAGE
        old_quantity INT NOT NULL,
        new_quantity INT NOT NULL,
        quantity_change AS (new_quantity - old_quantity) PERSISTED,
        reason NVARCHAR(500),
        performed_by UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        performed_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_logs_inventory_id ON inventory_logs(inventory_id);
    CREATE INDEX IX_logs_product_id ON inventory_logs(product_id);
    CREATE INDEX IX_logs_action ON inventory_logs(action);
    CREATE INDEX IX_logs_performed_at ON inventory_logs(performed_at);
END
GO

-- =====================================================
-- Table: transfers
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'transfers')
BEGIN
    CREATE TABLE transfers (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        transfer_number NVARCHAR(50) NOT NULL UNIQUE, -- TRF-2024-001
        from_location_type NVARCHAR(50) NOT NULL, -- WAREHOUSE | STORE
        from_location_id UNIQUEIDENTIFIER NOT NULL,
        to_location_type NVARCHAR(50) NOT NULL, -- WAREHOUSE | STORE
        to_location_id UNIQUEIDENTIFIER NOT NULL,
        transfer_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        expected_delivery DATETIME2,
        actual_delivery DATETIME2,
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | IN_TRANSIT | DELIVERED | CANCELLED
        shipped_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        received_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        notes NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        restock_request_id UNIQUEIDENTIFIER -- restock_requests.id (logical ref)
    );
    
    CREATE INDEX IX_transfers_transfer_number ON transfers(transfer_number);
    CREATE INDEX IX_transfers_from_location ON transfers(from_location_type, from_location_id);
    CREATE INDEX IX_transfers_to_location ON transfers(to_location_type, to_location_id);
    CREATE INDEX IX_transfers_status ON transfers(status);
    CREATE INDEX IX_transfers_date ON transfers(transfer_date);
END
GO

-- =====================================================
-- Table: transfer_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'transfer_items')
BEGIN
    CREATE TABLE transfer_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        transfer_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        batch_id UNIQUEIDENTIFIER, -- product_batches.id
        requested_quantity INT NOT NULL,
        shipped_quantity INT,
        received_quantity INT,
        damaged_quantity INT DEFAULT 0,
        notes NVARCHAR(500),
        CONSTRAINT FK_transfer_items_transfers FOREIGN KEY (transfer_id) REFERENCES transfers(id),
        CONSTRAINT FK_transfer_items_batches FOREIGN KEY (batch_id) REFERENCES product_batches(id)
    );
    
    CREATE INDEX IX_transfer_items_transfer_id ON transfer_items(transfer_id);
    CREATE INDEX IX_transfer_items_product_id ON transfer_items(product_id);
    CREATE INDEX IX_transfer_items_batch_id ON transfer_items(batch_id);
END
GO

-- =====================================================
-- Table: restock_requests
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'restock_requests')
BEGIN
    CREATE TABLE restock_requests (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        request_number NVARCHAR(50) NOT NULL UNIQUE,        -- RST-2024-001
        -- Hierarchy flow:
        --   Store Manager    → from=branch_warehouse, to=store,         to_type=STORE
        --   Warehouse Manager→ from=kho_tong,         to=branch,        to_type=WAREHOUSE
        --   Warehouse Admin  → from=NULL (supplier),  to=kho_tong,      to_type=WAREHOUSE
        from_warehouse_id UNIQUEIDENTIFIER NULL,            -- source of goods (NULL = external supplier)
        from_location_type NVARCHAR(20) NOT NULL DEFAULT 'WAREHOUSE', -- WAREHOUSE | STORE
        to_warehouse_id   UNIQUEIDENTIFIER NULL,            -- destination / requester location (parent or child warehouse)
        to_location_type  NVARCHAR(20) NOT NULL DEFAULT 'WAREHOUSE',   -- WAREHOUSE | STORE
        requested_by UNIQUEIDENTIFIER NOT NULL,             -- IdentityDB.users.id
        requested_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        priority NVARCHAR(50) NOT NULL DEFAULT 'NORMAL',    -- NORMAL | HIGH | URGENT
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING',     -- PENDING | APPROVED | PROCESSING | COMPLETED | REJECTED
        approved_by UNIQUEIDENTIFIER,
        approved_date DATETIME2,
        transfer_id UNIQUEIDENTIFIER,
        notes NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        CONSTRAINT FK_restock_transfers FOREIGN KEY (transfer_id) REFERENCES transfers(id)
    );
    
    CREATE INDEX IX_restock_request_number ON restock_requests(request_number);
    CREATE INDEX IX_restock_from_warehouse ON restock_requests(from_warehouse_id);
    CREATE INDEX IX_restock_to_warehouse   ON restock_requests(to_warehouse_id);
    CREATE INDEX IX_restock_status ON restock_requests(status);
    CREATE INDEX IX_restock_priority ON restock_requests(priority);
END
GO

-- =====================================================
-- Table: restock_request_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'restock_request_items')
BEGIN
    CREATE TABLE restock_request_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        request_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        requested_quantity INT NOT NULL,
        current_quantity INT NOT NULL, -- Stock at time of request
        approved_quantity INT,
        reason NVARCHAR(500),
        CONSTRAINT FK_restock_items_requests FOREIGN KEY (request_id) REFERENCES restock_requests(id)
    );
    
    CREATE INDEX IX_restock_items_request_id ON restock_request_items(request_id);
    CREATE INDEX IX_restock_items_product_id ON restock_request_items(product_id);
END
GO

-- =====================================================
-- Table: damage_reports
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'damage_reports')
BEGIN
    CREATE TABLE damage_reports (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        report_number NVARCHAR(50) NOT NULL UNIQUE, -- DMG-2024-001
        location_type NVARCHAR(50) NOT NULL, -- WAREHOUSE | STORE
        location_id UNIQUEIDENTIFIER NOT NULL,
        damage_type NVARCHAR(50) NOT NULL, -- EXPIRED | PHYSICAL_DAMAGE | QUALITY_ISSUE | THEFT | OTHER
        reported_by UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        reported_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        total_value DECIMAL(18,2),
        description NVARCHAR(MAX),
        photos NVARCHAR(MAX), -- JSON array of image URLs
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | APPROVED | REJECTED
        approved_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        approved_date DATETIME2,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_damage_report_number ON damage_reports(report_number);
    CREATE INDEX IX_damage_location ON damage_reports(location_type, location_id);
    CREATE INDEX IX_damage_type ON damage_reports(damage_type);
    CREATE INDEX IX_damage_status ON damage_reports(status);
END
GO

-- =====================================================
-- Table: inventory_checks
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_checks')
BEGIN
    CREATE TABLE inventory_checks (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        check_number NVARCHAR(50) NOT NULL UNIQUE, -- IC-2024-001
        location_type NVARCHAR(50) NOT NULL, -- WAREHOUSE | STORE
        location_id UNIQUEIDENTIFIER NOT NULL,
        check_type NVARCHAR(50) NOT NULL, -- FULL | PARTIAL | SPOT
        check_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        checked_by UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | COMPLETED
        total_discrepancies INT DEFAULT 0,
        notes NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_check_number ON inventory_checks(check_number);
    CREATE INDEX IX_check_location ON inventory_checks(location_type, location_id);
    CREATE INDEX IX_check_status ON inventory_checks(status);
END
GO

-- =====================================================
-- Table: inventory_check_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_check_items')
BEGIN
    CREATE TABLE inventory_check_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        check_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        system_quantity INT NOT NULL,
        actual_quantity INT NOT NULL,
        difference AS (actual_quantity - system_quantity) PERSISTED,
        note NVARCHAR(500),
        CONSTRAINT FK_check_items_checks FOREIGN KEY (check_id) REFERENCES inventory_checks(id)
    );
    
    CREATE INDEX IX_check_items_check_id ON inventory_check_items(check_id);
    CREATE INDEX IX_check_items_product_id ON inventory_check_items(product_id);
    -- Removed filtered index: Cannot use expression in WHERE clause
    -- CREATE INDEX IX_check_items_difference ON inventory_check_items(product_id) WHERE (actual_quantity - system_quantity) <> 0;
END
GO

-- =====================================================
-- Table: store_receiving_logs
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'store_receiving_logs')
BEGIN
    CREATE TABLE store_receiving_logs (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        transfer_id UNIQUEIDENTIFIER NOT NULL,
        store_id UNIQUEIDENTIFIER NOT NULL,
        received_by UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        received_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        condition_status NVARCHAR(50) NOT NULL, -- GOOD | DAMAGED | PARTIAL
        notes NVARCHAR(MAX),
        photos NVARCHAR(MAX), -- JSON array of image URLs
        CONSTRAINT FK_receiving_transfers FOREIGN KEY (transfer_id) REFERENCES transfers(id)
    );
    
    CREATE INDEX IX_receiving_transfer_id ON store_receiving_logs(transfer_id);
    CREATE INDEX IX_receiving_store_id ON store_receiving_logs(store_id);
END
GO

-- =====================================================
-- Insert sample warehouses (per-row check để idempotent)
-- =====================================================
IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'A0000001-0001-0001-0001-000000000001')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('A0000001-0001-0001-0001-000000000001', N'Kho Tổng HCM', N'Quận Thủ Đức, TP. Hồ Chí Minh', 120, 'ACTIVE', NULL,
     '11111111-1111-1111-1111-111111111111', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'A0000001-0001-0001-0001-000000000002')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('A0000001-0001-0001-0001-000000000002', N'Kho Chi Nhánh Quận 12', N'Quận 12, TP. Hồ Chí Minh', 100, 'ACTIVE', 'A0000001-0001-0001-0001-000000000001',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'A0000001-0001-0001-0001-000000000003')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('A0000001-0001-0001-0001-000000000003', N'Kho Chi Nhánh Bình Dương', N'Thành phố Thủ Dầu Một, Bình Dương', 80, 'ACTIVE', 'A0000001-0001-0001-0001-000000000001',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'A0000001-0001-0001-0001-000000000004')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('A0000001-0001-0001-0001-000000000004', N'Kho Chi Nhánh Long An', N'Thị xã Tân An, Long An', 60, 'ACTIVE', 'A0000001-0001-0001-0001-000000000001',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'B0000001-0001-0001-0001-000000000001')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('B0000001-0001-0001-0001-000000000001', N'Cửa Hàng Thủ Đức', N'123 Lê Văn Việt, Quận 9, HCM', 50, 'ACTIVE', 'A0000001-0001-0001-0001-000000000002',
     '22222222-2222-2222-2222-222222222221', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'B0000001-0001-0001-0001-000000000002')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('B0000001-0001-0001-0001-000000000002', N'Cửa Hàng Quận 1', N'456 Nguyễn Huệ, Quận 1, HCM', 40, 'ACTIVE', 'A0000001-0001-0001-0001-000000000002',
     '22222222-2222-2222-2222-222222222221', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'B0000001-0001-0001-0001-000000000003')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('B0000001-0001-0001-0001-000000000003', N'Cửa Hàng Bình Dương', N'78 Đại lộ Bình Dương, Thủ Dầu Một, Bình Dương', 45, 'ACTIVE', 'A0000001-0001-0001-0001-000000000003',
     '22222222-2222-2222-2222-222222222221', GETUTCDATE());

IF NOT EXISTS (SELECT * FROM warehouses WHERE id = 'B0000001-0001-0001-0001-000000000004')
    INSERT INTO warehouses (id, name, location, capacity, status, parent_id, created_by, created_at) VALUES
    ('B0000001-0001-0001-0001-000000000004', N'Cửa Hàng Long An', N'12 Nguyễn Huệ, Tân An, Long An', 35, 'ACTIVE', 'A0000001-0001-0001-0001-000000000004',
     '22222222-2222-2222-2222-222222222221', GETUTCDATE());
GO

-- =====================================================
-- Migration: Add parent_id column to warehouses if missing
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('warehouses') AND name = 'parent_id')
BEGIN
    ALTER TABLE warehouses ADD parent_id UNIQUEIDENTIFIER NULL;
    ALTER TABLE warehouses ADD CONSTRAINT FK_warehouses_parent FOREIGN KEY (parent_id) REFERENCES warehouses(id);
END
GO

-- =====================================================
-- Insert sample inventories (per-row check để idempotent)
-- =====================================================
IF NOT EXISTS (SELECT * FROM inventories)
BEGIN
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level) VALUES
    -- Warehouse inventory
    (NEWID(), 'F0000001-0001-0001-0001-000000000001', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 500, 0, 50, 1000), -- Rau Muống
    (NEWID(), 'F0000001-0001-0001-0001-000000000002', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 300, 0, 30, 800), -- Cải Thảo
    (NEWID(), 'F0000001-0001-0001-0001-000000000003', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 800, 0, 100, 2000), -- Cam Sành
    (NEWID(), 'F0000001-0001-0001-0001-000000000005', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 1000, 0, 200, 3000), -- Sữa Vinamilk
    (NEWID(), 'F0000001-0001-0001-0001-000000000007', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 200, 0, 20, 500), -- Gạo ST25
    
    -- Store inventory (Cửa Hàng Thủ Đức)
    (NEWID(), 'F0000001-0001-0001-0001-000000000001', 'STORE', 'B0000001-0001-0001-0001-000000000001', 50, 0, 10, 100), -- Rau Muống
    (NEWID(), 'F0000001-0001-0001-0001-000000000003', 'STORE', 'B0000001-0001-0001-0001-000000000001', 80, 0, 20, 150), -- Cam Sành
    (NEWID(), 'F0000001-0001-0001-0001-000000000005', 'STORE', 'B0000001-0001-0001-0001-000000000001', 100, 0, 30, 200), -- Sữa Vinamilk
    (NEWID(), 'F0000001-0001-0001-0001-000000000007', 'STORE', 'B0000001-0001-0001-0001-000000000001', 15, 0, 5, 50); -- Gạo ST25
END
GO

-- =====================================================
-- Insert sample product batches
-- =====================================================
IF NOT EXISTS (SELECT * FROM product_batches)
BEGIN
    INSERT INTO product_batches (id, product_id, warehouse_id, batch_number, quantity, manufacturing_date, expiry_date, supplier, received_at, status) VALUES
    ('BA000001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000005', 'A0000001-0001-0001-0001-000000000001',
     'VNM-2024-001', 500, '2024-02-01', '2024-08-01', 'Vinamilk Co.', '2024-02-05', 'AVAILABLE'),
    ('BA000001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000006', 'A0000001-0001-0001-0001-000000000001',
     'TH-2024-001', 500, '2024-02-01', '2024-08-01', 'TH True Milk Co.', '2024-02-05', 'AVAILABLE'),
    ('BA000001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000007', 'A0000001-0001-0001-0001-000000000001',
     'ST25-2024-001', 200, '2024-01-15', '2025-01-15', 'ST25 Co.', '2024-01-20', 'AVAILABLE');
END
GO

-- =====================================================
-- Insert sample stock movements (receiving goods)
-- =====================================================
IF NOT EXISTS (SELECT * FROM stock_movements)
BEGIN
    INSERT INTO stock_movements (id, movement_number, movement_type, location_id, location_type, movement_date, supplier_name, received_by, status, notes) VALUES
    ('CA000001-0001-0001-0001-000000000001', 'SM-2024-001', 'INBOUND', 'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     '2024-02-05', 'Vinamilk Co.', '44444444-4444-4444-4444-444444444441', 'COMPLETED', N'Received 500 units of milk'),
    ('CA000001-0001-0001-0001-000000000002', 'SM-2024-002', 'INBOUND', 'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     '2024-02-10', N'Đà Lḛt Suppliers', '44444444-4444-4444-4444-444444444441', 'COMPLETED', N'Received fresh vegetables');
END
GO

-- =====================================================
-- Insert sample transfers (warehouse → store)
-- =====================================================
IF NOT EXISTS (SELECT * FROM transfers)
BEGIN
    INSERT INTO transfers (id, transfer_number, from_location_type, from_location_id, to_location_type, to_location_id, 
                           transfer_date, expected_delivery, status, shipped_by, notes) VALUES
    ('DA000001-0001-0001-0001-000000000001', 'TRF-2024-001', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 
     'STORE', 'B0000001-0001-0001-0001-000000000001', '2024-03-01', '2024-03-02', 'DELIVERED', 
     '44444444-4444-4444-4444-444444444441', N'Regular restock for Thủ Đức store');
END
GO

-- Insert transfer items
IF NOT EXISTS (SELECT * FROM transfer_items)
BEGIN
    DECLARE @TransferId UNIQUEIDENTIFIER = 'DA000001-0001-0001-0001-000000000001';
    INSERT INTO transfer_items (id, transfer_id, product_id, batch_id, requested_quantity, shipped_quantity, received_quantity, damaged_quantity) VALUES
    (NEWID(), @TransferId, 'F0000001-0001-0001-0001-000000000005', 'BA000001-0001-0001-0001-000000000001', 100, 100, 98, 2),
    (NEWID(), @TransferId, 'F0000001-0001-0001-0001-000000000007', 'BA000001-0001-0001-0001-000000000003', 20, 20, 20, 0);
END
GO

-- =====================================================
-- Insert sample restock requests
-- =====================================================
-- Migration: rename old columns if existing table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('restock_requests') AND name = 'store_id')
BEGIN
    EXEC sp_rename 'restock_requests.store_id',    'to_warehouse_id',   'COLUMN';
    EXEC sp_rename 'restock_requests.warehouse_id', 'from_warehouse_id', 'COLUMN';
END
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('restock_requests') AND name = 'from_location_type')
    ALTER TABLE restock_requests ADD from_location_type NVARCHAR(20) NOT NULL DEFAULT 'WAREHOUSE';
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('restock_requests') AND name = 'to_location_type')
    ALTER TABLE restock_requests ADD to_location_type NVARCHAR(20) NOT NULL DEFAULT 'WAREHOUSE';
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_restock_from_warehouse' AND object_id = OBJECT_ID('restock_requests'))
    CREATE INDEX IX_restock_from_warehouse ON restock_requests(from_warehouse_id);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_restock_to_warehouse' AND object_id = OBJECT_ID('restock_requests'))
    CREATE INDEX IX_restock_to_warehouse ON restock_requests(to_warehouse_id);
GO

-- =====================================================
-- Insert sample restock requests
-- =====================================================
IF NOT EXISTS (SELECT * FROM restock_requests)
BEGIN
    -- RST-2024-001: Store Manager (Thủ Đức) → Warehouse Manager (Kho Miền Bắc branch)
    -- Goods come FROM branch A...002  TO store B...001
    INSERT INTO restock_requests (id, request_number, from_warehouse_id, from_location_type, to_warehouse_id, to_location_type, requested_by, requested_date, priority, status, transfer_id, notes) VALUES
    ('EA000001-0001-0001-0001-000000000001', 'RST-2024-001',
     'A0000001-0001-0001-0001-000000000002', 'WAREHOUSE',
     'B0000001-0001-0001-0001-000000000001', 'STORE',
     '33333333-3333-3333-3333-333333333331', '2024-03-01', 'NORMAL', 'COMPLETED',
     'DA000001-0001-0001-0001-000000000003', N'Weekly restock'),
    -- RST-2024-002: Store Manager (Thủ Đức) → Warehouse Manager (urgent)
    ('EA000001-0001-0001-0001-000000000002', 'RST-2024-002',
     'A0000001-0001-0001-0001-000000000002', 'WAREHOUSE',
     'B0000001-0001-0001-0001-000000000001', 'STORE',
     '33333333-3333-3333-3333-333333333331', '2024-03-03', 'HIGH', 'PENDING',
     NULL, N'Urgent: Low stock on milk');
END
GO

-- Insert restock request items
IF NOT EXISTS (SELECT * FROM restock_request_items)
BEGIN
    DECLARE @RestockId1 UNIQUEIDENTIFIER = 'EA000001-0001-0001-0001-000000000001';
    DECLARE @RestockId2 UNIQUEIDENTIFIER = 'EA000001-0001-0001-0001-000000000002';
    
    INSERT INTO restock_request_items (id, request_id, product_id, requested_quantity, current_quantity, approved_quantity, reason) VALUES
    (NEWID(), @RestockId1, 'F0000001-0001-0001-0001-000000000005', 100, 25, 100, N'Low stock'),
    (NEWID(), @RestockId1, 'F0000001-0001-0001-0001-000000000007', 20, 5, 20, N'Low stock'),
    (NEWID(), @RestockId2, 'F0000001-0001-0001-0001-000000000005', 150, 15, NULL, N'Urgent - below threshold');
END
GO

-- =====================================================
-- Inventories bổ sung cho 4 địa điểm hiện có
-- Profile tồn kho mỗi nơi khác nhau rõ ràng:
--   Kho Tổng HCM  (A...001): số lượng lớn, main warehouse
--   Kho Miền Bắc  (A...002): số lượng vừa, có reserved
--   CH Thủ Đức    (B...001): store bình thường, vài SKU sắp hết
--   CH Quận 1     (B...002): store lớn hơn, reserved nhiều hơn
-- =====================================================

-- Kho Tổng HCM: bổ sung F006 (TH True Milk) hiện chưa có
IF NOT EXISTS (SELECT * FROM inventories WHERE product_id = 'F0000001-0001-0001-0001-000000000006'
               AND location_id = 'A0000001-0001-0001-0001-000000000001')
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level)
    VALUES (NEWID(), 'F0000001-0001-0001-0001-000000000006', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 700,  50, 100, 1500);
GO

-- Kho Miền Bắc: đầy đủ sản phẩm, tồn kho vừa phải
IF NOT EXISTS (SELECT * FROM inventories WHERE location_id = 'A0000001-0001-0001-0001-000000000002')
BEGIN
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level) VALUES
    (NEWID(), 'F0000001-0001-0001-0001-000000000001', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 150,   0,  30,  400),
    (NEWID(), 'F0000001-0001-0001-0001-000000000002', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 120,   0,  20,  300),
    (NEWID(), 'F0000001-0001-0001-0001-000000000003', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 180,   0,  25,  500),
    (NEWID(), 'F0000001-0001-0001-0001-000000000005', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 400,  80, 100, 1000), -- reserved 80 (đang chờ transfer)
    (NEWID(), 'F0000001-0001-0001-0001-000000000006', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 350,   0,  80,  800),
    (NEWID(), 'F0000001-0001-0001-0001-000000000007', 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 120,  20,  30,  400);
END
GO

-- CH Thủ Đức: bổ sung F002, F006 còn thiếu
IF NOT EXISTS (SELECT * FROM inventories WHERE product_id = 'F0000001-0001-0001-0001-000000000002'
               AND location_id = 'B0000001-0001-0001-0001-000000000001')
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level)
    VALUES (NEWID(), 'F0000001-0001-0001-0001-000000000002', 'STORE', 'B0000001-0001-0001-0001-000000000001', 22, 0, 8, 60);
GO
IF NOT EXISTS (SELECT * FROM inventories WHERE product_id = 'F0000001-0001-0001-0001-000000000006'
               AND location_id = 'B0000001-0001-0001-0001-000000000001')
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level)
    VALUES (NEWID(), 'F0000001-0001-0001-0001-000000000006', 'STORE', 'B0000001-0001-0001-0001-000000000001',  9, 0, 15, 80); -- ⚠ LOW STOCK
GO

-- CH Quận 1: đầy đủ sản phẩm, level cao hơn Thủ Đức, có reservation online
IF NOT EXISTS (SELECT * FROM inventories WHERE location_id = 'B0000001-0001-0001-0001-000000000002')
BEGIN
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level) VALUES
    (NEWID(), 'F0000001-0001-0001-0001-000000000001', 'STORE', 'B0000001-0001-0001-0001-000000000002',  45,  5,  10,  100),
    (NEWID(), 'F0000001-0001-0001-0001-000000000002', 'STORE', 'B0000001-0001-0001-0001-000000000002',  38,  0,   8,   80),
    (NEWID(), 'F0000001-0001-0001-0001-000000000003', 'STORE', 'B0000001-0001-0001-0001-000000000002',  72,  0,  15,  150),
    (NEWID(), 'F0000001-0001-0001-0001-000000000005', 'STORE', 'B0000001-0001-0001-0001-000000000002',  95, 20,  20,  180), -- reserved 20 (đơn online)
    (NEWID(), 'F0000001-0001-0001-0001-000000000006', 'STORE', 'B0000001-0001-0001-0001-000000000002',  65, 10,  15,  120),
    (NEWID(), 'F0000001-0001-0001-0001-000000000007', 'STORE', 'B0000001-0001-0001-0001-000000000002',  28,  0,   5,   60);
END
GO

-- CH Bình Dương: tồn kho đủ dùng
IF NOT EXISTS (SELECT * FROM inventories WHERE location_id = 'B0000001-0001-0001-0001-000000000003')
BEGIN
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level) VALUES
    (NEWID(), 'F0000001-0001-0001-0001-000000000001', 'STORE', 'B0000001-0001-0001-0001-000000000003',  40,  0,  10,   90),
    (NEWID(), 'F0000001-0001-0001-0001-000000000002', 'STORE', 'B0000001-0001-0001-0001-000000000003',  30,  0,   8,   70),
    (NEWID(), 'F0000001-0001-0001-0001-000000000003', 'STORE', 'B0000001-0001-0001-0001-000000000003',  55,  0,  12,  120),
    (NEWID(), 'F0000001-0001-0001-0001-000000000005', 'STORE', 'B0000001-0001-0001-0001-000000000003',  70, 10,  20,  150),
    (NEWID(), 'F0000001-0001-0001-0001-000000000006', 'STORE', 'B0000001-0001-0001-0001-000000000003',  50,  0,  15,  110),
    (NEWID(), 'F0000001-0001-0001-0001-000000000007', 'STORE', 'B0000001-0001-0001-0001-000000000003',  20,  0,   5,   50);
END
GO

-- CH Long An: tồn kho nhỏ hơn, nhu cầu thấp
IF NOT EXISTS (SELECT * FROM inventories WHERE location_id = 'B0000001-0001-0001-0001-000000000004')
BEGIN
    INSERT INTO inventories (id, product_id, location_type, location_id, quantity, reserved_quantity, min_stock_level, max_stock_level) VALUES
    (NEWID(), 'F0000001-0001-0001-0001-000000000001', 'STORE', 'B0000001-0001-0001-0001-000000000004',  25,  0,   8,   60),
    (NEWID(), 'F0000001-0001-0001-0001-000000000002', 'STORE', 'B0000001-0001-0001-0001-000000000004',  18,  0,   6,   50),
    (NEWID(), 'F0000001-0001-0001-0001-000000000003', 'STORE', 'B0000001-0001-0001-0001-000000000004',  35,  0,  10,   80),
    (NEWID(), 'F0000001-0001-0001-0001-000000000005', 'STORE', 'B0000001-0001-0001-0001-000000000004',  45,  5,  15,  100),
    (NEWID(), 'F0000001-0001-0001-0001-000000000006', 'STORE', 'B0000001-0001-0001-0001-000000000004',  12,  0,  10,   70), -- ⚠ gần ngưỡng tối thiểu
    (NEWID(), 'F0000001-0001-0001-0001-000000000007', 'STORE', 'B0000001-0001-0001-0001-000000000004',  10,  0,   4,   35);
END
GO

-- =====================================================
-- Thêm product batches
-- =====================================================
IF NOT EXISTS (SELECT * FROM product_batches WHERE id = 'BA000001-0001-0001-0001-000000000004')
BEGIN
    INSERT INTO product_batches
        (id, product_id, warehouse_id, batch_number, quantity,
         manufacturing_date, expiry_date, supplier, supplier_id, received_at, status)
    VALUES
    -- Kho Chi Nhánh Miền Bắc: Vinamilk batch 2
    ('BA000001-0001-0001-0001-000000000004',
     'F0000001-0001-0001-0001-000000000005', 'A0000001-0001-0001-0001-000000000002',
     'VNM-2024-002', 400, '2024-03-01', '2024-09-01',
     'Vinamilk Co.', '50000001-0001-0001-0001-000000000001', '2024-03-05', 'AVAILABLE'),

    -- Kho Chi Nhánh Miền Bắc: TH True Milk batch 2
    ('BA000001-0001-0001-0001-000000000005',
     'F0000001-0001-0001-0001-000000000006', 'A0000001-0001-0001-0001-000000000002',
     'TH-2024-002', 350, '2024-03-10', '2024-09-10',
     'TH True Milk Co.', '50000001-0001-0001-0001-000000000002', '2024-03-15', 'AVAILABLE'),

    -- Kho Tổng HCM: Gạo ST25 batch 2
    ('BA000001-0001-0001-0001-000000000006',
     'F0000001-0001-0001-0001-000000000007', 'A0000001-0001-0001-0001-000000000001',
     'ST25-2024-002', 80, '2024-03-05', '2025-03-05',
     'ST25 Co.', '50000001-0001-0001-0001-000000000003', '2024-03-10', 'AVAILABLE'),

    -- Kho Tổng HCM: Vinamilk batch 3 (sắp hết hạn, cảnh báo)
    ('BA000001-0001-0001-0001-000000000007',
     'F0000001-0001-0001-0001-000000000005', 'A0000001-0001-0001-0001-000000000001',
     'VNM-2024-003', 150, '2024-01-15', '2024-07-15',
     'Vinamilk Co.', '50000001-0001-0001-0001-000000000001', '2024-01-20', 'AVAILABLE');
END
GO

-- =====================================================
-- Thêm transfers (phải insert TRƯỚC stock_movements)
-- =====================================================
IF NOT EXISTS (SELECT * FROM transfers WHERE id = 'DA000001-0001-0001-0001-000000000002')
BEGIN
    INSERT INTO transfers
        (id, transfer_number, from_location_type, from_location_id,
         to_location_type, to_location_id,
         transfer_date, expected_delivery, actual_delivery, status,
         shipped_by, received_by, notes)
    VALUES
    -- Kho Tổng HCM → Cửa Hàng Quận 1 (DELIVERED)
    ('DA000001-0001-0001-0001-000000000002', 'TRF-2024-002',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001',
     'STORE',     'B0000001-0001-0001-0001-000000000002',
     '2024-03-20', '2024-03-21', '2024-03-21', 'DELIVERED',
     '44444444-4444-4444-4444-444444444441', '33333333-3333-3333-3333-333333333331',
     N'Nhập hàng lần đầu cho Cửa Hàng Quận 1'),

    -- Kho Miền Bắc → Cửa Hàng Thủ Đức (DELIVERED)
    ('DA000001-0001-0001-0001-000000000003', 'TRF-2024-003',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002',
     'STORE',     'B0000001-0001-0001-0001-000000000001',
     '2024-03-22', '2024-03-23', '2024-03-23', 'DELIVERED',
     '44444444-4444-4444-4444-444444444441', '33333333-3333-3333-3333-333333333331',
     N'Bổ sung hàng từ Kho Miền Bắc cho Cửa Hàng Thủ Đức'),

    -- Kho Tổng HCM → Kho Miền Bắc (warehouse-to-warehouse, đang vận chuyển)
    ('DA000001-0001-0001-0001-000000000004', 'TRF-2024-004',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002',
     '2024-03-28', '2024-03-30', NULL, 'IN_TRANSIT',
     '44444444-4444-4444-4444-444444444441', NULL,
     N'Điều phối hàng từ Kho Tổng HCM sang Kho Miền Bắc'),

    -- Kho Tổng HCM → Cửa Hàng Quận 1 (bổ sung lần 2)
    ('DA000001-0001-0001-0001-000000000005', 'TRF-2024-005',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001',
     'STORE',     'B0000001-0001-0001-0001-000000000002',
     '2024-03-25', '2024-03-26', '2024-03-26', 'DELIVERED',
     '44444444-4444-4444-4444-444444444441', '33333333-3333-3333-3333-333333333331',
     N'Bổ sung hàng lần 2 cho Cửa Hàng Quận 1'),

    -- Kho Chi Nhánh Bình Dương → Cửa Hàng Bình Dương (DELIVERED)
    ('DA000001-0001-0001-0001-000000000006', 'TRF-2024-006',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000003',
     'STORE',     'B0000001-0001-0001-0001-000000000003',
     '2024-03-18', '2024-03-19', '2024-03-19', 'DELIVERED',
     '44444444-4444-4444-4444-444444444441', '33333333-3333-3333-3333-333333333331',
     N'Nhập hàng lần đầu cho Cửa Hàng Bình Dương'),

    -- Kho Chi Nhánh Long An → Cửa Hàng Long An (DELIVERED)
    ('DA000001-0001-0001-0001-000000000007', 'TRF-2024-007',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000004',
     'STORE',     'B0000001-0001-0001-0001-000000000004',
     '2024-03-19', '2024-03-20', '2024-03-20', 'DELIVERED',
     '44444444-4444-4444-4444-444444444441', '33333333-3333-3333-3333-333333333331',
     N'Nhập hàng lần đầu cho Cửa Hàng Long An');
END
GO

IF NOT EXISTS (SELECT * FROM transfer_items WHERE transfer_id = 'DA000001-0001-0001-0001-000000000002')
BEGIN
    INSERT INTO transfer_items
        (id, transfer_id, product_id, batch_id, requested_quantity, shipped_quantity, received_quantity, damaged_quantity)
    VALUES
    -- TRF-2024-002: Kho Tổng → Quận 1
    (NEWID(), 'DA000001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000005', 'BA000001-0001-0001-0001-000000000001',
      60,  60,  60, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000007', 'BA000001-0001-0001-0001-000000000003',
      25,  25,  24, 1),

    -- TRF-2024-003: Kho Miền Bắc → Thủ Đức
    (NEWID(), 'DA000001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000005', 'BA000001-0001-0001-0001-000000000004',
      80,  80,  80, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000006', 'BA000001-0001-0001-0001-000000000005',
      70,  70,  68, 2),

    -- TRF-2024-004: Kho Tổng → Kho Miền Bắc (đang vận chuyển, chưa nhận)
    (NEWID(), 'DA000001-0001-0001-0001-000000000004', 'F0000001-0001-0001-0001-000000000005', 'BA000001-0001-0001-0001-000000000007',
     150, 150, NULL, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000004', 'F0000001-0001-0001-0001-000000000007', 'BA000001-0001-0001-0001-000000000006',
      30,  30, NULL, 0),

    -- TRF-2024-005: Kho Tổng → Quận 1
    (NEWID(), 'DA000001-0001-0001-0001-000000000005', 'F0000001-0001-0001-0001-000000000003', NULL,
      70,  70,  70, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000005', 'F0000001-0001-0001-0001-000000000005', 'BA000001-0001-0001-0001-000000000001',
      90,  90,  88, 2);
END
GO

-- Transfer items cho 2 cửa hàng mới
IF NOT EXISTS (SELECT * FROM transfer_items WHERE transfer_id = 'DA000001-0001-0001-0001-000000000006')
BEGIN
    INSERT INTO transfer_items
        (id, transfer_id, product_id, batch_id, requested_quantity, shipped_quantity, received_quantity, damaged_quantity)
    VALUES
    -- TRF-2024-006: Kho Bình Dương → CH Bình Dương
    (NEWID(), 'DA000001-0001-0001-0001-000000000006', 'F0000001-0001-0001-0001-000000000005', NULL,  70,  70,  70, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000006', 'F0000001-0001-0001-0001-000000000006', NULL,  50,  50,  50, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000006', 'F0000001-0001-0001-0001-000000000007', NULL,  20,  20,  20, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000006', 'F0000001-0001-0001-0001-000000000001', NULL,  40,  40,  40, 0),

    -- TRF-2024-007: Kho Long An → CH Long An
    (NEWID(), 'DA000001-0001-0001-0001-000000000007', 'F0000001-0001-0001-0001-000000000005', NULL,  45,  45,  45, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000007', 'F0000001-0001-0001-0001-000000000006', NULL,  12,  12,  12, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000007', 'F0000001-0001-0001-0001-000000000007', NULL,  10,  10,  10, 0),
    (NEWID(), 'DA000001-0001-0001-0001-000000000007', 'F0000001-0001-0001-0001-000000000003', NULL,  35,  35,  35, 0);
END
GO

-- =====================================================
-- Thêm stock movements (sau transfers vì SM tham chiếu transfer_id)
-- =====================================================
IF NOT EXISTS (SELECT * FROM stock_movements WHERE id = 'CA000001-0001-0001-0001-000000000003')
BEGIN
    INSERT INTO stock_movements
        (id, movement_number, movement_type, location_id, location_type,
         movement_date, supplier_name, transfer_id,
         received_by, status, notes)
    VALUES
    ('CA000001-0001-0001-0001-000000000003',
     'SM-2024-003', 'INBOUND', 'A0000001-0001-0001-0001-000000000002', 'WAREHOUSE',
     '2024-03-05', 'Vinamilk Co.', NULL,
     '44444444-4444-4444-4444-444444444441', 'COMPLETED',
     N'Nhận 400 hộp sữa Vinamilk tại Kho Miền Bắc'),

    ('CA000001-0001-0001-0001-000000000004',
     'SM-2024-004', 'INBOUND', 'A0000001-0001-0001-0001-000000000002', 'WAREHOUSE',
     '2024-03-15', 'TH True Milk Co.', NULL,
     '44444444-4444-4444-4444-444444444441', 'COMPLETED',
     N'Nhận 350 hộp TH True Milk tại Kho Miền Bắc'),

    ('CA000001-0001-0001-0001-000000000005',
     'SM-2024-005', 'TRANSFER', 'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     '2024-03-20', NULL, 'DA000001-0001-0001-0001-000000000002',
     '44444444-4444-4444-4444-444444444441', 'COMPLETED',
     N'Xuất hàng cho Cửa Hàng Quận 1 (TRF-2024-002)'),

    ('CA000001-0001-0001-0001-000000000006',
     'SM-2024-006', 'TRANSFER', 'A0000001-0001-0001-0001-000000000002', 'WAREHOUSE',
     '2024-03-22', NULL, 'DA000001-0001-0001-0001-000000000003',
     '44444444-4444-4444-4444-444444444441', 'COMPLETED',
     N'Xuất hàng lần đầu cho Cửa Hàng Thủ Đức từ Kho Miền Bắc'),

    ('CA000001-0001-0001-0001-000000000007',
     'SM-2024-007', 'ADJUSTMENT', 'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     '2024-03-25', NULL, NULL,
     '11111111-1111-1111-1111-111111111111', 'COMPLETED',
     N'Điều chỉnh sau kiểm kê tháng 3 - hao hụt rau củ');
END
GO

IF NOT EXISTS (SELECT * FROM stock_movement_items WHERE movement_id = 'CA000001-0001-0001-0001-000000000003')
BEGIN
    INSERT INTO stock_movement_items (id, movement_id, product_id, quantity, unit_price) VALUES
    (NEWID(), 'CA000001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000005',
     400, 32000),
    (NEWID(), 'CA000001-0001-0001-0001-000000000004', 'F0000001-0001-0001-0001-000000000006',
     350, 38000),
    (NEWID(), 'CA000001-0001-0001-0001-000000000005', 'F0000001-0001-0001-0001-000000000005',
      60, 32000),
    (NEWID(), 'CA000001-0001-0001-0001-000000000005', 'F0000001-0001-0001-0001-000000000007',
      25, 150000),
    (NEWID(), 'CA000001-0001-0001-0001-000000000006', 'F0000001-0001-0001-0001-000000000005',
      80, 32000),
    (NEWID(), 'CA000001-0001-0001-0001-000000000006', 'F0000001-0001-0001-0001-000000000006',
      70, 38000),
    (NEWID(), 'CA000001-0001-0001-0001-000000000007', 'F0000001-0001-0001-0001-000000000001',
     -5, 0); -- Điều chỉnh: -5 kg rau muống hỏng
END
GO

-- =====================================================
-- Thêm restock requests
-- =====================================================
IF NOT EXISTS (SELECT * FROM restock_requests WHERE id = 'EA000001-0001-0001-0001-000000000003')
BEGIN
    INSERT INTO restock_requests
        (id, request_number, from_warehouse_id, from_location_type, to_warehouse_id, to_location_type,
         requested_by, requested_date, priority, status, transfer_id, notes)
    VALUES
    -- Thủ Đức URGENT: TH True Milk hết (quantity=9, min=15 → LOW STOCK)
    -- Goods come FROM Kho Tổng HCM (A...001) TO Cửa Hàng Thủ Đức (B...001)
    ('EA000001-0001-0001-0001-000000000003', 'RST-2024-003',
     'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     'B0000001-0001-0001-0001-000000000001', 'STORE',
     '33333333-3333-3333-3333-333333333331', '2024-03-28', 'URGENT', 'APPROVED',
     NULL,
     N'TH True Milk dưới ngưỡng tối thiểu, cần bổ sung gấp'),

    -- Quận 1 bổ sung định kỳ: FROM Kho Tổng HCM TO CH Quận 1
    ('EA000001-0001-0001-0001-000000000004', 'RST-2024-004',
     'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     'B0000001-0001-0001-0001-000000000002', 'STORE',
     '33333333-3333-3333-3333-333333333331', '2024-03-29', 'NORMAL', 'PENDING',
     NULL,
     N'Bổ sung định kỳ tuần 2 cho CH Quận 1'),

    -- Quận 1 rau củ mùa vụ (đang xử lý): FROM Kho Tổng HCM TO CH Quận 1
    ('EA000001-0001-0001-0001-000000000005', 'RST-2024-005',
     'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     'B0000001-0001-0001-0001-000000000002', 'STORE',
     '33333333-3333-3333-3333-333333333331', '2024-03-30', 'HIGH', 'PROCESSING',
     'DA000001-0001-0001-0001-000000000005',
     N'Rau củ tuần tới tăng tiêu thụ - mùa lễ'),

    -- Thủ Đức bị từ chối (yêu cầu quá nhiều): FROM Kho Tổng HCM TO CH Thủ Đức
    ('EA000001-0001-0001-0001-000000000006', 'RST-2024-006',
     'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE',
     'B0000001-0001-0001-0001-000000000001', 'STORE',
     '33333333-3333-3333-3333-333333333331', '2024-03-27', 'NORMAL', 'REJECTED',
     NULL,
     N'Yêu cầu không hợp lệ - số lượng vượt giới hạn tháng');
END
GO

IF NOT EXISTS (SELECT * FROM restock_request_items WHERE request_id = 'EA000001-0001-0001-0001-000000000003')
BEGIN
    DECLARE @R3 UNIQUEIDENTIFIER = 'EA000001-0001-0001-0001-000000000003';
    DECLARE @R4 UNIQUEIDENTIFIER = 'EA000001-0001-0001-0001-000000000004';
    DECLARE @R5 UNIQUEIDENTIFIER = 'EA000001-0001-0001-0001-000000000005';
    DECLARE @R6 UNIQUEIDENTIFIER = 'EA000001-0001-0001-0001-000000000006';

    INSERT INTO restock_request_items
        (id, request_id, product_id, requested_quantity, current_quantity, approved_quantity, reason)
    VALUES
    -- RST-2024-003: Thủ Đức URGENT (TH True Milk low stock)
    (NEWID(), @R3, 'F0000001-0001-0001-0001-000000000006',  80,  9,  80, N'Dưới ngưỡng tối thiểu (9/15)'),
    (NEWID(), @R3, 'F0000001-0001-0001-0001-000000000005',  50, 45,  50, N'Sắp hết trong tuần'),

    -- RST-2024-004: Quận 1 NORMAL định kỳ
    (NEWID(), @R4, 'F0000001-0001-0001-0001-000000000001',  40, 45, NULL, N'Bổ sung định kỳ'),
    (NEWID(), @R4, 'F0000001-0001-0001-0001-000000000002',  30, 38, NULL, N'Bổ sung định kỳ'),

    -- RST-2024-005: Quận 1 HIGH mùa vụ
    (NEWID(), @R5, 'F0000001-0001-0001-0001-000000000003',  80, 72,  80, N'Cam sành tăng tiêu thụ mùa lễ'),
    (NEWID(), @R5, 'F0000001-0001-0001-0001-000000000001',  50, 45,  50, N'Rau muống tăng đột biến'),

    -- RST-2024-006: Thủ Đức REJECTED
    (NEWID(), @R6, 'F0000001-0001-0001-0001-000000000005', 500, 100, 0, N'Yêu cầu quá nhiều - từ chối');
END
GO

-- =====================================================
-- Damage reports mẫu
-- =====================================================
IF NOT EXISTS (SELECT * FROM damage_reports)
BEGIN
    INSERT INTO damage_reports
        (id, report_number, location_type, location_id, damage_type,
         reported_by, reported_date, total_value, description, status, approved_by, approved_date)
    VALUES
    ('FA000001-0001-0001-0001-000000000001', 'DMG-2024-001',
     'STORE', 'B0000001-0001-0001-0001-000000000001', 'EXPIRED',
     '33333333-3333-3333-3333-333333333331', '2024-03-15', 320000,
     N'2 hộp sữa Vinamilk hết hạn tại Cửa Hàng Thủ Đức',
     'APPROVED', '22222222-2222-2222-2222-222222222221', '2024-03-16'),

    ('FA000001-0001-0001-0001-000000000002', 'DMG-2024-002',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 'PHYSICAL_DAMAGE',
     '44444444-4444-4444-4444-444444444441', '2024-03-20', 750000,
     N'5 kg rau muống hỏng trong quá trình vận chuyển',
     'APPROVED', '11111111-1111-1111-1111-111111111111', '2024-03-21'),

    ('FA000001-0001-0001-0001-000000000003', 'DMG-2024-003',
     'STORE', 'B0000001-0001-0001-0001-000000000002', 'QUALITY_ISSUE',
     '33333333-3333-3333-3333-333333333331', '2024-03-29', 192000,
     N'1 túi gạo ST25 bị ẩm khi nhận hàng tại CH Quận 1',
     'PENDING', NULL, NULL);
END
GO

-- =====================================================
-- Inventory checks mẫu
-- =====================================================
IF NOT EXISTS (SELECT * FROM inventory_checks)
BEGIN
    INSERT INTO inventory_checks
        (id, check_number, location_type, location_id, check_type,
         check_date, checked_by, status, total_discrepancies, notes)
    VALUES
    ('0A000001-0001-0001-0001-000000000001', 'IC-2024-001',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001', 'FULL',
     '2024-03-25', '44444444-4444-4444-4444-444444444441', 'COMPLETED', 2,
     N'Kiểm kê tháng 3 - phát hiện hao hụt rau muống và cam'),

    ('0A000001-0001-0001-0001-000000000002', 'IC-2024-002',
     'STORE', 'B0000001-0001-0001-0001-000000000001', 'SPOT',
     '2024-03-28', '33333333-3333-3333-3333-333333333331', 'COMPLETED', 0,
     N'Kiểm tra đột xuất - không phát hiện chênh lệch'),

    ('0A000001-0001-0001-0001-000000000003', 'IC-2024-003',
     'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002', 'PARTIAL',
     '2024-03-30', '44444444-4444-4444-4444-444444444441', 'PENDING', 0,
     N'Kiểm kê một phần - đang thực hiện');
END
GO

IF NOT EXISTS (SELECT * FROM inventory_check_items WHERE check_id = '0A000001-0001-0001-0001-000000000001')
BEGIN
    INSERT INTO inventory_check_items (id, check_id, product_id, system_quantity, actual_quantity, note) VALUES
    (NEWID(), '0A000001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000001', 500, 495, N'Hao hụt 5 kg rau muống'),
    (NEWID(), '0A000001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000003', 800, 798, N'Thiếu 2 kg cam'),
    (NEWID(), '0A000001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000005', 1000, 1000, N'Khớp số liệu'),
    (NEWID(), '0A000001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000007',  200,  200, N'Khớp số liệu'),
    (NEWID(), '0A000001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000005',  100,  100, N'Khớp số liệu'),
    (NEWID(), '0A000001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000007',   15,   15, N'Khớp số liệu');
END
GO

-- =====================================================
-- Update transfers với restock_request_id tương ứng
-- =====================================================
-- TRF-2024-001 ← RST-2024-001 (COMPLETED, Thủ Đức weekly)
UPDATE transfers SET restock_request_id = 'EA000001-0001-0001-0001-000000000001'
WHERE id = 'DA000001-0001-0001-0001-000000000001' AND restock_request_id IS NULL;
-- TRF-2024-003 ← RST-2024-001 cũng liên quan (Kho Miền Bắc → Thủ Đức)
UPDATE transfers SET restock_request_id = 'EA000001-0001-0001-0001-000000000001'
WHERE id = 'DA000001-0001-0001-0001-000000000003' AND restock_request_id IS NULL;
-- TRF-2024-005 ← RST-2024-005 (PROCESSING, Quận 1 mùa vụ)
UPDATE transfers SET restock_request_id = 'EA000001-0001-0001-0001-000000000005'
WHERE id = 'DA000001-0001-0001-0001-000000000005' AND restock_request_id IS NULL;
GO

PRINT '===============================================';
PRINT 'Inventory Management Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Total Tables: 15 tables';
PRINT 'Sample Data:';
PRINT '  - 8 Warehouses/Stores (4 warehouses + 4 stores)';
PRINT '  - 20+ Inventory Records';
PRINT '  - 7 Product Batches';
PRINT '  - 7 Stock Movements';
PRINT '  - 7 Transfers (with items, with restock_request_id)';
PRINT '  - 6 Restock Requests (with transfer_id where applicable)';
PRINT '  - 3 Damage Reports';
PRINT '  - 3 Inventory Checks';
PRINT '===============================================';
GO
