-- ================================================================
-- Reseed deadlineDeals scenario data
-- Created: 2026-02-23
-- Purpose: Remove previous incorrect test rows and insert dedicated
--          deadlineDeals dataset (D-0~D-3 + exclusion controls)
-- ================================================================

-- ================================================================
-- STEP 1: Clean up previous temporary datasets
-- ================================================================

DELETE FROM user_favorites
WHERE favorite_type_id = 1
  AND target_id IN
  (
      SELECT id
      FROM events
      WHERE title LIKE '[TEST] 마감임박 핫딜%'
         OR title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
         OR title LIKE '[DEADLINE] %'
  );

DELETE FROM tickets
WHERE schedule_id LIKE 'T007_TEST_%'
   OR schedule_id LIKE 'T007_DD_%'
   OR event_id IN
   (
       SELECT id
       FROM events
       WHERE title LIKE '[TEST] 마감임박 핫딜%'
          OR title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
          OR title LIKE '[DEADLINE] %'
   );

DELETE FROM event_seat_grades
WHERE event_id IN
(
    SELECT id
    FROM events
    WHERE title LIKE '[TEST] 마감임박 핫딜%'
       OR title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
       OR title LIKE '[DEADLINE] %'
);

DELETE FROM events
WHERE title LIKE '[TEST] 마감임박 핫딜%'
   OR title = '[TEST] 비교군 콘서트 D-4 (제외 확인용)'
   OR title LIKE '[DEADLINE] %';

-- ================================================================
-- STEP 2: Insert deadlineDeals target events
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
VALUES
(
    1,
    NULL,
    '[DEADLINE] 핫딜 K-POP 쇼케이스 D-1',
    'deadlineDeals 노출 검증용 (D-1, 고할인)',
    'https://example.com/posters/deadline-kpop-d1.jpg',
    '잠실 실내체육관',
    '서울 송파구 올림픽로 25',
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 1 DAY), '20:00:00'),
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 1 DAY), '22:30:00'),
    NULL,
    1,
    920
),
(
    1,
    NULL,
    '[DEADLINE] 핫딜 시티팝 라이브 D-0',
    'deadlineDeals 노출 검증용 (D-0)',
    'https://example.com/posters/deadline-citypop-d0.jpg',
    '홍대 라이브홀',
    '서울 마포구 와우산로 77',
    TIMESTAMP(CURDATE(), '19:00:00'),
    TIMESTAMP(CURDATE(), '21:30:00'),
    NULL,
    1,
    921
),
(
    1,
    NULL,
    '[DEADLINE] 핫딜 재즈 페스티벌 D-2',
    'deadlineDeals 노출 검증용 (D-2)',
    'https://example.com/posters/deadline-jazz-d2.jpg',
    '세종문화회관',
    '서울 종로구 세종대로 175',
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 2 DAY), '18:00:00'),
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 2 DAY), '20:30:00'),
    NULL,
    1,
    922
),
(
    3,
    NULL,
    '[DEADLINE] 핫딜 뮤지컬 갈라 D-1',
    'deadlineDeals 노출 검증용 (D-1)',
    'https://example.com/posters/deadline-musical-d1.jpg',
    '예술의전당 오페라극장',
    '서울 서초구 남부순환로 2406',
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 1 DAY), '17:00:00'),
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 1 DAY), '19:30:00'),
    NULL,
    1,
    923
),
(
    1,
    NULL,
    '[DEADLINE] 핫딜 오케스트라 나이트 D-3',
    'deadlineDeals 노출 검증용 (D-3)',
    'https://example.com/posters/deadline-orchestra-d3.jpg',
    '롯데콘서트홀',
    '서울 송파구 올림픽로 300',
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 3 DAY), '20:00:00'),
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 3 DAY), '22:00:00'),
    NULL,
    1,
    924
),
(
    1,
    NULL,
    '[DEADLINE] 핫딜 인디밴드 클럽공연 D-0',
    'deadlineDeals 노출 검증용 (D-0)',
    'https://example.com/posters/deadline-indie-d0.jpg',
    '합정 클럽A',
    '서울 마포구 양화로 50',
    TIMESTAMP(CURDATE(), '21:00:00'),
    TIMESTAMP(CURDATE(), '23:00:00'),
    NULL,
    1,
    925
),
(
    1,
    NULL,
    '[DEADLINE] 제외 샘플 D-4',
    'D-3 범위 밖 제외 검증용',
    'https://example.com/posters/deadline-out-d4.jpg',
    '테스트홀 외곽',
    '서울 강동구 테스트로 1',
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 4 DAY), '18:00:00'),
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 4 DAY), '20:00:00'),
    NULL,
    1,
    926
),
(
    1,
    NULL,
    '[DEADLINE] 제외 샘플 매진 D-2',
    '남은 수량 0 제외 검증용',
    'https://example.com/posters/deadline-soldout-d2.jpg',
    '테스트홀 매진',
    '서울 중구 테스트로 2',
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 2 DAY), '16:00:00'),
    TIMESTAMP(DATE_ADD(CURDATE(), INTERVAL 2 DAY), '18:00:00'),
    NULL,
    1,
    927
);

-- ================================================================
-- STEP 3: Insert event seat grades for deadline events
-- ================================================================

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 200000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 K-POP 쇼케이스 D-1';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 200000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 시티팝 라이브 D-0';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 220000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 재즈 페스티벌 D-2';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 180000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 뮤지컬 갈라 D-1';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 200000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 오케스트라 나이트 D-3';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 170000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 인디밴드 클럽공연 D-0';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 300000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 제외 샘플 D-4';

INSERT INTO event_seat_grades (event_id, seat_grade_id, code, name_ko, name_en, original_price, is_active, sort_order)
SELECT e.id, 1, 'VIP', 'VIP석', 'VIP', 200000, 1, 1
FROM events e
WHERE e.title = '[DEADLINE] 제외 샘플 매진 D-2';

-- ================================================================
-- STEP 4: Insert deadlineDeals target tickets
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
    CONCAT('T007_DD_E1_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'A-01',
    4,
    1,
    4,
    70000,
    'deadlineDeals D-1 고할인(65%) 샘플',
    1,
    esg.id,
    2,
    1,
    '2,4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 K-POP 쇼케이스 D-1';

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
    CONCAT('T007_DD_E2_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'B-11',
    2,
    1,
    2,
    90000,
    'deadlineDeals D-0 고할인(55%) 샘플',
    1,
    esg.id,
    1,
    1,
    '2'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 시티팝 라이브 D-0';

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
    CONCAT('T007_DD_E2_B_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'B-12',
    1,
    0,
    1,
    100000,
    'deadlineDeals D-0 보조 티켓 샘플',
    1,
    esg.id,
    2,
    1,
    '4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 시티팝 라이브 D-0';

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
    10,
    e.id,
    CONCAT('T007_DD_E3_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'C-21',
    3,
    0,
    3,
    121000,
    'deadlineDeals D-2 할인(45%) 샘플',
    1,
    esg.id,
    3,
    1,
    '2,4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 재즈 페스티벌 D-2';

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
    CONCAT('T007_DD_E4_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'D-31',
    2,
    1,
    2,
    108000,
    'deadlineDeals D-1 할인(40%) 샘플',
    1,
    esg.id,
    1,
    1,
    '2'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 뮤지컬 갈라 D-1';

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
    CONCAT('T007_DD_E4_B_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'D-32',
    1,
    0,
    1,
    126000,
    'deadlineDeals D-1 보조 티켓 샘플',
    1,
    esg.id,
    2,
    1,
    '4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 뮤지컬 갈라 D-1';

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
    CONCAT('T007_DD_E5_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'E-41',
    3,
    1,
    3,
    130000,
    'deadlineDeals D-3 할인(35%) 샘플 A',
    1,
    esg.id,
    3,
    1,
    '2,4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 오케스트라 나이트 D-3';

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
    CONCAT('T007_DD_E5_B_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'E-42',
    2,
    0,
    2,
    130000,
    'deadlineDeals D-3 할인(35%) 샘플 B',
    1,
    esg.id,
    1,
    1,
    '2'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 오케스트라 나이트 D-3';

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
    CONCAT('T007_DD_E5_C_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'E-43',
    2,
    0,
    2,
    130000,
    'deadlineDeals D-3 할인(35%) 샘플 C',
    1,
    esg.id,
    2,
    1,
    '4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 오케스트라 나이트 D-3';

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
    10,
    e.id,
    CONCAT('T007_DD_E6_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'F-51',
    2,
    1,
    2,
    119000,
    'deadlineDeals D-0 할인(30%) 샘플',
    1,
    esg.id,
    1,
    1,
    '2,4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 핫딜 인디밴드 클럽공연 D-0';

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
    CONCAT('T007_DD_E7_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'G-61',
    1,
    0,
    1,
    90000,
    'deadlineDeals D-4 제외 검증용',
    1,
    esg.id,
    2,
    1,
    '4'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 제외 샘플 D-4';

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
    CONCAT('T007_DD_E8_A_', e.id),
    e.category_id,
    e.start_at,
    NULL,
    NULL,
    'H-71',
    2,
    0,
    0,
    100000,
    'deadlineDeals 매진 제외 검증용',
    1,
    esg.id,
    1,
    1,
    '2'
FROM events e
INNER JOIN event_seat_grades esg ON esg.event_id = e.id AND esg.seat_grade_id = 1
WHERE e.title = '[DEADLINE] 제외 샘플 매진 D-2';

-- ================================================================
-- STEP 5: Add event favorites (optional support for home lists)
-- ================================================================

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 12, 1, e.id
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 K-POP 쇼케이스 D-1';

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 13, 1, e.id
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 K-POP 쇼케이스 D-1';

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 13, 1, e.id
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 시티팝 라이브 D-0';

INSERT INTO user_favorites (user_id, favorite_type_id, target_id)
SELECT 14, 1, e.id
FROM events e
WHERE e.title = '[DEADLINE] 핫딜 재즈 페스티벌 D-2';

-- ================================================================
-- Verification queries
-- ================================================================

SELECT
    e.id,
    e.title,
    DATE(e.start_at) AS event_date,
    DATEDIFF(DATE(e.start_at), CURDATE()) AS days_left
FROM events e
WHERE e.title LIKE '[DEADLINE] %'
ORDER BY e.start_at ASC;

SELECT
    t.id,
    e.title,
    t.schedule_id,
    t.quantity,
    t.remaining_quantity,
    t.price,
    t.status_id
FROM tickets t
INNER JOIN events e ON e.id = t.event_id
WHERE t.schedule_id LIKE 'T007_DD_%'
ORDER BY t.id ASC;

SELECT
    e.title,
    DATEDIFF(DATE(e.start_at), CURDATE()) AS days_left,
    MAX(
        CASE
            WHEN COALESCE(esg.original_price, t.price) > 0
            THEN ROUND((COALESCE(esg.original_price, t.price) - t.price) / COALESCE(esg.original_price, t.price) * 100)
            ELSE 0
        END
    ) AS ticket_discount_rate,
    COUNT(t.id) AS available_ticket_count,
    MIN(t.price) AS min_ticket_price,
    MIN(COALESCE(esg.original_price, t.price)) AS original_min_ticket_price
FROM events e
INNER JOIN tickets t ON e.id = t.event_id
LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
WHERE e.is_active = 1
  AND t.status_id = 1
  AND t.deleted_at IS NULL
  AND t.remaining_quantity > 0
  AND DATE(e.start_at) >= CURDATE()
  AND DATE(e.start_at) <= DATE_ADD(CURDATE(), INTERVAL 3 DAY)
  AND e.title LIKE '[DEADLINE] %'
GROUP BY e.id, e.title, e.start_at
ORDER BY ticket_discount_rate DESC, days_left ASC, available_ticket_count DESC;
