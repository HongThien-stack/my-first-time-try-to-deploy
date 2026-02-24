-- =====================================================
-- Identity and Access Management Service - UPDATED
-- =====================================================
-- Database: IdentityDB
-- Purpose: Authentication, Authorization, User Management
-- With Login & Audit Logging Support
-- =====================================================

USE master;
GO

-- Drop existing database if you want a fresh start (CAREFUL!)
-- DROP DATABASE IF EXISTS IdentityDB;
-- GO

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
-- Insert default roles
-- =====================================================
IF NOT EXISTS (SELECT * FROM roles WHERE is_system = 1)
BEGIN
    SET IDENTITY_INSERT roles ON;
    
    INSERT INTO roles (id, name, description, is_system, created_at) VALUES
    (1, 'Admin', 'System Administrator - Full Access', 1, GETUTCDATE()),
    (2, 'Manager', 'Store Manager - Manage store operations', 1, GETUTCDATE()),
    (3, 'Store Staff', 'Store Staff - Process sales transactions', 1, GETUTCDATE()),
    (4, 'Warehouse Staff', 'Warehouse Staff - Manage inventory', 1, GETUTCDATE()),
    (5, 'Customer', 'Customer - Online shopping', 1, GETUTCDATE());
    
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
    
    INSERT INTO users (id, email, password_hash, full_name, role_id, status) VALUES
    -- Admin Users (role_id = 1)
    (NEWID(), 'admin@company.com', @TempPasswordHash, N'System Administrator', 1, 'ACTIVE'),
    (NEWID(), 'admin2@company.com', @TempPasswordHash, N'Nguyễn Văn Admin', 1, 'ACTIVE'),
    
    -- Manager Users (role_id = 2)
    (NEWID(), 'manager1@company.com', @TempPasswordHash, N'Trần Thị Manager', 2, 'ACTIVE'),
    (NEWID(), 'manager2@company.com', @TempPasswordHash, N'Lê Văn Quản Lý', 2, 'ACTIVE'),
    
    -- Store Staff Users (role_id = 3)
    (NEWID(), 'cashier1@company.com', @TempPasswordHash, N'Phạm Thị Thu Ngân', 3, 'ACTIVE'),
    (NEWID(), 'cashier2@company.com', @TempPasswordHash, N'Hoàng Văn Cashier', 3, 'ACTIVE'),
    (NEWID(), 'cashier3@company.com', @TempPasswordHash, N'Vũ Thị Hoa', 3, 'ACTIVE'),
    
    -- Warehouse Staff Users (role_id = 4)
    (NEWID(), 'warehouse1@company.com', @TempPasswordHash, N'Đỗ Văn Kho', 4, 'ACTIVE'),
    (NEWID(), 'warehouse2@company.com', @TempPasswordHash, N'Bùi Thị Kho Bãi', 4, 'ACTIVE'),
    
    -- Customer Users (role_id = 5)
    (NEWID(), 'customer1@gmail.com', @TempPasswordHash, N'Nguyễn Văn Khách', 5, 'ACTIVE'),
    (NEWID(), 'customer2@gmail.com', @TempPasswordHash, N'Trần Thị Hương', 5, 'ACTIVE'),
    (NEWID(), 'customer3@gmail.com', @TempPasswordHash, N'Lê Văn Minh', 5, 'ACTIVE'),
    (NEWID(), 'customer4@gmail.com', @TempPasswordHash, N'Phạm Thị Lan', 5, 'ACTIVE'),
    
    -- Inactive User (for testing)
    (NEWID(), 'inactive@company.com', @TempPasswordHash, N'Nguyễn Văn Inactive', 3, 'INACTIVE'),
    
    -- Suspended User (for testing)
    (NEWID(), 'suspended@company.com', @TempPasswordHash, N'Trần Văn Suspended', 5, 'SUSPENDED');
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
PRINT 'Store Staff Accounts:';
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
PRINT '===============================================';
PRINT '';
PRINT 'New Features:';
PRINT '  ✅ Login history tracking (user_login_logs table)';
PRINT '  ✅ Audit logging for user management (user_audit_logs table)';
PRINT '  ✅ Automatic login attempt logging (SUCCESS/FAILED/BLOCKED)';
PRINT '===============================================';
GO
