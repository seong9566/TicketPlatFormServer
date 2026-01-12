-- ========================================
-- 채팅 기능 데이터베이스 검증 스크립트
-- ========================================

-- 1. 채팅방 상태 코드 확인 및 삽입
-- ========================================
SELECT * FROM chat_room_statuses;

-- 필요한 상태 코드가 없으면 삽입
INSERT IGNORE INTO chat_room_statuses (code, name_ko, is_active, sort_order) VALUES
('active', '활성', 1, 1),
('locked', '잠김', 1, 2),
('closed', '종료', 1, 3),
('cancelled', '취소됨', 1, 4);

-- 2. ChatRoom 테이블 인덱스 확인
-- ========================================
SHOW INDEX FROM chat_rooms;

-- 필요한 인덱스 추가
-- (ticket_id, buyer_id) UNIQUE 인덱스
CREATE UNIQUE INDEX IF NOT EXISTS idx_chat_rooms_ticket_buyer
ON chat_rooms (ticket_id, buyer_id);

-- buyer_id, last_message_at DESC 인덱스
CREATE INDEX IF NOT EXISTS idx_chat_rooms_buyer_last_message
ON chat_rooms (buyer_id, last_message_at DESC);

-- seller_id, last_message_at DESC 인덱스
CREATE INDEX IF NOT EXISTS idx_chat_rooms_seller_last_message
ON chat_rooms (seller_id, last_message_at DESC);

-- deleted_at 인덱스
CREATE INDEX IF NOT EXISTS idx_chat_rooms_deleted_at
ON chat_rooms (deleted_at);

-- transaction_id 인덱스
CREATE INDEX IF NOT EXISTS idx_chat_rooms_transaction_id
ON chat_rooms (transaction_id);

-- 3. ChatMessage 테이블 인덱스 확인
-- ========================================
SHOW INDEX FROM chat_messages;

-- 필요한 인덱스 추가
-- (room_id, created_at DESC) 인덱스
CREATE INDEX IF NOT EXISTS idx_chat_messages_room_created
ON chat_messages (room_id, created_at DESC);

-- room_id 인덱스 (이미 있을 수 있음)
CREATE INDEX IF NOT EXISTS idx_chat_messages_room_id
ON chat_messages (room_id);

-- 4. 테이블 구조 확인
-- ========================================
DESC chat_rooms;
DESC chat_messages;
DESC chat_room_statuses;

-- 5. 기존 데이터 확인
-- ========================================
SELECT COUNT(*) AS total_chat_rooms FROM chat_rooms WHERE deleted_at IS NULL;
SELECT COUNT(*) AS total_messages FROM chat_messages;
SELECT COUNT(*) AS total_statuses FROM chat_room_statuses WHERE is_active = 1;

-- 6. 인덱스 효율성 확인
-- ========================================
EXPLAIN SELECT * FROM chat_rooms
WHERE (buyer_id = 1 OR seller_id = 1)
AND deleted_at IS NULL
ORDER BY last_message_at DESC
LIMIT 20;

EXPLAIN SELECT * FROM chat_messages
WHERE room_id = 1
ORDER BY created_at DESC
LIMIT 50;

-- 7. 만료된 채팅방 조회 쿼리 테스트 (CleanupService용)
-- ========================================
SELECT cr.id, cr.closed_at, t.confirmed_at
FROM chat_rooms cr
LEFT JOIN transactions t ON cr.transaction_id = t.id
WHERE cr.deleted_at IS NULL
    AND (cr.closed_at IS NOT NULL OR t.confirmed_at IS NOT NULL)
    AND (
        (cr.closed_at IS NOT NULL AND cr.closed_at < DATE_SUB(NOW(), INTERVAL 90 DAY))
        OR
        (t.confirmed_at IS NOT NULL AND t.confirmed_at < DATE_SUB(NOW(), INTERVAL 90 DAY))
    )
LIMIT 10;

-- ========================================
-- 검증 완료 메시지
-- ========================================
SELECT
    '데이터베이스 검증이 완료되었습니다.' AS status,
    (SELECT COUNT(*) FROM chat_room_statuses WHERE is_active = 1) AS status_count,
    (SELECT COUNT(*) FROM information_schema.statistics
     WHERE table_schema = DATABASE()
     AND table_name = 'chat_rooms') AS chat_rooms_indexes,
    (SELECT COUNT(*) FROM information_schema.statistics
     WHERE table_schema = DATABASE()
     AND table_name = 'chat_messages') AS chat_messages_indexes;
