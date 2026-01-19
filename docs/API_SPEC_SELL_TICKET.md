# 티켓 판매 등록 API 문서

## 개요

티켓 판매 등록 플로우에 사용되는 API 목록입니다.  
모든 API는 `Authorization: Bearer {token}` 헤더가 필요합니다.

---

## API 목록

| 단계 | 엔드포인트 | 설명 |
|------|-----------|------|
| 1단계 | `GET /api/sell/categories` | 카테고리 목록 조회 |
| 2단계 | `GET /api/sell/events` | 공연 목록 조회 |
| 3단계 | `GET /api/sell/events/schedules` | 공연 일정 조회 |
| 4단계 | `GET /api/sell/events/seat-options` | 좌석 옵션 조회 (등급/위치/구역) |
| 5단계 | `GET /api/sell/events/original-price` | **좌석 정가 조회 (신규)** |
| 6단계 | `GET /api/sell/features` | 특이사항 목록 조회 |
| 7단계 | `GET /api/sell/trade-methods` | **거래 방식 목록 조회 (신규)** |
| 최종 | `POST /api/sell/tickets` | **티켓 판매 등록** |

---

## 1. 카테고리 목록 조회

```
GET /api/sell/categories
```

### Response
```json
{
  "success": true,
  "statusCode": 200,
  "message": "카테고리 목록 조회 성공",
  "data": [
    { "categoryId": 1, "code": "concert", "name": "콘서트", "iconUrl": null },
    { "categoryId": 2, "code": "musical", "name": "뮤지컬/연극", "iconUrl": null }
  ]
}
```

---

## 2. 공연 목록 조회

```
GET /api/sell/events?categoryId={id}&page=1&size=10&keyword={검색어}
```

### Query Parameters
| 파라미터 | 필수 | 설명 |
|---------|------|------|
| categoryId | ✅ | 카테고리 ID |
| page | ❌ | 페이지 번호 (기본: 1) |
| size | ❌ | 페이지 크기 (기본: 10) |
| keyword | ❌ | 검색어 (공연명/장소명) |

### Response
```json
{
  "data": {
    "events": [
      {
        "eventId": 1,
        "title": "2024 월드 투어 서울",
        "posterImageUrl": "https://...",
        "venueName": "올림픽공원 체조경기장",
        "startAt": "2026-01-28",
        "endAt": "2026-01-28"
      }
    ],
    "totalCount": 50,
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

---

## 3. 공연 일정 조회

```
GET /api/sell/events/schedules?eventId={id}
```

### Response
```json
{
  "data": {
    "schedules": [
      { "scheduleId": "sch_001", "date": "2026-01-28", "time": "19:00", "dayOfWeek": "수" },
      { "scheduleId": "sch_002", "date": "2026-01-29", "time": "15:00", "dayOfWeek": "목" }
    ]
  }
}
```

---

## 4. 좌석 옵션 조회

```
GET /api/sell/events/seat-options?eventId={id}
```

### Response
```json
{
  "data": {
    "grades": [
      { "gradeId": 1, "code": "VIP", "gradeName": "VIP" },
      { "gradeId": 2, "code": "GENERAL", "gradeName": "일반" },
      { "gradeId": 3, "code": "RESERVED", "gradeName": "지정석" }
    ],
    "locations": [
      { "locationId": 1, "locationName": "1층" },
      { "locationId": 2, "locationName": "2층" }
    ],
    "areas": [
      { "areaId": 1, "areaName": "A구역" },
      { "areaId": 2, "areaName": "B구역" }
    ],
    "allowCustomLocation": true
  }
}
```

> **Note**: 기존 `originalPrice`는 제거되었으며, 정가는 별도 API(`GET /api/sell/events/original-price`)로 조회합니다.

---

## 5. 좌석 정가 조회 (신규)

```
GET /api/sell/events/original-price?eventId={id}&gradeId={id}&locationId={id}&areaId={id}
```

### Response
```json
{
  "success": true,
  "statusCode": 200,
  "message": "정가 조회 성공",
  "data": 150000
}
```
> **Note**: `data` 필드에 정가(int)가 반환됩니다. 정가가 없으면 `null`이 반환될 수 있습니다.

---

## 6. 특이사항 목록 조회

```
GET /api/sell/features
```

### Response
```json
{
  "data": [
    { "id": 1, "code": "reservation_id", "nameKo": "예매처 ID로 전달" },
    { "id": 2, "code": "on_site_pickup", "nameKo": "현장발권" },
    { "id": 3, "code": "mobile_ticket", "nameKo": "모바일티켓" },
    { "id": 4, "code": "discount_ticket", "nameKo": "할인티켓(증빙필요)" },
    { "id": 5, "code": "id_required", "nameKo": "신분증필요" },
    { "id": 6, "code": "restricted_view", "nameKo": "시야제한석" },
    { "id": 7, "code": "on_site_help", "nameKo": "현장도움" }
  ]
}
```

---

## 7. 거래 방식 목록 조회 (신규)

```
GET /api/sell/trade-methods
```

### Response
```json
{
  "success": true,
  "statusCode": 200,
  "message": "거래 방식 목록 조회 성공",
  "data": [
    { "id": 1, "code": "pin_trade", "nameKo": "PIN거래", "nameEn": "PIN Trade", "description": "예매 PIN 번호를 전달하는 방식입니다" },
    { "id": 2, "code": "delivery", "nameKo": "배송거래", "nameEn": "Delivery", "description": "실물 티켓을 배송하는 방식입니다" },
    { "id": 3, "code": "on_site", "nameKo": "현장거래", "nameEn": "On-site Trade", "description": "현장에서 직접 만나 거래하는 방식입니다" },
    { "id": 4, "code": "other", "nameKo": "기타거래", "nameEn": "Other", "description": "기타 방식의 거래입니다" }
  ]
}
```

---

## 8. 티켓 판매 등록 ⭐

```
POST /api/sell/tickets
Content-Type: multipart/form-data
```

### Request Body

| 필드명 | 타입 | 필수 | 설명 |
|--------|------|------|------|
| eventId | int | ✅ | 공연 ID |
| scheduleId | string | ✅ | 일정 ID |
| seatGradeId | int | ✅ | 좌석 등급 ID |
| locationId | int | ❌ | 좌석 위치 ID |
| areaId | int | ❌ | 좌석 구역 ID |
| row | string | ❌ | 열/입장 정보 (예: "5열") |
| quantity | int | ✅ | 판매 수량 (1~100) |
| isConsecutive | bool | ❌ | 연석 여부 (기본: false) |
| price | int | ✅ | 판매가 (정가 이하만 가능) |
| originalPrice | int | ✅ | 정가 |
| tradeMethodId | int | ✅ | 거래 방식 ID |
| hasTicket | bool | ✅ | 티켓 보유 여부 |
| description | string | ❌ | 거래 설명 (최대 500자) |
| images | File[] | ❌ | 티켓 이미지 (최대 5개) |
| featureIds | int[] | ❌ | 특이사항 ID 목록 (다중 선택) |

### 거래 방식 (tradeMethodId)

| ID | 코드 | 이름 |
|----|------|------|
| 1 | pin_trade | PIN거래 |
| 2 | delivery | 배송거래 |
| 3 | on_site | 현장거래 |
| 4 | other | 기타거래 |

### Request 예시

```bash
curl -X POST "https://api.example.com/api/sell/tickets" \
  -H "Authorization: Bearer {token}" \
  -F "eventId=1" \
  -F "scheduleId=sch_001" \
  -F "seatGradeId=1" \
  -F "locationId=2" \
  -F "areaId=3" \
  -F "row=5열" \
  -F "quantity=2" \
  -F "isConsecutive=true" \
  -F "price=130000" \
  -F "originalPrice=150000" \
  -F "tradeMethodId=1" \
  -F "hasTicket=true" \
  -F "description=직거래 가능, 양도 사유: 일정 변경" \
  -F "featureIds=1" \
  -F "featureIds=3" \
  -F "images=@ticket1.jpg" \
  -F "images=@ticket2.jpg"
```

### Response (성공)
```json
{
  "success": true,
  "statusCode": 200,
  "message": "티켓 판매 등록 성공",
  "data": {
    "ticketId": 123,
    "status": "pending_review",
    "message": "티켓 판매 등록이 완료되었습니다. 검수 후 판매가 시작됩니다.",
    "images": [
      { "imageId": 1, "imageUrl": "https://signed-url...", "expiresAt": "2026-01-19T13:00:00Z" },
      { "imageId": 2, "imageUrl": "https://signed-url...", "expiresAt": "2026-01-19T13:00:00Z" }
    ]
  }
}
```

### Response (에러)
```json
{
  "success": false,
  "statusCode": 400,
  "message": "판매가는 정가를 초과할 수 없습니다."
}
```

---

## 에러 코드

| 상태 코드 | 설명 |
|----------|------|
| 400 | 잘못된 요청 (필수 값 누락, 유효성 검사 실패) |
| 401 | 인증 실패 |
| 404 | 공연/일정을 찾을 수 없음 |
| 500 | 서버 오류 |
