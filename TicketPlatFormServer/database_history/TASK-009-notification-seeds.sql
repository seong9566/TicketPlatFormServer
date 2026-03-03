-- TASK-009: 정산 알림 타입 시드
INSERT INTO notification_types (id, code, name_ko, is_active, sort_order)
VALUES (8, 'SETTLEMENT_COMPLETED', '정산 완료', 1, 8)
ON DUPLICATE KEY UPDATE is_active = 1;

INSERT INTO notification_types (id, code, name_ko, is_active, sort_order)
VALUES (9, 'SETTLEMENT_FAILED', '정산 실패', 1, 9)
ON DUPLICATE KEY UPDATE is_active = 1;
