-- 결제 수단 코드 시드 (필수)
-- 사전 시드 후 결제 기능이 정상 동작합니다.

INSERT INTO payment_methods (id, code, name_ko, is_active, sort_order) VALUES
(1, 'card', '카드', true, 1),
(2, 'virtual_account', '가상계좌', true, 2),
(3, 'transfer', '계좌이체', true, 3),
(4, 'mobile', '휴대폰', true, 4),
(5, 'easy_pay', '간편결제', true, 5)
ON DUPLICATE KEY UPDATE
    name_ko = VALUES(name_ko),
    is_active = VALUES(is_active),
    sort_order = VALUES(sort_order);

-- 확인용
SELECT id, code, name_ko, is_active, sort_order
FROM payment_methods
WHERE code IN ('card', 'virtual_account', 'transfer', 'mobile', 'easy_pay')
ORDER BY sort_order;
