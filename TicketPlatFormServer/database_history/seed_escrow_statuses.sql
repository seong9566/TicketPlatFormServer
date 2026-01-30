-- 에스크로 상태 코드 시드 (필수)
-- 사전 시드 후 결제 기능이 정상 동작합니다.

INSERT INTO escrow_statuses (id, code, name_ko, is_active, sort_order) VALUES
(1, 'holding', '보관 중', true, 1),
(2, 'released', '정산 완료', true, 2),
(3, 'refunded', '환불 완료', true, 3)
ON DUPLICATE KEY UPDATE
    name_ko = VALUES(name_ko),
    is_active = VALUES(is_active),
    sort_order = VALUES(sort_order);

-- 확인용
SELECT id, code, name_ko, is_active, sort_order
FROM escrow_statuses
WHERE code IN ('holding', 'released', 'refunded')
ORDER BY sort_order;
