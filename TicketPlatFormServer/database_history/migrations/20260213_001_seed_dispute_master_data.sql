INSERT INTO dispute_types (id, code, name_ko, is_active, sort_order)
SELECT 1, 'FAKE_TICKET', '가짜/위조 티켓', 1, 1
WHERE NOT EXISTS (SELECT 1 FROM dispute_types WHERE code = 'FAKE_TICKET');

INSERT INTO dispute_types (id, code, name_ko, is_active, sort_order)
SELECT 2, 'WRONG_TICKET', '잘못된 티켓', 1, 2
WHERE NOT EXISTS (SELECT 1 FROM dispute_types WHERE code = 'WRONG_TICKET');

INSERT INTO dispute_types (id, code, name_ko, is_active, sort_order)
SELECT 3, 'NO_DELIVERY', '티켓 미배송', 1, 3
WHERE NOT EXISTS (SELECT 1 FROM dispute_types WHERE code = 'NO_DELIVERY');

INSERT INTO dispute_types (id, code, name_ko, is_active, sort_order)
SELECT 4, 'RUDE_BEHAVIOR', '비매너 행위', 1, 4
WHERE NOT EXISTS (SELECT 1 FROM dispute_types WHERE code = 'RUDE_BEHAVIOR');

INSERT INTO dispute_types (id, code, name_ko, is_active, sort_order)
SELECT 5, 'OTHER', '기타', 1, 5
WHERE NOT EXISTS (SELECT 1 FROM dispute_types WHERE code = 'OTHER');

INSERT INTO dispute_statuses (id, code, name_ko, is_active, sort_order)
SELECT 1, 'PENDING', '접수 대기', 1, 1
WHERE NOT EXISTS (SELECT 1 FROM dispute_statuses WHERE code = 'PENDING');

INSERT INTO dispute_statuses (id, code, name_ko, is_active, sort_order)
SELECT 2, 'IN_REVIEW', '검토 중', 1, 2
WHERE NOT EXISTS (SELECT 1 FROM dispute_statuses WHERE code = 'IN_REVIEW');

INSERT INTO dispute_statuses (id, code, name_ko, is_active, sort_order)
SELECT 3, 'RESOLVED_BUYER', '구매자 승', 1, 3
WHERE NOT EXISTS (SELECT 1 FROM dispute_statuses WHERE code = 'RESOLVED_BUYER');

INSERT INTO dispute_statuses (id, code, name_ko, is_active, sort_order)
SELECT 4, 'RESOLVED_SELLER', '판매자 승', 1, 4
WHERE NOT EXISTS (SELECT 1 FROM dispute_statuses WHERE code = 'RESOLVED_SELLER');

INSERT INTO dispute_statuses (id, code, name_ko, is_active, sort_order)
SELECT 5, 'REJECTED', '신고 기각', 1, 5
WHERE NOT EXISTS (SELECT 1 FROM dispute_statuses WHERE code = 'REJECTED');

INSERT INTO dispute_statuses (id, code, name_ko, is_active, sort_order)
SELECT 6, 'CANCELLED', '신고자 취소', 1, 6
WHERE NOT EXISTS (SELECT 1 FROM dispute_statuses WHERE code = 'CANCELLED');

INSERT INTO escrow_statuses (id, code, name_ko, is_active, sort_order)
SELECT 4, 'frozen', '동결', 1, 4
WHERE NOT EXISTS (SELECT 1 FROM escrow_statuses WHERE code = 'frozen');
