-- ============================================
-- BIGINT to INT 마이그레이션 스크립트 (완전 버전)
-- 모든 외래 키를 제거하고 변경한 후 다시 추가
-- ============================================

SET FOREIGN_KEY_CHECKS = 0;

-- ============================================
-- 1단계: 모든 외래 키 제약 조건 제거
-- ============================================

-- users.id를 참조하는 외래 키
ALTER TABLE admin_actions DROP FOREIGN KEY IF EXISTS fk_admin_actions_admin;
ALTER TABLE bank_account DROP FOREIGN KEY IF EXISTS fk_bank_account_user;
ALTER TABLE chat_messages DROP FOREIGN KEY IF EXISTS fk_chat_messages_sender;
ALTER TABLE chat_rooms DROP FOREIGN KEY IF EXISTS fk_chat_rooms_buyer;
ALTER TABLE chat_rooms DROP FOREIGN KEY IF EXISTS fk_chat_rooms_seller;
ALTER TABLE disputes DROP FOREIGN KEY IF EXISTS fk_disputes_claimant;
ALTER TABLE notification_token DROP FOREIGN KEY IF EXISTS fk_notification_token_user;
ALTER TABLE notifications DROP FOREIGN KEY IF EXISTS fk_notifications_user;
ALTER TABLE refunds DROP FOREIGN KEY IF EXISTS fk_refunds_requested_by;
ALTER TABLE settlements DROP FOREIGN KEY IF EXISTS fk_settlements_seller;
ALTER TABLE ticket_price_history DROP FOREIGN KEY IF EXISTS fk_ticket_price_history_user;
ALTER TABLE ticket_verification DROP FOREIGN KEY IF EXISTS fk_ticket_verification_user;
ALTER TABLE transactions DROP FOREIGN KEY IF EXISTS fk_transactions_buyer;
ALTER TABLE transactions DROP FOREIGN KEY IF EXISTS fk_transactions_seller;
ALTER TABLE user_reputation DROP FOREIGN KEY IF EXISTS fk_user_reputation_reviewer;
ALTER TABLE user_reputation DROP FOREIGN KEY IF EXISTS fk_user_reputation_user;
ALTER TABLE user_profile DROP FOREIGN KEY IF EXISTS fk_user_profile_user;
ALTER TABLE user_verification DROP FOREIGN KEY IF EXISTS fk_user_verification_user;
ALTER TABLE events DROP FOREIGN KEY IF EXISTS fk_events_admin;
ALTER TABLE tickets DROP FOREIGN KEY IF EXISTS fk_tickets_seller;
ALTER TABLE artist_followers DROP FOREIGN KEY IF EXISTS artist_followers_ibfk_2;
ALTER TABLE user_favorites DROP FOREIGN KEY IF EXISTS fk_user_favorites_user;

-- tickets.id를 참조하는 외래 키
ALTER TABLE chat_rooms DROP FOREIGN KEY IF EXISTS fk_chat_rooms_ticket;
ALTER TABLE transaction_items DROP FOREIGN KEY IF EXISTS fk_trans_items_ticket;
ALTER TABLE ticket_images DROP FOREIGN KEY IF EXISTS fk_ticket_images_ticket;
ALTER TABLE ticket_price_history DROP FOREIGN KEY IF EXISTS fk_ticket_price_history_ticket;

-- 기타 외래 키
ALTER TABLE events DROP FOREIGN KEY IF EXISTS fk_events_category;
ALTER TABLE events DROP FOREIGN KEY IF EXISTS fk_events_artist;
ALTER TABLE users DROP FOREIGN KEY IF EXISTS fk_users_provider;
ALTER TABLE users DROP FOREIGN KEY IF EXISTS fk_users_role;
ALTER TABLE artists DROP FOREIGN KEY IF EXISTS artists_ibfk_1;
ALTER TABLE event_sessions DROP FOREIGN KEY IF EXISTS fk_event_sessions_event;
ALTER TABLE tickets DROP FOREIGN KEY IF EXISTS fk_tickets_event_session;
ALTER TABLE tickets DROP FOREIGN KEY IF EXISTS fk_ticket_category;
ALTER TABLE tickets DROP FOREIGN KEY IF EXISTS fk_tickets_status;
ALTER TABLE artist_followers DROP FOREIGN KEY IF EXISTS artist_followers_ibfk_1;
ALTER TABLE user_favorites DROP FOREIGN KEY IF EXISTS fk_user_favorites_type;

-- ============================================
-- 2단계: 참조되는 테이블(부모) 먼저 변경
-- ============================================
ALTER TABLE ticket_category MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE auth_providers MODIFY id INT NOT NULL;
ALTER TABLE auth_roles MODIFY id INT NOT NULL;
ALTER TABLE ticket_statuses MODIFY id INT NOT NULL;
ALTER TABLE favorite_types MODIFY id INT NOT NULL;
ALTER TABLE users MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE artists MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE events MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE event_sessions MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE tickets MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE artist_followers MODIFY id INT NOT NULL AUTO_INCREMENT;
ALTER TABLE user_favorites MODIFY id INT NOT NULL AUTO_INCREMENT;

-- ============================================
-- 3단계: 참조하는 테이블(자식) 변경
-- ============================================
ALTER TABLE users MODIFY provider_id INT NOT NULL DEFAULT 1;
ALTER TABLE users MODIFY role_id INT NOT NULL DEFAULT 1;
ALTER TABLE user_profile MODIFY user_id INT NOT NULL;
ALTER TABLE artists MODIFY category_id INT NOT NULL;
ALTER TABLE events MODIFY category_id INT NOT NULL;
ALTER TABLE events MODIFY artist_id INT DEFAULT NULL;
ALTER TABLE events MODIFY created_by_admin_id INT DEFAULT NULL;
ALTER TABLE event_sessions MODIFY event_id INT NOT NULL;
ALTER TABLE tickets MODIFY seller_id INT NOT NULL;
ALTER TABLE tickets MODIFY event_session_id INT DEFAULT NULL;
ALTER TABLE tickets MODIFY category_id INT NOT NULL;
ALTER TABLE tickets MODIFY status_id INT NOT NULL DEFAULT 1;
ALTER TABLE artist_followers MODIFY artist_id INT NOT NULL;
ALTER TABLE artist_followers MODIFY user_id INT NOT NULL;
ALTER TABLE user_favorites MODIFY user_id INT NOT NULL;
ALTER TABLE user_favorites MODIFY favorite_type_id INT NOT NULL;
ALTER TABLE user_favorites MODIFY target_id INT NOT NULL;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================
-- 4단계: 외래 키 제약 조건 다시 추가
-- ============================================
-- (필요한 경우 수동으로 추가)

