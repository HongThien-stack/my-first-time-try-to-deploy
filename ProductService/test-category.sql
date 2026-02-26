-- Kiểm tra CategoryId có tồn tại không
USE ProductDB;
GO

SELECT id, name, status 
FROM categories 
WHERE id = '3fa85f64-5717-4562-b3fc-2c963f66afa6';

-- Nếu không có, lấy 1 category bất kỳ để test
SELECT TOP 5 id, name, status 
FROM categories 
WHERE status = 'ACTIVE';
