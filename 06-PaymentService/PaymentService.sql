-- =====================================================
-- Payment Management Service
-- =====================================================
-- Database: PaymentDB
-- Purpose: Payment Processing, Transaction Management
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PaymentDB')
BEGIN
    CREATE DATABASE PaymentDB;
END
GO

USE PaymentDB;
GO

-- =====================================================
-- Table: payments
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payments')
BEGIN
    CREATE TABLE payments (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        order_id UNIQUEIDENTIFIER NOT NULL, -- Reference to OrderDB.orders.id
        method NVARCHAR(50) NOT NULL, -- CASH | CREDIT_CARD | DEBIT_CARD | E_WALLET | BANK_TRANSFER
        amount DECIMAL(18, 2) NOT NULL,
        status NVARCHAR(50) NOT NULL, -- PENDING | COMPLETED | FAILED | REFUNDED
        transaction_id NVARCHAR(255),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_payments_order_id ON payments(order_id);
    CREATE INDEX IX_payments_method ON payments(method);
    CREATE INDEX IX_payments_status ON payments(status);
    CREATE INDEX IX_payments_created_at ON payments(created_at);
    CREATE INDEX IX_payments_transaction_id ON payments(transaction_id);
END
GO

PRINT 'Payment Management Database Created Successfully';
GO
