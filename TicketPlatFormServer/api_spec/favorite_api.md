# 티켓 찜(좋아요) API 명세서

## 개요
사용자가 판매 중인 티켓을 찜(북마크)할 수 있는 기능을 제공하는 API입니다.

**기본 URL**: `/api/favorites`

**인증**: 현재 JWT 미구현 (userId를 파라미터로 전달)

**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트

### 1. 티켓 찜 토글 (추가/삭제)

이미 찜한 티켓은 찜 해제, 찜하지 않은 티켓은 찜 추가합니다.

**Endpoint**: `POST /api/favorites/tickets`

**Request Body**:
```json
{
  "userId": 1,
  "ticketId": 123
}
```

**Request Parameters**:
| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| userId | integer | O | 사용자 ID |
| ticketId | integer | O | 티켓 ID |

**Success Response (찜 추가)**:
```json
{
  "message": "티켓 찜 추가 완료",
  "data": {
    "ticketId": 123,
    "isFavorited": true
  },
  "statusCode": 200,
  "success": true
}
```

**Success Response (찜 해제)**:
```json
{
  "message": "티켓 찜 해제 완료",
  "data": {
    "ticketId": 123,
    "isFavorited": false
  },
  "statusCode": 200,
  "success": true
}
```

**Response Fields**:
| 필드 | 타입 | 설명 |
|------|------|------|
| ticketId | integer | 티켓 ID |
| isFavorited | boolean | 찜 상태 (true: 찜됨, false: 찜 해제됨) |

**Error Responses**:

1. **유효하지 않은 사용자 ID**:
```json
{
  "message": "유효하지 않은 사용자 ID입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

2. **유효하지 않은 티켓 ID**:
```json
{
  "message": "유효하지 않은 티켓 ID입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

3. **티켓을 찾을 수 없음**:
```json
{
  "message": "티켓을 찾을 수 없거나 판매가 중단되었습니다.",
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
curl -X POST "http://localhost:5000/api/favorites/tickets" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "ticketId": 123
  }'
```

**예시 코드 (Dart/Flutter)**:
```dart
final response = await http.post(
  Uri.parse('$baseUrl/api/favorites/tickets'),
  headers: {'Content-Type': 'application/json'},
  body: jsonEncode({
    'userId': userId,
    'ticketId': ticketId,
  }),
);

final result = jsonDecode(response.body);
if (result['success']) {
  final isFavorited = result['data']['isFavorited'];
  print(isFavorited ? '찜 추가됨' : '찜 해제됨');
}
```

---

### 2. 사용자가 찜한 티켓 목록 조회

사용자가 찜한 티켓 목록을 찜한 날짜 기준 내림차순으로 조회합니다.

**Endpoint**: `GET /api/favorites/tickets?userId={userId}`

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| userId | integer | O | 사용자 ID |

**Success Response**:
```json
{
  "message": "찜한 티켓 목록 조회 성공",
  "data": [
    {
      "ticketId": 123,
      "ticketTitle": "위키드 VIP석",
      "seatInfo": "1층 5구역 3열",
      "seatType": "VIP석",
      "price": 150000,
      "originalPrice": 200000,
      "remainingQuantity": 2,
      "createdAt": "2026-01-01T10:00:00",
      "favoritedAt": "2026-01-05T14:30:00",
      "eventTitle": "뮤지컬 위키드",
      "eventDate": "2026.02.15",
      "venueName": "충무아트센터 대극장",
      "eventPosterImageUrl": "https://example.com/poster.jpg",
      "seller": {
        "userId": 10,
        "nickname": "티켓매니아",
        "profileImageUrl": "https://example.com/profile.jpg",
        "mannerTemperature": 42.5,
        "totalTradeCount": 15,
        "responseRate": 95.5,
        "isSecurePayment": true
      }
    }
  ],
  "statusCode": 200,
  "success": true
}
```

**Response Fields**:
| 필드 | 타입 | 설명 |
|------|------|------|
| ticketId | integer | 티켓 ID |
| ticketTitle | string | 티켓 제목 |
| seatInfo | string | 좌석 정보 (예: "1층 5구역 3열") |
| seatType | string | 좌석 타입 (예: "VIP석", "R석", "S석", "A석") |
| price | integer | 판매가 |
| originalPrice | integer | 정가 |
| remainingQuantity | integer | 남은 수량 |
| createdAt | datetime | 티켓 등록 날짜 |
| favoritedAt | datetime | 찜한 날짜 |
| eventTitle | string | 이벤트 제목 |
| eventDate | string | 공연 날짜 (YYYY.MM.DD 형식) |
| venueName | string | 공연 장소명 |
| eventPosterImageUrl | string | 이벤트 포스터 이미지 URL |
| seller | object | 판매자 정보 |

**판매자 정보 (seller)**:
| 필드 | 타입 | 설명 |
|------|------|------|
| userId | integer | 판매자 ID |
| nickname | string | 판매자 닉네임 |
| profileImageUrl | string | 프로필 이미지 URL |
| mannerTemperature | float | 매너 온도 (36.5~99.9) |
| totalTradeCount | integer | 총 거래 횟수 |
| responseRate | float | 응답률 (0~100) |
| isSecurePayment | boolean | 안심결제 가능 여부 |

**참고사항**:
- 판매 중단된 티켓 (status_id ≠ 1) 제외
- 삭제된 티켓 (deleted_at IS NOT NULL) 제외
- 품절된 티켓 (remaining_quantity = 0) 제외
- 찜한 날짜 기준 내림차순 정렬

**Error Responses**:

1. **유효하지 않은 사용자 ID**:
```json
{
  "message": "유효하지 않은 사용자 ID입니다.",
  "data": null,
  "statusCode": 400,
  "success": false
}
```

**HTTP Status Codes**:
- `200 OK`: 성공 (빈 배열도 200 반환)
- `400 Bad Request`: 유효하지 않은 입력값

**예시 코드 (cURL)**:
```bash
curl -X GET "http://localhost:5000/api/favorites/tickets?userId=1"
```

**예시 코드 (Dart/Flutter)**:
```dart
final response = await http.get(
  Uri.parse('$baseUrl/api/favorites/tickets?userId=$userId'),
);

final result = jsonDecode(response.body);
if (result['success']) {
  final List<dynamic> tickets = result['data'];
  print('찜한 티켓 개수: ${tickets.length}');
}
```

---

### 3. 티켓 상세 조회 (찜 정보 포함)

티켓 상세 조회 API 명세는 별도 문서로 분리되었습니다.  
`TicketPlatFormServer/api_spec/ticket_detail_api.md`

**예시 코드 (Dart/Flutter)**:
```dart
// 로그인 사용자
final response = await http.get(
  Uri.parse('$baseUrl/api/tickets/detail?ticketId=$ticketId'),
  headers: {'Authorization': 'Bearer $accessToken'},
);

// 비로그인 사용자
final response = await http.get(
  Uri.parse('$baseUrl/api/tickets/detail?ticketId=$ticketId'),
);

final result = jsonDecode(response.body);
if (result['success']) {
  final ticket = result['data'];
  final isFavorited = ticket['isFavorited']; // true, false, 또는 null
}
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

---

## 데이터베이스 스키마

### user_favorites 테이블
```sql
CREATE TABLE user_favorites (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    favorite_type_id INT NOT NULL,  -- 2: 티켓 찜
    target_id INT NOT NULL,         -- 티켓 ID
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_user_favorite (user_id, favorite_type_id, target_id),
    INDEX idx_user_favorites_user (user_id),
    INDEX idx_user_favorites_type_target (favorite_type_id, target_id)
);
```

### favorite_types 테이블
```sql
CREATE TABLE favorite_types (
    id INT PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name_ko VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    sort_order INT
);

-- 데이터
INSERT INTO favorite_types VALUES
(1, 'event', '공연', TRUE, 1),
(2, 'ticket', '티켓', TRUE, 2);
```

---

## 비즈니스 로직

### 찜 토글 로직
1. 사용자가 이미 해당 티켓을 찜했는지 확인
2. 찜한 경우: 찜 해제 (DELETE) → `isFavorited: false` 반환
3. 찜하지 않은 경우: 찜 추가 (INSERT) → `isFavorited: true` 반환
4. UNIQUE KEY 제약 조건으로 중복 방지

### 찜 목록 조회 필터링
자동으로 다음 조건을 만족하는 티켓만 조회:
- `status_id = 1` (판매중)
- `deleted_at IS NULL` (삭제되지 않음)
- `remaining_quantity > 0` (재고 있음)

판매 중단되거나 품절된 티켓을 찜했더라도 목록에서 보이지 않으며, 다시 판매 시작하면 자동으로 표시됩니다.

---

## 에러 코드 정리

| 상태 코드 | 메시지 | 설명 |
|----------|--------|------|
| 400 | 유효하지 않은 사용자 ID입니다. | userId가 0 이하 |
| 400 | 유효하지 않은 티켓 ID입니다. | ticketId가 0 이하 |
| 404 | 티켓을 찾을 수 없거나 판매가 중단되었습니다. | 티켓 미존재 또는 status_id ≠ 1 또는 품절 |
| 404 | 티켓을 찾을 수 없습니다. | 티켓 상세 조회 시 미존재 |

---

## 향후 개선 사항

### JWT 인증 구현 후
현재는 userId를 쿼리 파라미터/바디로 전달하지만, JWT 구현 후:
- `[Authorize]` 애트리뷰트 추가
- `HttpContext.User.Claims`에서 userId 추출
- 클라이언트는 userId 전달 불필요

**예시**:
```csharp
[HttpPost("tickets")]
[Authorize]
public async Task<IActionResult> ToggleTicketFavorite([FromBody] FavoriteToggleReqDto req)
{
    var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
    req.UserId = userId; // 덮어쓰기
    // ...
}
```

### 찜 개수 표시 (향후)
티켓 목록에 찜 개수를 표시하려면:
- DTO에 `FavoriteCount` 필드 추가
- SQL에서 LEFT JOIN으로 찜 개수 집계

---

## 변경 이력

| 버전 | 날짜 | 변경 내용 |
|------|------|----------|
| 1.0.0 | 2026-01-08 | 티켓 찜 기능 최초 릴리스 |

---
