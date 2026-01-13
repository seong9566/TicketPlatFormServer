# 티켓 판매 API 스펙

## 개요
티켓 판매 기능 구현을 위한 API 명세서입니다.

---

## 1. 카테고리 목록 조회

### `GET /api/sell/categories`

**설명**: 판매 가능한 티켓 카테고리 목록을 조회합니다.

**Request**
```
GET /api/sell/categories
Authorization: Bearer {token}
```

**Response**
```json
{
  "statusCode": 200,
  "success": true,
  "data": [
    {
      "categoryId": "CONCERT",
      "categoryName": "콘서트",
      "iconUrl": "https://..."
    },
    {
      "categoryId": "MUSICAL",
      "categoryName": "뮤지컬",
      "iconUrl": "https://..."
    }
  ]
}
```

---

## 2. 공연 목록 조회 (카테고리별)

### `GET /api/sell/events`

**설명**: 카테고리별 판매 가능한 공연 목록을 조회합니다.

**Request**
```
GET /api/sell/events?categoryId={categoryId}&keyword={keyword}&page={page}&size={size}
Authorization: Bearer {token}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| categoryId | String | O | 카테고리 ID |
| keyword | String | X | 검색 키워드 |
| page | Int | X | 페이지 번호 (default: 0) |
| size | Int | X | 페이지 크기 (default: 20) |

**Response**
```json
{
  "statusCode": 200,
  "success": true,
  "data": {
    "content": [
      {
        "eventId": "12345",
        "title": "아이유 콘서트",
        "venue": "올림픽공원 체조경기장",
        "thumbnailUrl": "https://...",
        "startDate": "2026-02-01",
        "endDate": "2026-02-03"
      }
    ],
    "totalPages": 5,
    "totalElements": 100,
    "currentPage": 0
  }
}
```

---

## 3. 공연 일정 조회

### `GET /api/sell/events/{eventId}/schedules`

**설명**: 특정 공연의 판매 가능한 일정(날짜/시간)을 조회합니다.

**Request**
```
GET /api/sell/events/{eventId}/schedules
Authorization: Bearer {token}
```

**Response**
```json
{
  "statusCode": 200,
  "success": true,
  "data": {
    "eventId": "12345",
    "eventTitle": "아이유 콘서트",
    "schedules": [
      {
        "date": "2026-02-01",
        "times": [
          { "scheduleId": "sch001", "time": "14:00" },
          { "scheduleId": "sch002", "time": "19:00" }
        ]
      },
      {
        "date": "2026-02-02",
        "times": [
          { "scheduleId": "sch003", "time": "15:00" }
        ]
      }
    ]
  }
}
```

---

## 4. 좌석 옵션 조회

### `GET /api/sell/events/{eventId}/seat-options`

**설명**: 특정 공연의 좌석 위치 옵션을 조회합니다.

**Request**
```
GET /api/sell/events/{eventId}/seat-options
Authorization: Bearer {token}
```

**Response**
```json
{
  "statusCode": 200,
  "success": true,
  "data": {
    "locations": [
      { "locationId": "LOC_1F", "locationName": "1층" },
      { "locationId": "LOC_2F", "locationName": "2층" },
      { "locationId": "LOC_STANDING", "locationName": "스탠딩" },
      { "locationId": "LOC_VIP", "locationName": "VIP석" }
    ],
    "allowCustomLocation": true
  }
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
| eventId | String | O | 공연 ID |
| scheduleId | String | O | 일정 ID |
| location | String | O | 좌석 위치 |
| area | String | O | 구역 |
| row | String | O | 열 |
| quantity | Int | O | 수량 (1 또는 2 이상) |
| isConsecutive | Boolean | X | 연석 여부 (2매 이상일 때) |
| price | Int | O | 판매 가격 (원) |
| notes | String | X | 특이사항 |
| ticketImage | File | X | 티켓 이미지 (최대 1장) |

**Response (성공)**
```json
{
  "statusCode": 201,
  "success": true,
  "data": {
    "ticketId": "TKT-20260113-001",
    "status": "PENDING_REVIEW",
    "message": "티켓이 등록되었습니다. 검수 후 판매가 시작됩니다."
  }
}
```

**Response (실패 - 가격 초과)**
```json
{
  "statusCode": 400,
  "success": false,
  "error": {
    "code": "PRICE_EXCEEDS_LIMIT",
    "message": "판매 가격은 정가를 초과할 수 없습니다."
  }
}
```

---

## 6. 내 판매 티켓 목록 조회

### `GET /api/sell/my-tickets`

**설명**: 내가 등록한 판매 티켓 목록을 조회합니다.

**Request**
```
GET /api/sell/my-tickets?status={status}&page={page}&size={size}
Authorization: Bearer {token}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | String | X | 상태 필터 (`PENDING_REVIEW`, `ON_SALE`, `SOLD`, `CANCELLED`) |
| page | Int | X | 페이지 번호 |
| size | Int | X | 페이지 크기 |

**Response**
```json
{
  "statusCode": 200,
  "success": true,
  "data": {
    "content": [
      {
        "ticketId": "TKT-20260113-001",
        "eventTitle": "아이유 콘서트",
        "date": "2026-02-01",
        "time": "19:00",
        "location": "1층",
        "area": "A구역",
        "row": "5열",
        "price": 150000,
        "status": "ON_SALE",
        "createdAt": "2026-01-13T10:30:00"
      }
    ],
    "totalPages": 1,
    "totalElements": 1
  }
}
```

---

## 7. 티켓 판매 취소

### `DELETE /api/sell/tickets/{ticketId}`

**설명**: 등록한 티켓 판매를 취소합니다.

**Request**
```
DELETE /api/sell/tickets/{ticketId}
Authorization: Bearer {token}
```

**Response**
```json
{
  "statusCode": 200,
  "success": true,
  "data": {
    "ticketId": "TKT-20260113-001",
    "status": "CANCELLED",
    "message": "판매가 취소되었습니다."
  }
}
```

---

## 공통 에러 응답

| HTTP Status | Error Code | Description |
|-------------|------------|-------------|
| 400 | BAD_REQUEST | 잘못된 요청 |
| 401 | UNAUTHORIZED | 인증 필요 |
| 403 | FORBIDDEN | 권한 없음 |
| 404 | NOT_FOUND | 리소스 없음 |
| 500 | INTERNAL_ERROR | 서버 오류 |

---

**작성일**: 2026-01-13  
**버전**: v1.0
