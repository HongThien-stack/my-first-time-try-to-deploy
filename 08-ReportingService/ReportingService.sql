-- =====================================================
-- Reporting Service
-- =====================================================
-- Database: ReportingDB
-- Purpose: Report Generation, Analytics, Business Intelligence
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ReportingDB')
BEGIN
    CREATE DATABASE ReportingDB;
END
GO

USE ReportingDB;
GO

-- =====================================================
-- Table: reports
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'reports')
BEGIN
    CREATE TABLE reports (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        report_type NVARCHAR(50) NOT NULL, -- SALES | PRODUCT | INVENTORY
        generated_by UNIQUEIDENTIFIER NOT NULL, -- Reference to IdentityDB.users.id
        description NVARCHAR(MAX),
        start_date DATE,
        end_date DATE,
        file_path NVARCHAR(500),
        status NVARCHAR(50) NOT NULL DEFAULT 'COMPLETED', -- PENDING | COMPLETED | FAILED
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_reports_report_type ON reports(report_type);
    CREATE INDEX IX_reports_generated_by ON reports(generated_by);
    CREATE INDEX IX_reports_created_at ON reports(created_at);
    CREATE INDEX IX_reports_start_date ON reports(start_date);
    CREATE INDEX IX_reports_end_date ON reports(end_date);
END
GO

PRINT 'Reporting Database Created Successfully';
GO
