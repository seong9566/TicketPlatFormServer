# 티켓 상세 조회 API 명세서 (GetTicketDetail)

## 개요
티켓의 상세 정보를 조회하는 API입니다. 티켓 정보, 판매자 정보, 이벤트 정보를 포함합니다.

**기본 URL**: `/api/tickets`
**인증**: 선택 (로그인 시 `Authorization` 헤더 필요)
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트

### 1. 티켓 상세 정보 조회

**Endpoint**: `GET /api/tickets/detail?ticketId={ticketId}`

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| ticketId | integer | O | 티켓 ID |

**Headers (선택)**:
| 헤더 | 값 | 설명 |
|------|-----|------|
| Authorization | Bearer {accessToken} | 로그인 사용자만 필요 (찜 여부 포함) |

---

## 📤 Response

### Success Response (로그인 사용자)
```json
{
  "message": "티켓 상세 정보 조회 성공",
  "data": {
    "ticketId": 34,
    "seatGradeId": 1,
    "seatGradeCode": "VIP",
    "seatGradeName": "VIP석",
    "seatGradeNameEn": "VIP Seat",
    "areaId": 5,
    "area": "A구역",
    "locationId": 2,
    "locationName": "1층",
    "row": "5열",
    "price": 150000,
    "originalPrice": 180000,
    "isConsecutive": true,
    "tradeMethodId": 1,
    "tradeMethodName": "PIN거래",
    "hasTicket": true,
    "description": "급처합니다. 연락주세요.",
    "createdAt": "2026-01-12T11:30:00",
    "quantity": 2,
    "remainingQuantity": 2,
    "isSingleTicket": false,
    "ticketImages": [
      "https://storage.supabase.co/tickets/34/image1.jpg?signed=...",
      "https://storage.supabase.co/tickets/34/image2.jpg?signed=..."
    ],
    "isFavorited": true,
    "features": [
      {
        "featureId": 1,
        "code": "CONSECUTIVE",
        "nameKo": "연석"
      },
      {
        "featureId": 3,
        "code": "AISLE_SEAT",
        "nameKo": "통로석"
      }
    ],
    "seller": {
      "userId": 12,
      "nickname": "티켓마스터",
      "profileImageUrl": "https://storage.supabase.co/profiles/12.jpg?signed=...",
      "mannerTemperature": 36.5,
      "totalTradeCount": 24,
      "responseRate": 95.5,
      "isSecurePayment": true
    },
    "event": {
      "eventId": 10,
      "eventTitle": "아이유 콘서트 2024",
      "posterImageUrl": "https://storage.supabase.co/events/10/poster.jpg?signed=...",
      "startAt": "2026-01-15T19:30:00",
      "endAt": "2026-01-15T22:00:00",
      "venueName": "올림픽공원 체조경기장"
    }
  },
  "statusCode": 200,
  "success": true
}
```

### Success Response (비로그인 사용자)
```json
{
  "message": "티켓 상세 정보 조회 성공",
  "data": {
    "ticketId": 34,
    "seatGradeName": "VIP석",
    "area": "A구역",
    "locationName": "1층",
    "row": "5열",
    "price": 150000,
    "originalPrice": 180000,
    "isConsecutive": true,
    "tradeMethodName": "PIN거래",
    "hasTicket": true,
    "description": "급처합니다. 연락주세요.",
    "createdAt": "2026-01-12T11:30:00",
    "quantity": 2,
    "remainingQuantity": 2,
    "isSingleTicket": false,
    "ticketImages": [
      "https://storage.supabase.co/tickets/34/image1.jpg?signed=...",
      "https://storage.supabase.co/tickets/34/image2.jpg?signed=..."
    ],
    "isFavorited": null,
    "features": [
      {
        "featureId": 1,
        "code": "CONSECUTIVE",
        "nameKo": "연석"
      }
    ],
    "seller": {
      "userId": 12,
      "nickname": "티켓마스터",
      "profileImageUrl": "https://storage.supabase.co/profiles/12.jpg?signed=...",
      "mannerTemperature": 36.5,
      "totalTradeCount": 24,
      "responseRate": 95.5,
      "isSecurePayment": true
    },
    "event": {
      "eventId": 10,
      "eventTitle": "아이유 콘서트 2024",
      "posterImageUrl": "https://storage.supabase.co/events/10/poster.jpg?signed=...",
      "startAt": "2026-01-15T19:30:00",
      "endAt": "2026-01-15T22:00:00",
      "venueName": "올림픽공원 체조경기장"
    }
  },
  "statusCode": 200,
  "success": true
}
```

---

## 📊 Response 데이터 구조

### TicketDetailRespDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| ticketId | int | ❌ | 티켓 ID |
| seatGradeId | int | ✅ | 좌석 등급 ID |
| seatGradeCode | string | ✅ | 좌석 등급 코드 (예: "VIP", "R", "S") |
| seatGradeName | string | ✅ | 좌석 등급 이름 (예: "VIP석", "일반석") |
| seatGradeNameEn | string | ✅ | 좌석 등급 영문명 (예: "VIP Seat") |
| areaId | int | ✅ | 구역 ID |
| area | string | ✅ | 구역 (예: "A구역") |
| locationId | int | ✅ | 위치 ID |
| locationName | string | ✅ | 위치명 (예: "1층", "2층", "플로어석") |
| row | string | ✅ | 열 (예: "5열") |
| price | int | ❌ | 판매가 |
| originalPrice | int | ❌ | 정가 |
| isConsecutive | bool | ✅ | 연석 여부 |
| tradeMethodId | int | ✅ | 거래 방법 ID |
| tradeMethodName | string | ✅ | 거래 방법 이름 (예: "PIN거래", "배송거래") |
| hasTicket | bool | ✅ | 티켓 보유 여부 |
| description | string | ✅ | 판매 사유/설명 |
| createdAt | DateTime | ❌ | 티켓 등록 날짜 |
| quantity | int | ❌ | 티켓 수량 |
| remainingQuantity | int | ❌ | 남은 수량 |
| isSingleTicket | bool | ❌ | 1인 1매 여부 (quantity가 1이면 true) |
| ticketImages | string[] | ❌ | 티켓 이미지 URL 목록 (Signed URL) |
| isFavorited | bool | ✅ | 찜 여부 (로그인 시 true/false, 비로그인 시 null) |
| features | TicketFeatureDto[] | ✅ | 티켓 특이사항 목록 |
| seller | SellerInfoDto | ❌ | 판매자 정보 |
| event | EventInfoDto | ❌ | 이벤트 정보 |

### TicketFeatureDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| featureId | int | ❌ | 특이사항 ID |
| code | string | ❌ | 특이사항 코드 (예: "CONSECUTIVE", "AISLE_SEAT") |
| nameKo | string | ❌ | 특이사항 한글명 (예: "연석", "통로석") |

### SellerInfoDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| userId | int | ❌ | 사용자 ID |
| nickname | string | ❌ | 닉네임 |
| profileImageUrl | string | ✅ | 프로필 이미지 URL (Signed URL) |
| mannerTemperature | float | ✅ | 매너 온도 |
| totalTradeCount | int | ❌ | 총 거래 횟수 |
| responseRate | float | ✅ | 응답률 (%) |
| isSecurePayment | bool | ❌ | 안심결제 가능 여부 |

### EventInfoDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| eventId | int | ❌ | 이벤트 ID |
| eventTitle | string | ❌ | 이벤트 제목 |
| posterImageUrl | string | ✅ | 포스터 이미지 URL (Signed URL) |
| startAt | DateTime | ✅ | 공연 시작 일시 |
| endAt | DateTime | ✅ | 공연 종료 일시 |
| venueName | string | ✅ | 공연 장소명 |

---

## ⚠️ Error Responses

### 400 Bad Request
```json
{
  "message": "유효하지 않은 티켓 ID입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

### 404 Not Found
```json
{
  "message": "티켓을 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

---

## 📝 참고사항

### 이미지 URL (Signed URL)
- `ticketImages`, `seller.profileImageUrl`, `event.posterImageUrl`은 모두 Supabase Storage의 Signed URL입니다.
- Signed URL은 일정 시간(기본 1시간) 후 만료되므로, 만료 시 재발급이 필요합니다.
- 이미지 URL이 이미 HTTP/HTTPS로 시작하는 경우 Signed URL 변환을 하지 않습니다.
- **중요**: 백엔드에서 자동으로 Signed URL로 변환하여 반환하므로, 프론트엔드에서 추가 처리 없이 바로 사용 가능합니다.

### 찜 여부 (isFavorited)
- 로그인하지 않으면 `isFavorited`는 `null`로 반환됩니다.
- 로그인한 경우, 해당 사용자의 찜 여부가 `true` 또는 `false`로 반환됩니다.

### 이벤트 정보
- 티켓과 연결된 이벤트 정보가 함께 반환됩니다.
- 이벤트가 삭제되거나 비활성화된 경우에도 티켓 조회는 가능하며, 이벤트 정보는 기본값으로 반환됩니다.

### 티켓 상태
- `remainingQuantity > 0`인 티켓만 조회 가능합니다.
- 삭제되었거나 판매 중단된 티켓은 조회되지 않습니다.

### 판매자 정보
- 상세 조회에서는 판매자의 전체 정보가 반환됩니다.
- 매너 온도, 총 거래 횟수, 응답률 등을 확인할 수 있습니다.

---

## 예시 코드 (Dart/Flutter)

### 로그인 사용자
```dart
final uri = Uri.parse('$baseUrl/api/tickets/detail?ticketId=$ticketId');
final headers = {
  'Authorization': 'Bearer $accessToken',
};

final response = await http.get(uri, headers: headers);
final result = jsonDecode(response.body);

if (result['success'] == true) {
  final data = result['data'];
  final ticketId = data['ticketId'];
  final price = data['price'];
  final isFavorited = data['isFavorited']; // true or false
  final eventTitle = data['event']['eventTitle'];
  final venueName = data['event']['venueName'];

  print('Ticket #$ticketId: $eventTitle at $venueName');
  print('Price: ₩$price');
  print('Favorited: $isFavorited');
}
```

### 비로그인 사용자
```dart
final uri = Uri.parse('$baseUrl/api/tickets/detail?ticketId=$ticketId');

final response = await http.get(uri);
final result = jsonDecode(response.body);

if (result['success'] == true) {
  final data = result['data'];
  final isFavorited = data['isFavorited']; // null

  print('Favorited: $isFavorited'); // null
}
```

---

## 비교: 목록 조회 vs 상세 조회

| 항목 | 목록 조회 (TicketListRespDto) | 상세 조회 (TicketDetailRespDto) |
|------|------------------------------|--------------------------------|
| 티켓 이미지 | 첫 번째 이미지만 (썸네일) | 모든 이미지 |
| 판매자 정보 | 기본 정보만 (이름, 프로필) | 전체 정보 (매너온도, 거래횟수, 응답률) |
| 이벤트 정보 | ❌ 포함 안 됨 | ✅ 포함 (제목, 포스터, 날짜, 장소) |
| 좌석 상세 | 기본 정보 | 확장 정보 (등급 코드/영문명 포함) |
| 사용 목적 | 이벤트 상세 화면의 티켓 목록 | 티켓 상세 화면 |

---

## 관련 API
- **이벤트 상세 + 티켓 목록 조회**: `GET /api/events/tickets?eventId={eventId}` - 특정 이벤트의 티켓 목록 조회
- **찜하기**: `POST /api/favorites` - 티켓 찜하기/찜 해제
- **채팅 시작**: `POST /api/chat/rooms` - 판매자와 채팅 시작
