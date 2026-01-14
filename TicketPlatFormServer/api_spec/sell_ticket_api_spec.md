# 티켓 판매 API 스펙

## 개요
티켓 판매 기능 구현을 위한 API 명세서입니다.

**Base URL**: `http://localhost:5224/api/sell`

**인증**: 모든 API는 JWT Bearer Token 인증이 필요합니다.

---

## 1. 카테고리 목록 조회

### `GET /api/sell/categories`

**설명**: 판매 가능한 티켓 카테고리 목록을 조회합니다.

**Request**
```
GET /api/sell/categories
Authorization: Bearer {token}
```

**Response** `200 OK`
```json
[
  {
    "categoryId": 1,
    "code": "concert",
    "name": "콘서트",
    "iconUrl": null
  },
  {
    "categoryId": 2,
    "code": "sports",
    "name": "스포츠",
    "iconUrl": null
  },
  {
    "categoryId": 3,
    "code": "musical",
    "name": "뮤지컬/연극",
    "iconUrl": null
  },
  {
    "categoryId": 4,
    "code": "exhibition",
    "name": "전시/행사",
    "iconUrl": null
  }
]
```

---

## 2. 공연 목록 조회 (카테고리별)

### `GET /api/sell/events`

**설명**: 카테고리별 판매 가능한 공연 목록을 페이징하여 조회합니다.

**Request**
```
GET /api/sell/events?categoryId={categoryId}&keyword={keyword}&page={page}&size={size}
Authorization: Bearer {token}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| categoryId | int | O | 카테고리 ID |
| keyword | string | X | 검색 키워드 (제목, 장소명 검색) |
| page | int | X | 페이지 번호 (1부터 시작, default: 1) |
| size | int | X | 페이지 크기 (default: 20) |

**Response** `200 OK`
```json
{
  "events": [
    {
      "eventId": 1,
      "title": "2024 월드 투어 서울",
      "posterImageUrl": "https://...",
      "venueName": "올림픽공원 체조경기장",
      "startAt": "2025-01-15T19:00:00",
      "endAt": "2025-01-15T21:00:00"
    },
    {
      "eventId": 2,
      "title": "Bunnies Camp 2024",
      "posterImageUrl": "https://...",
      "venueName": "고척스카이돔",
      "startAt": "2025-02-10T18:00:00",
      "endAt": "2025-02-10T20:00:00"
    }
  ],
  "totalCount": 18,
  "currentPage": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

## 3. 공연 일정 조회

### `GET /api/sell/events/schedules`

**설명**: 특정 공연의 판매 가능한 일정(날짜/시간)을 조회합니다.

**Request**
```
GET /api/sell/events/schedules?eventId={eventId}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| eventId | int | O | 공연 ID |

**Response** `200 OK`
```json
{
  "schedules": [
    {
      "scheduleId": "SCH001",
      "date": "2025-01-15",
      "time": "19:00",
      "dayOfWeek": "수"
    },
    {
      "scheduleId": "SCH002",
      "date": "2025-02-10",
      "time": "18:00",
      "dayOfWeek": "월"
    }
  ]
}
```

**Error Response** `404 Not Found`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "해당 공연을 찾을 수 없습니다."
}
```

---

## 4. 좌석 옵션 조회

### `GET /api/sell/events/seat-options`

**설명**: 특정 공연의 좌석 위치 옵션을 조회합니다.

**Request**
```
GET /api/sell/events/seat-options?eventId={eventId}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| eventId | int | O | 공연 ID |

**Response** `200 OK`
```json
{
  "locations": [
    {
      "locationId": "LOC_1F",
      "locationName": "1층"
    },
    {
      "locationId": "LOC_2F",
      "locationName": "2층"
    },
    {
      "locationId": "LOC_STANDING",
      "locationName": "스탠딩"
    },
    {
      "locationId": "LOC_VIP",
      "locationName": "VIP석"
    }
  ],
  "allowCustomLocation": true
}
```

**참고**:
- `locations`: 사전 정의된 좌석 위치 옵션 (공연별 + 전역 옵션)
- `allowCustomLocation`: true인 경우 사용자가 직접 위치를 입력할 수 있음

**Error Response** `404 Not Found`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "해당 공연을 찾을 수 없습니다."
}
```

---

## 5. 티켓 판매 등록

### `POST /api/sell/tickets`

**설명**: 새로운 티켓 판매를 등록합니다.

**Request**
```
POST /api/sell/tickets
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| eventId | int | O | 공연 ID |
| scheduleId | string | O | 일정 ID (예: SCH001) |
| locationId | string | X | 좌석 위치 ID (예: LOC_1F) |
| area | string | X | 구역 (예: A구역) |
| row | string | X | 열 (예: 5열) |
| seatInfo | string | O | 좌석 정보 (자유 형식, 예: "1층 A구역 5열 10번") |
| isConsecutive | bool | X | 연석 여부 (default: false) |
| quantity | int | O | 수량 (1~100) |
| price | int | O | 판매 가격 (원, 1 이상) |
| originalPrice | int | O | 정가 (원, 1 이상) |
| description | string | X | 상세 설명 |
| images | File[] | X | 티켓 이미지 (최대 5개, jpg/jpeg/png, 5MB 이하) |

**Validation Rules**:
- `price` ≤ `originalPrice` (판매가는 정가를 초과할 수 없음)
- `quantity`: 1~100 범위
- `seatInfo`: 필수 입력
- `images`: 최대 5개까지 업로드 가능

**Response** `200 OK`
```json
{
  "ticketId": 123,
  "status": "pending_review",
  "message": "티켓 판매 등록이 완료되었습니다. 검수 후 판매가 시작됩니다."
}
```

**Error Response** `400 Bad Request` (가격 초과)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "판매가는 정가를 초과할 수 없습니다."
}
```

**Error Response** `404 Not Found` (공연 없음)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "해당 공연을 찾을 수 없습니다."
}
```

**Error Response** `404 Not Found` (일정 없음)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "해당 일정을 찾을 수 없습니다."
}
```

---

## 6. 내 판매 티켓 목록 조회

### `GET /api/sell/my-tickets`

**설명**: 내가 등록한 판매 티켓 목록을 페이징하여 조회합니다.

**Request**
```
GET /api/sell/my-tickets?status={status}&page={page}&size={size}
Authorization: Bearer {token}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | string | X | 상태 필터 (`pending_review`, `available`, `reserved`, `sold_out`, `expired`, `hidden`, `cancelled`) |
| page | int | X | 페이지 번호 (1부터 시작, default: 1) |
| size | int | X | 페이지 크기 (default: 20) |

**Response** `200 OK`
```json
{
  "tickets": [
    {
      "ticketId": 123,
      "title": "2024 월드 투어 서울 - 2025-01-15",
      "eventDatetime": "2025-01-15T19:00:00",
      "seatInfo": "1층 A구역 5열 10번",
      "quantity": 2,
      "remainingQuantity": 2,
      "price": 150000,
      "originalPrice": 180000,
      "status": "검수대기",
      "createdAt": "2026-01-14T10:30:00",
      "thumbnailUrl": null
    }
  ],
  "totalCount": 1,
  "currentPage": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

**참고**:
- 삭제된 티켓(`deleted_at != null`)은 조회되지 않습니다.
- `status`는 한글명으로 반환됩니다 (예: "검수대기", "판매중", "판매취소")
- 최신 등록순으로 정렬됩니다 (`created_at DESC`)

---

## 7. 티켓 판매 취소

### `DELETE /api/sell/tickets`

**설명**: 등록한 티켓 판매를 취소합니다. 상태가 `cancelled`로 변경됩니다.

**Request**
```
DELETE /api/sell/tickets?ticketId={ticketId}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| ticketId | int | O | 티켓 ID |

**Response** `200 OK`
```json
{
  "ticketId": 123,
  "status": "cancelled",
  "message": "티켓 판매가 취소되었습니다."
}
```

**Error Response** `403 Forbidden` (권한 없음)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "티켓을 취소할 권한이 없습니다."
}
```

**Error Response** `404 Not Found` (티켓 없음)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "티켓을 찾을 수 없습니다."
}
```

**참고**:
- 본인이 등록한 티켓만 취소 가능합니다 (`seller_id` 검증)
- 이미 삭제된 티켓(`deleted_at != null`)은 조회되지 않습니다

---

## 공통 사항

### 인증
모든 API는 JWT Bearer Token 인증이 필요합니다.

**Authorization Header**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 에러 응답

| HTTP Status | Description |
|-------------|-------------|
| 400 | Bad Request - 잘못된 요청 (유효성 검증 실패) |
| 401 | Unauthorized - 인증 필요 (토큰 없음 또는 만료) |
| 403 | Forbidden - 권한 없음 (본인 티켓 아님) |
| 404 | Not Found - 리소스 없음 |
| 500 | Internal Server Error - 서버 오류 |

### 티켓 상태 코드

| Code | Name (KR) | Description |
|------|-----------|-------------|
| `available` | 판매중 | 판매 가능한 상태 |
| `reserved` | 예약중 | 예약된 상태 |
| `sold_out` | 품절 | 판매 완료 |
| `expired` | 만료 | 기간 만료 |
| `hidden` | 숨김 | 숨김 처리 |
| `pending_review` | 검수대기 | 판매 등록 후 검수 대기 중 |
| `cancelled` | 판매취소 | 판매자가 취소함 |

### 페이징

페이징을 지원하는 API는 다음 파라미터를 사용합니다:
- `page`: 페이지 번호 (1부터 시작)
- `size`: 페이지 크기

응답에는 다음 정보가 포함됩니다:
- `totalCount`: 전체 항목 수
- `currentPage`: 현재 페이지 번호
- `pageSize`: 페이지 크기
- `totalPages`: 전체 페이지 수

---

## 데이터베이스 구조

### 주요 테이블
- `ticket_category`: 카테고리 정보
- `events`: 공연/이벤트 정보
- `event_schedules`: 공연 일정 (NEW)
- `seat_locations`: 좌석 위치 옵션 (NEW)
- `tickets`: 티켓 정보 (컬럼 추가: `schedule_id`, `location_id`, `area`, `row`, `is_consecutive`)
- `ticket_statuses`: 티켓 상태 (추가: `pending_review`, `cancelled`)
- `ticket_images`: 티켓 이미지

---

**작성일**: 2026-01-14
**버전**: v2.0
**최종 업데이트**: 실제 구현 기준으로 업데이트 완료
