-- =====================================================
-- 티켓 판매 API를 위한 DB 스키마 변경
-- 작성일: 2026-01-13
-- =====================================================

USE TicketPlatFormDB;

-- =====================================================
-- 1. event_schedules 테이블 생성
-- =====================================================
CREATE TABLE IF NOT EXISTS `event_schedules` (
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

-- =====================================================
-- 2. seat_locations 테이블 생성
-- =====================================================
CREATE TABLE IF NOT EXISTS `seat_locations` (
  `id` varchar(36) NOT NULL COMMENT '위치 ID (예: LOC_1F)',
  `event_id` int DEFAULT NULL COMMENT '공연 FK (NULL이면 전역 사용)',
  `location_name` varchar(100) NOT NULL COMMENT '위치명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `idx_locations_event` (`event_id`),
  CONSTRAINT `fk_locations_event` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='좌석 위치 옵션 테이블';

-- 기본 좌석 위치 데이터 삽입
INSERT INTO `seat_locations` (`id`, `event_id`, `location_name`, `sort_order`) VALUES
('LOC_1F', NULL, '1층', 1),
('LOC_2F', NULL, '2층', 2),
('LOC_3F', NULL, '3층', 3),
('LOC_STANDING', NULL, '스탠딩', 4),
('LOC_VIP', NULL, 'VIP석', 5),
('LOC_R', NULL, 'R석', 6),
('LOC_S', NULL, 'S석', 7),
('LOC_A', NULL, 'A석', 8),
('LOC_B', NULL, 'B석', 9)
ON DUPLICATE KEY UPDATE location_name=VALUES(location_name);

-- =====================================================
-- 3. tickets 테이블에 신규 컬럼 추가
-- =====================================================
ALTER TABLE `tickets`
  ADD COLUMN `schedule_id` varchar(36) DEFAULT NULL COMMENT '일정 FK' AFTER `event_id`,
  ADD COLUMN `location_id` varchar(36) DEFAULT NULL COMMENT '좌석 위치 FK' AFTER `seat_info`,
  ADD COLUMN `area` varchar(50) DEFAULT NULL COMMENT '구역 (예: A구역)' AFTER `location_id`,
  ADD COLUMN `row` varchar(20) DEFAULT NULL COMMENT '열 (예: 5열)' AFTER `area`,
  ADD COLUMN `is_consecutive` tinyint(1) DEFAULT '0' COMMENT '연석 여부' AFTER `quantity`;

-- 인덱스 추가
ALTER TABLE `tickets`
  ADD KEY `idx_tickets_schedule` (`schedule_id`),
  ADD KEY `idx_tickets_location` (`location_id`);

-- =====================================================
-- 4. ticket_statuses에 새로운 상태 추가
-- =====================================================
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`)
VALUES
  (6, 'pending_review', '검수대기', 1, 6),
  (7, 'cancelled', '판매취소', 1, 7)
ON DUPLICATE KEY UPDATE
  code=VALUES(code),
  name_ko=VALUES(name_ko);

-- =====================================================
-- 5. 기존 데이터 백필 (선택적)
-- =====================================================
-- events 테이블의 start_at을 기준으로 event_schedules에 일정 자동 생성
INSERT INTO `event_schedules` (`id`, `event_id`, `schedule_date`, `schedule_time`, `is_active`)
SELECT
  CONCAT('sch', LPAD(e.id, 5, '0')) as id,
  e.id as event_id,
  DATE(e.start_at) as schedule_date,
  TIME(e.start_at) as schedule_time,
  1 as is_active
FROM `events` e
WHERE e.start_at IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM `event_schedules` es WHERE es.event_id = e.id
  );

-- 기존 티켓에 schedule_id 연결 (event_datetime 기준)
UPDATE `tickets` t
INNER JOIN `events` e ON t.event_id = e.id
INNER JOIN `event_schedules` es ON es.event_id = e.id
  AND DATE(t.event_datetime) = es.schedule_date
SET t.schedule_id = es.id
WHERE t.schedule_id IS NULL AND t.event_id IS NOT NULL;

-- =====================================================
-- 완료
-- =====================================================
SELECT 'Schema migration completed successfully!' as status;
