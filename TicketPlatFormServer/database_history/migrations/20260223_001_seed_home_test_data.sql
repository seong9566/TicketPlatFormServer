-- ================================================================
-- Seed home visibility test data (events/tickets/favorites)
-- Created: 2026-02-23
-- Purpose: Add multiple near-term test datasets so home sections are visible
--          (popular/recommended and upcoming deadline deal candidates)
-- ================================================================

-- ================================================================
-- STEP 1: Insert test events (idempotent by title)
-- ================================================================

INSERT INTO events
(
    category_id,
    artist_id,
    title,
    description,
    poster_image_url,
    venue_name,
    venue_address,
    start_at,
    end_at,
    created_by_admin_id,
    is_active,
    sort_order
)
SELECT
    1,
    NULL,
    '[TEST] 마감임박 핫딜 콘서트 D-0',
    '홈 섹션 노출 확인용 테스트 이벤트 (오늘 공연)',
    'https://example.com/posters/test-deadline-d0.jpg',
    '테스트홀 A',
    '서울특별시 강남구 테스트로 10',
    DATE_ADD(CURDATE(), INTERVAL 10 HOUR),
    DATE_ADD(CURDATE(), INTERVAL 13 HOUR),
    NULL,
    1,
    910
WHERE NOT EXISTS
(
    SELECT 1 FROM events WHERE title = '[TEST] 마감임박 핫딜 콘서트 D-0'
);

INSERT INTO events
(
    category_id,
    artist_id,
    title,
    description,
    poster_image_url,
    venue_name,
    venue_address,
    start_at,
    end_at,
    created_by_admin_id,
    is_active,
    sort_order
)
SELECT
    1,
    NULL,
    '[TEST] 마감임박 핫딜 콘서트 D-1',
    '홈 섹션 노출 확인용 테스트 이벤트 (내일 공연)',
    'https://example.com/posters/test-deadline-d1.jpg',
    '테스트홀 B',
    '서울특별시 송파구 테스트로 20',
    DATE_ADD(CURDATE(), INTERVAL 1 DAY) + INTERVAL 19 HOUR,
    DATE_ADD(CURDATE(), INTERVAL 1 DAY) + INTERVAL 22 HOUR,
    NULL,
    1,
    911
WHERE NOT EXISTS
(
    SELECT 1 FROM events WHERE title = '[TEST] 마감임박 핫딜 콘서트 D-1'
);

INSERT INTO events
(
    category_id,
    artist_id,
    title,
    description,
    poster_image_url,
    venue_name,
    venue_address,
    start_at,
    end_at,
    created_by_admin_id,
    is_active,
    sort_order
)
SELECT
    1,
    NULL,
    '[TEST] 마감임박 핫딜 콘서트 D-2',
    '홈 섹션 노출 확인용 테스트 이벤트 (모레 공연)',
    'https://example.com/posters/test-deadline-d2.jpg',
    '테스트홀 C',
    '서울특별시 마포구 테스트로 30',
    DATE_ADD(CURDATE(), INTERVAL 2 DAY) + INTERVAL 18 HOUR,
    DATE_ADD(CURDATE(), INTERVAL 2 DAY) + INTERVAL 21 HOUR,
    NULL,
    1,
    912
WHERE NOT EXISTS
(
    SELECT 1 FROM events WHERE title = '[TEST] 마감임박 핫딜 콘서트 D-2'
);

INSERT INTO events
(
    category_id,
    artist_id,
    title,
    description,
    poster_image_url,
    venue_name,
    venue_address,
    start_at,
    end_at,
    created_by_admin_id,
    is_active,
    sort_order
)
SELECT
    1,
    NULL,
    '[TEST] 마감임박 핫딜 콘서트 D-3',
    '홈 섹션 노출 확인용 테스트 이벤트 (3일 후 공연)',
    'https://example.com/posters/test-deadline-d3.jpg',
    '테스트홀 D',
    '서울특별시 영등포구 테스트로 40',
    DATE_ADD(CURDATE(), INTERVAL 3 DAY) + INTERVAL 20 HOUR,
    DATE_ADD(CURDATE(), INTERVAL 3 DAY) + INTERVAL 23 HOUR,
    NULL,
    1,
    913
WHERE NOT EXISTS
(
    SELECT 1 FROM events WHERE title = '[TEST] 마감임박 핫딜 콘서트 D-3'
);

INSERT INTO events
(
    category_id,
    artist_id,
    title,
    description,
    poster_image_url,
    venue_name,
    venue_address,
    start_at,
    end_at,
    created_by_admin_id,
    is_active,
    sort_order
)
SELECT
    1,
    NULL,
    '[TEST] 비교군 콘서트 D-4 (제외 확인용)',
    'D-3 필터 제외 동작 확인용 테스트 이벤트',
    'https://example.com/posters/test-deadline-d4.jpg',
    '테스트홀 E',
    '서울특별시 용산구 테스트로 50',
    DATE_ADD(CURDATE(), INTERVAL 4 DAY) + INTERVAL 18 HOUR,
    DATE_ADD(CURDATE(), INTERVAL 4 DAY) + INTERVAL 21 HOUR,
    NULL,
    1,
    914
WHERE NOT EXISTS
(
    SELECT 1 FROM events WHERE title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
);

-- ================================================================
-- STEP 2: Insert event seat grades for test events (idempotent)
-- ================================================================

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 220000, 1, 1
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-0'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 1);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 2, 'R', 'R석', 'R', 160000, 1, 2
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-0'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 2);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 240000, 1, 1
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-1'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 1);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 2, 'R', 'R석', 'R', 170000, 1, 2
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-1'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 2);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 210000, 1, 1
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-2'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 1);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 2, 'R', 'R석', 'R', 150000, 1, 2
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-2'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 2);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 200000, 1, 1
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-3'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 1);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 2, 'R', 'R석', 'R', 140000, 1, 2
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-3'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 2);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 180000, 1, 1
FROM events e
WHERE e.title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 1);

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 2, 'R', 'R석', 'R', 130000, 1, 2
FROM events e
WHERE e.title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
  AND NOT EXISTS (SELECT 1 FROM event_seat_grades esg WHERE esg.event_id = e.id AND esg.seat_grade_id = 2);

-- ================================================================
-- STEP 3: Insert test tickets (idempotent by seller + schedule_id)
-- ================================================================

INSERT INTO tickets
(
    seller_id,
    event_id,
    schedule_id,
    category_id,
    event_datetime,
    seat_location_id,
    area_id,
    `row`,
    quantity,
    is_consecutive,
    remaining_quantity,
    price,
    description,
    status_id,
    seat_grade_id,
    trade_method_id,
    has_ticket,
    feature_ids
)
SELECT
    7,
    e.id,
    CONCAT('T007_TEST_D0_VIP_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'A-01',
    2,
    1,
    2,
    119000,
    'D-0 테스트 티켓 (VIP) - 홈 섹션 노출 확인용',
    1,
    esg.id,
    2,
    1,
    '2,4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-0'
  AND NOT EXISTS (SELECT 1 FROM tickets t WHERE t.seller_id = 7 AND t.schedule_id = CONCAT('T007_TEST_D0_VIP_', e.id));

INSERT INTO tickets
(
    seller_id,
    event_id,
    schedule_id,
    category_id,
    event_datetime,
    seat_location_id,
    area_id,
    `row`,
    quantity,
    is_consecutive,
    remaining_quantity,
    price,
    description,
    status_id,
    seat_grade_id,
    trade_method_id,
    has_ticket,
    feature_ids
)
SELECT
    8,
    e.id,
    CONCAT('T007_TEST_D1_R_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'B-11',
    3,
    1,
    3,
    99000,
    'D-1 테스트 티켓 (R석) - 홈 섹션 노출 확인용',
    1,
    esg.id,
    1,
    1,
    '2'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 2
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-1'
  AND NOT EXISTS (SELECT 1 FROM tickets t WHERE t.seller_id = 8 AND t.schedule_id = CONCAT('T007_TEST_D1_R_', e.id));

INSERT INTO tickets
(
    seller_id,
    event_id,
    schedule_id,
    category_id,
    event_datetime,
    seat_location_id,
    area_id,
    `row`,
    quantity,
    is_consecutive,
    remaining_quantity,
    price,
    description,
    status_id,
    seat_grade_id,
    trade_method_id,
    has_ticket,
    feature_ids
)
SELECT
    9,
    e.id,
    CONCAT('T007_TEST_D2_VIP_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'C-21',
    4,
    0,
    4,
    89000,
    'D-2 테스트 티켓 (VIP) - 홈 섹션 노출 확인용',
    1,
    esg.id,
    3,
    1,
    '4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-2'
  AND NOT EXISTS (SELECT 1 FROM tickets t WHERE t.seller_id = 9 AND t.schedule_id = CONCAT('T007_TEST_D2_VIP_', e.id));

INSERT INTO tickets
(
    seller_id,
    event_id,
    schedule_id,
    category_id,
    event_datetime,
    seat_location_id,
    area_id,
    `row`,
    quantity,
    is_consecutive,
    remaining_quantity,
    price,
    description,
    status_id,
    seat_grade_id,
    trade_method_id,
    has_ticket,
    feature_ids
)
SELECT
    12,
    e.id,
    CONCAT('T007_TEST_D3_R_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'D-31',
    2,
    1,
    2,
    75000,
    'D-3 테스트 티켓 (R석) - 홈 섹션 노출 확인용',
    1,
    esg.id,
    1,
    1,
    '2,4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 2
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-3'
  AND NOT EXISTS (SELECT 1 FROM tickets t WHERE t.seller_id = 12 AND t.schedule_id = CONCAT('T007_TEST_D3_R_', e.id));

INSERT INTO tickets
(
    seller_id,
    event_id,
    schedule_id,
    category_id,
    event_datetime,
    seat_location_id,
    area_id,
    `row`,
    quantity,
    is_consecutive,
    remaining_quantity,
    price,
    description,
    status_id,
    seat_grade_id,
    trade_method_id,
    has_ticket,
    feature_ids
)
SELECT
    11,
    e.id,
    CONCAT('T007_TEST_D4_CTRL_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'E-41',
    2,
    0,
    2,
    69000,
    'D-4 비교군 티켓 (필터 제외 확인용)',
    1,
    esg.id,
    2,
    1,
    '4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 2
WHERE e.title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
  AND NOT EXISTS (SELECT 1 FROM tickets t WHERE t.seller_id = 11 AND t.schedule_id = CONCAT('T007_TEST_D4_CTRL_', e.id));

-- ================================================================
-- STEP 4: Add event favorites for recommendation visibility
-- ================================================================

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 12, 1, e.id
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-0'
  AND NOT EXISTS (SELECT 1 FROM user_favorites uf WHERE uf.user_id = 12 AND uf.favorite_type_id = 1 AND uf.target_id = e.id);

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 13, 1, e.id
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-0'
  AND NOT EXISTS (SELECT 1 FROM user_favorites uf WHERE uf.user_id = 13 AND uf.favorite_type_id = 1 AND uf.target_id = e.id);

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 13, 1, e.id
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-1'
  AND NOT EXISTS (SELECT 1 FROM user_favorites uf WHERE uf.user_id = 13 AND uf.favorite_type_id = 1 AND uf.target_id = e.id);

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 14, 1, e.id
FROM events e
WHERE e.title = '[TEST] 마감임박 핫딜 콘서트 D-2'
  AND NOT EXISTS (SELECT 1 FROM user_favorites uf WHERE uf.user_id = 14 AND uf.favorite_type_id = 1 AND uf.target_id = e.id);

-- ================================================================
-- Verification queries
-- ================================================================

SELECT
    id,
    title,
    DATE(start_at) AS event_date,
    DATEDIFF(DATE(start_at), CURDATE()) AS days_left,
    is_active
FROM events
WHERE title LIKE '[TEST] %'
ORDER BY start_at ASC;

SELECT
    t.id,
    t.event_id,
    e.title,
    t.seller_id,
    t.schedule_id,
    t.quantity,
    t.remaining_quantity,
    t.price,
    t.status_id
FROM tickets t
INNER JOIN events e ON e.id = t.event_id
WHERE e.title LIKE '[TEST] %'
ORDER BY t.id ASC;

SELECT
    uf.user_id,
    uf.favorite_type_id,
    uf.target_id,
    e.title
FROM user_favorites uf
INNER JOIN events e ON e.id = uf.target_id
WHERE uf.favorite_type_id = 1
  AND e.title LIKE '[TEST] %'
ORDER BY uf.user_id, uf.target_id;
