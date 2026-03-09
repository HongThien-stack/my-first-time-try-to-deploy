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
        is_deleted BIT NOT NULL DEFAULT 0, -- Soft delete flag
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_categories_name ON categories(name);
    CREATE INDEX IX_categories_status ON categories(status);
    CREATE INDEX IX_categories_is_deleted ON categories(is_deleted);
END
GO

-- =====================================================
-- Table: suppliers
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'suppliers')
BEGIN
    CREATE TABLE suppliers (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(255) NOT NULL,
        phone NVARCHAR(20),
        email NVARCHAR(255),
        contact_person NVARCHAR(255), -- Người liên hệ
        status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE
        is_deleted BIT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );

    CREATE INDEX IX_suppliers_name ON suppliers(name);
    CREATE INDEX IX_suppliers_status ON suppliers(status);
    CREATE INDEX IX_suppliers_is_deleted ON suppliers(is_deleted);
END
GO

-- =====================================================
-- Table: products
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'products')
BEGIN
    CREATE TABLE products (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        
        -- Thông tin cơ bản
        sku NVARCHAR(50) NOT NULL UNIQUE, -- Mã SKU
        barcode NVARCHAR(50) UNIQUE, -- Mã vạch
        name NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX), -- Mô tả chi tiết
        
        -- Phân loại
        category_id UNIQUEIDENTIFIER NOT NULL,
        supplier_id UNIQUEIDENTIFIER NULL, -- suppliers.id
        brand NVARCHAR(100), -- Thương hiệu (Vinamilk, TH True Milk, etc.)
        origin NVARCHAR(100), -- Xuất xứ (Việt Nam, Nhật Bản, etc.)
        
        -- Giá và khuyến mãi
        price DECIMAL(18, 2) NOT NULL, -- Giá bán
        original_price DECIMAL(18, 2), -- Giá gốc (trước khuyến mãi)
        cost_price DECIMAL(18, 2), -- Giá vốn
        
        -- Đơn vị và khối lượng
        unit NVARCHAR(50) NOT NULL, -- Đơn vị: Kg, Gram, Lít, Chai, Hộp, Túi, Cái
        weight DECIMAL(10, 3), -- Khối lượng (kg)
        volume DECIMAL(10, 3), -- Thể tích (lít)
        quantity_per_unit INT DEFAULT 1, -- Số lượng mỗi đơn vị (vd: 1 thùng = 24 chai)
        
        -- Đặt hàng
        min_order_quantity INT DEFAULT 1, -- Số lượng đặt tối thiểu
        max_order_quantity INT, -- Số lượng đặt tối đa
        
        -- Hạn sử dụng và bảo quản
        expiration_date DATE, -- Hạn sử dụng (cho batch cụ thể)
        shelf_life_days INT, -- Hạn sử dụng (số ngày)
        storage_instructions NVARCHAR(500), -- Hướng dẫn bảo quản
        is_perishable BIT DEFAULT 0, -- Thực phẩm tươi sống
        
        -- Hình ảnh
        image_url NVARCHAR(500), -- Ảnh chính
        images NVARCHAR(MAX), -- JSON array các ảnh phụ
        
        -- Trạng thái
        is_available BIT NOT NULL DEFAULT 1,
        is_featured BIT DEFAULT 0, -- Sản phẩm nổi bật
        is_new BIT DEFAULT 0, -- Sản phẩm mới
        is_on_sale BIT DEFAULT 0, -- Đang khuyến mãi
        is_deleted BIT NOT NULL DEFAULT 0, -- Soft delete flag
        
        -- SEO
        slug NVARCHAR(255) UNIQUE, -- URL-friendly name
        meta_title NVARCHAR(255),
        meta_description NVARCHAR(500),
        meta_keywords NVARCHAR(500),
        
        -- Audit
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        created_by UNIQUEIDENTIFIER, -- Reference to IdentityDB.users.id
        updated_by UNIQUEIDENTIFIER,
        
        CONSTRAINT FK_products_categories FOREIGN KEY (category_id) REFERENCES categories(id),
        CONSTRAINT FK_products_suppliers FOREIGN KEY (supplier_id) REFERENCES suppliers(id)
    );
    
    CREATE INDEX IX_products_sku ON products(sku);
    CREATE INDEX IX_products_barcode ON products(barcode);
    CREATE INDEX IX_products_name ON products(name);
    CREATE INDEX IX_products_slug ON products(slug);
    CREATE INDEX IX_products_category_id ON products(category_id);
    CREATE INDEX IX_products_brand ON products(brand);
    CREATE INDEX IX_products_is_available ON products(is_available);
    CREATE INDEX IX_products_is_featured ON products(is_featured);
    CREATE INDEX IX_products_is_on_sale ON products(is_on_sale);
    CREATE INDEX IX_products_price ON products(price);
    CREATE INDEX IX_products_created_at ON products(created_at);
    CREATE INDEX IX_products_is_deleted ON products(is_deleted);
    CREATE INDEX IX_products_supplier_id ON products(supplier_id);
END
GO

-- =====================================================
-- Migration: Add supplier_id to products if missing
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('products') AND name = 'supplier_id')
BEGIN
    ALTER TABLE products ADD supplier_id UNIQUEIDENTIFIER NULL
        CONSTRAINT FK_products_suppliers FOREIGN KEY REFERENCES suppliers(id);
    CREATE INDEX IX_products_supplier_id ON products(supplier_id);
END
GO

-- =====================================================
-- Insert default categories for Bách Hóa
-- =====================================================
IF NOT EXISTS (SELECT * FROM categories)
BEGIN
    INSERT INTO categories (id, name, status, created_at) VALUES
    -- Fixed UUID for categories (for reference in other databases)
    ('C0000001-0001-0001-0001-000000000001', N'Rau Củ Quả Tươi', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000002', N'Rau Ăn Lá', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000003', N'Củ Quả', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000004', N'Trái Cây Tươi', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000005', N'Thịt Tươi Sống', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000006', N'Thịt Heo', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000007', N'Thịt Bò', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000008', N'Thịt Gà', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000009', N'Cá & Hải Sản', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000010', N'Trứng & Sữa', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000011', N'Sữa Các Loại', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000012', N'Sữa Chua & Pho Mát', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000013', N'Gạo & Bột', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000014', N'Mì, Miến, Phở, Bún', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000015', N'Dầu Ăn', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000016', N'Gia Vị', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000017', N'Nước Giải Khát', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000018', N'Bia & Rượu', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000019', N'Cà Phê & Trà', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000020', N'Snack & Bánh Kẹo', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000021', N'Đồ Ăn Liền', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000022', N'Chăm Sóc Cá Nhân', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000023', N'Vệ Sinh Nhà Cửa', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000024', N'Đồ Gia Dụng', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000025', N'Đồ Dùng Nhà Bếp', 'ACTIVE', GETUTCDATE()),
    ('C0000001-0001-0001-0001-000000000026', N'Khác', 'ACTIVE', GETUTCDATE());
END
GO

-- =====================================================
-- Insert sample suppliers (with fixed UUIDs)
-- =====================================================
IF NOT EXISTS (SELECT * FROM suppliers)
BEGIN
    INSERT INTO suppliers (id, name, phone, email, contact_person, status, created_at) VALUES
    ('50000001-0001-0001-0001-000000000001', N'Vinamilk Co.',      '02838293939', 'order@vinamilk.com.vn',  N'Nguyễn Văn A', 'ACTIVE', GETUTCDATE()),
    ('50000001-0001-0001-0001-000000000002', N'TH True Milk Co.',  '02436362699', 'supply@thmilk.vn',       N'Trần Thị B',   'ACTIVE', GETUTCDATE()),
    ('50000001-0001-0001-0001-000000000003', N'ST25 Co.',          '02963829123', 'st25@gaosieuthi.vn',     N'Lê Văn C',     'ACTIVE', GETUTCDATE()),
    ('50000001-0001-0001-0001-000000000004', N'Jasmine Rice Co.',  '02963000456', 'supply@jasminerice.vn',  N'Phạm Thị D',   'ACTIVE', GETUTCDATE()),
    ('50000001-0001-0001-0001-000000000005', N'Đà Lạt Suppliers', '02633822999', 'rau@dalatsupply.vn',     N'Hoàng Văn E',  'ACTIVE', GETUTCDATE()),
    ('50000001-0001-0001-0001-000000000006', N'Cao Phong Farm',    '02183829081', 'farm@caophong.vn',       N'Vũ Thị F',     'ACTIVE', GETUTCDATE()),
    ('50000001-0001-0001-0001-000000000007', N'NZ Fresh Import',   '02838001234', 'import@nzfresh.com.vn',  N'Đỗ Văn G',     'ACTIVE', GETUTCDATE());
END
GO

-- =====================================================
-- Insert sample products (with fixed UUIDs)
-- =====================================================
IF NOT EXISTS (SELECT * FROM products)
BEGIN
    INSERT INTO products (
        id, sku, barcode, name, category_id, supplier_id, brand, origin,
        price, original_price, cost_price, unit, weight, is_perishable, shelf_life_days,
        storage_instructions, is_available, slug, created_by, created_at
    ) VALUES
    -- Rau củ (Category: Rau Ăn Lá)
    ('F0000001-0001-0001-0001-000000000001', 'RAU-001', '8934560001234', N'Rau Muống',
     'C0000001-0001-0001-0001-000000000002', '50000001-0001-0001-0001-000000000005',
     N'Đà Lạt', N'Việt Nam',
     15000, 20000, 10000, N'Kg', 1.0, 1, 3,
     N'Bảo quản nơi khô ráo, thoáng mát', 1, 'rau-muong',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    ('F0000001-0001-0001-0001-000000000002', 'RAU-002', '8934560001241', N'Cải Thảo',
     'C0000001-0001-0001-0001-000000000002', '50000001-0001-0001-0001-000000000005',
     N'Đà Lạt', N'Việt Nam',
     25000, 30000, 18000, N'Kg', 1.0, 1, 5,
     N'Bảo quản ngăn mát tủ lạnh', 1, 'cai-thao',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    -- Trái cây (Category: Trái Cây Tươi)
    ('F0000001-0001-0001-0001-000000000003', 'TC-001', '8934560002234', N'Cam Sành',
     'C0000001-0001-0001-0001-000000000004', '50000001-0001-0001-0001-000000000006',
     N'Cao Phong', N'Việt Nam',
     35000, 40000, 25000, N'Kg', 1.0, 1, 7,
     N'Bảo quản nơi khô ráo, thoáng mát', 1, 'cam-sanh',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    ('F0000001-0001-0001-0001-000000000004', 'TC-002', '8934560002241', N'Táo Envy',
     'C0000001-0001-0001-0001-000000000004', '50000001-0001-0001-0001-000000000007',
     N'Envy', N'New Zealand',
     120000, 150000, 95000, N'Kg', 1.0, 1, 14,
     N'Bảo quản ngăn mát tủ lạnh', 1, 'tao-envy',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    -- Sữa (Category: Sữa Các Loại)
    ('F0000001-0001-0001-0001-000000000005', 'SUA-001', '8934560003234', N'Sữa Tươi Vinamilk 100%',
     'C0000001-0001-0001-0001-000000000011', '50000001-0001-0001-0001-000000000001',
     N'Vinamilk', N'Việt Nam',
     32000, 35000, 24000, N'Hộp', 1.0, 1, 30,
     N'Bảo quản ngăn mát tủ lạnh, dùng trong vòng 2 ngày sau khi mở', 1, 'sua-tuoi-vinamilk',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    ('F0000001-0001-0001-0001-000000000006', 'SUA-002', '8934560003241', N'Sữa TH True Milk',
     'C0000001-0001-0001-0001-000000000011', '50000001-0001-0001-0001-000000000002',
     N'TH True Milk', N'Việt Nam',
     38000, 42000, 28000, N'Hộp', 1.0, 1, 30,
     N'Bảo quản ngăn mát tủ lạnh', 1, 'sua-th-true-milk',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    -- Gạo (Category: Gạo & Bột)
    ('F0000001-0001-0001-0001-000000000007', 'GAO-001', '8934560004234', N'Gạo ST25',
     'C0000001-0001-0001-0001-000000000013', '50000001-0001-0001-0001-000000000003',
     N'ST25', N'Việt Nam',
     180000, 200000, 150000, N'Kg', 5.0, 0, 365,
     N'Bảo quản nơi khô ráo, thoáng mát, tránh ánh nắng trực tiếp', 1, 'gao-st25',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE()),

    ('F0000001-0001-0001-0001-000000000008', 'GAO-002', '8934560004241', N'Gạo Jasmine',
     'C0000001-0001-0001-0001-000000000013', '50000001-0001-0001-0001-000000000004',
     N'Jasmine', N'Việt Nam',
     120000, 140000, 95000, N'Kg', 5.0, 0, 365,
     N'Bảo quản nơi khô ráo', 1, 'gao-jasmine',
     '11111111-1111-1111-1111-111111111111', GETUTCDATE());
END
GO

PRINT '===============================================';
PRINT 'Product Management Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Tables: categories, suppliers, products';
PRINT 'Categories Created: 26 categories with fixed UUIDs';
PRINT 'Suppliers Created: 7 suppliers with fixed UUIDs';
PRINT '  - 50000001-0001-0001-0001-000000000001: Vinamilk Co.';
PRINT '  - 50000001-0001-0001-0001-000000000002: TH True Milk Co.';
PRINT '  - 50000001-0001-0001-0001-000000000003: ST25 Co.';
PRINT '  - 50000001-0001-0001-0001-000000000004: Jasmine Rice Co.';
PRINT '  - 50000001-0001-0001-0001-000000000005: Đà Lạt Suppliers';
PRINT '  - 50000001-0001-0001-0001-000000000006: Cao Phong Farm';
PRINT '  - 50000001-0001-0001-0001-000000000007: NZ Fresh Import';
PRINT 'Sample Products: 8 products (each linked to a supplier_id)';
PRINT '  - F0000001-0001-0001-0001-000000000001: Rau Muống (RAU-001)';
PRINT '  - F0000001-0001-0001-0001-000000000002: Cải Thảo (RAU-002)';
PRINT '  - F0000001-0001-0001-0001-000000000003: Cam Sành (TC-001)';
PRINT '  - F0000001-0001-0001-0001-000000000004: Táo Envy (TC-002)';
PRINT '  - F0000001-0001-0001-0001-000000000005: Sữa Vinamilk (SUA-001)';
PRINT '  - F0000001-0001-0001-0001-000000000006: Sữa TH (SUA-002)';
PRINT '  - F0000001-0001-0001-0001-000000000007: Gạo ST25 (GAO-001)';
PRINT '  - F0000001-0001-0001-0001-000000000008: Gạo Jasmine (GAO-002)';
PRINT '===============================================';
GO
