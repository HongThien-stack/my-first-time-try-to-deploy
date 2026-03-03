-- =====================================================
-- Promotion & Loyalty Program Service
-- =====================================================
-- Database: PromotionLoyaltyDB
-- Purpose: Promotions, Vouchers, Loyalty Program, Rewards
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PromotionLoyaltyDB')
BEGIN
    CREATE DATABASE PromotionLoyaltyDB;
END
GO

USE PromotionLoyaltyDB;
GO

-- =====================================================
-- PROMOTION MODULE (6 tables)
-- =====================================================

-- =====================================================
-- Table: promotions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'promotions')
BEGIN
    CREATE TABLE promotions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        promotion_code NVARCHAR(50) NOT NULL UNIQUE, -- FLASH10, SAVE50K
        name NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX),
        
        -- Type
        promotion_type NVARCHAR(50) NOT NULL, -- PERCENTAGE | FIXED | BUY_X_GET_Y | FREE_SHIPPING
        
        -- Discount values
        discount_percentage DECIMAL(5,2), -- For PERCENTAGE type (e.g., 10.00 = 10%)
        discount_amount DECIMAL(18,2), -- For FIXED type
        
        -- Conditions
        min_purchase_amount DECIMAL(18,2), -- Minimum purchase to apply
        max_discount_amount DECIMAL(18,2), -- Cap for PERCENTAGE discount
        
        -- Applicable to
        applicable_to NVARCHAR(50) NOT NULL DEFAULT 'ALL', -- ALL | SPECIFIC_PRODUCTS | SPECIFIC_CATEGORIES
        
        -- Date range
        start_date DATETIME2 NOT NULL,
        end_date DATETIME2 NOT NULL,
        
        -- Usage limits
        usage_limit INT, -- Total usage limit (NULL = unlimited)
        usage_limit_per_customer INT DEFAULT 1, -- Per customer limit
        usage_count INT NOT NULL DEFAULT 0, -- Current usage count
        
        -- Status
        is_active BIT NOT NULL DEFAULT 1,
        is_deleted BIT NOT NULL DEFAULT 0,
        
        -- Audit
        created_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_promotions_code ON promotions(promotion_code);
    CREATE INDEX IX_promotions_type ON promotions(promotion_type);
    CREATE INDEX IX_promotions_dates ON promotions(start_date, end_date);
    CREATE INDEX IX_promotions_is_active ON promotions(is_active);
END
GO

-- =====================================================
-- Table: promotion_rules
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'promotion_rules')
BEGIN
    CREATE TABLE promotion_rules (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        promotion_id UNIQUEIDENTIFIER NOT NULL,
        rule_type NVARCHAR(50) NOT NULL, -- PRODUCT | CATEGORY | CUSTOMER_TIER | DAY_OF_WEEK | TIME_RANGE
        
        -- Rule values (JSON for flexibility)
        rule_condition NVARCHAR(MAX) NOT NULL, -- JSON: {"product_ids": [...]} or {"category_ids": [...]}
        
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_promotion_rules_promotions FOREIGN KEY (promotion_id) REFERENCES promotions(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_rules_promotion_id ON promotion_rules(promotion_id);
    CREATE INDEX IX_rules_type ON promotion_rules(rule_type);
END
GO

-- =====================================================
-- Table: vouchers
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'vouchers')
BEGIN
    CREATE TABLE vouchers (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        voucher_code NVARCHAR(50) NOT NULL UNIQUE, -- SAVE50K-001, SAVE50K-002
        promotion_id UNIQUEIDENTIFIER NOT NULL,
        
        -- Assignment
        customer_id UNIQUEIDENTIFIER, -- IdentityDB.users.id (NULL = public voucher)
        
        -- Status
        is_used BIT NOT NULL DEFAULT 0,
        used_at DATETIME2,
        used_in_sale_id UNIQUEIDENTIFIER, -- POSDB.sales.id
        
        -- Expiry
        expires_at DATETIME2,
        
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_vouchers_promotions FOREIGN KEY (promotion_id) REFERENCES promotions(id)
    );
    
    CREATE INDEX IX_vouchers_code ON vouchers(voucher_code);
    CREATE INDEX IX_vouchers_promotion_id ON vouchers(promotion_id);
    CREATE INDEX IX_vouchers_customer_id ON vouchers(customer_id);
    CREATE INDEX IX_vouchers_is_used ON vouchers(is_used);
END
GO

-- =====================================================
-- Table: promotion_usages
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'promotion_usages')
BEGIN
    CREATE TABLE promotion_usages (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        promotion_id UNIQUEIDENTIFIER NOT NULL,
        voucher_id UNIQUEIDENTIFIER, -- If voucher used
        customer_id UNIQUEIDENTIFIER, -- IdentityDB.users.id
        sale_id UNIQUEIDENTIFIER NOT NULL, -- POSDB.sales.id
        
        -- Discount applied
        discount_amount DECIMAL(18,2) NOT NULL,
        order_amount DECIMAL(18,2) NOT NULL, -- Total before discount
        
        used_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_usage_promotions FOREIGN KEY (promotion_id) REFERENCES promotions(id),
        CONSTRAINT FK_usage_vouchers FOREIGN KEY (voucher_id) REFERENCES vouchers(id)
    );
    
    CREATE INDEX IX_usage_promotion_id ON promotion_usages(promotion_id);
    CREATE INDEX IX_usage_customer_id ON promotion_usages(customer_id);
    CREATE INDEX IX_usage_sale_id ON promotion_usages(sale_id);
    CREATE INDEX IX_usage_used_at ON promotion_usages(used_at);
END
GO

-- =====================================================
-- Table: sale_promotions (Link sales to promotions)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'sale_promotions')
BEGIN
    CREATE TABLE sale_promotions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        sale_id UNIQUEIDENTIFIER NOT NULL, -- POSDB.sales.id
        promotion_id UNIQUEIDENTIFIER NOT NULL,
        discount_amount DECIMAL(18,2) NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_sale_promotions_promotions FOREIGN KEY (promotion_id) REFERENCES promotions(id)
    );
    
    CREATE INDEX IX_sale_promotions_sale_id ON sale_promotions(sale_id);
    CREATE INDEX IX_sale_promotions_promotion_id ON sale_promotions(promotion_id);
END
GO

-- =====================================================
-- LOYALTY MODULE (6 tables)
-- =====================================================

-- =====================================================
-- Table: membership_tiers
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'membership_tiers')
BEGIN
    CREATE TABLE membership_tiers (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tier_name NVARCHAR(50) NOT NULL UNIQUE, -- BRONZE, SILVER, GOLD, PLATINUM
        tier_level INT NOT NULL UNIQUE, -- 1, 2, 3, 4
        
        -- Requirements
        min_points INT NOT NULL, -- Minimum points to reach this tier
        min_purchases DECIMAL(18,2), -- Minimum purchase amount (VND)
        
        -- Benefits
        discount_percentage DECIMAL(5,2) DEFAULT 0, -- Discount on all purchases
        points_multiplier DECIMAL(4,2) DEFAULT 1.0, -- Points earning rate (1.5x, 2.0x)
        birthday_bonus_points INT DEFAULT 0,
        
        -- Display
        color NVARCHAR(20), -- For UI: #FFD700
        icon_url NVARCHAR(500),
        
        is_active BIT NOT NULL DEFAULT 1,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_tiers_level ON membership_tiers(tier_level);
    CREATE INDEX IX_tiers_name ON membership_tiers(tier_name);
END
GO

-- =====================================================
-- Table: customer_loyalty
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'customer_loyalty')
BEGIN
    CREATE TABLE customer_loyalty (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        customer_id UNIQUEIDENTIFIER NOT NULL UNIQUE, -- IdentityDB.users.id
        membership_tier_id UNIQUEIDENTIFIER NOT NULL,
        
        -- Points
        total_points INT NOT NULL DEFAULT 0, -- Lifetime points earned
        available_points INT NOT NULL DEFAULT 0, -- Current available points
        used_points INT NOT NULL DEFAULT 0, -- Points spent on rewards
        expired_points INT NOT NULL DEFAULT 0,
        
        -- Purchase history
        total_purchases DECIMAL(18,2) NOT NULL DEFAULT 0, -- Lifetime purchase value
        purchase_count INT NOT NULL DEFAULT 0, -- Number of purchases
        
        -- Dates
        joined_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        last_purchase_at DATETIME2,
        tier_upgraded_at DATETIME2,
        
        CONSTRAINT FK_loyalty_tiers FOREIGN KEY (membership_tier_id) REFERENCES membership_tiers(id)
    );
    
    CREATE INDEX IX_loyalty_customer_id ON customer_loyalty(customer_id);
    CREATE INDEX IX_loyalty_tier_id ON customer_loyalty(membership_tier_id);
    CREATE INDEX IX_loyalty_available_points ON customer_loyalty(available_points);
END
GO

-- =====================================================
-- Table: points_transactions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'points_transactions')
BEGIN
    CREATE TABLE points_transactions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        customer_id UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        transaction_type NVARCHAR(50) NOT NULL, -- EARNED | REDEEMED | EXPIRED | ADJUSTED | BONUS
        points INT NOT NULL, -- Positive for earning, negative for spending
        
        -- Reference
        sale_id UNIQUEIDENTIFIER, -- POSDB.sales.id (for EARNED)
        redemption_id UNIQUEIDENTIFIER, -- reward_redemptions.id (for REDEEMED)
        
        -- Balance
        balance_before INT NOT NULL,
        balance_after INT NOT NULL,
        
        description NVARCHAR(500),
        
        -- Expiry (for EARNED points)
        expires_at DATE, -- Points expire after 12 months
        
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        created_by UNIQUEIDENTIFIER -- IdentityDB.users.id (for ADJUSTED)
    );
    
    CREATE INDEX IX_points_customer_id ON points_transactions(customer_id);
    CREATE INDEX IX_points_type ON points_transactions(transaction_type);
    CREATE INDEX IX_points_sale_id ON points_transactions(sale_id);
    CREATE INDEX IX_points_created_at ON points_transactions(created_at);
    CREATE INDEX IX_points_expires_at ON points_transactions(expires_at);
END
GO

-- =====================================================
-- Table: rewards_catalog
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'rewards_catalog')
BEGIN
    CREATE TABLE rewards_catalog (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        reward_code NVARCHAR(50) NOT NULL UNIQUE, -- VOUCHER-50K, FREE-GIFT-1
        reward_name NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX),
        
        -- Type
        reward_type NVARCHAR(50) NOT NULL, -- DISCOUNT | PRODUCT | GIFT_CARD
        
        -- Cost
        points_cost INT NOT NULL,
        
        -- Value
        discount_amount DECIMAL(18,2), -- For DISCOUNT type
        product_id UNIQUEIDENTIFIER, -- ProductDB.products.id (for PRODUCT type)
        
        -- Stock
        stock_quantity INT,
        redeemed_count INT NOT NULL DEFAULT 0,
        
        -- Validity
        valid_from DATETIME2,
        valid_until DATETIME2,
        voucher_expiry_days INT DEFAULT 30, -- Generated voucher expires in X days
        
        -- Display
        image_url NVARCHAR(500),
        
        is_active BIT NOT NULL DEFAULT 1,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_rewards_code ON rewards_catalog(reward_code);
    CREATE INDEX IX_rewards_type ON rewards_catalog(reward_type);
    CREATE INDEX IX_rewards_is_active ON rewards_catalog(is_active);
END
GO

-- =====================================================
-- Table: reward_redemptions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'reward_redemptions')
BEGIN
    CREATE TABLE reward_redemptions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        customer_id UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        reward_id UNIQUEIDENTIFIER NOT NULL,
        
        -- Points spent
        points_spent INT NOT NULL,
        
        -- Status
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | COMPLETED | CANCELLED
        
        -- Voucher generated (if reward_type = DISCOUNT)
        voucher_generated NVARCHAR(50), -- SAVE50K-123
        voucher_expires_at DATETIME2,
        
        -- Fulfillment (if reward_type = PRODUCT)
        fulfillment_status NVARCHAR(50), -- PENDING | SHIPPED | DELIVERED
        fulfillment_date DATETIME2,
        
        redeemed_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        completed_at DATETIME2,
        
        notes NVARCHAR(MAX),
        
        CONSTRAINT FK_redemptions_rewards FOREIGN KEY (reward_id) REFERENCES rewards_catalog(id)
    );
    
    CREATE INDEX IX_redemptions_customer_id ON reward_redemptions(customer_id);
    CREATE INDEX IX_redemptions_reward_id ON reward_redemptions(reward_id);
    CREATE INDEX IX_redemptions_status ON reward_redemptions(status);
    CREATE INDEX IX_redemptions_redeemed_at ON reward_redemptions(redeemed_at);
END
GO

-- =====================================================
-- Table: tier_upgrades (History of tier changes)
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tier_upgrades')
BEGIN
    CREATE TABLE tier_upgrades (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        customer_id UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id
        from_tier_id UNIQUEIDENTIFIER,
        to_tier_id UNIQUEIDENTIFIER NOT NULL,
        upgrade_reason NVARCHAR(50) NOT NULL, -- POINTS | PURCHASES | MANUAL
        upgraded_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        upgraded_by UNIQUEIDENTIFIER, -- IdentityDB.users.id (for MANUAL)
        
        CONSTRAINT FK_upgrades_from_tier FOREIGN KEY (from_tier_id) REFERENCES membership_tiers(id),
        CONSTRAINT FK_upgrades_to_tier FOREIGN KEY (to_tier_id) REFERENCES membership_tiers(id)
    );
    
    CREATE INDEX IX_upgrades_customer_id ON tier_upgrades(customer_id);
    CREATE INDEX IX_upgrades_upgraded_at ON tier_upgrades(upgraded_at);
END
GO

-- =====================================================
-- Insert sample membership tiers
-- =====================================================
IF NOT EXISTS (SELECT * FROM membership_tiers)
BEGIN
    INSERT INTO membership_tiers (id, tier_name, tier_level, min_points, min_purchases, discount_percentage, points_multiplier, birthday_bonus_points, color) VALUES
    ('TIER0001-0001-0001-0001-000000000001', 'BRONZE', 1, 0, 0, 0, 1.0, 100, '#CD7F32'),
    ('TIER0001-0001-0001-0001-000000000002', 'SILVER', 2, 1000, 5000000, 3, 1.2, 200, '#C0C0C0'),
    ('TIER0001-0001-0001-0001-000000000003', 'GOLD', 3, 5000, 20000000, 5, 1.5, 500, '#FFD700'),
    ('TIER0001-0001-0001-0001-000000000004', 'PLATINUM', 4, 15000, 50000000, 10, 2.0, 1000, '#E5E4E2');
END
GO

-- =====================================================
-- Insert sample promotions
-- =====================================================
IF NOT EXISTS (SELECT * FROM promotions)
BEGIN
    INSERT INTO promotions (
        id, promotion_code, name, description, promotion_type,
        discount_percentage, discount_amount, min_purchase_amount, max_discount_amount,
        applicable_to, start_date, end_date, usage_limit, usage_limit_per_customer,
        is_active, created_by
    ) VALUES
    -- Percentage discount
    (
        'PROMO001-0001-0001-0001-000000000001',
        'FLASH10', N'Flash Sale 10%', N'Giảm 10% cho đơn hàng từ 100k',
        'PERCENTAGE', 10.00, NULL, 100000, 50000,
        'ALL', '2024-03-01', '2024-03-31', 1000, 1,
        1, '11111111-1111-1111-1111-111111111111'
    ),
    -- Fixed discount
    (
        'PROMO001-0001-0001-0001-000000000002',
        'SAVE50K', N'Giảm 50K', N'Giảm 50,000đ cho đơn hàng từ 500k',
        'FIXED', NULL, 50000, 500000, NULL,
        'ALL', '2024-03-01', '2024-04-30', 500, 2,
        1, '11111111-1111-1111-1111-111111111111'
    ),
    -- New customer discount
    (
        'PROMO001-0001-0001-0001-000000000003',
        'NEWCUST20', N'Khách Hàng Mới', N'Giảm 20% cho khách hàng mới (đơn đầu tiên)',
        'PERCENTAGE', 20.00, NULL, 200000, 100000,
        'ALL', '2024-03-01', '2024-12-31', NULL, 1,
        1, '11111111-1111-1111-1111-111111111111'
    );
END
GO

-- =====================================================
-- Insert sample vouchers
-- =====================================================
IF NOT EXISTS (SELECT * FROM vouchers)
BEGIN
    DECLARE @PromoId UNIQUEIDENTIFIER = 'PROMO001-0001-0001-0001-000000000002';
    
    -- Public vouchers (no customer assignment)
    INSERT INTO vouchers (id, voucher_code, promotion_id, customer_id, expires_at) VALUES
    ('VOUCH001-0001-0001-0001-000000000001', 'SAVE50K-001', @PromoId, NULL, '2024-04-30'),
    ('VOUCH001-0001-0001-0001-000000000002', 'SAVE50K-002', @PromoId, NULL, '2024-04-30'),
    ('VOUCH001-0001-0001-0001-000000000003', 'SAVE50K-003', @PromoId, NULL, '2024-04-30');
    
    -- Assigned to specific customer
    INSERT INTO vouchers (id, voucher_code, promotion_id, customer_id, expires_at) VALUES
    ('VOUCH001-0001-0001-0001-000000000004', 'SAVE50K-VIP001', @PromoId, '55555555-5555-5555-5555-555555555551', '2024-04-30');
END
GO

-- =====================================================
-- Insert sample customer loyalty accounts
-- =====================================================
IF NOT EXISTS (SELECT * FROM customer_loyalty)
BEGIN
    -- Customer 1: GOLD tier
    INSERT INTO customer_loyalty (
        id, customer_id, membership_tier_id,
        total_points, available_points, used_points,
        total_purchases, purchase_count,
        joined_at, last_purchase_at
    ) VALUES (
        'LOYAL001-0001-0001-0001-000000000001',
        '55555555-5555-5555-5555-555555555551', -- customer1@gmail.com
        'TIER0001-0001-0001-0001-000000000003', -- GOLD
        7500, 6200, 1300,
        25000000, 45,
        '2024-01-01', '2024-03-01'
    );
    
    -- Customer 2: SILVER tier
    INSERT INTO customer_loyalty (
        id, customer_id, membership_tier_id,
        total_points, available_points, used_points,
        total_purchases, purchase_count,
        joined_at, last_purchase_at
    ) VALUES (
        'LOYAL001-0001-0001-0001-000000000002',
        '55555555-5555-5555-5555-555555555552', -- customer2@gmail.com
        'TIER0001-0001-0001-0001-000000000002', -- SILVER
        2500, 2300, 200,
        8000000, 18,
        '2024-01-15', '2024-03-01'
    );
    
    -- Customer 3: BRONZE tier (new)
    INSERT INTO customer_loyalty (
        id, customer_id, membership_tier_id,
        total_points, available_points, used_points,
        total_purchases, purchase_count,
        joined_at, last_purchase_at
    ) VALUES (
        'LOYAL001-0001-0001-0001-000000000003',
        '55555555-5555-5555-5555-555555555553', -- customer3@gmail.com
        'TIER0001-0001-0001-0001-000000000001', -- BRONZE
        800, 800, 0,
        1500000, 5,
        '2024-02-20', '2024-03-02'
    );
    
    -- Customer 4: BRONZE tier (new)
    INSERT INTO customer_loyalty (
        id, customer_id, membership_tier_id,
        total_points, available_points, used_points,
        total_purchases, purchase_count,
        joined_at
    ) VALUES (
        'LOYAL001-0001-0001-0001-000000000004',
        '55555555-5555-5555-5555-555555555554', -- customer4@gmail.com
        'TIER0001-0001-0001-0001-000000000001', -- BRONZE
        0, 0, 0,
        0, 0,
        '2024-03-03'
    );
END
GO

-- =====================================================
-- Insert sample points transactions
-- =====================================================
IF NOT EXISTS (SELECT * FROM points_transactions)
BEGIN
    -- Customer 1 earned points from SALE-2024-001 (76,000 VND → 76 points)
    INSERT INTO points_transactions (
        id, customer_id, transaction_type, points, sale_id,
        balance_before, balance_after, description, expires_at
    ) VALUES (
        NEWID(),
        '55555555-5555-5555-5555-555555555551',
        'EARNED', 114, -- 76 × 1.5 (GOLD multiplier)
        'SALE0001-0001-0001-0001-000000000001',
        6086, 6200,
        N'Earned from purchase SALE-2024-001',
        '2025-03-01' -- Expires in 12 months
    );
    
    -- Customer 2 earned points from SALE-2024-002 (378,000 VND → 378 points)
    INSERT INTO points_transactions (
        id, customer_id, transaction_type, points, sale_id,
        balance_before, balance_after, description, expires_at
    ) VALUES (
        NEWID(),
        '55555555-5555-5555-5555-555555555552',
        'EARNED', 454, -- 378 × 1.2 (SILVER multiplier)
        'SALE0001-0001-0001-0001-000000000002',
        1846, 2300,
        N'Earned from purchase SALE-2024-002',
        '2025-03-01'
    );
    
    -- Customer 3 earned points from SALE-2024-003 (195,000 VND → 195 points)
    INSERT INTO points_transactions (
        id, customer_id, transaction_type, points, sale_id,
        balance_before, balance_after, description, expires_at
    ) VALUES (
        NEWID(),
        '55555555-5555-5555-5555-555555555553',
        'EARNED', 195, -- 195 × 1.0 (BRONZE multiplier)
        'SALE0001-0001-0001-0001-000000000003',
        605, 800,
        N'Earned from purchase SALE-2024-003',
        '2025-03-02'
    );
END
GO

-- =====================================================
-- Insert sample rewards catalog
-- =====================================================
IF NOT EXISTS (SELECT * FROM rewards_catalog)
BEGIN
    -- Voucher rewards
    INSERT INTO rewards_catalog (
        id, reward_code, reward_name, description, reward_type,
        points_cost, discount_amount, stock_quantity, redeemed_count,
        valid_from, valid_until, voucher_expiry_days, is_active
    ) VALUES
    (
        'REWARD01-0001-0001-0001-000000000001',
        'VOUCHER-50K', N'Voucher Giảm 50K', N'Voucher giảm 50,000đ cho đơn hàng từ 300k',
        'DISCOUNT', 500, 50000, 100, 5,
        '2024-03-01', '2024-12-31', 30, 1
    ),
    (
        'REWARD01-0001-0001-0001-000000000002',
        'VOUCHER-100K', N'Voucher Giảm 100K', N'Voucher giảm 100,000đ cho đơn hàng từ 600k',
        'DISCOUNT', 1000, 100000, 50, 2,
        '2024-03-01', '2024-12-31', 30, 1
    ),
    
    -- Product rewards
    (
        'REWARD01-0001-0001-0001-000000000003',
        'FREE-MILK', N'Miễn Phí 1 Hộp Sữa', N'Đổi 1 hộp Sữa Vinamilk miễn phí',
        'PRODUCT', 300, NULL, 200, 10,
        '2024-03-01', '2024-12-31', NULL, 1
    );
END
GO

-- =====================================================
-- Insert sample reward redemptions
-- =====================================================
IF NOT EXISTS (SELECT * FROM reward_redemptions)
BEGIN
    -- Customer 1 redeemed VOUCHER-50K
    INSERT INTO reward_redemptions (
        id, customer_id, reward_id, points_spent,
        status, voucher_generated, voucher_expires_at,
        redeemed_at, completed_at
    ) VALUES (
        'REDEMP01-0001-0001-0001-000000000001',
        '55555555-5555-5555-5555-555555555551', -- customer1@gmail.com
        'REWARD01-0001-0001-0001-000000000001', -- VOUCHER-50K
        500,
        'COMPLETED', 'SAVE50K-RDM001', '2024-04-03',
        '2024-03-03', '2024-03-03'
    );
END
GO

-- =====================================================
-- Insert sample tier upgrades
-- =====================================================
IF NOT EXISTS (SELECT * FROM tier_upgrades)
BEGIN
    -- Customer 2 upgraded from BRONZE to SILVER
    INSERT INTO tier_upgrades (
        id, customer_id, from_tier_id, to_tier_id,
        upgrade_reason, upgraded_at
    ) VALUES (
        NEWID(),
        '55555555-5555-5555-5555-555555555552',
        'TIER0001-0001-0001-0001-000000000001', -- BRONZE
        'TIER0001-0001-0001-0001-000000000002', -- SILVER
        'POINTS',
        '2024-02-15'
    );
    
    -- Customer 1 upgraded from SILVER to GOLD
    INSERT INTO tier_upgrades (
        id, customer_id, from_tier_id, to_tier_id,
        upgrade_reason, upgraded_at
    ) VALUES (
        NEWID(),
        '55555555-5555-5555-5555-555555555551',
        'TIER0001-0001-0001-0001-000000000002', -- SILVER
        'TIER0001-0001-0001-0001-000000000003', -- GOLD
        'PURCHASES',
        '2024-02-25'
    );
END
GO

PRINT '===============================================';
PRINT 'Promotion & Loyalty Program Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Total Tables: 12 tables';
PRINT '  - Promotion Module: 6 tables';
PRINT '  - Loyalty Module: 6 tables';
PRINT '';
PRINT 'Sample Data:';
PRINT '  - 4 Membership tiers (BRONZE, SILVER, GOLD, PLATINUM)';
PRINT '  - 3 Promotions (FLASH10, SAVE50K, NEWCUST20)';
PRINT '  - 4 Vouchers';
PRINT '  - 4 Customer loyalty accounts';
PRINT '  - 3 Points transactions (earned from sales)';
PRINT '  - 3 Rewards in catalog';
PRINT '  - 1 Reward redemption';
PRINT '  - 2 Tier upgrade records';
PRINT '';
PRINT 'Loyalty Program:';
PRINT '  - Bronze: 0% discount, 1.0x points';
PRINT '  - Silver: 3% discount, 1.2x points (1,000+ points)';
PRINT '  - Gold: 5% discount, 1.5x points (5,000+ points)';
PRINT '  - Platinum: 10% discount, 2.0x points (15,000+ points)';
PRINT '===============================================';
GO
