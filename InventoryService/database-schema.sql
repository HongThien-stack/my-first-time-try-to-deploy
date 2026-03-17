-- =====================================================
-- Inventory Management Service - FIXED VERSION
-- =====================================================
-- Database: InventoryDB
-- Purpose: Warehouse Management, Stock Tracking, Batch Management
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
        capacity INT NOT NULL DEFAULT 0,
        status NVARCHAR(20) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE
        parent_id UNIQUEIDENTIFIER, -- Self-reference: sub-warehouse points to parent warehouse
        is_deleted BIT NOT NULL DEFAULT 0, -- Soft delete flag
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        created_by UNIQUEIDENTIFIER, -- Reference to IdentityDB.users.id
        updated_by UNIQUEIDENTIFIER,
        CONSTRAINT FK_warehouses_parent FOREIGN KEY (parent_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_warehouses_name ON warehouses(name);
    CREATE INDEX IX_warehouses_is_deleted ON warehouses(is_deleted);
    CREATE INDEX IX_warehouses_parent_id ON warehouses(parent_id);
END
GO

-- =====================================================
-- Table: warehouse_slots
-- Purpose: Physical storage slots inside a warehouse
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'warehouse_slots')
BEGIN
    CREATE TABLE warehouse_slots (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        slot_code NVARCHAR(50) NOT NULL,
        capacity INT NOT NULL DEFAULT 0,
        status NVARCHAR(20) NOT NULL DEFAULT 'AVAILABLE', -- AVAILABLE | OCCUPIED | BLOCKED
        is_deleted BIT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        created_by UNIQUEIDENTIFIER,
        updated_by UNIQUEIDENTIFIER,
        CONSTRAINT FK_warehouse_slots_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );

    CREATE UNIQUE INDEX UX_warehouse_slots_code ON warehouse_slots(warehouse_id, slot_code);
    CREATE INDEX IX_warehouse_slots_status ON warehouse_slots(status);
    CREATE INDEX IX_warehouse_slots_is_deleted ON warehouse_slots(is_deleted);
END
GO

-- =====================================================
-- Table: inventories
-- FIXED: Renamed stores_id → store_id, Added audit fields
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventories')
BEGIN
    CREATE TABLE inventories (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        store_id UNIQUEIDENTIFIER NOT NULL, -- FIXED: Was stores_id. Reference to OrderDB.stores.id (when exists)
        product_id UNIQUEIDENTIFIER NOT NULL, -- Reference to ProductDB.products.id
        quantity INT NOT NULL DEFAULT 0,
        alert_threshold INT NOT NULL DEFAULT 10,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        created_by UNIQUEIDENTIFIER, -- ADDED: Reference to IdentityDB.users.id
        updated_by UNIQUEIDENTIFIER  -- ADDED: Reference to IdentityDB.users.id
    );
    
    CREATE INDEX IX_inventories_store_id ON inventories(store_id);
    CREATE INDEX IX_inventories_product_id ON inventories(product_id);
    CREATE INDEX IX_inventories_quantity ON inventories(quantity);
    CREATE UNIQUE INDEX UX_inventories_store_product ON inventories(store_id, product_id);
END
GO

-- =====================================================
-- Table: product_batches
-- FIXED: Added UNIQUE constraint for batch_code per warehouse
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'product_batches')
BEGIN
    CREATE TABLE product_batches (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        product_id UNIQUEIDENTIFIER NOT NULL, -- Reference to ProductDB.products.id
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        batch_code NVARCHAR(100) NOT NULL,
        manufacture_date DATE,
        expiration_date DATE,
        quantity INT NOT NULL DEFAULT 0,
        is_deleted BIT NOT NULL DEFAULT 0, -- Soft delete flag
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        created_by UNIQUEIDENTIFIER, -- Reference to IdentityDB.users.id
        updated_by UNIQUEIDENTIFIER,
        CONSTRAINT FK_product_batches_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_product_batches_product_id ON product_batches(product_id);
    CREATE INDEX IX_product_batches_warehouse_id ON product_batches(warehouse_id);
    CREATE INDEX IX_product_batches_batch_code ON product_batches(batch_code);
    CREATE INDEX IX_product_batches_expiration_date ON product_batches(expiration_date);
    CREATE INDEX IX_product_batches_is_deleted ON product_batches(is_deleted);
    CREATE UNIQUE INDEX UX_product_batches_warehouse_code ON product_batches(warehouse_id, batch_code) WHERE is_deleted = 0; -- ADDED: Unique constraint
END
GO

-- =====================================================
-- Table: stock_movements
-- FIXED: Clarified created_by reference
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'stock_movements')
BEGIN
    CREATE TABLE stock_movements (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        store_id UNIQUEIDENTIFIER NULL, -- Reference to OrderDB.stores.id (only for EXPORT, when OrderDB exists)
        movement_type NVARCHAR(50) NOT NULL, -- IMPORT | EXPORT
        source_info NVARCHAR(500), -- Source information (only for IMPORT)
        note NVARCHAR(MAX),
        created_by UNIQUEIDENTIFIER NOT NULL, -- FIXED: Reference to IdentityDB.users.id (warehouse staff)
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_stock_movements_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_stock_movements_warehouse_id ON stock_movements(warehouse_id);
    CREATE INDEX IX_stock_movements_store_id ON stock_movements(store_id);
    CREATE INDEX IX_stock_movements_movement_type ON stock_movements(movement_type);
    CREATE INDEX IX_stock_movements_created_at ON stock_movements(created_at);
    CREATE INDEX IX_stock_movements_created_by ON stock_movements(created_by);
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
        quantity INT NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_stock_movement_items_movements FOREIGN KEY (movement_id) REFERENCES stock_movements(id)
    );
    
    CREATE INDEX IX_stock_movement_items_movement_id ON stock_movement_items(movement_id);
END
GO

-- =====================================================
-- Table: inventory_history
-- FIXED: Clarified created_by reference
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_history')
BEGIN
    CREATE TABLE inventory_history (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        batch_id UNIQUEIDENTIFIER NOT NULL,
        quantity_change INT NOT NULL,
        movement_id UNIQUEIDENTIFIER NOT NULL,
        created_by UNIQUEIDENTIFIER NOT NULL, -- FIXED: Reference to IdentityDB.users.id (warehouse staff)
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_inventory_history_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id),
        CONSTRAINT FK_inventory_history_batches FOREIGN KEY (batch_id) REFERENCES product_batches(id),
        CONSTRAINT FK_inventory_history_movements FOREIGN KEY (movement_id) REFERENCES stock_movements(id)
    );
    
    CREATE INDEX IX_inventory_history_warehouse_id ON inventory_history(warehouse_id);
    CREATE INDEX IX_inventory_history_batch_id ON inventory_history(batch_id);
    CREATE INDEX IX_inventory_history_movement_id ON inventory_history(movement_id);
    CREATE INDEX IX_inventory_history_created_at ON inventory_history(created_at);
    CREATE INDEX IX_inventory_history_created_by ON inventory_history(created_by);
END
GO

-- =====================================================
-- Table: inventory_logs
-- FIXED: Clarified created_by reference
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_logs')
BEGIN
    CREATE TABLE inventory_logs (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        product_id UNIQUEIDENTIFIER NOT NULL, -- Reference to ProductDB.products.id
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        change_quantity INT NOT NULL,
        action NVARCHAR(50) NOT NULL, -- IMPORT | EXPORT
        created_by UNIQUEIDENTIFIER NOT NULL, -- FIXED: Reference to IdentityDB.users.id (warehouse staff)
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_inventory_logs_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_inventory_logs_product_id ON inventory_logs(product_id);
    CREATE INDEX IX_inventory_logs_warehouse_id ON inventory_logs(warehouse_id);
    CREATE INDEX IX_inventory_logs_action ON inventory_logs(action);
    CREATE INDEX IX_inventory_logs_created_at ON inventory_logs(created_at);
    CREATE INDEX IX_inventory_logs_created_by ON inventory_logs(created_by);
END
GO

PRINT 'Inventory Management Database Created Successfully';
GO

-- =====================================================
-- Sample Data: warehouses and warehouse_slots
-- =====================================================
IF NOT EXISTS (SELECT * FROM warehouses)
BEGIN
    DECLARE @warehouseA UNIQUEIDENTIFIER = NEWID();
    DECLARE @warehouseB UNIQUEIDENTIFIER = NEWID();

    INSERT INTO warehouses (id, name, location) VALUES
    (@warehouseA, 'Central Warehouse', 'District 1, HCMC'),
    (@warehouseB, 'North Warehouse', 'Cau Giay, Hanoi');

    INSERT INTO warehouse_slots (warehouse_id, slot_code, capacity, status) VALUES
    (@warehouseA, 'A-01-01', 200, 'AVAILABLE'),
    (@warehouseA, 'A-01-02', 200, 'AVAILABLE'),
    (@warehouseA, 'A-02-01', 150, 'OCCUPIED'),
    (@warehouseA, 'A-02-02', 150, 'AVAILABLE'),
    (@warehouseB, 'B-01-01', 120, 'AVAILABLE'),
    (@warehouseB, 'B-01-02', 120, 'BLOCKED');
END
GO

PRINT '===============================================';
PRINT 'IMPORTANT NOTES:';
PRINT '===============================================';
PRINT '1. inventories.store_id and stock_movements.store_id';
PRINT '   → Reference to OrderDB.stores.id (create OrderDB first)';
PRINT '';
PRINT '2. All created_by fields → Reference to IdentityDB.users.id';
PRINT '   → User with role_id = 4 (Warehouse Staff)';
PRINT '';
PRINT '3. product_id fields → Reference to ProductDB.products.id';
PRINT '===============================================';
GO
