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
        capacity INT NOT NULL, -- Total slots
        status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE
        is_deleted BIT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        created_by UNIQUEIDENTIFIER -- IdentityDB.users.id
    );
    
    CREATE INDEX IX_warehouses_name ON warehouses(name);
    CREATE INDEX IX_warehouses_status ON warehouses(status);
END
GO

-- =====================================================
-- Table: warehouse_slots
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'warehouse_slots')
BEGIN
    CREATE TABLE warehouse_slots (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        slot_code NVARCHAR(50) NOT NULL, -- A-01-01
        zone NVARCHAR(10), -- A, B, C
        row_number INT,
        column_number INT,
        status NVARCHAR(50) NOT NULL DEFAULT 'EMPTY', -- EMPTY | OCCUPIED | RESERVED | MAINTENANCE
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_warehouse_slots_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id),
        CONSTRAINT UQ_warehouse_slot UNIQUE (warehouse_id, slot_code)
    );
    
    CREATE INDEX IX_slots_warehouse_id ON warehouse_slots(warehouse_id);
    CREATE INDEX IX_slots_status ON warehouse_slots(status);
END
GO

-- =====================================================
-- Migration: Add is_deleted column to warehouse_slots if missing
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('warehouse_slots') AND name = 'is_deleted')
BEGIN
    ALTER TABLE warehouse_slots ADD is_deleted BIT NOT NULL DEFAULT 0;
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
        slot_id UNIQUEIDENTIFIER, -- warehouse_slots.id
        batch_number NVARCHAR(100) NOT NULL,
        quantity INT NOT NULL,
        manufacturing_date DATE,
        expiry_date DATE,
        supplier NVARCHAR(255),
        received_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        status NVARCHAR(50) NOT NULL DEFAULT 'AVAILABLE', -- AVAILABLE | SOLD | EXPIRED | DAMAGED
        CONSTRAINT FK_batches_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id),
        CONSTRAINT FK_batches_slots FOREIGN KEY (slot_id) REFERENCES warehouse_slots(id),
        CONSTRAINT UQ_batch_number UNIQUE (batch_number)
    );
    
    CREATE INDEX IX_batches_product_id ON product_batches(product_id);
    CREATE INDEX IX_batches_warehouse_id ON product_batches(warehouse_id);
    CREATE INDEX IX_batches_expiry_date ON product_batches(expiry_date);
    CREATE INDEX IX_batches_status ON product_batches(status);
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
        supplier NVARCHAR(255), -- For INBOUND
        po_number NVARCHAR(100), -- Purchase Order number
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

-- =====================================================
-- Table: stock_movement_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'stock_movement_items')
BEGIN
    CREATE TABLE stock_movement_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        movement_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        batch_id UNIQUEIDENTIFIER, -- product_batches.id
        slot_id UNIQUEIDENTIFIER, -- warehouse_slots.id
        quantity INT NOT NULL,
        unit_price DECIMAL(18,2), -- For valuation
        CONSTRAINT FK_movement_items_movements FOREIGN KEY (movement_id) REFERENCES stock_movements(id),
        CONSTRAINT FK_movement_items_batches FOREIGN KEY (batch_id) REFERENCES product_batches(id)
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
        updated_at DATETIME2
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
END
GO

-- =====================================================
-- Table: restock_requests
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'restock_requests')
BEGIN
    CREATE TABLE restock_requests (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        request_number NVARCHAR(50) NOT NULL UNIQUE, -- RST-2024-001
        store_id UNIQUEIDENTIFIER NOT NULL, -- Store requesting restock
        warehouse_id UNIQUEIDENTIFIER, -- Warehouse to fulfill
        requested_by UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id (Store staff)
        requested_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        priority NVARCHAR(50) NOT NULL DEFAULT 'NORMAL', -- NORMAL | HIGH | URGENT
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | APPROVED | PROCESSING | COMPLETED | REJECTED
        approved_by UNIQUEIDENTIFIER, -- IdentityDB.users.id (Warehouse manager)
        approved_date DATETIME2,
        transfer_id UNIQUEIDENTIFIER, -- Link to created transfer
        notes NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        CONSTRAINT FK_restock_transfers FOREIGN KEY (transfer_id) REFERENCES transfers(id)
    );
    
    CREATE INDEX IX_restock_request_number ON restock_requests(request_number);
    CREATE INDEX IX_restock_store_id ON restock_requests(store_id);
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
-- Insert sample warehouses
-- =====================================================
IF NOT EXISTS (SELECT * FROM warehouses)
BEGIN
    INSERT INTO warehouses (id, name, location, capacity, status, created_by, created_at) VALUES
    ('A0000001-0001-0001-0001-000000000001', N'Kho Tổng HCM', N'Quận Thủ Đức, TP. Hồ Chí Minh', 120, 'ACTIVE', 
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),
    ('A0000001-0001-0001-0001-000000000002', N'Kho Miền Bắc', N'Quận Long Biên, Hà Nội', 100, 'ACTIVE', 
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),
    ('B0000001-0001-0001-0001-000000000001', N'Cửa Hàng Thủ Đức', N'123 Lê Văn Việt, Quận 9, HCM', 50, 'ACTIVE', 
     '22222222-2222-2222-2222-222222222221', GETUTCDATE()),
    ('B0000001-0001-0001-0001-000000000002', N'Cửa Hàng Quận 1', N'456 Nguyễn Huệ, Quận 1, HCM', 40, 'ACTIVE', 
     '22222222-2222-2222-2222-222222222221', GETUTCDATE());
END
GO

-- =====================================================
-- Insert sample warehouse slots for Kho Tổng HCM
-- =====================================================
DECLARE @WarehouseId UNIQUEIDENTIFIER = 'A0000001-0001-0001-0001-000000000001';
IF NOT EXISTS (SELECT * FROM warehouse_slots WHERE warehouse_id = @WarehouseId)
BEGIN
    INSERT INTO warehouse_slots (id, warehouse_id, slot_code, zone, row_number, column_number, status) VALUES
    ('AA000001-0001-0001-0001-000000000001', @WarehouseId, 'A-01-01', 'A', 1, 1, 'OCCUPIED'),
    ('AA000001-0001-0001-0001-000000000002', @WarehouseId, 'A-01-02', 'A', 1, 2, 'OCCUPIED'),
    ('AA000001-0001-0001-0001-000000000003', @WarehouseId, 'A-02-01', 'A', 2, 1, 'OCCUPIED'),
    ('AA000001-0001-0001-0001-000000000004', @WarehouseId, 'B-01-01', 'B', 1, 1, 'EMPTY'),
    ('AA000001-0001-0001-0001-000000000005', @WarehouseId, 'B-01-02', 'B', 1, 2, 'EMPTY');
END
GO

-- =====================================================
-- Insert sample inventories
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
    INSERT INTO product_batches (id, product_id, warehouse_id, slot_id, batch_number, quantity, manufacturing_date, expiry_date, supplier, received_at, status) VALUES
    ('BA000001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000005', 'A0000001-0001-0001-0001-000000000001', 
     'AA000001-0001-0001-0001-000000000001', 'VNM-2024-001', 500, '2024-02-01', '2024-08-01', 'Vinamilk Co.', '2024-02-05', 'AVAILABLE'),
    ('BA000001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000006', 'A0000001-0001-0001-0001-000000000001', 
     'AA000001-0001-0001-0001-000000000002', 'TH-2024-001', 500, '2024-02-01', '2024-08-01', 'TH True Milk Co.', '2024-02-05', 'AVAILABLE'),
    ('BA000001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000007', 'A0000001-0001-0001-0001-000000000001', 
     'AA000001-0001-0001-0001-000000000003', 'ST25-2024-001', 200, '2024-01-15', '2025-01-15', 'ST25 Co.', '2024-01-20', 'AVAILABLE');
END
GO

-- =====================================================
-- Insert sample stock movements (receiving goods)
-- =====================================================
IF NOT EXISTS (SELECT * FROM stock_movements)
BEGIN
    INSERT INTO stock_movements (id, movement_number, movement_type, location_id, location_type, movement_date, supplier, po_number, received_by, status, notes) VALUES
    ('CA000001-0001-0001-0001-000000000001', 'SM-2024-001', 'INBOUND', 'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE', 
     '2024-02-05', 'Vinamilk Co.', 'PO-2024-001', '44444444-4444-4444-4444-444444444441', 'COMPLETED', N'Received 500 units of milk'),
    ('CA000001-0001-0001-0001-000000000002', 'SM-2024-002', 'INBOUND', 'A0000001-0001-0001-0001-000000000001', 'WAREHOUSE', 
     '2024-02-10', N'Đà Lạt Suppliers', 'PO-2024-002', '44444444-4444-4444-4444-444444444441', 'COMPLETED', N'Received fresh vegetables');
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
IF NOT EXISTS (SELECT * FROM restock_requests)
BEGIN
    INSERT INTO restock_requests (id, request_number, store_id, warehouse_id, requested_by, requested_date, priority, status, notes) VALUES
    ('EA000001-0001-0001-0001-000000000001', 'RST-2024-001', 'B0000001-0001-0001-0001-000000000001', 'A0000001-0001-0001-0001-000000000001', 
     '33333333-3333-3333-3333-333333333331', '2024-03-01', 'NORMAL', 'COMPLETED', N'Weekly restock'),
    ('EA000001-0001-0001-0001-000000000002', 'RST-2024-002', 'B0000001-0001-0001-0001-000000000001', 'A0000001-0001-0001-0001-000000000001', 
     '33333333-3333-3333-3333-333333333331', '2024-03-03', 'HIGH', 'PENDING', N'Urgent: Low stock on milk');
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

PRINT '===============================================';
PRINT 'Inventory Management Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Total Tables: 16 tables';
PRINT 'Sample Data:';
PRINT '  - 4 Warehouses/Stores';
PRINT '  - 5 Warehouse Slots';
PRINT '  - 9 Inventory Records';
PRINT '  - 3 Product Batches';
PRINT '  - 2 Stock Movements';
PRINT '  - 1 Transfer (with 2 items)';
PRINT '  - 2 Restock Requests (with 3 items)';
PRINT '===============================================';
GO
