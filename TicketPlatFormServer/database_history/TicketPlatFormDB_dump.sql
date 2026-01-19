-- MySQL dump 10.13  Distrib 9.4.0, for macos15.4 (arm64)
--
-- Host: 127.0.0.1    Database: TicketPlatFormDB
-- ------------------------------------------------------
-- Server version	9.4.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `admin_action_types`
--

DROP TABLE IF EXISTS `admin_action_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `admin_action_types` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL COMMENT '한글 표시명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1' COMMENT '활성화 여부',
  `sort_order` int NOT NULL DEFAULT '0' COMMENT '정렬 순서',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_admin_action_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='관리자 액션 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `admin_action_types`
--

LOCK TABLES `admin_action_types` WRITE;
/*!40000 ALTER TABLE `admin_action_types` DISABLE KEYS */;
/*!40000 ALTER TABLE `admin_action_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `admin_actions`
--

DROP TABLE IF EXISTS `admin_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `admin_actions`
--

LOCK TABLES `admin_actions` WRITE;
/*!40000 ALTER TABLE `admin_actions` DISABLE KEYS */;
/*!40000 ALTER TABLE `admin_actions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `admin_target_types`
--

DROP TABLE IF EXISTS `admin_target_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `admin_target_types` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL COMMENT '한글 표시명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_admin_target_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='관리자 작업 대상 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `admin_target_types`
--

LOCK TABLES `admin_target_types` WRITE;
/*!40000 ALTER TABLE `admin_target_types` DISABLE KEYS */;
/*!40000 ALTER TABLE `admin_target_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `artist_followers`
--

DROP TABLE IF EXISTS `artist_followers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `artist_followers`
--

LOCK TABLES `artist_followers` WRITE;
/*!40000 ALTER TABLE `artist_followers` DISABLE KEYS */;
/*!40000 ALTER TABLE `artist_followers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `artists`
--

DROP TABLE IF EXISTS `artists`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `artists`
--

LOCK TABLES `artists` WRITE;
/*!40000 ALTER TABLE `artists` DISABLE KEYS */;
INSERT INTO `artists` VALUES (1,1,'아이유 (IU)','https://picsum.photos/200/300?random=1','대한민국 대표 솔로 가수',1,1,'2025-12-17 07:48:35','2025-12-17 07:54:49'),(2,1,'뉴진스 (NewJeans)','https://picsum.photos/200/300?random=2','HYBE 소속 5인조 걸그룹',1,2,'2025-12-17 07:48:35','2025-12-17 07:54:49'),(3,1,'싸이 (PSY)','https://picsum.photos/200/300?random=3','강남스타일로 세계를 놀라게 한 가수',1,3,'2025-12-17 07:48:35','2025-12-17 07:54:49'),(4,1,'임영웅','https://picsum.photos/200/300?random=4','트로트 황제, 국민 가수',1,4,'2025-12-17 07:48:35','2025-12-17 07:54:49'),(5,1,'데이식스 (DAY6)','https://picsum.photos/200/300?random=5','JYP 소속 밴드',1,5,'2025-12-17 07:48:35','2025-12-17 07:54:49'),(6,1,'BTS','https://picsum.photos/200/300?random=6','전세계를 사로잡은 K-POP 그룹',1,6,'2025-12-17 07:48:35','2025-12-17 07:54:49');
/*!40000 ALTER TABLE `artists` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `auth_providers`
--

DROP TABLE IF EXISTS `auth_providers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auth_providers` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_auth_providers_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='인증 제공자 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `auth_providers`
--

LOCK TABLES `auth_providers` WRITE;
/*!40000 ALTER TABLE `auth_providers` DISABLE KEYS */;
INSERT INTO `auth_providers` VALUES (1,'email','이메일',1,1),(2,'kakao','카카오',1,2),(3,'google','구글',1,3),(4,'apple','애플',1,4);
/*!40000 ALTER TABLE `auth_providers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `auth_roles`
--

DROP TABLE IF EXISTS `auth_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auth_roles` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_auth_roles_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 역할 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `auth_roles`
--

LOCK TABLES `auth_roles` WRITE;
/*!40000 ALTER TABLE `auth_roles` DISABLE KEYS */;
INSERT INTO `auth_roles` VALUES (1,'guest','게스트',1,1),(2,'user','일반 사용자',1,2),(3,'admin','관리자',1,3);
/*!40000 ALTER TABLE `auth_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `bank_account`
--

DROP TABLE IF EXISTS `bank_account`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bank_account`
--

LOCK TABLES `bank_account` WRITE;
/*!40000 ALTER TABLE `bank_account` DISABLE KEYS */;
/*!40000 ALTER TABLE `bank_account` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chat_messages`
--

DROP TABLE IF EXISTS `chat_messages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chat_messages`
--

LOCK TABLES `chat_messages` WRITE;
/*!40000 ALTER TABLE `chat_messages` DISABLE KEYS */;
/*!40000 ALTER TABLE `chat_messages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chat_room_statuses`
--

DROP TABLE IF EXISTS `chat_room_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chat_room_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(16) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_chat_room_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='채팅방 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chat_room_statuses`
--

LOCK TABLES `chat_room_statuses` WRITE;
/*!40000 ALTER TABLE `chat_room_statuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `chat_room_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chat_rooms`
--

DROP TABLE IF EXISTS `chat_rooms`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chat_rooms`
--

LOCK TABLES `chat_rooms` WRITE;
/*!40000 ALTER TABLE `chat_rooms` DISABLE KEYS */;
/*!40000 ALTER TABLE `chat_rooms` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dispute_evidence`
--

DROP TABLE IF EXISTS `dispute_evidence`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dispute_evidence`
--

LOCK TABLES `dispute_evidence` WRITE;
/*!40000 ALTER TABLE `dispute_evidence` DISABLE KEYS */;
/*!40000 ALTER TABLE `dispute_evidence` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dispute_statuses`
--

DROP TABLE IF EXISTS `dispute_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dispute_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_dispute_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='분쟁 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dispute_statuses`
--

LOCK TABLES `dispute_statuses` WRITE;
/*!40000 ALTER TABLE `dispute_statuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `dispute_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dispute_types`
--

DROP TABLE IF EXISTS `dispute_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dispute_types` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_dispute_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='분쟁 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dispute_types`
--

LOCK TABLES `dispute_types` WRITE;
/*!40000 ALTER TABLE `dispute_types` DISABLE KEYS */;
/*!40000 ALTER TABLE `dispute_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `disputes`
--

DROP TABLE IF EXISTS `disputes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `disputes`
--

LOCK TABLES `disputes` WRITE;
/*!40000 ALTER TABLE `disputes` DISABLE KEYS */;
/*!40000 ALTER TABLE `disputes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `escrow`
--

DROP TABLE IF EXISTS `escrow`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `escrow`
--

LOCK TABLES `escrow` WRITE;
/*!40000 ALTER TABLE `escrow` DISABLE KEYS */;
/*!40000 ALTER TABLE `escrow` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `escrow_statuses`
--

DROP TABLE IF EXISTS `escrow_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `escrow_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_escrow_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='에스크로 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `escrow_statuses`
--

LOCK TABLES `escrow_statuses` WRITE;
/*!40000 ALTER TABLE `escrow_statuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `escrow_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `event_schedules`
--

DROP TABLE IF EXISTS `event_schedules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_schedules`
--

LOCK TABLES `event_schedules` WRITE;
/*!40000 ALTER TABLE `event_schedules` DISABLE KEYS */;
INSERT INTO `event_schedules` VALUES ('SCH001',1,'2026-01-28','19:00:00',1,'2026-01-14 00:35:57'),('SCH002',2,'2026-02-23','18:00:00',1,'2026-01-14 00:35:57'),('SCH003',3,'2026-08-02','17:00:00',1,'2026-01-14 00:35:57'),('SCH004',4,'2026-03-14','18:00:00',1,'2026-01-14 00:35:57'),('SCH005',5,'2026-04-18','19:00:00',1,'2026-01-14 00:35:57'),('SCH006',6,'2026-10-28','19:00:00',1,'2026-01-14 00:35:57'),('SCH007',7,'2026-03-14','14:00:00',1,'2026-01-14 00:35:57'),('SCH008',8,'2026-04-23','19:30:00',1,'2026-01-14 00:35:57'),('SCH009',9,'2026-05-28','19:00:00',1,'2026-01-14 00:35:57'),('SCH010',10,'2026-07-03','19:00:00',1,'2026-01-14 00:35:57'),('SCH011',11,'2026-04-18','14:00:00',1,'2026-01-14 00:35:57'),('SCH012',12,'2026-04-25','18:30:00',1,'2026-01-14 00:35:57'),('SCH013',13,'2026-05-23','19:00:00',1,'2026-01-14 00:35:57'),('SCH014',14,'2026-11-28','18:00:00',1,'2026-01-14 00:35:57'),('SCH015',15,'2026-06-18','20:00:00',1,'2026-01-14 00:35:57'),('SCH016',16,'2026-01-14','10:00:00',1,'2026-01-14 00:35:57'),('SCH017',17,'2026-03-14','10:00:00',1,'2026-01-14 00:35:57'),('SCH018',18,'2026-04-14','10:00:00',1,'2026-01-14 00:35:57');
/*!40000 ALTER TABLE `event_schedules` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `event_seat_areas`
--

DROP TABLE IF EXISTS `event_seat_areas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `event_seat_areas` (
  `id` int NOT NULL AUTO_INCREMENT,
  `event_id` int NOT NULL,
  `area_name` varchar(50) NOT NULL COMMENT '구역명 (F1, 1구역 등)',
  `is_active` tinyint(1) DEFAULT '1',
  `sort_order` int DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_event` (`event_id`),
  CONSTRAINT `event_seat_areas_ibfk_1` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연별 좌석 구역';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_seat_areas`
--

LOCK TABLES `event_seat_areas` WRITE;
/*!40000 ALTER TABLE `event_seat_areas` DISABLE KEYS */;
INSERT INTO `event_seat_areas` VALUES (1,1,'A구역',1,1,'2026-01-17 09:12:04'),(2,1,'B구역',1,2,'2026-01-17 09:12:04');
/*!40000 ALTER TABLE `event_seat_areas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `event_seat_grades`
--

DROP TABLE IF EXISTS `event_seat_grades`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `event_seat_grades` (
  `id` int NOT NULL AUTO_INCREMENT,
  `event_id` int NOT NULL,
  `seat_grade_id` int NOT NULL,
  `code` varchar(50) NOT NULL,
  `name_ko` varchar(100) NOT NULL,
  `name_en` varchar(100) DEFAULT NULL,
  `original_price` int DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `sort_order` int DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_event_grade` (`event_id`,`seat_grade_id`),
  KEY `seat_grade_id` (`seat_grade_id`),
  CONSTRAINT `event_seat_grades_ibfk_1` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`) ON DELETE CASCADE,
  CONSTRAINT `event_seat_grades_ibfk_2` FOREIGN KEY (`seat_grade_id`) REFERENCES `seat_grades` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연별 좌석 등급 매핑';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_seat_grades`
--

LOCK TABLES `event_seat_grades` WRITE;
/*!40000 ALTER TABLE `event_seat_grades` DISABLE KEYS */;
INSERT INTO `event_seat_grades` VALUES (1,1,1,'REG','일반석','Regular',200000,1,1,'2026-01-17 09:12:04'),(2,1,2,'VIP','VIP석','VIP',100000,1,2,'2026-01-17 09:12:04');
/*!40000 ALTER TABLE `event_seat_grades` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `event_seat_locations`
--

DROP TABLE IF EXISTS `event_seat_locations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `event_seat_locations` (
  `id` int NOT NULL AUTO_INCREMENT,
  `event_id` int NOT NULL,
  `location_name` varchar(50) NOT NULL COMMENT '위치명 (플로어석, 1층 등)',
  `is_active` tinyint(1) DEFAULT '1',
  `sort_order` int DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_event` (`event_id`),
  CONSTRAINT `event_seat_locations_ibfk_1` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연별 좌석 위치';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_seat_locations`
--

LOCK TABLES `event_seat_locations` WRITE;
/*!40000 ALTER TABLE `event_seat_locations` DISABLE KEYS */;
INSERT INTO `event_seat_locations` VALUES (1,1,'1층',1,1,'2026-01-17 09:12:04'),(2,1,'2층',1,2,'2026-01-17 09:12:04');
/*!40000 ALTER TABLE `event_seat_locations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `events`
--

DROP TABLE IF EXISTS `events`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `events`
--

LOCK TABLES `events` WRITE;
/*!40000 ALTER TABLE `events` DISABLE KEYS */;
INSERT INTO `events` VALUES (1,1,1,'2024 월드 투어 서울','아이유의 2024 월드 투어 서울 공연','https://picsum.photos/400/600?random=1','올림픽공원 체조경기장','서울시 송파구 올림픽로 424','2026-01-28 19:00:00','2026-01-28 22:00:00',NULL,1,1,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(2,1,2,'Bunnies Camp 2024','뉴진스 팬미팅','https://picsum.photos/400/600?random=2','고척스카이돔','서울시 구로구 경인로 430','2026-02-23 18:00:00','2026-02-23 21:00:00',NULL,1,2,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(3,1,3,'흠뻑쇼 2024 - SUMMER SWAG','싸이의 여름 물총 축제','https://picsum.photos/400/600?random=3','잠실종합운동장','서울시 송파구 올림픽로 25','2026-08-02 17:00:00','2026-08-02 22:00:00',NULL,1,3,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(4,1,4,'IM HERO 앙코르 콘서트','임영웅 앙코르 콘서트','https://picsum.photos/400/600?random=4','KSPO돔','서울시 송파구 올림픽로 424','2026-03-14 18:00:00','2026-03-14 21:00:00',NULL,1,4,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(5,1,5,'Welcome to the Show','데이식스 콘서트','https://picsum.photos/400/600?random=5','블루스퀘어 마스터카드홀','서울시 용산구 이태원로 294','2026-04-18 19:00:00','2026-04-18 22:00:00',NULL,1,5,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(6,1,6,'BTS Yet To Come','BTS 부산 콘서트','https://picsum.photos/400/600?random=6','부산아시아드주경기장','부산시 연제구 월드컵대로 344','2026-10-28 19:00:00','2026-10-28 22:00:00',NULL,1,6,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(7,3,NULL,'위키드 (WICKED)','마법의 나라 오즈에서 펼쳐지는 두 마녀의 우정 이야기','https://picsum.photos/400/600?random=20','블루스퀘어 신한카드홀','서울시 용산구 이태원로 294','2026-03-14 14:00:00','2026-03-14 17:00:00',NULL,1,1,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(8,3,NULL,'지킬앤하이드','조승우 주연의 지킬앤하이드 공연','https://picsum.photos/400/600?random=21','예술의전당 오페라극장','서울시 서초구 남부순환로 2406','2026-04-23 19:30:00','2026-04-23 22:30:00',NULL,1,2,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(9,3,NULL,'엘리자벳','오스트리아 황후 엘리자벳의 이야기','https://picsum.photos/400/600?random=22','샤롯데씨어터','서울시 송파구 잠실로 240','2026-05-28 19:00:00','2026-05-28 22:00:00',NULL,1,3,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(10,3,NULL,'알라딘','디즈니 뮤지컬 알라딘','https://picsum.photos/400/600?random=23','디큐브아트센터','서울시 구로구 경인로 662','2026-07-03 19:00:00','2026-07-03 21:30:00',NULL,1,4,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(11,2,NULL,'2025 KBO 시즌 - KIA vs 두산','KBO 리그 정규시즌 경기','https://picsum.photos/400/600?random=30','광주 기아 챔피언스필드','광주시 북구 서림로 10','2026-04-18 14:00:00','2026-04-18 17:00:00',NULL,1,1,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(12,2,NULL,'2025 KBO 시즌 - 두산 홈경기','KBO 리그 두산 베어스 홈경기','https://picsum.photos/400/600?random=31','잠실야구장','서울시 송파구 올림픽로 25','2026-04-25 18:30:00','2026-04-25 21:30:00',NULL,1,2,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(13,2,NULL,'2025 K리그 - FC서울 홈경기','K리그 정규시즌 FC서울 홈경기','https://picsum.photos/400/600?random=32','서울월드컵경기장','서울시 마포구 월드컵로 240','2026-05-23 19:00:00','2026-05-23 21:00:00',NULL,1,3,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(14,2,NULL,'2025 KBL - 서울 삼성 vs SK','프로농구 정규시즌 경기','https://picsum.photos/400/600?random=33','잠실실내체육관','서울시 송파구 올림픽로 25','2026-11-28 18:00:00','2026-11-28 20:00:00',NULL,1,4,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(15,2,NULL,'손흥민 친선 경기','대한민국 vs 일본 친선경기','https://picsum.photos/400/600?random=34','서울월드컵경기장','서울시 마포구 월드컵로 240','2026-06-18 20:00:00','2026-06-18 22:00:00',NULL,1,5,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(16,4,NULL,'반 고흐 인사이드','빛의 시어터에서 만나는 반 고흐','https://picsum.photos/400/600?random=40','빛의 시어터 제주','제주시 애월읍 어음리 1942','2026-01-14 10:00:00','2026-07-13 20:00:00',NULL,1,1,'2025-12-18 04:02:19','2026-01-14 05:26:08'),(17,4,NULL,'팀랩 보더리스','디지털 아트 뮤지엄','https://picsum.photos/400/600?random=41','잠실 롯데월드타워','서울시 송파구 올림픽로 300','2026-03-14 10:00:00','2027-01-13 21:00:00',NULL,1,2,'2025-12-18 04:02:19','2026-01-14 05:26:08'),(18,4,NULL,'모네: 빛을 그리다','인상파 거장 모네 특별전','https://picsum.photos/400/600?random=42','예술의전당 한가람미술관','서울시 서초구 남부순환로 2406','2026-04-14 10:00:00','2026-08-13 19:00:00',NULL,1,3,'2025-12-18 04:02:19','2026-01-14 05:26:08');
/*!40000 ALTER TABLE `events` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `favorite_types`
--

DROP TABLE IF EXISTS `favorite_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `favorite_types` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_favorite_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='찜 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `favorite_types`
--

LOCK TABLES `favorite_types` WRITE;
/*!40000 ALTER TABLE `favorite_types` DISABLE KEYS */;
INSERT INTO `favorite_types` VALUES (1,'event','공연',1,1),(2,'ticket','티켓',1,2);
/*!40000 ALTER TABLE `favorite_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notification_platforms`
--

DROP TABLE IF EXISTS `notification_platforms`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notification_platforms` (
  `id` bigint NOT NULL,
  `code` varchar(16) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_notification_platforms_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 플랫폼 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification_platforms`
--

LOCK TABLES `notification_platforms` WRITE;
/*!40000 ALTER TABLE `notification_platforms` DISABLE KEYS */;
/*!40000 ALTER TABLE `notification_platforms` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notification_token`
--

DROP TABLE IF EXISTS `notification_token`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification_token`
--

LOCK TABLES `notification_token` WRITE;
/*!40000 ALTER TABLE `notification_token` DISABLE KEYS */;
/*!40000 ALTER TABLE `notification_token` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notification_types`
--

DROP TABLE IF EXISTS `notification_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notification_types` (
  `id` bigint NOT NULL,
  `code` varchar(64) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_notification_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification_types`
--

LOCK TABLES `notification_types` WRITE;
/*!40000 ALTER TABLE `notification_types` DISABLE KEYS */;
/*!40000 ALTER TABLE `notification_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_methods`
--

DROP TABLE IF EXISTS `payment_methods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_methods` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_payment_methods_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 수단 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_methods`
--

LOCK TABLES `payment_methods` WRITE;
/*!40000 ALTER TABLE `payment_methods` DISABLE KEYS */;
/*!40000 ALTER TABLE `payment_methods` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_statuses`
--

DROP TABLE IF EXISTS `payment_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_payment_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_statuses`
--

LOCK TABLES `payment_statuses` WRITE;
/*!40000 ALTER TABLE `payment_statuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `payment_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payments`
--

LOCK TABLES `payments` WRITE;
/*!40000 ALTER TABLE `payments` DISABLE KEYS */;
/*!40000 ALTER TABLE `payments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `refresh_tokens`
--

DROP TABLE IF EXISTS `refresh_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
) ENGINE=InnoDB AUTO_INCREMENT=168 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Refresh Token 저장 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refresh_tokens`
--

LOCK TABLES `refresh_tokens` WRITE;
/*!40000 ALTER TABLE `refresh_tokens` DISABLE KEYS */;
INSERT INTO `refresh_tokens` VALUES (1,12,'0c1d7846-5ec0-4201-b8ac-d6e99e9dcd4b','2026-01-19 00:54:21','2026-01-11 15:54:21',0,NULL,NULL),(2,12,'5a270563-1633-4a9f-a0cf-b40eb9efc8dc','2026-01-19 03:44:25','2026-01-11 18:44:25',0,NULL,NULL),(3,12,'45585bc9-570d-493f-9b70-6a9c85dcb8ce','2026-01-19 03:44:39','2026-01-11 18:44:39',0,NULL,NULL),(4,12,'9b554031-1d72-4763-b661-f7fb000fc59f','2026-01-19 03:46:06','2026-01-11 18:46:06',0,NULL,NULL),(5,12,'91af2324-73d8-438f-a91e-d99de41f90bd','2026-01-19 03:47:54','2026-01-11 18:47:54',0,NULL,NULL),(6,12,'0f0ecf18-75c0-4c77-963c-f782650638ca','2026-01-19 04:34:27','2026-01-11 19:34:27',0,NULL,NULL),(7,12,'f8a38c28-3aeb-47f4-a1e7-bb55f42b49aa','2026-01-19 04:36:25','2026-01-11 19:36:25',0,NULL,NULL),(8,12,'2d8f10dc-882b-43ee-9f8c-50aa975e28e8','2026-01-19 04:36:35','2026-01-11 19:36:35',0,NULL,NULL),(9,12,'61b1b3d3-adcc-4677-827c-94478a59a20a','2026-01-19 04:36:49','2026-01-11 19:36:49',1,'2026-01-12 04:37:51','9a1075aa-938d-4970-ba0c-b21c109a49b3'),(10,12,'9a1075aa-938d-4970-ba0c-b21c109a49b3','2026-01-19 04:37:51','2026-01-11 19:37:51',0,NULL,NULL),(11,12,'cc83b930-3491-4063-89db-c3ac369e1565','2026-01-19 04:42:01','2026-01-11 19:42:01',1,'2026-01-12 04:42:17','1c61d427-f253-4139-9399-a8e4c3497126'),(12,12,'1c61d427-f253-4139-9399-a8e4c3497126','2026-01-19 04:42:18','2026-01-11 19:42:18',0,NULL,NULL),(13,12,'ba01bdf3-915b-4e21-b1ff-bde069a69d5d','2026-01-19 05:39:17','2026-01-11 20:39:17',0,NULL,NULL),(14,12,'3671a52d-ae6e-4ac5-a534-65ce6ecd46f1','2026-01-19 05:52:43','2026-01-11 20:52:43',0,NULL,NULL),(15,12,'de7681bb-002c-4508-9892-14a4e16ba3de','2026-01-19 05:53:35','2026-01-11 20:53:35',1,'2026-01-12 06:10:19','c9ba8b1d-a939-4b9e-9d70-e56063d7b017'),(16,12,'c9ba8b1d-a939-4b9e-9d70-e56063d7b017','2026-01-19 06:10:19','2026-01-11 21:10:19',1,'2026-01-12 07:07:35','35ffa890-fa40-4d86-93e7-b60432da7f28'),(17,12,'35ffa890-fa40-4d86-93e7-b60432da7f28','2026-01-19 07:07:35','2026-01-11 22:07:35',1,'2026-01-12 07:22:54','537f1d00-a2ac-446e-ac56-6fa5b25e27fd'),(18,12,'dc2009f4-9acc-4f30-8516-545c8c7b58e6','2026-01-19 07:08:54','2026-01-11 22:08:54',0,NULL,NULL),(19,12,'537f1d00-a2ac-446e-ac56-6fa5b25e27fd','2026-01-19 07:22:54','2026-01-11 22:22:54',1,'2026-01-12 07:44:35','5ccc72f7-afc8-4067-9353-448a9b6dc2de'),(20,12,'8eb170cf-f30e-4f3f-a7fb-c19863d0201e','2026-01-19 07:35:24','2026-01-11 22:35:24',0,NULL,NULL),(21,12,'6dec184f-0652-4a3f-801d-566eb45aae46','2026-01-19 07:41:46','2026-01-11 22:41:46',0,NULL,NULL),(22,12,'5ccc72f7-afc8-4067-9353-448a9b6dc2de','2026-01-19 07:44:35','2026-01-11 22:44:35',1,'2026-01-12 08:02:58','4379a2bc-22f6-42d0-b6eb-40e778add5ca'),(23,12,'4379a2bc-22f6-42d0-b6eb-40e778add5ca','2026-01-19 08:02:58','2026-01-11 23:02:58',1,'2026-01-12 23:40:31','00ea9025-6853-4f90-97a9-ed53d5a03faa'),(24,12,'00ea9025-6853-4f90-97a9-ed53d5a03faa','2026-01-19 23:40:31','2026-01-12 14:40:31',0,NULL,NULL),(25,12,'8c37f718-1b97-4a8e-8e7b-b91b6ec7fb0c','2026-01-19 23:40:31','2026-01-12 14:40:31',0,NULL,NULL),(26,12,'8a3295b7-0cee-42ae-bec5-0bd4a5bb250f','2026-01-19 23:48:55','2026-01-12 14:48:55',1,'2026-01-13 00:41:00','3b52e462-1239-4784-8d2b-8886b81c6a78'),(27,12,'3b52e462-1239-4784-8d2b-8886b81c6a78','2026-01-20 00:41:00','2026-01-12 15:41:00',1,'2026-01-13 00:41:00','f399da0c-751f-4b35-bb65-f16c3a7d6f19'),(28,12,'f399da0c-751f-4b35-bb65-f16c3a7d6f19','2026-01-20 00:41:00','2026-01-12 15:41:00',1,'2026-01-13 01:09:56','897c2d7b-3de0-4ef2-87c4-9e8919846be8'),(29,12,'3eb59bd8-3a21-4926-babd-03fbaecd1ab4','2026-01-20 01:09:56','2026-01-12 16:09:56',0,NULL,NULL),(30,12,'897c2d7b-3de0-4ef2-87c4-9e8919846be8','2026-01-20 01:09:56','2026-01-12 16:09:56',1,'2026-01-13 01:34:18','5004683e-71e3-4b9d-bc56-0c4a44346282'),(31,12,'a6ee1f7e-503b-4f86-9bae-c68327f0b9e2','2026-01-20 01:34:18','2026-01-12 16:34:18',0,NULL,NULL),(32,12,'5004683e-71e3-4b9d-bc56-0c4a44346282','2026-01-20 01:34:18','2026-01-12 16:34:18',1,'2026-01-13 01:56:25','5af12285-9e47-4096-803d-5e6ee057d3d5'),(33,12,'7fb42627-4306-452e-bad3-47625b423b93','2026-01-20 01:56:25','2026-01-12 16:56:25',1,'2026-01-13 05:40:22','9092aed4-bcc2-4f44-91cf-2ad042258922'),(34,12,'5af12285-9e47-4096-803d-5e6ee057d3d5','2026-01-20 01:56:25','2026-01-12 16:56:25',0,NULL,NULL),(35,12,'9092aed4-bcc2-4f44-91cf-2ad042258922','2026-01-20 05:40:22','2026-01-12 20:40:22',1,'2026-01-13 06:37:32','8e5df1d8-b7ea-4565-a2de-a3e63557ea57'),(36,12,'1278f75b-b21b-4688-929d-928960d4037b','2026-01-20 05:40:22','2026-01-12 20:40:22',0,NULL,NULL),(37,12,'8e5df1d8-b7ea-4565-a2de-a3e63557ea57','2026-01-20 06:37:32','2026-01-12 21:37:32',0,NULL,NULL),(38,12,'bd6b46ed-f211-4dbb-b6cf-e4e58177148f','2026-01-20 06:37:32','2026-01-12 21:37:32',1,'2026-01-13 07:01:20','28cfa027-5107-4302-b76a-7e3ccb3433dd'),(39,12,'28cfa027-5107-4302-b76a-7e3ccb3433dd','2026-01-20 07:01:20','2026-01-12 22:01:20',0,NULL,NULL),(40,12,'1d6439b9-fb25-44d7-95a9-0757f8d16e2f','2026-01-20 07:01:20','2026-01-12 22:01:20',1,'2026-01-13 10:08:32','4a7fd49c-7e47-4be2-8d22-b71087ebdc90'),(41,12,'4a7fd49c-7e47-4be2-8d22-b71087ebdc90','2026-01-20 10:08:32','2026-01-13 01:08:32',0,NULL,NULL),(42,12,'d22435f5-00ac-4d59-87a8-db93313f03a2','2026-01-20 10:08:32','2026-01-13 01:08:32',1,'2026-01-14 03:50:28','a0440eb5-3976-4fd6-b904-eb9620dba0a9'),(43,12,'036c9877-e371-4700-81ca-571b2a421d20','2026-01-21 03:50:28','2026-01-13 18:50:28',1,'2026-01-14 04:14:35','da58f4aa-c32e-45d9-a70c-ab863b48226e'),(44,12,'a0440eb5-3976-4fd6-b904-eb9620dba0a9','2026-01-21 03:50:28','2026-01-13 18:50:28',0,NULL,NULL),(45,12,'7aeaaa68-b279-49f7-8e10-816c778bffed','2026-01-21 03:55:09','2026-01-13 18:55:09',0,NULL,NULL),(46,12,'ccbda0e3-9c81-408e-a853-408967f99fd7','2026-01-21 04:14:35','2026-01-13 19:14:35',1,'2026-01-14 04:43:32','7f612ee5-8df0-4792-b494-49bdafa2d70c'),(47,12,'da58f4aa-c32e-45d9-a70c-ab863b48226e','2026-01-21 04:14:35','2026-01-13 19:14:35',0,NULL,NULL),(48,12,'7d71c5d6-6f23-446e-b010-323a7d419840','2026-01-21 04:30:48','2026-01-13 19:30:48',0,NULL,NULL),(49,12,'7f612ee5-8df0-4792-b494-49bdafa2d70c','2026-01-21 04:43:32','2026-01-13 19:43:32',0,NULL,NULL),(50,12,'7e03b541-29d4-403b-8c00-baaa93104a71','2026-01-21 04:43:32','2026-01-13 19:43:32',1,'2026-01-14 05:14:08','e1319baf-40b2-46de-8b09-65dd141386d0'),(51,12,'e1319baf-40b2-46de-8b09-65dd141386d0','2026-01-21 05:14:08','2026-01-13 20:14:08',1,'2026-01-14 05:35:49','00860384-54d9-483c-b56c-6ae50b78b4fa'),(52,12,'3c077954-a109-461c-8309-3426f7d9779f','2026-01-21 05:14:08','2026-01-13 20:14:08',0,NULL,NULL),(53,12,'119fe39a-db8f-4358-9baf-5adec5f6d0cd','2026-01-21 05:29:10','2026-01-13 20:29:10',0,NULL,NULL),(54,12,'00860384-54d9-483c-b56c-6ae50b78b4fa','2026-01-21 05:35:49','2026-01-13 20:35:49',0,NULL,NULL),(55,12,'d460fb31-14b0-47e2-939d-5e8f6b9637a2','2026-01-21 05:35:49','2026-01-13 20:35:49',1,'2026-01-14 06:31:08','919f8ac6-01cb-4e55-91a2-c9c35cc8c3d7'),(56,12,'5434485c-f4fb-44a1-ae4a-309324cfb80a','2026-01-21 06:31:08','2026-01-13 21:31:08',0,NULL,NULL),(57,12,'919f8ac6-01cb-4e55-91a2-c9c35cc8c3d7','2026-01-21 06:31:08','2026-01-13 21:31:08',1,'2026-01-15 00:20:16','cd27ea3f-647b-4636-861a-58d1e911c330'),(58,12,'8f3ac0a2-8cea-40ae-9280-9eb71147cb59','2026-01-22 00:20:16','2026-01-14 15:20:16',0,NULL,NULL),(59,12,'cd27ea3f-647b-4636-861a-58d1e911c330','2026-01-22 00:20:16','2026-01-14 15:20:16',1,'2026-01-15 00:36:38','64d5cecc-db76-4ab5-bfe4-b2c8f697d4ed'),(60,12,'330baf4e-af9b-421e-92aa-913c2673e77d','2026-01-22 00:36:38','2026-01-14 15:36:38',0,NULL,NULL),(61,12,'64d5cecc-db76-4ab5-bfe4-b2c8f697d4ed','2026-01-22 00:36:38','2026-01-14 15:36:38',1,'2026-01-15 00:55:13','be77ba88-8425-484a-88f3-ba0910d97cc4'),(62,12,'93ecde18-431a-4698-aa62-1fa5681a268a','2026-01-22 00:55:14','2026-01-14 15:55:14',0,NULL,NULL),(63,12,'be77ba88-8425-484a-88f3-ba0910d97cc4','2026-01-22 00:55:14','2026-01-14 15:55:14',1,'2026-01-15 01:12:08','7a07826d-22dc-40de-b13e-764a6c38894f'),(64,12,'7a07826d-22dc-40de-b13e-764a6c38894f','2026-01-22 01:12:08','2026-01-14 16:12:08',0,NULL,NULL),(65,12,'da56d313-7564-45df-8e5b-70da747e867a','2026-01-22 01:12:08','2026-01-14 16:12:08',1,'2026-01-15 01:53:15','d0679a76-5606-4e12-bfd1-05b1280d7530'),(66,12,'241a75bc-5df4-49b6-bbf4-c3bd2b3f8a31','2026-01-22 01:53:15','2026-01-14 16:53:15',0,NULL,NULL),(67,12,'d0679a76-5606-4e12-bfd1-05b1280d7530','2026-01-22 01:53:15','2026-01-14 16:53:15',1,'2026-01-15 02:22:52','4837bb67-f3d4-433a-b823-85da463f544f'),(68,12,'eecb64f2-f0b6-4862-bf67-f30c15e5e4a0','2026-01-22 02:22:52','2026-01-14 17:22:52',0,NULL,NULL),(69,12,'4837bb67-f3d4-433a-b823-85da463f544f','2026-01-22 02:22:52','2026-01-14 17:22:52',1,'2026-01-15 03:18:04','8f77cfce-ed64-426e-b03e-bb3bd360d35d'),(70,12,'79cb8e0a-6779-4a53-974f-3d066792462e','2026-01-22 03:18:04','2026-01-14 18:18:04',0,NULL,NULL),(71,12,'8f77cfce-ed64-426e-b03e-bb3bd360d35d','2026-01-22 03:18:04','2026-01-14 18:18:04',0,NULL,NULL),(72,12,'a8a7b210-cde4-412b-81e5-c29c87dfa9fe','2026-01-22 03:40:27','2026-01-14 18:40:27',1,'2026-01-15 04:07:44','2e30fed6-7509-4c98-8eb0-c14201809472'),(73,12,'afaa60db-5668-4258-819a-37c553fdff21','2026-01-22 04:07:44','2026-01-14 19:07:44',0,NULL,NULL),(74,12,'2e30fed6-7509-4c98-8eb0-c14201809472','2026-01-22 04:07:44','2026-01-14 19:07:44',0,NULL,NULL),(75,12,'bb221f56-fa42-4c72-ba48-ae6caf097f56','2026-01-22 04:17:43','2026-01-14 19:17:43',1,'2026-01-15 05:07:36','23b747b1-d261-474b-a772-408219516de7'),(76,12,'c91b6368-8e2a-46cb-9206-f48c16bdbcc2','2026-01-22 05:07:36','2026-01-14 20:07:36',0,NULL,NULL),(77,12,'bc42eb5f-829b-4fa9-9b01-ade807f14bc9','2026-01-22 05:07:36','2026-01-14 20:07:36',0,NULL,NULL),(78,12,'23b747b1-d261-474b-a772-408219516de7','2026-01-22 05:07:36','2026-01-14 20:07:36',0,NULL,NULL),(79,15,'ce400efd-5243-4ec7-86cc-801b17fde341','2026-01-22 05:10:04','2026-01-14 20:10:04',0,NULL,NULL),(80,12,'c8c43997-479a-4c22-a61d-2982dcec6b65','2026-01-22 05:10:52','2026-01-14 20:10:52',0,NULL,NULL),(81,12,'2013c6fe-1307-4e47-b74b-5a5764f5b84a','2026-01-22 05:11:48','2026-01-14 20:11:48',0,NULL,NULL),(82,12,'5a7f2d76-1cdb-4abd-98b7-f3d7b698ee75','2026-01-22 05:24:12','2026-01-14 20:24:12',0,NULL,NULL),(83,15,'8e562d97-09b0-47f6-bcf3-590a4912ddb6','2026-01-22 05:25:04','2026-01-14 20:25:04',0,NULL,NULL),(84,12,'28ba62d0-268d-4b18-bae2-65bb5d5c1be9','2026-01-22 05:28:24','2026-01-14 20:28:24',0,NULL,NULL),(85,15,'b6e72ec0-fa37-4637-91a1-e1ff6e72c81a','2026-01-22 05:31:36','2026-01-14 20:31:36',0,NULL,NULL),(86,12,'fdf32d8e-8d10-47f0-9bbd-3626bf68c745','2026-01-22 05:31:53','2026-01-14 20:31:53',0,NULL,NULL),(87,12,'ebb0b913-7171-4a8c-973c-0a0e714bdd44','2026-01-22 05:32:14','2026-01-14 20:32:14',0,NULL,NULL),(88,12,'13871a7f-467d-4a69-9a85-51c2610b4cbc','2026-01-22 05:33:01','2026-01-14 20:33:01',0,NULL,NULL),(89,12,'9015c088-2f37-4001-adfe-d65c5313c8f9','2026-01-22 05:34:45','2026-01-14 20:34:45',0,NULL,NULL),(90,15,'9f7b5f65-293b-4204-a2f4-e91e7aa1d658','2026-01-22 05:34:59','2026-01-14 20:34:59',0,NULL,NULL),(91,12,'cea1c403-8065-4c83-b12b-11071f00482c','2026-01-22 05:39:34','2026-01-14 20:39:34',0,NULL,NULL),(92,15,'910c8a90-f81b-4c5b-b48b-51b71ca09acb','2026-01-22 05:39:46','2026-01-14 20:39:46',1,'2026-01-15 06:14:07','86f25868-5943-4b8e-b25e-faf534bdca7c'),(93,15,'d6b2905d-5156-479b-aa7b-e9bef8f28552','2026-01-22 06:14:07','2026-01-14 21:14:07',1,'2026-01-15 06:52:25','1a3a9b62-62d1-45e6-8a05-555c3596f4f1'),(94,15,'86f25868-5943-4b8e-b25e-faf534bdca7c','2026-01-22 06:14:07','2026-01-14 21:14:07',0,NULL,NULL),(95,15,'f07d4939-469d-4e88-8e16-853b87d45d18','2026-01-22 06:14:07','2026-01-14 21:14:07',0,NULL,NULL),(96,15,'90caac37-405c-4d53-b75e-6e2cfd5752f8','2026-01-22 06:52:25','2026-01-14 21:52:25',0,NULL,NULL),(97,15,'1a3a9b62-62d1-45e6-8a05-555c3596f4f1','2026-01-22 06:52:25','2026-01-14 21:52:25',0,NULL,NULL),(98,12,'6a42953c-bfda-4049-8be5-0af7dd633108','2026-01-22 07:19:07','2026-01-14 22:19:07',1,'2026-01-15 07:46:13','a0f32646-25d7-4ccf-b444-63290225410d'),(99,12,'1d9d5974-4043-4e41-bd76-fb68386cd7a8','2026-01-22 07:46:13','2026-01-14 22:46:13',1,'2026-01-15 08:07:16','179001e5-1284-4054-8085-0efb4c46c930'),(100,12,'4c3f09b9-c58c-4009-a6be-2b411ea14f61','2026-01-22 07:46:13','2026-01-14 22:46:13',0,NULL,NULL),(101,12,'a0f32646-25d7-4ccf-b444-63290225410d','2026-01-22 07:46:13','2026-01-14 22:46:13',0,NULL,NULL),(102,12,'179001e5-1284-4054-8085-0efb4c46c930','2026-01-22 08:07:16','2026-01-14 23:07:16',0,NULL,NULL),(103,12,'a5d64daf-dbc1-4edc-98c6-b2957fcae3cc','2026-01-22 08:07:40','2026-01-14 23:07:40',1,'2026-01-15 08:28:34','f34df0e9-5879-47fa-9fac-a7d83c4326bd'),(104,12,'0e662be2-3311-4d4d-b6b2-45ddced16f7e','2026-01-22 08:28:35','2026-01-14 23:28:35',0,NULL,NULL),(105,12,'11a08b14-f8f5-4b58-8476-cdad5161dd20','2026-01-22 08:28:35','2026-01-14 23:28:35',1,'2026-01-15 08:47:42','4c0a7ad9-323f-4de4-8195-11872f620486'),(106,12,'f34df0e9-5879-47fa-9fac-a7d83c4326bd','2026-01-22 08:28:35','2026-01-14 23:28:35',0,NULL,NULL),(107,12,'0daa8ca7-f03b-4779-b359-2a8b08f80208','2026-01-22 08:47:42','2026-01-14 23:47:42',0,NULL,NULL),(108,12,'7ea4c31f-252d-49bc-a3fc-ce3129840feb','2026-01-22 08:47:42','2026-01-14 23:47:42',1,'2026-01-15 09:57:01','b70f9360-a851-4863-8a79-805dcbb87758'),(109,12,'4c0a7ad9-323f-4de4-8195-11872f620486','2026-01-22 08:47:42','2026-01-14 23:47:42',0,NULL,NULL),(110,12,'b70f9360-a851-4863-8a79-805dcbb87758','2026-01-22 09:57:01','2026-01-15 00:57:01',0,NULL,NULL),(111,12,'c9173c7d-cc0e-47a7-b720-290488d7012c','2026-01-22 09:57:25','2026-01-15 00:57:25',1,'2026-01-15 10:13:30','259faba8-95d8-484d-93c3-cc7396ef7579'),(112,12,'9f6ef607-ac53-4cce-a8c6-bf2d6743bb66','2026-01-22 10:13:30','2026-01-15 01:13:30',1,'2026-01-16 00:10:05','d6cc6cec-5139-4a70-8d48-033ff24b8e0b'),(113,12,'259faba8-95d8-484d-93c3-cc7396ef7579','2026-01-22 10:13:30','2026-01-15 01:13:30',0,NULL,NULL),(114,12,'d68a4f71-6b9a-4e8c-98b4-650c5ada1a10','2026-01-22 10:13:30','2026-01-15 01:13:30',0,NULL,NULL),(115,12,'be010b3c-e665-4cb0-90bf-733312df463f','2026-01-23 00:10:05','2026-01-15 15:10:05',1,'2026-01-16 00:33:06','ed1eb18c-405e-42e4-bc7b-e8f871d3c3be'),(116,12,'d6cc6cec-5139-4a70-8d48-033ff24b8e0b','2026-01-23 00:10:05','2026-01-15 15:10:05',0,NULL,NULL),(117,12,'28632555-7a93-4c53-a5b5-e90b8ca327fa','2026-01-23 00:10:05','2026-01-15 15:10:05',0,NULL,NULL),(118,12,'8dd21c58-9efa-4c81-b780-937ae2698209','2026-01-23 00:33:06','2026-01-15 15:33:06',0,NULL,NULL),(119,12,'ed1eb18c-405e-42e4-bc7b-e8f871d3c3be','2026-01-23 00:33:06','2026-01-15 15:33:06',0,NULL,NULL),(120,12,'0936006c-ecc7-4534-9086-7df92b4fb0ad','2026-01-23 00:33:06','2026-01-15 15:33:06',1,'2026-01-16 01:06:24','11acb35a-7f81-4bb1-b302-48382d5d1bb1'),(121,12,'11acb35a-7f81-4bb1-b302-48382d5d1bb1','2026-01-23 01:06:24','2026-01-15 16:06:24',1,'2026-01-16 01:27:46','43814c6d-5f3f-44f3-a3ad-7a333ec10199'),(122,12,'43814c6d-5f3f-44f3-a3ad-7a333ec10199','2026-01-23 01:27:46','2026-01-15 16:27:46',1,'2026-01-16 01:55:35','f7040a2e-082f-4e3c-89c7-2d1ed644f7fb'),(123,12,'f7040a2e-082f-4e3c-89c7-2d1ed644f7fb','2026-01-23 01:55:35','2026-01-15 16:55:35',0,NULL,NULL),(124,12,'79f50d04-cff7-496d-b11e-22825abc9e99','2026-01-23 03:23:31','2026-01-15 18:23:31',1,'2026-01-16 03:43:47','9a4710b2-deb2-4106-86f5-525a294dc829'),(125,12,'9a4710b2-deb2-4106-86f5-525a294dc829','2026-01-23 03:43:47','2026-01-15 18:43:47',1,'2026-01-16 04:01:35','5a568481-3fe3-4021-891f-bde0629d3603'),(126,12,'5a568481-3fe3-4021-891f-bde0629d3603','2026-01-23 04:01:35','2026-01-15 19:01:35',1,'2026-01-16 04:22:48','9c869f09-0d9e-4113-9cc1-d4f5f0b0b838'),(127,12,'9c869f09-0d9e-4113-9cc1-d4f5f0b0b838','2026-01-23 04:22:48','2026-01-15 19:22:48',1,'2026-01-16 05:11:49','bd291ac0-0c63-4e41-a6b9-bdecebab542f'),(128,12,'9d92eb52-be86-49f1-a54c-746c3d7d64bd','2026-01-23 05:11:49','2026-01-15 20:11:49',0,NULL,NULL),(129,12,'82df70dc-7404-4c72-b507-b2f583adf7e9','2026-01-23 05:11:49','2026-01-15 20:11:49',0,NULL,NULL),(130,12,'bd291ac0-0c63-4e41-a6b9-bdecebab542f','2026-01-23 05:11:49','2026-01-15 20:11:49',0,NULL,NULL),(131,12,'3b43dd7a-c565-4c05-8747-1eef4d210a45','2026-01-23 05:12:16','2026-01-15 20:12:16',1,'2026-01-16 06:21:41','20ef8838-c2b3-4147-bcb6-47ca4e6ef654'),(132,12,'ef54a6a8-92ba-4639-90dc-3212dfc55183','2026-01-23 06:21:41','2026-01-15 21:21:41',0,NULL,NULL),(133,12,'20ef8838-c2b3-4147-bcb6-47ca4e6ef654','2026-01-23 06:21:41','2026-01-15 21:21:41',0,NULL,NULL),(134,12,'53e8d16c-bd74-45f4-8044-f078d8353828','2026-01-23 06:31:56','2026-01-15 21:31:56',0,NULL,NULL),(135,12,'2f2d6b20-f2ec-4721-ad63-974578603b24','2026-01-23 07:31:02','2026-01-15 22:31:02',1,'2026-01-16 07:49:28','72969867-d9e0-4f10-b7b3-e7a6d0d3a03e'),(136,12,'72969867-d9e0-4f10-b7b3-e7a6d0d3a03e','2026-01-23 07:49:28','2026-01-15 22:49:28',0,NULL,NULL),(137,12,'24627e39-014d-4bc8-81c1-cbb87c131cba','2026-01-23 07:56:31','2026-01-15 22:56:31',0,NULL,NULL),(138,12,'e3d01532-616a-4c60-a77f-8421f8e08e60','2026-01-24 07:19:54','2026-01-16 22:19:54',0,NULL,NULL),(139,12,'9585cb18-607d-4b94-85c3-dfe23f19f911','2026-01-24 07:33:46','2026-01-16 22:33:46',1,'2026-01-17 07:52:26','4f1aeb05-e3b6-4d11-b9d9-b6146cb7e70d'),(140,12,'4f1aeb05-e3b6-4d11-b9d9-b6146cb7e70d','2026-01-24 07:52:26','2026-01-16 22:52:26',0,NULL,NULL),(141,12,'2582b4af-0c19-4e84-8c3c-92ace9b243a4','2026-01-24 07:52:26','2026-01-16 22:52:26',1,'2026-01-17 08:54:34','8ad40975-307e-4d43-9735-cf539ac854e6'),(142,12,'c2d38042-9aa2-4ca3-b86c-14516bfbbacb','2026-01-24 07:52:26','2026-01-16 22:52:26',0,NULL,NULL),(143,12,'d006b73f-eda9-4c77-a963-7458bffe5052','2026-01-24 08:54:34','2026-01-16 23:54:34',0,NULL,NULL),(144,12,'17652c6b-2fa0-4145-8384-30cdc57fc299','2026-01-24 08:54:34','2026-01-16 23:54:34',0,NULL,NULL),(145,12,'8ad40975-307e-4d43-9735-cf539ac854e6','2026-01-24 08:54:34','2026-01-16 23:54:34',0,NULL,NULL),(146,12,'7764c59a-8b86-4a32-a228-bda824b66088','2026-01-24 08:59:37','2026-01-16 23:59:37',1,'2026-01-17 09:29:00','440f3083-b719-436e-a359-66d1e98335b5'),(147,12,'440f3083-b719-436e-a359-66d1e98335b5','2026-01-24 09:29:00','2026-01-17 00:29:00',0,NULL,NULL),(148,12,'b9570d70-99fc-4672-860c-d16aa8f23dff','2026-01-24 09:29:00','2026-01-17 00:29:00',0,NULL,NULL),(149,12,'0c088bff-a74e-4999-90c1-77cd3eb0313e','2026-01-24 09:29:00','2026-01-17 00:29:00',0,NULL,NULL),(150,12,'e9148a2a-3aa9-41e5-a05b-7dec531f31ca','2026-01-24 09:39:38','2026-01-17 00:39:38',0,NULL,NULL),(151,12,'50be20ef-66a2-49f5-8403-1841fb0eb2ec','2026-01-26 01:13:34','2026-01-18 16:13:34',1,'2026-01-19 01:36:45','428f44c4-413b-4b35-9372-9779344d87a1'),(152,12,'8130437d-68b7-4e5a-8f6c-8dc6d9b4e6d8','2026-01-26 01:36:46','2026-01-18 16:36:46',0,NULL,NULL),(153,12,'cdaa322d-443d-412b-a558-0a4792fd7273','2026-01-26 01:36:46','2026-01-18 16:36:46',0,NULL,NULL),(154,12,'428f44c4-413b-4b35-9372-9779344d87a1','2026-01-26 01:36:46','2026-01-18 16:36:46',1,'2026-01-19 01:52:59','2612aa7c-259c-4cc0-b435-b966b2aa77a7'),(155,12,'2612aa7c-259c-4cc0-b435-b966b2aa77a7','2026-01-26 01:52:59','2026-01-18 16:52:59',1,'2026-01-19 06:56:12','12c265c3-c721-4d60-86d2-7b51904efe01'),(156,12,'5cb51ce5-36e2-4475-8d21-e9e372ecbb02','2026-01-26 05:06:29','2026-01-18 20:06:29',0,NULL,NULL),(157,12,'12c265c3-c721-4d60-86d2-7b51904efe01','2026-01-26 06:56:12','2026-01-18 21:56:12',0,NULL,NULL),(158,12,'f3f271ff-c998-44c3-9d32-588a4f223d4c','2026-01-26 06:56:12','2026-01-18 21:56:12',0,NULL,NULL),(159,12,'243e675a-e259-4e9a-8fcf-455b1dd34c18','2026-01-26 06:56:12','2026-01-18 21:56:12',0,NULL,NULL),(160,12,'ac623f0a-7f8c-4cec-93bf-e91ae5ee3913','2026-01-26 07:03:15','2026-01-18 22:03:15',1,'2026-01-19 07:18:16','d5e39b08-5b17-4d6c-b24e-7271104ea5b4'),(161,12,'d5e39b08-5b17-4d6c-b24e-7271104ea5b4','2026-01-26 07:18:16','2026-01-18 22:18:16',1,'2026-01-19 07:38:12','102e78af-c5c7-4014-bcf3-6d0478d86f36'),(162,12,'4847e13b-7cd5-4385-a384-ec2b89610085','2026-01-26 07:20:26','2026-01-18 22:20:26',0,NULL,NULL),(163,12,'102e78af-c5c7-4014-bcf3-6d0478d86f36','2026-01-26 07:38:12','2026-01-18 22:38:12',1,'2026-01-19 08:01:02','a8a42497-880d-4f36-91cf-aa4aea7db1fb'),(164,12,'8c40b7ee-bcb5-4171-a182-7c685d128727','2026-01-26 07:40:54','2026-01-18 22:40:54',0,NULL,NULL),(165,12,'90c22f0a-1b25-4daa-86d3-44f04c44e53a','2026-01-26 08:01:02','2026-01-18 23:01:02',0,NULL,NULL),(166,12,'2405b2e4-76d2-4d99-9c3c-6fb81d482465','2026-01-26 08:01:02','2026-01-18 23:01:02',0,NULL,NULL),(167,12,'a8a42497-880d-4f36-91cf-aa4aea7db1fb','2026-01-26 08:01:02','2026-01-18 23:01:02',0,NULL,NULL);
/*!40000 ALTER TABLE `refresh_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `refund_reasons`
--

DROP TABLE IF EXISTS `refund_reasons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refund_reasons` (
  `id` bigint NOT NULL,
  `code` varchar(64) NOT NULL,
  `name_ko` varchar(128) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_refund_reasons_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='환불 사유 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refund_reasons`
--

LOCK TABLES `refund_reasons` WRITE;
/*!40000 ALTER TABLE `refund_reasons` DISABLE KEYS */;
/*!40000 ALTER TABLE `refund_reasons` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `refund_statuses`
--

DROP TABLE IF EXISTS `refund_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refund_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_refund_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='환불 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refund_statuses`
--

LOCK TABLES `refund_statuses` WRITE;
/*!40000 ALTER TABLE `refund_statuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `refund_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `refunds`
--

DROP TABLE IF EXISTS `refunds`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refunds`
--

LOCK TABLES `refunds` WRITE;
/*!40000 ALTER TABLE `refunds` DISABLE KEYS */;
/*!40000 ALTER TABLE `refunds` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reputation_rating_types`
--

DROP TABLE IF EXISTS `reputation_rating_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reputation_rating_types` (
  `id` bigint NOT NULL,
  `code` varchar(16) NOT NULL,
  `name_ko` varchar(32) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_reputation_rating_types_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='평판 평가 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reputation_rating_types`
--

LOCK TABLES `reputation_rating_types` WRITE;
/*!40000 ALTER TABLE `reputation_rating_types` DISABLE KEYS */;
/*!40000 ALTER TABLE `reputation_rating_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `settlement_statuses`
--

DROP TABLE IF EXISTS `settlement_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `settlement_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_settlement_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='정산 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `settlement_statuses`
--

LOCK TABLES `settlement_statuses` WRITE;
/*!40000 ALTER TABLE `settlement_statuses` DISABLE KEYS */;
/*!40000 ALTER TABLE `settlement_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `settlements`
--

DROP TABLE IF EXISTS `settlements`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `settlements`
--

LOCK TABLES `settlements` WRITE;
/*!40000 ALTER TABLE `settlements` DISABLE KEYS */;
/*!40000 ALTER TABLE `settlements` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_category`
--

DROP TABLE IF EXISTS `ticket_category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket_category` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(32) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_category_code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 카테고리 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_category`
--

LOCK TABLES `ticket_category` WRITE;
/*!40000 ALTER TABLE `ticket_category` DISABLE KEYS */;
INSERT INTO `ticket_category` VALUES (1,'concert','콘서트',1,1),(2,'sports','스포츠',1,2),(3,'musical','뮤지컬/연극',1,3),(4,'exhibition','전시/행사',1,4);
/*!40000 ALTER TABLE `ticket_category` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_features`
--

DROP TABLE IF EXISTS `ticket_features`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket_features` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '특이사항 코드',
  `name_ko` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '한글명',
  `name_en` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '영문명',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '설명',
  `sort_order` int DEFAULT '0' COMMENT '정렬 순서',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='티켓 특이사항 마스터';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_features`
--

LOCK TABLES `ticket_features` WRITE;
/*!40000 ALTER TABLE `ticket_features` DISABLE KEYS */;
INSERT INTO `ticket_features` VALUES (1,'reservation_id','예매처 ID로 전달','Transfer via Reservation ID','예매처 ID로 티켓을 전달합니다',1,'2026-01-16 04:49:58','2026-01-16 04:49:58'),(2,'on_site_pickup','현장발권','On-site Pickup','현장에서 직접 발권 가능합니다',2,'2026-01-16 04:49:58','2026-01-16 04:49:58'),(3,'mobile_ticket','모바일티켓','Mobile Ticket','모바일로 입장 가능합니다',3,'2026-01-16 04:49:58','2026-01-19 02:11:58'),(4,'discount_ticket','할인티켓(증빙필요)','Discount Ticket (Proof Required)','할인티켓으로 증빙 서류가 필요합니다',4,'2026-01-16 04:49:58','2026-01-19 02:11:58'),(5,'id_required','신분증필요','ID Required','입장 시 신분증이 필요합니다',5,'2026-01-16 04:49:58','2026-01-19 02:11:58'),(6,'restricted_view','시야제한석','Restricted View','시야가 제한될 수 있습니다',6,'2026-01-16 04:49:58','2026-01-16 04:49:58'),(7,'on_site_help','현장도움','On-site Assistance','현장에서 직원의 도움이 필요합니다',7,'2026-01-16 04:49:58','2026-01-16 04:49:58');
/*!40000 ALTER TABLE `ticket_features` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_images`
--

DROP TABLE IF EXISTS `ticket_images`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket_images` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ticket_id` bigint NOT NULL,
  `image_url` varchar(500) NOT NULL,
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_ticket_img_ticket` (`ticket_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 이미지 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_images`
--

LOCK TABLES `ticket_images` WRITE;
/*!40000 ALTER TABLE `ticket_images` DISABLE KEYS */;
INSERT INTO `ticket_images` VALUES (1,34,'tickets/34/9756f69bd8da4157b9c8ffff3fb41509.jpg','2026-01-15 00:56:16'),(2,35,'tickets/35/e190cd639795484a8b628852f511adfe.jpg','2026-01-16 03:49:16');
/*!40000 ALTER TABLE `ticket_images` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_price_history`
--

DROP TABLE IF EXISTS `ticket_price_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_price_history`
--

LOCK TABLES `ticket_price_history` WRITE;
/*!40000 ALTER TABLE `ticket_price_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticket_price_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_statuses`
--

DROP TABLE IF EXISTS `ticket_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket_statuses` (
  `id` int NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_statuses`
--

LOCK TABLES `ticket_statuses` WRITE;
/*!40000 ALTER TABLE `ticket_statuses` DISABLE KEYS */;
INSERT INTO `ticket_statuses` VALUES (1,'available','판매중',1,1),(2,'reserved','예약중',1,2),(3,'expired','만료',1,4),(4,'hidden','숨김',1,5),(5,'cancelled','판매취소',1,7);
/*!40000 ALTER TABLE `ticket_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_verification`
--

DROP TABLE IF EXISTS `ticket_verification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_verification`
--

LOCK TABLES `ticket_verification` WRITE;
/*!40000 ALTER TABLE `ticket_verification` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticket_verification` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket_verification_methods`
--

DROP TABLE IF EXISTS `ticket_verification_methods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket_verification_methods` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_ticket_verification_methods_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 검증 방법 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_verification_methods`
--

LOCK TABLES `ticket_verification_methods` WRITE;
/*!40000 ALTER TABLE `ticket_verification_methods` DISABLE KEYS */;
/*!40000 ALTER TABLE `ticket_verification_methods` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tickets`
--

DROP TABLE IF EXISTS `tickets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tickets` (
  `id` int NOT NULL AUTO_INCREMENT,
  `seller_id` int NOT NULL,
  `event_id` int DEFAULT NULL COMMENT '공연 FK',
  `schedule_id` varchar(36) DEFAULT NULL COMMENT '일정 FK',
  `category_id` int NOT NULL,
  `event_datetime` datetime NOT NULL COMMENT '공연 일시',
  `seat_location_id` int DEFAULT NULL,
  `area_id` int DEFAULT NULL,
  `row` varchar(20) DEFAULT NULL COMMENT '열 (예: 5열)',
  `quantity` int NOT NULL COMMENT '총 수량',
  `is_consecutive` tinyint(1) DEFAULT '0' COMMENT '연석 여부',
  `remaining_quantity` int NOT NULL DEFAULT '0' COMMENT '남은 수량',
  `price` int NOT NULL COMMENT '판매가',
  `description` text COMMENT '상세 설명',
  `status_id` int NOT NULL DEFAULT '1',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `deleted_at` timestamp NULL DEFAULT NULL COMMENT 'Soft Delete 시각',
  `seat_grade_id` int DEFAULT NULL COMMENT '좌석 등급 ID (VIP, 일반, 지정석 등)',
  `trade_method_id` int DEFAULT NULL COMMENT '거래 방식 ID',
  `has_ticket` tinyint(1) DEFAULT NULL COMMENT '티켓 보유 여부 (1: 보유, 0: 미보유)',
  `feature_ids` text COMMENT '티켓 특이사항 ID 목록 (콤마 구분)',
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
  KEY `idx_tickets_seat_grade` (`seat_grade_id`),
  KEY `idx_tickets_trade_method` (`trade_method_id`),
  KEY `idx_tickets_has_ticket` (`has_ticket`),
  KEY `fk_tickets_seat_location` (`seat_location_id`),
  KEY `fk_tickets_event_seat_area` (`area_id`),
  CONSTRAINT `fk_ticket_category` FOREIGN KEY (`category_id`) REFERENCES `ticket_category` (`id`),
  CONSTRAINT `fk_tickets_event` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`),
  CONSTRAINT `fk_tickets_event_seat_area` FOREIGN KEY (`area_id`) REFERENCES `event_seat_areas` (`id`),
  CONSTRAINT `fk_tickets_event_seat_grade` FOREIGN KEY (`seat_grade_id`) REFERENCES `event_seat_grades` (`id`) ON DELETE SET NULL,
  CONSTRAINT `fk_tickets_status` FOREIGN KEY (`status_id`) REFERENCES `ticket_statuses` (`id`),
  CONSTRAINT `fk_tickets_trade_method` FOREIGN KEY (`trade_method_id`) REFERENCES `trade_methods` (`id`) ON DELETE SET NULL,
  CONSTRAINT `tickets_ibfk_2` FOREIGN KEY (`seat_location_id`) REFERENCES `event_seat_locations` (`id`),
  CONSTRAINT `chk_ticket_price` CHECK ((`price` > 0)),
  CONSTRAINT `chk_ticket_quantity` CHECK ((`quantity` > 0)),
  CONSTRAINT `chk_ticket_remaining_qty` CHECK (((`remaining_quantity` >= 0) and (`remaining_quantity` <= `quantity`)))
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tickets`
--

LOCK TABLES `tickets` WRITE;
/*!40000 ALTER TABLE `tickets` DISABLE KEYS */;
INSERT INTO `tickets` VALUES (1,7,1,NULL,1,'2026-01-28 19:00:00',NULL,NULL,'19열',2,0,2,220000,'VIP석 연석 2장',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(2,7,1,NULL,1,'2026-01-28 19:00:00',NULL,NULL,'3열',1,0,1,150000,'R석 1장',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(3,7,1,NULL,1,'2026-01-28 19:00:00',NULL,NULL,'20열',3,0,1,99000,'S석',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(4,7,1,NULL,1,'2026-01-29 19:00:00',NULL,NULL,'11열',2,0,0,230000,'VIP석 연석 2장 (매진임박)',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(5,7,1,NULL,1,'2026-01-29 19:00:00',NULL,NULL,'14열',4,0,2,160000,'R석',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(6,7,2,NULL,1,'2026-02-23 18:00:00',NULL,NULL,'15열',10,0,8,180000,'VIP 입장권',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(7,7,2,NULL,1,'2026-02-23 18:00:00',NULL,NULL,'14열',30,0,25,99000,'일반 입장권',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(8,7,2,NULL,1,'2026-02-24 18:00:00',NULL,NULL,'5열',20,0,12,99000,'일반 입장권',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(9,7,3,NULL,1,'2026-08-02 17:00:00',NULL,NULL,'4열',50,0,30,132000,'스탠딩 입장권',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(10,7,3,NULL,1,'2026-08-02 17:00:00',NULL,NULL,'2열',20,0,2,165000,'지정석 입장권 (인기)',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(11,7,3,NULL,1,'2026-08-03 17:00:00',NULL,NULL,'20열',50,0,50,132000,'스탠딩 입장권',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(12,7,4,NULL,1,'2026-03-14 18:00:00',NULL,NULL,'12열',10,0,5,250000,'VIP석 (티켓 5장 남음)',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(13,7,6,NULL,1,'2026-10-28 19:00:00',NULL,NULL,'1열',100,0,20,180000,'스탠딩 입장권',1,'2025-12-17 07:49:16','2026-01-16 05:56:47',NULL,1,1,1,NULL),(14,7,7,NULL,3,'2026-03-14 14:00:00',NULL,NULL,'7열',2,0,2,180000,'VIP석 2연석입니다. 시야 최고!',1,'2025-12-18 04:03:13','2026-01-16 05:56:47',NULL,1,1,1,NULL),(15,8,7,NULL,3,'2026-03-14 14:00:00',NULL,NULL,'10열',1,0,1,130000,'R석 단석입니다.',1,'2025-12-18 04:03:13','2026-01-16 05:56:47',NULL,1,1,1,NULL),(16,9,8,NULL,3,'2026-04-23 19:30:00',NULL,NULL,'11열',2,0,2,90000,'조승우 캐스팅일입니다!',1,'2025-12-18 04:03:13','2026-01-16 05:56:47',NULL,1,1,1,NULL),(17,10,8,NULL,3,'2026-04-24 14:00:00',NULL,NULL,'6열',1,0,1,200000,'정중앙 최고의 시야',1,'2025-12-18 04:03:13','2026-01-16 05:56:47',NULL,1,1,1,NULL),(18,7,9,NULL,3,'2026-05-28 19:00:00',NULL,NULL,'13열',2,0,2,150000,'김준수 캐스팅, 연석',1,'2025-12-18 04:03:13','2026-01-16 05:56:47',NULL,1,1,1,NULL),(19,8,10,NULL,3,'2026-07-03 19:00:00',NULL,NULL,'9열',1,0,1,80000,'2층 맨 앞줄 시야 좋아요',1,'2025-12-18 04:03:13','2026-01-16 05:56:47',NULL,1,1,1,NULL),(20,10,11,NULL,2,'2026-04-18 14:00:00',NULL,NULL,'7열',2,0,2,25000,'야구 관람 좋은 자리입니다',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(21,7,11,NULL,2,'2026-04-18 14:00:00',NULL,NULL,'4열',4,0,4,12000,'외야 응원석 4장 함께 드려요',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(22,8,12,NULL,2,'2026-04-25 18:30:00',NULL,NULL,'19열',2,0,2,50000,'테이블석 연석, 맥주 마시며 관람',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(23,9,13,NULL,2,'2026-05-23 19:00:00',NULL,NULL,'5열',2,0,2,35000,'축구 경기 좋은 시야',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(24,11,13,NULL,2,'2026-05-23 19:00:00',NULL,NULL,'4열',1,0,1,20000,'가성비 좋은 자리',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(25,10,14,NULL,2,'2026-11-28 18:00:00',NULL,NULL,'7열',2,0,2,150000,'코트 바로 옆! 선수들 코앞에서!',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(26,7,15,NULL,2,'2026-06-18 20:00:00',NULL,NULL,'2열',2,0,2,80000,'손흥민 볼 수 있어요!',1,'2025-12-18 04:03:31','2026-01-16 05:56:47',NULL,1,1,1,NULL),(27,7,16,NULL,4,'2026-03-28 14:00:00',NULL,NULL,'10열',2,0,2,18000,'제주 빛의 시어터 반 고흐 전시 입장권입니다',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(28,8,17,NULL,4,'2026-05-14 15:00:00',NULL,NULL,'2열',1,0,1,25000,'디지털 아트 전시 입장권',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(29,9,18,NULL,4,'2026-06-02 11:00:00',NULL,NULL,'2열',2,0,2,16000,'모네 특별전 입장권 2매',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(30,10,1,NULL,4,'2026-06-14 10:00:00',NULL,NULL,'20열',2,0,2,45000,'에버랜드 1일 자유이용권입니다',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(31,11,1,NULL,4,'2026-07-28 10:00:00',NULL,NULL,'16열',1,0,1,40000,'롯데월드 1일권 판매합니다',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(32,7,1,NULL,4,'2026-08-23 10:00:00',NULL,NULL,'19열',3,0,3,35000,'여름 캐리비안베이 3장 일괄',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(33,8,1,NULL,4,'2027-01-13 23:59:59',NULL,NULL,'6열',5,0,5,10000,'CGV 영화 관람권 5장',1,'2025-12-18 04:03:48','2026-01-16 05:56:47',NULL,1,1,1,NULL),(34,12,1,'SCH001',1,'2026-01-28 19:00:00',NULL,NULL,'2열',2,1,2,5000,'테스트',1,'2026-01-15 00:56:14','2026-01-16 05:56:47',NULL,1,1,1,NULL),(35,12,1,'SCH001',1,'2026-01-28 19:00:00',NULL,NULL,'11열',2,1,2,60000,'티켓 어쩌고 저쩌고 테스트 하는 중',1,'2026-01-16 03:49:15','2026-01-16 05:56:47',NULL,1,1,1,NULL),(38,14,1,'SCH001',1,'2026-01-28 19:00:00',1,1,'1열',1,0,1,100000,'VIP석 1층 A구역 1열 좋은 자리입니다.',1,'2026-01-19 05:50:19','2026-01-19 05:50:19',NULL,2,NULL,1,'1,2'),(39,14,1,'SCH001',1,'2026-01-28 19:00:00',2,2,'5열',2,1,2,50000,'일반석 2층 B구역 연석 양도합니다.',1,'2026-01-19 05:50:19','2026-01-19 05:50:19',NULL,1,NULL,1,'3');
/*!40000 ALTER TABLE `tickets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `trade_methods`
--

DROP TABLE IF EXISTS `trade_methods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `trade_methods` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '거래 방식 코드',
  `name_ko` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '한글명',
  `name_en` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '영문명',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '설명',
  `sort_order` int DEFAULT '0' COMMENT '정렬 순서',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='거래 방식 마스터';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `trade_methods`
--

LOCK TABLES `trade_methods` WRITE;
/*!40000 ALTER TABLE `trade_methods` DISABLE KEYS */;
INSERT INTO `trade_methods` VALUES (1,'pin_trade','PIN거래','PIN Trade','예매 PIN 번호를 전달하는 방식입니다',1,'2026-01-16 04:50:05','2026-01-16 04:50:05'),(2,'delivery','배송거래','Delivery','실물 티켓을 배송하는 방식입니다',2,'2026-01-16 04:50:05','2026-01-19 02:11:57'),(3,'on_site','현장거래','On-site Trade','현장에서 직접 만나 거래하는 방식입니다',3,'2026-01-16 04:50:05','2026-01-16 04:50:05'),(4,'other','기타거래','Other','기타 방식의 거래입니다',4,'2026-01-16 04:50:05','2026-01-19 02:11:57');
/*!40000 ALTER TABLE `trade_methods` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `transaction_confirmed_bys`
--

DROP TABLE IF EXISTS `transaction_confirmed_bys`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `transaction_confirmed_bys` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_transaction_confirmed_bys_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 확인자 유형 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transaction_confirmed_bys`
--

LOCK TABLES `transaction_confirmed_bys` WRITE;
/*!40000 ALTER TABLE `transaction_confirmed_bys` DISABLE KEYS */;
INSERT INTO `transaction_confirmed_bys` VALUES (1,'buyer','구매자',1,1),(2,'seller','판매자',1,2);
/*!40000 ALTER TABLE `transaction_confirmed_bys` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `transaction_history`
--

DROP TABLE IF EXISTS `transaction_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transaction_history`
--

LOCK TABLES `transaction_history` WRITE;
/*!40000 ALTER TABLE `transaction_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `transaction_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `transaction_items`
--

DROP TABLE IF EXISTS `transaction_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transaction_items`
--

LOCK TABLES `transaction_items` WRITE;
/*!40000 ALTER TABLE `transaction_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `transaction_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `transaction_statuses`
--

DROP TABLE IF EXISTS `transaction_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `transaction_statuses` (
  `id` bigint NOT NULL,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_transaction_statuses_code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 상태 코드 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transaction_statuses`
--

LOCK TABLES `transaction_statuses` WRITE;
/*!40000 ALTER TABLE `transaction_statuses` DISABLE KEYS */;
INSERT INTO `transaction_statuses` VALUES (1,'reserved','예약중',1,1),(2,'pending_payment','결제대기',1,2),(3,'paid','결제완료',1,3),(4,'confirmed','구매확정',1,4),(5,'completed','거래완료',1,5),(6,'cancelled','취소됨',1,6),(7,'refunded','환불됨',1,7);
/*!40000 ALTER TABLE `transaction_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `transactions`
--

DROP TABLE IF EXISTS `transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transactions`
--

LOCK TABLES `transactions` WRITE;
/*!40000 ALTER TABLE `transactions` DISABLE KEYS */;
/*!40000 ALTER TABLE `transactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_favorites`
--

DROP TABLE IF EXISTS `user_favorites`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_favorites`
--

LOCK TABLES `user_favorites` WRITE;
/*!40000 ALTER TABLE `user_favorites` DISABLE KEYS */;
INSERT INTO `user_favorites` VALUES (1,7,1,1,'2025-12-18 02:05:53'),(2,7,1,2,'2025-12-18 02:05:53'),(3,8,1,1,'2025-12-18 02:05:53'),(4,8,1,6,'2025-12-18 02:05:53'),(5,9,1,4,'2025-12-18 02:05:53'),(6,10,1,5,'2025-12-18 02:05:53'),(7,7,2,1,'2025-12-18 02:05:53'),(8,7,2,3,'2025-12-18 02:05:53'),(9,8,2,2,'2025-12-18 02:05:53'),(10,9,2,1,'2025-12-18 02:05:53'),(11,10,2,4,'2025-12-18 02:05:53'),(12,11,2,5,'2025-12-18 02:05:53'),(15,1,2,9,'2026-01-08 10:27:49'),(22,12,2,3,'2026-01-12 07:21:08'),(31,12,2,2,'2026-01-15 01:06:10'),(32,12,2,34,'2026-01-15 04:10:12');
/*!40000 ALTER TABLE `user_favorites` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_profile`
--

DROP TABLE IF EXISTS `user_profile`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_profile`
--

LOCK TABLES `user_profile` WRITE;
/*!40000 ALTER TABLE `user_profile` DISABLE KEYS */;
INSERT INTO `user_profile` VALUES (7,'티켓마스터','https://picsum.photos/200/200?random=1','안녕하세요! 공연 티켓 거래합니다.',38.5,15),(8,'콘서트러버','https://picsum.photos/200/200?random=2','콘서트를 사랑하는 사람입니다',42,28),(9,'뮤지컬팬','https://picsum.photos/200/200?random=3','뮤지컬 덕후입니다 ^^',36.5,3),(10,'스포츠광','https://picsum.photos/200/200?random=4','야구, 축구 다 좋아해요',45.2,42),(11,'문화생활','https://picsum.photos/200/200?random=5','전시회도 좋아합니다',39.8,18),(12,'test success','profiles/12/6e264a7c09744146814b043d6272d1bd.jpg','자기소개란 테스트',36.5,0),(13,NULL,NULL,NULL,36.5,0),(14,NULL,NULL,NULL,36.5,0),(15,'겸손한앵무새',NULL,NULL,36.5,0);
/*!40000 ALTER TABLE `user_profile` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_reputation`
--

DROP TABLE IF EXISTS `user_reputation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_reputation`
--

LOCK TABLES `user_reputation` WRITE;
/*!40000 ALTER TABLE `user_reputation` DISABLE KEYS */;
/*!40000 ALTER TABLE `user_reputation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_verification`
--

DROP TABLE IF EXISTS `user_verification`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_verification`
--

LOCK TABLES `user_verification` WRITE;
/*!40000 ALTER TABLE `user_verification` DISABLE KEYS */;
/*!40000 ALTER TABLE `user_verification` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (7,'user7@example.com','$2a$11$KzrYQ.GE9g.HL71sWBdlYuRYR3iCxXkR2Q./S1rkMVrZkKwMNLvkq','01063937605',1,2,'2025-12-16 07:14:44','2026-01-11 15:38:14',0),(8,'user8@example.com','$2a$11$OhNNpB7gHZUfNylXL.A4l.bdqJfd2f5tEeBLItfdb5IzORpdFEkXm','string',1,2,'2025-12-16 07:19:04',NULL,0),(9,'user9@example.com','$2a$11$.0wEtmpZhxsQx2wr3jPiO.EVjyaJCd6Q9F/7mTBZJQzxTVghE5FOK','01063937605',1,2,'2025-12-16 07:21:54',NULL,0),(10,'user10@example.com','$2a$11$Ly39wSG/2fetq46qFoioXOXVp18G40kYQ/RDGC.EeRq94IM/HK23S','01063937605',1,2,'2025-12-16 07:22:47',NULL,0),(11,'user11@example.com','$2a$11$lUQ1UJ9l73n0VERun/8.s.gRLYDt.7bvudsuupJkEgws2AdLcCx/W','01063937605',1,2,'2025-12-16 07:28:32','2025-12-15 22:28:43',0),(12,'test@test.com','$2a$11$ZH/.ReLIZsYPK0nI6uIIOumilFm1y6Jlo/VY4ONfFYU8uASvvhq/.','01012345678',1,2,'2026-01-12 00:39:21','2026-01-18 22:40:54',0),(13,'hu@test.com','$2a$11$S.kZpHTadN5m54XtYQQMiusKeiWc8fJpB.Q13fWQ2eiXnLg99yOQW','01012345678',1,2,'2026-01-12 04:10:09',NULL,0),(14,'chan@test.com','$2a$11$jI36SxUcb2ynZ.nQbT1KKeANQukn.pNQ.cQqSGJG9gVsET7lIVrpS','01012345678',1,2,'2026-01-12 04:21:26',NULL,0),(15,'new@new.com','$2a$11$dpkX5fXt0Zl8l0gag3sWueShPeRZzed9tHdWChNqCZ4TrFKvYGBDq','01012345678',1,2,'2026-01-15 04:14:48','2026-01-14 20:39:46',0);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-01-19 17:07:56
