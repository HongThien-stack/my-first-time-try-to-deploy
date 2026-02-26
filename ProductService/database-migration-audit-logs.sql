-- =====================================================
-- Product Service - Audit Log Table Migration
-- =====================================================

USE ProductDB;
GO

-- =====================================================
-- Table: product_audit_logs
-- Purpose: Track all product creation and modification actions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'product_audit_logs')
BEGIN
    CREATE TABLE product_audit_logs (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        product_id UNIQUEIDENTIFIER NOT NULL,
        performed_by UNIQUEIDENTIFIER NOT NULL, -- User ID from Identity Service
        performed_by_name NVARCHAR(255) NOT NULL,
        action NVARCHAR(100) NOT NULL, -- CREATE | UPDATE | DELETE | ACTIVATE | DEACTIVATE
        old_values NVARCHAR(MAX) NULL, -- JSON format of old values
        new_values NVARCHAR(MAX) NULL, -- JSON format of new values
        description NVARCHAR(500) NULL,
        ip_address NVARCHAR(50) NULL,
        performed_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_product_audit_logs_products FOREIGN KEY (product_id) 
            REFERENCES products(id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_product_audit_logs_product_id ON product_audit_logs(product_id);
    CREATE INDEX IX_product_audit_logs_performed_by ON product_audit_logs(performed_by);
    CREATE INDEX IX_product_audit_logs_action ON product_audit_logs(action);
    CREATE INDEX IX_product_audit_logs_performed_at ON product_audit_logs(performed_at);
    
    PRINT '✅ Table product_audit_logs created successfully';
END
ELSE
BEGIN
    PRINT '⚠️  Table product_audit_logs already exists';
END
GO

PRINT '===============================================';
PRINT 'Product Audit Log Migration Completed';
PRINT '===============================================';
GO
