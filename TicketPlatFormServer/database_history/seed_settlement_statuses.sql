-- ================================================
-- Settlement Statuses Seed Data
-- 정산 상태 기초 데이터
-- ================================================

INSERT INTO settlement_statuses (code, name_ko, is_active, sort_order) VALUES
('pending', '정산 대기', true, 1),
('processing', '정산 처리중', true, 2),
('completed', '정산 완료', true, 3),
('failed', '정산 실패', true, 4)
ON DUPLICATE KEY UPDATE
    name_ko = VALUES(name_ko),
    is_active = VALUES(is_active),
    sort_order = VALUES(sort_order);
