-- =====================================================
-- Shift Management Service
-- =====================================================
-- Database: ShiftDB
-- Purpose: Staff Management, Shift Scheduling, Work Tracking
-- =====================================================

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ShiftDB')
BEGIN
    CREATE DATABASE ShiftDB;
END
GO

USE ShiftDB;
GO

-- =====================================================
-- Table: staff
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'staff')
BEGIN
    CREATE TABLE staff (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        user_id UNIQUEIDENTIFIER NOT NULL, -- Reference to IdentityDB.users.id
        store_id UNIQUEIDENTIFIER NOT NULL, -- Reference to OrderDB.stores.id
        staff_type NVARCHAR(50) NOT NULL, -- STORE | WAREHOUSE
        hired_date DATE NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_staff_user_id ON staff(user_id);
    CREATE INDEX IX_staff_store_id ON staff(store_id);
    CREATE INDEX IX_staff_type ON staff(staff_type);
END
GO

-- =====================================================
-- Table: shifts
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'shifts')
BEGIN
    CREATE TABLE shifts (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        name NVARCHAR(100) NOT NULL,
        start_time TIME NOT NULL,
        end_time TIME NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_shifts_name ON shifts(name);
END
GO

-- =====================================================
-- Table: staff_shifts
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'staff_shifts')
BEGIN
    CREATE TABLE staff_shifts (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        staff_id UNIQUEIDENTIFIER NOT NULL,
        shift_id UNIQUEIDENTIFIER NOT NULL,
        work_date DATE NOT NULL,
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_staff_shifts_staff FOREIGN KEY (staff_id) REFERENCES staff(id),
        CONSTRAINT FK_staff_shifts_shifts FOREIGN KEY (shift_id) REFERENCES shifts(id)
    );
    
    CREATE INDEX IX_staff_shifts_staff_id ON staff_shifts(staff_id);
    CREATE INDEX IX_staff_shifts_shift_id ON staff_shifts(shift_id);
    CREATE INDEX IX_staff_shifts_work_date ON staff_shifts(work_date);
    CREATE UNIQUE INDEX UX_staff_shifts_unique ON staff_shifts(staff_id, shift_id, work_date);
END
GO

-- =====================================================
-- Insert default shifts
-- =====================================================
IF NOT EXISTS (SELECT * FROM shifts)
BEGIN
    INSERT INTO shifts (id, name, start_time, end_time, created_at) VALUES
    (NEWID(), 'Morning Shift', '06:00:00', '14:00:00', GETUTCDATE()),
    (NEWID(), 'Afternoon Shift', '14:00:00', '22:00:00', GETUTCDATE()),
    (NEWID(), 'Night Shift', '22:00:00', '06:00:00', GETUTCDATE());
END
GO

PRINT 'Shift Management Database Created Successfully';
GO
