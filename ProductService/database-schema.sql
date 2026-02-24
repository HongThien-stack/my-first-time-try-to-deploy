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
        
        -- Thông tin cơ bản
        sku NVARCHAR(50) NOT NULL UNIQUE, -- Mã SKU
        barcode NVARCHAR(50) UNIQUE, -- Mã vạch
        name NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX), -- Mô tả chi tiết
        
        -- Phân loại
        category_id UNIQUEIDENTIFIER NOT NULL,
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
        
        CONSTRAINT FK_products_categories FOREIGN KEY (category_id) REFERENCES categories(id)
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
END
GO

-- =====================================================
-- Insert default categories for Bách Hóa Xanh
-- =====================================================
IF NOT EXISTS (SELECT * FROM categories)
BEGIN
    INSERT INTO categories (id, name, status, created_at) VALUES
    -- Rau củ quả tươi
    (NEWID(), N'Rau Củ Quả Tươi', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Rau Ăn Lá', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Củ Quả', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Trái Cây Tươi', 'ACTIVE', GETUTCDATE()),
    
    -- Thịt, cá, hải sản
    (NEWID(), N'Thịt Tươi Sống', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Thịt Heo', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Thịt Bò', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Thịt Gà', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Cá & Hải Sản', 'ACTIVE', GETUTCDATE()),
    
    -- Trứng, sữa
    (NEWID(), N'Trứng & Sữa', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Sữa Các Loại', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Sữa Chua & Pho Mát', 'ACTIVE', GETUTCDATE()),
    
    -- Thực phẩm khô
    (NEWID(), N'Gạo & Bột', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Mì, Miến, Phở, Bún', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Dầu Ăn', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Gia Vị', 'ACTIVE', GETUTCDATE()),
    
    -- Đồ uống
    (NEWID(), N'Nước Giải Khát', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Bia & Rượu', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Cà Phê & Trà', 'ACTIVE', GETUTCDATE()),
    
    -- Đồ ăn vặt
    (NEWID(), N'Snack & Bánh Kẹo', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Đồ Ăn Liền', 'ACTIVE', GETUTCDATE()),
    
    -- Chăm sóc cá nhân
    (NEWID(), N'Chăm Sóc Cá Nhân', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Vệ Sinh Nhà Cửa', 'ACTIVE', GETUTCDATE()),
    
    -- Gia dụng
    (NEWID(), N'Đồ Gia Dụng', 'ACTIVE', GETUTCDATE()),
    (NEWID(), N'Đồ Dùng Nhà Bếp', 'ACTIVE', GETUTCDATE());
END
GO

-- =====================================================
-- Insert sample products
-- =====================================================
DECLARE @CategoryRauId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM categories WHERE name = N'Rau Ăn Lá');
DECLARE @CategoryTraiCayId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM categories WHERE name = N'Trái Cây Tươi');
DECLARE @CategorySuaId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM categories WHERE name = N'Sữa Các Loại');
DECLARE @CategoryGaoId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM categories WHERE name = N'Gạo & Bột');

IF NOT EXISTS (SELECT * FROM products)
BEGIN
    INSERT INTO products (
        id, sku, barcode, name, category_id, brand, origin,
        price, original_price, unit, weight, is_perishable, shelf_life_days,
        storage_instructions, is_available, slug, created_at
    ) VALUES
    -- Rau củ
    (NEWID(), 'RAU-001', '8934560001234', N'Rau Muống', 
     @CategoryRauId, N'Đà Lạt', N'Việt Nam', 15000, 20000, N'Kg', 1.0, 1, 3,
     N'Bảo quản nơi khô ráo, thoáng mát', 1, 'rau-muong', GETUTCDATE()),
    
    (NEWID(), 'RAU-002', '8934560001241', N'Cải Thảo', 
     @CategoryRauId, N'Đà Lạt', N'Việt Nam', 25000, 30000, N'Kg', 1.0, 1, 5,
     N'Bảo quản ngăn mát tủ lạnh', 1, 'cai-thao', GETUTCDATE()),
    
    -- Trái cây
    (NEWID(), 'TC-001', '8934560002234', N'Cam Sành', 
     @CategoryTraiCayId, N'Cao Phong', N'Việt Nam', 35000, 40000, N'Kg', 1.0, 1, 7,
     N'Bảo quản nơi khô ráo, thoáng mát', 1, 'cam-sanh', GETUTCDATE()),
    
    (NEWID(), 'TC-002', '8934560002241', N'Táo Envy', 
     @CategoryTraiCayId, N'Envy', N'New Zealand', 120000, 150000, N'Kg', 1.0, 1, 14,
     N'Bảo quản ngăn mát tủ lạnh', 1, 'tao-envy', GETUTCDATE()),
    
    -- Sữa
    (NEWID(), 'SUA-001', '8934560003234', N'Sữa Tươi Vinamilk 100%', 
     @CategorySuaId, N'Vinamilk', N'Việt Nam', 32000, 35000, N'Hộp', 1.0, 1, 30,
     N'Bảo quản ngăn mát tủ lạnh, dùng trong vòng 2 ngày sau khi mở', 1, 'sua-tuoi-vinamilk', GETUTCDATE()),
    
    (NEWID(), 'SUA-002', '8934560003241', N'Sữa TH True Milk', 
     @CategorySuaId, N'TH True Milk', N'Việt Nam', 38000, 42000, N'Hộp', 1.0, 1, 30,
     N'Bảo quản ngăn mát tủ lạnh', 1, 'sua-th-true-milk', GETUTCDATE()),
    
    -- Gạo
    (NEWID(), 'GAO-001', '8934560004234', N'Gạo ST25', 
     @CategoryGaoId, N'ST25', N'Việt Nam', 180000, 200000, N'Kg', 5.0, 0, 365,
     N'Bảo quản nơi khô ráo, thoáng mát, tránh ánh nắng trực tiếp', 1, 'gao-st25', GETUTCDATE()),
    
    (NEWID(), 'GAO-002', '8934560004241', N'Gạo Jasmine', 
     @CategoryGaoId, N'Jasmine', N'Việt Nam', 120000, 140000, N'Kg', 5.0, 0, 365,
     N'Bảo quản nơi khô ráo', 1, 'gao-jasmine', GETUTCDATE());
END
GO

PRINT '===============================================';
PRINT 'Product Management Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Categories Created: 26 danh mục cho Bách Hóa Xanh';
PRINT 'Sample Products: 8 sản phẩm mẫu';
PRINT '===============================================';
GO
