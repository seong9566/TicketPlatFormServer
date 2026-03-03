-- BUGFIX-001: Transaction 22 Settlement 누락 복구
-- 원인: PaymentRepository의 Dapper 메서드가 EF Core 트랜잭션에 참여하지 않아
--       escrow UPDATE는 AUTO-COMMIT으로 성공했으나 Settlement INSERT는 FK 위반으로 실패
-- 영향: escrow=released, transaction=confirmed이지만 settlement 레코드 없음
-- 멱등성: WHERE NOT EXISTS로 중복 삽입 방지

INSERT INTO settlements (transaction_id, seller_id, amount, fee, net_amount, bank_account_id, status_id, scheduled_at, created_at)
SELECT 22, 22, 900000, 31500, 868500, NULL, 1005, DATE_ADD(NOW(), INTERVAL 3 DAY), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM settlements WHERE transaction_id = 22
);
