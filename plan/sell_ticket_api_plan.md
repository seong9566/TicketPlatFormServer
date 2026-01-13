# 티켓 판매 API 개발 계획

## 개요

티켓 판매 기능을 구현하기 위한 API 개발 계획서입니다.
`sell_ticket_api_spec.md`에 정의된 7개의 엔드포인트를 구현합니다.

---

## 1. 현재 DB/테이블 분석

### 1.1 기존 테이블 현황

| 테이블명 | 설명 | 상태 |
|----------|------|------|
| `ticket_category` | 카테고리 코드 (concert, sports, musical, exhibition) | ✅ 활용 가능 |
| `events` | 공연/이벤트 정보 | ✅ 활용 가능 |
| `tickets` | 티켓 정보 | ⚠️ 스키마 수정 필요 |
| `ticket_statuses` | 티켓 상태 코드 | ⚠️ 상태 추가 필요 |
| `ticket_images` | 티켓 이미지 | ✅ 활용 가능 |

### 1.2 기존 `tickets` 테이블 컬럼

```sql
-- 현재 컬럼
id, seller_id, event_id, category_id, title, event_datetime, seat_info,
quantity, remaining_quantity, price, original_price, description,
status_id, created_at, updated_at, deleted_at, seat_features
```

### 1.3 기존 `ticket_statuses` 데이터

| id | code | name_ko |
|----|------|---------|
| 1 | available | 판매중 |
| 2 | reserved | 예약중 |
| 3 | sold_out | 품절 |
| 4 | expired | 만료 |
| 5 | hidden | 숨김 |

---

## 2. 스키마 변경 계획

### 2.1 신규 테이블: `event_schedules`

API 스펙의 `scheduleId`를 지원하기 위해 공연 일정 테이블 추가 필요.

```sql
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='공연 일정 테이블';
```

### 2.2 신규 테이블: `seat_locations`

좌석 위치 옵션 마스터 테이블 추가.

```sql
CREATE TABLE `seat_locations` (
  `id` varchar(36) NOT NULL COMMENT '위치 ID (예: LOC_1F)',
  `event_id` int DEFAULT NULL COMMENT '공연 FK (NULL이면 전역 사용)',
  `location_name` varchar(100) NOT NULL COMMENT '위치명',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `sort_order` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `idx_locations_event` (`event_id`),
  CONSTRAINT `fk_locations_event` FOREIGN KEY (`event_id`) REFERENCES `events` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='좌석 위치 옵션 테이블';

-- 기본 데이터 삽입
INSERT INTO `seat_locations` (`id`, `event_id`, `location_name`, `sort_order`) VALUES
('LOC_1F', NULL, '1층', 1),
('LOC_2F', NULL, '2층', 2),
('LOC_STANDING', NULL, '스탠딩', 3),
('LOC_VIP', NULL, 'VIP석', 4);
```

### 2.3 `tickets` 테이블 변경

```sql
ALTER TABLE `tickets`
  ADD COLUMN `schedule_id` varchar(36) DEFAULT NULL COMMENT '일정 FK' AFTER `event_id`,
  ADD COLUMN `location_id` varchar(36) DEFAULT NULL COMMENT '좌석 위치 FK' AFTER `seat_info`,
  ADD COLUMN `area` varchar(50) DEFAULT NULL COMMENT '구역 (예: A구역)' AFTER `location_id`,
  ADD COLUMN `row` varchar(20) DEFAULT NULL COMMENT '열 (예: 5열)' AFTER `area`,
  ADD COLUMN `is_consecutive` tinyint(1) DEFAULT '0' COMMENT '연석 여부' AFTER `quantity`,
  ADD KEY `idx_tickets_schedule` (`schedule_id`),
  ADD KEY `idx_tickets_location` (`location_id`);
```

- 기존 데이터 호환을 위해 신규 컬럼은 `NULL` 허용 유지

### 2.4 `ticket_statuses` 데이터 추가

```sql
-- 판매 등록 후 검수 대기
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`)
VALUES (6, 'pending_review', '검수대기', 1, 6);

-- 판매 취소
INSERT INTO `ticket_statuses` (`id`, `code`, `name_ko`, `is_active`, `sort_order`)
VALUES (7, 'cancelled', '판매취소', 1, 7);
```

---

## 3. API 엔드포인트 구현 계획

### 3.1 `GET /api/sell/categories`

**목적**: 판매 가능한 카테고리 목록 조회

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/CategoryRespDto.cs` | 응답 DTO |
| Controller | `Controllers/SellController.cs` | 엔드포인트 |
| Service | `Services/Sell/ISellService.cs`, `SellService.cs` | 비즈니스 로직 |
| Repository | `Repository/Sell/ISellRepository.cs`, `SellRepository.cs` | 데이터 조회 |

**구현 내용**:
- `ticket_category` 테이블에서 `is_active = true` 조건으로 조회
- iconUrl 필드는 추후 추가 또는 하드코딩

---

### 3.2 `GET /api/sell/events`

**목적**: 카테고리별 판매 가능한 공연 목록 조회 (페이징)

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/SellEventListReqDto.cs` | 요청 DTO (categoryId, keyword, page, size) |
| DTO | `DTO/Sell/SellEventListRespDto.cs` | 응답 DTO (페이징 포함) |
| Controller | `Controllers/SellController.cs` | 엔드포인트 추가 |
| Service | `Services/Sell/SellService.cs` | 로직 추가 |
| Repository | `Repository/Sell/SellRepository.cs` | 데이터 조회 |

**구현 내용**:
- `events` 테이블에서 조회
- 카테고리 필터, 키워드 검색, 페이징 지원
- 기존 `IEventRepository` 확장 또는 새로운 메서드 추가

---

### 3.3 `GET /api/sell/events/{eventId}/schedules`

**목적**: 특정 공연의 일정(날짜/시간) 목록 조회

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/EventScheduleRespDto.cs` | 응답 DTO |
| DBModel | `DBModel/EventSchedule.cs` | 엔티티 추가 |
| Controller | `Controllers/SellController.cs` | 엔드포인트 추가 |
| Service | `Services/Sell/SellService.cs` | 로직 추가 |
| Repository | `Repository/Sell/SellRepository.cs` | 데이터 조회 |

**구현 내용**:
- `event_schedules` 테이블에서 이벤트별 일정 조회
- 날짜별로 그룹화하여 반환

---

### 3.4 `GET /api/sell/events/{eventId}/seat-options`

**목적**: 특정 공연의 좌석 위치 옵션 조회

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/SeatOptionRespDto.cs` | 응답 DTO |
| DBModel | `DBModel/SeatLocation.cs` | 엔티티 추가 |
| Controller | `Controllers/SellController.cs` | 엔드포인트 추가 |
| Service | `Services/Sell/SellService.cs` | 로직 추가 |
| Repository | `Repository/Sell/SellRepository.cs` | 데이터 조회 |

**구현 내용**:
- `seat_locations` 테이블에서 조회
- `allowCustomLocation` 플래그는 설정에서 관리

---

### 3.5 `POST /api/sell/tickets`

**목적**: 새로운 티켓 판매 등록

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/CreateSellTicketReqDto.cs` | 요청 DTO (multipart/form-data) |
| DTO | `DTO/Sell/CreateSellTicketRespDto.cs` | 응답 DTO |
| Controller | `Controllers/SellController.cs` | 엔드포인트 추가 |
| Service | `Services/Sell/SellService.cs` | 로직 추가 |
| Repository | `Repository/Sell/SellRepository.cs` | 데이터 저장 |

**구현 내용**:
1. 요청 유효성 검증
   - eventId, scheduleId, location, area, row, quantity, price 필수
   - price ≤ original_price 검증 (PRICE_EXCEEDS_LIMIT 에러)
2. 티켓 레코드 생성 (status = pending_review)
3. 이미지 업로드 시 `ticket_images` 테이블에 저장
4. 응답: `ticketId`, `status`, `message`

---

### 3.6 `GET /api/sell/my-tickets`

**목적**: 내가 등록한 판매 티켓 목록 조회

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/MyTicketListReqDto.cs` | 요청 DTO (status, page, size) |
| DTO | `DTO/Sell/MyTicketListRespDto.cs` | 응답 DTO |
| Controller | `Controllers/SellController.cs` | 엔드포인트 추가 |
| Service | `Services/Sell/SellService.cs` | 로직 추가 |
| Repository | `Repository/Sell/SellRepository.cs` | 데이터 조회 |

**구현 내용**:
- `tickets` 테이블에서 `seller_id = 현재 사용자` 조건으로 조회
- 상태 필터, 페이징 지원
- JWT 토큰에서 userId 추출

---

### 3.7 `DELETE /api/sell/tickets/{ticketId}`

**목적**: 등록한 티켓 판매 취소

| Layer | 파일명 | 설명 |
|-------|--------|------|
| DTO | `DTO/Sell/CancelSellTicketRespDto.cs` | 응답 DTO |
| Controller | `Controllers/SellController.cs` | 엔드포인트 추가 |
| Service | `Services/Sell/SellService.cs` | 로직 추가 |
| Repository | `Repository/Sell/SellRepository.cs` | 데이터 수정 |

**구현 내용**:
1. 티켓 소유권 확인 (seller_id = 현재 사용자)
2. 상태 변경: `status_id = 7 (cancelled)`
3. 응답: `ticketId`, `status`, `message`

---

## 4. 파일 구조

```
TicketPlatFormServer/
├── Controllers/
│   └── SellController.cs                    [NEW]
├── Services/
│   └── Sell/
│       ├── ISellService.cs                  [NEW]
│       └── SellService.cs                   [NEW]
├── Repository/
│   └── Sell/
│       ├── ISellRepository.cs               [NEW]
│       └── SellRepository.cs                [NEW]
├── DTO/
│   └── Sell/
│       ├── CategoryRespDto.cs               [NEW]
│       ├── SellEventListReqDto.cs           [NEW]
│       ├── SellEventListRespDto.cs          [NEW]
│       ├── EventScheduleRespDto.cs          [NEW]
│       ├── SeatOptionRespDto.cs             [NEW]
│       ├── CreateSellTicketReqDto.cs        [NEW]
│       ├── CreateSellTicketRespDto.cs       [NEW]
│       ├── MyTicketListReqDto.cs            [NEW]
│       ├── MyTicketListRespDto.cs           [NEW]
│       └── CancelSellTicketRespDto.cs       [NEW]
├── DBModel/
│   ├── EventSchedule.cs                     [NEW]
│   └── SeatLocation.cs                      [NEW]
└── Migrations/
    └── [Migration files]                    [NEW]
```

---

## 5. 구현 순서

### Phase 1: DB 스키마 변경 (우선순위: 높음)

1. `event_schedules` 테이블 생성
2. `seat_locations` 테이블 생성 및 기본 데이터 삽입
3. `tickets` 테이블 컬럼 추가
4. `ticket_statuses` 상태 추가
5. 기존 데이터 백필(가능한 범위)
6. EF Core 마이그레이션 생성 및 적용
7. DBModel 클래스 스캐폴딩

### Phase 2: 조회 API 구현 (의존성 없음)

1. `GET /api/sell/categories` 구현
2. `GET /api/sell/events` 구현
3. `GET /api/sell/events/{eventId}/schedules` 구현
4. `GET /api/sell/events/{eventId}/seat-options` 구현

### Phase 3: 등록/취소 API 구현

1. `POST /api/sell/tickets` 구현
2. `GET /api/sell/my-tickets` 구현
3. `DELETE /api/sell/tickets/{ticketId}` 구현

---

## 6. 공통 고려사항

### 6.1 인증/인가

- 모든 API는 `[Authorize]` 어트리뷰트 적용
- JWT 토큰에서 userId 추출
- 본인 티켓만 취소 가능하도록 검증

### 6.2 에러 처리

| HTTP Status | Error Code | Description |
|-------------|------------|-------------|
| 400 | BAD_REQUEST | 잘못된 요청 |
| 400 | PRICE_EXCEEDS_LIMIT | 판매 가격 > 정가 |
| 401 | UNAUTHORIZED | 인증 필요 |
| 403 | FORBIDDEN | 권한 없음 (본인 티켓 아님) |
| 404 | NOT_FOUND | 리소스 없음 |
| 500 | INTERNAL_ERROR | 서버 오류 |

### 6.3 파일 업로드

- 기존 `FileUploadService` 활용
- 이미지 저장소: Supabase Storage 사용 예정
- 허용 형식: jpg, jpeg, png
- 최대 크기: 5MB

### 6.4 기존 데이터 호환 (Fallback)

- `schedule_id`가 없는 티켓은 `event_datetime`을 기준으로 스케줄 응답/정렬에 사용
- 좌석 정보는 `seat_info`를 우선 사용하고, `location_id/area/row`는 존재할 때만 노출
- 백필 이후에도 신규 컬럼이 NULL일 수 있으므로 API는 항상 NULL 허용 처리

---

## 7. 검증 계획

### 7.1 단위 테스트

- `SellService` 비즈니스 로직 테스트
- 가격 검증 로직 테스트 (price ≤ original_price)
- 소유권 검증 로직 테스트

### 7.2 통합 테스트

- API 엔드포인트 테스트 (Postman 또는 Swagger)
- DB 트랜잭션 테스트

### 7.3 수동 테스트

- Swagger UI를 통한 전체 플로우 테스트
  1. 카테고리 목록 조회
  2. 공연 목록 조회
  3. 일정 조회
  4. 좌석 옵션 조회
  5. 티켓 등록
  6. 내 티켓 목록 조회
  7. 티켓 삭제

---

## 8. 일정 추정

| Phase | 작업 | 예상 시간 |
|-------|------|----------|
| Phase 1 | DB 스키마 변경 | 2시간 |
| Phase 2 | 조회 API (4개) | 4시간 |
| Phase 3 | 등록/취소 API (3개) | 4시간 |
| 테스트 | 단위/통합 테스트 | 2시간 |
| **총계** | | **12시간** |

---

## 9. 참고 사항

### 9.1 기존 코드 패턴 준수

- Controller → Service → Repository 레이어 구조
- DTO ↔ Entity 변환은 Service 레이어에서 처리
- `AppException`을 사용한 통일된 에러 처리
- XML 주석 필수

### 9.2 네이밍 컨벤션

- Controller: `SellController`
- Service: `ISellService`, `SellService`
- Repository: `ISellRepository`, `SellRepository`
- DTO: `XxxReqDto`, `XxxRespDto`

---

**작성일**: 2026-01-13  
**버전**: v1.0
