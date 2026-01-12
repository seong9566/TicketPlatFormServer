# 티켓 상세 조회 API 명세서

## 개요
티켓 상세 정보와 찜 여부, 이벤트 정보를 조회하는 API입니다.

**기본 URL**: `/api/tickets`  
**인증**: 선택 (로그인 시 찜 여부 반환)  
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트

### 1. 티켓 상세 조회 (찜 정보 포함)

**Endpoint**: `GET /api/tickets/detail?ticketId={ticketId}&userId={userId}`

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| ticketId | integer | O | 티켓 ID |
| userId | integer | X | 사용자 ID (찜 여부 확인용, 미제공 시 isFavorited는 null) |

**Success Response (로그인 사용자)**:
```json
{
  "message": "티켓 상세 정보 조회 성공",
  "data": {
    "ticketId": 123,
    "ticketTitle": "위키드 VIP석",
    "seatInfo": "1층 5구역 3열",
    "seatType": "VIP석",
    "price": 150000,
    "originalPrice": 200000,
    "seatFeatures": ["연석", "통로석", "시야제한 없음"],
    "description": "급하게 팝니다!",
    "eventTitle": "뮤지컬 위키드",
    "eventDate": "2026.02.15",
    "venueName": "충무아트센터 대극장",
    "eventPosterImageUrl": "https://example.com/poster.jpg",
    "createdAt": "2026-01-01T10:00:00",
    "quantity": 2,
    "remainingQuantity": 2,
    "isSingleTicket": false,
    "ticketImages": [
      "https://example.com/ticket1.jpg",
      "https://example.com/ticket2.jpg"
    ],
    "isFavorited": true,
    "seller": {
      "userId": 10,
      "nickname": "티켓매니아",
      "profileImageUrl": "https://example.com/profile.jpg",
      "mannerTemperature": 42.5,
      "totalTradeCount": 15,
      "responseRate": 95.5,
      "isSecurePayment": true
    }
  },
  "statusCode": 200,
  "success": true
}
```

**Success Response (비로그인 사용자)**:
```json
{
  "message": "티켓 상세 정보 조회 성공",
  "data": {
    "ticketId": 123,
    "ticketTitle": "위키드 VIP석",
    // ... (기타 필드 동일)
    "isFavorited": null,  // userId 미제공 시 null
    "seller": { ... }
  },
  "statusCode": 200,
  "success": true
}
```

**Response Fields** (기존 필드 + 추가):
| 필드 | 타입 | 설명 |
|------|------|------|
| isFavorited | boolean \| null | 찜 여부 (userId 제공 시만 true/false, 미제공 시 null) |
| eventTitle | string \| null | 이벤트 제목 |
| eventDate | string \| null | 공연 날짜 (YYYY.MM.DD) |
| venueName | string \| null | 장소명 |
| eventPosterImageUrl | string \| null | 이벤트 포스터 이미지 URL |

**참고사항**:
- **기존 API와 호환**: userId 파라미터는 선택사항이므로 기존 클라이언트에 영향 없음
- **찜 여부 확인**: userId를 전달하면 해당 사용자의 찜 여부를 확인하여 반환
- **비로그인 상태**: userId를 전달하지 않으면 isFavorited는 null

**Error Responses**:

1. **유효하지 않은 티켓 ID**:
```json
{
  "message": "유효하지 않은 티켓 ID입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

2. **티켓을 찾을 수 없음**:
```json
{
  "message": "티켓을 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

**HTTP Status Codes**:
- `200 OK`: 성공
- `400 Bad Request`: 유효하지 않은 입력값
- `404 Not Found`: 티켓을 찾을 수 없음

**예시 코드 (cURL)**:
```bash
# 로그인 사용자
curl -X GET "http://localhost:5000/api/tickets/detail?ticketId=123&userId=1"

# 비로그인 사용자
curl -X GET "http://localhost:5000/api/tickets/detail?ticketId=123"
```

---

## 공통 응답 구조

모든 API는 다음과 같은 통일된 응답 구조를 사용합니다:

```json
{
  "message": "응답 메시지",
  "data": { ... },
  "statusCode": 200,
  "success": true
}
```

**공통 필드**:
| 필드 | 타입 | 설명 |
|------|------|------|
| message | string | 응답 메시지 (한글) |
| data | object \| array \| null | 응답 데이터 |
| statusCode | integer | HTTP 상태 코드 |
| success | boolean | 성공 여부 (200-299: true, 그 외: false) |

