-- =====================================================
-- Order Management Service (POS and Online Orders)
-- =====================================================
-- Database: OrderDB
-- Purpose: Order Processing, Customer Management, Store Management
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OrderDB')
BEGIN
    CREATE DATABASE OrderDB;
END
GO

USE OrderDB;
GO

-- =====================================================
-- Table: stores
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'stores')
BEGIN
    CREATE TABLE stores (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(255) NOT NULL,
        location NVARCHAR(500),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_stores_name ON stores(name);
END
GO

-- =====================================================
-- Table: customers
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'customers')
BEGIN
    CREATE TABLE customers (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        user_id UNIQUEIDENTIFIER NOT NULL, -- Reference to IdentityDB.users.id
        full_name NVARCHAR(255),
        phone NVARCHAR(20),
        address NVARCHAR(500),
        loyalty_id UNIQUEIDENTIFIER, -- Reference to LoyaltyDB.loyalty.id
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_customers_user_id ON customers(user_id);
    CREATE INDEX IX_customers_phone ON customers(phone);
    CREATE INDEX IX_customers_loyalty_id ON customers(loyalty_id);
END
GO

-- =====================================================
-- Table: promotions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'promotions')
BEGIN
    CREATE TABLE promotions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(255) NOT NULL,
        discount_type NVARCHAR(50) NOT NULL, -- PERCENT | FIXED
        discount_value DECIMAL(18, 2) NOT NULL,
        start_date DATE NOT NULL,
        end_date DATE NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_promotions_name ON promotions(name);
    CREATE INDEX IX_promotions_start_date ON promotions(start_date);
    CREATE INDEX IX_promotions_end_date ON promotions(end_date);
END
GO

-- =====================================================
-- Table: orders
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'orders')
BEGIN
    CREATE TABLE orders (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        customer_id UNIQUEIDENTIFIER NOT NULL,
        store_id UNIQUEIDENTIFIER NOT NULL,
        created_by UNIQUEIDENTIFIER NOT NULL, -- Reference to ShiftDB.staff.id
        status NVARCHAR(50) NOT NULL, -- PENDING | CONFIRMED | PROCESSING | SHIPPED | DELIVERED | CANCELLED
        total_amount DECIMAL(18, 2) NOT NULL,
        address NVARCHAR(500),
        description NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        CONSTRAINT FK_orders_customers FOREIGN KEY (customer_id) REFERENCES customers(id),
        CONSTRAINT FK_orders_stores FOREIGN KEY (store_id) REFERENCES stores(id)
    );
    
    CREATE INDEX IX_orders_customer_id ON orders(customer_id);
    CREATE INDEX IX_orders_store_id ON orders(store_id);
    CREATE INDEX IX_orders_status ON orders(status);
    CREATE INDEX IX_orders_created_at ON orders(created_at);
END
GO

-- =====================================================
-- Table: order_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'order_items')
BEGIN
    CREATE TABLE order_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        order_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- Reference to ProductDB.products.id
        quantity INT NOT NULL,
        price DECIMAL(18, 2) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_order_items_orders FOREIGN KEY (order_id) REFERENCES orders(id)
    );
    
    CREATE INDEX IX_order_items_order_id ON order_items(order_id);
    CREATE INDEX IX_order_items_product_id ON order_items(product_id);
END
GO

-- =====================================================
-- Table: order_tracking
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'order_tracking')
BEGIN
    CREATE TABLE order_tracking (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        order_id UNIQUEIDENTIFIER NOT NULL,
        status NVARCHAR(50) NOT NULL,
        updated_by UNIQUEIDENTIFIER NOT NULL, -- Reference to ShiftDB.staff.id
        updated_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_order_tracking_orders FOREIGN KEY (order_id) REFERENCES orders(id)
    );
    
    CREATE INDEX IX_order_tracking_order_id ON order_tracking(order_id);
    CREATE INDEX IX_order_tracking_status ON order_tracking(status);
    CREATE INDEX IX_order_tracking_updated_at ON order_tracking(updated_at);
END
GO

-- =====================================================
-- Table: order_promotions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'order_promotions')
BEGIN
    CREATE TABLE order_promotions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        order_id UNIQUEIDENTIFIER NOT NULL,
        promotion_id UNIQUEIDENTIFIER NOT NULL,
        discount_amount DECIMAL(18, 2) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_order_promotions_orders FOREIGN KEY (order_id) REFERENCES orders(id),
        CONSTRAINT FK_order_promotions_promotions FOREIGN KEY (promotion_id) REFERENCES promotions(id)
    );
    
    CREATE INDEX IX_order_promotions_order_id ON order_promotions(order_id);
    CREATE INDEX IX_order_promotions_promotion_id ON order_promotions(promotion_id);
    CREATE UNIQUE INDEX UX_order_promotions ON order_promotions(order_id, promotion_id);
END
GO

PRINT 'Order Management Database Created Successfully';
GO
