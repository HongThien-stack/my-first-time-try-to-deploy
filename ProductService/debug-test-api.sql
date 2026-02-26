-- ================================================
-- Script kiểm tra và fix lỗi test API
-- ================================================

USE ProductDB;
GO

-- 1. Kiểm tra CategoryId trong request có tồn tại không
PRINT '=== Kiểm tra CategoryId từ request ===';
SELECT 
    COUNT(*) as [Exists],
    CASE WHEN COUNT(*) > 0 THEN 'FOUND' ELSE 'NOT FOUND' END as [Status]
FROM categories 
WHERE id = '3fa85f64-5717-4562-b3fc-2c963f66afa6';

-- 2. Lấy danh sách các CategoryId thực tế để test
PRINT '';
PRINT '=== Danh sách CategoryId có thể dùng để test ===';
SELECT TOP 5
    id as [CategoryId],
    name as [CategoryName],
    status
FROM categories 
WHERE status = 'ACTIVE'
ORDER BY created_at;

-- 3. Lấy 1 CategoryId cụ thể để test ngay
PRINT '';
PRINT '=== CategoryId đầu tiên (dùng để test) ===';
DECLARE @TestCategoryId UNIQUEIDENTIFIER;
SELECT TOP 1 @TestCategoryId = id FROM categories WHERE status = 'ACTIVE';

SELECT 
    @TestCategoryId as [Use_This_CategoryId],
    name as [CategoryName]
FROM categories 
WHERE id = @TestCategoryId;

PRINT '';
PRINT '=== Copy CategoryId phía trên để dùng trong request ===';

-- 4. Kiểm tra product_audit_logs table có tồn tại chưa
PRINT '';
PRINT '=== Kiểm tra bảng product_audit_logs ===';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'product_audit_logs')
BEGIN
    PRINT '✅ Bảng product_audit_logs đã tồn tại';
    SELECT COUNT(*) as [Total_Audit_Logs] FROM product_audit_logs;
END
ELSE
BEGIN
    PRINT '❌ Bảng product_audit_logs CHƯA tồn tại - CẦN CHẠY MIGRATION!';
    PRINT 'Chạy file: database-migration-audit-logs.sql';
END
