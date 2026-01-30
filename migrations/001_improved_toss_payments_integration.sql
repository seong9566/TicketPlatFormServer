-- =========================================================================
-- 토스페이먼츠 통합 개선 마이그레이션 (Codex 검증 완료)
-- =========================================================================
-- 작성일: 2026-01-28
-- 목적: TossPaymentResponseDto와 DB 스키마 완전 동기화
-- Codex 피드백 반영: Online DDL, 데이터 타입, 보안, 정규화 개선
-- =========================================================================

-- -------------------------------------------------------------------------
-- Phase 1: payments 테이블 확장 (Online DDL Safe)
-- -------------------------------------------------------------------------

-- Step 1-0: PK 타입 정합성 개선 (선택) (BIGINT → BIGINT UNSIGNED)
-- 주의: 기존 FK가 있는 경우 동일 타입으로 먼저 정합성 확보 필요
ALTER TABLE `payments`
    MODIFY COLUMN `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ALGORITHM=INPLACE, LOCK=NONE;

-- Step 1-1: 기존 amount 컬럼 타입 변경 (INT → BIGINT UNSIGNED)
ALTER TABLE `payments`
    MODIFY COLUMN `amount` BIGINT UNSIGNED NOT NULL COMMENT '결제 금액 (KRW, 원 단위)',
    ALGORITHM=INPLACE, LOCK=NONE;

-- Step 1-2: 새 컬럼 추가 (AFTER 제거, 맨 끝에 추가 for INSTANT)
ALTER TABLE `payments`
    -- 에스크로 및 취소 관련 (최우선)
    ADD COLUMN `use_escrow` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '에스크로 사용 여부',
    ADD COLUMN `is_partial_cancelable` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '부분 취소 가능 여부',

    -- 결제 유형 및 추적
    ADD COLUMN `payment_type` VARCHAR(20) NULL COMMENT '결제 타입 (NORMAL, BILLING)',
    ADD COLUMN `last_transaction_key` VARCHAR(255) NULL COMMENT '최종 거래 키 (deprecated: use payment_transactions)',

    -- 가맹점 및 시스템 정보
    ADD COLUMN `merchant_id` VARCHAR(50) NULL COMMENT '토스 가맹점 ID (mId)',
    ADD COLUMN `api_version` VARCHAR(20) NULL COMMENT '토스 API 버전',
    ADD COLUMN `country` CHAR(2) DEFAULT 'KR' COMMENT '국가 코드 (ISO-3166-1 alpha-2)',

    -- 선택 필드
    ADD COLUMN `culture_expense` BOOLEAN DEFAULT FALSE COMMENT '문화비 소득공제 여부',
    ADD COLUMN `metadata` JSON NULL COMMENT '커스텀 메타데이터',
    ADD COLUMN `discount_info` JSON NULL COMMENT '할인 정보',

    ALGORITHM=INSTANT, LOCK=NONE;

-- Step 1-2.5: payment_key/order_id 컬럼을 대소문자 구분 콜레이션으로 변경
ALTER TABLE `payments`
    MODIFY COLUMN `payment_key` VARCHAR(255) COLLATE utf8mb4_0900_as_cs NULL,
    MODIFY COLUMN `order_id` VARCHAR(255) COLLATE utf8mb4_0900_as_cs NULL,
    ALGORITHM=INPLACE, LOCK=NONE;

-- Step 1-2.6: 유니크 인덱스 추가 전 중복 데이터 정리 (더미 데이터 기준)
-- payment_key 중복 제거 (NULL 제외, 최신 id 유지)
DELETE p
FROM `payments` p
JOIN (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY payment_key ORDER BY id DESC) AS rn
    FROM `payments`
    WHERE payment_key IS NOT NULL
) d ON p.id = d.id
WHERE d.rn > 1;

-- order_id 중복 제거 (NULL 제외, 최신 id 유지)
DELETE p
FROM `payments` p
JOIN (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY order_id ORDER BY id DESC) AS rn
    FROM `payments`
    WHERE order_id IS NOT NULL
) d ON p.id = d.id
WHERE d.rn > 1;

-- Step 1-3: 유니크 제약 추가 (Idempotency 보장)
ALTER TABLE `payments`
    ADD UNIQUE INDEX `uk_payments_payment_key` (`payment_key`),
    ADD UNIQUE INDEX `uk_payments_order_id` (`order_id`),
    ADD INDEX `idx_payments_transaction_id` (`transaction_id`),
    ALGORITHM=INPLACE, LOCK=NONE;

-- Step 1-4: 필요한 인덱스만 추가 (boolean 필드 인덱스 제외)
ALTER TABLE `payments`
    ADD INDEX `idx_payments_type` (`payment_type`),
    ADD INDEX `idx_payments_merchant` (`merchant_id`),
    ADD INDEX `idx_payments_paid_at` (`paid_at`),
    ADD INDEX `idx_payments_status` (`status_id`),
    ALGORITHM=INPLACE, LOCK=NONE;

-- -------------------------------------------------------------------------
-- Phase 2: 결제 수단별 상세 테이블 생성 (개선판)
-- -------------------------------------------------------------------------

-- Table 2-1: 카드 결제 상세 정보
CREATE TABLE `payment_card_details` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `payment_id` BIGINT UNSIGNED NOT NULL COMMENT 'payments FK',

    -- 기존 필드
    `company` VARCHAR(50) NOT NULL COMMENT '카드사명',
    `card_number` VARCHAR(20) NOT NULL COMMENT '마스킹된 카드번호 (PCI DSS 준수)',
    `installment_plan_months` INT NOT NULL DEFAULT 0 COMMENT '할부 개월 수',
    `approve_no` VARCHAR(50) NOT NULL COMMENT '승인번호',
    `card_type` VARCHAR(20) NOT NULL COMMENT '신용/체크',
    `owner_type` VARCHAR(20) NOT NULL COMMENT '개인/법인',
    `acquire_status` VARCHAR(50) NOT NULL COMMENT '매입 상태',
    `is_interest_free` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '무이자 여부',

    -- 신규 추가 필드 (Codex 검증 완료)
    `issuer_code` VARCHAR(10) NULL COMMENT '카드 발급사 코드',
    `acquirer_code` VARCHAR(10) NULL COMMENT '카드 매입사 코드',
    `interest_payer` VARCHAR(20) NULL COMMENT '무이자 할부 부담자 (BUYER/CARD_COMPANY/MERCHANT)',
    `use_card_point` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '카드 포인트 사용 여부',
    `amount` BIGINT UNSIGNED NOT NULL COMMENT '카드 결제 금액',

    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_card_details_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
    UNIQUE INDEX `uk_card_details_payment` (`payment_id`), -- 1:1 관계 강제
    INDEX `idx_card_details_issuer` (`issuer_code`),
    INDEX `idx_card_details_acquirer` (`acquirer_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='카드 결제 상세 정보 (PCI DSS 주의: 마스킹된 정보만 저장)';

-- Table 2-2: 가상계좌 결제 상세 정보
CREATE TABLE `payment_virtual_account_details` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `payment_id` BIGINT UNSIGNED NOT NULL COMMENT 'payments FK',

    -- 기존 필드
    `account_number` VARCHAR(50) NOT NULL COMMENT '가상계좌 번호 (민감정보: 암호화 권장)',
    `bank_code` VARCHAR(10) NOT NULL COMMENT '은행 코드',
    `customer_name` VARCHAR(100) NOT NULL COMMENT '입금자명 (PII: 암호화 권장)',
    `due_date` DATETIME NOT NULL COMMENT '입금 기한',
    `refund_status` VARCHAR(50) NULL COMMENT '환불 상태',
    `expired` BOOLEAN NOT NULL DEFAULT FALSE COMMENT '만료 여부',
    `settlement_status` VARCHAR(50) NULL COMMENT '정산 상태',

    -- 신규 추가 필드
    `account_type` VARCHAR(20) NULL COMMENT '계좌 유형 (일반/고정)',
    `refund_receive_account` TEXT NULL COMMENT '환불 받을 계좌 정보 (암호화 필수, Base64 인코딩된 암호문)',
    `secret` VARCHAR(255) NULL COMMENT '가상계좌 시크릿 (민감정보: 암호화 필수)',

    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_va_details_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
    UNIQUE INDEX `uk_va_details_payment` (`payment_id`), -- 1:1 관계 강제
    INDEX `idx_va_details_account` (`account_number`),
    INDEX `idx_va_details_due_date` (`due_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='가상계좌 결제 상세 정보 (민감정보 암호화 필수)';

-- Table 2-3: 간편결제 상세 정보
CREATE TABLE `payment_easy_pay_details` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `payment_id` BIGINT UNSIGNED NOT NULL COMMENT 'payments FK',

    `provider` VARCHAR(50) NOT NULL COMMENT '간편결제 제공자 (토스페이/카카오페이/네이버페이)',
    `amount` BIGINT UNSIGNED NOT NULL COMMENT '간편결제 금액',
    `discount_amount` BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '할인 금액',

    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_easy_pay_details_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
    UNIQUE INDEX `uk_easy_pay_details_payment` (`payment_id`), -- 1:1 관계 강제
    INDEX `idx_easy_pay_details_provider` (`provider`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='간편결제 상세 정보';

-- Table 2-4: 현금영수증 정보
CREATE TABLE `payment_cash_receipts` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `payment_id` BIGINT UNSIGNED NOT NULL COMMENT 'payments FK',

    `receipt_type` VARCHAR(20) NOT NULL COMMENT '소득공제/지출증빙',
    `receipt_key` VARCHAR(255) NOT NULL COMMENT '현금영수증 키',
    `issue_number` VARCHAR(50) NOT NULL COMMENT '발급 번호',
    `receipt_url` VARCHAR(500) NOT NULL COMMENT '현금영수증 URL',
    `amount` BIGINT UNSIGNED NOT NULL COMMENT '현금영수증 금액',
    `tax_free_amount` BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '비과세 금액',

    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT `fk_cash_receipt_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
    INDEX `idx_cash_receipt_payment` (`payment_id`),
    UNIQUE INDEX `uk_cash_receipt_key` (`receipt_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='현금영수증 정보 (1:N 관계 허용)';

-- -------------------------------------------------------------------------
-- Phase 3: 거래 히스토리 테이블 (Codex 권장사항)
-- -------------------------------------------------------------------------

-- Table 3-1: 결제 거래 이벤트 로그
CREATE TABLE `payment_transactions` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `payment_id` BIGINT UNSIGNED NOT NULL COMMENT 'payments FK',

    `transaction_key` VARCHAR(255) NOT NULL COMMENT '거래 키 (토스페이먼츠 제공)',
    `transaction_type` VARCHAR(50) NOT NULL COMMENT '거래 유형 (PAYMENT, CANCEL, PARTIAL_CANCEL)',
    `amount` BIGINT UNSIGNED NOT NULL COMMENT '거래 금액',
    `balance_amount` BIGINT UNSIGNED NULL COMMENT '잔액 (부분 취소 후 잔여 금액)',
    `tax_free_amount` BIGINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '비과세 금액',
    `currency` CHAR(3) NOT NULL DEFAULT 'KRW' COMMENT '통화 코드 (ISO-4217)',
    `status` VARCHAR(50) NOT NULL COMMENT '거래 상태 (DONE, FAILED, PENDING)',
    `reason` TEXT NULL COMMENT '거래 사유 (취소 시 필수)',
    `toss_response` TEXT NULL COMMENT '토스 API 전체 응답 (암호화 필수, Base64 인코딩된 암호문)',

    `event_at` TIMESTAMP NULL COMMENT '토스 이벤트 발생 시각 (API 제공)',
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT '저장 시각 (UTC)',

    CONSTRAINT `fk_payment_txn_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
    UNIQUE INDEX `uk_payment_txn_key` (`transaction_key`), -- 거래 키 유니크
    INDEX `idx_payment_txn_type` (`transaction_type`),
    INDEX `idx_payment_txn_payment_created` (`payment_id`, `created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='결제 거래 히스토리 (승인/취소/부분취소 모든 이벤트 추적)';

-- -------------------------------------------------------------------------
-- 마이그레이션 완료 검증 쿼리
-- -------------------------------------------------------------------------

-- 추가된 컬럼 확인
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT,
    COLUMN_COMMENT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'payments'
  AND COLUMN_NAME IN ('use_escrow', 'is_partial_cancelable', 'payment_type', 'merchant_id', 'country')
ORDER BY ORDINAL_POSITION;

-- 유니크 인덱스 확인
SELECT
    TABLE_NAME,
    INDEX_NAME,
    NON_UNIQUE,
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX) AS COLUMNS
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'payments'
  AND INDEX_NAME LIKE 'uk_%'
GROUP BY TABLE_NAME, INDEX_NAME, NON_UNIQUE;

-- 생성된 테이블 확인
SELECT
    TABLE_NAME,
    TABLE_COMMENT,
    CREATE_TIME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN (
      'payment_card_details',
      'payment_virtual_account_details',
      'payment_easy_pay_details',
      'payment_cash_receipts',
      'payment_transactions'
  )
ORDER BY TABLE_NAME;

-- =========================================================================
-- 보안 및 준수 사항 (중요!)
-- =========================================================================
--
-- 1. PCI DSS 준수:
--    - card_number: 반드시 마스킹된 값만 저장 (예: 1234-****-****-5678)
--    - CVV, 전체 PAN 절대 저장 금지
--
-- 2. 민감 정보 암호화 필수:
--    - secret (가상계좌)
--    - refund_receive_account
--    - account_number
--    - customer_name
--    → 애플리케이션 레벨에서 AES-256-GCM 암호화 후 저장
--
-- 3. 접근 제어:
--    - 민감 테이블/컬럼에 대한 접근 로그 감사
--    - 최소 권한 원칙 적용
--
-- 4. 데이터 보존:
--    - ON DELETE RESTRICT: 하드 딜리트 방지
--    - 소프트 딜리트 권장 (deleted_at 컬럼 추가 고려)
--
-- 5. GDPR/개인정보보호법:
--    - customer_name, account_number는 PII
--    - 데이터 보존 기간 정책 수립 필요
--    - 사용자 요청 시 데이터 삭제/익명화 절차 마련
--
-- =========================================================================
