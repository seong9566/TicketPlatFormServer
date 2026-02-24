SET @fk_exists := (
    SELECT COUNT(*)
    FROM information_schema.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'settlements'
      AND CONSTRAINT_NAME = 'fk_settlements_bank'
);

SET @drop_fk_sql := IF(
    @fk_exists > 0,
    'ALTER TABLE settlements DROP FOREIGN KEY fk_settlements_bank',
    'SELECT 1'
);
PREPARE stmt FROM @drop_fk_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE settlements
    MODIFY COLUMN bank_account_id BIGINT NULL COMMENT '정산 계좌 FK';

SET @fk_exists := (
    SELECT COUNT(*)
    FROM information_schema.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'settlements'
      AND CONSTRAINT_NAME = 'fk_settlements_bank'
);

SET @add_fk_sql := IF(
    @fk_exists = 0,
    'ALTER TABLE settlements ADD CONSTRAINT fk_settlements_bank FOREIGN KEY (bank_account_id) REFERENCES bank_account (id)',
    'SELECT 1'
);
PREPARE stmt FROM @add_fk_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
