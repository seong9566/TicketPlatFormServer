ALTER TABLE user_profile
ADD COLUMN IF NOT EXISTS average_rating DECIMAL(3,2) NULL DEFAULT NULL,
ADD COLUMN IF NOT EXISTS review_count INT NOT NULL DEFAULT 0;

SET @uk_exists := (
    SELECT COUNT(*)
    FROM information_schema.statistics
    WHERE table_schema = DATABASE()
      AND table_name = 'user_reputation'
      AND index_name = 'uk_reputation_tx_reviewer'
);

SET @dup_exists := (
    SELECT COUNT(*)
    FROM (
        SELECT transaction_id, reviewer_id, COUNT(*) AS cnt
        FROM user_reputation
        GROUP BY transaction_id, reviewer_id
        HAVING COUNT(*) > 1
    ) AS dup
);

SET @uk_sql := IF(
    @uk_exists > 0 OR @dup_exists > 0,
    'SELECT 1',
    'ALTER TABLE user_reputation ADD UNIQUE KEY uk_reputation_tx_reviewer (transaction_id, reviewer_id)'
);

PREPARE stmt_add_uk FROM @uk_sql;
EXECUTE stmt_add_uk;
DEALLOCATE PREPARE stmt_add_uk;

INSERT INTO reputation_rating_types (id, code, name_ko, is_active, sort_order)
VALUES (1, 'GENERAL', '전체 평가', 1, 1)
ON DUPLICATE KEY UPDATE
    name_ko = VALUES(name_ko),
    is_active = VALUES(is_active),
    sort_order = VALUES(sort_order);

INSERT INTO notification_types (code, name_ko, is_active)
VALUES ('REVIEW_REQUEST', '리뷰 요청', 1)
ON DUPLICATE KEY UPDATE is_active = 1;
