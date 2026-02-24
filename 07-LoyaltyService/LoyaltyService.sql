-- =====================================================
-- Loyalty Management Service
-- =====================================================
-- Database: LoyaltyDB
-- Purpose: Customer Loyalty, Points Management
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'LoyaltyDB')
BEGIN
    CREATE DATABASE LoyaltyDB;
END
GO

USE LoyaltyDB;
GO

-- =====================================================
-- Table: loyalty
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'loyalty')
BEGIN
    CREATE TABLE loyalty (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        customer_id UNIQUEIDENTIFIER NOT NULL, -- Reference to OrderDB.customers.id
        total_points INT NOT NULL DEFAULT 0,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_loyalty_customer_id ON loyalty(customer_id);
    CREATE INDEX IX_loyalty_total_points ON loyalty(total_points);
END
GO

-- =====================================================
-- Table: loyalty_transactions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'loyalty_transactions')
BEGIN
    CREATE TABLE loyalty_transactions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        loyalty_id UNIQUEIDENTIFIER NOT NULL,
        order_id UNIQUEIDENTIFIER NOT NULL, -- Reference to OrderDB.orders.id
        points INT NOT NULL,
        type NVARCHAR(50) NOT NULL, -- EARN | REDEEM
        description NVARCHAR(500),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_loyalty_transactions_loyalty FOREIGN KEY (loyalty_id) REFERENCES loyalty(id)
    );
    
    CREATE INDEX IX_loyalty_transactions_loyalty_id ON loyalty_transactions(loyalty_id);
    CREATE INDEX IX_loyalty_transactions_order_id ON loyalty_transactions(order_id);
    CREATE INDEX IX_loyalty_transactions_type ON loyalty_transactions(type);
    CREATE INDEX IX_loyalty_transactions_created_at ON loyalty_transactions(created_at);
END
GO

PRINT 'Loyalty Management Database Created Successfully';
GO
