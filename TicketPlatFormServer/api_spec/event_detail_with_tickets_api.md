# 이벤트 상세 + 티켓 목록 조회 API 명세서 (GetEventDetailWithTickets)

## 개요
이벤트 상세 정보와 해당 이벤트의 판매 티켓 목록을 함께 조회하는 API입니다.

**기본 URL**: `/api/events`  
**인증**: 선택 (로그인 시 `Authorization` 헤더 필요)  
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트

### 1. 이벤트 상세 + 티켓 목록 조회

**Endpoint**: `GET /api/events/tickets?eventId={eventId}`

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| eventId | integer | O | 이벤트 ID |

**Headers (선택)**:
| 헤더 | 값 | 설명 |
|------|-----|------|
| Authorization | Bearer {accessToken} | 로그인 사용자만 필요 (찜 여부 포함) |

---

## 📤 Response

### Success Response (로그인 사용자)
```json
{
  "message": "이벤트 상세 정보 조회 성공",
  "data": {
    "eventId": 10,
    "eventTitle": "아이유 콘서트 2024",
    "eventPosterImageUrl": "https://example.com/poster.jpg",
    "startAt": "2026-01-15T19:30:00",
    "endAt": "2026-01-15T22:00:00",
    "venueName": "올림픽공원 체조경기장",
    "venueAddress": "서울 송파구 올림픽로 424",
    "artistId": 3,
    "artistName": "아이유",
    "isSoldOutImminent": true,
    "seatTypeFilters": [
      { "seatTypeName": "전체좌석", "ticketCount": 4 },
      { "seatTypeName": "R석", "ticketCount": 1 },
      { "seatTypeName": "VIP석", "ticketCount": 3 }
    ],
    "tickets": [
      {
        "ticketId": 34,
        "seatGradeId": 1,
        "seatGradeName": "VIP석",
        "area": "A구역",
        "row": "2열",
        "price": 150000,
        "originalPrice": 180000,
        "isConsecutive": true,
        "tradeMethodId": 1,
        "tradeMethodName": "PIN거래",
        "hasTicket": true,
        "description": "급처",
        "createdAt": "2026-01-12T11:30:00",
        "quantity": 2,
        "remainingQuantity": 1,
        "isSingleTicket": false,
        "ticketImages": [],
        "isFavorited": true,
        "seller": {
          "userId": 12,
          "nickname": "티켓마스터",
          "profileImageUrl": "https://example.com/profile.jpg",
          "mannerTemperature": 36.5,
          "totalTradeCount": 0,
          "responseRate": null,
          "isSecurePayment": false
        }
      }
    ]
  },
  "statusCode": 200,
  "success": true
}
```

### Success Response (비로그인 사용자)
```json
{
  "message": "이벤트 상세 정보 조회 성공",
  "data": {
    "eventId": 10,
    "eventTitle": "아이유 콘서트 2024",
    "isSoldOutImminent": false,
    "seatTypeFilters": [
      { "seatTypeName": "전체좌석", "ticketCount": 1 }
    ],
    "tickets": [
      {
        "ticketId": 34,
        "seatGradeName": "VIP석",
        "price": 150000,
        "originalPrice": 180000,
        "createdAt": "2026-01-12T11:30:00",
        "quantity": 2,
        "remainingQuantity": 1,
        "isSingleTicket": false,
        "ticketImages": [],
        "isFavorited": null,
        "seller": { "userId": 12, "nickname": "티켓마스터", "isSecurePayment": false }
      }
    ]
  },
  "statusCode": 200,
  "success": true
}
```

---

## 📊 Response 데이터 구조

### EventDetailRespDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| eventId | int | ❌ | 이벤트 ID |
| eventTitle | string | ❌ | 이벤트 제목 |
| eventPosterImageUrl | string | ✅ | 이벤트 포스터 이미지 URL |
| startAt | DateTime | ✅ | 공연 시작 날짜/시간 |
| endAt | DateTime | ✅ | 공연 종료 날짜/시간 |
| venueName | string | ✅ | 장소명 |
| venueAddress | string | ✅ | 장소 주소 |
| artistId | int | ✅ | 아티스트 ID (콘서트인 경우) |
| artistName | string | ✅ | 아티스트명 (콘서트인 경우) |
| isSoldOutImminent | bool | ❌ | 매진 임박 여부 (remainingQuantity <= 5 인 티켓 존재 시 true) |
| seatTypeFilters | SeatTypeFilterDto[] | ❌ | 좌석 타입 필터 목록 |
| tickets | TicketListRespDto[] | ❌ | 판매 티켓 목록 |

### SeatTypeFilterDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| seatTypeName | string | ❌ | 좌석 타입명 (예: "전체좌석", "VIP석") |
| ticketCount | int | ❌ | 해당 좌석 타입의 티켓 개수 |

### TicketListRespDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| ticketId | int | ❌ | 티켓 ID |
| seatGradeId | int | ✅ | 좌석 등급 ID |
| seatGradeName | string | ✅ | 좌석 등급 이름 |
| area | string | ✅ | 구역 |
| row | string | ✅ | 열 |
| price | int | ❌ | 판매가 |
| originalPrice | int | ❌ | 정가 |
| isConsecutive | bool | ✅ | 연석 여부 |
| tradeMethodId | int | ✅ | 거래 방법 ID |
| tradeMethodName | string | ✅ | 거래 방법 이름 |
| hasTicket | bool | ✅ | 티켓 보유 여부 |
| description | string | ✅ | 판매 사유/설명 |
| createdAt | DateTime | ❌ | 티켓 등록 날짜 |
| quantity | int | ❌ | 티켓 수량 |
| remainingQuantity | int | ❌ | 남은 수량 |
| isSingleTicket | bool | ❌ | 1인 1매 여부 |
| ticketImages | string[] | ❌ | 티켓 이미지 URL 목록 (이 API에서는 빈 배열 반환) |
| isFavorited | bool | ✅ | 찜 여부 (로그인 시 true/false, 비로그인 시 null) |
| seller | SellerInfoDto | ❌ | 판매자 정보 |

### SellerInfoDto
| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| userId | int | ❌ | 사용자 ID |
| nickname | string | ❌ | 닉네임 |
| profileImageUrl | string | ✅ | 프로필 이미지 URL |
| mannerTemperature | float | ✅ | 매너 온도 |
| totalTradeCount | int | ❌ | 총 거래 횟수 (목록에서는 0으로 반환) |
| responseRate | float | ✅ | 응답률 (목록에서는 null 반환) |
| isSecurePayment | bool | ❌ | 안심결제 가능 여부 (목록에서는 false 반환) |

---

## ⚠️ Error Responses

### 400 Bad Request
```json
{
  "message": "유효하지 않은 이벤트 ID입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

### 404 Not Found
```json
{
  "message": "이벤트를 찾을 수 없습니다.",
  "data": null,
  "statusCode": 404,
  "success": false
}
```

---

## 📝 참고사항
- `seatTypeFilters`에는 항상 `"전체좌석"` 항목이 포함됩니다.
- `ticketImages`는 목록 조회 특성상 빈 배열로 반환됩니다. 상세 이미지는 티켓 상세 조회 API를 사용하세요.
- 로그인하지 않으면 `isFavorited`는 `null`로 반환됩니다.

---

## 예시 코드 (Dart/Flutter)
```dart
final uri = Uri.parse('$baseUrl/api/events/tickets?eventId=$eventId');
final headers = <String, String>{};

if (accessToken != null && accessToken.isNotEmpty) {
  headers['Authorization'] = 'Bearer $accessToken';
}

final response = await http.get(uri, headers: headers);
final result = jsonDecode(response.body);

if (result['success'] == true) {
  final data = result['data'];
  final eventTitle = data['eventTitle'];
  final tickets = (data['tickets'] as List<dynamic>);
  print('$eventTitle: ${tickets.length} tickets');
}
```
