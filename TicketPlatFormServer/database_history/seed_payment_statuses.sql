-- 결제 상태 코드 시드 (필수)
-- 사전 시드 후 결제 기능이 정상 동작합니다.

INSERT INTO payment_statuses (id, code, name_ko, is_active, sort_order) VALUES
(1, 'pending', '결제 대기', true, 1),
(2, 'paid', '결제 완료', true, 2),
(3, 'cancelled', '결제 취소', true, 3)
ON DUPLICATE KEY UPDATE
    name_ko = VALUES(name_ko),
    is_active = VALUES(is_active),
    sort_order = VALUES(sort_order);

-- 확인용
SELECT id, code, name_ko, is_active, sort_order
FROM payment_statuses
WHERE code IN ('pending', 'paid', 'cancelled')
ORDER BY sort_order;
