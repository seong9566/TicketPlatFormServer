CREATE DATABASE TicketPlatFormDB;

use TicketPlatFormDB;

-- ======================================================
-- USERS
-- 스토리지 엔진 : InnoDB
-- MySql 기본 엔진이 InnoDB이다.
-- 장점 : 장애 복구,외래키,트랜잭션 처리 지원
-- ======================================================

CREATE TABLE users (
                       id BIGINT PRIMARY KEY AUTO_INCREMENT,
                       email VARCHAR(255) NOT NULL UNIQUE,
                       password_hash VARCHAR(255) NULL,
                       phone VARCHAR(20) NULL,
                       provider ENUM('email','google','kakao','apple') NOT NULL DEFAULT 'email',
                       role ENUM('USER','ADMIN','SELLER') NOT NULL DEFAULT 'USER',
                       created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                       last_login_at TIMESTAMP NULL,
                       is_deleted TINYINT(1) DEFAULT 0
) ENGINE=InnoDB;

-- 컬럼에 인덱스 부여
-- 역할 : 빠른 검색 속도
-- 로그인 시 이메일 검색, 회원 존재 여부, 이메일 중복 체크 여부에 사용 됌.
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_provider ON users(provider);
CREATE INDEX idx_users_deleted ON users(is_deleted);


-- User Profile
CREATE TABLE user_profile (
                              user_id BIGINT PRIMARY KEY,
                              nickname VARCHAR(50) NOT NULL,
                              profile_image_url VARCHAR(500),
                              bio TEXT, -- 자기 소개
                              buyer_rating FLOAT DEFAULT 0, -- 판매자 평가
                              buyer_trade_count INT DEFAULT 0, -- 평가된 갯수
                              FOREIGN KEY (user_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_user_profile_nickname ON user_profile(nickname);


-- User Verification
CREATE TABLE user_verification (
                                   user_id BIGINT PRIMARY KEY,
                                   name VARCHAR(50), -- 사용자 실명
                                   birth DATE, -- 생년 월 일
                                   identity_verified TINYINT(1) DEFAULT 0, -- 실명/본인 인증 여부
                                   phone_verified TINYINT(1) DEFAULT 0, -- 휴대폰 인증 여부
                                   account_verified TINYINT(1) DEFAULT 0, -- 계좌 인증 여부
                                   verified_at TIMESTAMP NULL, -- 전체 인증 완료 시점
                                   FOREIGN KEY (user_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_verif_identity ON user_verification(identity_verified);
CREATE INDEX idx_verif_account ON user_verification(account_verified);
CREATE INDEX idx_verif_all_verified ON user_verification(identity_verified, phone_verified, account_verified);


-- Bank Account (정산 계좌)
CREATE TABLE bank_account (
                              id BIGINT PRIMARY KEY AUTO_INCREMENT, -- 은행 식별 번호
                              user_id BIGINT NOT NULL,
                              bank_name VARCHAR(100), -- 은행 명
                              account_number VARCHAR(50), -- 계좌 번호
                              account_holder VARCHAR(50), -- 예금주 이름 (실명 인증 결과와 비교 후 본인 계좌인지 판별)
                              verified TINYINT(1) DEFAULT 0, -- 정산 계좌 인증 여부(ex) Toss페이먼츠 계좌 본인 인증)
                              created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                              FOREIGN KEY (user_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_bank_user ON bank_account(user_id);
CREATE INDEX idx_bank_verified ON bank_account(user_id, verified);


-- Device Tokens
CREATE TABLE notification_token (
                                    id BIGINT PRIMARY KEY AUTO_INCREMENT,
                                    user_id BIGINT NOT NULL,
                                    device_token VARCHAR(500) NOT NULL, -- FCM 토큰
                                    platform ENUM('ios','android','web') NOT NULL,
                                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                    FOREIGN KEY (user_id) REFERENCES users(id)
) ENGINE=InnoDB;


-- ======================================================
-- TICKETS
-- ======================================================

CREATE TABLE tickets (
                         id BIGINT PRIMARY KEY AUTO_INCREMENT,
                         seller_id BIGINT NOT NULL, -- 판매자 id 값
                         category ENUM('concert','musical','sports','festival','other') NOT NULL, -- 티켓 카테고리 ( 추가 가능 )
                         title VARCHAR(255) NOT NULL, -- 제목
                         event_datetime DATETIME NOT NULL, -- 공연 일시
                         seat_info VARCHAR(255), -- 좌석 정보
                         quantity INT NOT NULL, -- 판매자가 등록한 티켓의 수량
                         is_continuous TINYINT(1) DEFAULT 0, -- 연석 여부
                         price INT NOT NULL, -- 가격
                         original_price INT NOT NULL, -- 티켓 원가
                         description TEXT, -- 설명 , 특이사항
                         status ENUM('ON_SALE','RESERVED','SOLD_OUT','DELETED','EXPIRED') NOT NULL, -- 판매 상태
                         created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 생성 일자
                         updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 수정 일자
                         deleted_at TIMESTAMP NULL, -- 삭제 일자
                         FOREIGN KEY (seller_id) REFERENCES users(id),
                         CONSTRAINT chk_ticket_price CHECK (price > 0), -- 티켓의 가격이 0 원 이하는 불가능
                         CONSTRAINT chk_ticket_original_price CHECK (original_price >= price),-- 제약 조건 : 원가보다 높게 팔 수 없음
                         CONSTRAINT chk_ticket_quantity CHECK (quantity > 0) -- 판매 갯수가 0원 이하 불가능
) ENGINE=InnoDB;

CREATE INDEX idx_tickets_seller ON tickets(seller_id);
CREATE INDEX idx_tickets_status ON tickets(status);
CREATE INDEX idx_tickets_category ON tickets(category);
CREATE INDEX idx_tickets_event_date ON tickets(event_datetime);
CREATE INDEX idx_tickets_created ON tickets(created_at);
CREATE INDEX idx_tickets_list ON tickets(category, status, event_datetime);
CREATE INDEX idx_tickets_search ON tickets(status, category, event_datetime, price);
CREATE INDEX idx_tickets_not_deleted ON tickets(deleted_at);


CREATE TABLE ticket_images (
                               id BIGINT PRIMARY KEY AUTO_INCREMENT,
                               ticket_id BIGINT NOT NULL,
                               image_url VARCHAR(500) NOT NULL,
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                               FOREIGN KEY (ticket_id) REFERENCES tickets(id)
) ENGINE=InnoDB;

CREATE INDEX idx_ticket_img_ticket ON ticket_images(ticket_id);


CREATE TABLE ticket_price_history (
                                      id BIGINT PRIMARY KEY AUTO_INCREMENT,
                                      ticket_id BIGINT NOT NULL,
                                      old_price INT NOT NULL, -- 변경 전 판매 가격 ( 원본 가격이 아님 )
                                      new_price INT NOT NULL, -- 변경 후 판매 가격
                                      reason VARCHAR(255), -- 가격 변동 사유
                                      changed_by BIGINT NULL, -- 가격 변경 주체(userID 입력)
                                      changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 변경 일시
                                      FOREIGN KEY (ticket_id) REFERENCES tickets(id),
                                      FOREIGN KEY (changed_by) REFERENCES users(id)
) ENGINE=InnoDB;


-- ======================================================
-- TRANSACTIONS
-- ======================================================

CREATE TABLE transactions (
                              id BIGINT PRIMARY KEY AUTO_INCREMENT,
                              ticket_id BIGINT NOT NULL, -- 티켓 id
                              buyer_id BIGINT NOT NULL, -- 구매자 id
                              seller_id BIGINT NOT NULL, -- 판매자 id
                              status ENUM('REQUESTED','PAID','VERIFIED','COMPLETED','CANCELLED') NOT NULL, -- 판매 상태
                              reserved_at DATETIME NULL, -- 구매자의 예약한 시점
                              reservation_expires_at DATETIME NULL, -- 예약 만료 시간 (10분으로 설정 -> 구매자가 10분 지나도록 결제 하지 않으면  자동 취소 처리)
                              confirmed_at DATETIME NULL, -- 거래 완료 시점 (수동)
                              auto_confirm_at DATETIME NULL, -- 자동 거래 완료 시점 ( 사용자 미응답 방지용 ex) 티켓 전달 후 24시간이 지나도록 거래 완료 하지 않으면 완료 처리 )
                              confirmed_by ENUM('BUYER','AUTO','ADMIN') NULL, -- 거래 완료를 누가 했는지 (판매자, 자동, 관리자)
                              cancelled_at DATETIME NULL, -- 거리 취소 시점
                              created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 거래 생성 시점
                              deleted_at TIMESTAMP NULL, -- 실제 데이터를 지우는게 아닌 해당 필드로 지움 여부 판별 하기 위함
                              FOREIGN KEY (ticket_id) REFERENCES tickets(id),
                              FOREIGN KEY (buyer_id) REFERENCES users(id),
                              FOREIGN KEY (seller_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_trans_ticket ON transactions(ticket_id);
CREATE INDEX idx_trans_buyer ON transactions(buyer_id);
CREATE INDEX idx_trans_seller ON transactions(seller_id);
CREATE INDEX idx_trans_status ON transactions(status);
CREATE INDEX idx_trans_created ON transactions(created_at);
CREATE INDEX idx_trans_buyer_status ON transactions(buyer_id, status);
CREATE INDEX idx_trans_seller_status ON transactions(seller_id, status);
CREATE INDEX idx_trans_not_deleted ON transactions(deleted_at);


-- 거래 상태 변경 이력
CREATE TABLE transaction_history (
                                     id BIGINT PRIMARY KEY AUTO_INCREMENT,
                                     transaction_id BIGINT NOT NULL, -- 어떤 거래의 상태가 변경 되었는지
                                     old_status VARCHAR(50), -- 변경 전 상태 (ex) REQUEST)
                                     new_status VARCHAR(50), -- 변경 후 상태 (ex) PAID)
                                     changed_by BIGINT, -- 상태변경 유저 id
                                     changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 변경 시점
                                     FOREIGN KEY (transaction_id) REFERENCES transactions(id)
) ENGINE=InnoDB;


-- ======================================================
-- PAYMENTS
-- ======================================================

CREATE TABLE payments (
                          id BIGINT PRIMARY KEY AUTO_INCREMENT,
                          transaction_id BIGINT NOT NULL, -- 어떤 거래에 대한 결제 인지
                          pg_provider VARCHAR(50), -- PG사 이름
                          payment_key VARCHAR(255), -- 결제 트랜잭션 고유 값
                          order_id VARCHAR(255), -- 결제 요청시 생성한 주문 id
                          amount INT NOT NULL,  -- 결제 금액
                          method ENUM('card','vbank') NOT NULL, -- 결제 수단 (일반 카드 / 가상계좌)
                          paid_at DATETIME, -- 결제 완료 시간
                          status ENUM('PAID','CANCELLED','REFUNDED') NOT NULL, -- 결제 상태 완료, 취소, 환불
                          FOREIGN KEY (transaction_id) REFERENCES transactions(id)
) ENGINE=InnoDB;

CREATE INDEX idx_payments_trans ON payments(transaction_id);
CREATE INDEX idx_payments_key ON payments(payment_key);
CREATE INDEX idx_payments_order ON payments(order_id);


-- ======================================================
-- ESCROW
-- ======================================================

CREATE TABLE escrow (
                        id BIGINT PRIMARY KEY AUTO_INCREMENT,
                        transaction_id BIGINT NOT NULL, -- 어떤 거래 id
                        amount INT NOT NULL, -- 전체 결제 금액
                        fee_amount INT NOT NULL DEFAULT 0, -- 플랫폼 수수료
                        seller_amount INT NOT NULL, -- 판매자에게 지급될 금액
    -- 거래 금액을 HOLD 해두고, 인증 후 완료 되면 RELEASE(지급), 문제가 있으면 환불(REFUNDED),
    -- 정상 거래 : HOLD -> RELEASED
    -- 환불 : HOLE -> REFUND_PENDING -> REFUNDED
    -- 신고 및 환불 절차 : HOLD -> FROZEN -> (조치 후 RELEASED OR REFUNDED)
    -- FROZEN : 신고 및 분쟁이 발생 했을때 잠시 멈추는 상태
    -- HOLD -> FROZEN -> 조사 -> RELEASE 또는 REFUND_PENDING -> REFUNDED
                        status ENUM('HOLD','RELEASED','FROZEN','REFUND_PENDING','REFUNDED') NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 생성 시간
                        released_at DATETIME NULL, -- 정산 완료 시간
                        refunded_at DATETIME NULL,  -- 환불 완료 시간
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 상태 변경시 자동 업데이트
                        FOREIGN KEY (transaction_id) REFERENCES transactions(id),
                        CONSTRAINT chk_escrow_amounts CHECK (amount = fee_amount + seller_amount) -- 제약 조건 : 총 금액 = 수수료 + 판매자 정산 금액
) ENGINE=InnoDB;

ALTER TABLE escrow
    ADD UNIQUE KEY unique_transaction (transaction_id);


-- ======================================================
-- SETTLEMENT (정산)
-- ======================================================

CREATE TABLE settlements (
                             id BIGINT PRIMARY KEY AUTO_INCREMENT,
                             transaction_id BIGINT NOT NULL, -- 거래 ID
                             seller_id BIGINT NOT NULL, -- 판매자 id
                             amount INT NOT NULL, -- 총 정산 금액
                             fee INT NOT NULL, -- 플랫폼 수수료
                             net_amount INT NOT NULL, -- 판매자에게 실제 입금 되는 금액
                             bank_account_id BIGINT NOT NULL, -- 정산 받을 은행 계좌
    -- 정산 상태
    -- PENDING : 정산 예정
    -- PROCESSING : 정산 처리 중
    -- COMPLETED : 판매자 계좌로 입금 완료
    -- FAILED : 정산 실패
                             status ENUM('PENDING','PROCESSING','COMPLETED','FAILED') NOT NULL DEFAULT 'PENDING',
                             scheduled_at DATETIME NOT NULL, -- 정산 예정 시간 (보통 매일 새벽 02:00 정산)
                             processed_at DATETIME NULL, -- 실제 정산이 처리 된 시간
                             failure_reason TEXT, -- 실패 원인
                             retry_count INT DEFAULT 0,-- 정산 실패 시 재시도 횟수 (보통 3~5회 후 관리자에게 넘어감)
                             created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 정산 레코드 생성 시간
                             updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 상태 변경될 때마다 업데이트
                             FOREIGN KEY (transaction_id) REFERENCES transactions(id),
                             FOREIGN KEY (seller_id) REFERENCES users(id),
                             FOREIGN KEY (bank_account_id) REFERENCES bank_account(id),
                             CONSTRAINT chk_settlement_amounts CHECK (amount = fee + net_amount), -- 제약조건 : 총 금액 : 수수료 + 판매자 입금 금액
                             CONSTRAINT chk_settlement_retry CHECK (retry_count >= 0 AND retry_count <= 5) -- 정산 실패 무한 루프 방지용
) ENGINE=InnoDB;

CREATE INDEX idx_settlements_seller ON settlements(seller_id);
CREATE INDEX idx_settlements_status ON settlements(status);
CREATE INDEX idx_settlements_scheduled ON settlements(scheduled_at);
CREATE INDEX idx_settlements_failed ON settlements(status, retry_count, scheduled_at);


-- ======================================================
-- REFUNDS (환불 요청 테이블)
-- ======================================================

CREATE TABLE refunds (
                         id BIGINT PRIMARY KEY AUTO_INCREMENT,
                         transaction_id BIGINT NOT NULL,
                         payment_id BIGINT NOT NULL,
                         amount INT NOT NULL, -- 환불 필요한 실제 금액
    -- 환불 실패 사유
    -- VERIFICATION_FAILED : 티켓 검증 실패
    -- WRONG_TICKET : 판매자의 티켓 전달 실수
    -- EVENT_CANCELLED : 공연 자체 취소
    -- FRAUD_CLAIM : 사기 의심 또는 허위 판매
    -- BUYER_CANCEL : 구매자의 단순 변심
                         reason ENUM('VERIFICATION_FAILED','WRONG_TICKET','EVENT_CANCELLED','FRAUD_CLAIM','BUYER_CANCEL') NOT NULL, -- 환불 사유
                         status ENUM('REQUESTED','APPROVED','PROCESSING','COMPLETED','REJECTED') NOT NULL DEFAULT 'REQUESTED', -- 환불 상태
                         requested_by BIGINT NOT NULL, -- 환불 요청자 id
                         approved_by BIGINT NULL, -- 환불 승인한 사람 id
                         processed_at DATETIME NULL, -- PG사의 환불 처리 시점
                         created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                         FOREIGN KEY (transaction_id) REFERENCES transactions(id),
                         FOREIGN KEY (payment_id) REFERENCES payments(id),
                         FOREIGN KEY (requested_by) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_refunds_trans ON refunds(transaction_id);
CREATE INDEX idx_refunds_status ON refunds(status);


-- ======================================================
-- VERIFICATION ( 티켓 검증 테이블 )
-- ======================================================

CREATE TABLE ticket_verification (
                                     id BIGINT PRIMARY KEY AUTO_INCREMENT,
                                     transaction_id BIGINT NOT NULL,
                                     method ENUM('qr','ocr','number') NOT NULL, -- 티켓 검증 방식 QR, OCR, 티켓 번호
                                     raw_data TEXT, -- 검증에 사용된 원본 데이터
                                     verification_result TINYINT(1), -- 검증 성공 여부 (1 -> 유효, 0 -> 유효 하지않음)
                                     verified_by BIGINT NULL, -- 검증 수행자 ( 운영자 id, 시스템 자동검증 null , 구매자가 직접 검증-> 구매자 id)
                                     ocr_confidence FLOAT NULL, -- OCR 신뢰도 0.92 -> 92%
                                     qr_code_hash VARCHAR(255) NULL, -- QR코드 해시값 (QR원본 저장 x 해시값으로 젖아)
                                     ticket_number VARCHAR(100) NULL, -- 티켓 번호 기반 검증시
                                     verified_at TIMESTAMP NULL, -- 검증 완료 시간
                                     FOREIGN KEY (transaction_id) REFERENCES transactions(id),
                                     FOREIGN KEY (verified_by) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_verify_trans ON ticket_verification(transaction_id);

ALTER TABLE ticket_verification
    ADD UNIQUE KEY unique_transaction_method (transaction_id, method);


-- ======================================================
-- DISPUTES 신고 처리 테이블
-- ======================================================

CREATE TABLE disputes (
                          id BIGINT PRIMARY KEY AUTO_INCREMENT,
                          transaction_id BIGINT NOT NULL,
                          claimant_id BIGINT NOT NULL, -- 신고자 id
    -- 신고 유형
    -- FRAUD : 사기 의심
    -- WRONG_TICKET : 판매자 잘못된 티켓 전달
    -- MISLEAING : 잘못된 정보 ( 좌석 다름, 조건 불일치 )
    -- OTHER : 기타
                          type ENUM('FRAUD','WRONG_TICKET','MISLEAING','OTHER') NOT NULL,
                          description TEXT, -- 신고 내용 상세
                          status ENUM('OPEN','UNDER_REVIEW','APPROVED','REJECTED') NOT NULL DEFAULT 'OPEN',-- 신고 처리 상태
                          created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 신고 생성 시간
                          FOREIGN KEY (transaction_id) REFERENCES transactions(id),
                          FOREIGN KEY (claimant_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_dispute_trans ON disputes(transaction_id);
CREATE INDEX idx_dispute_status ON disputes(status);

-- 신고 증거 테이블 (관리자의 심사를 위해 필요함)
CREATE TABLE dispute_evidence (
                                  id BIGINT PRIMARY KEY AUTO_INCREMENT,
                                  dispute_id BIGINT NOT NULL,
                                  image_url VARCHAR(500), -- 증거 사진
                                  note TEXT, -- 증거자료 설명
                                  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                  FOREIGN KEY (dispute_id) REFERENCES disputes(id)
) ENGINE=InnoDB;

CREATE INDEX idx_dispute_evidence_dispute ON dispute_evidence(dispute_id);


-- ======================================================
-- CHAT ROOMS (거래 전 채팅 가능 / 티켓 기준 1:N)
-- ======================================================

CREATE TABLE chat_rooms (
                            id BIGINT PRIMARY KEY AUTO_INCREMENT,
                            ticket_id BIGINT NOT NULL,
                            transaction_id BIGINT NULL, -- 판매자가 예약 완료를 누르면 트랜잭션이 생성 됌.
                            buyer_id BIGINT NOT NULL, -- 구매자
                            seller_id BIGINT NOT NULL, -- 판매자
    -- ACTIVE : 거래 전 채팅 이용 가능
    -- LOCKED : 판매쟈의 예약 활성화로 다른 구매자들이 채팅 불가능
    -- CLOSED : 거래 완료
                            status ENUM('ACTIVE','LOCKED','CLOSED') NOT NULL DEFAULT 'ACTIVE',
                            last_message_at TIMESTAMP NULL, -- 마지막 메세지 시간 ( 목록 정렬, 푸시 알림, 채팅방 살아 있는지 판단 기준 )
                            unread_count_buyer INT DEFAULT 0, -- 각 사용자의 안읽은 메세지 수
                            unread_count_seller INT DEFAULT 0, -- ''
                            locked_at DATETIME NULL, -- STATUS의 LOCKED 시점
                            closed_at DATETIME NULL, -- STATUS의 CLOSED 시점
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- 방 생성 시점
                            deleted_at TIMESTAMP NULL, -- 채팅방 삭제 시점 (사용자의 채팅 나가기 UI 처리시 필요)
                            FOREIGN KEY (ticket_id) REFERENCES tickets(id),
                            FOREIGN KEY (buyer_id) REFERENCES users(id),
                            FOREIGN KEY (seller_id) REFERENCES users(id),
                            FOREIGN KEY (transaction_id) REFERENCES transactions(id)
) ENGINE=InnoDB;

CREATE INDEX idx_chat_ticket_buyer ON chat_rooms(ticket_id, buyer_id);
CREATE INDEX idx_chat_transaction ON chat_rooms(transaction_id);
CREATE INDEX idx_chat_seller ON chat_rooms(seller_id);
CREATE INDEX idx_chat_buyer_status ON chat_rooms(buyer_id, status);
CREATE INDEX idx_chat_buyer_last_msg ON chat_rooms(buyer_id, last_message_at DESC);
CREATE INDEX idx_chat_seller_last_msg ON chat_rooms(seller_id, last_message_at DESC);
CREATE INDEX idx_chat_not_deleted ON chat_rooms(deleted_at);

ALTER TABLE chat_rooms
    ADD UNIQUE KEY unique_ticket_buyer (ticket_id, buyer_id);


-- CHAT MESSAGES
CREATE TABLE chat_messages (
                               id BIGINT PRIMARY KEY AUTO_INCREMENT,
                               room_id BIGINT NOT NULL, -- 채팅방 id
                               sender_id BIGINT NOT NULL, -- 메시지 전송자 id 메세지가 누구에게서 왔는지 구분
                               message TEXT,
                               image_url VARCHAR(500),
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                               FOREIGN KEY (room_id) REFERENCES chat_rooms(id),
                               FOREIGN KEY (sender_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_msg_room ON chat_messages(room_id);
CREATE INDEX idx_msg_created ON chat_messages(created_at);
CREATE INDEX idx_msg_room_created ON chat_messages(room_id, created_at);


-- ======================================================
-- NOTIFICATIONS
-- ======================================================

CREATE TABLE notifications (
                               id BIGINT PRIMARY KEY AUTO_INCREMENT,
                               user_id BIGINT NOT NULL, -- 알림 받는 사용자
    -- REQUEST 구매 요청 발생
    -- PAID 결제 완료 알림
    -- VERIFY_REQUEST 티켓 검증 요청 알림
    -- CONFIRMED 거래 확정(검증 성공) 알림
    -- SETTLED 판매자 정산 완료 알림
    -- DISPUTE 분쟁이 제기됨
    -- REFUND_APPROVED 환불 승인 완료
    -- CHAT_MESSAGE 채팅방 새 메시지 알림
    -- PRICE_CHANGED 티켓 가격 변경 알림
    -- TICKET_EXPIRED 티켓 만료 및 종료 알림
                               type ENUM('REQUEST','PAID','VERIFY_REQUEST','CONFIRMED','SETTLED','DISPUTE','REFUND_APPROVED','CHAT_MESSAGE','PRICE_CHANGED','TICKET_EXPIRED') NOT NULL,
                               title VARCHAR(255),
                               body VARCHAR(500),
                               read_flag TINYINT(1) DEFAULT 0,
                               read_at TIMESTAMP NULL,
                               data JSON NULL,
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                               FOREIGN KEY (user_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_noti_user ON notifications(user_id);
CREATE INDEX idx_noti_read ON notifications(read_flag);
CREATE INDEX idx_noti_created ON notifications(created_at);
CREATE INDEX idx_noti_user_created ON notifications(user_id, created_at);


-- ======================================================
-- ADMIN ACTIONS
-- 관리자 로그 테이블
-- ======================================================

CREATE TABLE admin_actions (
                               id BIGINT PRIMARY KEY AUTO_INCREMENT,
                               admin_id BIGINT NOT NULL, -- 관리자 id
    -- DISPUTE_RESOLVE 신고 처리(승인/거절)
    -- REFUND_APPROVE 환불 승인
    -- USER_SUSPEND 유저 정지/정책 위반 조치
    -- TICKET_DELETE 티켓 삭제(위조/부적절)
                               action_type ENUM('DISPUTE_RESOLVE','REFUND_APPROVE','USER_SUSPEND','TICKET_DELETE') NOT NULL,
    -- 어떤 타입의 신고를 처리 했는지 (유저 정지, 티켓 삭제, 신고 처리,TRANSACTION : 관리자가 거래에 직접 참여)
                               target_type ENUM('USER','TICKET','TRANSACTION','DISPUTE') NOT NULL,
                               target_id BIGINT NOT NULL,
                               reason TEXT, -- 운영자의 조치 이유
                               created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                               FOREIGN KEY (admin_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_admin_actions_target ON admin_actions(target_type, target_id);
CREATE INDEX idx_admin_actions_admin ON admin_actions(admin_id, created_at);


-- ======================================================
-- USER REPUTATION (transactions 참조하므로 마지막에 생성)
-- 사용자 평판
-- ======================================================

CREATE TABLE user_reputation (
                                 id BIGINT PRIMARY KEY AUTO_INCREMENT,
                                 user_id BIGINT NOT NULL,
                                 reviewer_id BIGINT NOT NULL, -- 평가 한 사람 id
                                 transaction_id BIGINT NOT NULL, -- 어떤 거래에 대한 평가인지 id
                                 rating_type ENUM('SELLER','BUYER') NOT NULL, -- 평가 대상이 어떤 역할인가?(판매자, 구매자)
                                 score INT NOT NULL, -- 평가 점수
                                 comment TEXT, -- 리뷰 내용
                                 created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                 CONSTRAINT chk_score CHECK (score BETWEEN 1 AND 5), -- 1~5점까지만 설정 가능
                                 FOREIGN KEY (user_id) REFERENCES users(id),
                                 FOREIGN KEY (reviewer_id) REFERENCES users(id),
                                 FOREIGN KEY (transaction_id) REFERENCES transactions(id)
) ENGINE=InnoDB;

CREATE INDEX idx_reputation_user ON user_reputation(user_id, created_at);
CREATE INDEX idx_reputation_trans ON user_reputation(transaction_id);