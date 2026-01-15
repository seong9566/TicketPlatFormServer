-- MySQL dump for TicketPlatFormDB
-- Generated: 2026-01-15 19:59:31
-- Database: TicketPlatFormDB
-- Tables: 53
--

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;


-- ----------------------------
-- Table structure for __EFMigrationsHistory
-- ----------------------------
DROP TABLE IF EXISTS `__EFMigrationsHistory`;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of __EFMigrationsHistory
-- ----------------------------
-- No data in __EFMigrationsHistory


-- ----------------------------
-- Table structure for admin_action_types
-- ----------------------------
DROP TABLE IF EXISTS `admin_action_types`;
CREATE TABLE `admin_action_types` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL COMMENT '한글 표시명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1' COMMENT '활성화 여부',
  `sort_order` int NOT NULL DEFAULT '0' COMMENT '정렬 순서',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_admin_action_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='관리자 액션 유형 코드 테이블';

-- ----------------------------
-- Records of admin_action_types
-- ----------------------------
-- No data in admin_action_types


-- ----------------------------
-- Table structure for admin_actions
-- ----------------------------
DROP TABLE IF EXISTS `admin_actions`;
CREATE TABLE `admin_actions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `admin_id` bigint NOT NULL COMMENT '관리자 FK',
  `action_type_id` bigint NOT NULL COMMENT '액션 유형 FK',
  `target_type_id` bigint NOT NULL COMMENT '대상 유형 FK',
  `target_id` bigint NOT NULL COMMENT '대상 ID',
  `reason` text COMMENT '사유',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_admin_actions_admin` (`admin_id`,`created_at`),
  KEY `idx_admin_actions_action_type_id` (`action_type_id`),
  KEY `idx_admin_actions_target` (`target_type_id`,`target_id`),
  CONSTRAINT `fk_admin_actions_action_type` FOREIGN KEY (`action_type_id`) REFERENCES `admin_action_types` (`id`),
  CONSTRAINT `fk_admin_actions_target_type` FOREIGN KEY (`target_type_id`) REFERENCES `admin_target_types` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='관리자 액션 로그 테이블';

-- ----------------------------
-- Records of admin_actions
-- ----------------------------
-- No data in admin_actions


-- ----------------------------
-- Table structure for admin_target_types
-- ----------------------------
DROP TABLE IF EXISTS `admin_target_types`;
CREATE TABLE `admin_target_types` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL COMMENT '한글 표시명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_admin_target_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='관리자 작업 대상 유형 코드 테이블';

-- ----------------------------
-- Records of admin_target_types
-- ----------------------------
-- No data in admin_target_types


-- ----------------------------
-- Table structure for artist_followers
-- ----------------------------
DROP TABLE IF EXISTS `artist_followers`;
CREATE TABLE `artist_followers` (
  `id` int NOT NULL AUTO_INCREMENT,
  `artist_id` int NOT NULL,
  `user_id` int NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_artist_user` (`artist_id`,`user_id`),
  KEY `idx_artist_followers_artist` (`artist_id`),
  KEY `idx_artist_followers_user` (`user_id`),
  CONSTRAINT `artist_followers_ibfk_1` FOREIGN KEY (`artist_id`) REFERENCES `artists` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of artist_followers
-- ----------------------------
-- No data in artist_followers


-- ----------------------------
-- Table structure for artists
-- ----------------------------
DROP TABLE IF EXISTS `artists`;
CREATE TABLE `artists` (
  `id` int NOT NULL AUTO_INCREMENT,
  `category_id` int NOT NULL,
  `name` varchar(100) NOT NULL,
  `profile_image_url` varchar(500) DEFAULT NULL,
  `description` text,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_artists_category` (`category_id`),
  KEY `idx_artists_name` (`name`),
  KEY `idx_artists_active` (`is_active`),
  CONSTRAINT `artists_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `ticket_category` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of artists
-- ----------------------------
INSERT INTO `artists` (`id`, `category_id`, `name`, `profile_image_url`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (1, 1, '아이유 (IU)', 'https://picsum.photos/200/300?random=1', '대한민국 대표 솔로 가수', 1, 1, '2025-12-17 16:48:35', '2025-12-17 16:54:49');
INSERT INTO `artists` (`id`, `category_id`, `name`, `profile_image_url`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (2, 1, '뉴진스 (NewJeans)', 'https://picsum.photos/200/300?random=2', 'HYBE 소속 5인조 걸그룹', 1, 2, '2025-12-17 16:48:35', '2025-12-17 16:54:49');
INSERT INTO `artists` (`id`, `category_id`, `name`, `profile_image_url`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (3, 1, '싸이 (PSY)', 'https://picsum.photos/200/300?random=3', '강남스타일로 세계를 놀라게 한 가수', 1, 3, '2025-12-17 16:48:35', '2025-12-17 16:54:49');
INSERT INTO `artists` (`id`, `category_id`, `name`, `profile_image_url`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (4, 1, '임영웅', 'https://picsum.photos/200/300?random=4', '트로트 황제, 국민 가수', 1, 4, '2025-12-17 16:48:35', '2025-12-17 16:54:49');
INSERT INTO `artists` (`id`, `category_id`, `name`, `profile_image_url`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (5, 1, '데이식스 (DAY6)', 'https://picsum.photos/200/300?random=5', 'JYP 소속 밴드', 1, 5, '2025-12-17 16:48:35', '2025-12-17 16:54:49');
INSERT INTO `artists` (`id`, `category_id`, `name`, `profile_image_url`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (6, 1, 'BTS', 'https://picsum.photos/200/300?random=6', '전세계를 사로잡은 K-POP 그룹', 1, 6, '2025-12-17 16:48:35', '2025-12-17 16:54:49');


-- ----------------------------
-- Table structure for auth_providers
-- ----------------------------
DROP TABLE IF EXISTS `auth_providers`;
CREATE TABLE `auth_providers` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_auth_providers_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='인증 제공자 코드 테이블';

-- ----------------------------
-- Records of auth_providers
-- ----------------------------
INSERT INTO `auth_providers` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'email', '이메일', 1, 1);
INSERT INTO `auth_providers` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'kakao', '카카오', 1, 2);
INSERT INTO `auth_providers` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (3, 'google', '구글', 1, 3);
INSERT INTO `auth_providers` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (4, 'apple', '애플', 1, 4);


-- ----------------------------
-- Table structure for auth_roles
-- ----------------------------
DROP TABLE IF EXISTS `auth_roles`;
CREATE TABLE `auth_roles` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_auth_roles_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 역할 코드 테이블';

-- ----------------------------
-- Records of auth_roles
-- ----------------------------
INSERT INTO `auth_roles` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'guest', '게스트', 1, 1);
INSERT INTO `auth_roles` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'user', '일반 사용자', 1, 2);
INSERT INTO `auth_roles` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (3, 'admin', '관리자', 1, 3);


-- ----------------------------
-- Table structure for bank_account
-- ----------------------------
DROP TABLE IF EXISTS `bank_account`;
CREATE TABLE `bank_account` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL,
  `bank_name` varchar(100) DEFAULT NULL COMMENT '은행명',
  `account_number` varchar(50) DEFAULT NULL COMMENT '계좌번호',
  `account_holder` varchar(50) DEFAULT NULL COMMENT '예금주',
  `verified` tinyint(1) DEFAULT '0' COMMENT '계좌 인증 여부',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_bank_user` (`user_id`),
  KEY `idx_bank_verified` (`user_id`,`verified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 은행 계좌 정보 테이블';

-- ----------------------------
-- Records of bank_account
-- ----------------------------
-- No data in bank_account


-- ----------------------------
-- Table structure for chat_messages
-- ----------------------------
DROP TABLE IF EXISTS `chat_messages`;
CREATE TABLE `chat_messages` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `room_id` bigint NOT NULL COMMENT '채팅방 FK',
  `sender_id` bigint NOT NULL COMMENT '발신자 FK',
  `message` text COMMENT '메시지 내용',
  `image_url` varchar(500) DEFAULT NULL COMMENT '이미지 URL',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_msg_room` (`room_id`),
  KEY `idx_msg_room_created` (`room_id`,`created_at`),
  KEY `idx_msg_created` (`created_at`),
  KEY `idx_msg_sender_created` (`sender_id`,`created_at`),
  CONSTRAINT `fk_chat_messages_room` FOREIGN KEY (`room_id`) REFERENCES `chat_rooms` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='채팅 메시지 테이블';

-- ----------------------------
-- Records of chat_messages
-- ----------------------------
-- No data in chat_messages


-- ----------------------------
-- Table structure for chat_room_statuses
-- ----------------------------
DROP TABLE IF EXISTS `chat_room_statuses`;
CREATE TABLE `chat_room_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(16) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_chat_room_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='채팅방 상태 코드 테이블';

-- ----------------------------
-- Records of chat_room_statuses
-- ----------------------------
-- No data in chat_room_statuses


-- ----------------------------
-- Table structure for chat_rooms
-- ----------------------------
DROP TABLE IF EXISTS `chat_rooms`;
CREATE TABLE `chat_rooms` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ticket_id` bigint NOT NULL COMMENT '티켓 FK',
  `transaction_id` bigint DEFAULT NULL COMMENT '거래 FK (거래 성사 시)',
  `buyer_id` bigint NOT NULL COMMENT '구매자 FK',
  `seller_id` bigint NOT NULL COMMENT '판매자 FK',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '상태 FK',
  `last_message_at` timestamp NULL DEFAULT NULL COMMENT '마지막 메시지 시각',
  `unread_count_buyer` int DEFAULT '0' COMMENT '구매자 읽지 않은 수',
  `unread_count_seller` int DEFAULT '0' COMMENT '판매자 읽지 않은 수',
  `locked_at` datetime DEFAULT NULL COMMENT '채팅 잠금 시각',
  `closed_at` datetime DEFAULT NULL COMMENT '채팅 종료 시각',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `deleted_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_chat_rooms_ticket_buyer` (`ticket_id`,`buyer_id`),
  KEY `idx_chat_ticket_buyer` (`ticket_id`,`buyer_id`),
  KEY `idx_chat_seller` (`seller_id`),
  KEY `idx_chat_transaction` (`transaction_id`),
  KEY `idx_chat_status_id` (`status_id`),
  KEY `idx_chat_not_deleted` (`deleted_at`),
  KEY `idx_chat_buyer_status` (`buyer_id`,`status_id`),
  KEY `idx_chat_buyer_last_msg` (`buyer_id`,`last_message_at` DESC),
  KEY `idx_chat_seller_last_msg` (`seller_id`,`last_message_at` DESC),
  CONSTRAINT `fk_chat_rooms_status` FOREIGN KEY (`status_id`) REFERENCES `chat_room_statuses` (`id`),
  CONSTRAINT `fk_chat_rooms_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='채팅방 테이블';

-- ----------------------------
-- Records of chat_rooms
-- ----------------------------
-- No data in chat_rooms


-- ----------------------------
-- Table structure for dispute_evidence
-- ----------------------------
DROP TABLE IF EXISTS `dispute_evidence`;
CREATE TABLE `dispute_evidence` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `dispute_id` bigint NOT NULL COMMENT '분쟁 FK',
  `image_url` varchar(500) DEFAULT NULL COMMENT '증거 이미지 URL',
  `note` text COMMENT '설명',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_dispute_evidence_dispute` (`dispute_id`),
  CONSTRAINT `fk_dispute_evidence_dispute` FOREIGN KEY (`dispute_id`) REFERENCES `disputes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='분쟁 증거 자료 테이블';

-- ----------------------------
-- Records of dispute_evidence
-- ----------------------------
-- No data in dispute_evidence


-- ----------------------------
-- Table structure for dispute_statuses
-- ----------------------------
DROP TABLE IF EXISTS `dispute_statuses`;
CREATE TABLE `dispute_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_dispute_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='분쟁 상태 코드 테이블';

-- ----------------------------
-- Records of dispute_statuses
-- ----------------------------
-- No data in dispute_statuses


-- ----------------------------
-- Table structure for dispute_types
-- ----------------------------
DROP TABLE IF EXISTS `dispute_types`;
CREATE TABLE `dispute_types` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_dispute_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='분쟁 유형 코드 테이블';

-- ----------------------------
-- Records of dispute_types
-- ----------------------------
-- No data in dispute_types


-- ----------------------------
-- Table structure for disputes
-- ----------------------------
DROP TABLE IF EXISTS `disputes`;
CREATE TABLE `disputes` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL COMMENT '거래 FK',
  `claimant_id` bigint NOT NULL COMMENT '신고자 FK',
  `type_id` bigint NOT NULL DEFAULT '4' COMMENT '분쟁 유형 FK',
  `description` text COMMENT '분쟁 내용',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '상태 FK',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_dispute_trans` (`transaction_id`),
  KEY `idx_dispute_claimant` (`claimant_id`),
  KEY `idx_dispute_type_id` (`type_id`),
  KEY `idx_dispute_status` (`status_id`),
  CONSTRAINT `fk_disputes_status` FOREIGN KEY (`status_id`) REFERENCES `dispute_statuses` (`id`),
  CONSTRAINT `fk_disputes_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`),
  CONSTRAINT `fk_disputes_type` FOREIGN KEY (`type_id`) REFERENCES `dispute_types` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='분쟁 테이블';

-- ----------------------------
-- Records of disputes
-- ----------------------------
-- No data in disputes


-- ----------------------------
-- Table structure for escrow
-- ----------------------------
DROP TABLE IF EXISTS `escrow`;
CREATE TABLE `escrow` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL COMMENT '거래 FK (1:1)',
  `amount` int NOT NULL COMMENT '총 금액',
  `fee_amount` int NOT NULL DEFAULT '0' COMMENT '수수료',
  `seller_amount` int NOT NULL COMMENT '판매자 정산 금액',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '상태 FK',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `released_at` datetime DEFAULT NULL COMMENT '정산 완료 시각',
  `refunded_at` datetime DEFAULT NULL COMMENT '환불 완료 시각',
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_escrow_transaction` (`transaction_id`),
  KEY `idx_escrow_status_id` (`status_id`),
  CONSTRAINT `fk_escrow_status` FOREIGN KEY (`status_id`) REFERENCES `escrow_statuses` (`id`),
  CONSTRAINT `fk_escrow_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`),
  CONSTRAINT `chk_escrow_amounts` CHECK ((`amount` = (`fee_amount` + `seller_amount`)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='에스크로 (결제 대금 보관) 테이블';

-- ----------------------------
-- Records of escrow
-- ----------------------------
-- No data in escrow


-- ----------------------------
-- Table structure for escrow_statuses
-- ----------------------------
DROP TABLE IF EXISTS `escrow_statuses`;
CREATE TABLE `escrow_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_escrow_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='에스크로 상태 코드 테이블';

-- ----------------------------
-- Records of escrow_statuses
-- ----------------------------
-- No data in escrow_statuses


-- ----------------------------
-- Table structure for event_schedules
-- ----------------------------
DROP TABLE IF EXISTS `event_schedules`;
CREATE TABLE `event_schedules` (
  `id` varchar(36) NOT NULL COMMENT '일정 ID (예: sch001)',
  `event_id` int NOT NULL COMMENT '공연 FK',
  `schedule_date` date NOT NULL COMMENT '공연 날짜',
  `schedule_time` time NOT NULL COMMENT '공연 시간',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_schedules_event` (`event_id`),
  KEY `idx_schedules_date` (`schedule_date`),
  CONSTRAINT `fk_schedules_event` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연 일정 테이블';

-- ----------------------------
-- Records of event_schedules
-- ----------------------------
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH001', 1, '2026-01-28', '19:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH002', 2, '2026-02-23', '18:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH003', 3, '2026-08-02', '17:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH004', 4, '2026-03-14', '18:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH005', 5, '2026-04-18', '19:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH006', 6, '2026-10-28', '19:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH007', 7, '2026-03-14', '14:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH008', 8, '2026-04-23', '19:30:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH009', 9, '2026-05-28', '19:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH010', 10, '2026-07-03', '19:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH011', 11, '2026-04-18', '14:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH012', 12, '2026-04-25', '18:30:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH013', 13, '2026-05-23', '19:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH014', 14, '2026-11-28', '18:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH015', 15, '2026-06-18', '20:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH016', 16, '2026-01-14', '10:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH017', 17, '2026-03-14', '10:00:00', 1, '2026-01-14 09:35:57');
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`, `created_at`) VALUES ('SCH018', 18, '2026-04-14', '10:00:00', 1, '2026-01-14 09:35:57');


-- ----------------------------
-- Table structure for events
-- ----------------------------
DROP TABLE IF EXISTS `events`;
CREATE TABLE `events` (
  `id` int NOT NULL AUTO_INCREMENT,
  `category_id` int NOT NULL,
  `artist_id` int DEFAULT NULL,
  `title` varchar(255) NOT NULL COMMENT '공연/이벤트 제목',
  `description` text COMMENT '설명',
  `poster_image_url` varchar(500) DEFAULT NULL COMMENT '포스터 이미지 URL',
  `venue_name` varchar(255) DEFAULT NULL COMMENT '장소명',
  `venue_address` varchar(500) DEFAULT NULL COMMENT '장소 주소',
  `start_at` datetime DEFAULT NULL COMMENT '공연 시작 시간',
  `end_at` datetime DEFAULT NULL COMMENT '공연 종료 시간',
  `created_by_admin_id` int DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1' COMMENT '활성화 여부',
  `sort_order` int NOT NULL DEFAULT '0' COMMENT '정렬 순서',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `fk_events_admin` (`created_by_admin_id`),
  KEY `idx_events_category_active_sort` (`category_id`,`is_active`,`sort_order`),
  KEY `idx_events_title` (`title`),
  KEY `idx_events_artist` (`artist_id`),
  CONSTRAINT `fk_events_artist` FOREIGN KEY (`artist_id`) REFERENCES `artists` (`id`),
  CONSTRAINT `fk_events_category` FOREIGN KEY (`category_id`) REFERENCES `ticket_category` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='이벤트/공연 정보 테이블';

-- ----------------------------
-- Records of events
-- ----------------------------
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (1, 1, 1, '2024 월드 투어 서울', '아이유의 2024 월드 투어 서울 공연', 'https://picsum.photos/400/600?random=1', '올림픽공원 체조경기장', '서울시 송파구 올림픽로 424', '2026-01-28 19:00:00', '2026-01-28 22:00:00', NULL, 1, 1, '2025-12-17 16:48:43', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (2, 1, 2, 'Bunnies Camp 2024', '뉴진스 팬미팅', 'https://picsum.photos/400/600?random=2', '고척스카이돔', '서울시 구로구 경인로 430', '2026-02-23 18:00:00', '2026-02-23 21:00:00', NULL, 1, 2, '2025-12-17 16:48:43', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (3, 1, 3, '흠뻑쇼 2024 - SUMMER SWAG', '싸이의 여름 물총 축제', 'https://picsum.photos/400/600?random=3', '잠실종합운동장', '서울시 송파구 올림픽로 25', '2026-08-02 17:00:00', '2026-08-02 22:00:00', NULL, 1, 3, '2025-12-17 16:48:43', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (4, 1, 4, 'IM HERO 앙코르 콘서트', '임영웅 앙코르 콘서트', 'https://picsum.photos/400/600?random=4', 'KSPO돔', '서울시 송파구 올림픽로 424', '2026-03-14 18:00:00', '2026-03-14 21:00:00', NULL, 1, 4, '2025-12-17 16:48:43', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (5, 1, 5, 'Welcome to the Show', '데이식스 콘서트', 'https://picsum.photos/400/600?random=5', '블루스퀘어 마스터카드홀', '서울시 용산구 이태원로 294', '2026-04-18 19:00:00', '2026-04-18 22:00:00', NULL, 1, 5, '2025-12-17 16:48:43', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (6, 1, 6, 'BTS Yet To Come', 'BTS 부산 콘서트', 'https://picsum.photos/400/600?random=6', '부산아시아드주경기장', '부산시 연제구 월드컵대로 344', '2026-10-28 19:00:00', '2026-10-28 22:00:00', NULL, 1, 6, '2025-12-17 16:48:43', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (7, 3, NULL, '위키드 (WICKED)', '마법의 나라 오즈에서 펼쳐지는 두 마녀의 우정 이야기', 'https://picsum.photos/400/600?random=20', '블루스퀘어 신한카드홀', '서울시 용산구 이태원로 294', '2026-03-14 14:00:00', '2026-03-14 17:00:00', NULL, 1, 1, '2025-12-18 13:02:01', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (8, 3, NULL, '지킬앤하이드', '조승우 주연의 지킬앤하이드 공연', 'https://picsum.photos/400/600?random=21', '예술의전당 오페라극장', '서울시 서초구 남부순환로 2406', '2026-04-23 19:30:00', '2026-04-23 22:30:00', NULL, 1, 2, '2025-12-18 13:02:01', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (9, 3, NULL, '엘리자벳', '오스트리아 황후 엘리자벳의 이야기', 'https://picsum.photos/400/600?random=22', '샤롯데씨어터', '서울시 송파구 잠실로 240', '2026-05-28 19:00:00', '2026-05-28 22:00:00', NULL, 1, 3, '2025-12-18 13:02:01', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (10, 3, NULL, '알라딘', '디즈니 뮤지컬 알라딘', 'https://picsum.photos/400/600?random=23', '디큐브아트센터', '서울시 구로구 경인로 662', '2026-07-03 19:00:00', '2026-07-03 21:30:00', NULL, 1, 4, '2025-12-18 13:02:01', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (11, 2, NULL, '2025 KBO 시즌 - KIA vs 두산', 'KBO 리그 정규시즌 경기', 'https://picsum.photos/400/600?random=30', '광주 기아 챔피언스필드', '광주시 북구 서림로 10', '2026-04-18 14:00:00', '2026-04-18 17:00:00', NULL, 1, 1, '2025-12-18 13:02:10', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (12, 2, NULL, '2025 KBO 시즌 - 두산 홈경기', 'KBO 리그 두산 베어스 홈경기', 'https://picsum.photos/400/600?random=31', '잠실야구장', '서울시 송파구 올림픽로 25', '2026-04-25 18:30:00', '2026-04-25 21:30:00', NULL, 1, 2, '2025-12-18 13:02:10', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (13, 2, NULL, '2025 K리그 - FC서울 홈경기', 'K리그 정규시즌 FC서울 홈경기', 'https://picsum.photos/400/600?random=32', '서울월드컵경기장', '서울시 마포구 월드컵로 240', '2026-05-23 19:00:00', '2026-05-23 21:00:00', NULL, 1, 3, '2025-12-18 13:02:10', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (14, 2, NULL, '2025 KBL - 서울 삼성 vs SK', '프로농구 정규시즌 경기', 'https://picsum.photos/400/600?random=33', '잠실실내체육관', '서울시 송파구 올림픽로 25', '2026-11-28 18:00:00', '2026-11-28 20:00:00', NULL, 1, 4, '2025-12-18 13:02:10', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (15, 2, NULL, '손흥민 친선 경기', '대한민국 vs 일본 친선경기', 'https://picsum.photos/400/600?random=34', '서울월드컵경기장', '서울시 마포구 월드컵로 240', '2026-06-18 20:00:00', '2026-06-18 22:00:00', NULL, 1, 5, '2025-12-18 13:02:10', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (16, 4, NULL, '반 고흐 인사이드', '빛의 시어터에서 만나는 반 고흐', 'https://picsum.photos/400/600?random=40', '빛의 시어터 제주', '제주시 애월읍 어음리 1942', '2026-01-14 10:00:00', '2026-07-13 20:00:00', NULL, 1, 1, '2025-12-18 13:02:19', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (17, 4, NULL, '팀랩 보더리스', '디지털 아트 뮤지엄', 'https://picsum.photos/400/600?random=41', '잠실 롯데월드타워', '서울시 송파구 올림픽로 300', '2026-03-14 10:00:00', '2027-01-13 21:00:00', NULL, 1, 2, '2025-12-18 13:02:19', '2026-01-14 14:26:08');
INSERT INTO `events` (`id`, `category_id`, `artist_id`, `title`, `description`, `poster_image_url`, `venue_name`, `venue_address`, `start_at`, `end_at`, `created_by_admin_id`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (18, 4, NULL, '모네: 빛을 그리다', '인상파 거장 모네 특별전', 'https://picsum.photos/400/600?random=42', '예술의전당 한가람미술관', '서울시 서초구 남부순환로 2406', '2026-04-14 10:00:00', '2026-08-13 19:00:00', NULL, 1, 3, '2025-12-18 13:02:19', '2026-01-14 14:26:08');


-- ----------------------------
-- Table structure for favorite_types
-- ----------------------------
DROP TABLE IF EXISTS `favorite_types`;
CREATE TABLE `favorite_types` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_favorite_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='찜 유형 코드 테이블';

-- ----------------------------
-- Records of favorite_types
-- ----------------------------
INSERT INTO `favorite_types` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'event', '공연', 1, 1);
INSERT INTO `favorite_types` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'ticket', '티켓', 1, 2);


-- ----------------------------
-- Table structure for notification_platforms
-- ----------------------------
DROP TABLE IF EXISTS `notification_platforms`;
CREATE TABLE `notification_platforms` (
  `id` bigint NOT NULL,
  `code` varchar(16) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_notification_platforms_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 플랫폼 코드 테이블';

-- ----------------------------
-- Records of notification_platforms
-- ----------------------------
-- No data in notification_platforms


-- ----------------------------
-- Table structure for notification_token
-- ----------------------------
DROP TABLE IF EXISTS `notification_token`;
CREATE TABLE `notification_token` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL COMMENT '사용자 FK',
  `device_token` varchar(500) NOT NULL COMMENT 'FCM/APNs 토큰',
  `platform_id` bigint NOT NULL COMMENT '플랫폼 FK',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_notification_token_user` (`user_id`),
  KEY `idx_notification_token_platform_id` (`platform_id`),
  CONSTRAINT `fk_notification_token_platform` FOREIGN KEY (`platform_id`) REFERENCES `notification_platforms` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 디바이스 토큰 테이블';

-- ----------------------------
-- Records of notification_token
-- ----------------------------
-- No data in notification_token


-- ----------------------------
-- Table structure for notification_types
-- ----------------------------
DROP TABLE IF EXISTS `notification_types`;
CREATE TABLE `notification_types` (
  `id` bigint NOT NULL,
  `code` varchar(64) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_notification_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 유형 코드 테이블';

-- ----------------------------
-- Records of notification_types
-- ----------------------------
-- No data in notification_types


-- ----------------------------
-- Table structure for notifications
-- ----------------------------
DROP TABLE IF EXISTS `notifications`;
CREATE TABLE `notifications` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL COMMENT '수신자 FK',
  `type_id` bigint NOT NULL DEFAULT '1' COMMENT '알림 유형 FK',
  `title` varchar(255) DEFAULT NULL COMMENT '알림 제목',
  `body` varchar(500) DEFAULT NULL COMMENT '알림 내용',
  `read_flag` tinyint(1) DEFAULT '0' COMMENT '읽음 여부',
  `read_at` timestamp NULL DEFAULT NULL COMMENT '읽은 시각',
  `data` json DEFAULT NULL COMMENT '추가 데이터 (페이로드)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_noti_user` (`user_id`),
  KEY `idx_noti_type` (`type_id`),
  KEY `idx_noti_read` (`read_flag`),
  KEY `idx_noti_created` (`created_at`),
  KEY `idx_noti_user_created` (`user_id`,`created_at`),
  KEY `idx_noti_user_type_created` (`user_id`,`type_id`,`created_at`),
  CONSTRAINT `fk_notifications_type` FOREIGN KEY (`type_id`) REFERENCES `notification_types` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 테이블';

-- ----------------------------
-- Records of notifications
-- ----------------------------
-- No data in notifications


-- ----------------------------
-- Table structure for payment_methods
-- ----------------------------
DROP TABLE IF EXISTS `payment_methods`;
CREATE TABLE `payment_methods` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_payment_methods_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 수단 코드 테이블';

-- ----------------------------
-- Records of payment_methods
-- ----------------------------
-- No data in payment_methods


-- ----------------------------
-- Table structure for payment_statuses
-- ----------------------------
DROP TABLE IF EXISTS `payment_statuses`;
CREATE TABLE `payment_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_payment_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 상태 코드 테이블';

-- ----------------------------
-- Records of payment_statuses
-- ----------------------------
-- No data in payment_statuses


-- ----------------------------
-- Table structure for payments
-- ----------------------------
DROP TABLE IF EXISTS `payments`;
CREATE TABLE `payments` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL COMMENT '거래 FK',
  `pg_provider` varchar(50) DEFAULT NULL COMMENT 'PG사 (예: toss, kakao)',
  `payment_key` varchar(255) DEFAULT NULL COMMENT 'PG사 결제 키',
  `order_id` varchar(255) DEFAULT NULL COMMENT '주문 ID',
  `amount` int NOT NULL COMMENT '결제 금액',
  `method_id` bigint NOT NULL DEFAULT '1' COMMENT '결제 수단 FK',
  `paid_at` datetime DEFAULT NULL COMMENT '결제 완료 시각',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '결제 상태 FK',
  PRIMARY KEY (`id`),
  KEY `idx_payments_trans` (`transaction_id`),
  KEY `idx_payments_key` (`payment_key`),
  KEY `idx_payments_order` (`order_id`),
  KEY `idx_payments_method_id` (`method_id`),
  KEY `idx_payments_status_id` (`status_id`),
  KEY `idx_payments_trans_status` (`transaction_id`,`status_id`),
  CONSTRAINT `fk_payments_method` FOREIGN KEY (`method_id`) REFERENCES `payment_methods` (`id`),
  CONSTRAINT `fk_payments_status` FOREIGN KEY (`status_id`) REFERENCES `payment_statuses` (`id`),
  CONSTRAINT `fk_payments_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 정보 테이블';

-- ----------------------------
-- Records of payments
-- ----------------------------
-- No data in payments


-- ----------------------------
-- Table structure for refresh_tokens
-- ----------------------------
DROP TABLE IF EXISTS `refresh_tokens`;
CREATE TABLE `refresh_tokens` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `token` varchar(500) NOT NULL,
  `expiry_date` datetime NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `is_revoked` tinyint(1) DEFAULT '0',
  `revoked_at` datetime DEFAULT NULL,
  `replaced_by_token` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `token` (`token`),
  KEY `idx_user_token` (`user_id`,`token`),
  KEY `idx_expiry` (`expiry_date`),
  CONSTRAINT `refresh_tokens_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=115 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Refresh Token 저장 테이블';

-- ----------------------------
-- Records of refresh_tokens
-- ----------------------------
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (1, 12, '0c1d7846-5ec0-4201-b8ac-d6e99e9dcd4b', '2026-01-19 00:54:21', '2026-01-12 00:54:21', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (2, 12, '5a270563-1633-4a9f-a0cf-b40eb9efc8dc', '2026-01-19 03:44:25', '2026-01-12 03:44:25', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (3, 12, '45585bc9-570d-493f-9b70-6a9c85dcb8ce', '2026-01-19 03:44:39', '2026-01-12 03:44:39', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (4, 12, '9b554031-1d72-4763-b661-f7fb000fc59f', '2026-01-19 03:46:06', '2026-01-12 03:46:06', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (5, 12, '91af2324-73d8-438f-a91e-d99de41f90bd', '2026-01-19 03:47:54', '2026-01-12 03:47:54', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (6, 12, '0f0ecf18-75c0-4c77-963c-f782650638ca', '2026-01-19 04:34:27', '2026-01-12 04:34:27', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (7, 12, 'f8a38c28-3aeb-47f4-a1e7-bb55f42b49aa', '2026-01-19 04:36:25', '2026-01-12 04:36:25', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (8, 12, '2d8f10dc-882b-43ee-9f8c-50aa975e28e8', '2026-01-19 04:36:35', '2026-01-12 04:36:35', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (9, 12, '61b1b3d3-adcc-4677-827c-94478a59a20a', '2026-01-19 04:36:49', '2026-01-12 04:36:49', 1, '2026-01-12 04:37:51', '9a1075aa-938d-4970-ba0c-b21c109a49b3');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (10, 12, '9a1075aa-938d-4970-ba0c-b21c109a49b3', '2026-01-19 04:37:51', '2026-01-12 04:37:51', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (11, 12, 'cc83b930-3491-4063-89db-c3ac369e1565', '2026-01-19 04:42:01', '2026-01-12 04:42:01', 1, '2026-01-12 04:42:17', '1c61d427-f253-4139-9399-a8e4c3497126');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (12, 12, '1c61d427-f253-4139-9399-a8e4c3497126', '2026-01-19 04:42:18', '2026-01-12 04:42:18', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (13, 12, 'ba01bdf3-915b-4e21-b1ff-bde069a69d5d', '2026-01-19 05:39:17', '2026-01-12 05:39:17', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (14, 12, '3671a52d-ae6e-4ac5-a534-65ce6ecd46f1', '2026-01-19 05:52:43', '2026-01-12 05:52:43', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (15, 12, 'de7681bb-002c-4508-9892-14a4e16ba3de', '2026-01-19 05:53:35', '2026-01-12 05:53:35', 1, '2026-01-12 06:10:19', 'c9ba8b1d-a939-4b9e-9d70-e56063d7b017');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (16, 12, 'c9ba8b1d-a939-4b9e-9d70-e56063d7b017', '2026-01-19 06:10:19', '2026-01-12 06:10:19', 1, '2026-01-12 07:07:35', '35ffa890-fa40-4d86-93e7-b60432da7f28');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (17, 12, '35ffa890-fa40-4d86-93e7-b60432da7f28', '2026-01-19 07:07:35', '2026-01-12 07:07:35', 1, '2026-01-12 07:22:54', '537f1d00-a2ac-446e-ac56-6fa5b25e27fd');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (18, 12, 'dc2009f4-9acc-4f30-8516-545c8c7b58e6', '2026-01-19 07:08:54', '2026-01-12 07:08:54', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (19, 12, '537f1d00-a2ac-446e-ac56-6fa5b25e27fd', '2026-01-19 07:22:54', '2026-01-12 07:22:54', 1, '2026-01-12 07:44:35', '5ccc72f7-afc8-4067-9353-448a9b6dc2de');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (20, 12, '8eb170cf-f30e-4f3f-a7fb-c19863d0201e', '2026-01-19 07:35:24', '2026-01-12 07:35:24', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (21, 12, '6dec184f-0652-4a3f-801d-566eb45aae46', '2026-01-19 07:41:46', '2026-01-12 07:41:46', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (22, 12, '5ccc72f7-afc8-4067-9353-448a9b6dc2de', '2026-01-19 07:44:35', '2026-01-12 07:44:35', 1, '2026-01-12 08:02:58', '4379a2bc-22f6-42d0-b6eb-40e778add5ca');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (23, 12, '4379a2bc-22f6-42d0-b6eb-40e778add5ca', '2026-01-19 08:02:58', '2026-01-12 08:02:58', 1, '2026-01-12 23:40:31', '00ea9025-6853-4f90-97a9-ed53d5a03faa');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (24, 12, '00ea9025-6853-4f90-97a9-ed53d5a03faa', '2026-01-19 23:40:31', '2026-01-12 23:40:31', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (25, 12, '8c37f718-1b97-4a8e-8e7b-b91b6ec7fb0c', '2026-01-19 23:40:31', '2026-01-12 23:40:31', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (26, 12, '8a3295b7-0cee-42ae-bec5-0bd4a5bb250f', '2026-01-19 23:48:55', '2026-01-12 23:48:55', 1, '2026-01-13 00:41:00', '3b52e462-1239-4784-8d2b-8886b81c6a78');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (27, 12, '3b52e462-1239-4784-8d2b-8886b81c6a78', '2026-01-20 00:41:00', '2026-01-13 00:41:00', 1, '2026-01-13 00:41:00', 'f399da0c-751f-4b35-bb65-f16c3a7d6f19');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (28, 12, 'f399da0c-751f-4b35-bb65-f16c3a7d6f19', '2026-01-20 00:41:00', '2026-01-13 00:41:00', 1, '2026-01-13 01:09:56', '897c2d7b-3de0-4ef2-87c4-9e8919846be8');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (29, 12, '3eb59bd8-3a21-4926-babd-03fbaecd1ab4', '2026-01-20 01:09:56', '2026-01-13 01:09:56', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (30, 12, '897c2d7b-3de0-4ef2-87c4-9e8919846be8', '2026-01-20 01:09:56', '2026-01-13 01:09:56', 1, '2026-01-13 01:34:18', '5004683e-71e3-4b9d-bc56-0c4a44346282');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (31, 12, 'a6ee1f7e-503b-4f86-9bae-c68327f0b9e2', '2026-01-20 01:34:18', '2026-01-13 01:34:18', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (32, 12, '5004683e-71e3-4b9d-bc56-0c4a44346282', '2026-01-20 01:34:18', '2026-01-13 01:34:18', 1, '2026-01-13 01:56:25', '5af12285-9e47-4096-803d-5e6ee057d3d5');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (33, 12, '7fb42627-4306-452e-bad3-47625b423b93', '2026-01-20 01:56:25', '2026-01-13 01:56:25', 1, '2026-01-13 05:40:22', '9092aed4-bcc2-4f44-91cf-2ad042258922');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (34, 12, '5af12285-9e47-4096-803d-5e6ee057d3d5', '2026-01-20 01:56:25', '2026-01-13 01:56:25', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (35, 12, '9092aed4-bcc2-4f44-91cf-2ad042258922', '2026-01-20 05:40:22', '2026-01-13 05:40:22', 1, '2026-01-13 06:37:32', '8e5df1d8-b7ea-4565-a2de-a3e63557ea57');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (36, 12, '1278f75b-b21b-4688-929d-928960d4037b', '2026-01-20 05:40:22', '2026-01-13 05:40:22', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (37, 12, '8e5df1d8-b7ea-4565-a2de-a3e63557ea57', '2026-01-20 06:37:32', '2026-01-13 06:37:32', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (38, 12, 'bd6b46ed-f211-4dbb-b6cf-e4e58177148f', '2026-01-20 06:37:32', '2026-01-13 06:37:32', 1, '2026-01-13 07:01:20', '28cfa027-5107-4302-b76a-7e3ccb3433dd');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (39, 12, '28cfa027-5107-4302-b76a-7e3ccb3433dd', '2026-01-20 07:01:20', '2026-01-13 07:01:20', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (40, 12, '1d6439b9-fb25-44d7-95a9-0757f8d16e2f', '2026-01-20 07:01:20', '2026-01-13 07:01:20', 1, '2026-01-13 10:08:32', '4a7fd49c-7e47-4be2-8d22-b71087ebdc90');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (41, 12, '4a7fd49c-7e47-4be2-8d22-b71087ebdc90', '2026-01-20 10:08:32', '2026-01-13 10:08:32', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (42, 12, 'd22435f5-00ac-4d59-87a8-db93313f03a2', '2026-01-20 10:08:32', '2026-01-13 10:08:32', 1, '2026-01-14 03:50:28', 'a0440eb5-3976-4fd6-b904-eb9620dba0a9');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (43, 12, '036c9877-e371-4700-81ca-571b2a421d20', '2026-01-21 03:50:28', '2026-01-14 03:50:28', 1, '2026-01-14 04:14:35', 'da58f4aa-c32e-45d9-a70c-ab863b48226e');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (44, 12, 'a0440eb5-3976-4fd6-b904-eb9620dba0a9', '2026-01-21 03:50:28', '2026-01-14 03:50:28', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (45, 12, '7aeaaa68-b279-49f7-8e10-816c778bffed', '2026-01-21 03:55:09', '2026-01-14 03:55:09', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (46, 12, 'ccbda0e3-9c81-408e-a853-408967f99fd7', '2026-01-21 04:14:35', '2026-01-14 04:14:35', 1, '2026-01-14 04:43:32', '7f612ee5-8df0-4792-b494-49bdafa2d70c');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (47, 12, 'da58f4aa-c32e-45d9-a70c-ab863b48226e', '2026-01-21 04:14:35', '2026-01-14 04:14:35', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (48, 12, '7d71c5d6-6f23-446e-b010-323a7d419840', '2026-01-21 04:30:48', '2026-01-14 04:30:48', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (49, 12, '7f612ee5-8df0-4792-b494-49bdafa2d70c', '2026-01-21 04:43:32', '2026-01-14 04:43:32', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (50, 12, '7e03b541-29d4-403b-8c00-baaa93104a71', '2026-01-21 04:43:32', '2026-01-14 04:43:32', 1, '2026-01-14 05:14:08', 'e1319baf-40b2-46de-8b09-65dd141386d0');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (51, 12, 'e1319baf-40b2-46de-8b09-65dd141386d0', '2026-01-21 05:14:08', '2026-01-14 05:14:08', 1, '2026-01-14 05:35:49', '00860384-54d9-483c-b56c-6ae50b78b4fa');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (52, 12, '3c077954-a109-461c-8309-3426f7d9779f', '2026-01-21 05:14:08', '2026-01-14 05:14:08', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (53, 12, '119fe39a-db8f-4358-9baf-5adec5f6d0cd', '2026-01-21 05:29:10', '2026-01-14 05:29:10', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (54, 12, '00860384-54d9-483c-b56c-6ae50b78b4fa', '2026-01-21 05:35:49', '2026-01-14 05:35:49', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (55, 12, 'd460fb31-14b0-47e2-939d-5e8f6b9637a2', '2026-01-21 05:35:49', '2026-01-14 05:35:49', 1, '2026-01-14 06:31:08', '919f8ac6-01cb-4e55-91a2-c9c35cc8c3d7');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (56, 12, '5434485c-f4fb-44a1-ae4a-309324cfb80a', '2026-01-21 06:31:08', '2026-01-14 06:31:08', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (57, 12, '919f8ac6-01cb-4e55-91a2-c9c35cc8c3d7', '2026-01-21 06:31:08', '2026-01-14 06:31:08', 1, '2026-01-15 00:20:16', 'cd27ea3f-647b-4636-861a-58d1e911c330');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (58, 12, '8f3ac0a2-8cea-40ae-9280-9eb71147cb59', '2026-01-22 00:20:16', '2026-01-15 00:20:16', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (59, 12, 'cd27ea3f-647b-4636-861a-58d1e911c330', '2026-01-22 00:20:16', '2026-01-15 00:20:16', 1, '2026-01-15 00:36:38', '64d5cecc-db76-4ab5-bfe4-b2c8f697d4ed');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (60, 12, '330baf4e-af9b-421e-92aa-913c2673e77d', '2026-01-22 00:36:38', '2026-01-15 00:36:38', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (61, 12, '64d5cecc-db76-4ab5-bfe4-b2c8f697d4ed', '2026-01-22 00:36:38', '2026-01-15 00:36:38', 1, '2026-01-15 00:55:13', 'be77ba88-8425-484a-88f3-ba0910d97cc4');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (62, 12, '93ecde18-431a-4698-aa62-1fa5681a268a', '2026-01-22 00:55:14', '2026-01-15 00:55:14', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (63, 12, 'be77ba88-8425-484a-88f3-ba0910d97cc4', '2026-01-22 00:55:14', '2026-01-15 00:55:14', 1, '2026-01-15 01:12:08', '7a07826d-22dc-40de-b13e-764a6c38894f');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (64, 12, '7a07826d-22dc-40de-b13e-764a6c38894f', '2026-01-22 01:12:08', '2026-01-15 01:12:08', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (65, 12, 'da56d313-7564-45df-8e5b-70da747e867a', '2026-01-22 01:12:08', '2026-01-15 01:12:08', 1, '2026-01-15 01:53:15', 'd0679a76-5606-4e12-bfd1-05b1280d7530');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (66, 12, '241a75bc-5df4-49b6-bbf4-c3bd2b3f8a31', '2026-01-22 01:53:15', '2026-01-15 01:53:15', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (67, 12, 'd0679a76-5606-4e12-bfd1-05b1280d7530', '2026-01-22 01:53:15', '2026-01-15 01:53:15', 1, '2026-01-15 02:22:52', '4837bb67-f3d4-433a-b823-85da463f544f');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (68, 12, 'eecb64f2-f0b6-4862-bf67-f30c15e5e4a0', '2026-01-22 02:22:52', '2026-01-15 02:22:52', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (69, 12, '4837bb67-f3d4-433a-b823-85da463f544f', '2026-01-22 02:22:52', '2026-01-15 02:22:52', 1, '2026-01-15 03:18:04', '8f77cfce-ed64-426e-b03e-bb3bd360d35d');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (70, 12, '79cb8e0a-6779-4a53-974f-3d066792462e', '2026-01-22 03:18:04', '2026-01-15 03:18:04', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (71, 12, '8f77cfce-ed64-426e-b03e-bb3bd360d35d', '2026-01-22 03:18:04', '2026-01-15 03:18:04', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (72, 12, 'a8a7b210-cde4-412b-81e5-c29c87dfa9fe', '2026-01-22 03:40:27', '2026-01-15 03:40:27', 1, '2026-01-15 04:07:44', '2e30fed6-7509-4c98-8eb0-c14201809472');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (73, 12, 'afaa60db-5668-4258-819a-37c553fdff21', '2026-01-22 04:07:44', '2026-01-15 04:07:44', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (74, 12, '2e30fed6-7509-4c98-8eb0-c14201809472', '2026-01-22 04:07:44', '2026-01-15 04:07:44', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (75, 12, 'bb221f56-fa42-4c72-ba48-ae6caf097f56', '2026-01-22 04:17:43', '2026-01-15 04:17:43', 1, '2026-01-15 05:07:36', '23b747b1-d261-474b-a772-408219516de7');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (76, 12, 'c91b6368-8e2a-46cb-9206-f48c16bdbcc2', '2026-01-22 05:07:36', '2026-01-15 05:07:36', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (77, 12, 'bc42eb5f-829b-4fa9-9b01-ade807f14bc9', '2026-01-22 05:07:36', '2026-01-15 05:07:36', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (78, 12, '23b747b1-d261-474b-a772-408219516de7', '2026-01-22 05:07:36', '2026-01-15 05:07:36', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (79, 15, 'ce400efd-5243-4ec7-86cc-801b17fde341', '2026-01-22 05:10:04', '2026-01-15 05:10:04', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (80, 12, 'c8c43997-479a-4c22-a61d-2982dcec6b65', '2026-01-22 05:10:52', '2026-01-15 05:10:52', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (81, 12, '2013c6fe-1307-4e47-b74b-5a5764f5b84a', '2026-01-22 05:11:48', '2026-01-15 05:11:48', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (82, 12, '5a7f2d76-1cdb-4abd-98b7-f3d7b698ee75', '2026-01-22 05:24:12', '2026-01-15 05:24:12', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (83, 15, '8e562d97-09b0-47f6-bcf3-590a4912ddb6', '2026-01-22 05:25:04', '2026-01-15 05:25:04', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (84, 12, '28ba62d0-268d-4b18-bae2-65bb5d5c1be9', '2026-01-22 05:28:24', '2026-01-15 05:28:24', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (85, 15, 'b6e72ec0-fa37-4637-91a1-e1ff6e72c81a', '2026-01-22 05:31:36', '2026-01-15 05:31:36', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (86, 12, 'fdf32d8e-8d10-47f0-9bbd-3626bf68c745', '2026-01-22 05:31:53', '2026-01-15 05:31:53', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (87, 12, 'ebb0b913-7171-4a8c-973c-0a0e714bdd44', '2026-01-22 05:32:14', '2026-01-15 05:32:14', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (88, 12, '13871a7f-467d-4a69-9a85-51c2610b4cbc', '2026-01-22 05:33:01', '2026-01-15 05:33:01', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (89, 12, '9015c088-2f37-4001-adfe-d65c5313c8f9', '2026-01-22 05:34:45', '2026-01-15 05:34:45', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (90, 15, '9f7b5f65-293b-4204-a2f4-e91e7aa1d658', '2026-01-22 05:34:59', '2026-01-15 05:34:59', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (91, 12, 'cea1c403-8065-4c83-b12b-11071f00482c', '2026-01-22 05:39:34', '2026-01-15 05:39:34', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (92, 15, '910c8a90-f81b-4c5b-b48b-51b71ca09acb', '2026-01-22 05:39:46', '2026-01-15 05:39:46', 1, '2026-01-15 06:14:07', '86f25868-5943-4b8e-b25e-faf534bdca7c');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (93, 15, 'd6b2905d-5156-479b-aa7b-e9bef8f28552', '2026-01-22 06:14:07', '2026-01-15 06:14:07', 1, '2026-01-15 06:52:25', '1a3a9b62-62d1-45e6-8a05-555c3596f4f1');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (94, 15, '86f25868-5943-4b8e-b25e-faf534bdca7c', '2026-01-22 06:14:07', '2026-01-15 06:14:07', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (95, 15, 'f07d4939-469d-4e88-8e16-853b87d45d18', '2026-01-22 06:14:07', '2026-01-15 06:14:07', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (96, 15, '90caac37-405c-4d53-b75e-6e2cfd5752f8', '2026-01-22 06:52:25', '2026-01-15 06:52:25', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (97, 15, '1a3a9b62-62d1-45e6-8a05-555c3596f4f1', '2026-01-22 06:52:25', '2026-01-15 06:52:25', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (98, 12, '6a42953c-bfda-4049-8be5-0af7dd633108', '2026-01-22 07:19:07', '2026-01-15 07:19:07', 1, '2026-01-15 07:46:13', 'a0f32646-25d7-4ccf-b444-63290225410d');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (99, 12, '1d9d5974-4043-4e41-bd76-fb68386cd7a8', '2026-01-22 07:46:13', '2026-01-15 07:46:13', 1, '2026-01-15 08:07:16', '179001e5-1284-4054-8085-0efb4c46c930');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (100, 12, '4c3f09b9-c58c-4009-a6be-2b411ea14f61', '2026-01-22 07:46:13', '2026-01-15 07:46:13', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (101, 12, 'a0f32646-25d7-4ccf-b444-63290225410d', '2026-01-22 07:46:13', '2026-01-15 07:46:13', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (102, 12, '179001e5-1284-4054-8085-0efb4c46c930', '2026-01-22 08:07:16', '2026-01-15 08:07:16', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (103, 12, 'a5d64daf-dbc1-4edc-98c6-b2957fcae3cc', '2026-01-22 08:07:40', '2026-01-15 08:07:40', 1, '2026-01-15 08:28:34', 'f34df0e9-5879-47fa-9fac-a7d83c4326bd');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (104, 12, '0e662be2-3311-4d4d-b6b2-45ddced16f7e', '2026-01-22 08:28:35', '2026-01-15 08:28:35', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (105, 12, '11a08b14-f8f5-4b58-8476-cdad5161dd20', '2026-01-22 08:28:35', '2026-01-15 08:28:35', 1, '2026-01-15 08:47:42', '4c0a7ad9-323f-4de4-8195-11872f620486');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (106, 12, 'f34df0e9-5879-47fa-9fac-a7d83c4326bd', '2026-01-22 08:28:35', '2026-01-15 08:28:35', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (107, 12, '0daa8ca7-f03b-4779-b359-2a8b08f80208', '2026-01-22 08:47:42', '2026-01-15 08:47:42', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (108, 12, '7ea4c31f-252d-49bc-a3fc-ce3129840feb', '2026-01-22 08:47:42', '2026-01-15 08:47:42', 1, '2026-01-15 09:57:01', 'b70f9360-a851-4863-8a79-805dcbb87758');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (109, 12, '4c0a7ad9-323f-4de4-8195-11872f620486', '2026-01-22 08:47:42', '2026-01-15 08:47:42', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (110, 12, 'b70f9360-a851-4863-8a79-805dcbb87758', '2026-01-22 09:57:01', '2026-01-15 09:57:01', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (111, 12, 'c9173c7d-cc0e-47a7-b720-290488d7012c', '2026-01-22 09:57:25', '2026-01-15 09:57:25', 1, '2026-01-15 10:13:30', '259faba8-95d8-484d-93c3-cc7396ef7579');
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (112, 12, '9f6ef607-ac53-4cce-a8c6-bf2d6743bb66', '2026-01-22 10:13:30', '2026-01-15 10:13:30', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (113, 12, '259faba8-95d8-484d-93c3-cc7396ef7579', '2026-01-22 10:13:30', '2026-01-15 10:13:30', 0, NULL, NULL);
INSERT INTO `refresh_tokens` (`id`, `user_id`, `token`, `expiry_date`, `created_at`, `is_revoked`, `revoked_at`, `replaced_by_token`) VALUES (114, 12, 'd68a4f71-6b9a-4e8c-98b4-650c5ada1a10', '2026-01-22 10:13:30', '2026-01-15 10:13:30', 0, NULL, NULL);


-- ----------------------------
-- Table structure for refund_reasons
-- ----------------------------
DROP TABLE IF EXISTS `refund_reasons`;
CREATE TABLE `refund_reasons` (
  `id` bigint NOT NULL,
  `code` varchar(64) NOT NULL,
  `name_ko` varchar(128) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_refund_reasons_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='환불 사유 코드 테이블';

-- ----------------------------
-- Records of refund_reasons
-- ----------------------------
-- No data in refund_reasons


-- ----------------------------
-- Table structure for refund_statuses
-- ----------------------------
DROP TABLE IF EXISTS `refund_statuses`;
CREATE TABLE `refund_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_refund_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='환불 상태 코드 테이블';

-- ----------------------------
-- Records of refund_statuses
-- ----------------------------
-- No data in refund_statuses


-- ----------------------------
-- Table structure for refunds
-- ----------------------------
DROP TABLE IF EXISTS `refunds`;
CREATE TABLE `refunds` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL,
  `payment_id` bigint NOT NULL COMMENT '결제 FK',
  `amount` int NOT NULL COMMENT '환불 금액',
  `reason_id` bigint NOT NULL DEFAULT '1' COMMENT '환불 사유 FK',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '상태 FK',
  `requested_by` bigint NOT NULL COMMENT '요청자 FK',
  `approved_by` bigint DEFAULT NULL COMMENT '승인자 FK',
  `processed_at` datetime DEFAULT NULL COMMENT '처리 완료 시각',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_refunds_trans` (`transaction_id`),
  KEY `idx_refunds_payment` (`payment_id`),
  KEY `idx_refunds_reason_id` (`reason_id`),
  KEY `idx_refunds_status_id` (`status_id`),
  KEY `idx_refunds_requested_by` (`requested_by`),
  KEY `idx_refunds_trans_status` (`transaction_id`,`status_id`),
  CONSTRAINT `fk_refunds_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`),
  CONSTRAINT `fk_refunds_reason` FOREIGN KEY (`reason_id`) REFERENCES `refund_reasons` (`id`),
  CONSTRAINT `fk_refunds_status` FOREIGN KEY (`status_id`) REFERENCES `refund_statuses` (`id`),
  CONSTRAINT `fk_refunds_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='환불 정보 테이블';

-- ----------------------------
-- Records of refunds
-- ----------------------------
-- No data in refunds


-- ----------------------------
-- Table structure for reputation_rating_types
-- ----------------------------
DROP TABLE IF EXISTS `reputation_rating_types`;
CREATE TABLE `reputation_rating_types` (
  `id` bigint NOT NULL,
  `code` varchar(16) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_reputation_rating_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='평판 평가 유형 코드 테이블';

-- ----------------------------
-- Records of reputation_rating_types
-- ----------------------------
-- No data in reputation_rating_types


-- ----------------------------
-- Table structure for seat_locations
-- ----------------------------
DROP TABLE IF EXISTS `seat_locations`;
CREATE TABLE `seat_locations` (
  `id` varchar(36) NOT NULL COMMENT '위치 ID (예: LOC_1F)',
  `event_id` int DEFAULT NULL COMMENT '공연 FK (NULL이면 전역 사용)',
  `location_name` varchar(100) NOT NULL COMMENT '위치명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `idx_locations_event` (`event_id`),
  CONSTRAINT `fk_locations_event` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='좌석 위치 옵션 테이블';

-- ----------------------------
-- Records of seat_locations
-- ----------------------------
INSERT INTO `seat_locations` (`id`, `event_id`, `location_name`, `is_active`, `sort_order`) VALUES ('LOC_1F', NULL, '1층', 1, 1);
INSERT INTO `seat_locations` (`id`, `event_id`, `location_name`, `is_active`, `sort_order`) VALUES ('LOC_2F', NULL, '2층', 1, 2);
INSERT INTO `seat_locations` (`id`, `event_id`, `location_name`, `is_active`, `sort_order`) VALUES ('LOC_STANDING', NULL, '스탠딩', 1, 3);
INSERT INTO `seat_locations` (`id`, `event_id`, `location_name`, `is_active`, `sort_order`) VALUES ('LOC_VIP', NULL, 'VIP석', 1, 4);


-- ----------------------------
-- Table structure for settlement_statuses
-- ----------------------------
DROP TABLE IF EXISTS `settlement_statuses`;
CREATE TABLE `settlement_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_settlement_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='정산 상태 코드 테이블';

-- ----------------------------
-- Records of settlement_statuses
-- ----------------------------
-- No data in settlement_statuses


-- ----------------------------
-- Table structure for settlements
-- ----------------------------
DROP TABLE IF EXISTS `settlements`;
CREATE TABLE `settlements` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL,
  `seller_id` bigint NOT NULL COMMENT '판매자 FK',
  `amount` int NOT NULL COMMENT '총 금액',
  `fee` int NOT NULL COMMENT '수수료',
  `net_amount` int NOT NULL COMMENT '순 정산 금액',
  `bank_account_id` bigint NOT NULL COMMENT '정산 계좌 FK',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '상태 FK',
  `scheduled_at` datetime NOT NULL COMMENT '정산 예정 일시',
  `processed_at` datetime DEFAULT NULL COMMENT '정산 완료 시각',
  `failure_reason` text COMMENT '실패 사유',
  `retry_count` int DEFAULT '0' COMMENT '재시도 횟수',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_settlements_trans` (`transaction_id`),
  KEY `idx_settlements_seller` (`seller_id`),
  KEY `idx_settlements_bank` (`bank_account_id`),
  KEY `idx_settlements_status` (`status_id`),
  KEY `idx_settlements_scheduled` (`scheduled_at`),
  KEY `idx_settlements_status_scheduled` (`status_id`,`scheduled_at`),
  KEY `idx_settlements_failed` (`status_id`,`retry_count`,`scheduled_at`),
  CONSTRAINT `fk_settlements_bank` FOREIGN KEY (`bank_account_id`) REFERENCES `bank_account` (`id`),
  CONSTRAINT `fk_settlements_status` FOREIGN KEY (`status_id`) REFERENCES `settlement_statuses` (`id`),
  CONSTRAINT `fk_settlements_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`),
  CONSTRAINT `chk_settlement_amounts` CHECK ((`amount` = (`fee` + `net_amount`))),
  CONSTRAINT `chk_settlement_retry` CHECK (((`retry_count` >= 0) and (`retry_count` <= 5)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='정산 정보 테이블';

-- ----------------------------
-- Records of settlements
-- ----------------------------
-- No data in settlements


-- ----------------------------
-- Table structure for ticket_category
-- ----------------------------
DROP TABLE IF EXISTS `ticket_category`;
CREATE TABLE `ticket_category` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_category_code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 카테고리 코드 테이블';

-- ----------------------------
-- Records of ticket_category
-- ----------------------------
INSERT INTO `ticket_category` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'concert', '콘서트', 1, 1);
INSERT INTO `ticket_category` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'sports', '스포츠', 1, 2);
INSERT INTO `ticket_category` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (3, 'musical', '뮤지컬/연극', 1, 3);
INSERT INTO `ticket_category` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (4, 'exhibition', '전시/행사', 1, 4);


-- ----------------------------
-- Table structure for ticket_images
-- ----------------------------
DROP TABLE IF EXISTS `ticket_images`;
CREATE TABLE `ticket_images` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ticket_id` bigint NOT NULL,
  `image_url` varchar(500) NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_ticket_img_ticket` (`ticket_id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 이미지 테이블';

-- ----------------------------
-- Records of ticket_images
-- ----------------------------
INSERT INTO `ticket_images` (`id`, `ticket_id`, `image_url`, `created_at`) VALUES (1, 34, 'tickets/34/9756f69bd8da4157b9c8ffff3fb41509.jpg', '2026-01-15 09:56:16');


-- ----------------------------
-- Table structure for ticket_price_history
-- ----------------------------
DROP TABLE IF EXISTS `ticket_price_history`;
CREATE TABLE `ticket_price_history` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ticket_id` bigint NOT NULL,
  `old_price` int NOT NULL COMMENT '변경 전 가격',
  `new_price` int NOT NULL COMMENT '변경 후 가격',
  `reason` varchar(255) DEFAULT NULL COMMENT '변경 사유',
  `changed_by` bigint DEFAULT NULL COMMENT '변경자 FK',
  `changed_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_ticket_price_ticket` (`ticket_id`),
  KEY `idx_ticket_price_changed_by` (`changed_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 가격 변경 이력 테이블';

-- ----------------------------
-- Records of ticket_price_history
-- ----------------------------
-- No data in ticket_price_history


-- ----------------------------
-- Table structure for ticket_statuses
-- ----------------------------
DROP TABLE IF EXISTS `ticket_statuses`;
CREATE TABLE `ticket_statuses` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 상태 코드 테이블';

-- ----------------------------
-- Records of ticket_statuses
-- ----------------------------
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'available', '판매중', 1, 1);
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'reserved', '예약중', 1, 2);
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (3, 'sold_out', '품절', 1, 3);
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (4, 'expired', '만료', 1, 4);
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (5, 'hidden', '숨김', 1, 5);
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (6, 'pending_review', '검수대기', 1, 6);
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (7, 'cancelled', '판매취소', 1, 7);


-- ----------------------------
-- Table structure for ticket_verification
-- ----------------------------
DROP TABLE IF EXISTS `ticket_verification`;
CREATE TABLE `ticket_verification` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL COMMENT '거래 FK',
  `method_id` bigint NOT NULL COMMENT '검증 방법 FK',
  `raw_data` text COMMENT 'OCR/QR 원본 데이터',
  `verification_result` tinyint(1) DEFAULT NULL COMMENT '검증 결과',
  `verified_by` bigint DEFAULT NULL COMMENT '검증자 FK (수동 검증 시)',
  `ocr_confidence` float DEFAULT NULL COMMENT 'OCR 신뢰도',
  `qr_code_hash` varchar(255) DEFAULT NULL COMMENT 'QR코드 해시',
  `ticket_number` varchar(100) DEFAULT NULL COMMENT '티켓 번호',
  `verified_at` timestamp NULL DEFAULT NULL COMMENT '검증 시각',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_verification_trans_method` (`transaction_id`,`method_id`),
  KEY `fk_ticket_verification_method` (`method_id`),
  KEY `idx_verify_trans` (`transaction_id`),
  KEY `idx_verify_verified_by` (`verified_by`),
  CONSTRAINT `fk_ticket_verification_method` FOREIGN KEY (`method_id`) REFERENCES `ticket_verification_methods` (`id`),
  CONSTRAINT `fk_ticket_verification_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 검증 테이블';

-- ----------------------------
-- Records of ticket_verification
-- ----------------------------
-- No data in ticket_verification


-- ----------------------------
-- Table structure for ticket_verification_methods
-- ----------------------------
DROP TABLE IF EXISTS `ticket_verification_methods`;
CREATE TABLE `ticket_verification_methods` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_verification_methods_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 검증 방법 코드 테이블';

-- ----------------------------
-- Records of ticket_verification_methods
-- ----------------------------
-- No data in ticket_verification_methods


-- ----------------------------
-- Table structure for tickets
-- ----------------------------
DROP TABLE IF EXISTS `tickets`;
CREATE TABLE `tickets` (
  `id` int NOT NULL AUTO_INCREMENT,
  `seller_id` int NOT NULL,
  `event_id` int DEFAULT NULL COMMENT '공연 FK',
  `schedule_id` varchar(36) DEFAULT NULL COMMENT '일정 FK',
  `category_id` int NOT NULL,
  `title` varchar(255) NOT NULL COMMENT '티켓 제목',
  `event_datetime` datetime NOT NULL COMMENT '공연 일시',
  `seat_info` varchar(255) DEFAULT NULL COMMENT '좌석 정보',
  `location_id` varchar(36) DEFAULT NULL COMMENT '좌석 위치 FK',
  `area` varchar(50) DEFAULT NULL COMMENT '구역 (예: A구역)',
  `row` varchar(20) DEFAULT NULL COMMENT '열 (예: 5열)',
  `quantity` int NOT NULL COMMENT '총 수량',
  `is_consecutive` tinyint(1) DEFAULT '0' COMMENT '연석 여부',
  `remaining_quantity` int NOT NULL DEFAULT '0' COMMENT '남은 수량',
  `price` int NOT NULL COMMENT '판매가',
  `original_price` int NOT NULL COMMENT '정가',
  `description` text COMMENT '상세 설명',
  `status_id` int NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `deleted_at` timestamp NULL DEFAULT NULL COMMENT 'Soft Delete 시각',
  `seat_features` json DEFAULT NULL COMMENT '좌석 특징 키워드 (JSON 배열)',
  PRIMARY KEY (`id`),
  KEY `idx_tickets_seller` (`seller_id`),
  KEY `idx_tickets_status` (`status_id`),
  KEY `idx_tickets_event_date` (`event_datetime`),
  KEY `idx_tickets_created` (`created_at`),
  KEY `idx_tickets_not_deleted` (`deleted_at`),
  KEY `idx_tickets_remaining_qty` (`remaining_quantity`),
  KEY `idx_tickets_list` (`status_id`,`event_datetime`),
  KEY `idx_tickets_search` (`status_id`,`event_datetime`,`price`),
  KEY `idx_tickets_category_status` (`category_id`,`status_id`),
  KEY `fk_tickets_event` (`event_id`),
  KEY `idx_tickets_schedule` (`schedule_id`),
  KEY `idx_tickets_location` (`location_id`),
  CONSTRAINT `fk_ticket_category` FOREIGN KEY (`category_id`) REFERENCES `ticket_category` (`id`),
  CONSTRAINT `fk_tickets_event` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`),
  CONSTRAINT `fk_tickets_status` FOREIGN KEY (`status_id`) REFERENCES `ticket_statuses` (`id`),
  CONSTRAINT `chk_ticket_original_price` CHECK ((`original_price` >= `price`)),
  CONSTRAINT `chk_ticket_price` CHECK ((`price` > 0)),
  CONSTRAINT `chk_ticket_quantity` CHECK ((`quantity` > 0)),
  CONSTRAINT `chk_ticket_remaining_qty` CHECK (((`remaining_quantity` >= 0) and (`remaining_quantity` <= `quantity`)))
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 정보 테이블';

-- ----------------------------
-- Records of tickets
-- ----------------------------
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (1, 7, 1, NULL, 1, '아이유 콘서트 VIP석', '2026-01-28 19:00:00', 'VIP A구역 1열', NULL, NULL, NULL, 2, 0, 2, 220000, 250000, 'VIP석 연석 2장', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, '["VIP", "앞좌석", "통로석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (2, 7, 1, NULL, 1, '아이유 콘서트 R석', '2026-01-28 19:00:00', 'R구역 5열 10번', NULL, NULL, NULL, 1, 0, 1, 150000, 180000, 'R석 1장', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, '["연석", "중앙"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (3, 7, 1, NULL, 1, '아이유 콘서트 S석', '2026-01-28 19:00:00', 'S구역 10열', NULL, NULL, NULL, 3, 0, 1, 99000, 130000, 'S석', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, '["단석", "뒷좌석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (4, 7, 1, NULL, 1, '아이유 콘서트 VIP석', '2026-01-29 19:00:00', 'VIP B구역 2열', NULL, NULL, NULL, 2, 0, 0, 230000, 250000, 'VIP석 연석 2장 (매진임박)', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, '["VIP", "연석", "앞좌석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (5, 7, 1, NULL, 1, '아이유 콘서트 R석', '2026-01-29 19:00:00', 'R구역 3열 5번', NULL, NULL, NULL, 4, 0, 2, 160000, 180000, 'R석', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, '["통로석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (6, 7, 2, NULL, 1, '뉴진스 Bunnies Camp VIP', '2026-02-23 18:00:00', 'VIP 1구역', NULL, NULL, NULL, 10, 0, 8, 180000, 200000, 'VIP 입장권', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (7, 7, 2, NULL, 1, '뉴진스 Bunnies Camp 일반', '2026-02-23 18:00:00', '일반 구역', NULL, NULL, NULL, 30, 0, 25, 99000, 120000, '일반 입장권', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (8, 7, 2, NULL, 1, '뉴진스 Bunnies Camp 일반', '2026-02-24 18:00:00', '일반 구역', NULL, NULL, NULL, 20, 0, 12, 99000, 120000, '일반 입장권', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (9, 7, 3, NULL, 1, '싸이 흠뻑쇼 스탠딩', '2026-08-02 17:00:00', '스탠딩 A구역', NULL, NULL, NULL, 50, 0, 30, 132000, 150000, '스탠딩 입장권', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (10, 7, 3, NULL, 1, '싸이 흠뻑쇼 지정석', '2026-08-02 17:00:00', '지정석 1열', NULL, NULL, NULL, 20, 0, 2, 165000, 180000, '지정석 입장권 (인기)', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (11, 7, 3, NULL, 1, '싸이 흠뻑쇼 스탠딩', '2026-08-03 17:00:00', '스탠딩 B구역', NULL, NULL, NULL, 50, 0, 50, 132000, 150000, '스탠딩 입장권', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (12, 7, 4, NULL, 1, '임영웅 앙코르 VIP', '2026-03-14 18:00:00', 'VIP석', NULL, NULL, NULL, 10, 0, 5, 250000, 280000, 'VIP석 (티켓 5장 남음)', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (13, 7, 6, NULL, 1, 'BTS Yet To Come 스탠딩', '2026-10-28 19:00:00', '스탠딩', NULL, NULL, NULL, 100, 0, 20, 180000, 200000, '스탠딩 입장권', 1, '2025-12-17 16:49:16', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (14, 7, 7, NULL, 3, '위키드 VIP석', '2026-03-14 14:00:00', '1층 A구역 3열 5번', NULL, NULL, NULL, 2, 0, 2, 180000, 198000, 'VIP석 2연석입니다. 시야 최고!', 1, '2025-12-18 13:03:13', '2026-01-14 14:26:08', NULL, '["VIP", "연석", "앞좌석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (15, 8, 7, NULL, 3, '위키드 R석', '2026-03-14 14:00:00', '1층 B구역 10열 15번', NULL, NULL, NULL, 1, 0, 1, 130000, 154000, 'R석 단석입니다.', 1, '2025-12-18 13:03:13', '2026-01-14 14:26:08', NULL, '["단석", "중앙"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (16, 9, 8, NULL, 3, '지킬앤하이드 S석', '2026-04-23 19:30:00', '2층 C구역 2열 8번', NULL, NULL, NULL, 2, 0, 2, 90000, 110000, '조승우 캐스팅일입니다!', 1, '2025-12-18 13:03:13', '2026-01-14 14:26:08', NULL, '["연석", "2층"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (17, 10, 8, NULL, 3, '지킬앤하이드 VIP석', '2026-04-24 14:00:00', '1층 정중앙 5열', NULL, NULL, NULL, 1, 0, 1, 200000, 220000, '정중앙 최고의 시야', 1, '2025-12-18 13:03:13', '2026-01-14 14:26:08', NULL, '["VIP", "중앙", "앞좌석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (18, 7, 9, NULL, 3, '엘리자벳 R석', '2026-05-28 19:00:00', '1층 D구역 8열 20번', NULL, NULL, NULL, 2, 0, 2, 150000, 176000, '김준수 캐스팅, 연석', 1, '2025-12-18 13:03:13', '2026-01-14 14:26:08', NULL, '["연석", "통로석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (19, 8, 10, NULL, 3, '알라딘 S석', '2026-07-03 19:00:00', '2층 A구역 1열 5번', NULL, NULL, NULL, 1, 0, 1, 80000, 99000, '2층 맨 앞줄 시야 좋아요', 1, '2025-12-18 13:03:13', '2026-01-14 14:26:08', NULL, '["단석", "앞좌석", "2층"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (20, 10, 11, NULL, 2, 'KIA vs 두산 내야석', '2026-04-18 14:00:00', '1루 내야 C블록 5열 12번', NULL, NULL, NULL, 2, 0, 2, 25000, 30000, '야구 관람 좋은 자리입니다', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["연석", "내야석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (21, 7, 11, NULL, 2, 'KIA vs 두산 외야석', '2026-04-18 14:00:00', '외야 응원석 자유석', NULL, NULL, NULL, 4, 0, 4, 12000, 15000, '외야 응원석 4장 함께 드려요', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["자유석", "외야석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (22, 8, 12, NULL, 2, '두산 홈경기 테이블석', '2026-04-25 18:30:00', '테이블석 T구역 3번', NULL, NULL, NULL, 2, 0, 2, 50000, 60000, '테이블석 연석, 맥주 마시며 관람', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["테이블석", "연석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (23, 9, 13, NULL, 2, 'FC서울 W석', '2026-05-23 19:00:00', 'W구역 10열 5번', NULL, NULL, NULL, 2, 0, 2, 35000, 40000, '축구 경기 좋은 시야', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["연석", "중앙"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (24, 11, 13, NULL, 2, 'FC서울 S석', '2026-05-23 19:00:00', 'S구역 15열 20번', NULL, NULL, NULL, 1, 0, 1, 20000, 25000, '가성비 좋은 자리', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["단석"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (25, 10, 14, NULL, 2, '삼성 vs SK 코트사이드', '2026-11-28 18:00:00', '코트사이드 A열 5번', NULL, NULL, NULL, 2, 0, 2, 150000, 180000, '코트 바로 옆! 선수들 코앞에서!', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["연석", "코트사이드", "VIP"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (26, 7, 15, NULL, 2, '대한민국 vs 일본 S석', '2026-06-18 20:00:00', 'S구역 5열 30번', NULL, NULL, NULL, 2, 0, 2, 80000, 100000, '손흥민 볼 수 있어요!', 1, '2025-12-18 13:03:31', '2026-01-14 14:26:08', NULL, '["연석", "중앙"]');
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (27, 7, 16, NULL, 4, '반 고흐 인사이드 입장권', '2026-03-28 14:00:00', NULL, NULL, NULL, NULL, 2, 0, 2, 18000, 22000, '제주 빛의 시어터 반 고흐 전시 입장권입니다', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (28, 8, 17, NULL, 4, '팀랩 보더리스 티켓', '2026-05-14 15:00:00', NULL, NULL, NULL, NULL, 1, 0, 1, 25000, 30000, '디지털 아트 전시 입장권', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (29, 9, 18, NULL, 4, '모네 빛을 그리다 티켓', '2026-06-02 11:00:00', NULL, NULL, NULL, NULL, 2, 0, 2, 16000, 20000, '모네 특별전 입장권 2매', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (30, 10, NULL, NULL, 4, '에버랜드 자유이용권', '2026-06-14 10:00:00', NULL, NULL, NULL, NULL, 2, 0, 2, 45000, 58000, '에버랜드 1일 자유이용권입니다', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (31, 11, NULL, NULL, 4, '롯데월드 자유이용권', '2026-07-28 10:00:00', NULL, NULL, NULL, NULL, 1, 0, 1, 40000, 52000, '롯데월드 1일권 판매합니다', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (32, 7, NULL, NULL, 4, '캐리비안베이 입장권', '2026-08-23 10:00:00', NULL, NULL, NULL, NULL, 3, 0, 3, 35000, 45000, '여름 캐리비안베이 3장 일괄', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (33, 8, NULL, NULL, 4, 'CGV 영화 예매권', '2027-01-13 23:59:59', NULL, NULL, NULL, NULL, 5, 0, 5, 10000, 14000, 'CGV 영화 관람권 5장', 1, '2025-12-18 13:03:48', '2026-01-14 14:26:08', NULL, NULL);
INSERT INTO `tickets` (`id`, `seller_id`, `event_id`, `schedule_id`, `category_id`, `title`, `event_datetime`, `seat_info`, `location_id`, `area`, `row`, `quantity`, `is_consecutive`, `remaining_quantity`, `price`, `original_price`, `description`, `status_id`, `created_at`, `updated_at`, `deleted_at`, `seat_features`) VALUES (34, 12, 1, 'SCH001', 1, '2024 월드 투어 서울 - 2026-01-28', '2026-01-28 19:00:00', '1층 a구역 2열', 'LOC_1F', 'a구역', '2열', 2, 1, 2, 5000, 5000, '테스트', 1, '2026-01-15 09:56:14', '2026-01-15 10:12:40', NULL, NULL);


-- ----------------------------
-- Table structure for transaction_confirmed_bys
-- ----------------------------
DROP TABLE IF EXISTS `transaction_confirmed_bys`;
CREATE TABLE `transaction_confirmed_bys` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_transaction_confirmed_bys_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 확인자 유형 코드 테이블';

-- ----------------------------
-- Records of transaction_confirmed_bys
-- ----------------------------
INSERT INTO `transaction_confirmed_bys` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'buyer', '구매자', 1, 1);
INSERT INTO `transaction_confirmed_bys` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'seller', '판매자', 1, 2);


-- ----------------------------
-- Table structure for transaction_history
-- ----------------------------
DROP TABLE IF EXISTS `transaction_history`;
CREATE TABLE `transaction_history` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL,
  `old_status` varchar(50) DEFAULT NULL COMMENT '이전 상태 코드',
  `new_status` varchar(50) DEFAULT NULL COMMENT '새 상태 코드',
  `changed_by` bigint DEFAULT NULL COMMENT '변경자 FK',
  `changed_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_trans_history_trans` (`transaction_id`),
  CONSTRAINT `fk_transaction_history_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 상태 변경 이력 테이블';

-- ----------------------------
-- Records of transaction_history
-- ----------------------------
-- No data in transaction_history


-- ----------------------------
-- Table structure for transaction_items
-- ----------------------------
DROP TABLE IF EXISTS `transaction_items`;
CREATE TABLE `transaction_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL COMMENT '거래 FK',
  `ticket_id` bigint NOT NULL COMMENT '티켓 FK',
  `quantity` int NOT NULL COMMENT '구매 수량',
  `unit_price` int NOT NULL COMMENT '단가',
  `total_price` int NOT NULL COMMENT '소계 (단가 × 수량)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_trans_items_trans_ticket` (`transaction_id`,`ticket_id`),
  KEY `idx_trans_items_trans` (`transaction_id`),
  KEY `idx_trans_items_ticket` (`ticket_id`),
  CONSTRAINT `fk_trans_items_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`),
  CONSTRAINT `chk_trans_items_price` CHECK (((`unit_price` >= 0) and (`total_price` >= 0))),
  CONSTRAINT `chk_trans_items_qty` CHECK ((`quantity` > 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 항목 테이블 (티켓별 구매 정보)';

-- ----------------------------
-- Records of transaction_items
-- ----------------------------
-- No data in transaction_items


-- ----------------------------
-- Table structure for transaction_statuses
-- ----------------------------
DROP TABLE IF EXISTS `transaction_statuses`;
CREATE TABLE `transaction_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_transaction_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 상태 코드 테이블';

-- ----------------------------
-- Records of transaction_statuses
-- ----------------------------
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (1, 'reserved', '예약중', 1, 1);
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (2, 'pending_payment', '결제대기', 1, 2);
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (3, 'paid', '결제완료', 1, 3);
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (4, 'confirmed', '구매확정', 1, 4);
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (5, 'completed', '거래완료', 1, 5);
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (6, 'cancelled', '취소됨', 1, 6);
INSERT INTO `transaction_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`) VALUES (7, 'refunded', '환불됨', 1, 7);


-- ----------------------------
-- Table structure for transactions
-- ----------------------------
DROP TABLE IF EXISTS `transactions`;
CREATE TABLE `transactions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `buyer_id` bigint NOT NULL COMMENT '구매자 FK',
  `seller_id` bigint NOT NULL COMMENT '판매자 FK',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '상태 FK',
  `reserved_at` datetime DEFAULT NULL COMMENT '예약 시각',
  `reservation_expires_at` datetime DEFAULT NULL COMMENT '예약 만료 시각',
  `confirmed_at` datetime DEFAULT NULL COMMENT '구매 확정 시각',
  `auto_confirm_at` datetime DEFAULT NULL COMMENT '자동 확정 예정 시각',
  `confirmed_by_id` bigint DEFAULT NULL COMMENT '확정자 유형 FK',
  `cancelled_at` datetime DEFAULT NULL COMMENT '취소 시각',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `deleted_at` timestamp NULL DEFAULT NULL COMMENT 'Soft Delete 시각',
  PRIMARY KEY (`id`),
  KEY `fk_transactions_confirmed_by` (`confirmed_by_id`),
  KEY `idx_trans_buyer` (`buyer_id`),
  KEY `idx_trans_seller` (`seller_id`),
  KEY `idx_trans_status` (`status_id`),
  KEY `idx_trans_created` (`created_at`),
  KEY `idx_trans_not_deleted` (`deleted_at`),
  KEY `idx_trans_buyer_status` (`buyer_id`,`status_id`),
  KEY `idx_trans_seller_status` (`seller_id`,`status_id`),
  CONSTRAINT `fk_transactions_confirmed_by` FOREIGN KEY (`confirmed_by_id`) REFERENCES `transaction_confirmed_bys` (`id`),
  CONSTRAINT `fk_transactions_status` FOREIGN KEY (`status_id`) REFERENCES `transaction_statuses` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 정보 테이블 (하나의 거래에 여러 티켓 항목 가능)';

-- ----------------------------
-- Records of transactions
-- ----------------------------
-- No data in transactions


-- ----------------------------
-- Table structure for user_favorites
-- ----------------------------
DROP TABLE IF EXISTS `user_favorites`;
CREATE TABLE `user_favorites` (
  `id` int NOT NULL AUTO_INCREMENT,
  `user_id` int NOT NULL,
  `favorite_type_id` int NOT NULL,
  `target_id` int NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_user_favorite` (`user_id`,`favorite_type_id`,`target_id`),
  KEY `idx_user_favorites_user` (`user_id`),
  KEY `idx_user_favorites_type_target` (`favorite_type_id`,`target_id`),
  CONSTRAINT `fk_user_favorites_type` FOREIGN KEY (`favorite_type_id`) REFERENCES `favorite_types` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 찜 테이블';

-- ----------------------------
-- Records of user_favorites
-- ----------------------------
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (1, 7, 1, 1, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (2, 7, 1, 2, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (3, 8, 1, 1, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (4, 8, 1, 6, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (5, 9, 1, 4, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (6, 10, 1, 5, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (7, 7, 2, 1, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (8, 7, 2, 3, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (9, 8, 2, 2, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (10, 9, 2, 1, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (11, 10, 2, 4, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (12, 11, 2, 5, '2025-12-18 11:05:53');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (15, 1, 2, 9, '2026-01-08 19:27:49');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (22, 12, 2, 3, '2026-01-12 16:21:08');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (31, 12, 2, 2, '2026-01-15 10:06:10');
INSERT INTO `user_favorites` (`id`, `user_id`, `favorite_type_id`, `target_id`, `created_at`) VALUES (32, 12, 2, 34, '2026-01-15 13:10:12');


-- ----------------------------
-- Table structure for user_profile
-- ----------------------------
DROP TABLE IF EXISTS `user_profile`;
CREATE TABLE `user_profile` (
  `user_id` int NOT NULL,
  `nickname` varchar(50) DEFAULT NULL,
  `profile_image_url` varchar(500) DEFAULT NULL COMMENT '프로필 이미지 URL',
  `bio` text COMMENT '자기소개',
  `manner_temperature` float DEFAULT '36.5' COMMENT '매너 온도 (36.5~99.9)',
  `total_trade_count` int DEFAULT '0' COMMENT '총 거래 횟수',
  PRIMARY KEY (`user_id`),
  KEY `idx_user_profile_nickname` (`nickname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 프로필 테이블';

-- ----------------------------
-- Records of user_profile
-- ----------------------------
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (7, '티켓마스터', 'https://picsum.photos/200/200?random=1', '안녕하세요! 공연 티켓 거래합니다.', 38.5, 15);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (8, '콘서트러버', 'https://picsum.photos/200/200?random=2', '콘서트를 사랑하는 사람입니다', 42.0, 28);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (9, '뮤지컬팬', 'https://picsum.photos/200/200?random=3', '뮤지컬 덕후입니다 ^^', 36.5, 3);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (10, '스포츠광', 'https://picsum.photos/200/200?random=4', '야구, 축구 다 좋아해요', 45.2, 42);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (11, '문화생활', 'https://picsum.photos/200/200?random=5', '전시회도 좋아합니다', 39.8, 18);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (12, 'test success', 'profiles/12/df0c26f16204404596b163c90c40d637.jpg', '자기소개란 테스트', 36.5, 0);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (13, NULL, NULL, NULL, 36.5, 0);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (14, NULL, NULL, NULL, 36.5, 0);
INSERT INTO `user_profile` (`user_id`, `nickname`, `profile_image_url`, `bio`, `manner_temperature`, `total_trade_count`) VALUES (15, '겸손한앵무새', NULL, NULL, 36.5, 0);


-- ----------------------------
-- Table structure for user_reputation
-- ----------------------------
DROP TABLE IF EXISTS `user_reputation`;
CREATE TABLE `user_reputation` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL COMMENT '평가 대상 FK',
  `reviewer_id` bigint NOT NULL COMMENT '평가자 FK',
  `transaction_id` bigint NOT NULL COMMENT '거래 FK',
  `rating_type_id` bigint NOT NULL COMMENT '평가 유형 FK',
  `score` int NOT NULL COMMENT '점수 (1-5)',
  `comment` text COMMENT '리뷰 내용',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_reputation_user` (`user_id`,`created_at`),
  KEY `idx_reputation_reviewer` (`reviewer_id`),
  KEY `idx_reputation_trans` (`transaction_id`),
  KEY `idx_reputation_rating_type_id` (`rating_type_id`),
  CONSTRAINT `fk_user_reputation_rating_type` FOREIGN KEY (`rating_type_id`) REFERENCES `reputation_rating_types` (`id`),
  CONSTRAINT `fk_user_reputation_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`),
  CONSTRAINT `chk_score` CHECK ((`score` between 1 and 5))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 평판 (리뷰) 테이블';

-- ----------------------------
-- Records of user_reputation
-- ----------------------------
-- No data in user_reputation


-- ----------------------------
-- Table structure for user_verification
-- ----------------------------
DROP TABLE IF EXISTS `user_verification`;
CREATE TABLE `user_verification` (
  `user_id` bigint NOT NULL,
  `name` varchar(50) DEFAULT NULL COMMENT '실명',
  `birth` date DEFAULT NULL COMMENT '생년월일',
  `identity_verified` tinyint(1) DEFAULT '0' COMMENT '본인 인증 완료',
  `phone_verified` tinyint(1) DEFAULT '0' COMMENT '휴대폰 인증 완료',
  `account_verified` tinyint(1) DEFAULT '0' COMMENT '계좌 인증 완료',
  `verified_at` timestamp NULL DEFAULT NULL COMMENT '인증 완료 시각',
  PRIMARY KEY (`user_id`),
  KEY `idx_verif_identity` (`identity_verified`),
  KEY `idx_verif_account` (`account_verified`),
  KEY `idx_verif_all_verified` (`identity_verified`,`phone_verified`,`account_verified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 본인 인증 정보 테이블';

-- ----------------------------
-- Records of user_verification
-- ----------------------------
-- No data in user_verification


-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `email` varchar(255) NOT NULL COMMENT '이메일 (로그인 ID)',
  `password_hash` varchar(255) DEFAULT NULL COMMENT '비밀번호 해시 (소셜 로그인 시 NULL)',
  `phone` varchar(20) DEFAULT NULL COMMENT '연락처',
  `provider_id` int NOT NULL DEFAULT '1',
  `role_id` int NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `last_login_at` timestamp NULL DEFAULT NULL COMMENT '마지막 로그인 시각',
  `is_deleted` tinyint(1) DEFAULT '0' COMMENT '탈퇴 여부 (Soft Delete)',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_email` (`email`),
  KEY `idx_users_email` (`email`),
  KEY `idx_users_deleted` (`is_deleted`),
  KEY `idx_users_provider_id` (`provider_id`),
  KEY `idx_users_role_id` (`role_id`),
  CONSTRAINT `fk_users_provider` FOREIGN KEY (`provider_id`) REFERENCES `auth_providers` (`id`),
  CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `auth_roles` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 기본 정보 테이블';

-- ----------------------------
-- Records of users
-- ----------------------------
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (7, 'user7@example.com', '$2a$11$KzrYQ.GE9g.HL71sWBdlYuRYR3iCxXkR2Q./S1rkMVrZkKwMNLvkq', '01063937605', 1, 2, '2025-12-16 16:14:44', '2026-01-12 00:38:14', 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (8, 'user8@example.com', '$2a$11$OhNNpB7gHZUfNylXL.A4l.bdqJfd2f5tEeBLItfdb5IzORpdFEkXm', 'string', 1, 2, '2025-12-16 16:19:04', NULL, 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (9, 'user9@example.com', '$2a$11$.0wEtmpZhxsQx2wr3jPiO.EVjyaJCd6Q9F/7mTBZJQzxTVghE5FOK', '01063937605', 1, 2, '2025-12-16 16:21:54', NULL, 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (10, 'user10@example.com', '$2a$11$Ly39wSG/2fetq46qFoioXOXVp18G40kYQ/RDGC.EeRq94IM/HK23S', '01063937605', 1, 2, '2025-12-16 16:22:47', NULL, 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (11, 'user11@example.com', '$2a$11$lUQ1UJ9l73n0VERun/8.s.gRLYDt.7bvudsuupJkEgws2AdLcCx/W', '01063937605', 1, 2, '2025-12-16 16:28:32', '2025-12-16 07:28:43', 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (12, 'test@test.com', '$2a$11$ZH/.ReLIZsYPK0nI6uIIOumilFm1y6Jlo/VY4ONfFYU8uASvvhq/.', '01012345678', 1, 2, '2026-01-12 09:39:21', '2026-01-15 09:57:25', 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (13, 'hu@test.com', '$2a$11$S.kZpHTadN5m54XtYQQMiusKeiWc8fJpB.Q13fWQ2eiXnLg99yOQW', '01012345678', 1, 2, '2026-01-12 13:10:09', NULL, 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (14, 'chan@test.com', '$2a$11$jI36SxUcb2ynZ.nQbT1KKeANQukn.pNQ.cQqSGJG9gVsET7lIVrpS', '01012345678', 1, 2, '2026-01-12 13:21:26', NULL, 0);
INSERT INTO `users` (`id`, `email`, `password_hash`, `phone`, `provider_id`, `role_id`, `created_at`, `last_login_at`, `is_deleted`) VALUES (15, 'new@new.com', '$2a$11$dpkX5fXt0Zl8l0gag3sWueShPeRZzed9tHdWChNqCZ4TrFKvYGBDq', '01012345678', 1, 2, '2026-01-15 13:14:48', '2026-01-15 05:39:46', 0);

SET FOREIGN_KEY_CHECKS = 1;
