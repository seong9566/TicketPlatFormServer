-- ================================================================
-- Seed settlement_statuses (idempotent)
-- Created: 2026-02-12
-- Purpose: Ensure settlement status codes exist for escrow release flow
-- ================================================================

INSERT INTO settlement_statuses (id, code, name_ko, is_active, sort_order)
SELECT 1001, 'pending', '정산 대기', 1, 1
WHERE NOT EXISTS (
    SELECT 1 FROM settlement_statuses WHERE code = 'pending'
);

INSERT INTO settlement_statuses (id, code, name_ko, is_active, sort_order)
SELECT 1002, 'processing', '정산 처리중', 1, 2
WHERE NOT EXISTS (
    SELECT 1 FROM settlement_statuses WHERE code = 'processing'
);

INSERT INTO settlement_statuses (id, code, name_ko, is_active, sort_order)
SELECT 1003, 'completed', '정산 완료', 1, 3
WHERE NOT EXISTS (
    SELECT 1 FROM settlement_statuses WHERE code = 'completed'
);

INSERT INTO settlement_statuses (id, code, name_ko, is_active, sort_order)
SELECT 1004, 'failed', '정산 실패', 1, 4
WHERE NOT EXISTS (
    SELECT 1 FROM settlement_statuses WHERE code = 'failed'
);

SELECT id, code, name_ko, is_active, sort_order
FROM settlement_statuses
ORDER BY sort_order, id;
