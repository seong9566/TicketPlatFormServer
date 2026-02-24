SET @comment_col_exists := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'user_reputation'
      AND COLUMN_NAME = 'comment'
);

SET @drop_comment_col_sql := IF(
    @comment_col_exists > 0,
    'ALTER TABLE user_reputation DROP COLUMN comment',
    'SELECT 1'
);

PREPARE stmt FROM @drop_comment_col_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
