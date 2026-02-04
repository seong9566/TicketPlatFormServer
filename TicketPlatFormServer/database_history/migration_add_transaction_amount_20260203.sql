-- Migration: Add Amount column to transaction table
-- Date: 2026-02-03
-- Purpose: Flutter 팀 요청 - Transaction API에 amount 필드 추가
-- Related: GET /api/chat/rooms/detail 응답에 거래 금액 포함

USE TicketPlatFormDB;

-- 1. Add Amount column to transaction table
ALTER TABLE `transactions`
ADD COLUMN amount INT NULL COMMENT '총 거래 금액 (TransactionItem의 TotalPrice 합계)';

-- 2. Migrate existing data: Set Amount from TransactionItem.TotalPrice
UPDATE `transactions` t
SET t.amount = (
    SELECT SUM(ti.total_price)
    FROM `transaction_items` ti
    WHERE ti.transaction_id = t.id
)
WHERE t.amount IS NULL;

-- 3. Verify migration
SELECT 
    t.id AS TransactionId,
    t.amount AS TransactionAmount,
    SUM(ti.total_price) AS CalculatedAmount,
    CASE 
        WHEN t.amount = SUM(ti.total_price) THEN 'OK'
        WHEN t.amount IS NULL AND SUM(ti.total_price) IS NULL THEN 'OK (No Items)'
        ELSE 'MISMATCH'
    END AS ValidationStatus
FROM `transactions` t
LEFT JOIN `transaction_items` ti ON ti.transaction_id = t.id
GROUP BY t.id, t.amount
ORDER BY t.id DESC
LIMIT 10;
