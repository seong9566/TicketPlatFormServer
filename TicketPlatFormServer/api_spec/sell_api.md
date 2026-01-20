# 티켓 판매 API 명세서 (Sell APIs)

## 개요
티켓 판매와 관련된 모든 기능을 제공하는 API 모음입니다. 판매 가능한 공연 조회, 티켓 등록, 내 판매 내역 관리 등을 포함합니다.

**기본 URL**: `/api/sell`
**인증**: 필수 (`Authorization: Bearer {accessToken}`)
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트 목록

| Method | Endpoint | 설명 |
|--------|----------|------|
| GET | `/api/sell/categories` | 판매 가능한 카테고리 목록 조회 |
| GET | `/api/sell/events` | 판매 가능한 공연 목록 조회 (검색/페이징) |
| GET | `/api/sell/events/schedules` | 특정 공연의 일정 조회 |
| GET | `/api/sell/events/seat-options` | 특정 공연의 좌석 등급/구역 옵션 조회 |
| GET | `/api/sell/events/original-price` | 좌석 정가 조회 |
| GET | `/api/sell/trade-methods` | 거래 방식 목록 조회 |
| GET | `/api/sell/features` | 티켓 특이사항 옵션 조회 |
| POST | `/api/sell/tickets` | 티켓 판매 등록 |
| GET | `/api/sell/my-tickets` | 내 판매 티켓 목록 조회 |
| DELETE | `/api/sell/tickets` | 판매 티켓 취소 |
| GET | `/api/sell/tickets/images/refresh` | 티켓 이미지 URL 재발급 |

---

## 1. 기초 데이터 조회

### 1-1. 판매 카테고리 조회
**GET** `/api/sell/categories`

#### Response
```json
{
  "message": "카테고리 목록 조회 성공",
  "data": [
    { "id": 1, "name": "콘서트" },
    { "id": 2, "name": "뮤지컬" }
  ],
  "statusCode": 200,
  "success": true
}
```

### 1-2. 거래 방식 조회
**GET** `/api/sell/trade-methods`

#### Response
```json
{
  "message": "거래 방식 목록 조회 성공",
  "data": [
    {
      "id": 1,
      "code": "PIN_TRADE",
      "nameKo": "PIN 거래",
      "nameEn": "PIN Transaction",
      "description": "핀번호만 전달"
    },
    {
      "id": 2,
      "code": "DELIVERY",
      "nameKo": "배송 거래",
      "nameEn": "Delivery",
      "description": "실물 티켓 배송"
    }
  ],
  "statusCode": 200,
  "success": true
}
```

### 1-3. 티켓 특이사항 조회
**GET** `/api/sell/features`

#### Response
```json
{
  "message": "특이사항 목록 조회 성공",
  "data": [
    { "id": 1, "name": "단석" },
    { "id": 2, "name": "연석가능" }
  ],
  "statusCode": 200,
  "success": true
}
```

---

## 2. 공연 및 좌석 정보 조회

### 2-1. 공연 목록 조회
**GET** `/api/sell/events`

#### Query Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| categoryId | int | X | 카테고리 필터 (기본: 전체) |
| keyword | string | X | 검색어 (공연명) |
| page | int | X | 페이지 번호 (기본: 1) |
| size | int | X | 페이지 크기 (기본: 20) |

#### Response
```json
{
  "message": "공연 목록 조회 성공",
  "data": {
    "totalCount": 100,
    "events": [
      {
        "id": 10,
        "title": "아이유 콘서트",
        "posterUrl": "https://...",
        "startAt": "2026-05-01T00:00:00",
        "endAt": "2026-05-03T00:00:00",
        "venueName": "잠실주경기장"
      }
    ]
  },
  "statusCode": 200,
  "success": true
}
```

### 2-2. 공연 일정 조회
**GET** `/api/sell/events/schedules`

#### Query Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| eventId | int | O | 공연 ID |

#### Response
```json
{
  "message": "일정 목록 조회 성공",
  "data": {
    "eventId": 10,
    "schedules": [
      {
        "scheduleId": "SCH_001",
        "eventDate": "2026-05-01",
        "eventTime": "19:00",
        "round": 1
      }
    ]
  },
  "statusCode": 200,
  "success": true
}
```

### 2-3. 좌석 옵션 조회
**GET** `/api/sell/events/seat-options`

#### Query Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| eventId | int | O | 공연 ID |

#### Response
```json
{
  "message": "좌석 옵션 조회 성공",
  "data": {
    "grades": [
      { "gradeId": 1, "code": "VIP", "gradeName": "VIP석" }
    ],
    "locations": [
      { "locationId": 1, "name": "1층" }
    ],
    "areas": [
      { "areaId": 1, "name": "A구역" }
    ],
    "allowCustomLocation": true
  },
  "statusCode": 200,
  "success": true
}
```

### 2-4. 정가 조회 (Original Price)
**GET** `/api/sell/events/original-price`

#### Query Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| eventId | int | O | 공연 ID |
| gradeId | int | O | 좌석 등급 ID |
| locationId | int | X | 좌석 위치 ID |
| areaId | int | X | 좌석 구역 ID |

#### Response
```json
{
  "message": "정가 조회 성공",
  "data": 150000,
  "statusCode": 200,
  "success": true
}
```

---

## 3. 티켓 판매 등록 및 관리

### 3-1. 티켓 판매 등록
**POST** `/api/sell/tickets`
**Content-Type**: `multipart/form-data`

#### Form Data Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| EventId | int | O | 공연 ID |
| ScheduleId | string | O | 일정 ID |
| SeatGradeId | int | O | 좌석 등급 ID |
| LocationId | int | X | 좌석 위치 ID |
| AreaId | int | X | 좌석 구역 ID |
| Row | string | X | 좌석 열 |
| IsConsecutive | bool | X | 연석 여부 |
| TradeMethodId | int | O | 거래 방식 ID |
| Price | int | O | 판매 가격 |
| Quantity | int | O | 판매 수량 |
| FeatureIds | int[] | X | 선택된 특이사항 ID 목록 |
| Description | string | X | 판매자 메모 |
| IsAutoPrice | bool | X | 자동 가격 설정 여부 |
| SeatImages | File[] | X | 티켓/좌석 이미지 파일 목록 |

#### Response
```json
{
  "message": "티켓 판매 등록 성공",
  "data": {
    "ticketId": 123,
    "createdAt": "2026-01-20T14:00:00"
  },
  "statusCode": 200,
  "success": true
}
```

### 3-2. 내 판매 티켓 조회
**GET** `/api/sell/my-tickets`

#### Query Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| status | string | X | 상태 필터 (ONSALE, SOLD_OUT 등) |
| page | int | X | 페이지 (기본: 1) |
| size | int | X | 크기 (기본: 20) |

#### Response
```json
{
  "message": "내 판매 티켓 목록 조회 성공",
  "data": {
    "tickets": [
      {
        "ticketId": 123,
        "title": "아이유 콘서트",
        "eventDatetime": "2026-05-01T19:00:00",
        "seatGradeName": "VIP석",
        "price": 150000,
        "status": "ONSALE",
        "thumbnailUrl": "https://..."
      }
    ],
    "totalCount": 1,
    "currentPage": 1,
    "totalPages": 1
  },
  "statusCode": 200,
  "success": true
}
```

### 3-3. 판매 취소
**DELETE** `/api/sell/tickets`

#### Query Parameters
| 파라미터 | 타입 | 필수 | 설명 |
|---|---|---|---|
| ticketId | int | O | 취소할 티켓 ID |

#### Response
```json
{
  "message": "티켓 판매 취소 성공",
  "data": {
    "ticketId": 123,
    "status": "CANCELED"
  },
  "statusCode": 200,
  "success": true
}
```
