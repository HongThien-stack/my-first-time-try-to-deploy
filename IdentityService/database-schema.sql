-- =====================================================
-- Identity and Access Management Service
-- =====================================================
-- Database: IdentityDB
-- Purpose: Authentication, Authorization, User Management
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

        CONSTRAINT FK_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
    );
    
    CREATE INDEX IX_users_email ON users(email);
    CREATE INDEX IX_users_phone ON users(phone);
    CREATE INDEX IX_users_role_id ON users(role_id);
    CREATE INDEX IX_users_status ON users(status);
    CREATE INDEX IX_users_refresh_token ON users(refresh_token);
    CREATE INDEX IX_users_otp_code ON users(otp_code);
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
        user_id UNIQUEIDENTIFIER NOT NULL,
        login_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ip_address NVARCHAR(50) NULL,
        user_agent NVARCHAR(500) NULL,
        status NVARCHAR(50) NOT NULL, -- SUCCESS | FAILED | BLOCKED
        failure_reason NVARCHAR(255) NULL,
        CONSTRAINT FK_login_logs_users FOREIGN KEY (user_id) REFERENCES users(id)
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
        action NVARCHAR(100) NOT NULL, -- CREATE | UPDATE | DELETE | ACTIVATE | DEACTIVATE | SUSPEND | RESET_PASSWORD | CHANGE_ROLE
        entity_type NVARCHAR(50) NOT NULL DEFAULT 'USER', -- USER | STAFF
        old_values NVARCHAR(MAX) NULL, -- JSON format of old values
        new_values NVARCHAR(MAX) NULL, -- JSON format of new values
        description NVARCHAR(500) NULL,
        ip_address NVARCHAR(50) NULL,
        performed_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_audit_logs_user FOREIGN KEY (user_id) REFERENCES users(id),
        CONSTRAINT FK_audit_logs_performed_by FOREIGN KEY (performed_by) REFERENCES users(id)
    );
    
    CREATE INDEX IX_audit_logs_user_id ON user_audit_logs(user_id);
    CREATE INDEX IX_audit_logs_performed_by ON user_audit_logs(performed_by);
    CREATE INDEX IX_audit_logs_action ON user_audit_logs(action);
    CREATE INDEX IX_audit_logs_entity_type ON user_audit_logs(entity_type);
    CREATE INDEX IX_audit_logs_performed_at ON user_audit_logs(performed_at);
END
GO

-- =====================================================
-- Table: staff
-- Purpose: Staff Management (User extension for Store/Warehouse staff)
-- Note: Moved from ShiftService to IdentityDB for centralized identity management
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'staff')
BEGIN
    CREATE TABLE staff (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        user_id UNIQUEIDENTIFIER NOT NULL, -- Reference to users.id
        workplace_id UNIQUEIDENTIFIER NOT NULL, -- Reference to OrderDB.stores.id OR InventoryDB.warehouses.id
        workplace_type NVARCHAR(50) NOT NULL, -- STORE | WAREHOUSE
        position NVARCHAR(100), -- Cashier, Store Manager, Warehouse Manager, etc.
        hired_date DATE NOT NULL,
        status NVARCHAR(50) NOT NULL DEFAULT 'ACTIVE', -- ACTIVE | INACTIVE | TERMINATED
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        created_by UNIQUEIDENTIFIER,
        updated_by UNIQUEIDENTIFIER,
        CONSTRAINT FK_staff_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
        CONSTRAINT FK_staff_created_by FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE NO ACTION,
        CONSTRAINT FK_staff_updated_by FOREIGN KEY (updated_by) REFERENCES users(id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_staff_user_id ON staff(user_id);
    CREATE INDEX IX_staff_workplace_id ON staff(workplace_id);
    CREATE INDEX IX_staff_workplace_type ON staff(workplace_type);
    CREATE INDEX IX_staff_status ON staff(status);
    CREATE INDEX IX_staff_hired_date ON staff(hired_date);
    CREATE UNIQUE INDEX UX_staff_user_workplace ON staff(user_id, workplace_id);
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
    (2, 'WareHouse Manager', 'WareHouse Manager - Manage WareHouse operations', 1, GETUTCDATE()),
    (3, 'Store Manager', 'Store Manager - Manage store operations', 1, GETUTCDATE()),
    (5, 'Store Staff', 'Store Staff - Process sales transactions', 1, GETUTCDATE()),
    (4, 'Warehouse Staff', 'Warehouse Staff - Manage inventory', 1, GETUTCDATE()),
    (6, 'Customer', 'Customer - Online shopping', 1, GETUTCDATE());
    
    SET IDENTITY_INSERT roles OFF;
END
GO

-- =====================================================
-- Insert sample users
-- Note: Password hash for 'Password123!' 
-- In production, use proper password hashing (bcrypt, Argon2, etc.)
-- Hash: AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==
-- =====================================================
IF NOT EXISTS (SELECT * FROM users WHERE email = 'admin@company.com')
BEGIN
    INSERT INTO users (id, email, password_hash, full_name, role_id, status) VALUES
    -- Admin Users (role_id = 1)
    (NEWID(), 'admin@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'System Administrator', 1, 'ACTIVE'),
    (NEWID(), 'admin2@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Nguyễn Văn Admin', 1, 'ACTIVE'),
    
    -- Manager Users (role_id = 2)
    (NEWID(), 'manager1@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Trần Thị Manager', 2, 'ACTIVE'),
    (NEWID(), 'manager2@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Lê Văn Quản Lý', 2, 'ACTIVE'),
    
    -- Store Staff Users (role_id = 3)
    (NEWID(), 'cashier1@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Phạm Thị Thu Ngân', 3, 'ACTIVE'),
    (NEWID(), 'cashier2@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Hoàng Văn Cashier', 3, 'ACTIVE'),
    (NEWID(), 'cashier3@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Vũ Thị Hoa', 3, 'ACTIVE'),
    
    -- Warehouse Staff Users (role_id = 4)
    (NEWID(), 'warehouse1@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Đỗ Văn Kho', 4, 'ACTIVE'),
    (NEWID(), 'warehouse2@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Bùi Thị Kho Bãi', 4, 'ACTIVE'),
    
    -- Customer Users (role_id = 5)
    (NEWID(), 'customer1@gmail.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Nguyễn Văn Khách', 5, 'ACTIVE'),
    (NEWID(), 'customer2@gmail.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Trần Thị Hương', 5, 'ACTIVE'),
    (NEWID(), 'customer3@gmail.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Lê Văn Minh', 5, 'ACTIVE'),
    (NEWID(), 'customer4@gmail.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Phạm Thị Lan', 5, 'ACTIVE'),
    
    -- Inactive User (for testing)
    (NEWID(), 'inactive@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Nguyễn Văn Inactive', 3, 'INACTIVE'),
    
    -- Suspended User (for testing)
    (NEWID(), 'suspended@company.com', 'AQAAAAIAAYagAAAAEKFJGZ5R8X8yN3bVN8pqHQxH8vN0hR9KjYzL3QmP7rT5wX2dN9vB8kM6sA4fC1eD0g==', N'Trần Văn Suspended', 5, 'SUSPENDED');
END
GO

-- =====================================================
-- Insert sample staff data
-- Note: workplace_id uses placeholder GUIDs
-- After creating OrderDB (stores) and InventoryDB (warehouses),
-- update these workplace_id values with actual IDs
-- =====================================================

-- Declare placeholder workplace IDs
DECLARE @Store1Id UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @Store2Id UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @Warehouse1Id UNIQUEIDENTIFIER = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA';
DECLARE @Warehouse2Id UNIQUEIDENTIFIER = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB';

-- Get user IDs for staff creation
DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM users WHERE email = 'admin@company.com');
DECLARE @Manager1Id UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'manager1@company.com');
DECLARE @Manager2Id UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'manager2@company.com');
DECLARE @Cashier1Id UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'cashier1@company.com');
DECLARE @Cashier2Id UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'cashier2@company.com');
DECLARE @Cashier3Id UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'cashier3@company.com');
DECLARE @Warehouse1UserId UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'warehouse1@company.com');
DECLARE @Warehouse2UserId UNIQUEIDENTIFIER = (SELECT id FROM users WHERE email = 'warehouse2@company.com');

IF NOT EXISTS (SELECT * FROM staff)
BEGIN
    INSERT INTO staff (id, user_id, workplace_id, workplace_type, position, hired_date, status, created_by, created_at) VALUES
    -- Store Staff (Cashiers)
    (NEWID(), @Cashier1Id, @Store1Id, 'STORE', N'Cashier', '2024-03-10', 'ACTIVE', @AdminId, GETUTCDATE()),
    (NEWID(), @Cashier2Id, @Store1Id, 'STORE', N'Cashier', '2024-03-15', 'ACTIVE', @AdminId, GETUTCDATE()),
    (NEWID(), @Cashier3Id, @Store2Id, 'STORE', N'Cashier', '2024-04-01', 'ACTIVE', @AdminId, GETUTCDATE()),
    
    -- Warehouse Staff
    (NEWID(), @Warehouse1UserId, @Warehouse1Id, 'WAREHOUSE', N'Warehouse Staff', '2024-01-20', 'ACTIVE', @AdminId, GETUTCDATE()),
    (NEWID(), @Warehouse2UserId, @Warehouse2Id, 'WAREHOUSE', N'Warehouse Staff', '2024-02-10', 'ACTIVE', @AdminId, GETUTCDATE());
    
    PRINT 'Sample staff data created successfully';
END
GO

PRINT '===============================================';
PRINT 'Identity and Access Management Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Default Users Created:';
PRINT '----------------------------------------';
PRINT 'Admin Accounts:';
PRINT '  - admin@company.com (Password: Password123!)';
PRINT '  - admin2@company.com (Password: Password123!)';
PRINT '';
PRINT 'Manager Accounts:';
PRINT '  - manager1@company.com (Password: Password123!)';
PRINT '  - manager2@company.com (Password: Password123!)';
PRINT '';
PRINT 'Cashier Accounts:';
PRINT '  - cashier1@company.com (Password: Password123!)';
PRINT '  - cashier2@company.com (Password: Password123!)';
PRINT '  - cashier3@company.com (Password: Password123!)';
PRINT '';
PRINT 'Warehouse Staff Accounts:';
PRINT '  - warehouse1@company.com (Password: Password123!)';
PRINT '  - warehouse2@company.com (Password: Password123!)';
PRINT '';
PRINT 'Customer Accounts:';
PRINT '  - customer1@gmail.com (Password: Password123!)';
PRINT '  - customer2@gmail.com (Password: Password123!)';
PRINT '  - customer3@gmail.com (Password: Password123!)';
PRINT '  - customer4@gmail.com (Password: Password123!)';
PRINT '';
PRINT 'Test Accounts:';
PRINT '  - inactive@company.com (Status: INACTIVE)';
PRINT '  - suspended@company.com (Status: SUSPENDED)';
PRINT '';
PRINT '===============================================';
PRINT 'Staff Records Created (5 records):';
PRINT '----------------------------------------';
PRINT 'Store Staff (Cashiers): 3 records';
PRINT '  - cashier1@company.com -> Store 1';
PRINT '  - cashier2@company.com -> Store 1';
PRINT '  - cashier3@company.com -> Store 2';
PRINT '';
PRINT 'Warehouse Staff: 2 records';
PRINT '  - warehouse1@company.com -> Warehouse 1 (placeholder ID)';
PRINT '  - warehouse2@company.com -> Warehouse 2 (placeholder ID)';
PRINT '';
PRINT '📝 NOTE: Store Managers are NOT in staff table';
PRINT '   Managers are identified by role_id = 2 in users table'
PRINT '';
PRINT '⚠️  NOTE: Staff records use placeholder workplace_id GUIDs';
PRINT '   After creating OrderDB and InventoryDB, update staff.workplace_id';
PRINT '   with actual store/warehouse IDs from those databases';
PRINT '';
PRINT '🔗 Cross-Database Relationships:';
PRINT '   IdentityDB.staff.workplace_id → OrderDB.stores.id (when workplace_type = STORE)';
PRINT '   IdentityDB.staff.workplace_id → InventoryDB.warehouses.id (when workplace_type = WAREHOUSE)';
PRINT '';
PRINT '📝 Example Queries:';
PRINT '   -- Get all staff with their workplace names';
PRINT '   SELECT s.*, u.full_name, u.email,';
PRINT '     CASE WHEN s.workplace_type = ''STORE'' ';
PRINT '       THEN (SELECT name FROM OrderDB.dbo.stores WHERE id = s.workplace_id)';
PRINT '       ELSE (SELECT name FROM InventoryDB.dbo.warehouses WHERE id = s.workplace_id)';
PRINT '     END AS workplace_name';
PRINT '   FROM staff s';
PRINT '   JOIN users u ON s.user_id = u.id';
PRINT '===============================================';
GO

-- =====================================================
-- Views: Cross-Database Staff Information
-- =====================================================

-- View: Staff with Workplace Information
IF OBJECT_ID('v_staff_with_workplace', 'V') IS NOT NULL
    DROP VIEW v_staff_with_workplace;
GO

CREATE VIEW v_staff_with_workplace AS
SELECT 
    s.id AS staff_id,
    s.user_id,
    u.email,
    u.full_name,
    u.phone,
    r.name AS role_name,
    s.workplace_id,
    s.workplace_type,
    CASE 
        WHEN s.workplace_type = 'STORE' THEN (SELECT name FROM OrderDB.dbo.stores WHERE id = s.workplace_id)
        WHEN s.workplace_type = 'WAREHOUSE' THEN (SELECT name FROM InventoryDB.dbo.warehouses WHERE id = s.workplace_id)
        ELSE NULL
    END AS workplace_name,
    CASE 
        WHEN s.workplace_type = 'STORE' THEN (SELECT location FROM OrderDB.dbo.stores WHERE id = s.workplace_id)
        WHEN s.workplace_type = 'WAREHOUSE' THEN (SELECT location FROM InventoryDB.dbo.warehouses WHERE id = s.workplace_id)
        ELSE NULL
    END AS workplace_location,
    s.position,
    s.hired_date,
    s.status AS staff_status,
    u.status AS user_status,
    s.created_at,
    s.updated_at
FROM staff s
INNER JOIN users u ON s.user_id = u.id
INNER JOIN roles r ON u.role_id = r.id;
GO

PRINT '';
PRINT '===============================================';
PRINT 'Views Created:';
PRINT '  ✅ v_staff_with_workplace - Staff with store/warehouse details';
PRINT '';
PRINT 'Usage Example:';
PRINT '  SELECT * FROM IdentityDB.dbo.v_staff_with_workplace';
PRINT '  WHERE workplace_type = ''STORE'';';
PRINT '===============================================';
GO
