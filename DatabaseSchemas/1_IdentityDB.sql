-- =====================================================
-- Identity and Access Management Service - UPDATED
-- =====================================================
-- Database: IdentityDB
-- Purpose: Authentication, Authorization, User Management
-- With Login & Audit Logging Support
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'IdentityDB')
BEGIN
    CREATE DATABASE IdentityDB;
END
GO

USE IdentityDB;
GO

-- =====================================================
-- Table: roles
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'roles')
BEGIN
    CREATE TABLE roles (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(100) NOT NULL,
        description NVARCHAR(MAX),
        is_system BIT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_roles_name ON roles(name);
END
GO

-- =====================================================
-- Table: users
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
BEGIN
    CREATE TABLE users (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        email NVARCHAR(255) NOT NULL UNIQUE,
        password_hash NVARCHAR(MAX) NOT NULL,
        full_name NVARCHAR(255),
        phone NVARCHAR(20) NULL,
        role_id INT NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE | SUSPENDED
        
        -- JWT Refresh Token (single device, simple approach)
        refresh_token NVARCHAR(500) NULL,
        refresh_token_expires_at DATETIME2 NULL,
        
        -- OTP for email verification / password reset
        otp_code NVARCHAR(10) NULL,
        otp_purpose NVARCHAR(50) NULL, -- EMAIL_VERIFICATION | PASSWORD_RESET | TWO_FACTOR_AUTH
        otp_expires_at DATETIME2 NULL,
        otp_attempts INT DEFAULT 0,
        
        -- Email verification
        email_verified BIT NOT NULL DEFAULT 0,
        
        -- Workplace Assignment (Option 2: Simple single location per user)
        workplace_type NVARCHAR(50) NULL, -- WAREHOUSE | STORE | NULL (for Admin/Customer)
        workplace_id UNIQUEIDENTIFIER NULL, -- InventoryDB.warehouses.id

        CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
    );
    
    CREATE INDEX IX_users_email ON users(email);
    CREATE INDEX IX_users_phone ON users(phone);
    CREATE INDEX IX_users_role_id ON users(role_id);
    CREATE INDEX IX_users_status ON users(status);
    CREATE INDEX IX_users_refresh_token ON users(refresh_token);
    CREATE INDEX IX_users_otp_code ON users(otp_code);
    CREATE INDEX IX_users_workplace ON users(workplace_type, workplace_id);
END
GO

-- =====================================================
-- Table: user_login_logs
-- Purpose: Track login history for all users
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'user_login_logs')
BEGIN
    CREATE TABLE user_login_logs (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        user_id UNIQUEIDENTIFIER NULL, -- NULL allowed for failed login attempts when user not found
        login_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ip_address NVARCHAR(50) NULL,
        user_agent NVARCHAR(500) NULL,
        status NVARCHAR(50) NOT NULL, -- SUCCESS | FAILED | BLOCKED
        failure_reason NVARCHAR(255) NULL,
        CONSTRAINT FK_login_logs_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_login_logs_user_id ON user_login_logs(user_id);
    CREATE INDEX IX_login_logs_login_at ON user_login_logs(login_at);
    CREATE INDEX IX_login_logs_status ON user_login_logs(status);
END
GO

-- =====================================================
-- Table: user_audit_logs
-- Purpose: Track user management actions by Admin/Manager
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'user_audit_logs')
BEGIN
    CREATE TABLE user_audit_logs (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        user_id UNIQUEIDENTIFIER NOT NULL, -- User being modified
        performed_by UNIQUEIDENTIFIER NOT NULL, -- Admin/Manager who made the change
        action NVARCHAR(100) NOT NULL, -- CREATE | UPDATE | DELETE | ACTIVATE | DEACTIVATE | SUSPEND | RESET_PASSWORD | CHANGE_ROLE | LOGIN | LOGOUT
        entity_type NVARCHAR(50) NOT NULL DEFAULT 'USER', -- USER | STAFF
        old_values NVARCHAR(MAX) NULL, -- JSON format of old values
        new_values NVARCHAR(MAX) NULL, -- JSON format of new values
        description NVARCHAR(500) NULL,
        ip_address NVARCHAR(50) NULL,
        performed_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_audit_logs_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE NO ACTION,
        CONSTRAINT FK_audit_logs_performed_by FOREIGN KEY (performed_by) REFERENCES users(id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_audit_logs_user_id ON user_audit_logs(user_id);
    CREATE INDEX IX_audit_logs_performed_by ON user_audit_logs(performed_by);
    CREATE INDEX IX_audit_logs_action ON user_audit_logs(action);
    CREATE INDEX IX_audit_logs_entity_type ON user_audit_logs(entity_type);
    CREATE INDEX IX_audit_logs_performed_at ON user_audit_logs(performed_at);
END
GO

-- =====================================================
-- Table: notifications
-- Purpose: System notifications and alerts
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'notifications')
BEGIN
    CREATE TABLE notifications (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        user_id UNIQUEIDENTIFIER NOT NULL, -- Recipient
        notification_type NVARCHAR(50) NOT NULL, -- LOW_STOCK | EXPIRING_SOON | APPROVAL_REQUIRED | TRANSFER_STATUS | PAYMENT_FAILED
        title NVARCHAR(255) NOT NULL,
        message NVARCHAR(MAX) NOT NULL,
        
        -- Reference to source entity
        reference_type NVARCHAR(50), -- RESTOCK_REQUEST | TRANSFER | DAMAGE_REPORT | SALE | PAYMENT
        reference_id UNIQUEIDENTIFIER, -- ID of the referenced entity
        
        -- Priority
        priority NVARCHAR(20) NOT NULL DEFAULT 'NORMAL', -- LOW | NORMAL | HIGH | URGENT
        
        -- Status
        is_read BIT NOT NULL DEFAULT 0,
        read_at DATETIME2,
        
        -- Expiry
        expires_at DATETIME2, -- Auto-expire old notifications
        
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_notifications_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_notifications_user_id ON notifications(user_id);
    CREATE INDEX IX_notifications_type ON notifications(notification_type);
    CREATE INDEX IX_notifications_is_read ON notifications(is_read);
    CREATE INDEX IX_notifications_priority ON notifications(priority);
    CREATE INDEX IX_notifications_created_at ON notifications(created_at);
    CREATE INDEX IX_notifications_reference ON notifications(reference_type, reference_id);
END
GO

-- =====================================================
-- Table: system_settings
-- Purpose: Application configuration and settings
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'system_settings')
BEGIN
    CREATE TABLE system_settings (
        id INT IDENTITY(1,1) PRIMARY KEY,
        setting_key NVARCHAR(100) NOT NULL UNIQUE,
        setting_value NVARCHAR(MAX) NOT NULL,
        data_type NVARCHAR(20) NOT NULL, -- STRING | INT | DECIMAL | BOOLEAN | JSON
        category NVARCHAR(50) NOT NULL, -- INVENTORY | LOYALTY | PAYMENT | NOTIFICATION | SYSTEM
        description NVARCHAR(500),
        is_public BIT NOT NULL DEFAULT 0, -- Can be accessed by frontend
        is_editable BIT NOT NULL DEFAULT 1, -- Can be changed via UI
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        updated_by UNIQUEIDENTIFIER -- IdentityDB.users.id
    );
    
    CREATE INDEX IX_settings_key ON system_settings(setting_key);
    CREATE INDEX IX_settings_category ON system_settings(category);
    CREATE INDEX IX_settings_is_public ON system_settings(is_public);
END
GO

-- =====================================================
-- Insert default roles
-- =====================================================
IF NOT EXISTS (SELECT * FROM roles WHERE is_system = 1)
BEGIN
    SET IDENTITY_INSERT roles ON;
    
    INSERT INTO roles (id, name, description, is_system, created_at) VALUES
    (1, 'Admin', 'System Administrator - Full Access', 1, GETUTCDATE()),
    (2, 'Manager', 'Store Manager - Manage store operations', 1, GETUTCDATE()),
    (3, 'Warehouse Manager', 'Warehouse Manager - Manage warehouse operations', 1, GETUTCDATE()),
    (4, 'Store Staff', 'Store Staff - Process sales transactions', 1, GETUTCDATE()),
    (5, 'Warehouse Staff', 'Warehouse Staff - Manage inventory', 1, GETUTCDATE()),
    (6, 'Customer', 'Customer - Online shopping', 1, GETUTCDATE());
    
    SET IDENTITY_INSERT roles OFF;
END
GO

-- =====================================================
-- Insert sample users
-- IMPORTANT: After creating users, you MUST run the API endpoint
-- POST /api/utility/update-passwords to hash passwords correctly
-- Or the password hash below may not work with your PasswordHasher
-- =====================================================
IF NOT EXISTS (SELECT * FROM users WHERE email = 'admin@company.com')
BEGIN
    -- NOTE: This password hash is for demonstration only
    -- You should call POST /api/utility/update-passwords after database creation
    DECLARE @TempPasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==';
    
    INSERT INTO users (id, email, password_hash, full_name, phone, role_id, status, email_verified, workplace_type, workplace_id) VALUES
    -- Admin Users (role_id = 1) - No workplace
    ('11111111-1111-1111-1111-111111111111', 'admin@company.com', @TempPasswordHash, N'System Administrator', '0901000001', 1, 'ACTIVE', 1, NULL, NULL),
    ('11111111-1111-1111-1111-111111111112', 'admin2@company.com', @TempPasswordHash, N'Nguyễn Văn Admin', '0901000002', 1, 'ACTIVE', 1, NULL, NULL),
    
    -- Manager Users (role_id = 2) - Assigned to warehouses/stores
    ('22222222-2222-2222-2222-222222222221', 'manager1@company.com', @TempPasswordHash, N'Trần Thị Manager', '0902000001', 2, 'ACTIVE', 1, 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001'),
    ('22222222-2222-2222-2222-222222222222', 'manager2@company.com', @TempPasswordHash, N'Lê Văn Quản Lý', '0902000002', 2, 'ACTIVE', 1, 'STORE', 'B0000001-0001-0001-0001-000000000001'),
    
    -- Store Staff Users (role_id = 3) - Assigned to stores
    ('33333333-3333-3333-3333-333333333331', 'cashier1@company.com', @TempPasswordHash, N'Phạm Thị Thu Ngân', '0903000001', 3, 'ACTIVE', 1, 'STORE', 'B0000001-0001-0001-0001-000000000001'),
    ('33333333-3333-3333-3333-333333333332', 'cashier2@company.com', @TempPasswordHash, N'Hoàng Văn Cashier', '0903000002', 3, 'ACTIVE', 1, 'STORE', 'B0000001-0001-0001-0001-000000000001'),
    ('33333333-3333-3333-3333-333333333333', 'cashier3@company.com', @TempPasswordHash, N'Vũ Thị Hoa', '0903000003', 3, 'ACTIVE', 1, 'STORE', 'B0000001-0001-0001-0001-000000000002'),
    
    -- Warehouse Staff Users (role_id = 4) - Assigned to warehouses
    ('44444444-4444-4444-4444-444444444441', 'warehouse1@company.com', @TempPasswordHash, N'Đỗ Văn Kho', '0904000001', 4, 'ACTIVE', 1, 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000001'),
    ('44444444-4444-4444-4444-444444444442', 'warehouse2@company.com', @TempPasswordHash, N'Bùi Thị Kho Bãi', '0904000002', 4, 'ACTIVE', 1, 'WAREHOUSE', 'A0000001-0001-0001-0001-000000000002'),
    
    -- Customer Users (role_id = 5) - No workplace
    ('55555555-5555-5555-5555-555555555551', 'customer1@gmail.com', @TempPasswordHash, N'Nguyễn Văn Khách', '0905000001', 5, 'ACTIVE', 1, NULL, NULL),
    ('55555555-5555-5555-5555-555555555552', 'customer2@gmail.com', @TempPasswordHash, N'Trần Thị Hương', '0905000002', 5, 'ACTIVE', 1, NULL, NULL),
    ('55555555-5555-5555-5555-555555555553', 'customer3@gmail.com', @TempPasswordHash, N'Lê Văn Minh', '0905000003', 5, 'ACTIVE', 1, NULL, NULL),
    ('55555555-5555-5555-5555-555555555554', 'customer4@gmail.com', @TempPasswordHash, N'Phạm Thị Lan', '0905000004', 5, 'ACTIVE', 1, NULL, NULL),
    
    -- Inactive User (for testing)
    ('66666666-6666-6666-6666-666666666661', 'inactive@company.com', @TempPasswordHash, N'Nguyễn Văn Inactive', '0906000001', 3, 'INACTIVE', 0, 'STORE', 'B0000001-0001-0001-0001-000000000001'),
    
    -- Suspended User (for testing)
    ('77777777-7777-7777-7777-777777777771', 'suspended@company.com', @TempPasswordHash, N'Trần Văn Suspended', '0907000001', 5, 'SUSPENDED', 1, NULL, NULL);
END
GO

-- =====================================================
-- Insert sample notifications
-- =====================================================
IF NOT EXISTS (SELECT * FROM notifications)
BEGIN
    -- Low stock alert for Manager 2
    INSERT INTO notifications (id, user_id, notification_type, title, message, reference_type, reference_id, priority) VALUES
    (NEWID(), '22222222-2222-2222-2222-222222222222', 'LOW_STOCK', 
     N'Sắp hết hàng: Sữa Vinamilk', 
     N'Sữa Tươi Vinamilk 100% còn 15 units, dưới mức tối thiểu (30 units). Vui lòng tạo restock request.',
     'INVENTORY', 
     NULL,
     'HIGH');
    
    -- Approval required for Manager 1
    INSERT INTO notifications (id, user_id, notification_type, title, message, reference_type, reference_id, priority) VALUES
    (NEWID(), '22222222-2222-2222-2222-222222222221', 'APPROVAL_REQUIRED',
     N'Yêu cầu nhập hàng mới: RST-2024-002',
     N'Cửa Hàng Thủ Đức yêu cầu nhập 150 units Sữa Vinamilk. Độ ưu tiên: HIGH',
     'RESTOCK_REQUEST',
     NULL,
     'URGENT');
    
    -- Expiring soon alert for Warehouse Staff 1
    INSERT INTO notifications (id, user_id, notification_type, title, message, reference_type, reference_id, priority) VALUES
    (NEWID(), '44444444-4444-4444-4444-444444444441', 'EXPIRING_SOON',
     N'Hàng sắp hết hạn: Batch VNM-2024-001',
     N'500 units Sữa Vinamilk (Batch VNM-2024-001) sẽ hết hạn trong 30 ngày.',
     'PRODUCT_BATCH',
     NULL,
     'NORMAL');
END
GO

-- =====================================================
-- Insert default system settings
-- =====================================================
IF NOT EXISTS (SELECT * FROM system_settings)
BEGIN
    INSERT INTO system_settings (setting_key, setting_value, data_type, category, description, is_public, is_editable) VALUES
    -- Inventory Settings
    ('LOW_STOCK_THRESHOLD_PERCENTAGE', '20', 'INT', 'INVENTORY', N'Cảnh báo khi tồn kho < X% của max_stock_level', 0, 1),
    ('EXPIRY_ALERT_DAYS', '30', 'INT', 'INVENTORY', N'Cảnh báo hàng sắp hết hạn trong X ngày', 0, 1),
    ('AUTO_APPROVE_RESTOCK_UNDER', '1000000', 'DECIMAL', 'INVENTORY', N'Tự động duyệt restock request dưới X VND', 0, 1),
    
    -- Loyalty Settings
    ('POINTS_PER_1000_VND', '1', 'INT', 'LOYALTY', N'Số điểm earned cho mỗi 1,000 VND', 1, 1),
    ('POINTS_EXPIRY_MONTHS', '12', 'INT', 'LOYALTY', N'Điểm hết hạn sau X tháng', 1, 1),
    ('MIN_POINTS_FOR_REDEMPTION', '100', 'INT', 'LOYALTY', N'Số điểm tối thiểu để đổi quà', 1, 1),
    
    -- Payment Settings
    ('VNPAY_MERCHANT_ID', 'DEMO', 'STRING', 'PAYMENT', N'VNPay Merchant/Terminal ID', 0, 1),
    ('VNPAY_SECRET_KEY', 'SECRETKEY123', 'STRING', 'PAYMENT', N'VNPay Secret Key', 0, 1),
    ('MOMO_PARTNER_CODE', 'MOMO', 'STRING', 'PAYMENT', N'Momo Partner Code', 0, 1),
    ('MOMO_ACCESS_KEY', 'ACCESSKEY123', 'STRING', 'PAYMENT', N'Momo Access Key', 0, 1),
    ('PAYMENT_TIMEOUT_MINUTES', '15', 'INT', 'PAYMENT', N'Payment link hết hạn sau X phút', 0, 1),
    
    -- Notification Settings
    ('NOTIFICATION_RETENTION_DAYS', '30', 'INT', 'NOTIFICATION', N'Xóa notification cũ sau X ngày', 0, 1),
    ('ENABLE_EMAIL_NOTIFICATIONS', 'false', 'BOOLEAN', 'NOTIFICATION', N'Bật thông báo qua email', 0, 1),
    ('ENABLE_SMS_NOTIFICATIONS', 'false', 'BOOLEAN', 'NOTIFICATION', N'Bật thông báo qua SMS', 0, 1),
    
    -- System Settings
    ('SYSTEM_NAME', N'Hệ Thống Quản Lý Kho', 'STRING', 'SYSTEM', N'Tên hệ thống', 1, 1),
    ('SYSTEM_TIMEZONE', 'Asia/Ho_Chi_Minh', 'STRING', 'SYSTEM', N'Múi giờ hệ thống', 0, 1),
    ('DEFAULT_CURRENCY', 'VND', 'STRING', 'SYSTEM', N'Đơn vị tiền tệ mặc định', 1, 0);
END
GO

PRINT '===============================================';
PRINT 'Identity and Access Management Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT '⚠️  IMPORTANT: After creating database, run this API endpoint:';
PRINT '   POST http://localhost:5000/api/utility/update-passwords';
PRINT '   This will update all password hashes to the correct format';
PRINT '';
PRINT 'Default Users Created (16 users with fixed UUIDs):';
PRINT '----------------------------------------';
PRINT 'Admin Accounts:';
PRINT '  - admin@company.com (ID: 11111111-1111-1111-1111-111111111111)';
PRINT '  - admin2@company.com (ID: 11111111-1111-1111-1111-111111111112)';
PRINT '';
PRINT 'Manager Accounts:';
PRINT '  - manager1@company.com (ID: 22222222-2222-2222-2222-222222222221)';
PRINT '    → Kho Tổng HCM (WAREHOUSE: A0000001-0001-0001-0001-000000000001)';
PRINT '  - manager2@company.com (ID: 22222222-2222-2222-2222-222222222222)';
PRINT '    → Cửa Hàng Thủ Đức (STORE: B0000001-0001-0001-0001-000000000001)';
PRINT '';
PRINT 'Store Staff Accounts:';
PRINT '  - cashier1@company.com (ID: 33333333-3333-3333-3333-333333333331)';
PRINT '    → Cửa Hàng Thủ Đức (STORE: B0000001-0001-0001-0001-000000000001)';
PRINT '  - cashier2@company.com (ID: 33333333-3333-3333-3333-333333333332)';
PRINT '    → Cửa Hàng Thủ Đức (STORE: B0000001-0001-0001-0001-000000000001)';
PRINT '  - cashier3@company.com (ID: 33333333-3333-3333-3333-333333333333)';
PRINT '    → Cửa Hàng Quận 1 (STORE: B0000001-0001-0001-0001-000000000002)';
PRINT '';
PRINT 'Warehouse Staff Accounts:';
PRINT '  - warehouse1@company.com (ID: 44444444-4444-4444-4444-444444444441)';
PRINT '    → Kho Tổng HCM (WAREHOUSE: A0000001-0001-0001-0001-000000000001)';
PRINT '  - warehouse2@company.com (ID: 44444444-4444-4444-4444-444444444442)';
PRINT '    → Kho Miền Bắc (WAREHOUSE: A0000001-0001-0001-0001-000000000002)';
PRINT '';
PRINT 'Customer Accounts:';
PRINT '  - customer1@gmail.com (ID: 55555555-5555-5555-5555-555555555551)';
PRINT '  - customer2@gmail.com (ID: 55555555-5555-5555-5555-555555555552)';
PRINT '  - customer3@gmail.com (ID: 55555555-5555-5555-5555-555555555553)';
PRINT '  - customer4@gmail.com (ID: 55555555-5555-5555-5555-555555555554)';
PRINT '';
PRINT 'All default password: Password123!';
PRINT '';
PRINT 'New Features Added:';
PRINT '  ✅ Workplace Assignment (Simple single location per user)';
PRINT '  ✅ Notifications System (3 sample alerts)';
PRINT '  ✅ System Settings (16 configuration keys)';
PRINT '===============================================';
GO
