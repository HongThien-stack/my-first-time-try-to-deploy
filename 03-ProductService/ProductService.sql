-- =====================================================
-- Product Management Service
-- =====================================================
-- Database: ProductDB
-- Purpose: Product Catalog, Categories Management
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductDB')
BEGIN
    CREATE DATABASE ProductDB;
END
GO

USE ProductDB;
GO

-- =====================================================
-- Table: categories
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'categories')
BEGIN
    CREATE TABLE categories (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(255) NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_categories_name ON categories(name);
    CREATE INDEX IX_categories_status ON categories(status);
END
GO

-- =====================================================
-- Table: products
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'products')
BEGIN
    CREATE TABLE products (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(255) NOT NULL,
        price DECIMAL(18, 2) NOT NULL,
        expiration_date DATE,
        is_available BIT NOT NULL DEFAULT 1,
        category_id UNIQUEIDENTIFIER NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        CONSTRAINT FK_products_categories FOREIGN KEY (category_id) REFERENCES categories(id)
    );
    
    CREATE INDEX IX_products_name ON products(name);
    CREATE INDEX IX_products_category_id ON products(category_id);
    CREATE INDEX IX_products_is_available ON products(is_available);
    CREATE INDEX IX_products_price ON products(price);
END
GO

-- =====================================================
-- Insert default categories
-- =====================================================
IF NOT EXISTS (SELECT * FROM categories)
BEGIN
    INSERT INTO categories (id, name, status, created_at) VALUES
    (NEWID(), 'Electronics', 'ACTIVE', GETUTCDATE()),
    (NEWID(), 'Food & Beverages', 'ACTIVE', GETUTCDATE()),
    (NEWID(), 'Clothing', 'ACTIVE', GETUTCDATE()),
    (NEWID(), 'Home & Garden', 'ACTIVE', GETUTCDATE()),
    (NEWID(), 'Health & Beauty', 'ACTIVE', GETUTCDATE());
END
GO

PRINT 'Product Management Database Created Successfully';
GO
