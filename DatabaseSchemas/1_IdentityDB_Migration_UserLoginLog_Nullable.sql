-- =====================================================
-- Migration: Allow NULL user_id in user_login_logs
-- Purpose: Fix FK violation when logging failed login attempts
-- =====================================================

USE IdentityDB;
GO

-- Check if column is already nullable
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'user_login_logs' 
    AND COLUMN_NAME = 'user_id' 
    AND IS_NULLABLE = 'NO'
)
BEGIN
    PRINT 'Updating user_login_logs.user_id to allow NULL...';
    
    -- Drop FK constraint temporarily
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_login_logs_users')
    BEGIN
        ALTER TABLE user_login_logs DROP CONSTRAINT FK_login_logs_users;
        PRINT '  ✓ Dropped FK constraint FK_login_logs_users';
    END
    
    -- Alter column to allow NULL
    ALTER TABLE user_login_logs ALTER COLUMN user_id UNIQUEIDENTIFIER NULL;
    PRINT '  ✓ Changed user_id to UNIQUEIDENTIFIER NULL';
    
    -- Recreate FK constraint
    ALTER TABLE user_login_logs 
    ADD CONSTRAINT FK_login_logs_users 
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;
    PRINT '  ✓ Recreated FK constraint FK_login_logs_users';
    
    PRINT '';
    PRINT '✅ Migration completed successfully!';
    PRINT '   user_login_logs.user_id now allows NULL for failed login attempts';
END
ELSE
BEGIN
    PRINT '⚠️  Column user_login_logs.user_id is already nullable. No changes needed.';
END
GO
