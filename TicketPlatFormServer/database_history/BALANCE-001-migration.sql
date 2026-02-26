CREATE TABLE IF NOT EXISTS user_balance (
    id BIGINT NOT NULL AUTO_INCREMENT,
    user_id BIGINT NOT NULL,
    available BIGINT NOT NULL DEFAULT 0,
    pending BIGINT NOT NULL DEFAULT 0,
    total_earned BIGINT NOT NULL DEFAULT 0,
    total_withdrawn BIGINT NOT NULL DEFAULT 0,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_user_balance_user_id (user_id),
    KEY idx_user_balance_user_id (user_id)
);

CREATE TABLE IF NOT EXISTS balance_transactions (
    id BIGINT NOT NULL AUTO_INCREMENT,
    user_id BIGINT NOT NULL,
    type VARCHAR(32) NOT NULL COMMENT 'CREDIT, DEBIT, REFUND',
    amount BIGINT NOT NULL,
    balance_after BIGINT NOT NULL,
    reference_type VARCHAR(50) NULL,
    reference_id BIGINT NULL,
    description TEXT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY idx_balance_transactions_user_id (user_id),
    KEY idx_balance_transactions_reference (reference_type, reference_id)
);

CREATE TABLE IF NOT EXISTS withdrawal_status (
    id BIGINT NOT NULL AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL,
    name_ko VARCHAR(64) NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_withdrawal_status_code (code)
);

CREATE TABLE IF NOT EXISTS withdrawal (
    id BIGINT NOT NULL AUTO_INCREMENT,
    user_id BIGINT NOT NULL,
    bank_account_id BIGINT NOT NULL,
    amount BIGINT NOT NULL,
    fee BIGINT NOT NULL DEFAULT 0,
    net_amount BIGINT NOT NULL,
    status_id BIGINT NOT NULL,
    idempotency_key VARCHAR(100) NULL,
    payout_id VARCHAR(100) NULL,
    failure_reason TEXT NULL,
    retry_count INT NULL DEFAULT 0,
    requested_at DATETIME NOT NULL,
    processed_at DATETIME NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_withdrawal_idempotency_key (idempotency_key),
    KEY idx_withdrawal_user_id (user_id),
    KEY idx_withdrawal_bank_account_id (bank_account_id),
    KEY idx_withdrawal_status_id (status_id),
    CONSTRAINT fk_withdrawal_status FOREIGN KEY (status_id) REFERENCES withdrawal_status (id),
    CONSTRAINT fk_withdrawal_bank_account FOREIGN KEY (bank_account_id) REFERENCES bank_account (id)
);

INSERT INTO withdrawal_status (id, code, name_ko)
VALUES
    (1, 'REQUESTED', '요청됨'),
    (2, 'PROCESSING', '처리중'),
    (3, 'COMPLETED', '완료됨'),
    (4, 'FAILED', '실패'),
    (5, 'CANCELLED', '취소됨')
ON DUPLICATE KEY UPDATE
    name_ko = VALUES(name_ko);
