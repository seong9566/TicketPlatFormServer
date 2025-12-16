-- ============================================
-- 개선된 티켓 플랫폼 데이터베이스 스키마
-- 버전: 2.0
-- 최종 수정일: 2025-12-16
-- ============================================

-- ============================================
-- 1. 코드성 테이블 (Code Tables)
-- 수동으로 ID를 관리하는 마스터 데이터
-- ============================================

-- 관리자 액션 유형 (예: 차단, 삭제, 승인 등)
CREATE TABLE admin_action_types (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL COMMENT '한글 표시명',
    is_active  TINYINT(1) DEFAULT 1 NOT NULL COMMENT '활성화 여부',
    sort_order INT        DEFAULT 0 NOT NULL COMMENT '정렬 순서',
    CONSTRAINT uq_admin_action_types_code UNIQUE (code)
) COMMENT '관리자 액션 유형 코드 테이블';

-- 관리자 대상 유형 (예: 사용자, 티켓, 거래 등)
CREATE TABLE admin_target_types (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL COMMENT '한글 표시명',
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_admin_target_types_code UNIQUE (code)
) COMMENT '관리자 작업 대상 유형 코드 테이블';

-- 인증 제공자 (예: email, kakao, google, apple 등)
CREATE TABLE auth_providers (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(32)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_auth_providers_code UNIQUE (code)
) COMMENT '인증 제공자 코드 테이블';

-- 사용자 역할 (예: guest, user, admin 등)
CREATE TABLE auth_roles (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(32)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_auth_roles_code UNIQUE (code)
) COMMENT '사용자 역할 코드 테이블';

-- 채팅방 상태 (예: active, locked, closed 등)
CREATE TABLE chat_room_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(16)          NOT NULL,
    name_ko    VARCHAR(32)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_chat_room_statuses_code UNIQUE (code)
) COMMENT '채팅방 상태 코드 테이블';

-- 분쟁 상태 (예: pending, in_review, resolved, rejected 등)
CREATE TABLE dispute_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_dispute_statuses_code UNIQUE (code)
) COMMENT '분쟁 상태 코드 테이블';

-- 분쟁 유형 (예: fake_ticket, no_delivery, wrong_seat 등)
CREATE TABLE dispute_types (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_dispute_types_code UNIQUE (code)
) COMMENT '분쟁 유형 코드 테이블';

-- 에스크로 상태 (예: holding, released, refunded 등)
CREATE TABLE escrow_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_escrow_statuses_code UNIQUE (code)
) COMMENT '에스크로 상태 코드 테이블';

-- 알림 플랫폼 (예: ios, android, web 등)
CREATE TABLE notification_platforms (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(16)          NOT NULL,
    name_ko    VARCHAR(32)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_notification_platforms_code UNIQUE (code)
) COMMENT '알림 플랫폼 코드 테이블';

-- 알림 유형 (예: chat_message, transaction_update, system 등)
CREATE TABLE notification_types (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(64)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_notification_types_code UNIQUE (code)
) COMMENT '알림 유형 코드 테이블';

-- 결제 수단 (예: card, virtual_account, kakao_pay 등)
CREATE TABLE payment_methods (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_payment_methods_code UNIQUE (code)
) COMMENT '결제 수단 코드 테이블';

-- 결제 상태 (예: pending, completed, failed, cancelled 등)
CREATE TABLE payment_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_payment_statuses_code UNIQUE (code)
) COMMENT '결제 상태 코드 테이블';

-- 환불 사유 (예: buyer_request, seller_cancel, dispute_resolved 등)
CREATE TABLE refund_reasons (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(64)          NOT NULL,
    name_ko    VARCHAR(128)         NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_refund_reasons_code UNIQUE (code)
) COMMENT '환불 사유 코드 테이블';

-- 환불 상태 (예: pending, approved, rejected, completed 등)
CREATE TABLE refund_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_refund_statuses_code UNIQUE (code)
) COMMENT '환불 상태 코드 테이블';

-- 평판 평가 유형 (예: as_buyer, as_seller 등)
CREATE TABLE reputation_rating_types (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(16)          NOT NULL,
    name_ko    VARCHAR(32)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_reputation_rating_types_code UNIQUE (code)
) COMMENT '평판 평가 유형 코드 테이블';

-- 정산 상태 (예: pending, processing, completed, failed 등)
CREATE TABLE settlement_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_settlement_statuses_code UNIQUE (code)
) COMMENT '정산 상태 코드 테이블';

-- 티켓 카테고리 (예: concert, sports, theater, exhibition 등)
CREATE TABLE ticket_category (
    id         BIGINT AUTO_INCREMENT PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(32)          NOT NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_ticket_category_code UNIQUE (code)
) COMMENT '티켓 카테고리 코드 테이블';

-- 티켓 상태 (예: available, reserved, sold, expired 등)
CREATE TABLE ticket_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_ticket_statuses_code UNIQUE (code)
) COMMENT '티켓 상태 코드 테이블';

-- 티켓 검증 방법 (예: ocr, qr_scan, manual 등)
CREATE TABLE ticket_verification_methods (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_ticket_verification_methods_code UNIQUE (code)
) COMMENT '티켓 검증 방법 코드 테이블';

-- 거래 확인자 유형 (예: buyer, seller, system, admin 등)
CREATE TABLE transaction_confirmed_bys (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_transaction_confirmed_bys_code UNIQUE (code)
) COMMENT '거래 확인자 유형 코드 테이블';

-- 거래 상태 (예: pending, paid, confirmed, completed, cancelled 등)
CREATE TABLE transaction_statuses (
    id         BIGINT               NOT NULL PRIMARY KEY,
    code       VARCHAR(32)          NOT NULL,
    name_ko    VARCHAR(64)          NULL,
    is_active  TINYINT(1) DEFAULT 1 NOT NULL,
    sort_order INT        DEFAULT 0 NOT NULL,
    CONSTRAINT uq_transaction_statuses_code UNIQUE (code)
) COMMENT '거래 상태 코드 테이블';


-- ============================================
-- 2. 사용자 관련 테이블 (User Tables)
-- ============================================

-- 사용자 기본 정보
CREATE TABLE users (
    id            BIGINT AUTO_INCREMENT PRIMARY KEY,
    email         VARCHAR(255)                         NOT NULL COMMENT '이메일 (로그인 ID)',
    password_hash VARCHAR(255)                         NULL COMMENT '비밀번호 해시 (소셜 로그인 시 NULL)',
    phone         VARCHAR(20)                          NULL COMMENT '연락처',
    provider_id   BIGINT     DEFAULT 1                 NOT NULL COMMENT '인증 제공자 FK',
    role_id       BIGINT     DEFAULT 1                 NOT NULL COMMENT '사용자 역할 FK',
    created_at    TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL,
    last_login_at TIMESTAMP                            NULL COMMENT '마지막 로그인 시각',
    is_deleted    TINYINT(1) DEFAULT 0                 NULL COMMENT '탈퇴 여부 (Soft Delete)',
    
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT fk_users_provider FOREIGN KEY (provider_id) REFERENCES auth_providers (id),
    CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES auth_roles (id)
) COMMENT '사용자 기본 정보 테이블';

-- 사용자 인덱스
CREATE INDEX idx_users_email ON users (email);
CREATE INDEX idx_users_deleted ON users (is_deleted);
CREATE INDEX idx_users_provider_id ON users (provider_id);
CREATE INDEX idx_users_role_id ON users (role_id);

-- 사용자 프로필
CREATE TABLE user_profile (
    user_id           BIGINT          NOT NULL PRIMARY KEY,
    nickname          VARCHAR(50)     NOT NULL COMMENT '닉네임',
    profile_image_url VARCHAR(500)    NULL COMMENT '프로필 이미지 URL',
    bio               TEXT            NULL COMMENT '자기소개',
    buyer_rating      FLOAT DEFAULT 0 NULL COMMENT '구매자 평점',
    buyer_trade_count INT   DEFAULT 0 NULL COMMENT '구매 거래 횟수',
    
    CONSTRAINT fk_user_profile_user FOREIGN KEY (user_id) REFERENCES users (id)
) COMMENT '사용자 프로필 테이블';

CREATE INDEX idx_user_profile_nickname ON user_profile (nickname);

-- 사용자 본인 인증 정보
CREATE TABLE user_verification (
    user_id           BIGINT               NOT NULL PRIMARY KEY,
    name              VARCHAR(50)          NULL COMMENT '실명',
    birth             DATE                 NULL COMMENT '생년월일',
    identity_verified TINYINT(1) DEFAULT 0 NULL COMMENT '본인 인증 완료',
    phone_verified    TINYINT(1) DEFAULT 0 NULL COMMENT '휴대폰 인증 완료',
    account_verified  TINYINT(1) DEFAULT 0 NULL COMMENT '계좌 인증 완료',
    verified_at       TIMESTAMP            NULL COMMENT '인증 완료 시각',
    
    CONSTRAINT fk_user_verification_user FOREIGN KEY (user_id) REFERENCES users (id)
) COMMENT '사용자 본인 인증 정보 테이블';

CREATE INDEX idx_verif_identity ON user_verification (identity_verified);
CREATE INDEX idx_verif_account ON user_verification (account_verified);
CREATE INDEX idx_verif_all_verified ON user_verification (identity_verified, phone_verified, account_verified);

-- 은행 계좌 정보
CREATE TABLE bank_account (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id        BIGINT                               NOT NULL,
    bank_name      VARCHAR(100)                         NULL COMMENT '은행명',
    account_number VARCHAR(50)                          NULL COMMENT '계좌번호',
    account_holder VARCHAR(50)                          NULL COMMENT '예금주',
    verified       TINYINT(1) DEFAULT 0                 NULL COMMENT '계좌 인증 여부',
    created_at     TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_bank_account_user FOREIGN KEY (user_id) REFERENCES users (id)
) COMMENT '사용자 은행 계좌 정보 테이블';

CREATE INDEX idx_bank_user ON bank_account (user_id);
CREATE INDEX idx_bank_verified ON bank_account (user_id, verified);


-- ============================================
-- 3. 이벤트/공연 관련 테이블 (Event Tables)
-- ============================================

-- 이벤트/공연 정보
CREATE TABLE events (
    id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    category_id         BIGINT                               NOT NULL COMMENT '카테고리 FK',
    title               VARCHAR(255)                         NOT NULL COMMENT '공연/이벤트 제목',
    description         TEXT                                 NULL COMMENT '설명',
    poster_image_url    VARCHAR(500)                         NULL COMMENT '포스터 이미지 URL',
    created_by_admin_id BIGINT                               NULL COMMENT '등록 관리자 FK',
    is_active           TINYINT(1) DEFAULT 1                 NOT NULL COMMENT '활성화 여부',
    sort_order          INT        DEFAULT 0                 NOT NULL COMMENT '정렬 순서',
    created_at          TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL,
    updated_at          TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_events_category FOREIGN KEY (category_id) REFERENCES ticket_category (id),
    CONSTRAINT fk_events_admin FOREIGN KEY (created_by_admin_id) REFERENCES users (id)
) COMMENT '이벤트/공연 정보 테이블';

CREATE INDEX idx_events_category_active_sort ON events (category_id, is_active, sort_order);
CREATE INDEX idx_events_title ON events (title);

-- 이벤트 회차/세션 정보
CREATE TABLE event_sessions (
    id            BIGINT AUTO_INCREMENT PRIMARY KEY,
    event_id      BIGINT                               NOT NULL COMMENT '이벤트 FK',
    start_at      DATETIME                             NOT NULL COMMENT '시작 일시',
    end_at        DATETIME                             NULL COMMENT '종료 일시',
    venue_name    VARCHAR(255)                         NULL COMMENT '공연장 이름',
    venue_address VARCHAR(500)                         NULL COMMENT '공연장 주소',
    is_active     TINYINT(1) DEFAULT 1                 NOT NULL,
    created_at    TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT uq_event_sessions_event_start UNIQUE (event_id, start_at),
    CONSTRAINT fk_event_sessions_event FOREIGN KEY (event_id) REFERENCES events (id)
) COMMENT '이벤트 회차/세션 정보 테이블';

CREATE INDEX idx_event_sessions_event_start ON event_sessions (event_id, start_at);
-- [개선] 날짜별 세션 조회를 위한 인덱스 추가
CREATE INDEX idx_event_sessions_start_at ON event_sessions (start_at);


-- ============================================
-- 4. 티켓 관련 테이블 (Ticket Tables)
-- ============================================

-- 티켓 정보
CREATE TABLE tickets (
    id                 BIGINT AUTO_INCREMENT PRIMARY KEY,
    seller_id          BIGINT                               NOT NULL COMMENT '판매자 FK',
    event_session_id   BIGINT                               NULL COMMENT '이벤트 세션 FK',
    category_id        BIGINT                               NOT NULL COMMENT '카테고리 FK',
    title              VARCHAR(255)                         NOT NULL COMMENT '티켓 제목',
    event_datetime     DATETIME                             NOT NULL COMMENT '공연 일시',
    seat_info          VARCHAR(255)                         NULL COMMENT '좌석 정보',
    quantity           INT                                  NOT NULL COMMENT '총 수량',
    remaining_quantity INT        DEFAULT 0                 NOT NULL COMMENT '남은 수량',
    is_continuous      TINYINT(1) DEFAULT 0                 NULL COMMENT '연석 여부',
    price              INT                                  NOT NULL COMMENT '판매가',
    original_price     INT                                  NOT NULL COMMENT '정가',
    description        TEXT                                 NULL COMMENT '상세 설명',
    status_id          BIGINT     DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    created_at         TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL,
    updated_at         TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    deleted_at         TIMESTAMP                            NULL COMMENT 'Soft Delete 시각',
    
    CONSTRAINT fk_tickets_seller FOREIGN KEY (seller_id) REFERENCES users (id),
    CONSTRAINT fk_tickets_event_session FOREIGN KEY (event_session_id) REFERENCES event_sessions (id),
    CONSTRAINT fk_ticket_category FOREIGN KEY (category_id) REFERENCES ticket_category (id),
    CONSTRAINT fk_tickets_status FOREIGN KEY (status_id) REFERENCES ticket_statuses (id),
    
    -- 데이터 무결성 체크
    CONSTRAINT chk_ticket_price CHECK (price > 0),
    CONSTRAINT chk_ticket_original_price CHECK (original_price >= price),
    CONSTRAINT chk_ticket_quantity CHECK (quantity > 0),
    CONSTRAINT chk_ticket_remaining_qty CHECK (remaining_quantity >= 0 AND remaining_quantity <= quantity)
) COMMENT '티켓 정보 테이블';

CREATE INDEX idx_tickets_seller ON tickets (seller_id);
CREATE INDEX idx_tickets_event_session ON tickets (event_session_id);
CREATE INDEX idx_tickets_status ON tickets (status_id);
CREATE INDEX idx_tickets_event_date ON tickets (event_datetime);
CREATE INDEX idx_tickets_created ON tickets (created_at);
CREATE INDEX idx_tickets_not_deleted ON tickets (deleted_at);
CREATE INDEX idx_tickets_remaining_qty ON tickets (remaining_quantity);
CREATE INDEX idx_tickets_list ON tickets (status_id, event_datetime);
CREATE INDEX idx_tickets_search ON tickets (status_id, event_datetime, price);
-- [개선] 카테고리별 조회를 위한 복합 인덱스 추가
CREATE INDEX idx_tickets_category_status ON tickets (category_id, status_id);

-- 티켓 이미지
CREATE TABLE ticket_images (
    id         BIGINT AUTO_INCREMENT PRIMARY KEY,
    ticket_id  BIGINT                              NOT NULL,
    image_url  VARCHAR(500)                        NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_ticket_images_ticket FOREIGN KEY (ticket_id) REFERENCES tickets (id)
) COMMENT '티켓 이미지 테이블';

CREATE INDEX idx_ticket_img_ticket ON ticket_images (ticket_id);

-- 티켓 가격 변경 이력
CREATE TABLE ticket_price_history (
    id         BIGINT AUTO_INCREMENT PRIMARY KEY,
    ticket_id  BIGINT                              NOT NULL,
    old_price  INT                                 NOT NULL COMMENT '변경 전 가격',
    new_price  INT                                 NOT NULL COMMENT '변경 후 가격',
    reason     VARCHAR(255)                        NULL COMMENT '변경 사유',
    changed_by BIGINT                              NULL COMMENT '변경자 FK',
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_ticket_price_history_ticket FOREIGN KEY (ticket_id) REFERENCES tickets (id),
    CONSTRAINT fk_ticket_price_history_user FOREIGN KEY (changed_by) REFERENCES users (id)
) COMMENT '티켓 가격 변경 이력 테이블';

CREATE INDEX idx_ticket_price_ticket ON ticket_price_history (ticket_id);
CREATE INDEX idx_ticket_price_changed_by ON ticket_price_history (changed_by);


-- ============================================
-- 5. 거래 관련 테이블 (Transaction Tables)
-- [개선] transactions.ticket_id 제거 - transaction_items로 관리
-- ============================================

-- 거래 정보
CREATE TABLE transactions (
    id                     BIGINT AUTO_INCREMENT PRIMARY KEY,
    -- [개선] ticket_id 제거: 하나의 거래에 여러 티켓이 연결될 수 있으므로 transaction_items로 관리
    buyer_id               BIGINT                              NOT NULL COMMENT '구매자 FK',
    seller_id              BIGINT                              NOT NULL COMMENT '판매자 FK',
    status_id              BIGINT    DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    reserved_at            DATETIME                            NULL COMMENT '예약 시각',
    reservation_expires_at DATETIME                            NULL COMMENT '예약 만료 시각',
    confirmed_at           DATETIME                            NULL COMMENT '구매 확정 시각',
    auto_confirm_at        DATETIME                            NULL COMMENT '자동 확정 예정 시각',
    confirmed_by_id        BIGINT                              NULL COMMENT '확정자 유형 FK',
    cancelled_at           DATETIME                            NULL COMMENT '취소 시각',
    created_at             TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    deleted_at             TIMESTAMP                           NULL COMMENT 'Soft Delete 시각',
    
    CONSTRAINT fk_transactions_buyer FOREIGN KEY (buyer_id) REFERENCES users (id),
    CONSTRAINT fk_transactions_seller FOREIGN KEY (seller_id) REFERENCES users (id),
    CONSTRAINT fk_transactions_status FOREIGN KEY (status_id) REFERENCES transaction_statuses (id),
    CONSTRAINT fk_transactions_confirmed_by FOREIGN KEY (confirmed_by_id) REFERENCES transaction_confirmed_bys (id)
) COMMENT '거래 정보 테이블 (하나의 거래에 여러 티켓 항목 가능)';

CREATE INDEX idx_trans_buyer ON transactions (buyer_id);
CREATE INDEX idx_trans_seller ON transactions (seller_id);
CREATE INDEX idx_trans_status ON transactions (status_id);
CREATE INDEX idx_trans_created ON transactions (created_at);
CREATE INDEX idx_trans_not_deleted ON transactions (deleted_at);
CREATE INDEX idx_trans_buyer_status ON transactions (buyer_id, status_id);
CREATE INDEX idx_trans_seller_status ON transactions (seller_id, status_id);

-- 거래 항목 (티켓별 수량/가격)
CREATE TABLE transaction_items (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT                              NOT NULL COMMENT '거래 FK',
    ticket_id      BIGINT                              NOT NULL COMMENT '티켓 FK',
    quantity       INT                                 NOT NULL COMMENT '구매 수량',
    unit_price     INT                                 NOT NULL COMMENT '단가',
    total_price    INT                                 NOT NULL COMMENT '소계 (단가 × 수량)',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT uq_trans_items_trans_ticket UNIQUE (transaction_id, ticket_id),
    CONSTRAINT fk_trans_items_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_trans_items_ticket FOREIGN KEY (ticket_id) REFERENCES tickets (id),
    CONSTRAINT chk_trans_items_qty CHECK (quantity > 0),
    CONSTRAINT chk_trans_items_price CHECK (unit_price >= 0 AND total_price >= 0)
) COMMENT '거래 항목 테이블 (티켓별 구매 정보)';

CREATE INDEX idx_trans_items_trans ON transaction_items (transaction_id);
CREATE INDEX idx_trans_items_ticket ON transaction_items (ticket_id);

-- 거래 상태 변경 이력
CREATE TABLE transaction_history (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT                              NOT NULL,
    old_status     VARCHAR(50)                         NULL COMMENT '이전 상태 코드',
    new_status     VARCHAR(50)                         NULL COMMENT '새 상태 코드',
    changed_by     BIGINT                              NULL COMMENT '변경자 FK',
    changed_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_transaction_history_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id)
) COMMENT '거래 상태 변경 이력 테이블';

CREATE INDEX idx_trans_history_trans ON transaction_history (transaction_id);


-- ============================================
-- 6. 결제/정산 관련 테이블 (Payment Tables)
-- ============================================

-- 결제 정보
CREATE TABLE payments (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT           NOT NULL COMMENT '거래 FK',
    pg_provider    VARCHAR(50)      NULL COMMENT 'PG사 (예: toss, kakao)',
    payment_key    VARCHAR(255)     NULL COMMENT 'PG사 결제 키',
    order_id       VARCHAR(255)     NULL COMMENT '주문 ID',
    amount         INT              NOT NULL COMMENT '결제 금액',
    method_id      BIGINT DEFAULT 1 NOT NULL COMMENT '결제 수단 FK',
    paid_at        DATETIME         NULL COMMENT '결제 완료 시각',
    status_id      BIGINT DEFAULT 1 NOT NULL COMMENT '결제 상태 FK',
    
    CONSTRAINT fk_payments_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_payments_method FOREIGN KEY (method_id) REFERENCES payment_methods (id),
    CONSTRAINT fk_payments_status FOREIGN KEY (status_id) REFERENCES payment_statuses (id)
) COMMENT '결제 정보 테이블';

CREATE INDEX idx_payments_trans ON payments (transaction_id);
CREATE INDEX idx_payments_key ON payments (payment_key);
CREATE INDEX idx_payments_order ON payments (order_id);
CREATE INDEX idx_payments_method_id ON payments (method_id);
CREATE INDEX idx_payments_status_id ON payments (status_id);
CREATE INDEX idx_payments_trans_status ON payments (transaction_id, status_id);

-- 에스크로 (결제 대금 보관)
CREATE TABLE escrow (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT                              NOT NULL COMMENT '거래 FK (1:1)',
    amount         INT                                 NOT NULL COMMENT '총 금액',
    fee_amount     INT       DEFAULT 0                 NOT NULL COMMENT '수수료',
    seller_amount  INT                                 NOT NULL COMMENT '판매자 정산 금액',
    status_id      BIGINT    DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    released_at    DATETIME                            NULL COMMENT '정산 완료 시각',
    refunded_at    DATETIME                            NULL COMMENT '환불 완료 시각',
    updated_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT uq_escrow_transaction UNIQUE (transaction_id),
    CONSTRAINT fk_escrow_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_escrow_status FOREIGN KEY (status_id) REFERENCES escrow_statuses (id),
    CONSTRAINT chk_escrow_amounts CHECK (amount = (fee_amount + seller_amount))
) COMMENT '에스크로 (결제 대금 보관) 테이블';

CREATE INDEX idx_escrow_status_id ON escrow (status_id);

-- 환불 정보
CREATE TABLE refunds (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT                              NOT NULL,
    payment_id     BIGINT                              NOT NULL COMMENT '결제 FK',
    amount         INT                                 NOT NULL COMMENT '환불 금액',
    reason_id      BIGINT    DEFAULT 1                 NOT NULL COMMENT '환불 사유 FK',
    status_id      BIGINT    DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    requested_by   BIGINT                              NOT NULL COMMENT '요청자 FK',
    approved_by    BIGINT                              NULL COMMENT '승인자 FK',
    processed_at   DATETIME                            NULL COMMENT '처리 완료 시각',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_refunds_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_refunds_payment FOREIGN KEY (payment_id) REFERENCES payments (id),
    CONSTRAINT fk_refunds_reason FOREIGN KEY (reason_id) REFERENCES refund_reasons (id),
    CONSTRAINT fk_refunds_status FOREIGN KEY (status_id) REFERENCES refund_statuses (id),
    CONSTRAINT fk_refunds_requested_by FOREIGN KEY (requested_by) REFERENCES users (id)
) COMMENT '환불 정보 테이블';

CREATE INDEX idx_refunds_trans ON refunds (transaction_id);
CREATE INDEX idx_refunds_payment ON refunds (payment_id);
CREATE INDEX idx_refunds_reason_id ON refunds (reason_id);
CREATE INDEX idx_refunds_status_id ON refunds (status_id);
CREATE INDEX idx_refunds_requested_by ON refunds (requested_by);
CREATE INDEX idx_refunds_trans_status ON refunds (transaction_id, status_id);

-- 정산 정보 (판매자에게 정산)
CREATE TABLE settlements (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id  BIGINT                              NOT NULL,
    seller_id       BIGINT                              NOT NULL COMMENT '판매자 FK',
    amount          INT                                 NOT NULL COMMENT '총 금액',
    fee             INT                                 NOT NULL COMMENT '수수료',
    net_amount      INT                                 NOT NULL COMMENT '순 정산 금액',
    bank_account_id BIGINT                              NOT NULL COMMENT '정산 계좌 FK',
    status_id       BIGINT    DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    scheduled_at    DATETIME                            NOT NULL COMMENT '정산 예정 일시',
    processed_at    DATETIME                            NULL COMMENT '정산 완료 시각',
    failure_reason  TEXT                                NULL COMMENT '실패 사유',
    retry_count     INT       DEFAULT 0                 NULL COMMENT '재시도 횟수',
    created_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    updated_at      TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL ON UPDATE CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_settlements_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_settlements_seller FOREIGN KEY (seller_id) REFERENCES users (id),
    CONSTRAINT fk_settlements_bank FOREIGN KEY (bank_account_id) REFERENCES bank_account (id),
    CONSTRAINT fk_settlements_status FOREIGN KEY (status_id) REFERENCES settlement_statuses (id),
    CONSTRAINT chk_settlement_amounts CHECK (amount = (fee + net_amount)),
    CONSTRAINT chk_settlement_retry CHECK (retry_count >= 0 AND retry_count <= 5)
) COMMENT '정산 정보 테이블';

CREATE INDEX idx_settlements_trans ON settlements (transaction_id);
CREATE INDEX idx_settlements_seller ON settlements (seller_id);
CREATE INDEX idx_settlements_bank ON settlements (bank_account_id);
CREATE INDEX idx_settlements_status ON settlements (status_id);
CREATE INDEX idx_settlements_scheduled ON settlements (scheduled_at);
CREATE INDEX idx_settlements_status_scheduled ON settlements (status_id, scheduled_at);
CREATE INDEX idx_settlements_failed ON settlements (status_id, retry_count, scheduled_at);


-- ============================================
-- 7. 채팅 관련 테이블 (Chat Tables)
-- ============================================

-- 채팅방
CREATE TABLE chat_rooms (
    id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    ticket_id           BIGINT                              NOT NULL COMMENT '티켓 FK',
    transaction_id      BIGINT                              NULL COMMENT '거래 FK (거래 성사 시)',
    buyer_id            BIGINT                              NOT NULL COMMENT '구매자 FK',
    seller_id           BIGINT                              NOT NULL COMMENT '판매자 FK',
    status_id           BIGINT    DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    last_message_at     TIMESTAMP                           NULL COMMENT '마지막 메시지 시각',
    unread_count_buyer  INT       DEFAULT 0                 NULL COMMENT '구매자 읽지 않은 수',
    unread_count_seller INT       DEFAULT 0                 NULL COMMENT '판매자 읽지 않은 수',
    locked_at           DATETIME                            NULL COMMENT '채팅 잠금 시각',
    closed_at           DATETIME                            NULL COMMENT '채팅 종료 시각',
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    deleted_at          TIMESTAMP                           NULL,
    
    CONSTRAINT uq_chat_rooms_ticket_buyer UNIQUE (ticket_id, buyer_id),
    CONSTRAINT fk_chat_rooms_ticket FOREIGN KEY (ticket_id) REFERENCES tickets (id),
    CONSTRAINT fk_chat_rooms_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_chat_rooms_buyer FOREIGN KEY (buyer_id) REFERENCES users (id),
    CONSTRAINT fk_chat_rooms_seller FOREIGN KEY (seller_id) REFERENCES users (id),
    CONSTRAINT fk_chat_rooms_status FOREIGN KEY (status_id) REFERENCES chat_room_statuses (id)
) COMMENT '채팅방 테이블';

CREATE INDEX idx_chat_ticket_buyer ON chat_rooms (ticket_id, buyer_id);
CREATE INDEX idx_chat_seller ON chat_rooms (seller_id);
CREATE INDEX idx_chat_transaction ON chat_rooms (transaction_id);
CREATE INDEX idx_chat_status_id ON chat_rooms (status_id);
CREATE INDEX idx_chat_not_deleted ON chat_rooms (deleted_at);
CREATE INDEX idx_chat_buyer_status ON chat_rooms (buyer_id, status_id);
CREATE INDEX idx_chat_buyer_last_msg ON chat_rooms (buyer_id ASC, last_message_at DESC);
CREATE INDEX idx_chat_seller_last_msg ON chat_rooms (seller_id ASC, last_message_at DESC);

-- 채팅 메시지
CREATE TABLE chat_messages (
    id         BIGINT AUTO_INCREMENT PRIMARY KEY,
    room_id    BIGINT                              NOT NULL COMMENT '채팅방 FK',
    sender_id  BIGINT                              NOT NULL COMMENT '발신자 FK',
    message    TEXT                                NULL COMMENT '메시지 내용',
    image_url  VARCHAR(500)                        NULL COMMENT '이미지 URL',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_chat_messages_room FOREIGN KEY (room_id) REFERENCES chat_rooms (id),
    CONSTRAINT fk_chat_messages_sender FOREIGN KEY (sender_id) REFERENCES users (id)
) COMMENT '채팅 메시지 테이블';

CREATE INDEX idx_msg_room ON chat_messages (room_id);
CREATE INDEX idx_msg_room_created ON chat_messages (room_id, created_at);
CREATE INDEX idx_msg_created ON chat_messages (created_at);
-- [개선] 발신자별 메시지 조회를 위한 복합 인덱스
CREATE INDEX idx_msg_sender_created ON chat_messages (sender_id, created_at);


-- ============================================
-- 8. 분쟁/티켓검증 관련 테이블 (Dispute Tables)
-- ============================================

-- 분쟁
CREATE TABLE disputes (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT                              NOT NULL COMMENT '거래 FK',
    claimant_id    BIGINT                              NOT NULL COMMENT '신고자 FK',
    type_id        BIGINT    DEFAULT 4                 NOT NULL COMMENT '분쟁 유형 FK',
    description    TEXT                                NULL COMMENT '분쟁 내용',
    status_id      BIGINT    DEFAULT 1                 NOT NULL COMMENT '상태 FK',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_disputes_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_disputes_claimant FOREIGN KEY (claimant_id) REFERENCES users (id),
    CONSTRAINT fk_disputes_type FOREIGN KEY (type_id) REFERENCES dispute_types (id),
    CONSTRAINT fk_disputes_status FOREIGN KEY (status_id) REFERENCES dispute_statuses (id)
) COMMENT '분쟁 테이블';

CREATE INDEX idx_dispute_trans ON disputes (transaction_id);
CREATE INDEX idx_dispute_claimant ON disputes (claimant_id);
CREATE INDEX idx_dispute_type_id ON disputes (type_id);
CREATE INDEX idx_dispute_status ON disputes (status_id);

-- 분쟁 증거 자료
CREATE TABLE dispute_evidence (
    id         BIGINT AUTO_INCREMENT PRIMARY KEY,
    dispute_id BIGINT                              NOT NULL COMMENT '분쟁 FK',
    image_url  VARCHAR(500)                        NULL COMMENT '증거 이미지 URL',
    note       TEXT                                NULL COMMENT '설명',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_dispute_evidence_dispute FOREIGN KEY (dispute_id) REFERENCES disputes (id)
) COMMENT '분쟁 증거 자료 테이블';

CREATE INDEX idx_dispute_evidence_dispute ON dispute_evidence (dispute_id);

-- 티켓 검증
CREATE TABLE ticket_verification (
    id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    transaction_id      BIGINT       NOT NULL COMMENT '거래 FK',
    method_id           BIGINT       NOT NULL COMMENT '검증 방법 FK',
    raw_data            TEXT         NULL COMMENT 'OCR/QR 원본 데이터',
    verification_result TINYINT(1)   NULL COMMENT '검증 결과',
    verified_by         BIGINT       NULL COMMENT '검증자 FK (수동 검증 시)',
    ocr_confidence      FLOAT        NULL COMMENT 'OCR 신뢰도',
    qr_code_hash        VARCHAR(255) NULL COMMENT 'QR코드 해시',
    ticket_number       VARCHAR(100) NULL COMMENT '티켓 번호',
    verified_at         TIMESTAMP    NULL COMMENT '검증 시각',
    
    CONSTRAINT uq_ticket_verification_trans_method UNIQUE (transaction_id, method_id),
    CONSTRAINT fk_ticket_verification_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_ticket_verification_method FOREIGN KEY (method_id) REFERENCES ticket_verification_methods (id),
    CONSTRAINT fk_ticket_verification_user FOREIGN KEY (verified_by) REFERENCES users (id)
) COMMENT '티켓 검증 테이블';

CREATE INDEX idx_verify_trans ON ticket_verification (transaction_id);
CREATE INDEX idx_verify_verified_by ON ticket_verification (verified_by);


-- ============================================
-- 9. 알림 관련 테이블 (Notification Tables)
-- ============================================

-- 알림
CREATE TABLE notifications (
    id         BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id    BIGINT                               NOT NULL COMMENT '수신자 FK',
    type_id    BIGINT     DEFAULT 1                 NOT NULL COMMENT '알림 유형 FK',
    title      VARCHAR(255)                         NULL COMMENT '알림 제목',
    body       VARCHAR(500)                         NULL COMMENT '알림 내용',
    read_flag  TINYINT(1) DEFAULT 0                 NULL COMMENT '읽음 여부',
    read_at    TIMESTAMP                            NULL COMMENT '읽은 시각',
    data       JSON                                 NULL COMMENT '추가 데이터 (페이로드)',
    created_at TIMESTAMP  DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT fk_notifications_type FOREIGN KEY (type_id) REFERENCES notification_types (id)
) COMMENT '알림 테이블';

CREATE INDEX idx_noti_user ON notifications (user_id);
CREATE INDEX idx_noti_type ON notifications (type_id);
CREATE INDEX idx_noti_read ON notifications (read_flag);
CREATE INDEX idx_noti_created ON notifications (created_at);
CREATE INDEX idx_noti_user_created ON notifications (user_id, created_at);
CREATE INDEX idx_noti_user_type_created ON notifications (user_id, type_id, created_at);

-- 알림 디바이스 토큰
CREATE TABLE notification_token (
    id           BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id      BIGINT                              NOT NULL COMMENT '사용자 FK',
    device_token VARCHAR(500)                        NOT NULL COMMENT 'FCM/APNs 토큰',
    platform_id  BIGINT                              NOT NULL COMMENT '플랫폼 FK',
    created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_notification_token_user FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT fk_notification_token_platform FOREIGN KEY (platform_id) REFERENCES notification_platforms (id)
) COMMENT '알림 디바이스 토큰 테이블';

CREATE INDEX idx_notification_token_user ON notification_token (user_id);
CREATE INDEX idx_notification_token_platform_id ON notification_token (platform_id);


-- ============================================
-- 10. 평판/관리자 관련 테이블 (Reputation & Admin Tables)
-- ============================================

-- 사용자 평판 (리뷰)
CREATE TABLE user_reputation (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    user_id        BIGINT                              NOT NULL COMMENT '평가 대상 FK',
    reviewer_id    BIGINT                              NOT NULL COMMENT '평가자 FK',
    transaction_id BIGINT                              NOT NULL COMMENT '거래 FK',
    rating_type_id BIGINT                              NOT NULL COMMENT '평가 유형 FK',
    score          INT                                 NOT NULL COMMENT '점수 (1-5)',
    comment        TEXT                                NULL COMMENT '리뷰 내용',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_user_reputation_user FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT fk_user_reputation_reviewer FOREIGN KEY (reviewer_id) REFERENCES users (id),
    CONSTRAINT fk_user_reputation_trans FOREIGN KEY (transaction_id) REFERENCES transactions (id),
    CONSTRAINT fk_user_reputation_rating_type FOREIGN KEY (rating_type_id) REFERENCES reputation_rating_types (id),
    CONSTRAINT chk_score CHECK (score BETWEEN 1 AND 5)
) COMMENT '사용자 평판 (리뷰) 테이블';

CREATE INDEX idx_reputation_user ON user_reputation (user_id, created_at);
CREATE INDEX idx_reputation_reviewer ON user_reputation (reviewer_id);
CREATE INDEX idx_reputation_trans ON user_reputation (transaction_id);
CREATE INDEX idx_reputation_rating_type_id ON user_reputation (rating_type_id);

-- 관리자 액션 로그
CREATE TABLE admin_actions (
    id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    admin_id       BIGINT                              NOT NULL COMMENT '관리자 FK',
    action_type_id BIGINT                              NOT NULL COMMENT '액션 유형 FK',
    target_type_id BIGINT                              NOT NULL COMMENT '대상 유형 FK',
    target_id      BIGINT                              NOT NULL COMMENT '대상 ID',
    reason         TEXT                                NULL COMMENT '사유',
    created_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP NULL,
    
    CONSTRAINT fk_admin_actions_admin FOREIGN KEY (admin_id) REFERENCES users (id),
    CONSTRAINT fk_admin_actions_action_type FOREIGN KEY (action_type_id) REFERENCES admin_action_types (id),
    CONSTRAINT fk_admin_actions_target_type FOREIGN KEY (target_type_id) REFERENCES admin_target_types (id)
) COMMENT '관리자 액션 로그 테이블';

CREATE INDEX idx_admin_actions_admin ON admin_actions (admin_id, created_at);
CREATE INDEX idx_admin_actions_action_type_id ON admin_actions (action_type_id);
CREATE INDEX idx_admin_actions_target ON admin_actions (target_type_id, target_id);


-- ============================================
-- 11. 저장 프로시저 (Stored Procedures)
-- [개선] 동시성 제어, 에러 핸들링 강화
-- ============================================

DELIMITER //

-- 거래 생성 프로시저
-- 티켓 재고 차감 → 거래 생성 → 거래 항목 생성을 원자적으로 처리
CREATE PROCEDURE sp_create_transaction_with_item(
    IN p_ticket_id  BIGINT,
    IN p_buy_qty    INT,
    IN p_buyer_id   BIGINT,
    IN p_seller_id  BIGINT,
    IN p_status_id  BIGINT
)
BEGIN
    -- 변수 선언
    DECLARE v_tx_id BIGINT;
    DECLARE v_unit_price INT;
    DECLARE v_remaining INT;
    
    -- [개선] 예외 발생 시 롤백 핸들러
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        -- 원래 예외를 다시 던짐
        RESIGNAL;
    END;
    
    -- ========================================
    -- 입력값 검증
    -- ========================================
    IF p_ticket_id IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'INVALID_TICKET_ID: ticket_id cannot be null';
    END IF;
    
    IF p_buy_qty IS NULL OR p_buy_qty <= 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'INVALID_QUANTITY: buy_qty must be greater than 0';
    END IF;
    
    IF p_buyer_id IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'INVALID_BUYER_ID: buyer_id cannot be null';
    END IF;
    
    IF p_seller_id IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'INVALID_SELLER_ID: seller_id cannot be null';
    END IF;
    
    -- 구매자와 판매자가 동일인인지 확인
    IF p_buyer_id = p_seller_id THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'INVALID_TRANSACTION: buyer and seller cannot be the same';
    END IF;
    
    -- ========================================
    -- 트랜잭션 시작
    -- ========================================
    START TRANSACTION;
    
    -- [개선] FOR UPDATE로 락 획득하여 동시성 제어
    -- 해당 티켓 레코드에 배타적 락을 걸어 다른 트랜잭션의 동시 접근 방지
    SELECT 
        price, 
        remaining_quantity 
    INTO 
        v_unit_price, 
        v_remaining
    FROM tickets
    WHERE id = p_ticket_id
      AND deleted_at IS NULL
      AND status_id = 1  -- 판매중 상태만
    FOR UPDATE;
    
    -- 티켓이 존재하지 않거나 삭제된 경우
    IF v_unit_price IS NULL THEN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'TICKET_NOT_FOUND: ticket does not exist or is deleted';
    END IF;
    
    -- 재고 확인
    IF v_remaining < p_buy_qty THEN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'OUT_OF_STOCK: insufficient ticket quantity';
    END IF;
    
    -- ========================================
    -- 재고 차감
    -- ========================================
    UPDATE tickets
    SET remaining_quantity = remaining_quantity - p_buy_qty,
        updated_at = NOW()
    WHERE id = p_ticket_id;
    
    -- 재고가 0이 되면 상태 변경 (선택적)
    -- UPDATE tickets
    -- SET status_id = 2  -- sold_out
    -- WHERE id = p_ticket_id AND remaining_quantity = 0;
    
    -- ========================================
    -- 거래 생성 (ticket_id 없이)
    -- ========================================
    INSERT INTO transactions (
        buyer_id, 
        seller_id, 
        status_id, 
        reserved_at,
        reservation_expires_at,
        created_at
    )
    VALUES (
        p_buyer_id, 
        p_seller_id, 
        p_status_id, 
        NOW(),
        DATE_ADD(NOW(), INTERVAL 30 MINUTE),  -- 30분 후 예약 만료
        NOW()
    );
    
    SET v_tx_id = LAST_INSERT_ID();
    
    -- ========================================
    -- 거래 항목 생성
    -- ========================================
    INSERT INTO transaction_items (
        transaction_id, 
        ticket_id, 
        quantity, 
        unit_price, 
        total_price,
        created_at
    )
    VALUES (
        v_tx_id,
        p_ticket_id,
        p_buy_qty,
        v_unit_price,
        v_unit_price * p_buy_qty,
        NOW()
    );
    
    -- ========================================
    -- 트랜잭션 커밋
    -- ========================================
    COMMIT;
    
    -- 생성된 거래 ID 반환
    SELECT 
        v_tx_id AS transaction_id,
        v_unit_price AS unit_price,
        (v_unit_price * p_buy_qty) AS total_price;
        
END //


-- 예약 만료 처리 프로시저
-- 예약 시간이 지난 거래를 취소하고 재고를 복구
CREATE PROCEDURE sp_expire_reservations()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE v_tx_id BIGINT;
    DECLARE v_ticket_id BIGINT;
    DECLARE v_qty INT;
    
    -- 만료된 예약 조회 커서
    DECLARE cur CURSOR FOR
        SELECT t.id, ti.ticket_id, ti.quantity
        FROM transactions t
        INNER JOIN transaction_items ti ON t.id = ti.transaction_id
        WHERE t.status_id = 1  -- 예약중 상태
          AND t.reservation_expires_at < NOW()
          AND t.deleted_at IS NULL;
    
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;
    
    START TRANSACTION;
    
    OPEN cur;
    
    read_loop: LOOP
        FETCH cur INTO v_tx_id, v_ticket_id, v_qty;
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- 재고 복구
        UPDATE tickets
        SET remaining_quantity = remaining_quantity + v_qty,
            updated_at = NOW()
        WHERE id = v_ticket_id
          AND deleted_at IS NULL;
        
        -- 거래 상태를 취소로 변경
        UPDATE transactions
        SET status_id = 6,  -- cancelled
            cancelled_at = NOW()
        WHERE id = v_tx_id;
        
        -- 거래 이력 기록
        INSERT INTO transaction_history (transaction_id, old_status, new_status, changed_at)
        VALUES (v_tx_id, 'reserved', 'expired', NOW());
        
    END LOOP;
    
    CLOSE cur;
    
    COMMIT;
    
END //


-- 거래 취소 프로시저
-- 거래 취소 시 재고 복구
CREATE PROCEDURE sp_cancel_transaction(
    IN p_transaction_id BIGINT,
    IN p_cancelled_by BIGINT
)
BEGIN
    DECLARE v_status_id BIGINT;
    DECLARE v_ticket_id BIGINT;
    DECLARE v_qty INT;
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;
    
    -- 입력값 검증
    IF p_transaction_id IS NULL THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'INVALID_TRANSACTION_ID';
    END IF;
    
    START TRANSACTION;
    
    -- 거래 상태 확인 (FOR UPDATE)
    SELECT status_id INTO v_status_id
    FROM transactions
    WHERE id = p_transaction_id
      AND deleted_at IS NULL
    FOR UPDATE;
    
    IF v_status_id IS NULL THEN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'TRANSACTION_NOT_FOUND';
    END IF;
    
    -- 이미 취소되었거나 완료된 거래인지 확인
    IF v_status_id IN (5, 6) THEN  -- completed, cancelled
        ROLLBACK;
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = 'CANNOT_CANCEL: transaction already completed or cancelled';
    END IF;
    
    -- 거래 항목 조회 및 재고 복구
    SELECT ticket_id, quantity INTO v_ticket_id, v_qty
    FROM transaction_items
    WHERE transaction_id = p_transaction_id
    LIMIT 1;  -- 단일 항목 가정
    
    IF v_ticket_id IS NOT NULL THEN
        UPDATE tickets
        SET remaining_quantity = remaining_quantity + v_qty,
            updated_at = NOW()
        WHERE id = v_ticket_id
          AND deleted_at IS NULL;
    END IF;
    
    -- 거래 상태 업데이트
    UPDATE transactions
    SET status_id = 6,  -- cancelled
        cancelled_at = NOW()
    WHERE id = p_transaction_id;
    
    -- 이력 기록
    INSERT INTO transaction_history (transaction_id, old_status, new_status, changed_by, changed_at)
    VALUES (p_transaction_id, (SELECT code FROM transaction_statuses WHERE id = v_status_id), 'cancelled', p_cancelled_by, NOW());
    
    COMMIT;
    
    SELECT 'SUCCESS' AS result, p_transaction_id AS transaction_id;
    
END //

DELIMITER ;


-- ============================================
-- 12. 초기 코드 데이터 (Seed Data)
-- ============================================

-- 인증 제공자
INSERT INTO auth_providers (id, code, name_ko, is_active, sort_order) VALUES
(1, 'email', '이메일', 1, 1),
(2, 'kakao', '카카오', 1, 2),
(3, 'google', '구글', 1, 3),
(4, 'apple', '애플', 1, 4);

-- 사용자 역할
INSERT INTO auth_roles (id, code, name_ko, is_active, sort_order) VALUES
(1, 'guest', '게스트', 1, 1),
(2, 'user', '일반 사용자', 1, 2),
(3, 'admin', '관리자', 1, 3);

-- 티켓 상태
INSERT INTO ticket_statuses (id, code, name_ko, is_active, sort_order) VALUES
(1, 'available', '판매중', 1, 1),
(2, 'reserved', '예약중', 1, 2),
(3, 'sold_out', '품절', 1, 3),
(4, 'expired', '만료', 1, 4),
(5, 'hidden', '숨김', 1, 5);

-- 거래 상태
INSERT INTO transaction_statuses (id, code, name_ko, is_active, sort_order) VALUES
(1, 'reserved', '예약중', 1, 1),
(2, 'pending_payment', '결제대기', 1, 2),
(3, 'paid', '결제완료', 1, 3),
(4, 'confirmed', '구매확정', 1, 4),
(5, 'completed', '거래완료', 1, 5),
(6, 'cancelled', '취소됨', 1, 6),
(7, 'refunded', '환불됨', 1, 7);

-- 거래 확인자 유형
INSERT INTO transaction_confirmed_bys (id, code, name_ko, is_active, sort_order) VALUES
(1, 'buyer', '구매자', 1, 1),
(2, 'seller', '판매자', 1, 2);