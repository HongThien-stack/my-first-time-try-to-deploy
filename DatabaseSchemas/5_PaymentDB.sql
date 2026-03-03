-- =====================================================
-- Payment Gateway Integration Service
-- =====================================================
-- Database: PaymentDB
-- Purpose: VNPay & Momo Integration, Payment Processing
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
-- Table: payment_methods
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payment_methods')
BEGIN
    CREATE TABLE payment_methods (
        id INT IDENTITY(1,1) PRIMARY KEY,
        code NVARCHAR(50) NOT NULL UNIQUE, -- CASH, CARD, VNPAY, MOMO
        name NVARCHAR(255) NOT NULL,
        description NVARCHAR(500),
        is_online BIT NOT NULL DEFAULT 0, -- Requires payment gateway
        is_active BIT NOT NULL DEFAULT 1,
        sort_order INT DEFAULT 0,
        icon_url NVARCHAR(500), -- Logo for UI
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2
    );
    
    CREATE INDEX IX_payment_methods_code ON payment_methods(code);
    CREATE INDEX IX_payment_methods_active ON payment_methods(is_active);
END
GO

-- =====================================================
-- Table: payment_transactions
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payment_transactions')
BEGIN
    CREATE TABLE payment_transactions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        transaction_number NVARCHAR(50) NOT NULL UNIQUE, -- PAY-2024-001
        
        -- Sale reference
        sale_id UNIQUEIDENTIFIER NOT NULL, -- POSDB.sales.id
        sale_number NVARCHAR(50) NOT NULL, -- POSDB.sales.sale_number
        
        -- Payment method
        payment_method NVARCHAR(50) NOT NULL, -- VNPAY | MOMO
        
        -- Amount
        amount DECIMAL(18,2) NOT NULL,
        currency NVARCHAR(10) NOT NULL DEFAULT 'VND',
        
        -- Status
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | PROCESSING | COMPLETED | FAILED | CANCELLED | REFUNDED
        
        -- Gateway details
        gateway_transaction_id NVARCHAR(255), -- Transaction ID from VNPay/Momo
        gateway_order_id NVARCHAR(255), -- Order ID sent to gateway
        gateway_response NVARCHAR(MAX), -- JSON response from gateway
        
        -- QR Code
        qr_code_url NVARCHAR(500), -- QR code for payment
        payment_url NVARCHAR(500), -- Deep link for mobile app
        
        -- Expiry
        expires_at DATETIME2, -- When payment link expires (usually 15 minutes)
        
        -- Completion
        paid_at DATETIME2, -- When payment completed
        
        -- Callbacks
        return_url NVARCHAR(500), -- URL to redirect after payment
        notify_url NVARCHAR(500), -- IPN callback URL
        
        -- User
        customer_id UNIQUEIDENTIFIER, -- IdentityDB.users.id
        
        -- Timestamps
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        updated_at DATETIME2,
        
        -- Notes
        notes NVARCHAR(MAX)
    );
    
    CREATE INDEX IX_transactions_number ON payment_transactions(transaction_number);
    CREATE INDEX IX_transactions_sale_id ON payment_transactions(sale_id);
    CREATE INDEX IX_transactions_method ON payment_transactions(payment_method);
    CREATE INDEX IX_transactions_status ON payment_transactions(status);
    CREATE INDEX IX_transactions_gateway_txn_id ON payment_transactions(gateway_transaction_id);
    CREATE INDEX IX_transactions_created_at ON payment_transactions(created_at);
END
GO

-- =====================================================
-- Table: payment_callbacks
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payment_callbacks')
BEGIN
    CREATE TABLE payment_callbacks (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        transaction_id UNIQUEIDENTIFIER NOT NULL,
        callback_type NVARCHAR(50) NOT NULL, -- IPN | RETURN
        payment_method NVARCHAR(50) NOT NULL, -- VNPAY | MOMO
        
        -- Raw data
        request_headers NVARCHAR(MAX), -- JSON
        request_body NVARCHAR(MAX), -- JSON or query string
        request_ip NVARCHAR(50),
        
        -- Response
        response_status INT, -- HTTP status code
        response_body NVARCHAR(MAX),
        
        -- Processing
        is_valid BIT NOT NULL DEFAULT 0, -- Signature verification passed
        is_processed BIT NOT NULL DEFAULT 0,
        error_message NVARCHAR(500),
        
        received_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        processed_at DATETIME2,
        
        CONSTRAINT FK_callbacks_transactions FOREIGN KEY (transaction_id) REFERENCES payment_transactions(id)
    );
    
    CREATE INDEX IX_callbacks_transaction_id ON payment_callbacks(transaction_id);
    CREATE INDEX IX_callbacks_type ON payment_callbacks(callback_type);
    CREATE INDEX IX_callbacks_method ON payment_callbacks(payment_method);
    CREATE INDEX IX_callbacks_is_valid ON payment_callbacks(is_valid);
    CREATE INDEX IX_callbacks_is_processed ON payment_callbacks(is_processed);
    CREATE INDEX IX_callbacks_received_at ON payment_callbacks(received_at);
END
GO

-- =====================================================
-- Table: payment_refunds
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payment_refunds')
BEGIN
    CREATE TABLE payment_refunds (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        refund_number NVARCHAR(50) NOT NULL UNIQUE, -- REF-2024-001
        transaction_id UNIQUEIDENTIFIER NOT NULL,
        original_amount DECIMAL(18,2) NOT NULL,
        refund_amount DECIMAL(18,2) NOT NULL,
        reason NVARCHAR(500),
        
        -- Gateway refund details
        gateway_refund_id NVARCHAR(255), -- Refund ID from VNPay/Momo
        gateway_response NVARCHAR(MAX), -- JSON response
        
        -- Status
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | PROCESSING | COMPLETED | FAILED
        
        -- User
        requested_by UNIQUEIDENTIFIER NOT NULL, -- IdentityDB.users.id (Manager/Admin)
        approved_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        
        -- Timestamps
        requested_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        approved_at DATETIME2,
        completed_at DATETIME2,
        
        notes NVARCHAR(MAX),
        
        CONSTRAINT FK_refunds_transactions FOREIGN KEY (transaction_id) REFERENCES payment_transactions(id)
    );
    
    CREATE INDEX IX_refunds_refund_number ON payment_refunds(refund_number);
    CREATE INDEX IX_refunds_transaction_id ON payment_refunds(transaction_id);
    CREATE INDEX IX_refunds_status ON payment_refunds(status);
    CREATE INDEX IX_refunds_requested_at ON payment_refunds(requested_at);
END
GO

-- =====================================================
-- Table: payment_reconciliation
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payment_reconciliation')
BEGIN
    CREATE TABLE payment_reconciliation (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        reconciliation_number NVARCHAR(50) NOT NULL UNIQUE, -- REC-2024-001
        payment_method NVARCHAR(50) NOT NULL, -- VNPAY | MOMO
        reconciliation_date DATE NOT NULL, -- Date being reconciled
        
        -- Summary
        total_transactions INT NOT NULL,
        total_amount DECIMAL(18,2) NOT NULL,
        gateway_total_amount DECIMAL(18,2), -- From gateway report
        
        -- Status
        status NVARCHAR(50) NOT NULL DEFAULT 'PENDING', -- PENDING | MATCHED | DISCREPANCY | RESOLVED
        discrepancy_amount AS (total_amount - ISNULL(gateway_total_amount, 0)) PERSISTED,
        discrepancy_count INT DEFAULT 0,
        
        -- Files
        system_report_file NVARCHAR(500), -- Path to system report
        gateway_report_file NVARCHAR(500), -- Path to gateway report
        
        -- User
        reconciled_by UNIQUEIDENTIFIER, -- IdentityDB.users.id
        reconciled_at DATETIME2,
        
        notes NVARCHAR(MAX),
        created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_reconciliation_number ON payment_reconciliation(reconciliation_number);
    CREATE INDEX IX_reconciliation_method ON payment_reconciliation(payment_method);
    CREATE INDEX IX_reconciliation_date ON payment_reconciliation(reconciliation_date);
    CREATE INDEX IX_reconciliation_status ON payment_reconciliation(status);
END
GO

-- =====================================================
-- Insert default payment methods
-- =====================================================
IF NOT EXISTS (SELECT * FROM payment_methods)
BEGIN
    INSERT INTO payment_methods (code, name, description, is_online, is_active, sort_order, icon_url) VALUES
    ('CASH', N'Tiền Mặt', N'Thanh toán bằng tiền mặt tại quầy', 0, 1, 1, '/icons/cash.png'),
    ('CARD', N'Thẻ Ngân Hàng', N'Thanh toán bằng thẻ ATM/Credit Card', 0, 1, 2, '/icons/card.png'),
    ('VNPAY', N'VNPay', N'Thanh toán qua VNPay QR Code', 1, 1, 3, '/icons/vnpay.png'),
    ('MOMO', N'Momo', N'Thanh toán qua Ví điện tử Momo', 1, 1, 4, '/icons/momo.png');
END
GO

-- =====================================================
-- Insert sample payment transactions
-- =====================================================
IF NOT EXISTS (SELECT * FROM payment_transactions)
BEGIN
    -- VNPay completed transaction
    INSERT INTO payment_transactions (
        id, transaction_number, sale_id, sale_number,
        payment_method, amount, currency, status,
        gateway_transaction_id, gateway_order_id,
        qr_code_url, payment_url, expires_at, paid_at,
        customer_id, created_at
    ) VALUES (
        'PAY0001-0001-0001-0001-000000000001',
        'PAY-2024-001',
        'SALE0001-0001-0001-0001-000000000002', -- SALE-2024-002
        'SALE-2024-002',
        'VNPAY', 378000, 'VND', 'COMPLETED',
        '14012024123456', 'VNPORDER2024001',
        'https://api.vietqr.io/image/970415-1234567890-compact.jpg',
        'https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...',
        DATEADD(MINUTE, 15, '2024-03-01 14:15:00'), -- Expires
        '2024-03-01 14:16:23', -- Paid
        '55555555-5555-5555-5555-555555555552', -- customer2@gmail.com
        '2024-03-01 14:15:00'
    ),
    
    -- Momo completed transaction
    (
        'PAY0001-0001-0001-0001-000000000002',
        'PAY-2024-002',
        'SALE0001-0001-0001-0001-000000000003', -- SALE-2024-003
        'SALE-2024-003',
        'MOMO', 195000, 'VND', 'COMPLETED',
        '1709359580789', 'MOMOORDER2024001',
        'https://api.momo.vn/qr/1709359580789',
        'https://test-payment.momo.vn/pay/...',
        DATEADD(MINUTE, 15, '2024-03-02 09:45:00'),
        '2024-03-02 09:46:12',
        '55555555-5555-5555-5555-555555555553', -- customer3@gmail.com
        '2024-03-02 09:45:00'
    ),
    
    -- VNPay pending transaction
    (
        'PAY0001-0001-0001-0001-000000000003',
        'PAY-2024-003',
        'SALE0001-0001-0001-0001-000000000005', -- SALE-2024-005
        'SALE-2024-005',
        'VNPAY', 280000, 'VND', 'PENDING',
        NULL, 'VNPORDER2024002',
        'https://api.vietqr.io/image/970415-1234567890-compact2.jpg',
        'https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?...',
        DATEADD(MINUTE, 15, '2024-03-03 16:00:00'),
        NULL, -- Not paid yet
        '55555555-5555-5555-5555-555555555554', -- customer4@gmail.com
        '2024-03-03 16:00:00'
    ),
    
    -- VNPay failed transaction
    (
        'PAY0001-0001-0001-0001-000000000004',
        'PAY-2024-004',
        NEWID(), -- Some sale
        'SALE-2024-006',
        'VNPAY', 150000, 'VND', 'FAILED',
        NULL, 'VNPORDER2024003',
        NULL, NULL,
        DATEADD(MINUTE, 15, '2024-03-03 10:00:00'),
        NULL,
        '55555555-5555-5555-5555-555555555551', -- customer1@gmail.com
        '2024-03-03 10:00:00'
    );
END
GO

-- =====================================================
-- Insert sample payment callbacks (IPN logs)
-- =====================================================
IF NOT EXISTS (SELECT * FROM payment_callbacks)
BEGIN
    -- VNPay IPN callback for PAY-2024-001
    INSERT INTO payment_callbacks (
        transaction_id, callback_type, payment_method,
        request_headers, request_body, request_ip,
        response_status, response_body,
        is_valid, is_processed, received_at, processed_at
    ) VALUES (
        'PAY0001-0001-0001-0001-000000000001',
        'IPN', 'VNPAY',
        '{"user-agent": "VNPay-IPN", "content-type": "application/x-www-form-urlencoded"}',
        'vnp_Amount=37800000&vnp_BankCode=NCB&vnp_OrderInfo=Payment+for+SALE-2024-002&vnp_ResponseCode=00&vnp_TmnCode=DEMO&vnp_TransactionNo=14012024123456&vnp_SecureHash=...',
        '203.0.113.5',
        200, '{"RspCode":"00","Message":"Success"}',
        1, 1,
        '2024-03-01 14:16:23', '2024-03-01 14:16:24'
    ),
    
    -- Momo IPN callback for PAY-2024-002
    (
        'PAY0001-0001-0001-0001-000000000002',
        'IPN', 'MOMO',
        '{"user-agent": "Momo-IPN", "content-type": "application/json"}',
        '{"partnerCode":"MOMO","orderId":"MOMOORDER2024001","requestId":"1709359580789","amount":195000,"orderInfo":"Payment for SALE-2024-003","resultCode":0,"message":"Successful.","responseTime":1709359572789,"signature":"..."}',
        '203.0.113.10',
        200, '{"status":"success"}',
        1, 1,
        '2024-03-02 09:46:12', '2024-03-02 09:46:13'
    );
END
GO

-- =====================================================
-- Insert sample refund
-- =====================================================
IF NOT EXISTS (SELECT * FROM payment_refunds)
BEGIN
    -- Refund for PAY-2024-001 (partial refund)
    INSERT INTO payment_refunds (
        id, refund_number, transaction_id,
        original_amount, refund_amount, reason,
        status, requested_by, requested_at, notes
    ) VALUES (
        'REF0001-0001-0001-0001-000000000001',
        'REF-2024-001',
        'PAY0001-0001-0001-0001-000000000001', -- PAY-2024-001
        378000, 180000, -- Refund 1 bag of rice (half the order)
        N'Customer returned 1 bag of Gạo ST25',
        'PENDING',
        '22222222-2222-2222-2222-222222222221', -- manager1@company.com
        '2024-03-02 10:00:00',
        N'Customer said rice was damaged'
    );
END
GO

-- =====================================================
-- Insert sample reconciliation
-- =====================================================
IF NOT EXISTS (SELECT * FROM payment_reconciliation)
BEGIN
    -- Daily reconciliation for March 1, 2024
    INSERT INTO payment_reconciliation (
        id, reconciliation_number, payment_method, reconciliation_date,
        total_transactions, total_amount, gateway_total_amount,
        status, discrepancy_count, reconciled_by, reconciled_at, notes
    ) VALUES (
        'REC0001-0001-0001-0001-000000000001',
        'REC-2024-0301-VNPAY',
        'VNPAY',
        '2024-03-01',
        1, -- 1 transaction (PAY-2024-001)
        378000, 378000, -- Matched
        'MATCHED', 0,
        '22222222-2222-2222-2222-222222222221', -- manager1@company.com
        '2024-03-02 08:00:00',
        N'Daily reconciliation completed - all transactions matched'
    ),
    
    -- Daily reconciliation for March 2, 2024
    (
        'REC0001-0001-0001-0001-000000000002',
        'REC-2024-0302-MOMO',
        'MOMO',
        '2024-03-02',
        1, -- 1 transaction (PAY-2024-002)
        195000, 195000, -- Matched
        'MATCHED', 0,
        '22222222-2222-2222-2222-222222222221', -- manager1@company.com
        '2024-03-03 08:00:00',
        N'Daily reconciliation completed'
    );
END
GO

PRINT '===============================================';
PRINT 'Payment Gateway Service Database Created Successfully';
PRINT '===============================================';
PRINT '';
PRINT 'Total Tables: 5 tables';
PRINT 'Sample Data:';
PRINT '  - 4 Payment methods (CASH, CARD, VNPAY, MOMO)';
PRINT '  - 4 Payment transactions';
PRINT '  - 2 Payment callbacks (IPN logs)';
PRINT '  - 1 Refund request (pending)';
PRINT '  - 2 Reconciliation records';
PRINT '';
PRINT 'Gateway Integrations: VNPay, Momo';
PRINT 'Total Transaction Value: 1,003,000 VND';
PRINT 'Successful Payments: 2 (378k + 195k = 573k)';
PRINT '===============================================';
GO
