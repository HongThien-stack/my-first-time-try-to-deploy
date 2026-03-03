-- =====================================================
-- Point of Sale Service
-- =====================================================
-- Database: POSDB
-- Purpose: Sales Transactions, Cashier Operations
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'POSDB')
BEGIN
    CREATE DATABASE POSDB;
END
GO

USE POSDB;
GO

-- =====================================================
-- Table: sales
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'sales')
BEGIN
    CREATE TABLE sales (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        sale_number NVARCHAR(50) NOT NULL UNIQUE, -- SALE-2024-001
        store_id UNIQUEIDENTIFIER NOT NULL, -- InventoryDB.warehouses.id (STORE type)
        cashier_id UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id (role: Store Staff)
        customer_id UNIQUEIDENTIFIER, -- IdentityDB.users.id (role: Customer) - Optional
        
        -- Totals
        subtotal DECIMAL(18,2) NOT NULL, -- Tổng tiền trước giảm giá
        discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Giảm giá
        tax_amount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Thuế (if any)
        total_amount DECIMAL(18,2) NOT NULL, -- Tổng tiền phải trả
        
        -- Payment
        payment_method NVARCHAR(50) NOT NULL, -- CASH | CARD | VNPAY | MOMO
        payment_status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | PAID | FAILED | REFUNDED
        payment_transaction_id UNIQUEIDENTIFIER, -- PaymentDB.payment_transactions.id
        
        -- Promotions
        promotion_id UNIQUEIDENTIFIER, -- PromotionLoyaltyDB.promotions.id
        voucher_code NVARCHAR(50), -- PromotionLoyaltyDB.vouchers.code
        points_earned INT DEFAULT 0, -- Loyalty points earned
        points_used INT DEFAULT 0, -- Loyalty points used (if any)
        
        -- Status
        status NVARCHAR(50) NOT NULL DEFAULT 'COMPLETED', -- PENDING | COMPLETED | CANCELLED
        
        -- Timestamps
        sale_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        
        -- Notes
        notes NVARCHAR(MAX)
    );
    
    CREATE INDEX IX_sales_sale_number ON sales(sale_number);
    CREATE INDEX IX_sales_store_id ON sales(store_id);
    CREATE INDEX IX_sales_cashier_id ON sales(cashier_id);
    CREATE INDEX IX_sales_customer_id ON sales(customer_id);
    CREATE INDEX IX_sales_payment_method ON sales(payment_method);
    CREATE INDEX IX_sales_payment_status ON sales(payment_status);
    CREATE INDEX IX_sales_status ON sales(status);
    CREATE INDEX IX_sales_date ON sales(sale_date);
    CREATE INDEX IX_sales_created_at ON sales(created_at);
END
GO

-- =====================================================
-- Table: sale_items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'sale_items')
BEGIN
    CREATE TABLE sale_items (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        sale_id UNIQUEIDENTIFIER NOT NULL,
        product_id UNIQUEIDENTIFIER NOT NULL, -- ProductDB.products.id
        product_name NVARCHAR(255) NOT NULL, -- Snapshot at time of sale
        sku NVARCHAR(50) NOT NULL, -- Snapshot
        barcode NVARCHAR(50), -- Snapshot
        
        quantity INT NOT NULL,
        unit_price DECIMAL(18,2) NOT NULL, -- Giá tại thời điểm bán
        discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Giảm giá per item
        line_total AS (quantity * unit_price - discount_amount) PERSISTED,
        
        -- Promotions applied to this item
        promotion_applied BIT DEFAULT 0,
        promotion_id UNIQUEIDENTIFIER, -- PromotionLoyaltyDB.promotions.id
        
        CONSTRAINT FK_sale_items_sales FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_sale_items_sale_id ON sale_items(sale_id);
    CREATE INDEX IX_sale_items_product_id ON sale_items(product_id);
END
GO

-- =====================================================
-- Table: payments (Optional - simple tracking)
-- For full payment gateway integration, use PaymentService
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payments')
BEGIN
    CREATE TABLE payments (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        sale_id UNIQUEIDENTIFIER NOT NULL,
        payment_method NVARCHAR(50) NOT NULL, -- CASH | CARD | VNPAY | MOMO
        amount DECIMAL(18,2) NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | COMPLETED | FAILED
        
        -- For cash payments
        cash_received DECIMAL(18,2), -- Tiền khách đưa
        cash_change DECIMAL(18,2), -- Tiền thừa
        
        -- For online payments
        transaction_reference NVARCHAR(255), -- Reference from payment gateway
        
        payment_date DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_payments_sales FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_payments_sale_id ON payments(sale_id);
    CREATE INDEX IX_payments_method ON payments(payment_method);
    CREATE INDEX IX_payments_status ON payments(status);
    CREATE INDEX IX_payments_date ON payments(payment_date);
END
GO

-- =====================================================
-- Insert sample sales data
-- =====================================================
IF NOT EXISTS (SELECT * FROM sales)
BEGIN
    -- Cash sale
    INSERT INTO sales (
        id, sale_number, store_id, cashier_id, customer_id,
        subtotal, discount_amount, total_amount,
        payment_method, payment_status, status,
        points_earned, sale_date, created_at
    ) VALUES (
        'SALE0001-0001-0001-0001-000000000001',
        'SALE-2024-001',
        'B0000001-0001-0001-0001-000000000001', -- Cửa Hàng Thủ Đức
        '33333333-3333-3333-3333-333333333331', -- cashier1@company.com
        '55555555-5555-5555-5555-555555555551', -- customer1@gmail.com
        76000, 0, 76000,
        'CASH', 'PAID', 'COMPLETED',
        76, -- 1 point per 1000 VND
        '2024-03-01 10:30:00', GETUTCDATE()
    ),
    
    -- VNPay sale with discount
    (
        'SALE0001-0001-0001-0001-000000000002',
        'SALE-2024-002',
        'B0000001-0001-0001-0001-000000000001', -- Cửa Hàng Thủ Đức
        '33333333-3333-3333-3333-333333333331', -- cashier1@company.com
        '55555555-5555-5555-5555-555555555552', -- customer2@gmail.com
        420000, 42000, 378000, -- 10% discount
        'VNPAY', 'PAID', 'COMPLETED',
        378, -- Points earned
        '2024-03-01 14:15:00', GETUTCDATE()
    ),
    
    -- Momo sale
    (
        'SALE0001-0001-0001-0001-000000000003',
        'SALE-2024-003',
        'B0000001-0001-0001-0001-000000000001', -- Cửa Hàng Thủ Đức
        '33333333-3333-3333-3333-333333333332', -- cashier2@company.com
        '55555555-5555-5555-5555-555555555553', -- customer3@gmail.com
        195000, 0, 195000,
        'MOMO', 'PAID', 'COMPLETED',
        195,
        '2024-03-02 09:45:00', GETUTCDATE()
    ),
    
    -- Cash sale without customer account
    (
        'SALE0001-0001-0001-0001-000000000004',
        'SALE-2024-004',
        'B0000001-0001-0001-0001-000000000002', -- Cửa Hàng Quận 1
        '33333333-3333-3333-3333-333333333333', -- cashier3@company.com
        NULL, -- Anonymous customer
        150000, 15000, 135000, -- Voucher applied
        'CASH', 'PAID', 'COMPLETED',
        0, -- No points (no customer account)
        '2024-03-02 11:20:00', GETUTCDATE()
    ),
    
    -- Pending payment
    (
        'SALE0001-0001-0001-0001-000000000005',
        'SALE-2024-005',
        'B0000001-0001-0001-0001-000000000001', -- Cửa Hàng Thủ Đức
        '33333333-3333-3333-3333-333333333331', -- cashier1@company.com
        '55555555-5555-5555-5555-555555555554', -- customer4@gmail.com
        280000, 0, 280000,
        'VNPAY', 'PENDING', 'PENDING',
        0,
        '2024-03-03 16:00:00', GETUTCDATE()
    );
END
GO

-- =====================================================
-- Insert sample sale items
-- =====================================================
IF NOT EXISTS (SELECT * FROM sale_items)
BEGIN
    -- SALE-2024-001 items (Sữa Vinamilk × 2)
    INSERT INTO sale_items (id, sale_id, product_id, product_name, sku, barcode, quantity, unit_price, discount_amount) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000001', 'F0000001-0001-0001-0001-000000000005', N'Sữa Tươi Vinamilk 100%', 'SUA-001', '8934560003234', 2, 38000, 0);
    
    -- SALE-2024-002 items (Gạo ST25 × 2, Sữa TH × 1)
    INSERT INTO sale_items (id, sale_id, product_id, product_name, sku, barcode, quantity, unit_price, discount_amount) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000007', N'Gạo ST25', 'GAO-001', '8934560004234', 2, 180000, 36000),
    (NEWID(), 'SALE0001-0001-0001-0001-000000000002', 'F0000001-0001-0001-0001-000000000006', N'Sữa TH True Milk', 'SUA-002', '8934560003241', 1, 42000, 4200);
    
    -- SALE-2024-003 items (Cam Sành × 5 kg, Rau Muống × 2 kg)
    INSERT INTO sale_items (id, sale_id, product_id, product_name, sku, barcode, quantity, unit_price, discount_amount) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000003', N'Cam Sành', 'TC-001', '8934560002234', 5, 35000, 0),
    (NEWID(), 'SALE0001-0001-0001-0001-000000000003', 'F0000001-0001-0001-0001-000000000001', N'Rau Muống', 'RAU-001', '8934560001234', 2, 20000, 0);
    
    -- SALE-2024-004 items (Táo Envy × 1 kg, Sữa Vinamilk × 1)
    INSERT INTO sale_items (id, sale_id, product_id, product_name, sku, barcode, quantity, unit_price, discount_amount) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000004', 'F0000001-0001-0001-0001-000000000004', N'Táo Envy', 'TC-002', '8934560002241', 1, 150000, 0),
    (NEWID(), 'SALE0001-0001-0001-0001-000000000004', 'F0000001-0001-0001-0001-000000000005', N'Sữa Tươi Vinamilk 100%', 'SUA-001', '8934560003234', 1, 35000, 0);
    
    -- SALE-2024-005 items (Gạo Jasmine × 2)
    INSERT INTO sale_items (id, sale_id, product_id, product_name, sku, barcode, quantity, unit_price, discount_amount) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000005', 'F0000001-0001-0001-0001-000000000008', N'Gạo Jasmine', 'GAO-002', '8934560004241', 2, 140000, 0);
END
GO

-- =====================================================
-- Insert sample payments
-- =====================================================
IF NOT EXISTS (SELECT * FROM payments)
BEGIN
    -- Cash payment for SALE-2024-001
    INSERT INTO payments (id, sale_id, payment_method, amount, status, cash_received, cash_change, payment_date) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000001', 'CASH', 76000, 'COMPLETED', 100000, 24000, '2024-03-01 10:30:00');
    
    -- VNPay payment for SALE-2024-002
    INSERT INTO payments (id, sale_id, payment_method, amount, status, transaction_reference, payment_date) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000002', 'VNPAY', 378000, 'COMPLETED', 'VNPAY-2024-0001-TXN123', '2024-03-01 14:16:00');
    
    -- Momo payment for SALE-2024-003
    INSERT INTO payments (id, sale_id, payment_method, amount, status, transaction_reference, payment_date) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000003', 'MOMO', 195000, 'COMPLETED', 'MOMO-2024-0001-TXN456', '2024-03-02 09:46:00');
    
    -- Cash payment for SALE-2024-004
    INSERT INTO payments (id, sale_id, payment_method, amount, status, cash_received, cash_change, payment_date) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000004', 'CASH', 135000, 'COMPLETED', 150000, 15000, '2024-03-02 11:20:00');
    
    -- Pending VNPay payment for SALE-2024-005
    INSERT INTO payments (id, sale_id, payment_method, amount, status, payment_date) VALUES
    (NEWID(), 'SALE0001-0001-0001-0001-000000000005', 'VNPAY', 280000, 'PENDING', '2024-03-03 16:00:00');
END
GO

PRINT '===============================================';
PRINT 'Point of Sale Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Total Tables: 3 tables';
PRINT 'Sample Data:';
PRINT '  - 5 Sales transactions';
PRINT '  - 9 Sale items';
PRINT '  - 5 Payment records';
PRINT '';
PRINT 'Payment Methods: CASH, VNPAY, MOMO';
PRINT 'Stores: Thủ Đức, Quận 1';
PRINT 'Total Sales Value: 1,064,000 VND';
PRINT '===============================================';
GO
