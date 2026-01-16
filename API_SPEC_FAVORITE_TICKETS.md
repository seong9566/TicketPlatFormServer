# 찜한 티켓 목록 조회 API 스펙

## 📋 기본 정보

- **Endpoint**: `GET /api/favorites/tickets`
- **설명**: 사용자가 찜한 티켓 목록을 조회합니다.
- **인증**: ✅ 필수 (Bearer 토큰)

---

## 📥 Request

### Headers

| 헤더 | 필수 | 설명 |
|------|------|------|
| `Authorization` | ✅ | Bearer {access_token} |

### 예시

```http
GET /api/favorites/tickets HTTP/1.1
Host: localhost:5224
Authorization: Bearer {access_token}
```

---

## 📤 Response

### Success Response (200 OK)

```json
{
  "message": "찜한 티켓 목록 조회 성공",
  "data": [
    {
      "ticketId": 34,
      "seatGradeId": 1,
      "seatGradeName": "VIP",
      "area": "a구역",
      "row": "2열",
      "price": 150000,
      "originalPrice": 180000,
      "remainingQuantity": 2,
      "isConsecutive": true,
      "tradeMethodId": 1,
      "tradeMethodName": "PIN거래",
      "hasTicket": true,
      "createdAt": "2026-01-15T09:56:14",
      "favoritedAt": "2026-01-16T15:30:00",
      "eventTitle": "아이유 콘서트 2024",
      "eventDate": "2024.12.25",
      "venueName": "올림픽공원 체조경기장",
      "eventPosterImageUrl": "https://example.com/poster.jpg",
      "seller": {
        "userId": 12,
        "nickname": "티켓마스터",
        "profileImageUrl": "https://example.com/profile.jpg",
        "mannerTemperature": 36.5,
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

---

## 📊 Response 데이터 구조

### FavoriteTicketListRespDto

| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| `ticketId` | `int` | ❌ | 티켓 ID |
| `seatGradeId` | `int` | ✅ | 좌석 등급 ID |
| `seatGradeName` | `string` | ✅ | 좌석 등급 이름 (예: "VIP석", "R석") |
| `area` | `string` | ✅ | 구역 (예: "A구역") |
| `row` | `string` | ✅ | 열 (예: "5열") |
| `price` | `int` | ❌ | 판매가 (원) |
| `originalPrice` | `int` | ❌ | 정가 (원) |
| `remainingQuantity` | `int` | ❌ | 남은 수량 |
| `isConsecutive` | `bool` | ✅ | 연석 여부 |
| `tradeMethodId` | `int` | ✅ | 거래 방법 ID |
| `tradeMethodName` | `string` | ✅ | 거래 방법 이름 (예: "PIN거래", "배송거래") |
| `hasTicket` | `bool` | ✅ | 티켓 보유 여부 |
| `createdAt` | `DateTime` | ❌ | 티켓 등록 날짜 |
| `favoritedAt` | `DateTime` | ❌ | 찜한 날짜 |
| `eventTitle` | `string` | ✅ | 이벤트 제목 |
| `eventDate` | `string` | ✅ | 공연 날짜 (포맷: YYYY.MM.DD) |
| `venueName` | `string` | ✅ | 장소명 |
| `eventPosterImageUrl` | `string` | ✅ | 이벤트 포스터 이미지 URL |
| `seller` | `SellerInfoDto` | ❌ | 판매자 정보 |

### SellerInfoDto

| 필드 | 타입 | Nullable | 설명 |
|------|------|----------|------|
| `userId` | `int` | ❌ | 사용자 ID |
| `nickname` | `string` | ❌ | 닉네임 |
| `profileImageUrl` | `string` | ✅ | 프로필 이미지 URL |
| `mannerTemperature` | `float` | ✅ | 매너 온도 (36.5 기준) |
| `totalTradeCount` | `int` | ❌ | 총 거래 횟수 |
| `responseRate` | `float` | ✅ | 응답률 (0-100, 판매자가 채팅에 응답한 비율) |
| `isSecurePayment` | `bool` | ❌ | 안심결제 가능 여부 (본인인증, 휴대폰인증, 계좌인증 모두 완료) |

---

## ⚠️ Error Responses

### 401 Unauthorized
```json
{
  "message": "인증 정보가 없습니다.",
  "data": null,
  "statusCode": 401,
  "success": false
}
```

### 500 Internal Server Error
```json
{
  "message": "서버 내부 오류 발생",
  "data": null,
  "statusCode": 500,
  "success": false
}
```

---

## 🔄 변경 사항 (2026-01-16 리팩토링)

### ❌ 제거된 필드

다음 필드가 API 응답에서 제거되었습니다:

- ~~`ticketTitle`~~ - 티켓 제목 (티켓 제목 개념 삭제됨, `eventTitle` 사용 권장)

### ✅ 유지된 필드

- `eventTitle`: 이벤트 제목이 티켓의 주요 식별 텍스트로 사용됩니다.
- `ticketId`: 티켓의 고유 식별자

---

## 📝 참고사항

- `ticketTitle`이 제거되었으므로, UI에서는 `eventTitle`을 메인 타이틀로 표시하고, 세부 좌석 정보(`seatGradeName`, `area`, `row`)를 서브 정보로 표시하는 것을 권장합니다.
- 정렬 순서는 찜한 날짜(`favoritedAt`) 내림차순(최신순)입니다.
