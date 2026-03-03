-- MySQL dump 10.13  Distrib 9.6.0, for macos26.2 (arm64)
--
-- Host: localhost    Database: TicketPlatFormDB
-- ------------------------------------------------------
-- Server version	9.6.0

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
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ '811ccd2c-62c8-11f0-9312-1a182a883054:1-3962,
83a29080-b2ce-11f0-b9bf-2bd4a7c7f163:1-1391';

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
-- Table structure for table `balance_transactions`
--

DROP TABLE IF EXISTS `balance_transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `balance_transactions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL,
  `type` varchar(32) NOT NULL,
  `amount` bigint NOT NULL,
  `balance_after` bigint NOT NULL,
  `reference_type` varchar(50) DEFAULT NULL,
  `reference_id` bigint DEFAULT NULL,
  `description` text,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_balance_transactions_user_id` (`user_id`),
  KEY `idx_balance_transactions_reference` (`reference_type`,`reference_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `balance_transactions`
--

LOCK TABLES `balance_transactions` WRITE;
/*!40000 ALTER TABLE `balance_transactions` DISABLE KEYS */;
/*!40000 ALTER TABLE `balance_transactions` ENABLE KEYS */;
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
  `bank_code` varchar(10) DEFAULT NULL COMMENT '은행 코드(토스 지급대행용)',
  `account_number` varchar(50) DEFAULT NULL COMMENT '계좌번호',
  `account_holder` varchar(50) DEFAULT NULL COMMENT '예금주',
  `verified` tinyint(1) DEFAULT '0' COMMENT '계좌 인증 여부',
  `verification_provider` varchar(20) DEFAULT NULL COMMENT 'CUSTOM|TOSS|HYBRID',
  `verification_tier` varchar(20) DEFAULT NULL COMMENT 'TIER_0..3',
  `verification_status` varchar(20) DEFAULT NULL COMMENT 'UNVERIFIED|PENDING|VERIFIED|FAILED|EXPIRED',
  `last_verification_failure_code` varchar(100) DEFAULT NULL COMMENT '최근 검증 실패 코드',
  `last_verification_at` datetime DEFAULT NULL COMMENT '최근 검증 시각',
  `verification_code` varchar(10) DEFAULT NULL COMMENT '1원 인증 코드',
  `verification_expires_at` datetime DEFAULT NULL COMMENT '인증 코드 만료 시각',
  `verified_at` datetime DEFAULT NULL COMMENT '인증 완료 시각',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_bank_user` (`user_id`),
  KEY `idx_bank_verified` (`user_id`,`verified`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 은행 계좌 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bank_account`
--

LOCK TABLES `bank_account` WRITE;
/*!40000 ALTER TABLE `bank_account` DISABLE KEYS */;
INSERT INTO `bank_account` VALUES (1,27,'토스뱅크','092','100039789655','이현성',0,'CUSTOM','TIER_0_NONE','UNVERIFIED',NULL,NULL,'9741','2026-02-25 04:18:58',NULL,'2026-02-24 15:06:26'),(3,22,'NH농협은행','011','3560591890293','이현성',0,'CUSTOM','TIER_0_NONE','UNVERIFIED',NULL,NULL,'3638','2026-02-25 06:08:42',NULL,'2026-02-24 20:31:38'),(5,12,'지역농협','012','3560591890293','이현성',1,'TOSS','TIER_2_ACCOUNT_VALID','VERIFIED',NULL,NULL,NULL,NULL,'2026-02-26 07:31:00','2026-02-25 22:30:59'),(7,500,'테스트은행','001','110518614326','홍길동',1,'TOSS','TIER_2_ACCOUNT_VALID','VERIFIED',NULL,NULL,NULL,NULL,'2026-03-03 03:47:57','2026-03-02 18:47:56'),(8,501,'테스트은행','001','110713621135','홍길동',1,'TOSS','TIER_2_ACCOUNT_VALID','VERIFIED',NULL,NULL,NULL,NULL,'2026-03-03 03:47:57','2026-03-02 18:47:57');
/*!40000 ALTER TABLE `bank_account` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chat_message_images`
--

DROP TABLE IF EXISTS `chat_message_images`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chat_message_images` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `message_id` bigint NOT NULL COMMENT '메시지 FK',
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Supabase Object Key',
  `sort_order` int NOT NULL DEFAULT '0' COMMENT '이미지 순서 (0부터 시작)',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_chat_message_images_order` (`message_id`,`sort_order`),
  KEY `idx_chat_message_images_message` (`message_id`),
  CONSTRAINT `fk_chat_message_images_message` FOREIGN KEY (`message_id`) REFERENCES `chat_messages` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='채팅 메시지 이미지 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chat_message_images`
--

LOCK TABLES `chat_message_images` WRITE;
/*!40000 ALTER TABLE `chat_message_images` DISABLE KEYS */;
INSERT INTO `chat_message_images` VALUES (1,39,'chat/1/12_1769054129_1cc92a63ddc84afb81e178fa42c6a248.jpg',0,'2026-01-22 03:55:30'),(2,40,'chat/1/12_1769054130_17a58c50db1343e6898a21a07ce76f13.jpg',0,'2026-01-22 03:55:31'),(4,41,'chat/1/12_1769058235_d35bc2da00c94d418650003894e2f768.jpg',0,'2026-01-22 05:03:57'),(5,41,'chat/1/12_1769058235_c446000014e34a769315097831eea24a.jpg',1,'2026-01-22 05:03:57'),(6,42,'chat/1/12_1769059044_cb04f098bcb646e5b11ee938b712df19.jpg',0,'2026-01-22 05:17:25'),(7,94,'chat/1/12_1769383271_e4452d8001994c3b9619752fa59d9860.jpg',0,'2026-01-25 23:21:12'),(8,95,'chat/1/12_1769383284_2eb7555f292e4a558e11faae844345ac.jpg',0,'2026-01-25 23:21:25'),(9,96,'chat/1/15_1769383800_3e85fc8f178748f3819d12d572eb5a0d.jpg',0,'2026-01-25 23:30:02'),(10,96,'chat/1/15_1769383801_02f15c8557fb4d298059808a00eb48ce.jpg',1,'2026-01-25 23:30:02'),(11,96,'chat/1/15_1769383801_e66e1a74716a4811ae5e1a7ff703c2d2.jpg',2,'2026-01-25 23:30:02'),(12,97,'chat/1/12_1769387013_071243c94be44dbf8f2477306f8fe363.jpg',0,'2026-01-26 00:23:35'),(13,97,'chat/1/12_1769387014_8a0a45036b7b4ee9b345bc23be108e25.jpg',1,'2026-01-26 00:23:35'),(14,98,'chat/1/12_1769387779_1861571db4ae498f857ebd1468f3235d.png',0,'2026-01-26 00:36:20'),(15,99,'chat/1/12_1769387795_24fcb63e9a054318af9c7184ad0dbbfd.png',0,'2026-01-26 00:36:36'),(16,100,'chat/1/15_1769387859_3b2d8dd36fea43db871f086633535d34.jpg',0,'2026-01-26 00:37:40'),(17,101,'chat/1/12_1769387923_e5551360b0654f9584c9f38342b31abe.png',0,'2026-01-26 00:38:45'),(18,102,'chat/1/12_1769388385_599e32b35c5c4d9a829720f46a2abb58.png',0,'2026-01-26 00:46:28'),(19,103,'chat/1/12_1769388394_3d12a7360fa24267b2e03676acf31ffe.jpg',0,'2026-01-26 00:46:35');
/*!40000 ALTER TABLE `chat_message_images` ENABLE KEYS */;
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
  `message_type` varchar(32) NOT NULL DEFAULT 'TEXT',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_msg_room` (`room_id`),
  KEY `idx_msg_room_created` (`room_id`,`created_at`),
  KEY `idx_msg_created` (`created_at`),
  KEY `idx_msg_sender_created` (`sender_id`,`created_at`),
  CONSTRAINT `fk_chat_messages_room` FOREIGN KEY (`room_id`) REFERENCES `chat_rooms` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=252 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='채팅 메시지 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chat_messages`
--

LOCK TABLES `chat_messages` WRITE;
/*!40000 ALTER TABLE `chat_messages` DISABLE KEYS */;
INSERT INTO `chat_messages` VALUES (1,1,15,'안녕하세요',NULL,'TEXT','2026-01-20 20:32:14'),(2,1,15,'ㅎㅎ',NULL,'TEXT','2026-01-20 20:33:33'),(3,1,15,'ㅎㄴ',NULL,'TEXT','2026-01-20 20:46:20'),(4,1,12,'ㅎㅇ',NULL,'TEXT','2026-01-20 20:46:23'),(5,1,15,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-01-20 20:46:42'),(6,1,12,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-01-20 20:46:43'),(7,1,12,'ㅔㅔ',NULL,'TEXT','2026-01-20 21:03:39'),(8,1,15,'zz',NULL,'TEXT','2026-01-20 21:03:42'),(9,1,12,'ㅐㅐ',NULL,'TEXT','2026-01-20 21:03:59'),(10,1,12,'oo',NULL,'TEXT','2026-01-20 21:12:03'),(11,1,12,'he',NULL,'TEXT','2026-01-20 21:20:17'),(12,1,15,'gd',NULL,'TEXT','2026-01-20 21:20:21'),(13,1,12,'o',NULL,'TEXT','2026-01-20 21:27:18'),(14,1,15,'9',NULL,'TEXT','2026-01-20 21:28:41'),(15,1,15,'dd',NULL,'TEXT','2026-01-20 21:47:49'),(16,1,12,'ㅔㅔ',NULL,'TEXT','2026-01-20 21:47:53'),(17,1,12,'ㅕㅕㅎ',NULL,'TEXT','2026-01-20 21:47:55'),(18,1,12,'ㅏㅍㅇㅅ',NULL,'TEXT','2026-01-20 21:47:58'),(19,1,12,'판매자가 입력 11',NULL,'TEXT','2026-01-20 21:48:22'),(20,1,15,'구매자가 입력 22',NULL,'TEXT','2026-01-20 21:48:28'),(21,1,12,'ㅔㅔ',NULL,'TEXT','2026-01-20 21:49:39'),(22,1,12,'ㅐㅐ',NULL,'TEXT','2026-01-20 21:49:42'),(23,1,15,'ㅇㅇ',NULL,'TEXT','2026-01-20 21:49:45'),(24,1,12,'ㅗㅗ',NULL,'TEXT','2026-01-20 21:53:12'),(25,1,12,'ㅎㅇ',NULL,'TEXT','2026-01-20 21:55:51'),(26,1,15,'ㅎㅇ',NULL,'TEXT','2026-01-20 21:55:53'),(27,1,15,'굿',NULL,'TEXT','2026-01-20 21:55:57'),(28,1,12,'굿',NULL,'TEXT','2026-01-20 21:56:00'),(29,1,12,'ㅎㅇ',NULL,'TEXT','2026-01-20 21:56:13'),(30,1,12,'ㅔㅔ',NULL,'TEXT','2026-01-20 21:56:16'),(31,1,15,'ㅇㅇ',NULL,'TEXT','2026-01-20 21:56:26'),(32,1,15,'ㄴㄴ',NULL,'TEXT','2026-01-20 21:56:28'),(33,1,12,'ㅕㅑ',NULL,'TEXT','2026-01-20 23:03:36'),(34,1,12,'ㅅㅅ',NULL,'TEXT','2026-01-20 23:09:40'),(35,1,12,'야처ㅠ채맴우차벱pskxcqpqsn iw9qkwjdjcnkspoqnxnclqpqksncnxkqop',NULL,'TEXT','2026-01-20 23:10:37'),(36,1,12,'88',NULL,'TEXT','2026-01-21 15:23:06'),(37,1,12,'88',NULL,'TEXT','2026-01-21 15:24:40'),(38,1,12,'gg',NULL,'TEXT','2026-01-21 15:24:42'),(39,1,12,'images upload input test','chat/1/12_1769054129_1cc92a63ddc84afb81e178fa42c6a248.jpg','TEXT','2026-01-21 18:55:30'),(40,1,12,NULL,'chat/1/12_1769054130_17a58c50db1343e6898a21a07ce76f13.jpg','TEXT','2026-01-21 18:55:31'),(41,1,12,'images input test!!','chat/1/12_1769058235_d35bc2da00c94d418650003894e2f768.jpg','TEXT','2026-01-21 20:03:56'),(42,1,12,'ee','chat/1/12_1769059044_cb04f098bcb646e5b11ee938b712df19.jpg','TEXT','2026-01-21 20:17:25'),(43,1,12,'hu',NULL,'TEXT','2026-01-21 20:17:29'),(44,1,12,'ii',NULL,'TEXT','2026-01-21 20:17:30'),(45,1,12,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-01-22 17:04:12'),(46,1,12,'ㅗㅇㅎㅇ',NULL,'TEXT','2026-01-22 17:04:26'),(47,1,12,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-01-22 17:05:10'),(48,1,12,'ㄴㅁ',NULL,'TEXT','2026-01-22 17:05:25'),(49,1,12,'ㅑㄷ어',NULL,'TEXT','2026-01-22 17:05:38'),(50,1,12,'ㅎㅇ',NULL,'TEXT','2026-01-22 17:08:09'),(51,1,12,'데리',NULL,'TEXT','2026-01-22 17:23:27'),(52,1,12,'ㅋʕ•ﻌ•ʔ',NULL,'TEXT','2026-01-22 17:23:36'),(53,1,12,'몸 ㅓ타',NULL,'TEXT','2026-01-22 17:24:21'),(54,1,12,'네뷰',NULL,'TEXT','2026-01-22 17:30:10'),(55,1,12,'오오',NULL,'TEXT','2026-01-22 17:30:51'),(56,1,12,'하이',NULL,'TEXT','2026-01-22 17:32:37'),(57,1,12,'ㅇㅇㅇ',NULL,'TEXT','2026-01-22 17:37:48'),(58,1,15,'hh',NULL,'TEXT','2026-01-22 17:37:52'),(59,1,12,'ㅇㄴ',NULL,'TEXT','2026-01-22 17:39:23'),(60,1,12,'도레미',NULL,'TEXT','2026-01-22 19:10:16'),(61,1,15,'ddd',NULL,'TEXT','2026-01-22 19:15:30'),(62,1,15,'gg',NULL,'TEXT','2026-01-22 19:15:45'),(63,1,15,'hh',NULL,'TEXT','2026-01-22 19:16:01'),(64,1,15,'ddff',NULL,'TEXT','2026-01-22 19:20:23'),(65,1,15,'d',NULL,'TEXT','2026-01-22 19:45:18'),(66,1,15,'dd',NULL,'TEXT','2026-01-22 19:46:32'),(67,1,15,'ss',NULL,'TEXT','2026-01-22 19:46:37'),(68,1,15,'999',NULL,'TEXT','2026-01-22 19:46:40'),(69,1,15,'kk',NULL,'TEXT','2026-01-22 19:46:42'),(70,1,12,'ㅇㅇㄹㅊㅍ',NULL,'TEXT','2026-01-22 19:46:52'),(71,1,12,'ㅐㅐ',NULL,'TEXT','2026-01-22 19:47:15'),(72,1,15,'pp',NULL,'TEXT','2026-01-22 19:47:29'),(73,1,15,'ff',NULL,'TEXT','2026-01-22 19:50:11'),(74,1,15,'dd',NULL,'TEXT','2026-01-22 19:52:28'),(75,1,15,'hh',NULL,'TEXT','2026-01-22 19:52:30'),(76,1,15,'jut',NULL,'TEXT','2026-01-22 19:52:31'),(77,1,15,'ivse',NULL,'TEXT','2026-01-22 19:52:33'),(78,1,15,'v v',NULL,'TEXT','2026-01-22 19:52:34'),(79,1,15,'vyc',NULL,'TEXT','2026-01-22 19:52:36'),(80,1,15,'gg',NULL,'TEXT','2026-01-22 19:59:35'),(81,1,12,'왜여',NULL,'TEXT','2026-01-22 19:59:41'),(82,1,15,'ss',NULL,'TEXT','2026-01-22 20:06:09'),(83,1,12,'ㅎㅎ',NULL,'TEXT','2026-01-22 20:06:15'),(84,1,12,'ㅋㄴ',NULL,'TEXT','2026-01-22 20:06:33'),(85,1,15,'qq',NULL,'TEXT','2026-01-22 20:09:43'),(86,1,12,'ㅅㅎㅍㅍㅅ',NULL,'TEXT','2026-01-22 20:11:34'),(87,1,15,'rr',NULL,'TEXT','2026-01-22 20:11:39'),(88,1,12,'11',NULL,'TEXT','2026-01-22 20:12:00'),(89,1,15,'22',NULL,'TEXT','2026-01-22 20:12:03'),(90,2,12,'ㄴㄴㄴㅇ',NULL,'TEXT','2026-01-25 14:14:59'),(91,1,12,'ㅇㄴㄴ',NULL,'TEXT','2026-01-25 14:15:09'),(92,1,12,'자다',NULL,'TEXT','2026-01-25 14:18:20'),(93,1,15,'ss',NULL,'TEXT','2026-01-25 14:18:35'),(94,1,12,NULL,'chat/1/12_1769383271_e4452d8001994c3b9619752fa59d9860.jpg','TEXT','2026-01-25 14:21:12'),(95,1,12,'이거 맞아요','chat/1/12_1769383284_2eb7555f292e4a558e11faae844345ac.jpg','TEXT','2026-01-25 14:21:25'),(96,1,15,'3개','chat/1/15_1769383800_3e85fc8f178748f3819d12d572eb5a0d.jpg','TEXT','2026-01-25 14:30:02'),(97,1,12,NULL,'chat/1/12_1769387013_071243c94be44dbf8f2477306f8fe363.jpg','TEXT','2026-01-25 15:23:35'),(98,1,12,NULL,'chat/1/12_1769387779_1861571db4ae498f857ebd1468f3235d.png','TEXT','2026-01-25 15:36:20'),(99,1,12,NULL,'chat/1/12_1769387795_24fcb63e9a054318af9c7184ad0dbbfd.png','TEXT','2026-01-25 15:36:36'),(100,1,15,NULL,'chat/1/15_1769387859_3b2d8dd36fea43db871f086633535d34.jpg','TEXT','2026-01-25 15:37:40'),(101,1,12,NULL,'chat/1/12_1769387923_e5551360b0654f9584c9f38342b31abe.png','TEXT','2026-01-25 15:38:45'),(102,1,12,NULL,'chat/1/12_1769388385_599e32b35c5c4d9a829720f46a2abb58.png','TEXT','2026-01-25 15:46:28'),(103,1,12,NULL,'chat/1/12_1769388394_3d12a7360fa24267b2e03676acf31ffe.jpg','TEXT','2026-01-25 15:46:35'),(104,1,12,'결제가 요청되었습니다.',NULL,'TEXT','2026-01-25 19:50:28'),(105,10,15,'dd',NULL,'TEXT','2026-01-25 20:23:05'),(106,10,12,'ㅎㅇ',NULL,'TEXT','2026-01-25 20:23:16'),(107,10,12,'결제가 요청되었습니다.',NULL,'TEXT','2026-01-25 20:23:20'),(108,10,15,'dd',NULL,'TEXT','2026-01-29 21:13:13'),(109,12,12,'ff',NULL,'TEXT','2026-01-29 21:43:10'),(110,12,15,'결제가 요청되었습니다.',NULL,'TEXT','2026-01-29 21:43:56'),(111,12,12,'결제가 완료되었습니다. 상품을 전송해주세요.',NULL,'TEXT','2026-01-29 21:45:28'),(112,13,15,'gd',NULL,'TEXT','2026-02-01 19:12:13'),(113,13,15,'dd',NULL,'TEXT','2026-02-01 19:12:24'),(114,13,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-01 19:12:55'),(115,13,15,NULL,NULL,'PAYMENT_SUCCESS','2026-02-01 19:13:28'),(116,13,15,'결제가 완료되었습니다. 상품을 전송해주세요.',NULL,'TEXT','2026-02-01 19:13:28'),(117,14,12,'ㅡㅏ',NULL,'TEXT','2026-02-01 19:23:59'),(118,14,15,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-01 19:24:04'),(119,14,12,NULL,NULL,'PAYMENT_SUCCESS','2026-02-01 19:24:42'),(120,14,12,'결제가 완료되었습니다. 상품을 전송해주세요.',NULL,'TEXT','2026-02-01 19:24:42'),(121,14,12,'이게맞냐?',NULL,'TEXT','2026-02-01 20:04:32'),(122,14,15,'ss',NULL,'TEXT','2026-02-01 20:04:42'),(123,14,12,NULL,NULL,'PURCHASE_CONFIRMED','2026-02-01 21:45:25'),(124,14,15,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-01 22:15:46'),(125,13,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-01 22:16:18'),(126,12,15,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-01 22:16:21'),(127,10,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-01 22:16:24'),(128,1,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-01 22:16:27'),(129,2,12,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-01 22:16:35'),(130,15,15,'구매원해요',NULL,'TEXT','2026-02-02 14:24:51'),(131,15,15,'ㅇㅇ',NULL,'TEXT','2026-02-02 14:29:41'),(132,15,12,'ㅇㅇ 3장 팜',NULL,'TEXT','2026-02-02 14:29:52'),(133,15,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-02 15:20:52'),(134,15,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 15:25:46'),(135,17,15,'d',NULL,'TEXT','2026-02-02 15:58:02'),(136,17,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-02 15:58:33'),(137,17,15,'ㄴㄴ 안할래',NULL,'TEXT','2026-02-02 15:58:40'),(138,17,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 15:58:45'),(139,18,15,'ㅇㅇ',NULL,'TEXT','2026-02-02 15:59:54'),(140,18,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 16:00:07'),(141,11,15,'ㅎㅎㅎㅎ',NULL,'TEXT','2026-02-02 16:02:13'),(142,11,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 16:02:53'),(143,21,15,'dd',NULL,'TEXT','2026-02-02 16:27:53'),(144,21,12,'ㅎㅇ',NULL,'TEXT','2026-02-02 16:28:18'),(145,21,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-02 16:34:36'),(146,21,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 16:34:59'),(147,23,15,'dd',NULL,'TEXT','2026-02-02 16:40:18'),(148,24,15,'하이요',NULL,'TEXT','2026-02-02 18:26:45'),(149,25,12,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-02-02 18:49:07'),(150,25,12,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:54:16'),(151,24,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:54:19'),(152,23,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:55:31'),(153,26,15,'ㅇㅇ',NULL,'TEXT','2026-02-02 18:56:17'),(154,27,15,'ㄴㄴ',NULL,'TEXT','2026-02-02 18:56:42'),(155,6,12,'ㅎㅇ',NULL,'TEXT','2026-02-02 18:57:06'),(156,28,12,'ㅔㅐ',NULL,'TEXT','2026-02-02 18:57:26'),(157,26,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:57:48'),(158,27,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:57:49'),(159,28,15,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:57:52'),(160,6,12,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 18:57:59'),(161,29,15,'ㅎㅇ',NULL,'TEXT','2026-02-02 19:01:07'),(162,29,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-02 19:14:10'),(163,29,15,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-02 21:27:10'),(164,17,12,'결제 요청이 만료되었습니다.',NULL,'TEXT','2026-02-04 21:38:09'),(165,29,12,'결제 요청이 만료되었습니다.',NULL,'TEXT','2026-02-04 21:38:09'),(166,30,22,'안녕하세요',NULL,'TEXT','2026-02-11 14:35:41'),(167,30,22,'반갑스비다',NULL,'TEXT','2026-02-11 14:36:01'),(168,30,12,'gdgd',NULL,'TEXT','2026-02-11 14:52:18'),(169,30,22,'하이요',NULL,'TEXT','2026-02-11 14:52:30'),(170,30,22,'하이요?',NULL,'TEXT','2026-02-11 14:57:55'),(171,30,22,'ㅎㅎ',NULL,'TEXT','2026-02-11 14:58:04'),(172,30,22,'ㅕㅑ',NULL,'TEXT','2026-02-11 14:58:08'),(173,30,22,'ㅓㅓ',NULL,'TEXT','2026-02-11 14:58:45'),(174,31,22,'안녕하세요 티켓 문의 드립니다',NULL,'TEXT','2026-02-11 14:59:45'),(175,30,22,'하이업?',NULL,'TEXT','2026-02-11 15:18:44'),(176,31,22,'하이여',NULL,'TEXT','2026-02-11 15:19:21'),(177,31,22,'놉',NULL,'TEXT','2026-02-11 15:19:31'),(178,31,22,'ㅎㅎ',NULL,'TEXT','2026-02-11 15:19:42'),(179,31,22,'ㄱㄱ',NULL,'TEXT','2026-02-11 15:33:34'),(180,31,22,'반가워용',NULL,'TEXT','2026-02-11 15:33:54'),(181,31,22,'그래용',NULL,'TEXT','2026-02-11 15:34:01'),(182,31,22,'ㅔㅔ',NULL,'TEXT','2026-02-11 15:34:09'),(183,31,22,'넹',NULL,'TEXT','2026-02-11 15:36:38'),(184,31,22,'ㅎㅇ',NULL,'TEXT','2026-02-11 15:51:49'),(185,31,12,'gd',NULL,'TEXT','2026-02-11 15:51:54'),(186,31,22,'๑°⌓°๑',NULL,'TEXT','2026-02-11 15:52:03'),(187,31,22,'ㅓㅓ',NULL,'TEXT','2026-02-11 15:52:10'),(188,31,22,'ㅔㅣㅓ',NULL,'TEXT','2026-02-11 15:52:25'),(189,31,22,'ㅏㅏㅏㅓㅠㅍ',NULL,'TEXT','2026-02-11 15:52:31'),(190,31,22,'ㅓㅓ',NULL,'TEXT','2026-02-11 15:52:42'),(191,31,22,'아니영',NULL,'TEXT','2026-02-11 15:53:06'),(192,31,22,'ㅇㅇ',NULL,'TEXT','2026-02-11 16:09:00'),(193,31,22,'ㅏㅏ',NULL,'TEXT','2026-02-11 16:09:07'),(194,31,22,'ㅔㅔ',NULL,'TEXT','2026-02-11 16:09:08'),(195,30,22,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-02-11 16:32:23'),(196,30,22,'ㅓㅓㅓ',NULL,'TEXT','2026-02-11 16:32:29'),(197,30,22,'ㅓㅓㅐ',NULL,'TEXT','2026-02-11 16:32:38'),(198,30,22,'ㅣㅣㅜ',NULL,'TEXT','2026-02-11 16:32:39'),(199,30,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-11 16:35:25'),(200,30,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 16:43:41'),(201,31,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 16:43:47'),(202,32,22,'안녕하세요 티켓남았나요',NULL,'TEXT','2026-02-11 16:46:21'),(203,32,12,'네',NULL,'TEXT','2026-02-11 16:46:34'),(204,32,22,'2장 주세요',NULL,'TEXT','2026-02-11 16:46:43'),(205,32,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-11 16:46:47'),(206,32,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 17:04:13'),(207,33,22,'안녕하세요',NULL,'TEXT','2026-02-11 17:15:09'),(208,33,22,'3장 주세요',NULL,'TEXT','2026-02-11 17:15:16'),(209,33,12,'네히',NULL,'TEXT','2026-02-11 17:15:21'),(210,33,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-11 17:15:25'),(211,33,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 17:23:06'),(212,34,22,'ㅎㅇㅎㅇ',NULL,'TEXT','2026-02-11 17:23:18'),(213,35,22,'하아요',NULL,'TEXT','2026-02-11 17:33:50'),(214,35,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-11 17:34:09'),(215,35,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 18:52:02'),(216,36,22,'ㅎㅇ',NULL,'TEXT','2026-02-11 18:52:43'),(217,36,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-11 18:53:13'),(218,36,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 18:55:50'),(219,37,22,'ㅡㅏ',NULL,'TEXT','2026-02-11 18:57:19'),(220,37,12,'결제가 요청되었습니다.',NULL,'PAYMENT_REQUEST','2026-02-11 18:57:24'),(221,37,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 19:11:56'),(222,38,22,'3장',NULL,'TEXT','2026-02-11 19:12:24'),(223,38,12,'결제가 요청되었습니다. (수량: 3장, 총 금액: 450,000원)',NULL,'PAYMENT_REQUEST','2026-02-11 19:12:32'),(224,38,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-11 20:55:10'),(225,39,22,'33',NULL,'TEXT','2026-02-11 21:00:34'),(226,39,12,'결제가 요청되었습니다. (수량: 3장, 총 금액: 450,000원)',NULL,'PAYMENT_REQUEST','2026-02-11 21:00:53'),(227,39,22,NULL,NULL,'PAYMENT_SUCCESS','2026-02-11 22:03:38'),(228,39,22,NULL,NULL,'PURCHASE_CONFIRMED','2026-02-11 22:10:05'),(229,40,22,'ㅎㄹ',NULL,'TEXT','2026-02-12 15:59:40'),(230,36,12,'결제 요청이 만료되었습니다.',NULL,'TEXT','2026-02-12 19:40:20'),(231,43,12,'hello i want 2 ticket',NULL,'TEXT','2026-02-22 21:08:16'),(232,43,22,'good',NULL,'TEXT','2026-02-22 21:08:27'),(233,43,22,'결제가 요청되었습니다. (수량: 2장, 총 금액: 360,000원)',NULL,'PAYMENT_REQUEST','2026-02-22 21:08:57'),(234,43,12,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-22 22:46:41'),(235,39,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-22 22:47:17'),(236,44,22,'남은거 다 구매함 ㅋ',NULL,'TEXT','2026-02-22 22:47:34'),(237,44,12,'결제가 요청되었습니다. (수량: 7장, 총 금액: 1,050,000원)',NULL,'PAYMENT_REQUEST','2026-02-22 22:48:40'),(238,44,22,NULL,NULL,'PAYMENT_SUCCESS','2026-02-22 22:55:18'),(239,44,22,NULL,NULL,'PURCHASE_CONFIRMED','2026-02-23 14:23:32'),(240,44,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-23 15:53:10'),(241,45,22,'ㅇㅇ',NULL,'TEXT','2026-02-23 15:53:18'),(242,45,12,'결제가 요청되었습니다. (수량: 7장, 총 금액: 1,050,000원)',NULL,'PAYMENT_REQUEST','2026-02-23 15:53:26'),(243,45,12,'판매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-23 15:54:47'),(244,40,22,'구매자가 채팅방을 나갔습니다.',NULL,'TEXT','2026-02-23 15:54:53'),(245,43,22,'결제 요청이 만료되었습니다.',NULL,'TEXT','2026-02-23 21:16:04'),(246,46,22,'ㅎㅇ4장만 좀 살게',NULL,'TEXT','2026-02-23 22:19:18'),(247,46,22,'ㅎㄹㅎㄹ',NULL,'TEXT','2026-02-23 22:22:26'),(248,46,27,'ㅎㅇ ㅍㅍ',NULL,'TEXT','2026-02-23 22:22:35'),(249,46,27,'결제가 요청되었습니다. (수량: 4장, 총 금액: 480,000원)',NULL,'PAYMENT_REQUEST','2026-02-23 22:22:43'),(250,46,22,NULL,NULL,'PAYMENT_SUCCESS','2026-02-23 22:36:12'),(251,45,12,'결제 요청이 만료되었습니다.',NULL,'TEXT','2026-02-24 16:05:35');
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
INSERT INTO `chat_room_statuses` VALUES (1,'active','활성',1,1),(2,'closed','종료',1,2),(3,'deleted','삭제',1,3);
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
  UNIQUE KEY `uq_chat_rooms_ticket_buyer_deleted` (`ticket_id`,`buyer_id`,`deleted_at`),
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
) ENGINE=InnoDB AUTO_INCREMENT=47 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='채팅방 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chat_rooms`
--

LOCK TABLES `chat_rooms` WRITE;
/*!40000 ALTER TABLE `chat_rooms` DISABLE KEYS */;
INSERT INTO `chat_rooms` VALUES (35,23,13,22,12,2,'2026-02-11 18:52:02',0,0,NULL,'2026-02-12 03:52:02','2026-02-11 17:33:36','2026-02-11 18:52:39'),(36,23,NULL,22,12,2,'2026-02-12 19:40:20',1,0,NULL,'2026-02-12 03:55:50','2026-02-11 18:52:39',NULL),(37,24,15,22,12,2,'2026-02-11 19:11:56',0,1,NULL,'2026-02-12 04:11:56','2026-02-11 18:57:17','2026-02-11 19:12:21'),(38,24,16,22,12,2,'2026-02-11 20:55:10',1,0,NULL,'2026-02-12 05:55:10','2026-02-11 19:12:21','2026-02-11 21:00:30'),(39,24,17,22,12,2,'2026-02-22 22:47:17',0,1,'2026-02-12 07:10:05','2026-02-23 07:47:17','2026-02-11 21:00:30','2026-02-22 22:47:19'),(40,3,NULL,22,8,2,'2026-02-23 15:54:53',0,2,NULL,'2026-02-24 00:54:53','2026-02-12 15:59:38',NULL),(41,1,NULL,22,7,1,NULL,0,0,NULL,NULL,'2026-02-12 15:59:59',NULL),(42,2,NULL,22,7,1,NULL,0,0,NULL,NULL,'2026-02-12 16:00:04',NULL),(43,42,NULL,12,22,2,'2026-02-23 21:16:04',1,1,NULL,'2026-02-23 07:46:41','2026-02-22 21:07:54',NULL),(44,24,19,22,12,2,'2026-02-23 15:53:10',0,1,'2026-02-23 23:23:32','2026-02-24 00:53:10','2026-02-22 22:47:19','2026-02-23 15:53:15'),(45,24,NULL,22,12,2,'2026-02-24 16:05:35',2,0,NULL,'2026-02-24 00:54:47','2026-02-23 15:53:15',NULL),(46,46,21,22,27,1,'2026-02-23 22:36:12',0,0,NULL,NULL,'2026-02-23 22:19:09',NULL);
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
INSERT INTO `dispute_statuses` VALUES (1,'PENDING','접수 대기',1,1),(2,'IN_REVIEW','검토 중',1,2),(3,'RESOLVED_BUYER','구매자 승',1,3),(4,'RESOLVED_SELLER','판매자 승',1,4),(5,'REJECTED','신고 기각',1,5),(6,'CANCELLED','신고자 취소',1,6);
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
INSERT INTO `dispute_types` VALUES (1,'FAKE_TICKET','가짜/위조 티켓',1,1),(2,'WRONG_TICKET','잘못된 티켓',1,2),(3,'NO_DELIVERY','티켓 미배송',1,3),(4,'RUDE_BEHAVIOR','비매너 행위',1,4),(5,'OTHER','기타',1,5);
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
  `resolved_at` datetime DEFAULT NULL,
  `resolved_by_id` bigint DEFAULT NULL,
  `resolution_note` text,
  PRIMARY KEY (`id`),
  KEY `idx_dispute_trans` (`transaction_id`),
  KEY `idx_dispute_claimant` (`claimant_id`),
  KEY `idx_dispute_type_id` (`type_id`),
  KEY `idx_dispute_status` (`status_id`),
  KEY `idx_dispute_resolved_by` (`resolved_by_id`),
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
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='에스크로 (결제 대금 보관) 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `escrow`
--

LOCK TABLES `escrow` WRITE;
/*!40000 ALTER TABLE `escrow` DISABLE KEYS */;
INSERT INTO `escrow` VALUES (2,1,360000,12600,347400,1,'2026-01-29 20:50:36',NULL,NULL,'2026-01-30 05:50:35'),(3,2,5000,175,4825,1,'2026-01-29 21:14:00',NULL,NULL,'2026-01-30 06:13:59'),(4,3,180000,6300,173700,1,'2026-01-29 21:45:28',NULL,NULL,'2026-01-30 06:45:28'),(5,4,300000,10500,289500,2,'2026-02-01 19:13:28','2026-02-02 07:16:10',NULL,'2026-02-01 22:16:10'),(6,5,180000,6300,173700,2,'2026-02-01 19:24:42','2026-02-02 06:45:07',NULL,'2026-02-01 21:45:07'),(7,17,450000,15750,434250,2,'2026-02-11 22:03:38','2026-02-12 07:05:04',NULL,'2026-02-11 22:05:04'),(8,19,1050000,36750,1013250,2,'2026-02-22 22:55:18','2026-02-23 07:58:16',NULL,'2026-02-22 22:58:16'),(9,21,480000,16800,463200,2,'2026-02-23 22:36:12','2026-02-24 07:36:38',NULL,'2026-02-23 22:36:38');
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
INSERT INTO `escrow_statuses` VALUES (1,'holding','보관 중',1,1),(2,'released','정산 완료',1,2),(3,'refunded','환불 완료',1,3),(4,'frozen','동결',1,4);
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
INSERT INTO `event_schedules` VALUES ('SCH001',1,'2026-01-28','19:00:00',1,'2026-01-14 00:35:57'),('SCH002',2,'2026-02-23','18:00:00',1,'2026-01-14 00:35:57'),('SCH002A',2,'2026-02-24','18:00:00',1,'2026-01-30 06:29:39'),('SCH002B',2,'2026-02-25','19:30:00',1,'2026-01-30 06:29:39'),('SCH002C',2,'2026-02-26','18:00:00',1,'2026-01-30 06:29:39'),('SCH003',3,'2026-02-24','18:00:00',1,'2026-01-14 00:35:57'),('SCH004',4,'2026-02-25','19:30:00',1,'2026-01-14 00:35:57'),('SCH005',5,'2026-02-26','18:00:00',1,'2026-01-14 00:35:57'),('SCH006',6,'2026-10-28','19:00:00',1,'2026-01-14 00:35:57'),('SCH007',7,'2026-03-14','14:00:00',1,'2026-01-14 00:35:57'),('SCH008',8,'2026-04-23','19:30:00',1,'2026-01-14 00:35:57'),('SCH009',9,'2026-05-28','19:00:00',1,'2026-01-14 00:35:57'),('SCH010',10,'2026-07-03','19:00:00',1,'2026-01-14 00:35:57'),('SCH011',11,'2026-04-18','14:00:00',1,'2026-01-14 00:35:57'),('SCH012',12,'2026-04-25','18:30:00',1,'2026-01-14 00:35:57'),('SCH013',13,'2026-05-23','19:00:00',1,'2026-01-14 00:35:57'),('SCH014',14,'2026-11-28','18:00:00',1,'2026-01-14 00:35:57'),('SCH015',15,'2026-06-18','20:00:00',1,'2026-01-14 00:35:57'),('SCH016',16,'2026-01-14','10:00:00',1,'2026-01-14 00:35:57'),('SCH017',17,'2026-03-14','10:00:00',1,'2026-01-14 00:35:57'),('SCH018',18,'2026-04-14','10:00:00',1,'2026-01-14 00:35:57');
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연별 좌석 구역';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_seat_areas`
--

LOCK TABLES `event_seat_areas` WRITE;
/*!40000 ALTER TABLE `event_seat_areas` DISABLE KEYS */;
INSERT INTO `event_seat_areas` VALUES (1,1,'A구역',1,1,'2026-01-17 09:12:04'),(2,1,'B구역',1,2,'2026-01-17 09:12:04'),(3,2,'A구역',1,1,'2026-01-30 06:28:37'),(4,2,'B구역',1,2,'2026-01-30 06:28:37'),(5,2,'C구역',1,3,'2026-01-30 06:28:37');
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
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연별 좌석 등급 매핑';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_seat_grades`
--

LOCK TABLES `event_seat_grades` WRITE;
/*!40000 ALTER TABLE `event_seat_grades` DISABLE KEYS */;
INSERT INTO `event_seat_grades` VALUES (1,1,1,'REG','일반석','Regular',200000,1,1,'2026-01-17 09:12:04'),(2,1,2,'VIP','VIP석','VIP',100000,1,2,'2026-01-17 09:12:04'),(7,2,1,'R','R석','R',180000,1,1,'2026-01-30 06:28:37'),(8,2,2,'S','S석','S',150000,1,2,'2026-01-30 06:28:37'),(9,2,3,'A','A석','A',120000,1,3,'2026-01-30 06:28:37'),(10,2,4,'B','B석','B',90000,1,4,'2026-01-30 06:28:37'),(21,24,1,'VIP','VIP석','VIP',200000,1,1,'2026-02-23 03:07:05'),(22,25,1,'VIP','VIP석','VIP',200000,1,1,'2026-02-23 03:07:05'),(23,26,1,'VIP','VIP석','VIP',220000,1,1,'2026-02-23 03:07:05'),(24,27,1,'VIP','VIP석','VIP',180000,1,1,'2026-02-23 03:07:05'),(25,28,1,'VIP','VIP석','VIP',200000,1,1,'2026-02-23 03:07:05'),(26,29,1,'VIP','VIP석','VIP',170000,1,1,'2026-02-23 03:07:05'),(27,30,1,'VIP','VIP석','VIP',300000,1,1,'2026-02-23 03:07:05'),(28,31,1,'VIP','VIP석','VIP',200000,1,1,'2026-02-23 03:07:05');
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='공연별 좌석 위치';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `event_seat_locations`
--

LOCK TABLES `event_seat_locations` WRITE;
/*!40000 ALTER TABLE `event_seat_locations` DISABLE KEYS */;
INSERT INTO `event_seat_locations` VALUES (1,1,'1층',1,1,'2026-01-17 09:12:04'),(2,1,'2층',1,2,'2026-01-17 09:12:04'),(3,2,'1층',1,1,'2026-01-30 06:28:37'),(4,2,'2층',1,2,'2026-01-30 06:28:37'),(5,2,'3층',1,3,'2026-01-30 06:28:37');
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
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='이벤트/공연 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `events`
--

LOCK TABLES `events` WRITE;
/*!40000 ALTER TABLE `events` DISABLE KEYS */;
INSERT INTO `events` VALUES (1,1,1,'2024 월드 투어 서울','아이유의 2024 월드 투어 서울 공연','https://picsum.photos/400/600?random=1','올림픽공원 체조경기장','서울시 송파구 올림픽로 424','2026-01-28 19:00:00','2026-01-28 22:00:00',NULL,1,1,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(2,1,2,'Bunnies Camp 2024','뉴진스 팬미팅','https://picsum.photos/400/600?random=2','고척스카이돔','서울시 구로구 경인로 430','2026-02-23 18:00:00','2026-02-23 21:00:00',NULL,1,2,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(3,1,3,'흠뻑쇼 2024 - SUMMER SWAG','싸이의 여름 물총 축제','https://picsum.photos/400/600?random=3','잠실종합운동장','서울시 송파구 올림픽로 25','2026-08-02 17:00:00','2026-08-02 22:00:00',NULL,1,3,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(4,1,4,'IM HERO 앙코르 콘서트','임영웅 앙코르 콘서트','https://picsum.photos/400/600?random=4','KSPO돔','서울시 송파구 올림픽로 424','2026-03-14 18:00:00','2026-03-14 21:00:00',NULL,1,4,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(5,1,5,'Welcome to the Show','데이식스 콘서트','https://picsum.photos/400/600?random=5','블루스퀘어 마스터카드홀','서울시 용산구 이태원로 294','2026-04-18 19:00:00','2026-04-18 22:00:00',NULL,1,5,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(6,1,6,'BTS Yet To Come','BTS 부산 콘서트','https://picsum.photos/400/600?random=6','부산아시아드주경기장','부산시 연제구 월드컵대로 344','2026-10-28 19:00:00','2026-10-28 22:00:00',NULL,1,6,'2025-12-17 07:48:43','2026-01-14 05:26:08'),(7,3,NULL,'위키드 (WICKED)','마법의 나라 오즈에서 펼쳐지는 두 마녀의 우정 이야기','https://picsum.photos/400/600?random=20','블루스퀘어 신한카드홀','서울시 용산구 이태원로 294','2026-03-14 14:00:00','2026-03-14 17:00:00',NULL,1,1,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(8,3,NULL,'지킬앤하이드','조승우 주연의 지킬앤하이드 공연','https://picsum.photos/400/600?random=21','예술의전당 오페라극장','서울시 서초구 남부순환로 2406','2026-04-23 19:30:00','2026-04-23 22:30:00',NULL,1,2,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(9,3,NULL,'엘리자벳','오스트리아 황후 엘리자벳의 이야기','https://picsum.photos/400/600?random=22','샤롯데씨어터','서울시 송파구 잠실로 240','2026-05-28 19:00:00','2026-05-28 22:00:00',NULL,1,3,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(10,3,NULL,'알라딘','디즈니 뮤지컬 알라딘','https://picsum.photos/400/600?random=23','디큐브아트센터','서울시 구로구 경인로 662','2026-07-03 19:00:00','2026-07-03 21:30:00',NULL,1,4,'2025-12-18 04:02:01','2026-01-14 05:26:08'),(11,2,NULL,'2025 KBO 시즌 - KIA vs 두산','KBO 리그 정규시즌 경기','https://picsum.photos/400/600?random=30','광주 기아 챔피언스필드','광주시 북구 서림로 10','2026-04-18 14:00:00','2026-04-18 17:00:00',NULL,1,1,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(12,2,NULL,'2025 KBO 시즌 - 두산 홈경기','KBO 리그 두산 베어스 홈경기','https://picsum.photos/400/600?random=31','잠실야구장','서울시 송파구 올림픽로 25','2026-04-25 18:30:00','2026-04-25 21:30:00',NULL,1,2,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(13,2,NULL,'2025 K리그 - FC서울 홈경기','K리그 정규시즌 FC서울 홈경기','https://picsum.photos/400/600?random=32','서울월드컵경기장','서울시 마포구 월드컵로 240','2026-05-23 19:00:00','2026-05-23 21:00:00',NULL,1,3,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(14,2,NULL,'2025 KBL - 서울 삼성 vs SK','프로농구 정규시즌 경기','https://picsum.photos/400/600?random=33','잠실실내체육관','서울시 송파구 올림픽로 25','2026-11-28 18:00:00','2026-11-28 20:00:00',NULL,1,4,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(15,2,NULL,'손흥민 친선 경기','대한민국 vs 일본 친선경기','https://picsum.photos/400/600?random=34','서울월드컵경기장','서울시 마포구 월드컵로 240','2026-06-18 20:00:00','2026-06-18 22:00:00',NULL,1,5,'2025-12-18 04:02:10','2026-01-14 05:26:08'),(16,4,NULL,'반 고흐 인사이드','빛의 시어터에서 만나는 반 고흐','https://picsum.photos/400/600?random=40','빛의 시어터 제주','제주시 애월읍 어음리 1942','2026-01-14 10:00:00','2026-07-13 20:00:00',NULL,1,1,'2025-12-18 04:02:19','2026-01-14 05:26:08'),(17,4,NULL,'팀랩 보더리스','디지털 아트 뮤지엄','https://picsum.photos/400/600?random=41','잠실 롯데월드타워','서울시 송파구 올림픽로 300','2026-03-14 10:00:00','2027-01-13 21:00:00',NULL,1,2,'2025-12-18 04:02:19','2026-01-14 05:26:08'),(18,4,NULL,'모네: 빛을 그리다','인상파 거장 모네 특별전','https://picsum.photos/400/600?random=42','예술의전당 한가람미술관','서울시 서초구 남부순환로 2406','2026-04-14 10:00:00','2026-08-13 19:00:00',NULL,1,3,'2025-12-18 04:02:19','2026-01-14 05:26:08'),(24,1,NULL,'[DEADLINE] 핫딜 K-POP 쇼케이스 D-1','deadlineDeals 노출 검증용 (D-1, 고할인)','https://example.com/posters/deadline-kpop-d1.jpg','잠실 실내체육관','서울 송파구 올림픽로 25','2026-02-24 20:00:00','2026-02-24 22:30:00',NULL,1,920,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(25,1,NULL,'[DEADLINE] 핫딜 시티팝 라이브 D-0','deadlineDeals 노출 검증용 (D-0)','https://example.com/posters/deadline-citypop-d0.jpg','홍대 라이브홀','서울 마포구 와우산로 77','2026-02-23 19:00:00','2026-02-23 21:30:00',NULL,1,921,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(26,1,NULL,'[DEADLINE] 핫딜 재즈 페스티벌 D-2','deadlineDeals 노출 검증용 (D-2)','https://example.com/posters/deadline-jazz-d2.jpg','세종문화회관','서울 종로구 세종대로 175','2026-02-25 18:00:00','2026-02-25 20:30:00',NULL,1,922,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(27,3,NULL,'[DEADLINE] 핫딜 뮤지컬 갈라 D-1','deadlineDeals 노출 검증용 (D-1)','https://example.com/posters/deadline-musical-d1.jpg','예술의전당 오페라극장','서울 서초구 남부순환로 2406','2026-02-24 17:00:00','2026-02-24 19:30:00',NULL,1,923,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(28,1,NULL,'[DEADLINE] 핫딜 오케스트라 나이트 D-3','deadlineDeals 노출 검증용 (D-3)','https://example.com/posters/deadline-orchestra-d3.jpg','롯데콘서트홀','서울 송파구 올림픽로 300','2026-02-26 20:00:00','2026-02-26 22:00:00',NULL,1,924,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(29,1,NULL,'[DEADLINE] 핫딜 인디밴드 클럽공연 D-0','deadlineDeals 노출 검증용 (D-0)','https://example.com/posters/deadline-indie-d0.jpg','합정 클럽A','서울 마포구 양화로 50','2026-02-23 21:00:00','2026-02-23 23:00:00',NULL,1,925,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(30,1,NULL,'[DEADLINE] 제외 샘플 D-4','D-3 범위 밖 제외 검증용','https://example.com/posters/deadline-out-d4.jpg','테스트홀 외곽','서울 강동구 테스트로 1','2026-02-27 18:00:00','2026-02-27 20:00:00',NULL,1,926,'2026-02-23 03:07:05','2026-02-23 03:07:05'),(31,1,NULL,'[DEADLINE] 제외 샘플 매진 D-2','남은 수량 0 제외 검증용','https://example.com/posters/deadline-soldout-d2.jpg','테스트홀 매진','서울 중구 테스트로 2','2026-02-25 16:00:00','2026-02-25 18:00:00',NULL,1,927,'2026-02-23 03:07:05','2026-02-23 03:07:05');
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
INSERT INTO `notification_platforms` VALUES (1,'ANDROID','안드로이드',1,1),(2,'IOS','iOS',1,2);
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
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 디바이스 토큰 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notification_token`
--

LOCK TABLES `notification_token` WRITE;
/*!40000 ALTER TABLE `notification_token` DISABLE KEYS */;
INSERT INTO `notification_token` VALUES (4,15,'d02NFsgiQBKRuhTSXZSHmr:APA91bE86VWAVlCp6g2lbVS4t4Q2tiAiFmgFUyz8DjZXaKhr83OmAj5HcF-KN8IZq7yaGJGW37FChM7HHrsRkqzKQwdzM3K34D5OQ-pGsBCCHk_Fs5AJtNk',1,'2026-02-24 19:14:20');
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
INSERT INTO `notification_types` VALUES (1,'CHAT_MESSAGE','채팅 메시지',1,1),(2,'PAYMENT_REQUEST','결제 요청',1,2),(3,'PAYMENT_SUCCESS','결제 완료',1,3),(4,'PURCHASE_CONFIRMED','구매 확정',1,4),(5,'DISPUTE_OPENED','신고 접수',1,5),(6,'DISPUTE_RESOLVED','신고 해결',1,6),(7,'REVIEW_REQUEST','리뷰 요청',1,7),(8,'SETTLEMENT_COMPLETED','정산 완료',1,8),(9,'SETTLEMENT_FAILED','정산 실패',1,9);
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
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='알림 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
INSERT INTO `notifications` VALUES (1,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"166\"}','2026-02-11 14:35:41'),(2,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"167\"}','2026-02-11 14:36:01'),(3,22,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"12\", \"messageId\": \"168\"}','2026-02-11 14:52:18'),(4,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"169\"}','2026-02-11 14:52:30'),(5,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"170\"}','2026-02-11 14:57:55'),(6,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"171\"}','2026-02-11 14:58:04'),(7,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"172\"}','2026-02-11 14:58:08'),(8,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"173\"}','2026-02-11 14:58:46'),(9,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"31\", \"senderId\": \"22\", \"messageId\": \"174\"}','2026-02-11 14:59:45'),(10,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"30\", \"senderId\": \"22\", \"messageId\": \"175\"}','2026-02-11 15:18:44'),(11,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"31\", \"senderId\": \"22\", \"messageId\": \"176\"}','2026-02-11 15:19:21'),(12,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"31\", \"senderId\": \"22\", \"messageId\": \"177\"}','2026-02-11 15:19:31'),(13,12,1,'새 메시지가 도착했습니다','채팅방에 새로운 메시지가 있습니다.',0,NULL,'{\"type\": \"CHAT_MESSAGE\", \"roomId\": \"31\", \"senderId\": \"22\", \"messageId\": \"178\"}','2026-02-11 15:19:42'),(14,12,1,'2024 월드 투어 서울','ㄱㄱ',0,NULL,'{\"body\": \"ㄱㄱ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㄱㄱ\", \"senderId\": \"22\", \"messageId\": \"179\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:33:34'),(15,12,1,'2024 월드 투어 서울','반가워용',0,NULL,'{\"body\": \"반가워용\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"반가워용\", \"senderId\": \"22\", \"messageId\": \"180\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:33:54'),(16,12,1,'2024 월드 투어 서울','그래용',0,NULL,'{\"body\": \"그래용\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"그래용\", \"senderId\": \"22\", \"messageId\": \"181\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:34:01'),(17,12,1,'2024 월드 투어 서울','ㅔㅔ',0,NULL,'{\"body\": \"ㅔㅔ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅔㅔ\", \"senderId\": \"22\", \"messageId\": \"182\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:34:09'),(18,12,1,'2024 월드 투어 서울','넹',0,NULL,'{\"body\": \"넹\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"넹\", \"senderId\": \"22\", \"messageId\": \"183\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:36:39'),(19,12,1,'2024 월드 투어 서울','ㅎㅇ',0,NULL,'{\"body\": \"ㅎㅇ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅎㅇ\", \"senderId\": \"22\", \"messageId\": \"184\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:51:50'),(20,22,1,'2024 월드 투어 서울','gd',0,NULL,'{\"body\": \"gd\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"gd\", \"senderId\": \"12\", \"messageId\": \"185\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:51:54'),(21,12,1,'2024 월드 투어 서울','๑°⌓°๑',0,NULL,'{\"body\": \"๑°⌓°๑\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"๑°⌓°๑\", \"senderId\": \"22\", \"messageId\": \"186\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:52:03'),(22,12,1,'2024 월드 투어 서울','ㅓㅓ',0,NULL,'{\"body\": \"ㅓㅓ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅓㅓ\", \"senderId\": \"22\", \"messageId\": \"187\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:52:10'),(23,12,1,'2024 월드 투어 서울','ㅔㅣㅓ',0,NULL,'{\"body\": \"ㅔㅣㅓ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅔㅣㅓ\", \"senderId\": \"22\", \"messageId\": \"188\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:52:25'),(24,12,1,'2024 월드 투어 서울','ㅏㅏㅏㅓㅠㅍ',0,NULL,'{\"body\": \"ㅏㅏㅏㅓㅠㅍ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅏㅏㅏㅓㅠㅍ\", \"senderId\": \"22\", \"messageId\": \"189\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:52:31'),(25,12,1,'2024 월드 투어 서울','ㅓㅓ',0,NULL,'{\"body\": \"ㅓㅓ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅓㅓ\", \"senderId\": \"22\", \"messageId\": \"190\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:52:43'),(26,12,1,'2024 월드 투어 서울','아니영',0,NULL,'{\"body\": \"아니영\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"아니영\", \"senderId\": \"22\", \"messageId\": \"191\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 15:53:07'),(27,12,1,'2024 월드 투어 서울','ㅇㅇ',0,NULL,'{\"body\": \"ㅇㅇ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅇㅇ\", \"senderId\": \"22\", \"messageId\": \"192\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 16:09:01'),(28,12,1,'2024 월드 투어 서울','ㅏㅏ',0,NULL,'{\"body\": \"ㅏㅏ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅏㅏ\", \"senderId\": \"22\", \"messageId\": \"193\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 16:09:07'),(29,12,1,'2024 월드 투어 서울','ㅔㅔ',0,NULL,'{\"body\": \"ㅔㅔ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"31\", \"message\": \"ㅔㅔ\", \"senderId\": \"22\", \"messageId\": \"194\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-11 16:09:08'),(30,12,1,'2024 월드 투어 서울','ㅎㅇㅎㅇ',0,NULL,'{\"body\": \"ㅎㅇㅎㅇ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"30\", \"message\": \"ㅎㅇㅎㅇ\", \"senderId\": \"22\", \"messageId\": \"195\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/40/5e82ad2b11674ebfae6fcf5680bac788.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80MC81ZTgyYWQyYjExNjc0ZWJmYWU2ZmNmNTY4MGJhYzc4OC5qcGciLCJpYXQiOjE3NzA4NTk5NDMsImV4cCI6MTc3MDg2MTc0M30.oc1n1atjuRJa78_FZgdZpU2-LiHfpdROWxqp_KrboUE\"}','2026-02-11 16:32:23'),(31,12,1,'2024 월드 투어 서울','ㅓㅓㅓ',0,NULL,'{\"body\": \"ㅓㅓㅓ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"30\", \"message\": \"ㅓㅓㅓ\", \"senderId\": \"22\", \"messageId\": \"196\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/40/5e82ad2b11674ebfae6fcf5680bac788.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80MC81ZTgyYWQyYjExNjc0ZWJmYWU2ZmNmNTY4MGJhYzc4OC5qcGciLCJpYXQiOjE3NzA4NTk5NDMsImV4cCI6MTc3MDg2MTc0M30.oc1n1atjuRJa78_FZgdZpU2-LiHfpdROWxqp_KrboUE\"}','2026-02-11 16:32:29'),(32,12,1,'2024 월드 투어 서울','ㅓㅓㅐ',0,NULL,'{\"body\": \"ㅓㅓㅐ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"30\", \"message\": \"ㅓㅓㅐ\", \"senderId\": \"22\", \"messageId\": \"197\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/40/5e82ad2b11674ebfae6fcf5680bac788.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80MC81ZTgyYWQyYjExNjc0ZWJmYWU2ZmNmNTY4MGJhYzc4OC5qcGciLCJpYXQiOjE3NzA4NTk5NDMsImV4cCI6MTc3MDg2MTc0M30.oc1n1atjuRJa78_FZgdZpU2-LiHfpdROWxqp_KrboUE\"}','2026-02-11 16:32:38'),(33,12,1,'2024 월드 투어 서울','ㅣㅣㅜ',0,NULL,'{\"body\": \"ㅣㅣㅜ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"30\", \"message\": \"ㅣㅣㅜ\", \"senderId\": \"22\", \"messageId\": \"198\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/40/5e82ad2b11674ebfae6fcf5680bac788.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80MC81ZTgyYWQyYjExNjc0ZWJmYWU2ZmNmNTY4MGJhYzc4OC5qcGciLCJpYXQiOjE3NzA4NTk5NDMsImV4cCI6MTc3MDg2MTc0M30.oc1n1atjuRJa78_FZgdZpU2-LiHfpdROWxqp_KrboUE\"}','2026-02-11 16:32:39'),(34,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"30\", \"transactionId\": \"10\"}','2026-02-11 16:35:25'),(35,12,1,'Bunnies Camp 2024','안녕하세요 티켓남았나요',0,NULL,'{\"body\": \"안녕하세요 티켓남았나요\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"32\", \"message\": \"안녕하세요 티켓남았나요\", \"senderId\": \"22\", \"messageId\": \"202\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 16:46:21'),(36,22,1,'Bunnies Camp 2024','네',0,NULL,'{\"body\": \"네\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"32\", \"message\": \"네\", \"senderId\": \"12\", \"messageId\": \"203\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 16:46:34'),(37,12,1,'Bunnies Camp 2024','2장 주세요',0,NULL,'{\"body\": \"2장 주세요\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"32\", \"message\": \"2장 주세요\", \"senderId\": \"22\", \"messageId\": \"204\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 16:46:43'),(38,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"32\", \"transactionId\": \"11\"}','2026-02-11 16:46:47'),(39,12,1,'Bunnies Camp 2024','안녕하세요',0,NULL,'{\"body\": \"안녕하세요\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"33\", \"message\": \"안녕하세요\", \"senderId\": \"22\", \"messageId\": \"207\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 17:15:09'),(40,12,1,'Bunnies Camp 2024','3장 주세요',0,NULL,'{\"body\": \"3장 주세요\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"33\", \"message\": \"3장 주세요\", \"senderId\": \"22\", \"messageId\": \"208\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 17:15:16'),(41,22,1,'Bunnies Camp 2024','네히',0,NULL,'{\"body\": \"네히\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"33\", \"message\": \"네히\", \"senderId\": \"12\", \"messageId\": \"209\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 17:15:21'),(42,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"33\", \"transactionId\": \"12\"}','2026-02-11 17:15:25'),(43,12,1,'Bunnies Camp 2024','ㅎㅇㅎㅇ',0,NULL,'{\"body\": \"ㅎㅇㅎㅇ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"34\", \"message\": \"ㅎㅇㅎㅇ\", \"senderId\": \"22\", \"messageId\": \"212\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 17:23:18'),(44,12,1,'Bunnies Camp 2024','하아요',0,NULL,'{\"body\": \"하아요\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"35\", \"message\": \"하아요\", \"senderId\": \"22\", \"messageId\": \"213\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 17:33:50'),(45,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"35\", \"transactionId\": \"13\"}','2026-02-11 17:34:09'),(46,12,1,'Bunnies Camp 2024','ㅎㅇ',0,NULL,'{\"body\": \"ㅎㅇ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"36\", \"message\": \"ㅎㅇ\", \"senderId\": \"22\", \"messageId\": \"216\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 18:52:43'),(47,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"36\", \"transactionId\": \"14\"}','2026-02-11 18:53:13'),(48,12,1,'Bunnies Camp 2024','ㅡㅏ',0,NULL,'{\"body\": \"ㅡㅏ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"37\", \"message\": \"ㅡㅏ\", \"senderId\": \"22\", \"messageId\": \"219\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 18:57:19'),(49,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"37\", \"transactionId\": \"15\"}','2026-02-11 18:57:24'),(50,12,1,'Bunnies Camp 2024','3장',0,NULL,'{\"body\": \"3장\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"38\", \"message\": \"3장\", \"senderId\": \"22\", \"messageId\": \"222\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 19:12:24'),(51,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"38\", \"transactionId\": \"16\"}','2026-02-11 19:12:32'),(52,12,1,'Bunnies Camp 2024','33',0,NULL,'{\"body\": \"33\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"39\", \"message\": \"33\", \"senderId\": \"22\", \"messageId\": \"225\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-11 21:00:34'),(53,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"39\", \"transactionId\": \"17\"}','2026-02-11 21:00:53'),(54,22,3,'결제가 완료되었습니다','결제가 정상적으로 완료되었습니다.',0,NULL,'{\"type\": \"PAYMENT_SUCCESS\", \"roomId\": \"39\", \"transactionId\": \"17\"}','2026-02-11 22:03:38'),(55,12,3,'결제가 완료되었습니다','구매자의 결제가 완료되었습니다.',0,NULL,'{\"type\": \"PAYMENT_SUCCESS\", \"roomId\": \"39\", \"transactionId\": \"17\"}','2026-02-11 22:03:38'),(56,12,4,'구매가 확정되었습니다','구매자가 거래를 확정했습니다.',0,NULL,'{\"type\": \"PURCHASE_CONFIRMED\", \"roomId\": \"39\", \"transactionId\": \"17\"}','2026-02-11 22:10:05'),(57,8,1,'2024 월드 투어 서울','ㅎㄹ',0,NULL,'{\"body\": \"ㅎㄹ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"2024 월드 투어 서울\", \"roomId\": \"40\", \"message\": \"ㅎㄹ\", \"senderId\": \"22\", \"messageId\": \"229\", \"messageType\": \"TEXT\", \"ticketTitle\": \"2024 월드 투어 서울\", \"ticketImageUrl\": \"\"}','2026-02-12 15:59:40'),(58,22,1,'Bunnies Camp 2024','hello i want 2 ticket',0,NULL,'{\"body\": \"hello i want 2 ticket\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"43\", \"message\": \"hello i want 2 ticket\", \"senderId\": \"12\", \"messageId\": \"231\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/42/94592dd04b9e4cc3ada10926d5dc5ba1.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80Mi85NDU5MmRkMDRiOWU0Y2MzYWRhMTA5MjZkNWRjNWJhMS5qcGciLCJpYXQiOjE3NzE4MjY1MzgsImV4cCI6MTc3MTgzMDEzOH0.pPzD3qRx2jNns4NmkuMVsBjmpHZY7eOJXxNQz6vOMqI\"}','2026-02-22 21:08:16'),(59,12,1,'Bunnies Camp 2024','good',0,NULL,'{\"body\": \"good\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"43\", \"message\": \"good\", \"senderId\": \"22\", \"messageId\": \"232\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/42/94592dd04b9e4cc3ada10926d5dc5ba1.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80Mi85NDU5MmRkMDRiOWU0Y2MzYWRhMTA5MjZkNWRjNWJhMS5qcGciLCJpYXQiOjE3NzE4MjY1MzgsImV4cCI6MTc3MTgzMDEzOH0.pPzD3qRx2jNns4NmkuMVsBjmpHZY7eOJXxNQz6vOMqI\"}','2026-02-22 21:08:27'),(60,12,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"43\", \"transactionId\": \"18\"}','2026-02-22 21:08:57'),(61,12,1,'Bunnies Camp 2024','남은거 다 구매함 ㅋ',0,NULL,'{\"body\": \"남은거 다 구매함 ㅋ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"44\", \"message\": \"남은거 다 구매함 ㅋ\", \"senderId\": \"22\", \"messageId\": \"236\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-22 22:47:34'),(62,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"44\", \"transactionId\": \"19\"}','2026-02-22 22:48:40'),(63,22,3,'결제가 완료되었습니다','결제가 정상적으로 완료되었습니다.',0,NULL,'{\"type\": \"PAYMENT_SUCCESS\", \"roomId\": \"44\", \"transactionId\": \"19\"}','2026-02-22 22:55:18'),(64,12,3,'결제가 완료되었습니다','구매자의 결제가 완료되었습니다.',0,NULL,'{\"type\": \"PAYMENT_SUCCESS\", \"roomId\": \"44\", \"transactionId\": \"19\"}','2026-02-22 22:55:18'),(65,12,4,'구매가 확정되었습니다','구매자가 거래를 확정했습니다.',0,NULL,'{\"type\": \"PURCHASE_CONFIRMED\", \"roomId\": \"44\", \"transactionId\": \"19\"}','2026-02-23 14:23:32'),(66,22,7,'거래는 어떠셨나요?','test success 판매자에 대한 리뷰를 남겨주세요.',0,NULL,'{\"type\": \"REVIEW_REQUEST\", \"roomId\": \"44\", \"transactionId\": \"19\"}','2026-02-23 14:23:32'),(67,12,1,'Bunnies Camp 2024','ㅇㅇ',0,NULL,'{\"body\": \"ㅇㅇ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"45\", \"message\": \"ㅇㅇ\", \"senderId\": \"22\", \"messageId\": \"241\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"\"}','2026-02-23 15:53:18'),(68,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"45\", \"transactionId\": \"20\"}','2026-02-23 15:53:26'),(69,27,1,'Bunnies Camp 2024','ㅎㅇ4장만 좀 살게',0,NULL,'{\"body\": \"ㅎㅇ4장만 좀 살게\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"46\", \"message\": \"ㅎㅇ4장만 좀 살게\", \"senderId\": \"22\", \"messageId\": \"246\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/46/ac510d4dd4224634b32ad425903712b6.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80Ni9hYzUxMGQ0ZGQ0MjI0NjM0YjMyYWQ0MjU5MDM3MTJiNi5qcGciLCJpYXQiOjE3NzE5MTc1MDgsImV4cCI6MTc3MTkyMTEwOH0.xBAlWLeKVzYjyv7xGIuE1ef_7-iQus3pHS_nqKrYkTk\"}','2026-02-23 22:19:18'),(70,27,1,'Bunnies Camp 2024','ㅎㄹㅎㄹ',0,NULL,'{\"body\": \"ㅎㄹㅎㄹ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"46\", \"message\": \"ㅎㄹㅎㄹ\", \"senderId\": \"22\", \"messageId\": \"247\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/46/ac510d4dd4224634b32ad425903712b6.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80Ni9hYzUxMGQ0ZGQ0MjI0NjM0YjMyYWQ0MjU5MDM3MTJiNi5qcGciLCJpYXQiOjE3NzE5MTc1MDgsImV4cCI6MTc3MTkyMTEwOH0.xBAlWLeKVzYjyv7xGIuE1ef_7-iQus3pHS_nqKrYkTk\"}','2026-02-23 22:22:26'),(71,22,1,'Bunnies Camp 2024','ㅎㅇ ㅍㅍ',0,NULL,'{\"body\": \"ㅎㅇ ㅍㅍ\", \"type\": \"CHAT_MESSAGE\", \"title\": \"Bunnies Camp 2024\", \"roomId\": \"46\", \"message\": \"ㅎㅇ ㅍㅍ\", \"senderId\": \"27\", \"messageId\": \"248\", \"messageType\": \"TEXT\", \"ticketTitle\": \"Bunnies Camp 2024\", \"ticketImageUrl\": \"https://nbtsfiwerdxprhsoidrm.supabase.co/storage/v1/object/sign/ticket-image/tickets/46/ac510d4dd4224634b32ad425903712b6.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV83MjA4NzhkMC02MzZjLTRlNzEtOTZiYS1lNmE3YjYxMGI3MDQiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJ0aWNrZXQtaW1hZ2UvdGlja2V0cy80Ni9hYzUxMGQ0ZGQ0MjI0NjM0YjMyYWQ0MjU5MDM3MTJiNi5qcGciLCJpYXQiOjE3NzE5MTc1MDgsImV4cCI6MTc3MTkyMTEwOH0.xBAlWLeKVzYjyv7xGIuE1ef_7-iQus3pHS_nqKrYkTk\"}','2026-02-23 22:22:35'),(72,22,2,'결제 요청이 도착했습니다','판매자가 결제를 요청했습니다.',0,NULL,'{\"type\": \"PAYMENT_REQUEST\", \"roomId\": \"46\", \"transactionId\": \"21\"}','2026-02-23 22:22:43'),(73,22,3,'결제가 완료되었습니다','결제가 정상적으로 완료되었습니다.',0,NULL,'{\"type\": \"PAYMENT_SUCCESS\", \"roomId\": \"46\", \"transactionId\": \"21\"}','2026-02-23 22:36:12'),(74,27,3,'결제가 완료되었습니다','구매자의 결제가 완료되었습니다.',0,NULL,'{\"type\": \"PAYMENT_SUCCESS\", \"roomId\": \"46\", \"transactionId\": \"21\"}','2026-02-23 22:36:12');
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_card_details`
--

DROP TABLE IF EXISTS `payment_card_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_card_details` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `payment_id` bigint unsigned NOT NULL COMMENT 'payments FK',
  `company` varchar(50) NOT NULL COMMENT '카드사명',
  `card_number` varchar(20) NOT NULL COMMENT '마스킹된 카드번호 (PCI DSS 준수)',
  `installment_plan_months` int NOT NULL DEFAULT '0' COMMENT '할부 개월 수',
  `approve_no` varchar(50) NOT NULL COMMENT '승인번호',
  `card_type` varchar(20) NOT NULL COMMENT '신용/체크',
  `owner_type` varchar(20) NOT NULL COMMENT '개인/법인',
  `acquire_status` varchar(50) NOT NULL COMMENT '매입 상태',
  `is_interest_free` tinyint(1) NOT NULL DEFAULT '0' COMMENT '무이자 여부',
  `issuer_code` varchar(10) DEFAULT NULL COMMENT '카드 발급사 코드',
  `acquirer_code` varchar(10) DEFAULT NULL COMMENT '카드 매입사 코드',
  `interest_payer` varchar(20) DEFAULT NULL COMMENT '무이자 할부 부담자 (BUYER/CARD_COMPANY/MERCHANT)',
  `use_card_point` tinyint(1) NOT NULL DEFAULT '0' COMMENT '카드 포인트 사용 여부',
  `amount` bigint unsigned NOT NULL COMMENT '카드 결제 금액',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_card_details_payment` (`payment_id`),
  KEY `idx_card_details_issuer` (`issuer_code`),
  KEY `idx_card_details_acquirer` (`acquirer_code`),
  CONSTRAINT `fk_card_details_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='카드 결제 상세 정보 (PCI DSS 주의: 마스킹된 정보만 저장)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_card_details`
--

LOCK TABLES `payment_card_details` WRITE;
/*!40000 ALTER TABLE `payment_card_details` DISABLE KEYS */;
INSERT INTO `payment_card_details` VALUES (1,12,'UNKNOWN','53275011****425*',0,'00000000','신용','개인','READY',0,'24','21',NULL,0,450000,'2026-02-11 22:03:37','2026-02-12 07:03:37'),(2,13,'토스페이','53275074****033*',0,'00000000','신용','개인','READY',0,'24','21',NULL,0,1050000,'2026-02-22 22:55:17','2026-02-23 07:55:17'),(3,14,'토스페이','53275074****033*',0,'00000000','신용','개인','READY',0,'24','21',NULL,0,480000,'2026-02-23 22:36:11','2026-02-24 07:36:11');
/*!40000 ALTER TABLE `payment_card_details` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_cash_receipts`
--

DROP TABLE IF EXISTS `payment_cash_receipts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_cash_receipts` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `payment_id` bigint unsigned NOT NULL COMMENT 'payments FK',
  `receipt_type` varchar(20) NOT NULL COMMENT '소득공제/지출증빙',
  `receipt_key` varchar(255) NOT NULL COMMENT '현금영수증 키',
  `issue_number` varchar(50) NOT NULL COMMENT '발급 번호',
  `receipt_url` varchar(500) NOT NULL COMMENT '현금영수증 URL',
  `amount` bigint unsigned NOT NULL COMMENT '현금영수증 금액',
  `tax_free_amount` bigint unsigned NOT NULL DEFAULT '0' COMMENT '비과세 금액',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_cash_receipt_key` (`receipt_key`),
  KEY `idx_cash_receipt_payment` (`payment_id`),
  CONSTRAINT `fk_cash_receipt_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='현금영수증 정보 (1:N 관계 허용)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_cash_receipts`
--

LOCK TABLES `payment_cash_receipts` WRITE;
/*!40000 ALTER TABLE `payment_cash_receipts` DISABLE KEYS */;
/*!40000 ALTER TABLE `payment_cash_receipts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_easy_pay_details`
--

DROP TABLE IF EXISTS `payment_easy_pay_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_easy_pay_details` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `payment_id` bigint unsigned NOT NULL COMMENT 'payments FK',
  `provider` varchar(50) NOT NULL COMMENT '간편결제 제공자 (토스페이/카카오페이/네이버페이)',
  `amount` bigint unsigned NOT NULL COMMENT '간편결제 금액',
  `discount_amount` bigint unsigned NOT NULL DEFAULT '0' COMMENT '할인 금액',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_easy_pay_details_payment` (`payment_id`),
  KEY `idx_easy_pay_details_provider` (`provider`),
  CONSTRAINT `fk_easy_pay_details_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='간편결제 상세 정보';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_easy_pay_details`
--

LOCK TABLES `payment_easy_pay_details` WRITE;
/*!40000 ALTER TABLE `payment_easy_pay_details` DISABLE KEYS */;
INSERT INTO `payment_easy_pay_details` VALUES (1,13,'토스페이',0,0,'2026-02-22 22:55:17','2026-02-23 07:55:17'),(2,14,'토스페이',0,0,'2026-02-23 22:36:11','2026-02-24 07:36:11');
/*!40000 ALTER TABLE `payment_easy_pay_details` ENABLE KEYS */;
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
INSERT INTO `payment_methods` VALUES (1,'card','카드',1,1),(2,'virtual_account','가상계좌',1,2),(3,'transfer','계좌이체',1,3),(4,'mobile','휴대폰',1,4),(5,'easy_pay','간편결제',1,5);
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
INSERT INTO `payment_statuses` VALUES (1,'pending','결제 대기',1,1),(2,'paid','결제 완료',1,2),(3,'cancelled','결제 취소',1,3);
/*!40000 ALTER TABLE `payment_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_transactions`
--

DROP TABLE IF EXISTS `payment_transactions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_transactions` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `payment_id` bigint unsigned NOT NULL COMMENT 'payments FK',
  `transaction_key` varchar(255) NOT NULL COMMENT '거래 키 (토스페이먼츠 제공)',
  `transaction_type` varchar(50) NOT NULL COMMENT '거래 유형 (PAYMENT, CANCEL, PARTIAL_CANCEL)',
  `amount` bigint unsigned NOT NULL COMMENT '거래 금액',
  `balance_amount` bigint unsigned DEFAULT NULL COMMENT '잔액 (부분 취소 후 잔여 금액)',
  `tax_free_amount` bigint unsigned NOT NULL DEFAULT '0' COMMENT '비과세 금액',
  `currency` char(3) NOT NULL DEFAULT 'KRW' COMMENT '통화 코드 (ISO-4217)',
  `status` varchar(50) NOT NULL COMMENT '거래 상태 (DONE, FAILED, PENDING)',
  `reason` text COMMENT '거래 사유 (취소 시 필수)',
  `toss_response` text COMMENT '토스 API 전체 응답 (암호화 필수, Base64 인코딩된 암호문)',
  `event_at` timestamp NULL DEFAULT NULL COMMENT '토스 이벤트 발생 시각 (API 제공)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP COMMENT '저장 시각 (UTC)',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_payment_txn_key` (`transaction_key`),
  KEY `idx_payment_txn_type` (`transaction_type`),
  KEY `idx_payment_txn_payment_created` (`payment_id`,`created_at`),
  CONSTRAINT `fk_payment_txn_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 거래 히스토리 (승인/취소/부분취소 모든 이벤트 추적)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_transactions`
--

LOCK TABLES `payment_transactions` WRITE;
/*!40000 ALTER TABLE `payment_transactions` DISABLE KEYS */;
INSERT INTO `payment_transactions` VALUES (3,6,'tgen_20260130145023qODz3','PAYMENT',360000,360000,0,'KRW','DONE',NULL,'lrh1MLe/D3Fat0B+Ivh77HOkN4QjowOBsDLSOoqELFMlkG3OJ4yCtv7ucm2NA7yq6i/NzrpT0YurlwLcJf4VtaoxPsprTBbg8sGbpPfTgWtV+8ZdO5KsfoJ/jid7VZOv8Vlc2NXv4sO9cZzGkHvdDBTKHYHWpl/4lgbXvxedFRVDB7SsDEI7ba3DFq2ctRW0bz38cfUYIpRQ7paSosURsKiIeZstN3wUfZl4xTQYUfl5AyKuMfpfsY2hKJ4l0hdF9PMlcYtoT/VEJ/xZ7A59lavKgf0moJDCfNZU81HoGPqNMxga7Pjq4rW1K9kUs87qLd0xhnirSy5JL/enjBXbUraaZL4zW/sEdkkoFTw52R3kIH3hb3lpSuF3kPM23S2SqU4DXEKImcCucLzgxh07Bk0WRDGA91KlUFziqNs/cDZCtr7FPMXO+3mVu5OOLkWZ2YBDhZpt0RpeRcYO0dviAmjPXYY3oYjqrAnRmHAb6Xr27FZR1JHAjA0J0tnwKK0dS3Y5PpUZgcq2tu4th2Gl6EGSwcKRLziAvKmc3OHpRiscH+rahjRzC61ionkXqeQvBgAxfCN3Grk0QIxACvxgc2zWnHsw0VkBUm1E0BI+g9LZQdHYJ+DKchNGCQ0CcXAGhMB9aD7KOQbALy0Rf7TjAmODCdX2ljEb2Jx22bjiVrLmcvmw5Zu4pgcuEAUnIhhATn6DrYDh1zvYSFVBZhT5NW91AG8iZP52CAXyDBWrc7NkqTZEJbHkStfxgrdd0mFF+tJUYHbPFM3JE0SG0B56zHCekNVTYIBEnWdxqqbB678M6Ag1HRap75oM6+YBL6aWgALF1pCMC/nDx8S4NpcyM+j+1SNIqu8CIXIFmNgL48VrJAdW3ZBX4UgeTjMXn8dcnxQ4LoP6zSfRKWjQOZMM/Zafj8T46M3BtJ02GMTCxoA9p8j3c+wvOoT2OT26F+zG25rJTLwCaqtQocoHk52ZVqFPeL99OyCPo2kFBN0IYtUstFqaq74IfdWdDTr5lTL5xybW/iuZS/WdtMcVfgpiBTyCUNrY8KC89NZZUEHIzMChBuEka2FoKea+sFOVANDasLT8euAU5On1bO09vOpG03V0mCAnQMuFrQTmi/xc2Uo+VZRTLQr4bFYrV10T2b0oWyW0rRr+t509MznKMEfDY9QFjb16Yq3nPetWEVcRTU01HQwXiO8S4DenUtF0qNVVlkZLoBr53tRrUWWQmcazkphFipEu2qYzXG4EsYOnKN01bh63xRk0Ma4XuyEJsSLU00e0wTvPEn1Vedj3KipYBUHXfbhlhGP2q73MOux9XJ0tMLcIsRsXfbvkPvzK+OwIzTYdimlgQnpH2zjHl2SNofwRW1rH0Z6tFthWTSg1sLwgVChApuLAus0kdjZLdT305rAsX1Jxu4tbFq8Y0f7mE9mkIQwENj5L7lUTExmJ4E/X9xptdAfF87zksuaevxSr9zAfpNlUYJvzsmjfaXqaDZqPOONHFKJlB9edHaxrcP5+4wltgC7cQuGBrMsxsCbMp/L5weEOyVwmlrw8tU6SaCTtsYyxOLBCjBxPHynxI3eDYdHcRHSZ1BXyvRcMGA+UvncuMllcx4c7U5U9RJuobP4bVot4wlXUYWGsemnHrGHhiZ9716Deo7RCi1Lcpks1kVNaLSMj1IqVVTsVWyUWYXmAG8pZMfZGkVVbST+idWYwoM99Xpey9Z2iHGIqBff5QGg=','2026-01-29 20:50:35','2026-01-29 20:50:36'),(4,7,'tgen_20260130151330rV6d3','PAYMENT',5000,5000,0,'KRW','DONE',NULL,'XtfmoXE2AOzdZu3OyaLotCtIk702ftM9FikGNdJLiEaTKuqvUJxGBq0MrYlI4eQiVeN8GtzfhJBS4que9+1uwEV5KrfZs6viAOL2x5Tux9sQuQO5WWD7Gj8d7yN1EEckGVu0BcUOlVJFQUzpFOBcAgB4ypcGiUuQu5Jf0XSKWxXD801qX80kc1R4pophaT2IQt7Jj8stWAx+I9d//O9ctjq4SVWm5cUbxJVbMDcZRwW1juyEu4uzs5u2axlNogEsY4OoShBW++RfLH4XngxS8JBKtLDx3SPI/Hwitsw+gM+IwmVTtIklO+XA06qN+gOK+rBjDRYZE5juv1y4+q+v7g/yIYAfWqKEPX5oqvmZdLiOkj1cnrNLXiT3bqjOjOcxon922o1XAfqh2g0x81QkkwrvF2vQmmxAOwfEvoWO8tH4HITWk1BDCnC5sq4jadyXQrs/5M5ITTAQmn0toYZf0tTJH3YCK/YLNDFQK5LpeX1gaRkLZ70V0kWp4lm1pqgjv5tVJDle93POflb8TqIljDXQzCKCspCvAFmesjvu8Rf2rPtlBgyYVhlh0LRWL+KLFHL0/HrKx8azX8VlC1dSRjLCymsiQise4rUz0VQHvpbuNZfyXofk5SjFTlUd0Aoe1kTbbfWFK0BUBwkJWnv1zHEiTJzJkEPh9RouJ+kiXoLHmSaQU81qfDNpgQt2scn/gGgkRzYgpBx0G2tT3xAT65idVwYYEselv+TW8Mcz2rYnGorIWg3SH2Wze6AxMm4h0hCmyTK8tRdyQsG9kLAOGrw4q4obyaO95EQkuuaeJ5/gwWLM17eLMQ4LyqHiCDEa/7Gl5vf6Gzu/oRZek2cL8QXne1vcX8ZOQ/0COmceFKy2aJF99M0jnMfjuK9RfbGXgHBB8oEfNFvfTYnhFXFNaQfPMcjZyiq3KYljdz9V50OlCTnNx+jL6cCq2uYLceIn0eGYkIwx/DWkfQc3+2kFP6hep59o273jxlADFOjAG8890kgMvW2YyJbPev1KqRTNVAYET6pDrVlsYveQr7oRqBz48y3sGDkZVcUPHLm+U0L7UUgMksuy2DQhOO7l1B4jsT8T11+Ow+qog2bbjobGOicRJPrjW+qEXu4GPi8lNtxZEb284AAsHaWWYX70hx3ylBVWzwswvDfjmS+g+75eFZPPBa0jGy76Fr4UC33Um/lCMbYIBteJIkioJPZ8/plRwxbcgyPJLadh0F2KjJc3V0RfyoEc4hLRBXmTYHRHNaKlN5ZwvnssHqE2ph9Ap+38KnkEGj1jYWuW+izZKxG+6Pg+sEUVDAypFd2G/sYnpS+xu3KVq1h0Rg/2q+/MRAGA86s+dKcS6w9IuXpUhlz6dLocwvt868onfkx0p0Rs8C6Onuvll79IjgafapF7/MoqpkNiJLdLbdebS7JKUyqcQBVEKYfSn/0Xzff3aitobeIZKH8FR4sM0Orb6EgoJVZKshVeIRgy+lTq0YZpewto6VqfpG4pvqcxNjm6FMf2MebZ59fRbgFXTCSc6F16sm8wQM2E4Ek7oErSOyEX2QsayhO/SH9rf0rrwnO3XRgq3VhWmNxB4hObGJHVzBwLRVz2RL6X71F7TSc3MpEZlQ/8RDJaodB/M+Ir75Nqhws2B6LgxZhz8yIoqh4QAfK0sEuDUdpQO88EZnLTajJbt9Jy9x+zTLP9KP4fHEXQLQ==','2026-01-29 21:13:59','2026-01-29 21:14:00'),(5,8,'tgen_20260130154459tIdu2','PAYMENT',180000,180000,0,'KRW','DONE',NULL,'MbZ9VZriMMvfICrTdViTW0cjacZntxFYY4M2IMDetKiO7WrB06Bd7wIF8Mf22UbAhYdYM0nm4NnxVwnzCRJKZKSB0m390pFmAJ+gzq4zA4jaY/urZF4MSKqzVnIFGO/tHjP7hThdemSu4hzV7hH9y6cFMDkHohPJPQsbNbkv6TzXijcrvE6Y3Co0Lp+kk2xvSm8bDnuLo3olKfvrPScFgTXLv/9QP1BD4EGAfvBRemt4rHKcNFrB5FozeyIESym30jDzPLPNubyyte5+FulkZja1i+CVMqfD5FQ9wHT0uReaHOxjCfTygx58gsv/Hvnxr+tTovPYMM7kZHgaI0lSN1BjMOVlFejyoM32LFVJVWWauUulj9rxTJH9Y9IaNr5ORlKIfTi0SSibau4E3O1UlwP4S6HJQUG7cq1Us7ZcLMx3sGaxYXsWzpA7/Pdstlj/qifRtQ8MnLN3eJV0t7ApjZFYcycrz6X7UnBuQZP60JJbNqYQaIm125+Y9EOwxlctg6KXg5f+BWKZ2jtIMQuw4wo1sBLOwRmmGPFgCdzJCOxjkJpMml/wcN67yJEvW/ecc48/uWjtqt3WFqL4h7N0AQou2+beTwDSpA5xD54JRul+cqEqzH5UJkVtND5fSrB9PbxV0nCZuJbVd7k+7BrooM7F/pHwpEhj+0xAdVIl80JnD+PaisctTq/ex+4vlmQb3PJEglUlo16HRl7PyzKgIdO9jWXyLxUj55nAVZFoEy6ijAx9b33r6oSzd79exkwY1NYGQGZjB8TIZ4ffO16LCxPVoAXbvWkAnBfY8cprgd6WMx4vc1RQ9TO+LwIX2is0R2Ch6BT3YX0DhxDzPnfgJR6Q9C66vaXASQSI3arzJj4cgr4joIrvmqWk2xa2jK+6+r5GYlDOnXlBkGpibIbozX6SyWaMM2Rv/EL1AU/EdG6IXbdXjGB/HTdJ7lJKRtUp5TcJFCvl+5TGGaeJcZrRVqiyxYzlurIVLNhBOqyCA6MGt6Ys/Z28CSoOHzoMkWJxF7JMU3+EsuVTeJyU9PBodg9zw+VDdqZsJ3eq22fN6AVKRLC8QrB5VNtbU142Zwv/yd/CjSR5rfSf++gNAx+ZSuCgag75FsN1yoa0bL/b+9H02uFVoqRv4GyNSdh4W270C9Sf/1ns0Wy1MfE++hSiQXkYHU0E2JGYaRfhLEYHTCqRposJRsAkg0vb7fYc9cOdEjPYMeUYvjPwILSX24288r05QliJt94ScKQ+76jt6grrq2Gp8vE3ERx21sfXcFkQBvEfvxlwI+0hpWRaApPfeFVQ3Qmq8FXt9gwjDUxQ+lYNs/80qA804osBvPhlp2IxE4mAjRozbYaeiRyyyx2m2ot7dXhy9nvK1fgwP1HIBJeoBZv8McvTSke6f9mSOz+ZELks7AZoE0XXthxnyWBjXD9A7oSLBBT5CI4kgKoST6A0xdiST/VmetFiXErRH4AaMr/IF1JX9ynGB+U5H6BS51aR7MkIzXpRuYMDcDjqDnA8InP5q2JiH/KzPCUaTfy620LE1GmE0aMJpcFNRbG+/LHqx6rbARaRK3jzpYJVfbMQHDMfWEEXixJHbcfHCVkuyGVrYQgYtpvoHFt3kIMgB1itALAXxJTTjuktL9x3ebJLk/mnHQ==','2026-01-29 21:45:27','2026-01-29 21:45:28'),(6,9,'tgen_20260202131310sZzZ5','PAYMENT',300000,300000,0,'KRW','DONE',NULL,'a44gjr7bnYr0vsBOpAotZtg9O6ohCbMhJKMYtNmfCGjRywf9SrqJpa2imvFCjaIbGPMsZAxQwlRlGoIPfGewe6Dn9s+st720YwDKrPW9WKQL6jBKJnS2ljRzdOO4Ts4Tf1Xeq6reR3x0IvX9gXArgkTrvZgNGdaTQsvSGvH5gEIv3PLwgM9lv+teW6RpUpVNlCg23w+B7CgI6lg/SWG/2iQV6u4hbogj7m6JK/Lo/nf3lI6U7nuhimYyReU6QIwBq3GLzIJo+y0TW1x03aq9EJqxjuSHAyYo0/AVuBziKMfGkkBKWSH5YH0JTcBVYnKaGFSpLjx+CyN9IBvkgwV+xDDXuJiYEenNkddYV+05+zHSEt0HdUKY0dp+089tDu4EfhPFQQFavhS+O1nClmEZqYwjt7Hq2lyzAmFvjHrGL0qsbcuk2LNwpwsRKGNuWHu37vPTvbKkdCa9vzoAmjqR0ZeX1lJKmk5kSV6pld7jUz2yeq3TRl0I2Oe9J14zQi11GJOuikFycCIUGBRYiOwtd5uYIMOV9pqggvxtVyIsBkzteymPVLzQN/FKgZePt1neMVs9tTFW/w3nWNhKZMWjKuo5+Ayq5zu/auJq0/Buu+POfBByA94YPFPCV09bAa7AvC1PtuSMXt3DVMK1y+atKNtdgjarYuqUGgHHr+NJQYbdCFxaz3uc/8JfUMQHG2uFHxVh6mGVrytkcc+EDJX8U1lRUtGqwiC7ClnyD6FKIl5cA//GjnxJs+Y78dczk/Lq2MB58cQYL0Iu6Uo4PImCoAEetL/aah8IyqUjsRjDXUCz2iGYkoabOOZkvWS+Pf1VIxMcNqO0Jw/gPsud5hXqVN4sQn73QfL6SP557sQPiDxNcBUyBu6gWri3cm6MymZ4E8QZ6ER5Iv8VY4rNFFJLywidDbQSsLQJABC1NqvRp4qepi9mLoizVRBsMW5OGIJG4C7VF/hLN7/Cep040pXNm+4HCE+PQ/9UJ8VTRLia58+I4rCeqgRVefvzFbh0br+1KniBq5VP9V9N2diyu1Ife/mks/MpOO9cAvqqnbEYehxi2w9aD3BWOou1Nvj+01ulm3aVgNBInQovYOJYFe24hS/euojtI0idjm3Rr8+7pdYbIys3eH1ibm3X2E3uqrL+Aq1aJx1YBzGW7EpMBXyd+imjBpSxtkIhp9HCAX1Fm7N9IDhm6HiCMgFeUzei7C67FftrFsViQ3Hl03YdUvuWxFFZz9/lbGdPWTPr1g0FRdHMLmJiS58t/jbxWCqP+mFBLeSEDKWcwpNO2iB473JIGBKo0p/h2EV77KQ2j/h7apBG4QTzchqTKyqKLELdDPQT893qZ24kl0G4F0+E+DVGLDar3rLPztjSmiyM5QNlOPzuDGvnwgOB1rY4nyTGihOKFlkKpCMJqVH0rJ4x7yVfXNZnF178bMfRUm74gudshKHTwrrVyRlfOVCSN/Rs2rslgRzyZ2+lJNTMm5TnpRuaU4j3hxIjhAxwqhPdHvYg3V/6YZWQO1l62c8i6hnNGgeye5B2FAn/Fnbn/EbxdWe+BZ+RfuLjRcNfqZstFgNqk+0vRSgMLIT36Ujz4Bb5rOIqgllPc74A/850mwbW7Yh2EmLxMFl31bcAiXcIocMxkYJL48hz','2026-02-01 19:13:27','2026-02-01 19:13:28'),(7,10,'tgen_20260202132432tfoi6','PAYMENT',180000,180000,0,'KRW','DONE',NULL,'cwJ8K1XHQy/gGQIpkMcFSNvD1miTOdRFVEJYRuwIljB3daRNCHpPIq20J0Q3iLbo3d2uY9TV7SyPuH/+/xemfw/T+vUIqtQSK9xfLrtSIqKPKuBJNW/7RFUmFV9HanFExIW77MN1V8ly5PK6mKWtgwkX4XQ+DGQBMCNBw3i7e5DQTkK+vFUlApSowkFYqyDDiKwBCSPS7wGiHNVcJcDkMB8iBK/MGo1mxgG2wY/RxsU9dFhh5q+WD7PblVQRQZicvIDo0pKhti0jlGxaBzDZUkHsr3ReZUX1hRc+9aRvJw/7qW9XITvBbGd05U1MNzzUrcfH/fLz9YWv/BYPNGh6+cBdEVlM7e2LHucSXyOTto8BnZRJ1bjOAuxbzWjOImc1Au6zvXPOTRLjXPl+gJGhOgZkSatJRRP5TpLsKIdv4I2eFo2IYPPfpNa/E6irBtPfYxOT1aYjEVVo86JXY+w1H4KyCGItSSU+vfbotQgw2yni348sug1dZXE7JGLqxjSCM0R+pJ6zN3sonjU2Q7E52sWtU7U4s/OYxjZwKekelblp3oi1fB3cPJySHypDgnfFd/LqhfeD1weB+I91sT1zHavkrT10GScEO6kpd1D9XU3wc3fBqSqPULDpERFaAnnBFk7VlI0bPeXdjQG01Sv2VoxE0kWCXmgE7zo0MRdbOK5kQXZ09xtrBuP0SNn8l8IkmshRLjAglxu9nVpXLUSVn50/rmTeE8e7hqFc1UIjJGJ8ilqWQCVBGQMPqsgTHJx4gQYPEdlgrXgM3VI2KEXKLJr0sK/eUdRyJ+HmGqHz/WI6v9eXkD/rRTn52TVjA8mVXqAzBFUo/MFeQ15vgwufpCnOKTYHX0NWgef7izYc8l6SLCdhAkrbRneje1XQIqBndohx5Mhh3Tir+OhDojj3L3STAH+tDtY6lPvp4lxzwTk8JOwsznTJf82cKW5EYoACAnrpXPohOiArLrGsWT5UBA2DxF5jpD8YMWD/SHHfZblkGt67H1suESxIBh3aP4Mvp5Elnwcz6GmFlQXnOE+Q6nKRHDJec/DwSR/hQio+87TnS6XKEyelQNs6vcX6JNFii3Gy3je0itBqVy0am1vP9v5KtWw+5UptbYeLPszLWz6Qwgy3wdWKCpYh3c62ZUnmrnDzwE7Qjct0264gHSfdqrj8fG0xiB9VpOOCcyWF5Y7RuZjB71dDCNlIzAh70oNmNO9Q/fEWfD94id2oXyGV2JRNYR5DpOH1j8j/ZM8C3QijwaG/mIrgw0UWGr/mWs1HOgLWlMOm0K19027hb92GC80Qh5R7XFKyQlKF4nCmNpGLB1TfWlZcwvWMjsDZIhctaaV828o05E2SGz+ivYsHxLOkQBxEedCeqRX2MZhkpC3/xcUQyP4bVUHLzEbqAYK0hfJe2mwtuETmq+g6OLzDCPNnW/QSs1OmfCohC5n8UBqJ+2Cf7nCH6CP3KTICBaNRRwAY3UeQJTfrp81QxawsmadABRcFIxPPlGb+Ef712jiyVGEab1YKpCsBtApUTB4uyAFzu3izbBAtZC5QntWV68iil20dbCaNGzzyd2rjbXSHIT7qNvE6FbrA/yBnOOrkcxXYGU6y5Id5mQdbmf+dI6SK3YMYYJoWhiwRQhZDeIjG47Xf6A==','2026-02-01 19:24:41','2026-02-01 19:24:42'),(8,12,'tviva20260212160318oKWT5','PAYMENT',450000,450000,0,'KRW','DONE',NULL,'vgpOP+MqPHwy8JWvddSKDgXlbYflmotdeGCIPiSTN5WiZBxzkiDzF75UIOxJfO6kL9ZJWahWtEspuvNspWiDblE+1oFGwITPOUdOP6Wveay8riRZui13kEe/lncfxto3XSd8l3Jge7K4PI88MhmYalY3iCcc178XBnG3sHSrRqxaXG8csRBMwZa1FAmf6NFJBMZ0pRIUkDY1OPbmtzp5fLLLMZMLTKXz8Lfbcl+1eWCJGSpo8sIjEjklmVzRioupILnzTLXc7x4b72/j16IF5HFrbI+iLnFKqK7BdMRjAzV6T99VgfmNjtiiyq+BPIDqgrmslBhERs7IhAQgFcJKPORHPDfgVGjnJz6aGd/mUgkeU8uAhFre1Yq0JVrjtDfzYqNBDGzjrCG3BVLMZx2cCKTuqWDlUiVCI+/ubBbVYbpHkEzL0Td3du1u5FC9BZg9ukuBSh3xza6jDejYGizFfVwpu4NY1DMpmuDS/Wll8LS5gybhvpt+SvTV9nwI7M/SCLDJSGs1Q6A8KYJ7NubSxFpdQb6e7wYaa6jCBP9Mfq/FDf4YAwj9hxLSbOMwfkANqdt/6wAYm4kGhURvIUp27mQL3skxWCn9rEP684C+tHtEszad5B5y9p7Y+weIshq6lEoQ8sD+L/T7ncyMtai1AbeMK06PcutHA9UqhTlUCh9N8YZ/C/9rN6jmscZzv9hbqX8rDxnWIfvalFYXAH9qeET9Y+744mBBsEPujp06OYhjmErZtEkdCQdV3ZN9BV9rLhI2X2JIJXUs4ZCH20BBum4JH1r7RZldVDUVd4eWYBxJCP3y6d8cuv2MyA0aE9GoaxpX4RsDnoT4nSo2NgyL79pqLTQiNRdnGADVljKlymFbon/WaKA2ZsRqMvFpzaLi9wk1AaGW//YqbWUS8sJgYdufArfgazubkvaevq8trYlBUEoe2LT4ga5yqHwBe2yJNnAkyQWa7JzeMCNy5U0VOUPe/JZBtBbAt9zQs+L1mJ9kli1ctQS7xiiKKPMTGCrC7SqvR+PSyMqR7fYLfkf6P5sz/urUK4g/8XmFnPz3+zluZ6Cy+o00eebHUDLJ99EBww8IJ/sbvusvlQJQeR86Cp3dPZ7La77/jmRP63AAaeuecm3VUe6Vy71ATPTDpeZ8X983/K1Q5ARZLqA//aOXwLNtPFXOBHeSrL1oOYoy1wklKx8xarx2X1mfD+M4kpuGrSMBS8g/MFNaEsblH8rNbGE59PtinrMOzT2Q2DwWC1f1/cg2ZUtgXukvwP98jVNgx1IihshBOaz/IVpomtr4MXMuvIwOF7Gq0fZQ0LV9DabJAEwOmYVQdy8klZg93YmLzNXXcpOKSPRE5/IIPPAxOIKFPgH63OWeOq+66/TwwxJoSKoGSl4JmCz/Z/K6CNywDeecx/xIpkxYF1wLSMg7CrlvQcYtf7X/PNoxamX/8uj3FymqI3GGWGCGEkUIXv+dMPU8GyptN8EPNh3AfYjf0y80So4Q1GnqmCkuj1KtSVreFGw+uTqoMAhOXCj4NFwnY18q23+Fbkar24ZT0jV+MC6Yt6YhJmI0X4vDYKQlh9lmqwvz2n7MUpEptMKOiXMBw2ihhXKTjJ4pOLPCKV8Zy0YKlFYEF+qQ21pwoJmBKv/0lBVp54yklbr4AHOJi+/EQKwoq3zTyB8i6+nuR8CE4/Ki9X4LVVw8zHeqdYev9zM82P1o9mMSSpUsDHpcp2wvFQmx9KFkmjxUppzuHSqWjrQW0jPNKgYQPqNcNyBANL847BSh+13eXG6G2xctBP9IVcC7HGBGNVVZ1SDI3BWJCSYGRMc8UlYL7/WT6D4wCFLn1Yd4SCjGTjW0fyRV9md0pZmNUEOc+5NM17K+AC98ZPskW8IF4t7SvBi8hsc3CCPuGNaIDPmWg6mFvcv7wLmW2Xo1B4UmbiGCBt6uu8oA2oAOhGRBqALtALdDVddeUkGaRw==','2026-02-11 22:03:36','2026-02-11 22:03:38'),(9,13,'tviva20260223165422KlGR1','PAYMENT',1050000,1050000,0,'KRW','DONE',NULL,'KYMikudHgBIlNSRmwzu+oU+7e3m4WJ+MD24Ro2lawFtICN5jS/Wo6Ls6QvpiSjAvry0qnvmEI5RLTR6mbsyey5sgzdrvgcS4e/nyi+aX+oXWZSFDT1VeIPL/GfCJqyVEzPws+igIjfV3DA570Q5A3Jd15LXzOH9spIM6up63Je7vaRVL3OLFNDmeeafFUYQUpOqPYILvFRfW/RfAJQcSNmf4lNY222BuKopvaY40YdSHjobHcUjOvuaJOyBu9HECnX7MZWMSyteJZoq1BBrIuV0ZHXVZqT/gnE3PlXtVu7ASZVxXSWQ5GAUXO+ukx6ZCOPOZQ8wXTjj2OvWxQBXlE9oVKoQAxOgCyDNFCjaypTNawTe2Clea9vKZcyyG1FWcLG1Gwu9TtRu6PJyueejbNHAmm1VHv8N8d3e6SU9TRC2sG1Pu8LGLFapzyVKMsXQoT+tuueBj+sVl8kSJm4J/EpYABGaISNrOjkMTIlKGqwk+47ixLeULo2LtM4OIyjQLkLUFCpHs524/5s/vTUtJ5ClBp9t09sieSVT9XXaRBMmsWJN7aHeloAyTRR+r7zjqZE4DuZQ1iU3EIUyy1MAcVQEGBPEsf7UUnj7PpQ4pOrSB2UqWzgHVowcg32TsPr9wzal/xrXraw/HH8E6jfCptAyLpiM/xbNNp/+7zKLpGyGXlQ4altFXUqeMFOxr2NIoFRFlUeEd0WTltYMOEs4f4NtuSr7ha0CA9IXNRaRQ30rDbxC2h8HAyGMuQYMiYCQfW+96P14neUyQmaFoMKvcc+P+ubalb5WP29vikwozMgbDv80vC3c2pwo1X4gg2NIA0V12yfY8M2igKyPIKRxiADFm7yw4rgB1K3LWsEwIqSd/UdV0enGyMztYoQJmETjjo2soQRc3ACDQvGn3Qrt9kd8oNbhMVj2XQ+LkgwSxRL0Jk8vX+8mRVy5YqyhSNKwcBO0V1T2tqYdnBIWf6G/SxeGMo+UJYKCWp+MviBxnvdFGAZ/Z+WtY5aEGncm4p7hFkAu2vA3iUWNbDYMrFitMR/tGIKgwFRP7itMa3AmuIw7GlFv6iQcHWrYC27IzUFNp/eglwWlm5h96IIJqz393oU3RTQSgfeQRYuaPwr88MIjM9XoJIdaBGnXZOKw66gd7q1Plsu0AAxAzeoiIMFKeyle6YQl3EW/jL8161dGXc1cUuDn1XAz3kngddVHkmpo7q0NgD8WqkYKvC5kOAw+F0j7FDIPl94P6bigpjyTlibijRHnHAmqNkzQ/ZbOqUB97CyU4+VQN9SOMYz99SpGphBHqHUO5lbkfXiRQllIdubTPNoZniJVKk8Zu1gKbXxGvFqZdAXrEcq5hMNiIXkP5yGZBBFlgZTIXAQA9gepADd1THuToUcJiKA/sHgSAlVRyCkngZt3x3dqwjN5FTaLAyMho/sZf0119R0709JzDptQZfZ2AO46Y9gn7rAgbKlhCjOFot/7PwzssIfRq9HQ8x1XhqJCyGQRmuKzfcVVoTT7bpkLd5W3Kpdmb3t13ZPMdJX3pcv1Q3Wq2VNogG6L+Lz5luXdn+pt90Bvg2BW0FixASODl8snutXhD6Vz7ETPlJy3Q8z4Yg+N6od7QjV2r0QY9fiJtwGd94HOkylkUKUVAY0WfTyIQ6XFNgOCACNhQUrLQrooIf+yjWYgEbhtlf8UsQ7DBwXB2/LJ+2q0nJlMNJiw5HQeW8KiJ0TZQs6rNOWMvbT6ncZPmx8wdIzqcgGgMYj7OsONu0MG2uURHt1Nn6qglZG7Qn9bi69L0yn53lRMQcnHqXuw6x0pJWKdTaDGMv53N6l1jDQ/i9r71Kw4v5e2UMPmCwEdPpUKWTCCnu70P/jVtE6YcjgzregCkTePvtef+kKIsbpMCw2DPvzMGKHioDjiclcu6EF9ul3qEg+4OeM/OZto=','2026-02-22 22:55:16','2026-02-22 22:55:17'),(10,14,'tviva20260224163553VIhd3','PAYMENT',480000,480000,0,'KRW','DONE',NULL,'d2h9pRtMHYx4mFBEcw1jv8WhggZfUMvv2kQz9JbrTp0Qfmrhx0vBL6QRM7lXI5udS28v2z86lfZ5D9jB8hxpzfZjJmchM3kD+nQoZ1ZpLchSPFQedZAVj/RNd1Djp/loGwCZILMdWwKJKRJBnB9rCqDrrVVpHmD+qL9X88fSFOadbW1yAdISCbO15nqAS/ssmK5iPVoIq9aBj7+rHhAaCQ50LL2RUJBXpNMTZ8crtma4UszP3Z2e+orAc2WYHSEkJ257IxFfssUCuDX4WR+qEFiq2SxZl+2mREoLw00ENtP9VKXEotDWSZmE3Ir+b0QSVW4muWsjTPtfsJ3nC5EuNMDbGWK1sehQdSsWayRcEibpEbunCfXGdxMJeadukwIfzifpJKaiy0Lvw98wVysb+XuCRvVkUFSFdIJbWDLjOfgsYRNHVmdDQtGWVMBjfmuavrnGTemieAgQi7ktthDHk22DJ/W8PgX79MShpY0C8Q1ocW4sWyNpGuwAgo9oq62rKKzpZoz/cLgpAgWKzFjewoVc31a8prC5vwS+VUc/Gmz902pLCHRHMrOgv3LvToZ753kFXynDMC0ydqRTHIA0fio+Y4j2bacT7L2Ln+WQVplY1X1IYsnNuYyqJre1RgTU63gICqYnmG4lQiXRB2f9woAh+dVtug3OASq/WZNuPOKovQ1Pzu1bpmLW5Reamr6rVX+GIHeCzHh6jNczTEY4XkkZjRXgcFweQot1PABNP7N5QtQSeIdX+ObyX4e85LsG8kP/iZnY9wPadPDJ0z/HvzLZLaiGsaSEkGEdLcS6KyuFcxzCZbgHHUsskuDzsoEB9/FOZnqd645IRfmuI+AUYYm5VNX3diagUQ+a5NCoECgvp9Jj3IkIR5zKcxVGvkRZEjxda2i9+fmCvQBKd1rtprnt6kn1USbHAglRZ9MOsBz1frIYiNjuoUiHrZqPhcBrZpv6efs2/MB9t/YmJ9ayOf7ARikMnvUs9aKcx9u9z3vH10TsAKOP2G98JQ7u1Nes9VY4wvAjLcQCSVldKgVMC7lzdC9KllhAGp492rL3omyWIoSn7AU12ggo2ddeN5v1k4Id8iLAZXS0UEbedN8/8GfN0sE3W4kFbEnhN0e9GJH5n0xGQNZcyGOis9NMnKwabzyfJkhZcjt+Wal1luuXxmbBSUr1xk6D8kkCC+08pIbbLq2H+lUEFZMotq2tCy09bJq6KqdujAoCQKpy5G7ar4DF3U6vUi4951NbZTlBdU2yN+eJQPY61EOL4d69Wo7A4Beqg0vMvFg23M3xT0KEND/0E2T9dwW71BmOIdmek7LR34Q2yw4WfXi6ZB/vwnTagyp+SDSRs/nmROhd2FSDApxFVF3SiKiJ7EUsbBWxIX7meuwXuMbR4XtDx0USp2plcP5QCH2Bbomv7lhDN9ukmSFTwh/be8H7hXKpGZ/obi2B9aG8LqU3K3ik5e+3uioMACxanDGUdlvZwWVl3qPx4h32PwWcFnAG2D7TPCY28OCls7/h2kY5BuC+sroLOGk8LbTsYjw7fd6kkrF2eJKdt2bxSrZYnlLO38h8SMm7TF+Qy9sbjBEyxPm+0e5uXv4iMk2k7O549Q6WSwTHRwpGJE4mhC/xiW7T+zXr1EZc6QNd9x6eRN6bF4X/zuGEOzUayv7oe5u89WTu1hgq4V/hbFKENOWt1yNQM+jAfTEzY2FMKjmkZPPpqMBRKzT8pVmp1gdweS7h8Xrr+54RJllEjIir3k623TJod+MUNp7SW+aH3jziz7EsbwKWrQdUybmlQjDwYWBlQYjegsOh+JwCr2zRcnlcZfSDJPm8H/+g0W/AbCBc+Huoq2vRHzmpe25qXxvw/8YU3wt/h5TuVYViYa7PFqBvhtTIge/cRRTv3FUY1yCcvg9sZc1qbnJ2+YXFIPP0HJY=','2026-02-23 22:36:10','2026-02-23 22:36:12');
/*!40000 ALTER TABLE `payment_transactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payment_virtual_account_details`
--

DROP TABLE IF EXISTS `payment_virtual_account_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payment_virtual_account_details` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `payment_id` bigint unsigned NOT NULL COMMENT 'payments FK',
  `account_number` varchar(50) NOT NULL COMMENT '가상계좌 번호 (민감정보: 암호화 권장)',
  `bank_code` varchar(10) NOT NULL COMMENT '은행 코드',
  `customer_name` varchar(100) NOT NULL COMMENT '입금자명 (PII: 암호화 권장)',
  `due_date` datetime NOT NULL COMMENT '입금 기한',
  `refund_status` varchar(50) DEFAULT NULL COMMENT '환불 상태',
  `expired` tinyint(1) NOT NULL DEFAULT '0' COMMENT '만료 여부',
  `settlement_status` varchar(50) DEFAULT NULL COMMENT '정산 상태',
  `account_type` varchar(20) DEFAULT NULL COMMENT '계좌 유형 (일반/고정)',
  `refund_receive_account` text COMMENT '환불 받을 계좌 정보 (암호화 필수, Base64 인코딩된 암호문)',
  `secret` varchar(255) DEFAULT NULL COMMENT '가상계좌 시크릿 (민감정보: 암호화 필수)',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_va_details_payment` (`payment_id`),
  KEY `idx_va_details_account` (`account_number`),
  KEY `idx_va_details_due_date` (`due_date`),
  CONSTRAINT `fk_va_details_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='가상계좌 결제 상세 정보 (민감정보 암호화 필수)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payment_virtual_account_details`
--

LOCK TABLES `payment_virtual_account_details` WRITE;
/*!40000 ALTER TABLE `payment_virtual_account_details` DISABLE KEYS */;
/*!40000 ALTER TABLE `payment_virtual_account_details` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payments` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `transaction_id` bigint NOT NULL COMMENT '거래 FK',
  `pg_provider` varchar(50) DEFAULT NULL COMMENT 'PG사 (예: toss, kakao)',
  `payment_key` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_as_cs DEFAULT NULL,
  `order_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_as_cs DEFAULT NULL,
  `amount` bigint unsigned NOT NULL COMMENT '결제 금액 (KRW, 원 단위)',
  `method_id` bigint NOT NULL DEFAULT '1' COMMENT '결제 수단 FK',
  `paid_at` datetime DEFAULT NULL COMMENT '결제 완료 시각',
  `status_id` bigint NOT NULL DEFAULT '1' COMMENT '결제 상태 FK',
  `use_escrow` tinyint(1) NOT NULL DEFAULT '0' COMMENT '에스크로 사용 여부',
  `is_partial_cancelable` tinyint(1) NOT NULL DEFAULT '0' COMMENT '부분 취소 가능 여부',
  `payment_type` varchar(20) DEFAULT NULL COMMENT '결제 타입 (NORMAL, BILLING)',
  `last_transaction_key` varchar(255) DEFAULT NULL COMMENT '최종 거래 키 (deprecated: use payment_transactions)',
  `merchant_id` varchar(50) DEFAULT NULL COMMENT '토스 가맹점 ID (mId)',
  `api_version` varchar(20) DEFAULT NULL COMMENT '토스 API 버전',
  `country` char(2) DEFAULT 'KR' COMMENT '국가 코드 (ISO-3166-1 alpha-2)',
  `culture_expense` tinyint(1) DEFAULT '0' COMMENT '문화비 소득공제 여부',
  `metadata` json DEFAULT NULL COMMENT '커스텀 메타데이터',
  `discount_info` json DEFAULT NULL COMMENT '할인 정보',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_payments_payment_key` (`payment_key`),
  UNIQUE KEY `uk_payments_order_id` (`order_id`),
  KEY `idx_payments_trans` (`transaction_id`),
  KEY `idx_payments_key` (`payment_key`),
  KEY `idx_payments_order` (`order_id`),
  KEY `idx_payments_method_id` (`method_id`),
  KEY `idx_payments_status_id` (`status_id`),
  KEY `idx_payments_trans_status` (`transaction_id`,`status_id`),
  KEY `idx_payments_transaction_id` (`transaction_id`),
  KEY `idx_payments_type` (`payment_type`),
  KEY `idx_payments_merchant` (`merchant_id`),
  KEY `idx_payments_paid_at` (`paid_at`),
  KEY `idx_payments_status` (`status_id`),
  CONSTRAINT `fk_payments_method` FOREIGN KEY (`method_id`) REFERENCES `payment_methods` (`id`),
  CONSTRAINT `fk_payments_status` FOREIGN KEY (`status_id`) REFERENCES `payment_statuses` (`id`),
  CONSTRAINT `fk_payments_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='결제 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `payments`
--

LOCK TABLES `payments` WRITE;
/*!40000 ALTER TABLE `payments` DISABLE KEYS */;
INSERT INTO `payments` VALUES (6,1,'toss','tgen_20260130145023qODz3','TXN_1_5d27bfe4e38547e890c157b1f382b19a',360000,3,'2026-01-30 05:50:35',2,0,1,'NORMAL','txrd_a01kg6q837bg90nw0fn00rdqcx2','tgen_docs','2022-11-16','KR',0,NULL,NULL),(7,2,'toss','tgen_20260130151330rV6d3','TXN_2_ad10fa5aae7f499e975651c0fc58ffe6',5000,3,'2026-01-30 06:13:59',2,0,1,'NORMAL','txrd_a01kg6rjydae3s1d7843he0naez','tgen_docs','2022-11-16','KR',0,NULL,NULL),(8,3,'toss','tgen_20260130154459tIdu2','TXN_3_9b78e9cd4d8e466d90601e1fbf9770ae',180000,3,'2026-01-30 06:45:27',2,0,1,'NORMAL','txrd_a01kg6tcjg7tttxzs3j8958smch','tgen_docs','2022-11-16','KR',0,NULL,NULL),(9,4,'toss','tgen_20260202131310sZzZ5','TXN_4_2c5afc95da014bfca9255447f96e1228',300000,3,'2026-02-02 04:13:27',2,0,1,'NORMAL','txrd_a01kge8wcshx64gfj9h9gc7qpdm','tgen_docs','2022-11-16','KR',0,NULL,NULL),(10,5,'toss','tgen_20260202132432tfoi6','TXN_5_9ee25ff691e048cda59b004bc46e503f',180000,3,'2026-02-02 04:24:41',2,0,1,'NORMAL','txrd_a01kge9gzrcxrard3060jy0qne7','tgen_docs','2022-11-16','KR',0,NULL,NULL),(12,17,'toss','tviva20260212160318oKWT5','TXN_17_0e9524809dd641b2baf18c60026536a1',450000,1,'2026-02-12 07:03:36',2,0,1,'NORMAL','txrd_a01kh8ak5dhs0a35k497kf953yv','tvivarepublica','2024-06-01','KR',0,NULL,NULL),(13,19,'toss','tviva20260223165422KlGR1','TXN_19_6c72be4d3d754ff8a4dafae714da0030',1050000,5,'2026-02-23 07:55:16',2,0,1,'NORMAL','txrd_a01kj4qxnhc0afepr57t8nrd67t','tvivarepublica','2024-06-01','KR',0,NULL,NULL),(14,21,'toss','tviva20260224163553VIhd3','TXN_21_f7c62123aad04690b6d631eb238282a7',480000,5,'2026-02-24 07:36:10',2,0,1,'NORMAL','txrd_a01kj797d7ezarcy3px2mpktt61','tvivarepublica','2024-06-01','KR',0,NULL,NULL);
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
) ENGINE=InnoDB AUTO_INCREMENT=722 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Refresh Token 저장 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `refresh_tokens`
--

LOCK TABLES `refresh_tokens` WRITE;
/*!40000 ALTER TABLE `refresh_tokens` DISABLE KEYS */;
INSERT INTO `refresh_tokens` VALUES (1,12,'0c1d7846-5ec0-4201-b8ac-d6e99e9dcd4b','2026-01-19 00:54:21','2026-01-11 15:54:21',0,NULL,NULL),(2,12,'5a270563-1633-4a9f-a0cf-b40eb9efc8dc','2026-01-19 03:44:25','2026-01-11 18:44:25',0,NULL,NULL),(3,12,'45585bc9-570d-493f-9b70-6a9c85dcb8ce','2026-01-19 03:44:39','2026-01-11 18:44:39',0,NULL,NULL),(4,12,'9b554031-1d72-4763-b661-f7fb000fc59f','2026-01-19 03:46:06','2026-01-11 18:46:06',0,NULL,NULL),(5,12,'91af2324-73d8-438f-a91e-d99de41f90bd','2026-01-19 03:47:54','2026-01-11 18:47:54',0,NULL,NULL),(6,12,'0f0ecf18-75c0-4c77-963c-f782650638ca','2026-01-19 04:34:27','2026-01-11 19:34:27',0,NULL,NULL),(7,12,'f8a38c28-3aeb-47f4-a1e7-bb55f42b49aa','2026-01-19 04:36:25','2026-01-11 19:36:25',0,NULL,NULL),(8,12,'2d8f10dc-882b-43ee-9f8c-50aa975e28e8','2026-01-19 04:36:35','2026-01-11 19:36:35',0,NULL,NULL),(9,12,'61b1b3d3-adcc-4677-827c-94478a59a20a','2026-01-19 04:36:49','2026-01-11 19:36:49',1,'2026-01-12 04:37:51','9a1075aa-938d-4970-ba0c-b21c109a49b3'),(10,12,'9a1075aa-938d-4970-ba0c-b21c109a49b3','2026-01-19 04:37:51','2026-01-11 19:37:51',0,NULL,NULL),(11,12,'cc83b930-3491-4063-89db-c3ac369e1565','2026-01-19 04:42:01','2026-01-11 19:42:01',1,'2026-01-12 04:42:17','1c61d427-f253-4139-9399-a8e4c3497126'),(12,12,'1c61d427-f253-4139-9399-a8e4c3497126','2026-01-19 04:42:18','2026-01-11 19:42:18',0,NULL,NULL),(13,12,'ba01bdf3-915b-4e21-b1ff-bde069a69d5d','2026-01-19 05:39:17','2026-01-11 20:39:17',0,NULL,NULL),(14,12,'3671a52d-ae6e-4ac5-a534-65ce6ecd46f1','2026-01-19 05:52:43','2026-01-11 20:52:43',0,NULL,NULL),(15,12,'de7681bb-002c-4508-9892-14a4e16ba3de','2026-01-19 05:53:35','2026-01-11 20:53:35',1,'2026-01-12 06:10:19','c9ba8b1d-a939-4b9e-9d70-e56063d7b017'),(16,12,'c9ba8b1d-a939-4b9e-9d70-e56063d7b017','2026-01-19 06:10:19','2026-01-11 21:10:19',1,'2026-01-12 07:07:35','35ffa890-fa40-4d86-93e7-b60432da7f28'),(17,12,'35ffa890-fa40-4d86-93e7-b60432da7f28','2026-01-19 07:07:35','2026-01-11 22:07:35',1,'2026-01-12 07:22:54','537f1d00-a2ac-446e-ac56-6fa5b25e27fd'),(18,12,'dc2009f4-9acc-4f30-8516-545c8c7b58e6','2026-01-19 07:08:54','2026-01-11 22:08:54',0,NULL,NULL),(19,12,'537f1d00-a2ac-446e-ac56-6fa5b25e27fd','2026-01-19 07:22:54','2026-01-11 22:22:54',1,'2026-01-12 07:44:35','5ccc72f7-afc8-4067-9353-448a9b6dc2de'),(20,12,'8eb170cf-f30e-4f3f-a7fb-c19863d0201e','2026-01-19 07:35:24','2026-01-11 22:35:24',0,NULL,NULL),(21,12,'6dec184f-0652-4a3f-801d-566eb45aae46','2026-01-19 07:41:46','2026-01-11 22:41:46',0,NULL,NULL),(22,12,'5ccc72f7-afc8-4067-9353-448a9b6dc2de','2026-01-19 07:44:35','2026-01-11 22:44:35',1,'2026-01-12 08:02:58','4379a2bc-22f6-42d0-b6eb-40e778add5ca'),(23,12,'4379a2bc-22f6-42d0-b6eb-40e778add5ca','2026-01-19 08:02:58','2026-01-11 23:02:58',1,'2026-01-12 23:40:31','00ea9025-6853-4f90-97a9-ed53d5a03faa'),(24,12,'00ea9025-6853-4f90-97a9-ed53d5a03faa','2026-01-19 23:40:31','2026-01-12 14:40:31',0,NULL,NULL),(25,12,'8c37f718-1b97-4a8e-8e7b-b91b6ec7fb0c','2026-01-19 23:40:31','2026-01-12 14:40:31',0,NULL,NULL),(26,12,'8a3295b7-0cee-42ae-bec5-0bd4a5bb250f','2026-01-19 23:48:55','2026-01-12 14:48:55',1,'2026-01-13 00:41:00','3b52e462-1239-4784-8d2b-8886b81c6a78'),(27,12,'3b52e462-1239-4784-8d2b-8886b81c6a78','2026-01-20 00:41:00','2026-01-12 15:41:00',1,'2026-01-13 00:41:00','f399da0c-751f-4b35-bb65-f16c3a7d6f19'),(28,12,'f399da0c-751f-4b35-bb65-f16c3a7d6f19','2026-01-20 00:41:00','2026-01-12 15:41:00',1,'2026-01-13 01:09:56','897c2d7b-3de0-4ef2-87c4-9e8919846be8'),(29,12,'3eb59bd8-3a21-4926-babd-03fbaecd1ab4','2026-01-20 01:09:56','2026-01-12 16:09:56',0,NULL,NULL),(30,12,'897c2d7b-3de0-4ef2-87c4-9e8919846be8','2026-01-20 01:09:56','2026-01-12 16:09:56',1,'2026-01-13 01:34:18','5004683e-71e3-4b9d-bc56-0c4a44346282'),(31,12,'a6ee1f7e-503b-4f86-9bae-c68327f0b9e2','2026-01-20 01:34:18','2026-01-12 16:34:18',0,NULL,NULL),(32,12,'5004683e-71e3-4b9d-bc56-0c4a44346282','2026-01-20 01:34:18','2026-01-12 16:34:18',1,'2026-01-13 01:56:25','5af12285-9e47-4096-803d-5e6ee057d3d5'),(33,12,'7fb42627-4306-452e-bad3-47625b423b93','2026-01-20 01:56:25','2026-01-12 16:56:25',1,'2026-01-13 05:40:22','9092aed4-bcc2-4f44-91cf-2ad042258922'),(34,12,'5af12285-9e47-4096-803d-5e6ee057d3d5','2026-01-20 01:56:25','2026-01-12 16:56:25',0,NULL,NULL),(35,12,'9092aed4-bcc2-4f44-91cf-2ad042258922','2026-01-20 05:40:22','2026-01-12 20:40:22',1,'2026-01-13 06:37:32','8e5df1d8-b7ea-4565-a2de-a3e63557ea57'),(36,12,'1278f75b-b21b-4688-929d-928960d4037b','2026-01-20 05:40:22','2026-01-12 20:40:22',0,NULL,NULL),(37,12,'8e5df1d8-b7ea-4565-a2de-a3e63557ea57','2026-01-20 06:37:32','2026-01-12 21:37:32',0,NULL,NULL),(38,12,'bd6b46ed-f211-4dbb-b6cf-e4e58177148f','2026-01-20 06:37:32','2026-01-12 21:37:32',1,'2026-01-13 07:01:20','28cfa027-5107-4302-b76a-7e3ccb3433dd'),(39,12,'28cfa027-5107-4302-b76a-7e3ccb3433dd','2026-01-20 07:01:20','2026-01-12 22:01:20',0,NULL,NULL),(40,12,'1d6439b9-fb25-44d7-95a9-0757f8d16e2f','2026-01-20 07:01:20','2026-01-12 22:01:20',1,'2026-01-13 10:08:32','4a7fd49c-7e47-4be2-8d22-b71087ebdc90'),(41,12,'4a7fd49c-7e47-4be2-8d22-b71087ebdc90','2026-01-20 10:08:32','2026-01-13 01:08:32',0,NULL,NULL),(42,12,'d22435f5-00ac-4d59-87a8-db93313f03a2','2026-01-20 10:08:32','2026-01-13 01:08:32',1,'2026-01-14 03:50:28','a0440eb5-3976-4fd6-b904-eb9620dba0a9'),(43,12,'036c9877-e371-4700-81ca-571b2a421d20','2026-01-21 03:50:28','2026-01-13 18:50:28',1,'2026-01-14 04:14:35','da58f4aa-c32e-45d9-a70c-ab863b48226e'),(44,12,'a0440eb5-3976-4fd6-b904-eb9620dba0a9','2026-01-21 03:50:28','2026-01-13 18:50:28',0,NULL,NULL),(45,12,'7aeaaa68-b279-49f7-8e10-816c778bffed','2026-01-21 03:55:09','2026-01-13 18:55:09',0,NULL,NULL),(46,12,'ccbda0e3-9c81-408e-a853-408967f99fd7','2026-01-21 04:14:35','2026-01-13 19:14:35',1,'2026-01-14 04:43:32','7f612ee5-8df0-4792-b494-49bdafa2d70c'),(47,12,'da58f4aa-c32e-45d9-a70c-ab863b48226e','2026-01-21 04:14:35','2026-01-13 19:14:35',0,NULL,NULL),(48,12,'7d71c5d6-6f23-446e-b010-323a7d419840','2026-01-21 04:30:48','2026-01-13 19:30:48',0,NULL,NULL),(49,12,'7f612ee5-8df0-4792-b494-49bdafa2d70c','2026-01-21 04:43:32','2026-01-13 19:43:32',0,NULL,NULL),(50,12,'7e03b541-29d4-403b-8c00-baaa93104a71','2026-01-21 04:43:32','2026-01-13 19:43:32',1,'2026-01-14 05:14:08','e1319baf-40b2-46de-8b09-65dd141386d0'),(51,12,'e1319baf-40b2-46de-8b09-65dd141386d0','2026-01-21 05:14:08','2026-01-13 20:14:08',1,'2026-01-14 05:35:49','00860384-54d9-483c-b56c-6ae50b78b4fa'),(52,12,'3c077954-a109-461c-8309-3426f7d9779f','2026-01-21 05:14:08','2026-01-13 20:14:08',0,NULL,NULL),(53,12,'119fe39a-db8f-4358-9baf-5adec5f6d0cd','2026-01-21 05:29:10','2026-01-13 20:29:10',0,NULL,NULL),(54,12,'00860384-54d9-483c-b56c-6ae50b78b4fa','2026-01-21 05:35:49','2026-01-13 20:35:49',0,NULL,NULL),(55,12,'d460fb31-14b0-47e2-939d-5e8f6b9637a2','2026-01-21 05:35:49','2026-01-13 20:35:49',1,'2026-01-14 06:31:08','919f8ac6-01cb-4e55-91a2-c9c35cc8c3d7'),(56,12,'5434485c-f4fb-44a1-ae4a-309324cfb80a','2026-01-21 06:31:08','2026-01-13 21:31:08',0,NULL,NULL),(57,12,'919f8ac6-01cb-4e55-91a2-c9c35cc8c3d7','2026-01-21 06:31:08','2026-01-13 21:31:08',1,'2026-01-15 00:20:16','cd27ea3f-647b-4636-861a-58d1e911c330'),(58,12,'8f3ac0a2-8cea-40ae-9280-9eb71147cb59','2026-01-22 00:20:16','2026-01-14 15:20:16',0,NULL,NULL),(59,12,'cd27ea3f-647b-4636-861a-58d1e911c330','2026-01-22 00:20:16','2026-01-14 15:20:16',1,'2026-01-15 00:36:38','64d5cecc-db76-4ab5-bfe4-b2c8f697d4ed'),(60,12,'330baf4e-af9b-421e-92aa-913c2673e77d','2026-01-22 00:36:38','2026-01-14 15:36:38',0,NULL,NULL),(61,12,'64d5cecc-db76-4ab5-bfe4-b2c8f697d4ed','2026-01-22 00:36:38','2026-01-14 15:36:38',1,'2026-01-15 00:55:13','be77ba88-8425-484a-88f3-ba0910d97cc4'),(62,12,'93ecde18-431a-4698-aa62-1fa5681a268a','2026-01-22 00:55:14','2026-01-14 15:55:14',0,NULL,NULL),(63,12,'be77ba88-8425-484a-88f3-ba0910d97cc4','2026-01-22 00:55:14','2026-01-14 15:55:14',1,'2026-01-15 01:12:08','7a07826d-22dc-40de-b13e-764a6c38894f'),(64,12,'7a07826d-22dc-40de-b13e-764a6c38894f','2026-01-22 01:12:08','2026-01-14 16:12:08',0,NULL,NULL),(65,12,'da56d313-7564-45df-8e5b-70da747e867a','2026-01-22 01:12:08','2026-01-14 16:12:08',1,'2026-01-15 01:53:15','d0679a76-5606-4e12-bfd1-05b1280d7530'),(66,12,'241a75bc-5df4-49b6-bbf4-c3bd2b3f8a31','2026-01-22 01:53:15','2026-01-14 16:53:15',0,NULL,NULL),(67,12,'d0679a76-5606-4e12-bfd1-05b1280d7530','2026-01-22 01:53:15','2026-01-14 16:53:15',1,'2026-01-15 02:22:52','4837bb67-f3d4-433a-b823-85da463f544f'),(68,12,'eecb64f2-f0b6-4862-bf67-f30c15e5e4a0','2026-01-22 02:22:52','2026-01-14 17:22:52',0,NULL,NULL),(69,12,'4837bb67-f3d4-433a-b823-85da463f544f','2026-01-22 02:22:52','2026-01-14 17:22:52',1,'2026-01-15 03:18:04','8f77cfce-ed64-426e-b03e-bb3bd360d35d'),(70,12,'79cb8e0a-6779-4a53-974f-3d066792462e','2026-01-22 03:18:04','2026-01-14 18:18:04',0,NULL,NULL),(71,12,'8f77cfce-ed64-426e-b03e-bb3bd360d35d','2026-01-22 03:18:04','2026-01-14 18:18:04',0,NULL,NULL),(72,12,'a8a7b210-cde4-412b-81e5-c29c87dfa9fe','2026-01-22 03:40:27','2026-01-14 18:40:27',1,'2026-01-15 04:07:44','2e30fed6-7509-4c98-8eb0-c14201809472'),(73,12,'afaa60db-5668-4258-819a-37c553fdff21','2026-01-22 04:07:44','2026-01-14 19:07:44',0,NULL,NULL),(74,12,'2e30fed6-7509-4c98-8eb0-c14201809472','2026-01-22 04:07:44','2026-01-14 19:07:44',0,NULL,NULL),(75,12,'bb221f56-fa42-4c72-ba48-ae6caf097f56','2026-01-22 04:17:43','2026-01-14 19:17:43',1,'2026-01-15 05:07:36','23b747b1-d261-474b-a772-408219516de7'),(76,12,'c91b6368-8e2a-46cb-9206-f48c16bdbcc2','2026-01-22 05:07:36','2026-01-14 20:07:36',0,NULL,NULL),(77,12,'bc42eb5f-829b-4fa9-9b01-ade807f14bc9','2026-01-22 05:07:36','2026-01-14 20:07:36',0,NULL,NULL),(78,12,'23b747b1-d261-474b-a772-408219516de7','2026-01-22 05:07:36','2026-01-14 20:07:36',0,NULL,NULL),(79,15,'ce400efd-5243-4ec7-86cc-801b17fde341','2026-01-22 05:10:04','2026-01-14 20:10:04',0,NULL,NULL),(80,12,'c8c43997-479a-4c22-a61d-2982dcec6b65','2026-01-22 05:10:52','2026-01-14 20:10:52',0,NULL,NULL),(81,12,'2013c6fe-1307-4e47-b74b-5a5764f5b84a','2026-01-22 05:11:48','2026-01-14 20:11:48',0,NULL,NULL),(82,12,'5a7f2d76-1cdb-4abd-98b7-f3d7b698ee75','2026-01-22 05:24:12','2026-01-14 20:24:12',0,NULL,NULL),(83,15,'8e562d97-09b0-47f6-bcf3-590a4912ddb6','2026-01-22 05:25:04','2026-01-14 20:25:04',0,NULL,NULL),(84,12,'28ba62d0-268d-4b18-bae2-65bb5d5c1be9','2026-01-22 05:28:24','2026-01-14 20:28:24',0,NULL,NULL),(85,15,'b6e72ec0-fa37-4637-91a1-e1ff6e72c81a','2026-01-22 05:31:36','2026-01-14 20:31:36',0,NULL,NULL),(86,12,'fdf32d8e-8d10-47f0-9bbd-3626bf68c745','2026-01-22 05:31:53','2026-01-14 20:31:53',0,NULL,NULL),(87,12,'ebb0b913-7171-4a8c-973c-0a0e714bdd44','2026-01-22 05:32:14','2026-01-14 20:32:14',0,NULL,NULL),(88,12,'13871a7f-467d-4a69-9a85-51c2610b4cbc','2026-01-22 05:33:01','2026-01-14 20:33:01',0,NULL,NULL),(89,12,'9015c088-2f37-4001-adfe-d65c5313c8f9','2026-01-22 05:34:45','2026-01-14 20:34:45',0,NULL,NULL),(90,15,'9f7b5f65-293b-4204-a2f4-e91e7aa1d658','2026-01-22 05:34:59','2026-01-14 20:34:59',0,NULL,NULL),(91,12,'cea1c403-8065-4c83-b12b-11071f00482c','2026-01-22 05:39:34','2026-01-14 20:39:34',0,NULL,NULL),(92,15,'910c8a90-f81b-4c5b-b48b-51b71ca09acb','2026-01-22 05:39:46','2026-01-14 20:39:46',1,'2026-01-15 06:14:07','86f25868-5943-4b8e-b25e-faf534bdca7c'),(93,15,'d6b2905d-5156-479b-aa7b-e9bef8f28552','2026-01-22 06:14:07','2026-01-14 21:14:07',1,'2026-01-15 06:52:25','1a3a9b62-62d1-45e6-8a05-555c3596f4f1'),(94,15,'86f25868-5943-4b8e-b25e-faf534bdca7c','2026-01-22 06:14:07','2026-01-14 21:14:07',0,NULL,NULL),(95,15,'f07d4939-469d-4e88-8e16-853b87d45d18','2026-01-22 06:14:07','2026-01-14 21:14:07',0,NULL,NULL),(96,15,'90caac37-405c-4d53-b75e-6e2cfd5752f8','2026-01-22 06:52:25','2026-01-14 21:52:25',0,NULL,NULL),(97,15,'1a3a9b62-62d1-45e6-8a05-555c3596f4f1','2026-01-22 06:52:25','2026-01-14 21:52:25',0,NULL,NULL),(98,12,'6a42953c-bfda-4049-8be5-0af7dd633108','2026-01-22 07:19:07','2026-01-14 22:19:07',1,'2026-01-15 07:46:13','a0f32646-25d7-4ccf-b444-63290225410d'),(99,12,'1d9d5974-4043-4e41-bd76-fb68386cd7a8','2026-01-22 07:46:13','2026-01-14 22:46:13',1,'2026-01-15 08:07:16','179001e5-1284-4054-8085-0efb4c46c930'),(100,12,'4c3f09b9-c58c-4009-a6be-2b411ea14f61','2026-01-22 07:46:13','2026-01-14 22:46:13',0,NULL,NULL),(101,12,'a0f32646-25d7-4ccf-b444-63290225410d','2026-01-22 07:46:13','2026-01-14 22:46:13',0,NULL,NULL),(102,12,'179001e5-1284-4054-8085-0efb4c46c930','2026-01-22 08:07:16','2026-01-14 23:07:16',0,NULL,NULL),(103,12,'a5d64daf-dbc1-4edc-98c6-b2957fcae3cc','2026-01-22 08:07:40','2026-01-14 23:07:40',1,'2026-01-15 08:28:34','f34df0e9-5879-47fa-9fac-a7d83c4326bd'),(104,12,'0e662be2-3311-4d4d-b6b2-45ddced16f7e','2026-01-22 08:28:35','2026-01-14 23:28:35',0,NULL,NULL),(105,12,'11a08b14-f8f5-4b58-8476-cdad5161dd20','2026-01-22 08:28:35','2026-01-14 23:28:35',1,'2026-01-15 08:47:42','4c0a7ad9-323f-4de4-8195-11872f620486'),(106,12,'f34df0e9-5879-47fa-9fac-a7d83c4326bd','2026-01-22 08:28:35','2026-01-14 23:28:35',0,NULL,NULL),(107,12,'0daa8ca7-f03b-4779-b359-2a8b08f80208','2026-01-22 08:47:42','2026-01-14 23:47:42',0,NULL,NULL),(108,12,'7ea4c31f-252d-49bc-a3fc-ce3129840feb','2026-01-22 08:47:42','2026-01-14 23:47:42',1,'2026-01-15 09:57:01','b70f9360-a851-4863-8a79-805dcbb87758'),(109,12,'4c0a7ad9-323f-4de4-8195-11872f620486','2026-01-22 08:47:42','2026-01-14 23:47:42',0,NULL,NULL),(110,12,'b70f9360-a851-4863-8a79-805dcbb87758','2026-01-22 09:57:01','2026-01-15 00:57:01',0,NULL,NULL),(111,12,'c9173c7d-cc0e-47a7-b720-290488d7012c','2026-01-22 09:57:25','2026-01-15 00:57:25',1,'2026-01-15 10:13:30','259faba8-95d8-484d-93c3-cc7396ef7579'),(112,12,'9f6ef607-ac53-4cce-a8c6-bf2d6743bb66','2026-01-22 10:13:30','2026-01-15 01:13:30',1,'2026-01-16 00:10:05','d6cc6cec-5139-4a70-8d48-033ff24b8e0b'),(113,12,'259faba8-95d8-484d-93c3-cc7396ef7579','2026-01-22 10:13:30','2026-01-15 01:13:30',0,NULL,NULL),(114,12,'d68a4f71-6b9a-4e8c-98b4-650c5ada1a10','2026-01-22 10:13:30','2026-01-15 01:13:30',0,NULL,NULL),(115,12,'be010b3c-e665-4cb0-90bf-733312df463f','2026-01-23 00:10:05','2026-01-15 15:10:05',1,'2026-01-16 00:33:06','ed1eb18c-405e-42e4-bc7b-e8f871d3c3be'),(116,12,'d6cc6cec-5139-4a70-8d48-033ff24b8e0b','2026-01-23 00:10:05','2026-01-15 15:10:05',0,NULL,NULL),(117,12,'28632555-7a93-4c53-a5b5-e90b8ca327fa','2026-01-23 00:10:05','2026-01-15 15:10:05',0,NULL,NULL),(118,12,'8dd21c58-9efa-4c81-b780-937ae2698209','2026-01-23 00:33:06','2026-01-15 15:33:06',0,NULL,NULL),(119,12,'ed1eb18c-405e-42e4-bc7b-e8f871d3c3be','2026-01-23 00:33:06','2026-01-15 15:33:06',0,NULL,NULL),(120,12,'0936006c-ecc7-4534-9086-7df92b4fb0ad','2026-01-23 00:33:06','2026-01-15 15:33:06',1,'2026-01-16 01:06:24','11acb35a-7f81-4bb1-b302-48382d5d1bb1'),(121,12,'11acb35a-7f81-4bb1-b302-48382d5d1bb1','2026-01-23 01:06:24','2026-01-15 16:06:24',1,'2026-01-16 01:27:46','43814c6d-5f3f-44f3-a3ad-7a333ec10199'),(122,12,'43814c6d-5f3f-44f3-a3ad-7a333ec10199','2026-01-23 01:27:46','2026-01-15 16:27:46',1,'2026-01-16 01:55:35','f7040a2e-082f-4e3c-89c7-2d1ed644f7fb'),(123,12,'f7040a2e-082f-4e3c-89c7-2d1ed644f7fb','2026-01-23 01:55:35','2026-01-15 16:55:35',0,NULL,NULL),(124,12,'79f50d04-cff7-496d-b11e-22825abc9e99','2026-01-23 03:23:31','2026-01-15 18:23:31',1,'2026-01-16 03:43:47','9a4710b2-deb2-4106-86f5-525a294dc829'),(125,12,'9a4710b2-deb2-4106-86f5-525a294dc829','2026-01-23 03:43:47','2026-01-15 18:43:47',1,'2026-01-16 04:01:35','5a568481-3fe3-4021-891f-bde0629d3603'),(126,12,'5a568481-3fe3-4021-891f-bde0629d3603','2026-01-23 04:01:35','2026-01-15 19:01:35',1,'2026-01-16 04:22:48','9c869f09-0d9e-4113-9cc1-d4f5f0b0b838'),(127,12,'9c869f09-0d9e-4113-9cc1-d4f5f0b0b838','2026-01-23 04:22:48','2026-01-15 19:22:48',1,'2026-01-16 05:11:49','bd291ac0-0c63-4e41-a6b9-bdecebab542f'),(128,12,'9d92eb52-be86-49f1-a54c-746c3d7d64bd','2026-01-23 05:11:49','2026-01-15 20:11:49',0,NULL,NULL),(129,12,'82df70dc-7404-4c72-b507-b2f583adf7e9','2026-01-23 05:11:49','2026-01-15 20:11:49',0,NULL,NULL),(130,12,'bd291ac0-0c63-4e41-a6b9-bdecebab542f','2026-01-23 05:11:49','2026-01-15 20:11:49',0,NULL,NULL),(131,12,'3b43dd7a-c565-4c05-8747-1eef4d210a45','2026-01-23 05:12:16','2026-01-15 20:12:16',1,'2026-01-16 06:21:41','20ef8838-c2b3-4147-bcb6-47ca4e6ef654'),(132,12,'ef54a6a8-92ba-4639-90dc-3212dfc55183','2026-01-23 06:21:41','2026-01-15 21:21:41',0,NULL,NULL),(133,12,'20ef8838-c2b3-4147-bcb6-47ca4e6ef654','2026-01-23 06:21:41','2026-01-15 21:21:41',0,NULL,NULL),(134,12,'53e8d16c-bd74-45f4-8044-f078d8353828','2026-01-23 06:31:56','2026-01-15 21:31:56',0,NULL,NULL),(135,12,'2f2d6b20-f2ec-4721-ad63-974578603b24','2026-01-23 07:31:02','2026-01-15 22:31:02',1,'2026-01-16 07:49:28','72969867-d9e0-4f10-b7b3-e7a6d0d3a03e'),(136,12,'72969867-d9e0-4f10-b7b3-e7a6d0d3a03e','2026-01-23 07:49:28','2026-01-15 22:49:28',0,NULL,NULL),(137,12,'24627e39-014d-4bc8-81c1-cbb87c131cba','2026-01-23 07:56:31','2026-01-15 22:56:31',0,NULL,NULL),(138,12,'e3d01532-616a-4c60-a77f-8421f8e08e60','2026-01-24 07:19:54','2026-01-16 22:19:54',0,NULL,NULL),(139,12,'9585cb18-607d-4b94-85c3-dfe23f19f911','2026-01-24 07:33:46','2026-01-16 22:33:46',1,'2026-01-17 07:52:26','4f1aeb05-e3b6-4d11-b9d9-b6146cb7e70d'),(140,12,'4f1aeb05-e3b6-4d11-b9d9-b6146cb7e70d','2026-01-24 07:52:26','2026-01-16 22:52:26',0,NULL,NULL),(141,12,'2582b4af-0c19-4e84-8c3c-92ace9b243a4','2026-01-24 07:52:26','2026-01-16 22:52:26',1,'2026-01-17 08:54:34','8ad40975-307e-4d43-9735-cf539ac854e6'),(142,12,'c2d38042-9aa2-4ca3-b86c-14516bfbbacb','2026-01-24 07:52:26','2026-01-16 22:52:26',0,NULL,NULL),(143,12,'d006b73f-eda9-4c77-a963-7458bffe5052','2026-01-24 08:54:34','2026-01-16 23:54:34',0,NULL,NULL),(144,12,'17652c6b-2fa0-4145-8384-30cdc57fc299','2026-01-24 08:54:34','2026-01-16 23:54:34',0,NULL,NULL),(145,12,'8ad40975-307e-4d43-9735-cf539ac854e6','2026-01-24 08:54:34','2026-01-16 23:54:34',0,NULL,NULL),(146,12,'7764c59a-8b86-4a32-a228-bda824b66088','2026-01-24 08:59:37','2026-01-16 23:59:37',1,'2026-01-17 09:29:00','440f3083-b719-436e-a359-66d1e98335b5'),(147,12,'440f3083-b719-436e-a359-66d1e98335b5','2026-01-24 09:29:00','2026-01-17 00:29:00',0,NULL,NULL),(148,12,'b9570d70-99fc-4672-860c-d16aa8f23dff','2026-01-24 09:29:00','2026-01-17 00:29:00',0,NULL,NULL),(149,12,'0c088bff-a74e-4999-90c1-77cd3eb0313e','2026-01-24 09:29:00','2026-01-17 00:29:00',0,NULL,NULL),(150,12,'e9148a2a-3aa9-41e5-a05b-7dec531f31ca','2026-01-24 09:39:38','2026-01-17 00:39:38',0,NULL,NULL),(151,12,'50be20ef-66a2-49f5-8403-1841fb0eb2ec','2026-01-26 01:13:34','2026-01-18 16:13:34',1,'2026-01-19 01:36:45','428f44c4-413b-4b35-9372-9779344d87a1'),(152,12,'8130437d-68b7-4e5a-8f6c-8dc6d9b4e6d8','2026-01-26 01:36:46','2026-01-18 16:36:46',0,NULL,NULL),(153,12,'cdaa322d-443d-412b-a558-0a4792fd7273','2026-01-26 01:36:46','2026-01-18 16:36:46',0,NULL,NULL),(154,12,'428f44c4-413b-4b35-9372-9779344d87a1','2026-01-26 01:36:46','2026-01-18 16:36:46',1,'2026-01-19 01:52:59','2612aa7c-259c-4cc0-b435-b966b2aa77a7'),(155,12,'2612aa7c-259c-4cc0-b435-b966b2aa77a7','2026-01-26 01:52:59','2026-01-18 16:52:59',1,'2026-01-19 06:56:12','12c265c3-c721-4d60-86d2-7b51904efe01'),(156,12,'5cb51ce5-36e2-4475-8d21-e9e372ecbb02','2026-01-26 05:06:29','2026-01-18 20:06:29',0,NULL,NULL),(157,12,'12c265c3-c721-4d60-86d2-7b51904efe01','2026-01-26 06:56:12','2026-01-18 21:56:12',0,NULL,NULL),(158,12,'f3f271ff-c998-44c3-9d32-588a4f223d4c','2026-01-26 06:56:12','2026-01-18 21:56:12',0,NULL,NULL),(159,12,'243e675a-e259-4e9a-8fcf-455b1dd34c18','2026-01-26 06:56:12','2026-01-18 21:56:12',0,NULL,NULL),(160,12,'ac623f0a-7f8c-4cec-93bf-e91ae5ee3913','2026-01-26 07:03:15','2026-01-18 22:03:15',1,'2026-01-19 07:18:16','d5e39b08-5b17-4d6c-b24e-7271104ea5b4'),(161,12,'d5e39b08-5b17-4d6c-b24e-7271104ea5b4','2026-01-26 07:18:16','2026-01-18 22:18:16',1,'2026-01-19 07:38:12','102e78af-c5c7-4014-bcf3-6d0478d86f36'),(162,12,'4847e13b-7cd5-4385-a384-ec2b89610085','2026-01-26 07:20:26','2026-01-18 22:20:26',0,NULL,NULL),(163,12,'102e78af-c5c7-4014-bcf3-6d0478d86f36','2026-01-26 07:38:12','2026-01-18 22:38:12',1,'2026-01-19 08:01:02','a8a42497-880d-4f36-91cf-aa4aea7db1fb'),(164,12,'8c40b7ee-bcb5-4171-a182-7c685d128727','2026-01-26 07:40:54','2026-01-18 22:40:54',0,NULL,NULL),(165,12,'90c22f0a-1b25-4daa-86d3-44f04c44e53a','2026-01-26 08:01:02','2026-01-18 23:01:02',0,NULL,NULL),(166,12,'2405b2e4-76d2-4d99-9c3c-6fb81d482465','2026-01-26 08:01:02','2026-01-18 23:01:02',0,NULL,NULL),(167,12,'a8a42497-880d-4f36-91cf-aa4aea7db1fb','2026-01-26 08:01:02','2026-01-18 23:01:02',1,'2026-01-19 23:57:39','86ce546a-9ff1-40b7-b43d-66a1fafbd554'),(168,12,'9a51351e-2770-444e-8438-855003d077e4','2026-01-26 23:57:39','2026-01-19 14:57:39',0,NULL,NULL),(169,12,'8d927c60-c0fa-4196-8a08-44e783108dab','2026-01-26 23:57:40','2026-01-19 14:57:40',0,NULL,NULL),(170,12,'86ce546a-9ff1-40b7-b43d-66a1fafbd554','2026-01-26 23:57:40','2026-01-19 14:57:40',1,'2026-01-20 00:14:12','293ac117-3a13-461d-abb0-3a99a37ed690'),(171,12,'293ac117-3a13-461d-abb0-3a99a37ed690','2026-01-27 00:14:12','2026-01-19 15:14:12',1,'2026-01-20 00:30:32','ac2666c6-6a6d-431c-afd8-e35033c9d5e3'),(172,12,'ac2666c6-6a6d-431c-afd8-e35033c9d5e3','2026-01-27 00:30:32','2026-01-19 15:30:32',1,'2026-01-20 00:46:36','615c40e2-2cce-4d87-89cd-b3687549a502'),(173,12,'615c40e2-2cce-4d87-89cd-b3687549a502','2026-01-27 00:46:36','2026-01-19 15:46:36',0,NULL,NULL),(174,12,'c728f649-1a69-4187-9f02-81f180ad2366','2026-01-27 00:46:36','2026-01-19 15:46:36',1,'2026-01-20 01:06:38','2e9ec324-9b0f-43d1-8bcb-a7b18e618190'),(175,12,'2e9ec324-9b0f-43d1-8bcb-a7b18e618190','2026-01-27 01:06:38','2026-01-19 16:06:38',0,NULL,NULL),(176,12,'d3041486-4d25-4fe8-aa03-2c672b8c0bb5','2026-01-27 01:07:09','2026-01-19 16:07:09',1,'2026-01-20 02:05:37','45f0c8d3-1cda-4e5e-9f27-f920e0ac1f89'),(177,12,'3e5e4fb7-57ae-4702-bfdd-09b3e330131f','2026-01-27 02:05:37','2026-01-19 17:05:37',0,NULL,NULL),(178,12,'05a2b5ad-ad8d-48c1-88e0-117ab16c74f0','2026-01-27 02:05:37','2026-01-19 17:05:37',0,NULL,NULL),(179,12,'45f0c8d3-1cda-4e5e-9f27-f920e0ac1f89','2026-01-27 02:05:37','2026-01-19 17:05:37',1,'2026-01-20 03:51:13','c60c7aac-be57-4f42-810a-2d0f4408118f'),(180,12,'2449d1f4-44b5-4f3a-8052-209611ef0de7','2026-01-27 02:06:15','2026-01-19 17:06:15',0,NULL,NULL),(181,12,'29146020-290e-401a-bfc0-1ea7a0c2edd2','2026-01-27 03:51:13','2026-01-19 18:51:13',0,NULL,NULL),(182,12,'6fb6d6d7-61ef-47b4-b864-4b81e1735743','2026-01-27 03:51:13','2026-01-19 18:51:13',0,NULL,NULL),(183,12,'c60c7aac-be57-4f42-810a-2d0f4408118f','2026-01-27 03:51:13','2026-01-19 18:51:13',1,'2026-01-20 04:51:26','df4b855d-7e94-425a-b05c-f8685fb0d25d'),(184,12,'8bd2c336-af22-4bad-914c-381160c7a79c','2026-01-27 04:21:05','2026-01-19 19:21:05',0,NULL,NULL),(185,12,'82c2176a-87d9-44ca-a8f8-3b935efb7352','2026-01-27 04:51:26','2026-01-19 19:51:26',0,NULL,NULL),(186,12,'9ad0c830-1a37-4fd3-87e4-809da0d55352','2026-01-27 04:51:26','2026-01-19 19:51:26',1,'2026-01-20 05:32:29','31363ecf-996a-44be-92ac-cf103540f137'),(187,12,'df4b855d-7e94-425a-b05c-f8685fb0d25d','2026-01-27 04:51:26','2026-01-19 19:51:26',0,NULL,NULL),(188,12,'0d670dbd-d89c-4492-b3e3-f5b54f8ef1d1','2026-01-27 05:32:29','2026-01-19 20:32:29',0,NULL,NULL),(189,12,'31363ecf-996a-44be-92ac-cf103540f137','2026-01-27 05:32:29','2026-01-19 20:32:29',0,NULL,NULL),(190,12,'bdc46c4c-0c6d-4e37-b1e0-a391811749ac','2026-01-27 05:32:29','2026-01-19 20:32:29',1,'2026-01-20 06:00:26','bd2ef99e-fc14-4ece-9ef1-9c2483c3e9b3'),(191,12,'57130088-c334-4c3c-a749-46f253efedbe','2026-01-27 06:00:26','2026-01-19 21:00:26',0,NULL,NULL),(192,12,'bd2ef99e-fc14-4ece-9ef1-9c2483c3e9b3','2026-01-27 06:00:26','2026-01-19 21:00:26',1,'2026-01-20 06:20:35','2457cb27-0ea4-4612-8ca7-2211359de0c0'),(193,12,'20837aa3-608b-47a4-b9d7-8c292d0fe9ad','2026-01-27 06:00:26','2026-01-19 21:00:26',0,NULL,NULL),(194,12,'2457cb27-0ea4-4612-8ca7-2211359de0c0','2026-01-27 06:20:35','2026-01-19 21:20:35',1,'2026-01-20 06:47:31','06c0cd12-aac9-4551-a1f3-dfe35b12a88a'),(195,12,'06c0cd12-aac9-4551-a1f3-dfe35b12a88a','2026-01-27 06:47:31','2026-01-19 21:47:31',0,NULL,NULL),(196,12,'262c5620-2c14-48c0-8dcd-7f576230e2a4','2026-01-27 06:47:31','2026-01-19 21:47:31',1,'2026-01-20 07:21:37','9ad97b71-c0dd-40a0-b953-e4035e41e880'),(197,12,'48071126-d0fb-4adb-a9f1-8b4b0f3bddb6','2026-01-27 06:47:31','2026-01-19 21:47:31',0,NULL,NULL),(198,12,'9ad97b71-c0dd-40a0-b953-e4035e41e880','2026-01-27 07:21:37','2026-01-19 22:21:37',0,NULL,NULL),(199,12,'0b398ec1-f1ab-4f9e-b806-b742178c6337','2026-01-27 07:21:37','2026-01-19 22:21:37',0,NULL,NULL),(200,12,'3c626738-566a-45bb-bfdf-03d0513f0f17','2026-01-27 07:21:55','2026-01-19 22:21:55',1,'2026-01-20 07:40:34','21565032-8318-4198-9d7e-916e4734398f'),(201,12,'21565032-8318-4198-9d7e-916e4734398f','2026-01-27 07:40:35','2026-01-19 22:40:35',1,'2026-01-20 08:07:27','f3c214d9-38bc-426c-810b-a1c3c6039fce'),(202,12,'f3c214d9-38bc-426c-810b-a1c3c6039fce','2026-01-27 08:07:27','2026-01-19 23:07:27',0,NULL,NULL),(203,12,'b41f5ea9-7adb-4cb8-b74d-32749ac7bafb','2026-01-27 08:09:21','2026-01-19 23:09:21',1,'2026-01-20 09:48:12','8e66e206-912c-49b3-9173-a39b3fa8811f'),(204,12,'e91d615c-0974-4e2a-99fb-6e66a8e8c45c','2026-01-27 09:48:13','2026-01-20 00:48:13',1,'2026-01-21 04:00:18','fc5d7444-be92-49b2-af23-e2bbf42d58d3'),(205,12,'6aaadf13-7deb-46a3-99e8-f53a6a1fb23a','2026-01-27 09:48:13','2026-01-20 00:48:13',0,NULL,NULL),(206,12,'8e66e206-912c-49b3-9173-a39b3fa8811f','2026-01-27 09:48:13','2026-01-20 00:48:13',0,NULL,NULL),(207,12,'fc5d7444-be92-49b2-af23-e2bbf42d58d3','2026-01-28 04:00:18','2026-01-20 19:00:18',1,'2026-01-21 04:49:48','0854a424-78ba-44f2-bb54-e3d309772e79'),(208,12,'8cb612d4-7bb9-48c8-b015-073d865b45d6','2026-01-28 04:00:18','2026-01-20 19:00:18',0,NULL,NULL),(209,12,'49dcd64c-40d6-4132-989b-779304525f52','2026-01-28 04:00:18','2026-01-20 19:00:18',0,NULL,NULL),(210,12,'bbc56d5c-35b6-4c7a-ab94-249a20e7491f','2026-01-28 04:49:48','2026-01-20 19:49:48',0,NULL,NULL),(211,12,'f7b3c968-b62e-40cd-ae3e-23439c958190','2026-01-28 04:49:48','2026-01-20 19:49:48',0,NULL,NULL),(212,12,'0854a424-78ba-44f2-bb54-e3d309772e79','2026-01-28 04:49:48','2026-01-20 19:49:48',1,'2026-01-21 05:20:46','ed670cb0-f3b7-43b4-9109-a3b779fbd041'),(213,12,'80f7bb38-f734-48a4-96cb-a4984171bfb7','2026-01-28 05:17:30','2026-01-20 20:17:30',1,'2026-01-21 05:32:35','073e0768-2e98-46b4-9167-adbc7771669a'),(214,12,'ed670cb0-f3b7-43b4-9109-a3b779fbd041','2026-01-28 05:20:46','2026-01-20 20:20:46',0,NULL,NULL),(215,15,'2d5d1b60-77b4-4d6d-afb9-0d078619009b','2026-01-28 05:21:24','2026-01-20 20:21:24',1,'2026-01-21 05:39:47','ba69707e-9362-455f-8204-c58ee856aee6'),(216,12,'b716af38-74bb-4ea2-ab07-2bb6a15ba7c2','2026-01-28 05:32:35','2026-01-20 20:32:35',0,NULL,NULL),(217,12,'073e0768-2e98-46b4-9167-adbc7771669a','2026-01-28 05:32:35','2026-01-20 20:32:35',0,NULL,NULL),(218,12,'a788b608-63ed-41a3-98dc-6f81704ccea5','2026-01-28 05:33:17','2026-01-20 20:33:17',1,'2026-01-21 05:51:21','aa455ccd-9d57-4e50-b545-2df2a2c8ef64'),(219,15,'ba69707e-9362-455f-8204-c58ee856aee6','2026-01-28 05:39:48','2026-01-20 20:39:48',1,'2026-01-21 06:02:34','8d91ec02-aa5e-47f4-babb-1206837a49c3'),(220,12,'aa455ccd-9d57-4e50-b545-2df2a2c8ef64','2026-01-28 05:51:21','2026-01-20 20:51:21',1,'2026-01-21 06:08:24','e9cedfda-ea25-4abd-9a3b-6778afae9215'),(221,15,'8d91ec02-aa5e-47f4-babb-1206837a49c3','2026-01-28 06:02:34','2026-01-20 21:02:34',0,NULL,NULL),(222,15,'60306b90-638a-4d71-bef6-e7c71eedd8e4','2026-01-28 06:03:30','2026-01-20 21:03:30',1,'2026-01-21 06:19:31','e3e647da-1bd3-45a9-945e-90627f5a399a'),(223,12,'e9cedfda-ea25-4abd-9a3b-6778afae9215','2026-01-28 06:08:24','2026-01-20 21:08:24',0,NULL,NULL),(224,12,'bf745461-5985-4c75-bc5b-b4852f39e2e3','2026-01-28 06:11:31','2026-01-20 21:11:31',1,'2026-01-21 06:26:38','2ca27cb9-bc56-499a-9329-3495f10f6005'),(225,15,'e3e647da-1bd3-45a9-945e-90627f5a399a','2026-01-28 06:19:31','2026-01-20 21:19:31',0,NULL,NULL),(226,15,'206ea6cc-7ae6-4d64-a2b8-5b84c9196bdf','2026-01-28 06:20:02','2026-01-20 21:20:02',1,'2026-01-21 06:46:36','7bf16d81-29bf-4e7f-bd9f-9ee068d31a8a'),(227,12,'74cf9c6c-89e1-48d6-ae8b-1def9229f627','2026-01-28 06:26:38','2026-01-20 21:26:38',0,NULL,NULL),(228,12,'5412ffe2-fcb8-44d6-8593-5227ab2be2ef','2026-01-28 06:26:38','2026-01-20 21:26:38',1,'2026-01-21 06:46:43','ee90804c-ccf8-4bdf-b6b1-63ec4a565386'),(229,12,'2ca27cb9-bc56-499a-9329-3495f10f6005','2026-01-28 06:26:38','2026-01-20 21:26:38',0,NULL,NULL),(230,15,'7bf16d81-29bf-4e7f-bd9f-9ee068d31a8a','2026-01-28 06:46:36','2026-01-20 21:46:36',0,NULL,NULL),(231,15,'84a16827-342f-472e-8d49-a4961eabaf0a','2026-01-28 06:46:36','2026-01-20 21:46:36',0,NULL,NULL),(232,12,'cf4c7ef5-8c20-4398-b228-7fcdd6dd33e4','2026-01-28 06:46:43','2026-01-20 21:46:43',0,NULL,NULL),(233,12,'27ef6b16-b7d5-4ee6-8737-79fb20c0739d','2026-01-28 06:46:43','2026-01-20 21:46:43',0,NULL,NULL),(234,12,'ee90804c-ccf8-4bdf-b6b1-63ec4a565386','2026-01-28 06:46:43','2026-01-20 21:46:43',1,'2026-01-23 02:02:31','c050ed93-43d3-40aa-83ce-7fa486a432e2'),(235,15,'0bdfca8d-de37-42fa-a86c-881508fc1475','2026-01-28 06:47:23','2026-01-20 21:47:23',1,'2026-01-21 07:11:47','b00161ec-94da-4414-9f07-65452735d9f5'),(236,15,'b00161ec-94da-4414-9f07-65452735d9f5','2026-01-28 07:11:47','2026-01-20 22:11:47',0,NULL,NULL),(237,12,'d4f256ed-1bb7-454c-963f-91c225349c2c','2026-01-28 07:28:44','2026-01-20 22:28:44',1,'2026-01-21 08:02:39','8e2ffbad-623d-4205-9824-9cfc80463ff9'),(238,12,'8e2ffbad-623d-4205-9824-9cfc80463ff9','2026-01-28 08:02:39','2026-01-20 23:02:39',1,'2026-01-22 00:19:32','cb8168d7-bc24-4da6-b557-88666a297647'),(239,12,'cb8168d7-bc24-4da6-b557-88666a297647','2026-01-29 00:19:32','2026-01-21 15:19:32',1,'2026-01-22 00:34:50','4f61271b-7d1f-4aa4-891c-46031b6469d5'),(240,12,'4f61271b-7d1f-4aa4-891c-46031b6469d5','2026-01-29 00:34:51','2026-01-21 15:34:51',1,'2026-01-22 00:52:04','6f554cee-e674-4519-be3f-a5e9f1759ed8'),(241,12,'6f554cee-e674-4519-be3f-a5e9f1759ed8','2026-01-29 00:52:05','2026-01-21 15:52:05',1,'2026-01-22 01:51:24','100d1314-f106-485b-969d-fac8c4269384'),(242,12,'8c7e558d-842f-451f-9657-28e55cc8eb4e','2026-01-29 01:51:24','2026-01-21 16:51:24',0,NULL,NULL),(243,12,'100d1314-f106-485b-969d-fac8c4269384','2026-01-29 01:51:24','2026-01-21 16:51:24',1,'2026-01-22 02:08:30','550de544-e118-40a1-86a0-d6794891b49f'),(244,12,'550de544-e118-40a1-86a0-d6794891b49f','2026-01-29 02:08:30','2026-01-21 17:08:30',0,NULL,NULL),(245,12,'74783aa9-0e89-46a5-8293-0d95f1d573e6','2026-01-29 02:08:30','2026-01-21 17:08:30',1,'2026-01-22 03:35:41','4a7bbfd6-5bf0-44ef-8730-1fe47ceaa07a'),(246,12,'4a7bbfd6-5bf0-44ef-8730-1fe47ceaa07a','2026-01-29 03:35:41','2026-01-21 18:35:41',1,'2026-01-22 03:54:32','62741c05-d233-4931-9f2e-cfd71d3f5232'),(247,12,'171f35bf-1af1-454a-bbc8-527a9cc85198','2026-01-29 03:54:32','2026-01-21 18:54:32',1,'2026-01-22 04:17:51','32e11c37-1b1e-406d-b247-352153e9656b'),(248,12,'62741c05-d233-4931-9f2e-cfd71d3f5232','2026-01-29 03:54:32','2026-01-21 18:54:32',0,NULL,NULL),(249,12,'32e11c37-1b1e-406d-b247-352153e9656b','2026-01-29 04:17:51','2026-01-21 19:17:51',1,'2026-01-22 05:02:36','04b5b877-c8be-4628-aab6-a4e0be31d258'),(250,12,'04b5b877-c8be-4628-aab6-a4e0be31d258','2026-01-29 05:02:37','2026-01-21 20:02:37',1,'2026-01-22 05:17:36','3ed58c8f-f685-40ec-b083-e5154ba18273'),(251,12,'3ed58c8f-f685-40ec-b083-e5154ba18273','2026-01-29 05:17:36','2026-01-21 20:17:36',1,'2026-01-22 05:34:08','d87e18bf-610a-4f7f-b363-063a1dc393ad'),(252,12,'d87e18bf-610a-4f7f-b363-063a1dc393ad','2026-01-29 05:34:08','2026-01-21 20:34:08',1,'2026-01-22 06:45:45','811115f8-0421-4b2c-a499-7f1d26ae832f'),(253,12,'811115f8-0421-4b2c-a499-7f1d26ae832f','2026-01-29 06:45:45','2026-01-21 21:45:45',1,'2026-01-22 07:23:21','bb2183fe-5df7-420c-b770-96504a03fbf1'),(254,12,'9be2c508-c69a-49fe-818a-c415f53b3fd3','2026-01-29 07:23:21','2026-01-21 22:23:21',1,'2026-01-22 23:43:06','9c0b0b38-3cb7-4168-bcb6-ebb19c80c8bc'),(255,12,'bb2183fe-5df7-420c-b770-96504a03fbf1','2026-01-29 07:23:21','2026-01-21 22:23:21',0,NULL,NULL),(256,12,'9c0b0b38-3cb7-4168-bcb6-ebb19c80c8bc','2026-01-29 23:43:06','2026-01-22 14:43:06',1,'2026-01-23 00:30:56','399a85eb-53a3-408b-8ecb-10876f9d3639'),(257,12,'d9079e09-ce6b-4de0-bbd2-2b58fd80fc79','2026-01-29 23:43:06','2026-01-22 14:43:06',0,NULL,NULL),(258,12,'399a85eb-53a3-408b-8ecb-10876f9d3639','2026-01-30 00:30:56','2026-01-22 15:30:56',1,'2026-01-23 01:10:07','d21b3e57-8d82-4fd5-b9dc-c333cefbd58b'),(259,12,'d21b3e57-8d82-4fd5-b9dc-c333cefbd58b','2026-01-30 01:10:07','2026-01-22 16:10:07',0,NULL,NULL),(260,12,'e1bb1184-4a91-447f-bb1c-e6b364d41cf1','2026-01-30 01:10:07','2026-01-22 16:10:07',1,'2026-01-23 01:27:20','cabcc0d6-7890-4696-959e-157dddf2998a'),(261,12,'8ffc438a-0556-41f2-aaea-c904ebda4292','2026-01-30 01:27:20','2026-01-22 16:27:20',0,NULL,NULL),(262,12,'44c4f2a8-d219-4036-ae93-888d4476eb16','2026-01-30 01:27:20','2026-01-22 16:27:20',1,'2026-01-23 01:54:39','b3fe25eb-9593-4f2b-a39c-20302b9493e8'),(263,12,'cabcc0d6-7890-4696-959e-157dddf2998a','2026-01-30 01:27:20','2026-01-22 16:27:20',0,NULL,NULL),(264,12,'b3fe25eb-9593-4f2b-a39c-20302b9493e8','2026-01-30 01:54:39','2026-01-22 16:54:39',0,NULL,NULL),(265,12,'7dfd4911-2b98-4b4b-9243-4c9dde2165a6','2026-01-30 02:02:31','2026-01-22 17:02:31',1,'2026-01-23 02:22:43','221bf9b2-0b5c-4cfd-a5f9-a8da5425e53d'),(266,12,'4db952dc-4bd7-4ca7-abb4-ade8ab1e0ab4','2026-01-30 02:02:31','2026-01-22 17:02:31',0,NULL,NULL),(267,12,'c050ed93-43d3-40aa-83ce-7fa486a432e2','2026-01-30 02:02:31','2026-01-22 17:02:31',0,NULL,NULL),(268,15,'f05c026a-4541-425d-95b1-dd5115862eee','2026-01-30 02:03:49','2026-01-22 17:03:49',1,'2026-01-23 02:21:34','db892cfb-6b40-4c11-886f-952201bc73f9'),(269,15,'db892cfb-6b40-4c11-886f-952201bc73f9','2026-01-30 02:21:34','2026-01-22 17:21:34',1,'2026-01-23 02:37:40','2dd8f115-5843-49eb-a5ea-d90d80b67940'),(270,12,'221bf9b2-0b5c-4cfd-a5f9-a8da5425e53d','2026-01-30 02:22:43','2026-01-22 17:22:43',1,'2026-01-23 02:37:43','6b14bce6-d775-418c-aaeb-b6ee45486460'),(271,15,'2dd8f115-5843-49eb-a5ea-d90d80b67940','2026-01-30 02:37:40','2026-01-22 17:37:40',1,'2026-01-23 04:07:42','b48c2e60-c705-44b1-80a0-3b3c7c3bcf09'),(272,12,'6b14bce6-d775-418c-aaeb-b6ee45486460','2026-01-30 02:37:43','2026-01-22 17:37:43',1,'2026-01-23 04:09:35','d5504783-0dff-4361-b7c7-a8bad0da31d6'),(273,15,'8b92c51b-31f9-40fe-a13f-d36b5837cab3','2026-01-30 04:07:42','2026-01-22 19:07:42',0,NULL,NULL),(274,15,'6baf73c7-9f3c-4e93-ab14-a1caa12d1104','2026-01-30 04:07:42','2026-01-22 19:07:42',0,NULL,NULL),(275,15,'b48c2e60-c705-44b1-80a0-3b3c7c3bcf09','2026-01-30 04:07:42','2026-01-22 19:07:42',1,'2026-01-23 04:44:37','9dc59bce-b387-44f8-9fb6-7cda3c6aeb7a'),(276,12,'d5504783-0dff-4361-b7c7-a8bad0da31d6','2026-01-30 04:09:35','2026-01-22 19:09:35',1,'2026-01-23 04:45:03','e71b2540-11b3-4de4-abbc-bfd83f52eaf0'),(277,15,'9dc59bce-b387-44f8-9fb6-7cda3c6aeb7a','2026-01-30 04:44:37','2026-01-22 19:44:37',1,'2026-01-23 05:03:49','c06ad476-8195-4f57-b03a-db1be8d11c55'),(278,15,'6301c0db-2777-4fa9-8afd-2be385f2d7ee','2026-01-30 04:44:37','2026-01-22 19:44:37',0,NULL,NULL),(279,12,'15df7603-9646-49fd-96f6-e0ecd0a11029','2026-01-30 04:45:03','2026-01-22 19:45:03',0,NULL,NULL),(280,12,'86f73bbd-e091-4e57-9e7e-355c4889d43b','2026-01-30 04:45:03','2026-01-22 19:45:03',1,'2026-01-23 05:03:45','f3c73464-7e89-40c4-867a-d1b975ad382e'),(281,12,'e71b2540-11b3-4de4-abbc-bfd83f52eaf0','2026-01-30 04:45:03','2026-01-22 19:45:03',0,NULL,NULL),(282,12,'9cf76a7c-2639-4d5d-8759-9a99d7bda95d','2026-01-30 05:03:45','2026-01-22 20:03:45',0,NULL,NULL),(283,12,'0f757ac3-66a5-47ba-a747-e3cbefa142ed','2026-01-30 05:03:45','2026-01-22 20:03:45',1,'2026-01-25 23:11:16','69aec978-09f7-4aea-8685-ecc0555c31f7'),(284,12,'f3c73464-7e89-40c4-867a-d1b975ad382e','2026-01-30 05:03:45','2026-01-22 20:03:45',0,NULL,NULL),(285,15,'c06ad476-8195-4f57-b03a-db1be8d11c55','2026-01-30 05:03:49','2026-01-22 20:03:49',0,NULL,NULL),(286,15,'dc450ee1-c018-4d8d-8784-84ed5a53fd53','2026-01-30 05:04:11','2026-01-22 20:04:11',1,'2026-01-25 23:08:43','dc9ba30d-c315-4009-8063-fc837bf8de11'),(287,15,'dc9ba30d-c315-4009-8063-fc837bf8de11','2026-02-01 23:08:43','2026-01-25 14:08:43',1,'2026-01-25 23:24:03','8fdb189c-0718-4cd4-9816-76db86afe6a3'),(288,15,'375ddb3b-f1a1-424b-a289-fe785ad36253','2026-02-01 23:08:43','2026-01-25 14:08:43',0,NULL,NULL),(289,12,'e8462434-1eab-40e5-a53e-5f86ef564d53','2026-02-01 23:11:16','2026-01-25 14:11:16',0,NULL,NULL),(290,12,'a3c93428-ac81-403e-a5ac-3ae7fe43d929','2026-02-01 23:11:16','2026-01-25 14:11:16',1,'2026-01-25 23:26:26','8a0e264c-db4b-4b74-9372-4a5ed1879f25'),(291,12,'69aec978-09f7-4aea-8685-ecc0555c31f7','2026-02-01 23:11:16','2026-01-25 14:11:16',0,NULL,NULL),(292,15,'8fdb189c-0718-4cd4-9816-76db86afe6a3','2026-02-01 23:24:03','2026-01-25 14:24:03',1,'2026-01-25 23:40:17','ca85af86-167c-4688-b6a2-690a18cd3c85'),(293,12,'8a0e264c-db4b-4b74-9372-4a5ed1879f25','2026-02-01 23:26:26','2026-01-25 14:26:26',1,'2026-01-25 23:41:59','7c5517b7-f8dd-4c10-b675-c0301a450e32'),(294,15,'ca85af86-167c-4688-b6a2-690a18cd3c85','2026-02-01 23:40:17','2026-01-25 14:40:17',1,'2026-01-26 00:11:22','91ebbac3-3a38-4b53-804c-fba7b05d67aa'),(295,12,'7c5517b7-f8dd-4c10-b675-c0301a450e32','2026-02-01 23:42:00','2026-01-25 14:42:00',1,'2026-01-26 00:22:28','a88d2016-c4dc-4a4a-adb0-a740170c3e31'),(296,15,'91ebbac3-3a38-4b53-804c-fba7b05d67aa','2026-02-02 00:11:22','2026-01-25 15:11:22',1,'2026-01-26 00:27:27','10431f20-5706-4d68-9301-225deac7dff2'),(297,15,'daa1b7d8-bf34-4a82-bd90-7ea9742f87a8','2026-02-02 00:11:22','2026-01-25 15:11:22',0,NULL,NULL),(298,12,'a88d2016-c4dc-4a4a-adb0-a740170c3e31','2026-02-02 00:22:28','2026-01-25 15:22:28',1,'2026-01-26 00:37:33','d1ea7ae0-bd73-415c-96ed-6d0efc02bc83'),(299,15,'2ddb4179-c606-4563-b2a8-edecf31b413e','2026-02-02 00:27:27','2026-01-25 15:27:27',0,NULL,NULL),(300,15,'10431f20-5706-4d68-9301-225deac7dff2','2026-02-02 00:27:27','2026-01-25 15:27:27',1,'2026-01-26 01:03:43','3128d0cb-8b93-462e-9a59-9b8983b97e85'),(301,12,'d1ea7ae0-bd73-415c-96ed-6d0efc02bc83','2026-02-02 00:37:33','2026-01-25 15:37:33',1,'2026-01-26 01:04:27','917bc5aa-ff83-472d-87b8-42adc9f4f56d'),(302,15,'3128d0cb-8b93-462e-9a59-9b8983b97e85','2026-02-02 01:03:43','2026-01-25 16:03:43',1,'2026-01-26 01:21:34','7cb981c1-fb12-49e6-8ae2-56660835d8c5'),(303,12,'917bc5aa-ff83-472d-87b8-42adc9f4f56d','2026-02-02 01:04:27','2026-01-25 16:04:27',1,'2026-01-26 01:21:01','d58c28a1-48a6-401b-9071-e490351917e6'),(304,12,'2aceb136-c632-407f-8d2a-df275a62b506','2026-02-02 01:21:01','2026-01-25 16:21:01',1,'2026-01-26 03:47:04','0618278a-debe-4c02-92af-b52b22be144f'),(305,12,'d58c28a1-48a6-401b-9071-e490351917e6','2026-02-02 01:21:01','2026-01-25 16:21:01',0,NULL,NULL),(306,12,'b8cff111-575f-44ed-9d28-746fb8ec3842','2026-02-02 01:21:01','2026-01-25 16:21:01',0,NULL,NULL),(307,15,'7cb981c1-fb12-49e6-8ae2-56660835d8c5','2026-02-02 01:21:34','2026-01-25 16:21:34',0,NULL,NULL),(308,12,'c74a361d-39c2-46c6-8f22-368cd48a9ce2','2026-02-02 01:26:35','2026-01-25 16:26:35',1,'2026-01-26 01:52:24','cc7ae720-ae9a-4031-9db4-448e402c4b51'),(309,12,'cc7ae720-ae9a-4031-9db4-448e402c4b51','2026-02-02 01:52:25','2026-01-25 16:52:25',0,NULL,NULL),(310,12,'eda3fe6b-295f-457e-8595-f866ab3da240','2026-02-02 01:52:25','2026-01-25 16:52:25',0,NULL,NULL),(311,12,'c244186e-0878-444a-861f-d40b449ef45b','2026-02-02 01:52:25','2026-01-25 16:52:25',1,'2026-01-26 03:32:23','0e014a67-c2a4-4a46-80bc-a7c20e17790a'),(312,12,'cf91c2d8-df81-4f2a-b687-5f8255924b6c','2026-02-02 01:52:25','2026-01-25 16:52:25',0,NULL,NULL),(313,12,'0e014a67-c2a4-4a46-80bc-a7c20e17790a','2026-02-02 03:32:23','2026-01-25 18:32:23',0,NULL,NULL),(314,12,'e9944f8d-8cd3-440b-a2c8-c56136ca22c7','2026-02-02 03:32:23','2026-01-25 18:32:23',0,NULL,NULL),(315,12,'d3a26132-4c52-4a4a-81e2-a87d1314fa21','2026-02-02 03:36:59','2026-01-25 18:36:59',0,NULL,NULL),(316,12,'2888175a-d8eb-483d-88c3-eb8005e01fbf','2026-02-02 03:47:04','2026-01-25 18:47:04',0,NULL,NULL),(317,12,'6a0d04a5-a7b2-4b3d-9144-7394b4c491cd','2026-02-02 03:47:04','2026-01-25 18:47:04',1,'2026-01-26 04:35:09','e078497c-e082-4f21-b61c-bd273aa3c38d'),(318,12,'0618278a-debe-4c02-92af-b52b22be144f','2026-02-02 03:47:04','2026-01-25 18:47:04',0,NULL,NULL),(319,15,'b1550d3e-dc55-42a4-83f7-040698d916c3','2026-02-02 03:49:28','2026-01-25 18:49:28',1,'2026-01-26 04:34:20','1c768c87-fb6f-491d-b111-6f6f5669e664'),(320,12,'978f157c-463e-4159-9bb1-f3398296cb9b','2026-02-02 03:57:33','2026-01-25 18:57:33',0,NULL,NULL),(321,15,'1c768c87-fb6f-491d-b111-6f6f5669e664','2026-02-02 04:34:20','2026-01-25 19:34:20',1,'2026-01-26 04:49:19','ca480866-b846-48ff-a50e-69d34f987ca6'),(322,12,'e078497c-e082-4f21-b61c-bd273aa3c38d','2026-02-02 04:35:09','2026-01-25 19:35:09',1,'2026-01-26 04:50:16','c0f37e13-c3bb-4889-b75f-1ad22af3e0c4'),(323,15,'ca480866-b846-48ff-a50e-69d34f987ca6','2026-02-02 04:49:20','2026-01-25 19:49:20',1,'2026-01-26 05:11:19','147932b3-1244-46fa-86d2-ce9275ea66ab'),(324,12,'c0f37e13-c3bb-4889-b75f-1ad22af3e0c4','2026-02-02 04:50:16','2026-01-25 19:50:16',1,'2026-01-26 05:11:18','e216e6e8-eeea-4a53-8137-d6e749ee7af9'),(325,12,'e216e6e8-eeea-4a53-8137-d6e749ee7af9','2026-02-02 05:11:18','2026-01-25 20:11:18',1,'2026-01-26 05:28:01','35c0162b-ab99-45cb-bd00-dcef553f3b3d'),(326,12,'5962709a-6652-4b5a-98a4-1e9a6e18150c','2026-02-02 05:11:18','2026-01-25 20:11:18',0,NULL,NULL),(327,15,'bcb9afdb-78bc-4307-b9d7-ea977e31554e','2026-02-02 05:11:19','2026-01-25 20:11:19',0,NULL,NULL),(328,15,'147932b3-1244-46fa-86d2-ce9275ea66ab','2026-02-02 05:11:19','2026-01-25 20:11:19',1,'2026-01-26 05:27:15','771ba188-edf0-4db3-8458-4e5633b59135'),(329,15,'771ba188-edf0-4db3-8458-4e5633b59135','2026-02-02 05:27:15','2026-01-25 20:27:15',1,'2026-01-26 05:56:20','8fefc7b2-c83e-4845-8fde-b75066a5455a'),(330,12,'35c0162b-ab99-45cb-bd00-dcef553f3b3d','2026-02-02 05:28:01','2026-01-25 20:28:01',1,'2026-01-26 05:56:20','ee3f6b80-b379-4ea8-9403-d8e06b1454bd'),(331,15,'91cfbf2f-6675-4f0d-98bb-138e7ce309ec','2026-02-02 05:32:31','2026-01-25 20:32:31',0,NULL,NULL),(332,12,'ee3f6b80-b379-4ea8-9403-d8e06b1454bd','2026-02-02 05:56:20','2026-01-25 20:56:20',1,'2026-01-27 08:06:50','e5abcea2-15db-439e-bf34-dc37e1349215'),(333,15,'26109c47-f3b8-46ed-8bbe-562468816f32','2026-02-02 05:56:20','2026-01-25 20:56:20',0,NULL,NULL),(334,15,'8fefc7b2-c83e-4845-8fde-b75066a5455a','2026-02-02 05:56:20','2026-01-25 20:56:20',1,'2026-01-26 06:18:31','0dd72a11-aa18-43af-aa43-fb80ad1ae275'),(335,15,'0dd72a11-aa18-43af-aa43-fb80ad1ae275','2026-02-02 06:18:32','2026-01-25 21:18:32',1,'2026-01-26 06:45:54','ed46c407-15ab-47b5-8cc5-2ef4ab5ea91f'),(336,15,'ed46c407-15ab-47b5-8cc5-2ef4ab5ea91f','2026-02-02 06:45:54','2026-01-25 21:45:54',1,'2026-01-26 07:00:56','24a5f8fc-b9e8-481b-b1eb-124129db0825'),(337,15,'24a5f8fc-b9e8-481b-b1eb-124129db0825','2026-02-02 07:00:56','2026-01-25 22:00:56',1,'2026-01-26 07:27:56','1e43c21b-6b35-47d0-81b4-18feda9c426f'),(338,15,'1e43c21b-6b35-47d0-81b4-18feda9c426f','2026-02-02 07:27:56','2026-01-25 22:27:56',1,'2026-01-26 07:56:47','ec864fe2-c183-4296-8eab-8178f34a4bc2'),(339,15,'ec864fe2-c183-4296-8eab-8178f34a4bc2','2026-02-02 07:56:47','2026-01-25 22:56:47',1,'2026-01-26 23:56:44','03975552-b5ab-47fb-8c53-a70c8c4f55bd'),(340,15,'7ae1a491-a9c7-4127-b4d9-1d0665f98a78','2026-02-02 23:56:44','2026-01-26 14:56:44',0,NULL,NULL),(341,15,'03975552-b5ab-47fb-8c53-a70c8c4f55bd','2026-02-02 23:56:44','2026-01-26 14:56:44',1,'2026-01-27 00:25:38','0dd11f81-a95e-44dc-ac5d-55109be5c794'),(342,15,'a729c97c-2e64-4c05-b0ee-1a20aa2db6f4','2026-02-03 00:25:38','2026-01-26 15:25:38',1,'2026-01-27 00:40:54','6cb59cd9-1404-4287-a7f9-37e2060e1733'),(343,15,'0dd11f81-a95e-44dc-ac5d-55109be5c794','2026-02-03 00:25:38','2026-01-26 15:25:38',0,NULL,NULL),(344,15,'684b43cf-8748-4783-b0a8-1623f80fb42c','2026-02-03 00:25:38','2026-01-26 15:25:38',0,NULL,NULL),(345,15,'6cb59cd9-1404-4287-a7f9-37e2060e1733','2026-02-03 00:40:54','2026-01-26 15:40:54',1,'2026-01-27 01:08:48','8c7fc11c-3a6a-420a-8e23-9e1243ba11c1'),(346,15,'8c7fc11c-3a6a-420a-8e23-9e1243ba11c1','2026-02-03 01:08:48','2026-01-26 16:08:48',1,'2026-01-27 02:07:11','89e5c614-e33f-48bb-b4f8-c153a83efe54'),(347,15,'89e5c614-e33f-48bb-b4f8-c153a83efe54','2026-02-03 02:07:11','2026-01-26 17:07:11',1,'2026-01-27 04:12:47','aa005282-ec8c-4081-86a6-66d7774d05e8'),(348,15,'aa005282-ec8c-4081-86a6-66d7774d05e8','2026-02-03 04:12:47','2026-01-26 19:12:47',1,'2026-01-27 04:28:56','27d7dc90-cbbe-4d39-9b6f-3ed78d4b1685'),(349,15,'27d7dc90-cbbe-4d39-9b6f-3ed78d4b1685','2026-02-03 04:28:56','2026-01-26 19:28:56',1,'2026-01-27 07:57:51','32985b2c-5109-4d93-b318-fc76b674928f'),(350,15,'f7ef2c4c-1abd-4226-87f3-ee219491f383','2026-02-03 07:57:51','2026-01-26 22:57:51',0,NULL,NULL),(351,15,'32985b2c-5109-4d93-b318-fc76b674928f','2026-02-03 07:57:51','2026-01-26 22:57:51',0,NULL,NULL),(352,12,'e5abcea2-15db-439e-bf34-dc37e1349215','2026-02-03 08:06:50','2026-01-26 23:06:50',0,NULL,NULL),(353,15,'4c118d9b-077d-42f1-b4b2-a7b46f534bc2','2026-02-03 08:12:53','2026-01-26 23:12:53',1,'2026-01-28 00:13:14','280e02e0-82ee-4924-9c8c-fb126e4d4d5d'),(354,15,'89696c24-7235-4ead-bb28-8e0002b79e0b','2026-02-04 00:13:14','2026-01-27 15:13:14',0,NULL,NULL),(355,15,'280e02e0-82ee-4924-9c8c-fb126e4d4d5d','2026-02-04 00:13:14','2026-01-27 15:13:14',1,'2026-01-28 00:34:34','adb7decf-3d45-40e0-bc81-c54c04b115bb'),(356,15,'54e90805-b1b3-40ce-896e-2e18eed12c28','2026-02-04 00:34:34','2026-01-27 15:34:34',0,NULL,NULL),(357,15,'adb7decf-3d45-40e0-bc81-c54c04b115bb','2026-02-04 00:34:34','2026-01-27 15:34:34',1,'2026-01-28 00:49:34','b4be688f-e742-4105-ae0f-132320f09216'),(358,15,'b4be688f-e742-4105-ae0f-132320f09216','2026-02-04 00:49:34','2026-01-27 15:49:34',1,'2026-01-28 01:12:14','315327bd-f01d-444b-ad0e-d4d1a352f262'),(359,15,'315327bd-f01d-444b-ad0e-d4d1a352f262','2026-02-04 01:12:14','2026-01-27 16:12:14',1,'2026-02-02 03:58:23','60680d68-e125-407d-aa0e-d78341dedc23'),(360,15,'f6ef013e-f23d-4c35-ae88-3d467addfe27','2026-02-06 03:59:54','2026-01-29 18:59:54',1,'2026-01-30 04:21:28','748d33e0-e244-44be-976e-e12d38126999'),(361,15,'748d33e0-e244-44be-976e-e12d38126999','2026-02-06 04:21:28','2026-01-29 19:21:28',0,NULL,NULL),(362,15,'888ae33f-be10-4e94-bb92-1abf1c589f3b','2026-02-06 04:21:28','2026-01-29 19:21:28',1,'2026-01-30 04:40:28','08c08cc5-f306-4772-9116-cecc72717dc1'),(363,15,'08c08cc5-f306-4772-9116-cecc72717dc1','2026-02-06 04:40:28','2026-01-29 19:40:28',1,'2026-01-30 05:10:39','4284e36c-6d08-49b1-9ad4-a4ea717e87d0'),(364,15,'4b038b32-5b54-4d2f-8ed7-345743cbdc49','2026-02-06 05:10:39','2026-01-29 20:10:39',0,NULL,NULL),(365,15,'4284e36c-6d08-49b1-9ad4-a4ea717e87d0','2026-02-06 05:10:39','2026-01-29 20:10:39',1,'2026-01-30 05:28:55','642f63c7-0f87-4aee-ac8d-3cd69b574d1f'),(366,15,'642f63c7-0f87-4aee-ac8d-3cd69b574d1f','2026-02-06 05:28:55','2026-01-29 20:28:55',1,'2026-01-30 05:45:18','062f1af2-f627-4c18-b828-8ac4650d4dcc'),(367,15,'879ef8be-0347-4c66-a43a-dfbb5fce93cf','2026-02-06 05:28:55','2026-01-29 20:28:55',0,NULL,NULL),(368,15,'062f1af2-f627-4c18-b828-8ac4650d4dcc','2026-02-06 05:45:18','2026-01-29 20:45:18',1,'2026-01-30 06:00:58','6eb7a87e-98c9-4db0-9304-a18303b60bf8'),(369,15,'6eb7a87e-98c9-4db0-9304-a18303b60bf8','2026-02-06 06:00:58','2026-01-29 21:00:58',1,'2026-01-30 06:16:00','0ab26632-f0a6-453d-b01a-e303b2d50434'),(370,15,'0ab26632-f0a6-453d-b01a-e303b2d50434','2026-02-06 06:16:00','2026-01-29 21:16:00',1,'2026-01-30 06:32:24','c4d769d8-4db4-4f4b-beda-a1bc0014bf59'),(371,15,'c4d769d8-4db4-4f4b-beda-a1bc0014bf59','2026-02-06 06:32:24','2026-01-29 21:32:24',0,NULL,NULL),(372,12,'a65d9a86-baea-4e85-9cb7-4bc9bcb366af','2026-02-06 06:42:58','2026-01-29 21:42:58',0,NULL,NULL),(373,15,'71960832-ef82-43a6-8f5d-ed63a65a00da','2026-02-06 06:43:42','2026-01-29 21:43:42',0,NULL,NULL),(374,12,'949ee3cc-e856-4e63-b3da-b361423cc01a','2026-02-06 06:44:18','2026-01-29 21:44:18',1,'2026-01-30 07:22:52','6e707668-b7cb-4a8a-bf34-2b5a1a138502'),(375,12,'6e707668-b7cb-4a8a-bf34-2b5a1a138502','2026-02-06 07:22:52','2026-01-29 22:22:52',0,NULL,NULL),(376,15,'847261f9-7fae-40f0-80f0-59d9115e18b5','2026-02-06 07:27:59','2026-01-29 22:27:59',1,'2026-02-02 02:23:11','75cce000-0b77-44ba-bc2e-e54bb3194593'),(377,15,'f8de1e5e-56e6-48f8-bbd0-1355cfacd366','2026-02-09 02:23:11','2026-02-01 17:23:11',1,'2026-02-02 03:43:57','c4a387c4-3467-447b-9092-19c0c67089f5'),(378,15,'75cce000-0b77-44ba-bc2e-e54bb3194593','2026-02-09 02:23:11','2026-02-01 17:23:11',0,NULL,NULL),(379,15,'ef3504b4-dd27-44ff-bb8d-cfaecbcc53c7','2026-02-09 03:43:57','2026-02-01 18:43:57',1,'2026-02-02 04:12:09','39e8b9ac-ab30-43da-b69c-7763732ae070'),(380,15,'c4a387c4-3467-447b-9092-19c0c67089f5','2026-02-09 03:43:57','2026-02-01 18:43:57',0,NULL,NULL),(381,15,'0d7285e1-e782-4ab4-8d02-1e21570d79d6','2026-02-09 03:58:23','2026-02-01 18:58:23',0,NULL,NULL),(382,15,'60680d68-e125-407d-aa0e-d78341dedc23','2026-02-09 03:58:23','2026-02-01 18:58:23',0,NULL,NULL),(383,12,'259e73e9-2719-47b3-91bd-e9aefc16f336','2026-02-09 04:11:01','2026-02-01 19:11:01',1,'2026-02-02 04:39:09','06d9ec4e-149c-4dc7-b1ca-7eafba5dbc6a'),(384,15,'39e8b9ac-ab30-43da-b69c-7763732ae070','2026-02-09 04:12:09','2026-02-01 19:12:09',1,'2026-02-02 04:27:32','9fae0fbf-7db5-443a-b4c2-7bfdd40c0cc8'),(385,15,'9fae0fbf-7db5-443a-b4c2-7bfdd40c0cc8','2026-02-09 04:27:32','2026-02-01 19:27:32',1,'2026-02-02 04:45:56','46a5f35e-6516-4ca1-bbd1-34b4cf87bbc0'),(386,12,'06d9ec4e-149c-4dc7-b1ca-7eafba5dbc6a','2026-02-09 04:39:09','2026-02-01 19:39:09',1,'2026-02-02 05:01:56','fbd7ab98-82fd-4d9d-b06b-58593a9f8c11'),(387,12,'b4a12682-4eeb-48f4-8156-b9cc3cc8e71d','2026-02-09 04:39:09','2026-02-01 19:39:09',0,NULL,NULL),(388,15,'46a5f35e-6516-4ca1-bbd1-34b4cf87bbc0','2026-02-09 04:45:56','2026-02-01 19:45:56',1,'2026-02-02 05:02:41','c65795fb-0927-4f9d-ae7d-1187bc19a30d'),(389,12,'fbd7ab98-82fd-4d9d-b06b-58593a9f8c11','2026-02-09 05:01:56','2026-02-01 20:01:56',1,'2026-02-02 05:19:35','dc0c1258-e02f-44f7-bad5-39631dafb824'),(390,15,'c65795fb-0927-4f9d-ae7d-1187bc19a30d','2026-02-09 05:02:41','2026-02-01 20:02:41',1,'2026-02-02 05:19:04','557deabc-aaca-4411-aad0-9a6fc7df6510'),(391,15,'557deabc-aaca-4411-aad0-9a6fc7df6510','2026-02-09 05:19:04','2026-02-01 20:19:04',0,NULL,NULL),(392,15,'2f20fe5f-4443-4726-967c-35e7ab3fcea3','2026-02-09 05:19:23','2026-02-01 20:19:23',1,'2026-02-02 05:47:39','f1b7e52e-d454-4c72-95d4-c46f8cc5b393'),(393,12,'8e077f6e-292a-4718-91d5-2496ec60f720','2026-02-09 05:19:35','2026-02-01 20:19:35',1,'2026-02-02 05:54:48','cbe860cc-9c7b-4ad6-96df-9b1154527183'),(394,12,'dc0c1258-e02f-44f7-bad5-39631dafb824','2026-02-09 05:19:35','2026-02-01 20:19:35',0,NULL,NULL),(395,15,'f1b7e52e-d454-4c72-95d4-c46f8cc5b393','2026-02-09 05:47:39','2026-02-01 20:47:39',0,NULL,NULL),(396,15,'273c418c-c8c6-40cd-88c0-2dc193add667','2026-02-09 05:47:55','2026-02-01 20:47:55',1,'2026-02-02 06:36:01','79266fb9-2a97-4880-9104-3ef6de6b999c'),(397,12,'d308ef34-0ef5-407d-8e46-5ba1e16c388a','2026-02-09 05:54:48','2026-02-01 20:54:48',1,'2026-02-02 06:36:32','77acf996-e74e-4d78-9897-3c0c69e3ab36'),(398,12,'cbe860cc-9c7b-4ad6-96df-9b1154527183','2026-02-09 05:54:48','2026-02-01 20:54:48',0,NULL,NULL),(399,15,'79266fb9-2a97-4880-9104-3ef6de6b999c','2026-02-09 06:36:01','2026-02-01 21:36:01',0,NULL,NULL),(400,15,'9dbc596b-b050-4f22-bbdc-ef223cf36ee5','2026-02-09 06:36:01','2026-02-01 21:36:01',1,'2026-02-02 06:59:05','ec16dee2-f785-4a69-aa77-d4e853b0a85c'),(401,12,'77acf996-e74e-4d78-9897-3c0c69e3ab36','2026-02-09 06:36:32','2026-02-01 21:36:32',1,'2026-02-02 07:04:58','61dcf374-771e-43a1-b05f-6480a310cd85'),(402,15,'ec16dee2-f785-4a69-aa77-d4e853b0a85c','2026-02-09 06:59:05','2026-02-01 21:59:05',1,'2026-02-02 07:15:29','4121e907-4171-43cd-be6c-ee12e8ea17a7'),(403,12,'61dcf374-771e-43a1-b05f-6480a310cd85','2026-02-09 07:04:58','2026-02-01 22:04:58',1,'2026-02-02 23:23:39','783e5469-52ef-46cd-8937-e85b189642a2'),(404,15,'4121e907-4171-43cd-be6c-ee12e8ea17a7','2026-02-09 07:15:29','2026-02-01 22:15:29',1,'2026-02-02 23:21:12','592275e9-8a9a-4908-bc8d-95c69721ffb8'),(405,15,'c5c1b1a3-8c50-4967-93af-e155efa7a4b8','2026-02-09 23:21:12','2026-02-02 14:21:12',1,'2026-02-02 23:46:40','dd68daf4-0906-4896-829a-64741a57d7de'),(406,15,'592275e9-8a9a-4908-bc8d-95c69721ffb8','2026-02-09 23:21:12','2026-02-02 14:21:12',0,NULL,NULL),(407,12,'783e5469-52ef-46cd-8937-e85b189642a2','2026-02-09 23:23:39','2026-02-02 14:23:39',1,'2026-02-02 23:46:39','65dcc09a-51f6-4cda-a5a8-c14f2047a136'),(408,12,'65dcc09a-51f6-4cda-a5a8-c14f2047a136','2026-02-09 23:46:39','2026-02-02 14:46:39',1,'2026-02-03 00:18:06','fb9f205c-0db0-4fb1-b16b-6bdc047d2c64'),(409,15,'dd68daf4-0906-4896-829a-64741a57d7de','2026-02-09 23:46:40','2026-02-02 14:46:40',0,NULL,NULL),(410,12,'caebdd51-bf6c-4c6d-a542-30a5a2e442db','2026-02-09 23:46:50','2026-02-02 14:46:50',0,NULL,NULL),(411,15,'55cb55d4-5cc0-48b7-b501-d07892b63b14','2026-02-09 23:47:10','2026-02-02 14:47:10',1,'2026-02-03 00:17:16','86fee346-b725-4a7b-aa9e-c3dc11799957'),(412,15,'bc330640-0020-4bd0-8620-ccc7e3718b85','2026-02-10 00:17:16','2026-02-02 15:17:16',0,NULL,NULL),(413,15,'86fee346-b725-4a7b-aa9e-c3dc11799957','2026-02-10 00:17:16','2026-02-02 15:17:16',1,'2026-02-03 00:39:02','6ecb99a9-2a08-40b7-b855-34c8aba742e8'),(414,12,'fb9f205c-0db0-4fb1-b16b-6bdc047d2c64','2026-02-10 00:18:06','2026-02-02 15:18:06',1,'2026-02-03 00:39:12','7dcb1d79-1aca-4368-8987-7bce27abaa9c'),(415,15,'6ecb99a9-2a08-40b7-b855-34c8aba742e8','2026-02-10 00:39:02','2026-02-02 15:39:02',1,'2026-02-03 00:56:19','96043cd2-d84c-4bbf-8fb6-3a70c5e7d220'),(416,12,'688a61c9-501f-4815-8fc5-57baf4d1cb68','2026-02-10 00:39:12','2026-02-02 15:39:12',1,'2026-02-03 00:57:05','4894f714-ee7f-44c8-b350-f2d21defe4e5'),(417,12,'6e2bd692-b5ce-4afa-9582-aff60573d1ba','2026-02-10 00:39:12','2026-02-02 15:39:12',0,NULL,NULL),(418,12,'7dcb1d79-1aca-4368-8987-7bce27abaa9c','2026-02-10 00:39:12','2026-02-02 15:39:12',0,NULL,NULL),(419,15,'96043cd2-d84c-4bbf-8fb6-3a70c5e7d220','2026-02-10 00:56:19','2026-02-02 15:56:19',1,'2026-02-03 01:11:22','86e50cf6-e646-483b-85fc-bedd22a746bf'),(420,12,'4894f714-ee7f-44c8-b350-f2d21defe4e5','2026-02-10 00:57:05','2026-02-02 15:57:05',1,'2026-02-03 01:26:27','0cc7c9d5-8878-4c98-9abf-37507a9e2d7a'),(421,15,'86e50cf6-e646-483b-85fc-bedd22a746bf','2026-02-10 01:11:22','2026-02-02 16:11:22',1,'2026-02-03 01:26:28','4aa76e38-a7e6-4c43-9c16-28da44ee4fad'),(422,12,'0cc7c9d5-8878-4c98-9abf-37507a9e2d7a','2026-02-10 01:26:27','2026-02-02 16:26:27',1,'2026-02-03 01:42:42','ae5ded69-47b2-440e-a1ac-8ba3338ac332'),(423,12,'ae2fe462-fc12-4580-863e-b15e3fcf6944','2026-02-10 01:26:27','2026-02-02 16:26:27',0,NULL,NULL),(424,15,'4aa76e38-a7e6-4c43-9c16-28da44ee4fad','2026-02-10 01:26:28','2026-02-02 16:26:28',0,NULL,NULL),(425,15,'74a0cd19-2acd-4904-ac67-1b2072e9e837','2026-02-10 01:26:39','2026-02-02 16:26:39',1,'2026-02-03 01:59:05','408f2bf1-79f2-48ac-9ddf-86e305b1ea1d'),(426,12,'ae5ded69-47b2-440e-a1ac-8ba3338ac332','2026-02-10 01:42:42','2026-02-02 16:42:42',1,'2026-02-03 01:59:10','a100feea-a298-4387-a1c7-dbf3132bc86b'),(427,15,'86d0dc8c-4e04-4fe4-8643-5787f2ac707a','2026-02-10 01:59:05','2026-02-02 16:59:05',0,NULL,NULL),(428,15,'408f2bf1-79f2-48ac-9ddf-86e305b1ea1d','2026-02-10 01:59:05','2026-02-02 16:59:05',1,'2026-02-03 02:17:04','da95b820-e52b-4bcf-8d5d-ebd3fd93d99b'),(429,12,'a100feea-a298-4387-a1c7-dbf3132bc86b','2026-02-10 01:59:10','2026-02-02 16:59:10',1,'2026-02-03 02:15:53','b0dcd447-7972-4e2d-a317-46a0ebc7c017'),(430,12,'b0dcd447-7972-4e2d-a317-46a0ebc7c017','2026-02-10 02:15:53','2026-02-02 17:15:53',1,'2026-02-03 03:24:48','a69ad7f5-6924-4efe-bd03-f66581d05294'),(431,15,'da95b820-e52b-4bcf-8d5d-ebd3fd93d99b','2026-02-10 02:17:04','2026-02-02 17:17:04',1,'2026-02-03 03:24:17','e0988752-d3ec-45e5-9117-53b1a49d492d'),(432,15,'e0988752-d3ec-45e5-9117-53b1a49d492d','2026-02-10 03:24:17','2026-02-02 18:24:17',1,'2026-02-03 03:47:46','67610374-2571-444b-9eba-daddfa86837c'),(433,12,'a69ad7f5-6924-4efe-bd03-f66581d05294','2026-02-10 03:24:48','2026-02-02 18:24:48',1,'2026-02-03 03:48:41','f2207c60-d3f9-4112-8d4d-52adeb426169'),(434,12,'f9e0fc92-179e-4e1a-aaee-cbd0281b2a01','2026-02-10 03:24:48','2026-02-02 18:24:48',0,NULL,NULL),(435,15,'48e4983d-3e8c-45b2-9c38-ea5936ace0ef','2026-02-10 03:47:46','2026-02-02 18:47:46',1,'2026-02-03 04:14:10','05a0d79a-3661-437c-bcee-049f10ddcd0d'),(436,15,'67610374-2571-444b-9eba-daddfa86837c','2026-02-10 03:47:46','2026-02-02 18:47:46',0,NULL,NULL),(437,12,'8460db39-0968-4da1-afa8-d15a75506783','2026-02-10 03:48:41','2026-02-02 18:48:41',0,NULL,NULL),(438,12,'04a5eca4-bd43-4433-8034-a6140d9c9ee0','2026-02-10 03:48:41','2026-02-02 18:48:41',0,NULL,NULL),(439,12,'f2207c60-d3f9-4112-8d4d-52adeb426169','2026-02-10 03:48:41','2026-02-02 18:48:41',1,'2026-02-03 04:14:06','fc9f6bf9-cd6c-4046-9478-68301e761095'),(440,12,'fc9f6bf9-cd6c-4046-9478-68301e761095','2026-02-10 04:14:06','2026-02-02 19:14:06',1,'2026-02-03 05:17:37','38eb3f70-b18e-498e-a2bf-26c6092fca06'),(441,15,'05a0d79a-3661-437c-bcee-049f10ddcd0d','2026-02-10 04:14:10','2026-02-02 19:14:10',1,'2026-02-03 05:16:33','75160d28-7af2-483b-a298-b45535357b19'),(442,15,'fb4566a8-5196-4a0a-a5cb-4b0e6d6865bf','2026-02-10 05:16:33','2026-02-02 20:16:33',1,'2026-02-03 05:31:56','cb32b578-e2c3-485e-b1be-4db59debee45'),(443,15,'75160d28-7af2-483b-a298-b45535357b19','2026-02-10 05:16:33','2026-02-02 20:16:33',0,NULL,NULL),(444,12,'38eb3f70-b18e-498e-a2bf-26c6092fca06','2026-02-10 05:17:37','2026-02-02 20:17:37',1,'2026-02-03 05:45:31','70e1b2f8-2f0e-40dd-a8f6-0f9bfe037712'),(445,15,'ad68be65-06f5-4339-99c6-ac6bcaf98467','2026-02-10 05:31:56','2026-02-02 20:31:56',0,NULL,NULL),(446,15,'cb32b578-e2c3-485e-b1be-4db59debee45','2026-02-10 05:31:56','2026-02-02 20:31:56',1,'2026-02-03 05:51:08','80c3293c-6b97-4bf7-b474-0a5251199c1d'),(447,12,'f03eccf0-f674-4ae8-867a-1eef9c3663e5','2026-02-10 05:45:31','2026-02-02 20:45:31',0,NULL,NULL),(448,12,'70e1b2f8-2f0e-40dd-a8f6-0f9bfe037712','2026-02-10 05:45:31','2026-02-02 20:45:31',0,NULL,NULL),(449,15,'80c3293c-6b97-4bf7-b474-0a5251199c1d','2026-02-10 05:51:08','2026-02-02 20:51:08',0,NULL,NULL),(450,15,'89069896-987b-4817-b31b-3f618315806e','2026-02-10 05:51:08','2026-02-02 20:51:08',1,'2026-02-03 06:15:34','97871ce8-9a50-4a89-afb3-c4cb1abae555'),(451,12,'9b92918e-24f4-4bbe-a277-a7b3a542a6f8','2026-02-10 05:51:52','2026-02-02 20:51:52',1,'2026-02-03 06:15:37','c61f6808-c819-4eeb-9906-b9766f3e67f1'),(452,15,'20cc9a06-60be-4a8a-a784-652f992ef152','2026-02-10 06:15:34','2026-02-02 21:15:34',0,NULL,NULL),(453,15,'97871ce8-9a50-4a89-afb3-c4cb1abae555','2026-02-10 06:15:34','2026-02-02 21:15:34',1,'2026-02-05 06:39:14','6831ebd0-51df-41db-8086-b5762b9ed517'),(454,12,'c61f6808-c819-4eeb-9906-b9766f3e67f1','2026-02-10 06:15:37','2026-02-02 21:15:37',0,NULL,NULL),(455,15,'76bbd4d3-fa14-469b-94a6-f952032b6981','2026-02-12 06:39:14','2026-02-04 21:39:14',0,NULL,NULL),(456,15,'6831ebd0-51df-41db-8086-b5762b9ed517','2026-02-12 06:39:14','2026-02-04 21:39:14',1,'2026-02-05 07:28:40','2d4ee6f4-14a0-444e-aab4-8f9625e6320c'),(457,16,'ef70cc3d-d002-419f-b632-71c88054fec6','2026-02-12 07:10:32','2026-02-04 22:10:32',0,NULL,NULL),(458,17,'607a42fe-0710-4f55-803a-edccdbb322fc','2026-02-12 07:12:41','2026-02-04 22:12:41',0,NULL,NULL),(459,18,'244cabcc-05a8-4b1e-b370-b34f623337f3','2026-02-12 07:13:42','2026-02-04 22:13:42',0,NULL,NULL),(460,15,'2d4ee6f4-14a0-444e-aab4-8f9625e6320c','2026-02-12 07:28:40','2026-02-04 22:28:40',1,'2026-02-09 01:55:33','15a0c6e8-a381-4453-940f-6e317479e800'),(461,15,'8d1b9cd0-18ee-47b0-a5d1-91c394312358','2026-02-16 01:55:33','2026-02-08 16:55:33',0,NULL,NULL),(462,15,'15a0c6e8-a381-4453-940f-6e317479e800','2026-02-16 01:55:33','2026-02-08 16:55:33',1,'2026-02-09 04:01:27','9b5e41a4-8237-4eeb-9c30-127adc9da0f7'),(463,15,'9b5e41a4-8237-4eeb-9c30-127adc9da0f7','2026-02-16 04:01:27','2026-02-08 19:01:27',1,'2026-02-09 04:23:14','c6d9411e-daf9-43d3-b6fc-221db4d0105a'),(464,15,'c6d9411e-daf9-43d3-b6fc-221db4d0105a','2026-02-16 04:23:14','2026-02-08 19:23:14',1,'2026-02-09 05:36:43','1465addd-92c1-4bdf-b994-eb7521b1a36d'),(465,15,'1465addd-92c1-4bdf-b994-eb7521b1a36d','2026-02-16 05:36:43','2026-02-08 20:36:43',0,NULL,NULL),(466,15,'2a173d69-683e-4a2e-9ddc-3f34b35ffac7','2026-02-16 05:36:43','2026-02-08 20:36:43',0,NULL,NULL),(467,15,'dfd254e0-c2de-48bb-9183-ea79f8671426','2026-02-16 05:36:43','2026-02-08 20:36:43',1,'2026-02-09 23:41:52','bc4eceb5-1425-4257-96a1-f3a129e08e43'),(468,15,'98d5c2dc-abac-4330-a31b-d6ed245adc3b','2026-02-16 23:41:52','2026-02-09 14:41:52',0,NULL,NULL),(469,15,'bc4eceb5-1425-4257-96a1-f3a129e08e43','2026-02-16 23:41:52','2026-02-09 14:41:52',1,'2026-02-09 23:44:05','61812bc5-176d-43f7-a03c-2fbcfa816dbb'),(470,15,'61812bc5-176d-43f7-a03c-2fbcfa816dbb','2026-02-16 23:44:05','2026-02-09 14:44:05',1,'2026-02-09 23:45:17','1d2f1b4a-bed7-4bcb-81b9-caeb5143a46b'),(471,15,'1d2f1b4a-bed7-4bcb-81b9-caeb5143a46b','2026-02-16 23:45:17','2026-02-09 14:45:17',1,'2026-02-10 00:12:52','e84d8342-6749-4024-84b8-314d3e5121f3'),(472,15,'248483b2-0cd2-4bf5-9ff8-c7fa76dc0ca4','2026-02-17 00:12:52','2026-02-09 15:12:52',0,NULL,NULL),(473,15,'e84d8342-6749-4024-84b8-314d3e5121f3','2026-02-17 00:12:52','2026-02-09 15:12:52',1,'2026-02-10 00:13:22','6fd78de2-523f-4213-8e04-ed9b747a3b0f'),(474,15,'6fd78de2-523f-4213-8e04-ed9b747a3b0f','2026-02-17 00:13:22','2026-02-09 15:13:22',0,NULL,NULL),(475,19,'a0843ee3-249b-41d2-af82-0fa4cb7a93fe','2026-02-17 00:17:14','2026-02-09 15:17:14',1,'2026-02-10 00:18:03','d3eb9ed8-24a5-4e41-9e6e-f2af8de48eab'),(476,19,'d3eb9ed8-24a5-4e41-9e6e-f2af8de48eab','2026-02-17 00:18:03','2026-02-09 15:18:03',1,'2026-02-10 00:25:04','4d9aef65-d3da-4d8b-91e7-dffcb3274b3f'),(477,19,'4d9aef65-d3da-4d8b-91e7-dffcb3274b3f','2026-02-17 00:25:05','2026-02-09 15:25:05',1,'2026-02-10 00:26:04','e9a78d60-c5a2-4868-aa1e-2084af968552'),(478,19,'e9a78d60-c5a2-4868-aa1e-2084af968552','2026-02-17 00:26:04','2026-02-09 15:26:04',1,'2026-02-10 00:36:19','aa2bde4f-ec74-4a20-ab27-130e7d7ab3db'),(479,19,'aa2bde4f-ec74-4a20-ab27-130e7d7ab3db','2026-02-17 00:36:19','2026-02-09 15:36:19',1,'2026-02-10 00:37:07','720929ef-f676-4b11-9909-a8151075b656'),(480,19,'720929ef-f676-4b11-9909-a8151075b656','2026-02-17 00:37:07','2026-02-09 15:37:07',0,NULL,NULL),(481,20,'88f0eb79-8ddc-40da-a3c5-1f55c6cc0391','2026-02-17 00:43:06','2026-02-09 15:43:06',1,'2026-02-10 00:43:30','7df68aee-1c07-4c77-8132-9fc24ff6e287'),(482,20,'7df68aee-1c07-4c77-8132-9fc24ff6e287','2026-02-17 00:43:30','2026-02-09 15:43:30',0,NULL,NULL),(483,20,'bf577dbd-2ac6-43e1-947a-3f0964c00909','2026-02-17 00:43:45','2026-02-09 15:43:45',1,'2026-02-10 00:46:51','c51d627f-ddaf-4473-b5c6-6580f9dd1b1d'),(484,20,'c51d627f-ddaf-4473-b5c6-6580f9dd1b1d','2026-02-17 00:46:51','2026-02-09 15:46:51',1,'2026-02-10 01:06:25','54cd86e2-df2a-41af-abb3-2a6cf7250dbd'),(485,20,'92a9c4bc-538c-47ad-8ca0-22179ab5edff','2026-02-17 01:06:25','2026-02-09 16:06:25',0,NULL,NULL),(486,20,'54cd86e2-df2a-41af-abb3-2a6cf7250dbd','2026-02-17 01:06:25','2026-02-09 16:06:25',1,'2026-02-10 01:06:53','5fa10f96-a6d3-4a72-b722-1dda0c745429'),(487,20,'5fa10f96-a6d3-4a72-b722-1dda0c745429','2026-02-17 01:06:53','2026-02-09 16:06:53',0,NULL,NULL),(488,12,'796ee703-c7be-4ca4-b3d5-b69506430622','2026-02-17 01:12:47','2026-02-09 16:12:47',0,NULL,NULL),(489,21,'b2263ab3-2170-4e93-a80f-02538330828d','2026-02-17 01:18:43','2026-02-09 16:18:43',0,NULL,NULL),(490,21,'8caa44db-361e-44de-ac8f-43ea2b62157b','2026-02-17 01:19:18','2026-02-09 16:19:18',0,NULL,NULL),(491,21,'42b48cc7-9d34-4512-adec-a72e95e1d0c8','2026-02-17 01:22:47','2026-02-09 16:22:47',1,'2026-02-10 01:31:54','bb7e112e-dbd8-4048-9825-e24839930921'),(492,21,'bb7e112e-dbd8-4048-9825-e24839930921','2026-02-17 01:31:54','2026-02-09 16:31:54',1,'2026-02-10 01:32:30','2a6d417d-cbd9-41f4-b368-725196fd98fd'),(493,21,'2a6d417d-cbd9-41f4-b368-725196fd98fd','2026-02-17 01:32:30','2026-02-09 16:32:30',1,'2026-02-10 23:07:45','4df90008-1f68-46e3-9add-b870c149f5cf'),(494,21,'4df90008-1f68-46e3-9add-b870c149f5cf','2026-02-17 23:07:45','2026-02-10 14:07:45',1,'2026-02-11 01:55:55','8a9c2300-12fe-4276-9fb0-eb23c97692fd'),(495,21,'c56c49ae-a476-469b-ad4f-73f657c2e0d9','2026-02-17 23:07:45','2026-02-10 14:07:45',0,NULL,NULL),(496,21,'c7b67cdd-e5f5-4ed5-84c9-70d36878b9bf','2026-02-18 01:55:55','2026-02-10 16:55:55',0,NULL,NULL),(497,21,'8a9c2300-12fe-4276-9fb0-eb23c97692fd','2026-02-18 01:55:55','2026-02-10 16:55:55',0,NULL,NULL),(498,12,'57404f22-c910-481d-a9b4-9604438969b7','2026-02-18 01:56:09','2026-02-10 16:56:09',0,NULL,NULL),(499,12,'c3bcc84f-1aa5-483b-9945-e746e3535fd3','2026-02-18 01:56:19','2026-02-10 16:56:19',0,NULL,NULL),(500,12,'2fd78d71-6338-4335-a4f9-659837f49f5b','2026-02-18 03:32:10','2026-02-10 18:32:10',0,NULL,NULL),(501,22,'7358198b-7cfc-44de-9a4c-bfc0097dbdfd','2026-02-18 04:05:30','2026-02-10 19:05:30',0,NULL,NULL),(502,22,'0588dd21-f396-4cad-90f6-109b2c0db9bd','2026-02-18 04:06:15','2026-02-10 19:06:15',0,NULL,NULL),(503,23,'35ab9f0c-80c5-4220-9b68-abfc23fc2221','2026-02-18 04:11:26','2026-02-10 19:11:26',0,NULL,NULL),(504,22,'476c821a-c5a8-4e6d-98a5-8a6561fed49f','2026-02-18 04:17:04','2026-02-10 19:17:04',1,'2026-02-11 04:38:45','6350efb9-a0e0-450d-bbe9-8a3f0cdd4c04'),(505,22,'6350efb9-a0e0-450d-bbe9-8a3f0cdd4c04','2026-02-18 04:38:45','2026-02-10 19:38:45',1,'2026-02-11 07:47:38','1ba9bd27-7b42-4f4a-94f7-d1e59d3aa2b1'),(506,22,'1ba9bd27-7b42-4f4a-94f7-d1e59d3aa2b1','2026-02-18 07:47:38','2026-02-10 22:47:38',0,NULL,NULL),(507,22,'7f1bd8a4-e81e-40e1-a91b-910cbae81dec','2026-02-18 07:47:38','2026-02-10 22:47:38',1,'2026-02-11 22:58:59','c9f9c044-8838-47ae-ae69-690579006833'),(508,22,'c9f9c044-8838-47ae-ae69-690579006833','2026-02-18 22:58:59','2026-02-11 13:58:59',0,NULL,NULL),(509,22,'0051565b-ba72-408b-9f38-8ba0422df69e','2026-02-18 22:58:59','2026-02-11 13:58:59',0,NULL,NULL),(510,12,'d07b3cce-d7ae-4d00-b960-115e5ee05b18','2026-02-18 22:59:22','2026-02-11 13:59:22',1,'2026-02-11 23:32:15','06f4fa9f-333f-4b15-b1ec-6d538e63ffd9'),(511,22,'0d30ed24-7038-4531-856d-d9fbfe4a42e4','2026-02-18 23:28:12','2026-02-11 14:28:12',1,'2026-02-11 23:46:40','fb14ccbb-a0a4-4e69-8298-12a5d6118563'),(512,12,'06f4fa9f-333f-4b15-b1ec-6d538e63ffd9','2026-02-18 23:32:15','2026-02-11 14:32:15',1,'2026-02-11 23:51:51','2e478db8-9e08-450e-98b8-0da9c84fa74c'),(513,22,'fb14ccbb-a0a4-4e69-8298-12a5d6118563','2026-02-18 23:46:40','2026-02-11 14:46:40',1,'2026-02-12 00:02:15','6c67a574-4b6c-4394-b32f-6248d43090d8'),(514,12,'a26e7976-19bd-44a8-a3d4-1e7f22751777','2026-02-18 23:51:51','2026-02-11 14:51:51',0,NULL,NULL),(515,12,'2e478db8-9e08-450e-98b8-0da9c84fa74c','2026-02-18 23:51:51','2026-02-11 14:51:51',0,NULL,NULL),(516,12,'2f12a5de-787e-4bcc-ae7d-92b817d3b7cd','2026-02-18 23:52:06','2026-02-11 14:52:06',1,'2026-02-12 00:18:35','9c6279a8-2bf7-4e76-9be9-ed0bcdd060da'),(517,22,'6c67a574-4b6c-4394-b32f-6248d43090d8','2026-02-19 00:02:15','2026-02-11 15:02:15',1,'2026-02-12 00:18:41','12e25f06-44c0-477b-81b5-b37bd6430feb'),(518,12,'f9bc471c-97e5-4d5f-8005-2614d2f1ec6c','2026-02-19 00:18:36','2026-02-11 15:18:36',0,NULL,NULL),(519,12,'23881c57-4470-48ae-8f8f-3f48149adecc','2026-02-19 00:18:36','2026-02-11 15:18:36',0,NULL,NULL),(520,12,'9c6279a8-2bf7-4e76-9be9-ed0bcdd060da','2026-02-19 00:18:36','2026-02-11 15:18:36',1,'2026-02-12 00:34:52','c0bfd423-649b-4641-a685-9b224aa37498'),(521,22,'12e25f06-44c0-477b-81b5-b37bd6430feb','2026-02-19 00:18:41','2026-02-11 15:18:41',1,'2026-02-12 00:33:44','b1055104-4938-4193-9c7d-1c9147b67add'),(522,22,'b1055104-4938-4193-9c7d-1c9147b67add','2026-02-19 00:33:44','2026-02-11 15:33:44',1,'2026-02-12 00:48:55','d3a37dcf-e6d2-452c-8991-55ded2fe1a99'),(523,12,'c0bfd423-649b-4641-a685-9b224aa37498','2026-02-19 00:34:52','2026-02-11 15:34:52',1,'2026-02-12 00:51:19','87586093-01c5-4c5e-adbf-60afc7bfb32d'),(524,22,'d3a37dcf-e6d2-452c-8991-55ded2fe1a99','2026-02-19 00:48:55','2026-02-11 15:48:55',1,'2026-02-12 01:04:21','4719e794-90a1-431b-9bf0-6c1e00d25265'),(525,12,'096d4aea-acad-4b12-b389-e58282134ae3','2026-02-19 00:51:19','2026-02-11 15:51:19',1,'2026-02-12 01:07:02','67c48071-900c-4b2f-8d2e-1b4987041460'),(526,12,'87586093-01c5-4c5e-adbf-60afc7bfb32d','2026-02-19 00:51:19','2026-02-11 15:51:19',0,NULL,NULL),(527,22,'4719e794-90a1-431b-9bf0-6c1e00d25265','2026-02-19 01:04:21','2026-02-11 16:04:21',1,'2026-02-12 01:20:46','cf06c898-be9f-4b1b-ad4b-1be34b925918'),(528,12,'67c48071-900c-4b2f-8d2e-1b4987041460','2026-02-19 01:07:02','2026-02-11 16:07:02',0,NULL,NULL),(529,12,'1c6f6780-780f-4eaf-b952-c2e9a7ec6472','2026-02-19 01:08:34','2026-02-11 16:08:34',1,'2026-02-12 01:23:45','b1f6238c-4ea9-42ef-9865-622b272625b7'),(530,22,'cf06c898-be9f-4b1b-ad4b-1be34b925918','2026-02-19 01:20:46','2026-02-11 16:20:46',1,'2026-02-12 01:36:35','42377c87-944b-4dcf-9baa-c8844329d6c1'),(531,12,'b1f6238c-4ea9-42ef-9865-622b272625b7','2026-02-19 01:23:45','2026-02-11 16:23:45',1,'2026-02-12 01:23:45','d86457b5-546c-4bf9-a90b-70e6575374a1'),(532,12,'d86457b5-546c-4bf9-a90b-70e6575374a1','2026-02-19 01:23:45','2026-02-11 16:23:45',0,NULL,NULL),(533,12,'3f3d994c-1de1-4721-8597-ee5e3a8b2b4f','2026-02-19 01:31:39','2026-02-11 16:31:39',1,'2026-02-12 01:46:43','944d9ed4-27a9-4a0b-b729-2354afa67ecd'),(534,22,'42377c87-944b-4dcf-9baa-c8844329d6c1','2026-02-19 01:36:35','2026-02-11 16:36:35',1,'2026-02-12 01:55:21','0fb25ddf-3980-4d6d-b8ff-b85c65c3db88'),(535,12,'e5e58c08-5191-4034-91a8-3eed1c686159','2026-02-19 01:46:43','2026-02-11 16:46:43',0,NULL,NULL),(536,12,'944d9ed4-27a9-4a0b-b729-2354afa67ecd','2026-02-19 01:46:43','2026-02-11 16:46:43',1,'2026-02-12 02:01:09','0b17f565-c4e0-45ca-92b6-2b015be3234f'),(537,22,'0fb25ddf-3980-4d6d-b8ff-b85c65c3db88','2026-02-19 01:55:21','2026-02-11 16:55:21',1,'2026-02-12 02:13:31','840ab506-5105-41d0-81a9-c4f8239206a1'),(538,12,'0b17f565-c4e0-45ca-92b6-2b015be3234f','2026-02-19 02:01:09','2026-02-11 17:01:09',1,'2026-02-12 02:22:42','c6a80f38-3cbd-41f3-b6d9-04430b534511'),(539,22,'840ab506-5105-41d0-81a9-c4f8239206a1','2026-02-19 02:13:31','2026-02-11 17:13:31',1,'2026-02-12 02:30:21','49bfaa0d-f3f3-4344-8ee9-cb4034ab3b8f'),(540,22,'e3310e13-8285-414b-bc8a-e39f5e90050d','2026-02-19 02:13:31','2026-02-11 17:13:31',0,NULL,NULL),(541,12,'c6a80f38-3cbd-41f3-b6d9-04430b534511','2026-02-19 02:22:42','2026-02-11 17:22:42',1,'2026-02-12 03:48:06','b2e6d8c0-32d7-4245-b2ea-0c61d29595ec'),(542,22,'49bfaa0d-f3f3-4344-8ee9-cb4034ab3b8f','2026-02-19 02:30:21','2026-02-11 17:30:21',1,'2026-02-12 03:49:09','e89064a2-3ac1-4bf7-99b6-13f6aad7f6fa'),(543,12,'8b7582ca-d68a-4f09-9e2f-15fa86651909','2026-02-19 03:48:06','2026-02-11 18:48:06',0,NULL,NULL),(544,12,'0e8afd00-6833-4f1a-a15d-c352a6de2ecb','2026-02-19 03:48:06','2026-02-11 18:48:06',1,'2026-02-12 04:05:30','af55e70e-2cc7-46da-8441-66dc27a79ba0'),(545,12,'b2e6d8c0-32d7-4245-b2ea-0c61d29595ec','2026-02-19 03:48:06','2026-02-11 18:48:06',0,NULL,NULL),(546,22,'e89064a2-3ac1-4bf7-99b6-13f6aad7f6fa','2026-02-19 03:49:09','2026-02-11 18:49:09',0,NULL,NULL),(547,22,'daa82c87-6c13-48e0-82a9-6db8dcee20d3','2026-02-19 03:49:35','2026-02-11 18:49:35',1,'2026-02-12 04:05:30','2bb4a3fc-0724-43ab-9536-4f6b1af1b54d'),(548,22,'2bb4a3fc-0724-43ab-9536-4f6b1af1b54d','2026-02-19 04:05:30','2026-02-11 19:05:30',0,NULL,NULL),(549,12,'af55e70e-2cc7-46da-8441-66dc27a79ba0','2026-02-19 04:05:31','2026-02-11 19:05:31',0,NULL,NULL),(550,22,'01727018-c6bd-4488-be74-bca48aea8971','2026-02-19 04:05:30','2026-02-11 19:05:30',1,'2026-02-12 04:20:52','fb99216e-0ec4-4998-8eba-f5d73eb6c4cb'),(551,12,'9f315900-296d-4587-9490-917b6d5736aa','2026-02-19 04:08:45','2026-02-11 19:08:45',1,'2026-02-12 04:29:35','caa48d2c-8546-45a9-9947-87b411d5cb3a'),(552,22,'cc51deaf-7945-4edf-bfa2-403922f309eb','2026-02-19 04:20:52','2026-02-11 19:20:52',1,'2026-02-12 04:36:32','df3f153d-5913-4afe-be39-73babf0fd7d1'),(553,12,'ea83442d-c051-4fd7-b7e9-f7dd972e4769','2026-02-19 04:29:35','2026-02-11 19:29:35',1,'2026-02-12 04:50:13','47f81582-4ddc-48b1-8791-88bdc7d67ca4'),(554,12,'e221abb8-f026-43f2-a637-8e0bd6c244c0','2026-02-19 04:29:35','2026-02-11 19:29:35',0,NULL,NULL),(555,12,'3f36c992-36a5-4ecc-83a1-fae38500cb28','2026-02-19 04:29:35','2026-02-11 19:29:35',0,NULL,NULL),(556,22,'d37f5e73-5273-4c79-a7b8-a62567b46f78','2026-02-19 04:36:32','2026-02-11 19:36:32',1,'2026-02-12 04:52:46','a0228fb9-66e1-49b0-9d49-764dc0066692'),(557,12,'4fdf9adc-794a-4243-8017-99957fff4925','2026-02-19 04:50:13','2026-02-11 19:50:13',1,'2026-02-12 05:19:13','b8aacf84-c9db-4a48-8d15-d55848f658a7'),(558,22,'2eac7036-1492-4393-9c2c-238e2f0862e4','2026-02-19 04:52:46','2026-02-11 19:52:46',1,'2026-02-12 05:07:09','3a323ebd-e450-441b-9c05-dd20aac6c40e'),(559,22,'fdb3fa88-c2bc-407b-8ca8-b32e77ec56c6','2026-02-19 05:07:09','2026-02-11 20:07:09',1,'2026-02-12 05:22:43','cb8282e1-8d1b-49c4-b5e5-e7a0c0a63ecc'),(560,12,'b4671a58-08e9-4add-b0c7-9608c6fa1113','2026-02-19 05:19:13','2026-02-11 20:19:13',1,'2026-02-12 05:53:17','029a340c-cf6f-49d3-975b-2a4d1eacb879'),(561,22,'85580362-247d-4e20-b1da-167151a1adb2','2026-02-19 05:22:43','2026-02-11 20:22:43',1,'2026-02-12 05:38:39','64018560-8637-42aa-8f2c-7926f5b0ad4e'),(562,22,'6597aad6-9f9b-4438-9acf-37728f33da6a','2026-02-19 05:38:39','2026-02-11 20:38:39',1,'2026-02-12 05:53:15','ad3f5238-79dc-4665-925e-43620c9d4df3'),(563,22,'6fb6fcd2-8c0b-40a8-900b-a14d94e56b99','2026-02-19 05:53:15','2026-02-11 20:53:15',1,'2026-02-12 06:07:56','0022a692-f027-43f6-8e0e-a427f111ac36'),(564,12,'41c63142-61c9-4a55-a5cd-08df5fb8119b','2026-02-19 05:53:17','2026-02-11 20:53:17',0,NULL,NULL),(565,12,'2071cbdb-011b-4310-a51f-820c0f74d11f','2026-02-19 05:53:54','2026-02-11 20:53:54',1,'2026-02-12 06:10:56','1a51d204-d6d4-4ff7-8e12-b86b96a43eb8'),(566,22,'c47d23ef-8ea1-4e40-868a-04c235b201bd','2026-02-19 06:07:56','2026-02-11 21:07:56',1,'2026-02-12 06:27:57','1945cf51-a892-4a61-8feb-6b56edce7088'),(567,12,'e298c9dd-a835-4cec-82f7-e5608f499025','2026-02-19 06:10:56','2026-02-11 21:10:56',1,'2026-02-12 06:58:53','65d7b943-1331-4b9c-a124-78dc06aea8c5'),(568,22,'a2e77c39-d336-4acc-8cd2-72d275a1a95d','2026-02-19 06:27:57','2026-02-11 21:27:57',1,'2026-02-12 06:52:39','5110f15c-8d68-4303-9f92-7691a42beaff'),(569,22,'21c0d384-494f-4e9c-ba70-eaa88ab59d0c','2026-02-19 06:52:39','2026-02-11 21:52:39',0,NULL,NULL),(570,12,'2e016ae0-dbb7-47f9-90aa-9e844c45964e','2026-02-19 06:58:53','2026-02-11 21:58:53',1,'2026-02-12 07:35:04','50183349-d9cf-4e08-8dcb-c757728da7b0'),(571,12,'5c21932d-5d5a-430a-9d76-9a10ed908834','2026-02-19 06:58:53','2026-02-11 21:58:53',0,NULL,NULL),(572,12,'d555a0b9-b361-49a9-980e-2a1293cbefbc','2026-02-19 06:58:53','2026-02-11 21:58:53',0,NULL,NULL),(573,22,'1304c345-1b86-435f-b019-525291c13b4a','2026-02-19 06:59:23','2026-02-11 21:59:23',1,'2026-02-12 07:18:20','80f35197-8926-48ff-8e6f-5a34a7108bae'),(574,22,'8494c4bb-a89f-4431-b930-cbdc7a472881','2026-02-19 07:18:20','2026-02-11 22:18:20',1,'2026-02-12 07:33:48','ca9246e4-ffb5-4b45-9249-361ce3475431'),(575,22,'05f6d545-b9ab-4303-8a59-9a6695fa98bd','2026-02-19 07:33:49','2026-02-11 22:33:49',1,'2026-02-12 07:50:27','18531bdd-0851-4ed2-b640-285bd0eba88e'),(576,12,'597c5955-e523-4136-be58-48f62c5efa27','2026-02-19 07:35:04','2026-02-11 22:35:04',1,'2026-02-12 07:52:28','e393453a-527e-4b4f-b8e9-9968e8ed2ff0'),(577,22,'7ac13a8c-e556-41d2-8371-054b7821202f','2026-02-19 07:50:27','2026-02-11 22:50:27',1,'2026-02-12 23:51:16','31d5eefc-e215-4a88-b552-09087d4e2522'),(578,12,'7b6b50c1-c209-4cea-9a84-daf0a043d340','2026-02-19 07:52:28','2026-02-11 22:52:28',0,NULL,NULL),(579,12,'cf134a2c-fd1a-42c7-9524-77c0c33273ce','2026-02-19 07:52:32','2026-02-11 22:52:32',1,'2026-02-12 22:54:29','015f7ab2-d52c-40a3-b94b-e695f72a5544'),(580,12,'4833163d-f715-4d69-90dd-3a9fc7cc8803','2026-02-19 22:54:29','2026-02-12 13:54:29',1,'2026-02-12 23:10:57','945a3734-a372-4887-b81e-844a91388e7b'),(581,12,'08e58453-0b05-4cb2-98d3-d0bcdbc1865e','2026-02-19 23:00:18','2026-02-12 14:00:18',0,NULL,NULL),(582,12,'7e531217-2f26-46f5-ab6d-b6bdd05ce626','2026-02-19 23:10:58','2026-02-12 14:10:58',0,NULL,NULL),(583,12,'f78375df-5aa8-4c64-bc4e-0833e0331618','2026-02-19 23:10:58','2026-02-12 14:10:58',1,'2026-02-12 23:33:26','e62b54fd-f1fd-4b23-90e3-dcc514c87f5e'),(584,12,'b13a169b-c4ed-44e6-9f08-4aa3daab8c3e','2026-02-19 23:10:58','2026-02-12 14:10:58',0,NULL,NULL),(585,12,'74e67d36-173a-4972-9ae9-8d2157ae21ae','2026-02-19 23:33:26','2026-02-12 14:33:26',1,'2026-02-12 23:48:26','5bea1f43-5a9c-4613-8aef-78081b8fa696'),(586,12,'35207827-2fbd-4dde-aafa-9a32c95751c3','2026-02-19 23:48:26','2026-02-12 14:48:26',1,'2026-02-13 00:39:55','fa81f85b-449b-471c-845e-1303045d6d07'),(587,22,'0f093625-5936-4add-b5a8-aaf476b5eb40','2026-02-19 23:51:16','2026-02-12 14:51:16',0,NULL,NULL),(588,22,'6a4e0baf-e68c-4352-8e04-c47555db09f4','2026-02-19 23:55:25','2026-02-12 14:55:25',1,'2026-02-13 00:41:01','d8509c4f-3d62-41ee-a720-a8ead72969e7'),(589,12,'771501df-5b71-400d-98f6-9552704652de','2026-02-20 00:39:55','2026-02-12 15:39:55',1,'2026-02-13 01:21:03','ee9addf1-9055-41ca-bb47-a78197789269'),(590,22,'c4a7838d-017c-4807-b306-ba14dfb2c38c','2026-02-20 00:41:01','2026-02-12 15:41:01',1,'2026-02-13 00:58:15','50550faf-4951-4b6a-aaa8-66fc179e7102'),(591,22,'93e62960-b503-4f97-b4f7-926622efe364','2026-02-20 00:58:15','2026-02-12 15:58:15',1,'2026-02-13 01:15:50','b5d467b1-d4c8-45fc-991e-4e8847b1533a'),(592,22,'69315f0d-6047-4ed8-a8f1-48f601089e9d','2026-02-20 01:15:50','2026-02-12 16:15:50',1,'2026-02-13 01:32:40','dc313206-aab1-4d2c-a121-37ec60b3375f'),(593,12,'9b695726-fa0b-40d5-b5a8-8db7de6dfac0','2026-02-20 01:21:03','2026-02-12 16:21:03',1,'2026-02-13 01:38:30','e8070a76-cc37-4a1a-9cbe-4036e800f5fd'),(594,22,'92d7dc91-952e-4443-b3b5-57dc8a07428f','2026-02-20 01:32:40','2026-02-12 16:32:40',1,'2026-02-13 01:51:21','6f429bf4-eb06-4873-8a48-b805ac17a576'),(595,12,'8abbbb33-de95-40c4-96d1-80c24647dd64','2026-02-20 01:38:30','2026-02-12 16:38:30',1,'2026-02-13 01:53:56','d1a09e4b-fc0b-46c2-9575-ae173a1b6ffe'),(596,22,'e94ee21f-a58f-4ed1-82d8-03a5ef121413','2026-02-20 01:51:21','2026-02-12 16:51:21',1,'2026-02-13 02:08:58','5ae918dd-038c-4854-b689-6c2749630e1c'),(597,12,'5b252823-7fd6-43ad-bec6-1f467293a68f','2026-02-20 01:53:56','2026-02-12 16:53:56',1,'2026-02-13 02:09:52','3dee644d-300e-4e25-b9ab-bc73954c1fa2'),(598,22,'204fda03-be22-4b12-a2a1-66f63c141d62','2026-02-20 02:08:58','2026-02-12 17:08:58',0,NULL,NULL),(599,12,'154e4465-ac8f-4c5a-8b49-4518ef0d6cbc','2026-02-20 02:09:52','2026-02-12 17:09:52',0,NULL,NULL),(600,12,'bb869429-bec1-4d0e-b316-c05774729c98','2026-03-01 23:50:49','2026-02-22 14:50:49',1,'2026-02-23 00:06:39','ece45c52-b46d-487f-bca9-e6856f7bd16b'),(601,22,'7e6b2f63-abe5-414e-8ba1-512030702acb','2026-03-01 23:51:02','2026-02-22 14:51:02',1,'2026-02-23 00:06:36','de337b9d-acd9-4736-8b38-14cf87e79e1b'),(602,22,'1b9c4cb0-a91d-4d60-94d7-e44c9ef27fce','2026-03-02 00:06:36','2026-02-22 15:06:36',1,'2026-02-23 00:21:59','8828ac92-caff-4f6e-9607-0dc131518a24'),(603,12,'f2e5f1c8-2906-4a04-a4d9-b3fecf4b2d95','2026-03-02 00:06:39','2026-02-22 15:06:39',1,'2026-02-23 00:21:59','69936009-4a2e-4f51-90b0-7b51a5745d32'),(604,22,'dd1de23d-d205-4f68-acbb-5b8436e2f96d','2026-03-02 00:21:59','2026-02-22 15:21:59',1,'2026-02-23 00:38:22','61d7c9c8-ccef-4e8f-9edb-4124e49c6724'),(605,12,'f6660e28-4808-4115-bc71-1ff3d160efc9','2026-03-02 00:21:59','2026-02-22 15:21:59',1,'2026-02-23 00:47:21','2ea39bd9-4366-477c-9406-cf1cc6552fbb'),(606,22,'39fd4265-5b29-407e-b48b-94aef45406f3','2026-03-02 00:38:22','2026-02-22 15:38:22',1,'2026-02-23 01:01:26','fd997a9c-d783-445e-ba98-b69af30ad0e6'),(607,12,'7c261578-e7a8-4657-9973-36656ca17872','2026-03-02 00:47:21','2026-02-22 15:47:21',1,'2026-02-23 02:00:16','c6f88383-2eb5-4f03-a9aa-a78e0685c1b1'),(608,22,'e26ea131-c167-4265-a863-c3df44ffd8b7','2026-03-02 01:01:26','2026-02-22 16:01:26',1,'2026-02-23 01:20:01','40ef2084-4dbf-4350-926b-40616ec1ec3b'),(609,22,'1644a7c4-43bc-43f3-8c59-05f9b26cf40e','2026-03-02 01:20:01','2026-02-22 16:20:01',1,'2026-02-23 01:36:03','913d573e-e28f-41da-a7ba-62a1397a9f41'),(610,22,'a6423207-fc10-4848-9f95-295b30084396','2026-03-02 01:36:03','2026-02-22 16:36:03',1,'2026-02-23 01:52:07','695b93c4-c1fa-4400-9e2c-cb634322610a'),(611,22,'d0f19f29-f6e5-4327-9a9c-d70a0894f791','2026-03-02 01:52:07','2026-02-22 16:52:07',1,'2026-02-23 02:12:32','46d12c3d-fe95-4938-833f-64239c2c8ec5'),(612,12,'546b96c5-b64b-49fd-886b-16ac554be346','2026-03-02 02:00:16','2026-02-22 17:00:16',1,'2026-02-23 02:53:18','da8cb12e-9070-413d-b13b-fe4c47c2e2c5'),(613,22,'e0b70ee3-f6dc-44ff-bc46-ad05afe6fcef','2026-03-02 02:12:32','2026-02-22 17:12:32',1,'2026-02-23 06:01:06','0f39da7f-7a90-4b1a-8f27-881d013185e2'),(614,12,'8982da62-1e37-4090-b242-efe687e60221','2026-03-02 02:53:19','2026-02-22 17:53:19',1,'2026-02-23 03:11:43','6a213ced-3164-452c-85b3-02bedda2d2b0'),(615,12,'ac5ca488-8fcd-487d-b644-11d0fc1e8816','2026-03-02 03:11:43','2026-02-22 18:11:43',1,'2026-02-23 03:38:48','b56b9235-6d24-48b0-83eb-881e767f52a3'),(616,12,'b9910513-d1f7-4614-a7b4-103921df434e','2026-03-02 03:38:48','2026-02-22 18:38:48',1,'2026-02-23 05:40:55','bd088c67-c958-4593-a3db-7f26f4dbae50'),(617,12,'f4dd44cd-5208-43f0-a5ea-67723ae6c21b','2026-03-02 05:40:55','2026-02-22 20:40:55',1,'2026-02-23 05:59:05','2c6bef4f-b596-4a3e-b86a-d7058146a6dd'),(618,12,'23053d1c-de6a-4c95-a7c0-afffdea48c09','2026-03-02 05:59:05','2026-02-22 20:59:05',1,'2026-02-23 06:14:21','b6907474-a676-4ef3-ae6a-7a32eacc8838'),(619,22,'74c162ab-e56f-4111-9cad-b94fee695ab9','2026-03-02 06:01:06','2026-02-22 21:01:06',1,'2026-02-23 06:16:06','dc591c99-461e-4e0a-867f-7c93f4a82c2f'),(620,12,'466a60a4-39ac-4802-9c70-1b7fa0e395f7','2026-03-02 06:14:21','2026-02-22 21:14:21',1,'2026-02-23 06:30:33','28326129-2e5f-4cde-b022-c837e6cc4a03'),(621,22,'c650d336-45e2-4e80-97cf-2f5a9925500f','2026-03-02 06:16:06','2026-02-22 21:16:06',1,'2026-02-23 06:36:14','9846e547-4b6b-4a0e-b3a5-e11deb36701f'),(622,12,'20665442-dc2c-4ae3-873e-9b2a0930ea00','2026-03-02 06:30:33','2026-02-22 21:30:33',1,'2026-02-23 07:06:20','34694555-33eb-4d0c-abb2-986378310f65'),(623,22,'94c53f87-b0b6-4300-8690-ce4d042cd026','2026-03-02 06:36:14','2026-02-22 21:36:14',1,'2026-02-23 07:03:11','e9b693a1-a3c5-4728-a14b-3fd3dd53e087'),(624,22,'a6d91308-7eaa-45ab-b11e-9c6014c76a88','2026-03-02 07:03:12','2026-02-22 22:03:12',1,'2026-02-23 07:33:31','8f9191b7-f428-4fc3-84f4-c8028cc4be66'),(625,12,'964ded13-5754-47b6-b902-09d59c61af74','2026-03-02 07:06:20','2026-02-22 22:06:20',1,'2026-02-23 07:25:44','4f0baed1-9340-43f4-afcc-6453bb61e229'),(626,12,'6d8ab5d9-03fe-4124-9f66-58f2a01b667f','2026-03-02 07:25:45','2026-02-22 22:25:45',1,'2026-02-23 07:45:13','2663fec1-d104-48c6-9999-8caaf66e565b'),(627,22,'0c959fd8-ba82-4784-a9e0-1ae936e52002','2026-03-02 07:33:31','2026-02-22 22:33:31',1,'2026-02-23 07:48:55','0b3f9b55-8c96-4f3d-afca-64fa279e2806'),(628,12,'383845bb-86db-4f41-ba28-3877c5c4718d','2026-03-02 07:45:13','2026-02-22 22:45:13',1,'2026-02-23 23:03:45','b1952af1-dc5d-4308-8015-b93ebb3509fa'),(629,22,'3489d409-d1c1-4a00-80c7-903aeae9ec3b','2026-03-02 07:48:55','2026-02-22 22:48:55',1,'2026-02-23 23:17:47','b589fd5f-9f1c-4200-9003-ceb1a0f0364a'),(630,12,'1b289ba2-dc30-4d7f-96ef-df08a101429f','2026-03-02 23:03:45','2026-02-23 14:03:45',1,'2026-02-23 23:20:37','dfc953cd-99b7-4c16-a8a3-3281e99bbd32'),(631,22,'5f356801-47b9-4b4c-b8a7-984a28963adf','2026-03-02 23:17:47','2026-02-23 14:17:47',1,'2026-02-23 23:34:27','88fc6de1-3c21-419e-8e6c-eb1797049415'),(632,12,'69eaed29-e372-434a-82c0-2f7ada30f72d','2026-03-02 23:20:37','2026-02-23 14:20:37',1,'2026-02-23 23:46:53','81c294e6-b3ad-4a7c-a96a-160b5d97a883'),(633,22,'3c37a14d-7df1-4e45-b96f-9c0f4a10cabb','2026-03-02 23:34:27','2026-02-23 14:34:27',1,'2026-02-23 23:57:16','08536129-5542-46b2-a379-5633433fee8c'),(634,12,'6cb6eb86-dd57-4ebe-9ac9-f9e476c15b59','2026-03-02 23:46:53','2026-02-23 14:46:53',1,'2026-02-24 00:39:17','604c8636-3a7b-492e-8527-6aa1f1f291d5'),(635,22,'c1a88205-0de9-4418-96d0-663f37cb8aba','2026-03-02 23:57:16','2026-02-23 14:57:16',1,'2026-02-24 00:28:50','b299bcaa-e0ce-49c9-97e7-b175a4c5074e'),(636,22,'7461b3de-2292-4f5e-a2e0-366bb2b2065c','2026-03-03 00:28:50','2026-02-23 15:28:50',1,'2026-02-24 00:46:52','efcdbf1c-7d67-438f-bb42-2ba927482406'),(637,12,'60702681-3eb5-400c-ba54-a32e30000594','2026-03-03 00:39:17','2026-02-23 15:39:17',1,'2026-02-24 00:54:47','f31210f7-5414-4ab5-932d-1c196ab06a48'),(638,22,'059917ba-4599-466a-ab2f-5f8f1baa4f50','2026-03-03 00:46:52','2026-02-23 15:46:52',1,'2026-02-24 01:03:02','e80bc6ff-9e63-4f4f-ac96-0837d87f80f2'),(639,12,'bc8a7b5c-49db-4d3b-a248-95545e8db1bf','2026-03-03 00:54:47','2026-02-23 15:54:47',0,NULL,NULL),(640,22,'fd181f65-9a87-4787-9e63-80dbb50dca52','2026-03-03 01:03:02','2026-02-23 16:03:02',1,'2026-02-24 06:19:48','af85937a-15e0-43b2-94f4-8b88643e4d69'),(641,12,'92dbb0d7-7b40-4bf9-b2db-b644b7314180','2026-03-03 04:52:45','2026-02-23 19:52:45',1,'2026-02-24 05:13:47','44588c6e-77af-40b5-afc8-fed53ab65c32'),(642,24,'461d056c-7021-488d-acf2-33c5ca093df5','2026-03-03 05:04:13','2026-02-23 20:04:13',0,NULL,NULL),(643,24,'aace6e72-a123-4992-8b06-c00c10e92b23','2026-03-03 05:04:18','2026-02-23 20:04:18',0,NULL,NULL),(644,24,'9ede8299-2317-4e84-9ba1-feb93a9ab7b8','2026-03-03 05:04:27','2026-02-23 20:04:27',0,NULL,NULL),(645,24,'c7dfec37-64f1-4f09-8b57-a4bcbcde39e1','2026-03-03 05:05:04','2026-02-23 20:05:04',0,NULL,NULL),(646,24,'3d6df1c4-d946-493d-b59f-447b87278e5a','2026-03-03 05:05:09','2026-02-23 20:05:09',0,NULL,NULL),(647,24,'ad19ffc4-a14e-49c9-a061-e9bf26855e9b','2026-03-03 05:05:14','2026-02-23 20:05:14',0,NULL,NULL),(648,24,'788d99c4-81d1-4323-b50c-19656589e84c','2026-03-03 05:05:18','2026-02-23 20:05:18',0,NULL,NULL),(649,24,'25f82b26-fc50-4d49-9bb0-6197d3c3cddc','2026-03-03 05:05:23','2026-02-23 20:05:23',0,NULL,NULL),(650,24,'dc6a259b-7b5b-4a66-842a-84e961ead326','2026-03-03 05:05:27','2026-02-23 20:05:27',0,NULL,NULL),(651,24,'4353ffce-677d-4608-864e-391054a57145','2026-03-03 05:05:31','2026-02-23 20:05:31',0,NULL,NULL),(652,24,'70dcc87c-520f-467d-b7c0-5a506dc133ca','2026-03-03 05:06:09','2026-02-23 20:06:09',0,NULL,NULL),(653,24,'cd2fb113-746e-4517-ad4c-d2a71b1ec1be','2026-03-03 05:06:13','2026-02-23 20:06:13',0,NULL,NULL),(654,24,'708e1e96-3e08-4973-9563-875fd3348d0a','2026-03-03 05:06:18','2026-02-23 20:06:18',0,NULL,NULL),(655,24,'a9d3d9a9-d339-4eec-aeb2-5b39de1a3cf6','2026-03-03 05:06:24','2026-02-23 20:06:24',0,NULL,NULL),(656,24,'c9a44cf9-feb9-4a3b-ba76-14385969270f','2026-03-03 05:06:29','2026-02-23 20:06:29',0,NULL,NULL),(657,24,'b4276355-19a3-4889-b58f-474324066810','2026-03-03 05:06:34','2026-02-23 20:06:34',0,NULL,NULL),(658,24,'2b6c39a2-38b6-4589-85e7-f28311f6e1db','2026-03-03 05:06:43','2026-02-23 20:06:43',0,NULL,NULL),(659,24,'73ef0620-abcf-44b5-a15e-8a223f22ae06','2026-03-03 05:06:48','2026-02-23 20:06:48',0,NULL,NULL),(660,24,'0dae277b-fe9d-464a-877c-67b86dd1328f','2026-03-03 05:09:43','2026-02-23 20:09:43',0,NULL,NULL),(661,24,'cbb267d4-2779-4701-90e8-30057534b3ab','2026-03-03 05:09:52','2026-02-23 20:09:52',0,NULL,NULL),(662,24,'aa05fb47-4716-4245-bae0-9c6154e8952d','2026-03-03 05:09:59','2026-02-23 20:09:59',0,NULL,NULL),(663,24,'0b9d89c8-4965-452f-830d-82ac30aecbab','2026-03-03 05:10:10','2026-02-23 20:10:10',0,NULL,NULL),(664,24,'6ed5a3a4-7e46-4b72-b6dc-ef3ac1b98acb','2026-03-03 05:10:51','2026-02-23 20:10:51',0,NULL,NULL),(665,24,'e42062b3-6225-4cb5-a71d-e97126559fd9','2026-03-03 05:11:51','2026-02-23 20:11:51',0,NULL,NULL),(666,24,'d5d18fbd-978e-4b30-8ada-86e7272d861b','2026-03-03 05:13:14','2026-02-23 20:13:14',0,NULL,NULL),(667,12,'4f8711d7-387b-457c-835e-60e1d9e19a57','2026-03-03 05:13:47','2026-02-23 20:13:47',1,'2026-02-24 05:44:10','7570a35f-3ca5-4504-94c2-1c8d303f65c5'),(668,24,'0317e8d3-8fc5-4b8f-97f1-4001b7659d6a','2026-03-03 05:14:14','2026-02-23 20:14:14',0,NULL,NULL),(669,24,'6fe29cf9-2bad-46b7-be27-63910e564f63','2026-03-03 05:14:22','2026-02-23 20:14:22',0,NULL,NULL),(670,25,'d6e117d9-2bc9-46f1-809e-791e05a0f693','2026-03-03 05:18:24','2026-02-23 20:18:24',0,NULL,NULL),(671,26,'16cfbc48-b22a-45e5-8272-c5150cf1a6e8','2026-03-03 05:20:44','2026-02-23 20:20:44',0,NULL,NULL),(672,26,'19b13fbb-5c6f-4391-833b-425e01265b74','2026-03-03 05:21:14','2026-02-23 20:21:14',0,NULL,NULL),(673,12,'e7ad44c1-0307-4733-a4b7-d7335e42ee81','2026-03-03 05:44:10','2026-02-23 20:44:10',1,'2026-02-24 06:01:13','57285820-127c-4c0c-b82f-a03e71e510cc'),(674,12,'3f7bade9-0995-4c07-902a-ee9ffbe402db','2026-03-03 06:01:13','2026-02-23 21:01:13',1,'2026-02-24 06:16:15','9a5e079c-a5b7-4e97-a38a-97ee23f8239b'),(675,12,'45617f20-d37d-4e0a-abc3-d5a9c1753537','2026-03-03 06:16:15','2026-02-23 21:16:15',1,'2026-02-24 06:41:17','d5be158d-434b-4464-b216-703bf0e5114f'),(676,22,'fb9b461e-c820-4084-ba5f-b421a31da378','2026-03-03 06:19:48','2026-02-23 21:19:48',1,'2026-02-24 07:09:09','f8ac0c71-a7bb-4c4c-8df5-3adeb669bcf4'),(677,12,'9fb8d8d8-25dd-4964-9439-c9c7a82286de','2026-03-03 06:41:17','2026-02-23 21:41:17',1,'2026-02-24 06:57:50','af683e9d-a8d0-46db-82fc-4d77bc33b5d6'),(678,12,'415e1432-d698-4e5b-bb0e-43e15890a89e','2026-03-03 06:57:51','2026-02-23 21:57:51',1,'2026-02-24 07:16:47','e8245301-1a29-43ea-9510-24c431d5ca80'),(679,22,'8a381dd3-b9fd-4f60-9fa5-6637af291a38','2026-03-03 07:09:09','2026-02-23 22:09:09',1,'2026-02-24 07:33:06','404b6a25-db0f-404c-baeb-bcf2b347195d'),(680,12,'d505f3e8-1e26-46e7-8126-02284d17805e','2026-03-03 07:16:47','2026-02-23 22:16:47',0,NULL,NULL),(681,27,'4b3fa39b-dde5-4f88-977a-66a92fade8b0','2026-03-03 07:17:39','2026-02-23 22:17:39',1,'2026-02-24 07:34:42','e0f3ee05-3f08-4a5e-9d15-8f6f2a84f2d8'),(682,22,'129af061-a1df-4b7a-8415-e7442772ae05','2026-03-03 07:33:06','2026-02-23 22:33:06',1,'2026-02-24 23:51:17','0354a59b-8066-4560-a59f-05314bbb0802'),(683,27,'264648d6-b7e0-4a02-9cf3-ddc36b6ced22','2026-03-03 07:34:42','2026-02-23 22:34:42',1,'2026-02-24 08:08:38','cd4719ef-a509-4111-b937-ccea50f018d3'),(684,27,'14b62d48-ad13-45e2-8835-542a9ad0c188','2026-03-03 08:08:38','2026-02-23 23:08:38',1,'2026-02-24 10:12:43','69d713ff-f8df-4579-965c-27b620ab7bb5'),(685,27,'a21b2d59-13c0-4fa1-8ee5-e145fee592cf','2026-03-03 10:12:43','2026-02-24 01:12:43',1,'2026-02-24 10:29:08','3ec06369-205e-474c-8fc4-f0e614fbab61'),(686,27,'5f64dcd9-ddf7-48b5-91d0-c928167ed62f','2026-03-03 10:29:08','2026-02-24 01:29:08',1,'2026-02-24 10:44:26','672e29b6-20ae-4a0f-9881-91c19d4f0594'),(687,27,'d85869da-536b-4161-a0bd-3aa8434d522e','2026-03-03 10:44:26','2026-02-24 01:44:26',1,'2026-02-24 23:47:51','5c9c4792-cc29-483f-9158-5f0b9b652e00'),(688,27,'c36abf06-0335-42b9-a1a4-06e582839287','2026-03-03 23:47:51','2026-02-24 14:47:51',1,'2026-02-25 00:06:26','a94345c7-e66b-4d21-82e9-6d4434415233'),(689,22,'ba1afca2-a4c2-48ab-8555-d797411994f4','2026-03-03 23:51:17','2026-02-24 14:51:17',1,'2026-02-25 05:31:07','27e9ca30-a32c-43ba-b5c7-fe4e720c30a1'),(690,27,'94e8dc29-1781-455c-8bfc-a3fcdf9f49b9','2026-03-04 00:06:26','2026-02-24 15:06:26',1,'2026-02-25 00:26:46','27122629-3d46-415c-a36d-c72f7bc98baf'),(691,27,'cc313153-d906-4633-b4e6-7fd8c10872d2','2026-03-04 00:26:46','2026-02-24 15:26:46',1,'2026-02-25 00:49:58','a4dd49a3-79e1-44d2-abc7-6256d4457771'),(692,27,'4a22b891-854d-4811-a47c-a51cca154b84','2026-03-04 00:49:58','2026-02-24 15:49:58',1,'2026-02-25 01:39:47','3b80e56c-26ac-4279-a1aa-47f98bc5356a'),(693,27,'8c9005d8-b3d3-4721-b5bd-f8c34ac42d3c','2026-03-04 01:39:47','2026-02-24 16:39:47',1,'2026-02-25 02:23:13','50cca913-215a-478b-9a42-29dbcf151cce'),(694,27,'f5021c4c-8d07-4da4-8879-d21f2bf6f3e3','2026-03-04 02:23:13','2026-02-24 17:23:13',1,'2026-02-25 03:47:09','fbf960cc-e4e2-430c-ad6c-002d4d5976c8'),(695,27,'b9796b8c-b1d3-4e0b-96de-3664e6cc357c','2026-03-04 03:47:09','2026-02-24 18:47:09',1,'2026-02-25 04:13:30','4fc9dc55-0c1c-4ad7-8d70-a447b2c4dcd4'),(696,27,'fb005942-fe2b-4db3-a45a-6f902085ea10','2026-03-04 04:13:30','2026-02-24 19:13:30',0,NULL,NULL),(697,12,'4d7bd1b0-9d5c-4f66-9f30-50438d07870d','2026-03-04 04:14:20','2026-02-24 19:14:20',1,'2026-02-25 04:33:43','de010608-cfb5-4180-af93-95809a2d794f'),(698,12,'6e67a815-1118-4695-af74-0a9b232f15d3','2026-03-04 04:33:43','2026-02-24 19:33:43',1,'2026-02-25 05:00:33','3dfd0e3a-cd09-4f40-a1ba-8a58c49d486a'),(699,12,'4183953b-b54a-4591-884c-fe98cb347f7b','2026-03-04 05:00:33','2026-02-24 20:00:33',1,'2026-02-25 05:20:31','a1a743f6-1dbc-449e-8f4b-0d92d0c3ed93'),(700,12,'ea248b7b-2a6d-4f21-a69b-15475f3ec162','2026-03-04 05:20:31','2026-02-24 20:20:31',1,'2026-02-25 05:35:56','217fd288-56cb-4281-a330-5bf36b40a501'),(701,22,'42411eff-65b0-481b-9756-fde3146ce3d0','2026-03-04 05:31:07','2026-02-24 20:31:07',1,'2026-02-25 05:51:52','96e24872-341f-4d56-b635-a185c66bb299'),(702,12,'1c33ab74-cfcf-4741-8395-7a8cba18c785','2026-03-04 05:35:56','2026-02-24 20:35:56',1,'2026-02-25 05:51:20','cf1a9b10-4d86-4a8a-a0d3-1365095d463c'),(703,12,'e51b15b1-a854-4051-ab45-656ebdf317c6','2026-03-04 05:51:21','2026-02-24 20:51:21',1,'2026-02-25 07:57:22','86b430f6-c86b-4524-be05-d789daa7d1a2'),(704,22,'85730075-21ef-4518-aaa7-4c647ddb4e60','2026-03-04 05:51:52','2026-02-24 20:51:52',1,'2026-02-25 06:51:18','e961c55a-6ae0-450b-a365-e21389b0eb4a'),(705,22,'db8379dc-eae0-49e6-8b8c-aa3bf6b5b205','2026-03-04 06:51:18','2026-02-24 21:51:18',0,NULL,NULL),(706,12,'76eb4070-f88b-448e-997f-26e17ee6294b','2026-03-04 07:57:22','2026-02-24 22:57:22',0,NULL,NULL),(707,12,'c2e2ad31-d757-40cc-b7d5-56253f2929b0','2026-03-04 23:24:36','2026-02-25 14:24:36',1,'2026-02-25 23:41:55','83a52f83-9113-4533-a8ee-47c9f9620820'),(708,12,'650e3df8-6740-47c3-9b26-6ea05a1caaaa','2026-03-04 23:41:55','2026-02-25 14:41:55',1,'2026-02-25 23:56:19','09a199be-7796-4283-9b7f-1aa9e4f189f1'),(709,12,'73c8547d-b3fe-46b3-a9c0-af6bf0534d43','2026-03-04 23:56:19','2026-02-25 14:56:19',0,NULL,NULL),(710,12,'014dbf89-2a91-4627-8e56-055465917195','2026-03-05 02:01:38','2026-02-25 17:01:38',1,'2026-02-26 07:29:41','a2a42033-397f-4bcd-af2e-ff66cc5fb217'),(711,12,'213dd83a-71e6-4792-8752-ca306a5b851b','2026-03-05 07:29:41','2026-02-25 22:29:41',1,'2026-02-26 07:49:56','7e26a908-5725-4fd8-91f3-87b56cd66aa2'),(712,12,'d4b5b46b-80ce-4a24-9d5d-4268f0d9aa41','2026-03-05 07:49:56','2026-02-25 22:49:56',1,'2026-02-26 23:30:48','3222397d-1eff-4011-864b-178eecb595a6'),(713,12,'08f46342-8932-40ac-9340-e5b0afe2377d','2026-03-05 23:30:48','2026-02-26 14:30:48',1,'2026-02-27 00:06:31','be908fd7-ad07-45fd-9631-10892c71df46'),(714,12,'d3529dc4-ee4b-4eae-be61-b64531618f64','2026-03-06 00:06:31','2026-02-26 15:06:31',1,'2026-02-27 00:27:04','dc97bd56-50db-4b7f-971c-65f837cc0310'),(715,12,'18c3c0d4-dbc3-42ec-840b-cb7903bed3e3','2026-03-06 00:27:05','2026-02-26 15:27:05',1,'2026-02-27 00:43:31','86d87c89-4a45-4a16-aa77-bf65401b00fb'),(716,12,'c2d8c8ba-565c-479f-b66f-189a552a6a49','2026-03-06 00:43:31','2026-02-26 15:43:31',1,'2026-02-27 01:04:08','52d9fe9d-8436-47fd-9305-3ecd5ebf2fa7'),(717,12,'702d17cf-3d1e-4914-abd8-05b7153460f0','2026-03-06 01:04:08','2026-02-26 16:04:08',0,NULL,NULL),(718,15,'3bad822d-9d44-4662-be15-c050c8c7adce','2026-03-10 01:04:39','2026-03-02 16:04:39',1,'2026-03-03 01:55:49','175a7b0c-6d8f-4dad-b2b9-f9734d3d20b7'),(719,15,'3ab4e31d-f67d-4fab-a92d-a7b0a5eb9420','2026-03-10 01:55:50','2026-03-02 16:55:50',0,NULL,NULL),(720,28,'c1e80c60-6da0-41b6-b975-fd9172493028','2026-03-10 03:47:54','2026-03-02 18:47:54',1,'2026-03-03 03:47:54','d1c3b4c3-ea69-4beb-a092-b27655160d86'),(721,28,'8bffd826-cf31-4bff-b190-885373a7eca0','2026-03-10 03:47:54','2026-03-02 18:47:54',1,'2026-03-03 03:47:54',NULL);
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
  `payment_id` bigint unsigned NOT NULL,
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
  CONSTRAINT `fk_refunds_payment` FOREIGN KEY (`payment_id`) REFERENCES `payments` (`id`) ON DELETE RESTRICT,
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
INSERT INTO `reputation_rating_types` VALUES (1,'GENERAL','전체 평가',1,1);
/*!40000 ALTER TABLE `reputation_rating_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `seat_grades`
--

DROP TABLE IF EXISTS `seat_grades`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `seat_grades` (
  `id` int NOT NULL AUTO_INCREMENT,
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '좌석 등급 코드',
  `name_ko` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '한글명',
  `name_en` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '영문명',
  `sort_order` int DEFAULT '0' COMMENT '정렬 순서',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='좌석 등급 마스터';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `seat_grades`
--

LOCK TABLES `seat_grades` WRITE;
/*!40000 ALTER TABLE `seat_grades` DISABLE KEYS */;
INSERT INTO `seat_grades` VALUES (1,'vip','VIP','VIP',1,'2026-01-30 06:28:20','2026-01-30 06:28:20'),(2,'general','일반','General',2,'2026-01-30 06:28:20','2026-01-30 06:28:20'),(3,'reserved','지정석','Reserved',3,'2026-01-30 06:28:20','2026-01-30 06:28:20'),(4,'standing','입장권','Standing',4,'2026-01-30 06:28:20','2026-01-30 06:28:20');
/*!40000 ALTER TABLE `seat_grades` ENABLE KEYS */;
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
INSERT INTO `settlement_statuses` VALUES (1001,'pending','정산 대기',1,1),(1002,'processing','정산 처리중',1,2),(1003,'completed','정산 완료',1,3),(1004,'failed','정산 실패',1,4),(1005,'on_hold','정산 보류',1,5);
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
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='정산 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `settlements`
--

LOCK TABLES `settlements` WRITE;
/*!40000 ALTER TABLE `settlements` DISABLE KEYS */;
INSERT INTO `settlements` VALUES (2,13,12,50000,1750,48250,5,1001,'2026-03-06 10:05:35',NULL,NULL,0,'2026-03-03 01:05:35','2026-03-03 01:05:35'),(3,14,12,80000,2800,77200,5,1003,'2026-03-02 10:05:35',NULL,NULL,0,'2026-03-03 01:05:35','2026-03-03 01:05:35'),(4,15,12,30000,1050,28950,5,1002,'2026-03-05 10:05:35',NULL,NULL,0,'2026-03-03 01:05:35','2026-03-03 01:05:35');
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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 이미지 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket_images`
--

LOCK TABLES `ticket_images` WRITE;
/*!40000 ALTER TABLE `ticket_images` DISABLE KEYS */;
INSERT INTO `ticket_images` VALUES (1,42,'tickets/42/94592dd04b9e4cc3ada10926d5dc5ba1.jpg','2026-02-23 06:02:18'),(2,43,'tickets/43/d68e13b6c30e4a32a768aa83dc02900f.png','2026-02-24 00:56:22'),(3,46,'tickets/46/ac510d4dd4224634b32ad425903712b6.jpg','2026-02-24 07:18:28');
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
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='티켓 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tickets`
--

LOCK TABLES `tickets` WRITE;
/*!40000 ALTER TABLE `tickets` DISABLE KEYS */;
INSERT INTO `tickets` VALUES (1,7,1,'SCH001A',1,'2026-01-28 19:00:00',1,1,'5열',2,0,2,110000,'VIP석 연석 2장 판매합니다',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,1,2,1,'2,4'),(2,7,1,'SCH001A',1,'2026-01-28 19:00:00',1,2,'10열',4,0,4,75000,'R석 4장 일괄 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,2,1,1,'2'),(3,8,1,'SCH001A',1,'2026-01-28 19:00:00',1,3,'15열',3,0,3,50000,'S석 3장 판매합니다',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,3,2,1,'4'),(4,7,1,'SCH001B',1,'2026-01-29 19:00:00',1,1,'3열',2,0,0,115000,'VIP석 연석 (매진)',3,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,1,2,1,'2,4'),(5,8,1,'SCH001B',1,'2026-01-29 19:00:00',1,2,'12열',5,0,5,75000,'R석 5장 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,2,1,1,'2'),(6,7,2,'SCH002A',1,'2026-02-23 18:00:00',1,4,'8열',10,0,8,90000,'VIP 입장권 10장',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,4,2,1,'2'),(7,8,2,'SCH002A',1,'2026-02-23 18:00:00',1,5,'20열',15,0,15,50000,'일반 입장권 15장',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,5,1,1,'4'),(8,9,2,'SCH002B',1,'2026-02-24 18:00:00',1,4,'10열',8,0,5,90000,'VIP 입장권 8장',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,4,2,1,'2'),(9,7,2,'SCH002B',1,'2026-02-24 18:00:00',1,5,'25열',20,0,18,50000,'일반 입장권 20장',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,5,1,1,'4'),(10,7,3,'SCH003A',1,'2026-08-02 17:00:00',2,6,'스탠딩A',30,0,25,66000,'스탠딩 A구역 30매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,6,1,1,'2'),(11,8,3,'SCH003A',1,'2026-08-02 17:00:00',1,7,'5열',10,0,8,82500,'지정석 10매 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,7,2,1,'2,4'),(12,9,3,'SCH003B',1,'2026-08-03 17:00:00',2,6,'스탠딩B',50,0,50,66000,'스탠딩 B구역 50매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,6,1,1,'4'),(13,7,3,'SCH003B',1,'2026-08-03 17:00:00',1,7,'8열',15,0,10,82500,'지정석 15매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,7,2,1,'2'),(14,8,4,'SCH004A',3,'2026-03-14 18:00:00',1,1,'7열',4,0,2,125000,'VIP석 4장 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,1,2,1,'2,4'),(15,9,4,'SCH004A',3,'2026-03-14 18:00:00',1,2,'15열',6,0,6,90000,'R석 6장 일괄',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,2,1,1,'2'),(16,7,4,'SCH004B',3,'2026-03-15 14:00:00',1,3,'20열',8,0,8,70000,'S석 8장 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,3,1,1,'4'),(17,7,6,'SCH006A',1,'2026-10-28 19:00:00',2,6,'스탠딩',50,0,30,90000,'스탠딩 50매 대량 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,6,1,1,'2'),(18,8,6,'SCH006A',1,'2026-10-28 19:00:00',1,1,'3열',3,0,3,150000,'VIP석 3연석 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,1,2,1,'2,4'),(19,9,6,'SCH006B',1,'2026-10-29 19:00:00',2,6,'스탠딩',40,0,35,90000,'스탠딩 40매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,6,1,1,'4'),(20,7,7,'SCH007A',1,'2026-03-14 14:00:00',1,1,'10열',2,0,2,90000,'VIP석 2연석 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,1,2,1,'2,4'),(21,8,7,'SCH007A',1,'2026-03-14 14:00:00',1,2,'18열',5,0,5,65000,'R석 5장 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,2,1,1,'2'),(22,9,7,'SCH007B',1,'2026-03-15 14:00:00',1,3,'25열',10,0,10,45000,'S석 10장 판매',1,'2026-02-12 02:28:14','2026-02-12 02:28:14',NULL,3,1,1,'4'),(23,12,2,'SCH002',1,'2026-02-23 18:00:00',4,4,'5',5,0,5,180000,'2월12일',1,'2026-02-12 02:33:19','2026-02-13 04:40:20',NULL,7,3,1,'2'),(24,12,2,'SCH002C',1,'2026-02-26 18:00:00',4,4,'ㄷ',10,0,10,150000,'222',1,'2026-02-12 03:57:01','2026-02-25 01:05:34',NULL,8,1,1,'1,3'),(30,7,24,'T007_DD_E1_A_24',1,'2026-02-24 20:00:00',NULL,NULL,'A-01',4,1,4,70000,'deadlineDeals D-1 고할인(65%) 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,21,2,1,'2,4'),(31,8,25,'T007_DD_E2_A_25',1,'2026-02-23 19:00:00',NULL,NULL,'B-11',2,1,2,90000,'deadlineDeals D-0 고할인(55%) 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,22,1,1,'2'),(32,9,25,'T007_DD_E2_B_25',1,'2026-02-23 19:00:00',NULL,NULL,'B-12',1,0,1,100000,'deadlineDeals D-0 보조 티켓 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,22,2,1,'4'),(33,10,26,'T007_DD_E3_A_26',1,'2026-02-25 18:00:00',NULL,NULL,'C-21',3,0,3,121000,'deadlineDeals D-2 할인(45%) 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,23,3,1,'2,4'),(34,11,27,'T007_DD_E4_A_27',3,'2026-02-24 17:00:00',NULL,NULL,'D-31',2,1,2,108000,'deadlineDeals D-1 할인(40%) 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,24,1,1,'2'),(35,12,27,'T007_DD_E4_B_27',3,'2026-02-24 17:00:00',NULL,NULL,'D-32',1,0,1,126000,'deadlineDeals D-1 보조 티켓 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,24,2,1,'4'),(36,7,28,'T007_DD_E5_A_28',1,'2026-02-26 20:00:00',NULL,NULL,'E-41',3,1,3,130000,'deadlineDeals D-3 할인(35%) 샘플 A',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,25,3,1,'2,4'),(37,8,28,'T007_DD_E5_B_28',1,'2026-02-26 20:00:00',NULL,NULL,'E-42',2,0,2,130000,'deadlineDeals D-3 할인(35%) 샘플 B',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,25,1,1,'2'),(38,9,28,'T007_DD_E5_C_28',1,'2026-02-26 20:00:00',NULL,NULL,'E-43',2,0,2,130000,'deadlineDeals D-3 할인(35%) 샘플 C',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,25,2,1,'4'),(39,10,29,'T007_DD_E6_A_29',1,'2026-02-23 21:00:00',NULL,NULL,'F-51',2,1,2,119000,'deadlineDeals D-0 할인(30%) 샘플',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,26,1,1,'2,4'),(40,11,30,'T007_DD_E7_A_30',1,'2026-02-27 18:00:00',NULL,NULL,'G-61',1,0,1,90000,'deadlineDeals D-4 제외 검증용',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,27,2,1,'4'),(41,12,31,'T007_DD_E8_A_31',1,'2026-02-25 16:00:00',NULL,NULL,'H-71',2,0,0,100000,'deadlineDeals 매진 제외 검증용',1,'2026-02-23 03:07:05','2026-02-23 03:07:05',NULL,28,1,1,'2'),(42,22,2,'SCH002C',1,'2026-02-26 18:00:00',3,4,'119',5,0,5,180000,'테스트 추가입니다',1,'2026-02-23 06:02:17','2026-02-24 06:16:03',NULL,7,2,1,'1,2'),(43,12,2,'SCH002B',1,'2026-02-25 19:30:00',4,4,'20',5,1,5,90000,'5장 판매시작',1,'2026-02-24 00:56:21','2026-02-24 00:56:21',NULL,10,3,1,'2'),(44,24,1,'SCH001',1,'2026-01-28 19:00:00',1,1,NULL,1,0,1,150000,NULL,1,'2026-02-24 05:06:23','2026-02-24 05:06:23',NULL,1,1,1,NULL),(45,24,1,'SCH001',1,'2026-01-28 19:00:00',1,1,NULL,1,0,1,150000,NULL,1,'2026-02-24 05:06:24','2026-02-24 05:06:24',NULL,1,1,1,NULL),(46,27,2,'SCH002C',1,'2026-02-26 18:00:00',3,4,'15',5,1,5,120000,'5장 12만',1,'2026-02-24 07:18:27','2026-02-24 07:22:43',NULL,9,2,1,'2'),(47,1,1,'SCH001',1,'2026-01-28 19:00:00',1,1,'A-1',1,0,1,200000,'E2E sell flow ticket',5,'2026-03-03 03:47:59','2026-03-03 03:47:59',NULL,1,1,1,NULL);
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
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='거래 방식 마스터';
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
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 항목 테이블 (티켓별 구매 정보)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transaction_items`
--

LOCK TABLES `transaction_items` WRITE;
/*!40000 ALTER TABLE `transaction_items` DISABLE KEYS */;
INSERT INTO `transaction_items` VALUES (13,13,23,3,36000,108000,'2026-02-11 17:34:09'),(14,14,23,2,180000,360000,'2026-02-11 18:53:13'),(15,15,24,1,150000,150000,'2026-02-11 18:57:24'),(16,16,24,3,150000,450000,'2026-02-11 19:12:32'),(17,17,24,3,150000,450000,'2026-02-11 21:00:53'),(18,18,42,2,180000,360000,'2026-02-22 21:08:57'),(19,19,24,7,150000,1050000,'2026-02-22 22:48:40'),(20,20,24,7,150000,1050000,'2026-02-23 15:53:26'),(21,21,46,4,120000,480000,'2026-02-23 22:22:43');
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
INSERT INTO `transaction_statuses` VALUES (1,'reserved','예약중',1,1),(2,'pending_payment','결제대기',1,2),(3,'paid','결제 완료',1,3),(4,'confirmed','구매 확정',1,4),(5,'completed','거래완료',1,5),(6,'cancelled','거래 취소',1,5),(7,'refunded','환불됨',1,7),(8,'pending','거래 대기',1,1),(9,'payment_requested','결제 요청됨',1,2);
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
  `Amount` int DEFAULT NULL COMMENT '총 거래 금액 (TransactionItem의 TotalPrice 합계)',
  PRIMARY KEY (`id`),
  KEY `fk_transactions_confirmed_by` (`confirmed_by_id`),
  KEY `idx_trans_buyer` (`buyer_id`),
  KEY `idx_trans_seller` (`seller_id`),
  KEY `idx_trans_status` (`status_id`),
  KEY `idx_trans_created` (`created_at`),
  KEY `idx_trans_not_deleted` (`deleted_at`),
  KEY `idx_trans_buyer_status` (`buyer_id`,`status_id`),
  KEY `idx_trans_seller_status` (`seller_id`,`status_id`),
  KEY `idx_trans_buyer_created_id` (`buyer_id`,`created_at` DESC,`id` DESC),
  KEY `idx_trans_seller_created_id` (`seller_id`,`created_at` DESC,`id` DESC),
  KEY `idx_trans_status_created` (`status_id`,`created_at` DESC),
  CONSTRAINT `fk_transactions_confirmed_by` FOREIGN KEY (`confirmed_by_id`) REFERENCES `transaction_confirmed_bys` (`id`),
  CONSTRAINT `fk_transactions_status` FOREIGN KEY (`status_id`) REFERENCES `transaction_statuses` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='거래 정보 테이블 (하나의 거래에 여러 티켓 항목 가능)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `transactions`
--

LOCK TABLES `transactions` WRITE;
/*!40000 ALTER TABLE `transactions` DISABLE KEYS */;
INSERT INTO `transactions` VALUES (13,22,12,6,'2026-02-12 02:34:09','2026-02-13 02:34:09',NULL,NULL,NULL,'2026-02-13 02:40:20','2026-02-11 17:34:09',NULL,108000),(14,22,12,6,'2026-02-12 03:53:13','2026-02-13 03:53:13',NULL,NULL,NULL,'2026-02-13 04:40:20','2026-02-11 18:53:13',NULL,360000),(15,22,12,6,'2026-02-12 03:57:24','2026-02-13 03:57:24',NULL,NULL,NULL,'2026-02-12 04:00:09','2026-02-11 18:57:24',NULL,150000),(16,22,12,6,'2026-02-12 04:12:31','2026-02-13 04:12:31',NULL,NULL,NULL,'2026-02-13 04:40:20','2026-02-11 19:12:31',NULL,450000),(17,22,12,4,'2026-02-12 06:00:53','2026-02-13 06:00:53','2026-02-12 07:05:04',NULL,NULL,NULL,'2026-02-11 21:00:53',NULL,450000),(18,12,22,6,'2026-02-23 06:08:57','2026-02-24 06:08:57',NULL,NULL,NULL,'2026-02-24 06:16:04','2026-02-22 21:08:57',NULL,360000),(19,22,12,4,'2026-02-23 07:48:40','2026-02-24 07:48:40','2026-02-23 07:58:16',NULL,NULL,NULL,'2026-02-22 22:48:40',NULL,1050000),(20,22,12,6,'2026-02-24 00:53:26','2026-02-25 00:53:26',NULL,NULL,NULL,'2026-02-25 01:05:35','2026-02-23 15:53:26',NULL,1050000),(21,22,27,3,'2026-02-24 07:22:43','2026-02-25 07:22:43',NULL,NULL,NULL,NULL,'2026-02-23 22:22:43',NULL,480000);
/*!40000 ALTER TABLE `transactions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_balance`
--

DROP TABLE IF EXISTS `user_balance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_balance` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL,
  `available` bigint NOT NULL DEFAULT '0',
  `pending` bigint NOT NULL DEFAULT '0',
  `total_earned` bigint NOT NULL DEFAULT '0',
  `total_withdrawn` bigint NOT NULL DEFAULT '0',
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_user_balance_user_id` (`user_id`),
  KEY `idx_user_balance_user_id` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_balance`
--

LOCK TABLES `user_balance` WRITE;
/*!40000 ALTER TABLE `user_balance` DISABLE KEYS */;
INSERT INTO `user_balance` VALUES (1,12,0,0,0,0,'2026-02-26 07:30:30','2026-02-26 07:30:30'),(2,15,0,0,0,0,'2026-03-03 01:04:39','2026-03-03 01:04:39'),(3,493,0,0,0,0,'2026-03-03 03:47:53','2026-03-03 03:47:53'),(4,497,0,0,0,0,'2026-03-03 03:47:55','2026-03-03 03:47:55');
/*!40000 ALTER TABLE `user_balance` ENABLE KEYS */;
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
) ENGINE=InnoDB AUTO_INCREMENT=49 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 찜 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_favorites`
--

LOCK TABLES `user_favorites` WRITE;
/*!40000 ALTER TABLE `user_favorites` DISABLE KEYS */;
INSERT INTO `user_favorites` VALUES (1,7,1,1,'2025-12-18 02:05:53'),(2,7,1,2,'2025-12-18 02:05:53'),(3,8,1,1,'2025-12-18 02:05:53'),(4,8,1,6,'2025-12-18 02:05:53'),(5,9,1,4,'2025-12-18 02:05:53'),(6,10,1,5,'2025-12-18 02:05:53'),(40,12,2,3,'2026-02-12 23:48:45'),(45,12,1,24,'2026-02-23 03:07:05'),(46,13,1,24,'2026-02-23 03:07:05'),(47,13,1,25,'2026-02-23 03:07:05'),(48,14,1,26,'2026-02-23 03:07:05');
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
  `average_rating` decimal(3,2) DEFAULT NULL,
  `review_count` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`user_id`),
  KEY `idx_user_profile_nickname` (`nickname`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 프로필 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_profile`
--

LOCK TABLES `user_profile` WRITE;
/*!40000 ALTER TABLE `user_profile` DISABLE KEYS */;
INSERT INTO `user_profile` VALUES (7,'티켓마스터','https://picsum.photos/200/200?random=1','안녕하세요! 공연 티켓 거래합니다.',38.5,15,NULL,0),(8,'콘서트러버','https://picsum.photos/200/200?random=2','콘서트를 사랑하는 사람입니다',42,28,NULL,0),(9,'뮤지컬팬','https://picsum.photos/200/200?random=3','뮤지컬 덕후입니다 ^^',36.5,3,NULL,0),(10,'스포츠광','https://picsum.photos/200/200?random=4','야구, 축구 다 좋아해요',45.2,42,NULL,0),(11,'문화생활','https://picsum.photos/200/200?random=5','전시회도 좋아합니다',39.8,18,NULL,0),(12,'test success','profiles/12/6e264a7c09744146814b043d6272d1bd.jpg','자기소개란 테스트',38.5,0,5.00,1),(13,NULL,NULL,NULL,36.5,0,NULL,0),(14,NULL,NULL,NULL,36.5,0,NULL,0),(15,'수정','profiles/15/18b6c07fa2274dd59d1d200d140d7ee1.jpg',NULL,36.5,0,NULL,0),(16,'진지한펭귄',NULL,NULL,36.5,0,NULL,0),(17,'행복한고양이',NULL,NULL,36.5,0,NULL,0),(18,'친절한물개',NULL,NULL,36.5,0,NULL,0),(19,'친절한앵무새',NULL,NULL,36.5,0,NULL,0),(20,'밝은곰',NULL,NULL,36.5,0,NULL,0),(21,'진지한고래',NULL,NULL,36.5,0,NULL,0),(22,'똑똑한강아지',NULL,NULL,36.5,0,NULL,0),(23,'활발한물개','https://lh3.googleusercontent.com/a/ACg8ocJz_ew3gPLhLPC-gvQ4_eBgXqTgxjnnxEeG0Pr-EarX7eTUBCA=s96-c',NULL,36.5,0,NULL,0),(24,'밝은코알라',NULL,NULL,36.5,0,NULL,0),(25,'친절한사자',NULL,NULL,36.5,0,NULL,0),(26,'용감한코끼리',NULL,NULL,36.5,0,NULL,0),(27,'씩씩한사슴',NULL,NULL,36.5,0,NULL,0),(28,'발랄한여우',NULL,NULL,36.5,0,NULL,0),(29,'신비한햄스터',NULL,NULL,36.5,0,NULL,0),(30,'멋진코끼리',NULL,NULL,36.5,0,NULL,0);
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
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_reputation_user` (`user_id`,`created_at`),
  KEY `idx_reputation_reviewer` (`reviewer_id`),
  KEY `idx_reputation_trans` (`transaction_id`),
  KEY `idx_reputation_rating_type_id` (`rating_type_id`),
  CONSTRAINT `fk_user_reputation_rating_type` FOREIGN KEY (`rating_type_id`) REFERENCES `reputation_rating_types` (`id`),
  CONSTRAINT `fk_user_reputation_trans` FOREIGN KEY (`transaction_id`) REFERENCES `transactions` (`id`),
  CONSTRAINT `chk_score` CHECK ((`score` between 1 and 5))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 평판 (리뷰) 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_reputation`
--

LOCK TABLES `user_reputation` WRITE;
/*!40000 ALTER TABLE `user_reputation` DISABLE KEYS */;
INSERT INTO `user_reputation` VALUES (3,12,22,19,1,5,'2026-02-23 15:41:37');
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
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='사용자 기본 정보 테이블';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (7,'user7@example.com','$2a$11$KzrYQ.GE9g.HL71sWBdlYuRYR3iCxXkR2Q./S1rkMVrZkKwMNLvkq','01063937605',1,2,'2025-12-16 07:14:44','2026-01-11 15:38:14',0),(8,'user8@example.com','$2a$11$OhNNpB7gHZUfNylXL.A4l.bdqJfd2f5tEeBLItfdb5IzORpdFEkXm','string',1,2,'2025-12-16 07:19:04',NULL,0),(9,'user9@example.com','$2a$11$.0wEtmpZhxsQx2wr3jPiO.EVjyaJCd6Q9F/7mTBZJQzxTVghE5FOK','01063937605',1,2,'2025-12-16 07:21:54',NULL,0),(10,'user10@example.com','$2a$11$Ly39wSG/2fetq46qFoioXOXVp18G40kYQ/RDGC.EeRq94IM/HK23S','01063937605',1,2,'2025-12-16 07:22:47',NULL,0),(11,'user11@example.com','$2a$11$lUQ1UJ9l73n0VERun/8.s.gRLYDt.7bvudsuupJkEgws2AdLcCx/W','01063937605',1,2,'2025-12-16 07:28:32','2025-12-15 22:28:43',0),(12,'test@test.com','$2a$11$Pq8lUVer3TUoIijmIvMk8exT84TnmlVBGZxxpEMKnNmCIGb4MuMmq','01012345678',1,2,'2026-01-12 00:39:21','2026-02-25 17:01:37',0),(13,'hu@test.com','$2a$11$S.kZpHTadN5m54XtYQQMiusKeiWc8fJpB.Q13fWQ2eiXnLg99yOQW','01012345678',1,2,'2026-01-12 04:10:09',NULL,0),(14,'chan@test.com','$2a$11$jI36SxUcb2ynZ.nQbT1KKeANQukn.pNQ.cQqSGJG9gVsET7lIVrpS','01012345678',1,2,'2026-01-12 04:21:26',NULL,0),(15,'new@new.com','$2a$11$uCpf2ejx3p2TdYoF4np5IOxVXBMI7oCedkvYXiXPA/jshaGTvbvP6','01012345678',1,2,'2026-01-15 04:14:48','2026-03-02 16:04:39',0),(16,'apitest1770275431@test.com','$2a$11$TimV0fdbNucvdmKZ5x6iXOq.S3B6EAXv0FSrZdHJuELlZ/Srk7nm6',NULL,1,2,'2026-02-05 07:10:31','2026-02-04 22:10:32',0),(17,'apitest1770275559@test.com','$2a$11$LLlJprNPf5Z4X.f51kbe/.VIOSRIRdh4rpehGFkurXiFLgkIODpY6',NULL,1,2,'2026-02-05 07:12:40','2026-02-04 22:12:41',0),(18,'apitest1770275620@test.com','$2a$11$.rAKMO1GRavnLPfa3zALP.c.w2GFQqaeLdfVr0lSmKGKPtoGqqN/G',NULL,1,2,'2026-02-05 07:13:41','2026-02-04 22:13:42',0),(19,'qwer@test.com','$2a$11$/z3CORn1fzYp4YWltBKtieK5KiZ.EMTimYLcJUhaQxe2KGrNiTfGC','01012345678',1,2,'2026-02-10 00:16:59','2026-02-09 15:17:14',0),(20,'qqqq@test.com','$2a$11$JNKRjps1oJlef1aG1sTxKuPxmI6s0wj4IPFOx97S//a/REGod4jLS','01012345678',1,2,'2026-02-10 00:42:58','2026-02-09 15:43:45',0),(21,'Seong0210@test.com','$2a$11$FJlN9WNa3dT.cxWhuwfBaODeQ.1NK4/l9P8b4p3tLzNtWRhF4zh2W','01012345678',1,2,'2026-02-10 01:18:24','2026-02-09 16:22:47',0),(22,'kakao_c810e955a4a1f1cc31ab4baa@social.local',NULL,NULL,2,2,'2026-02-11 04:05:29','2026-02-22 14:51:02',0),(23,'dlgustjd9566@gmail.com',NULL,NULL,3,2,'2026-02-11 04:11:26','2026-02-10 19:11:26',0),(24,'qa_test_user@test.com','$2a$11$XVjJGBflpDPHze5Rk1EcW.BzWK/JlxiG7gU9MbBBRiG44i/VZCWbC',NULL,1,2,'2026-02-24 05:04:09','2026-02-23 20:14:22',0),(25,'test@example.com','$2a$11$SDeS4AX7hTtSWeuN97qKLOWwJ33LZaOfXVrvYhFt3uzzH6Q71W.uW',NULL,1,2,'2026-02-24 05:18:16','2026-02-23 20:18:24',0),(26,'task012_1771910443@example.com','$2a$11$ZaSUh7qJJaqPCOVN6nAY1ODJSReHkDgDyLz5Hi0vJVdsoKfzNSbOS','01012345678',1,2,'2026-02-24 05:20:43','2026-02-23 20:21:14',0),(27,'qqqq@qqq.com','$2a$11$b3K5Jp.ZxsWrYwnfjwy0EeXuPTc0Jjt74llJ8UuL4BUBU9qMHeUx6','01012345678',1,2,'2026-02-24 07:17:22','2026-02-23 22:17:39',0),(28,'test_5aa9576bf13e4cdcaa3a0375def006e8@test.com','$2a$11$pr7.NRcPd.rx7iPlCCOqiOFb3kyECllKB.08HK9EBRqIsvm.iqa1.',NULL,1,2,'2026-03-03 03:47:54','2026-03-02 18:47:54',0),(29,'test_1d490035f33949eeab80d1417aaf46b7@test.com','$2a$11$LZkd6nlHYkdXFDAF7Oj5k.cW2vjJuccJx2Id7HU1OnqN7I5F4dnfC',NULL,1,2,'2026-03-03 03:47:54',NULL,0),(30,'test_eecfcc49bbfe4ea1a37da21a50c2168f@test.com','$2a$11$XaipESAQwMXEdyu1d8V08O9LWrIkikHtPgvqz5.izyLAweATJGvSm',NULL,1,2,'2026-03-03 03:47:54',NULL,0);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `withdrawal`
--

DROP TABLE IF EXISTS `withdrawal`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `withdrawal` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `user_id` bigint NOT NULL,
  `bank_account_id` bigint NOT NULL,
  `amount` bigint NOT NULL,
  `fee` bigint NOT NULL DEFAULT '0',
  `net_amount` bigint NOT NULL,
  `status_id` bigint NOT NULL,
  `idempotency_key` varchar(100) DEFAULT NULL,
  `payout_id` varchar(100) DEFAULT NULL,
  `failure_reason` text,
  `retry_count` int DEFAULT '0',
  `requested_at` datetime NOT NULL,
  `processed_at` datetime DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_withdrawal_idempotency_key` (`idempotency_key`),
  KEY `idx_withdrawal_user_id` (`user_id`),
  KEY `idx_withdrawal_bank_account_id` (`bank_account_id`),
  KEY `idx_withdrawal_status_id` (`status_id`),
  CONSTRAINT `fk_withdrawal_bank_account` FOREIGN KEY (`bank_account_id`) REFERENCES `bank_account` (`id`),
  CONSTRAINT `fk_withdrawal_status` FOREIGN KEY (`status_id`) REFERENCES `withdrawal_status` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `withdrawal`
--

LOCK TABLES `withdrawal` WRITE;
/*!40000 ALTER TABLE `withdrawal` DISABLE KEYS */;
/*!40000 ALTER TABLE `withdrawal` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `withdrawal_status`
--

DROP TABLE IF EXISTS `withdrawal_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `withdrawal_status` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `code` varchar(32) NOT NULL,
  `name_ko` varchar(64) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_withdrawal_status_code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `withdrawal_status`
--

LOCK TABLES `withdrawal_status` WRITE;
/*!40000 ALTER TABLE `withdrawal_status` DISABLE KEYS */;
INSERT INTO `withdrawal_status` VALUES (1,'REQUESTED','요청됨'),(2,'PROCESSING','처리중'),(3,'COMPLETED','완료됨'),(4,'FAILED','실패'),(5,'CANCELLED','취소됨');
/*!40000 ALTER TABLE `withdrawal_status` ENABLE KEYS */;
UNLOCK TABLES;
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-03 12:56:09
