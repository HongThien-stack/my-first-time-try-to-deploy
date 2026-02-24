-- =====================================================
-- Inventory Management Service
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
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_warehouses_name ON warehouses(name);
END
GO

-- =====================================================
-- Table: inventories
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventories')
BEGIN
    CREATE TABLE inventories (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        stores_id UNIQUEIDENTIFIER NOT NULL, -- Reference to OrderDB.stores.id
        product_id UNIQUEIDENTIFIER NOT NULL, -- Reference to ProductDB.products.id
        quantity INT NOT NULL DEFAULT 0,
        alert_threshold INT NOT NULL DEFAULT 10,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_inventories_stores_id ON inventories(stores_id);
    CREATE INDEX IX_inventories_product_id ON inventories(product_id);
    CREATE INDEX IX_inventories_quantity ON inventories(quantity);
    CREATE UNIQUE INDEX UX_inventories_store_product ON inventories(stores_id, product_id);
END
GO

-- =====================================================
-- Table: product_batches
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
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_product_batches_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_product_batches_product_id ON product_batches(product_id);
    CREATE INDEX IX_product_batches_warehouse_id ON product_batches(warehouse_id);
    CREATE INDEX IX_product_batches_batch_code ON product_batches(batch_code);
    CREATE INDEX IX_product_batches_expiration_date ON product_batches(expiration_date);
END
GO

-- =====================================================
-- Table: stock_movements
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'stock_movements')
BEGIN
    CREATE TABLE stock_movements (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        store_id UNIQUEIDENTIFIER NULL, -- Reference to OrderDB.stores.id (only for EXPORT)
        movement_type NVARCHAR(50) NOT NULL, -- IMPORT | EXPORT
        source_info NVARCHAR(500), -- Source information (only for IMPORT)
        note NVARCHAR(MAX),
        created_by UNIQUEIDENTIFIER NOT NULL, -- Reference to ShiftDB.staff.id
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_stock_movements_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_stock_movements_warehouse_id ON stock_movements(warehouse_id);
    CREATE INDEX IX_stock_movements_store_id ON stock_movements(store_id);
    CREATE INDEX IX_stock_movements_movement_type ON stock_movements(movement_type);
    CREATE INDEX IX_stock_movements_created_at ON stock_movements(created_at);
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
        batch_id UNIQUEIDENTIFIER NOT NULL,
        quantity INT NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_stock_movement_items_movements FOREIGN KEY (movement_id) REFERENCES stock_movements(id),
        CONSTRAINT FK_stock_movement_items_batches FOREIGN KEY (batch_id) REFERENCES product_batches(id)
    );
    
    CREATE INDEX IX_stock_movement_items_movement_id ON stock_movement_items(movement_id);
    CREATE INDEX IX_stock_movement_items_batch_id ON stock_movement_items(batch_id);
END
GO

-- =====================================================
-- Table: inventory_history
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_history')
BEGIN
    CREATE TABLE inventory_history (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        batch_id UNIQUEIDENTIFIER NOT NULL,
        quantity_change INT NOT NULL,
        movement_id UNIQUEIDENTIFIER NOT NULL,
        created_by UNIQUEIDENTIFIER NOT NULL, -- Reference to ShiftDB.staff.id
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_inventory_history_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id),
        CONSTRAINT FK_inventory_history_batches FOREIGN KEY (batch_id) REFERENCES product_batches(id),
        CONSTRAINT FK_inventory_history_movements FOREIGN KEY (movement_id) REFERENCES stock_movements(id)
    );
    
    CREATE INDEX IX_inventory_history_warehouse_id ON inventory_history(warehouse_id);
    CREATE INDEX IX_inventory_history_batch_id ON inventory_history(batch_id);
    CREATE INDEX IX_inventory_history_movement_id ON inventory_history(movement_id);
    CREATE INDEX IX_inventory_history_created_at ON inventory_history(created_at);
END
GO

-- =====================================================
-- Table: inventory_logs
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'inventory_logs')
BEGIN
    CREATE TABLE inventory_logs (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        product_id UNIQUEIDENTIFIER NOT NULL, -- Reference to ProductDB.products.id
        warehouse_id UNIQUEIDENTIFIER NOT NULL,
        change_quantity INT NOT NULL,
        action NVARCHAR(50) NOT NULL, -- IMPORT | EXPORT
        created_by UNIQUEIDENTIFIER NOT NULL, -- Reference to ShiftDB.staff.id
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_inventory_logs_warehouses FOREIGN KEY (warehouse_id) REFERENCES warehouses(id)
    );
    
    CREATE INDEX IX_inventory_logs_product_id ON inventory_logs(product_id);
    CREATE INDEX IX_inventory_logs_warehouse_id ON inventory_logs(warehouse_id);
    CREATE INDEX IX_inventory_logs_action ON inventory_logs(action);
    CREATE INDEX IX_inventory_logs_created_at ON inventory_logs(created_at);
END
GO

PRINT 'Inventory Management Database Created Successfully';
GO
