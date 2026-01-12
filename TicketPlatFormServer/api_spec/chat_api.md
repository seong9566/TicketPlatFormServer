# 채팅 API 명세서

## 개요
티켓 거래를 위한 채팅방 생성/조회, 메시지 송수신, 거래 상태 업데이트(결제 요청/구매 확정/취소)를 제공합니다.

**기본 URL**: `/api/chat`  
**인증**: JWT 필요 (`Authorization: Bearer <token>`)  
**응답 형식**: JSON (ApiResponse<T> 래퍼 사용)

---

## API 엔드포인트

### 1. 채팅방 생성 또는 조회

**Endpoint**: `POST /api/chat/rooms`

**Request Body**:
```json
{
  "ticketId": 123
}
```

**Request Parameters**:
| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| ticketId | integer | O | 티켓 ID |

**Success Response**:
```json
{
  "message": "채팅방 조회 성공",
  "data": {
    "roomId": 10,
    "ticket": {
      "ticketId": 123,
      "title": "VIP석 양도",
      "price": 150000,
      "thumbnailUrl": null
    },
    "buyer": {
      "userId": 2,
      "nickname": "buyer1",
      "profileImageUrl": null,
      "mannerTemperature": 36.5
    },
    "seller": {
      "userId": 1,
      "nickname": "seller1",
      "profileImageUrl": null,
      "mannerTemperature": 36.5
    },
    "statusCode": "active",
    "statusName": "활성",
    "transaction": null,
    "canSendMessage": true,
    "canRequestPayment": false,
    "canConfirmPurchase": false,
    "canCancelTransaction": false,
    "messages": []
  },
  "statusCode": 200,
  "success": true
}
```

**참고사항**:
- **티켓 1개당 구매자별 1개 채팅방**만 생성됩니다. (동일 구매자는 재요청 시 기존 방 반환)
- 신규 생성 시 `messages`는 빈 배열일 수 있습니다.

**Error Responses**:
- `404 Not Found`: `"티켓을 찾을 수 없습니다."`
- `400 Bad Request`: `"본인의 티켓과는 채팅할 수 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 2. 내 채팅방 목록 조회

**Endpoint**: `GET /api/chat/rooms`

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| page | integer | X | 페이지 번호 (기본값: 1) |
| pageSize | integer | X | 페이지 크기 (기본값: 20) |

**Success Response**:
```json
{
  "message": "채팅방 목록 조회 성공",
  "data": [
    {
      "roomId": 10,
      "ticketId": 123,
      "ticketTitle": "VIP석 양도",
      "otherUser": {
        "userId": 1,
        "nickname": "seller1",
        "profileImageUrl": null
      },
      "lastMessage": null,
      "lastMessageAt": "2026-01-10T10:00:00Z",
      "unreadCount": 0,
      "roomStatusCode": "active",
      "roomStatusName": "활성",
      "transactionId": null,
      "transactionStatusCode": null,
      "transactionStatusName": null
    }
  ],
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 3. 채팅방 상세 조회

**Endpoint**: `GET /api/chat/rooms/{roomId}`

**Path Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| roomId | integer | O | 채팅방 ID |

**Success Response**: `채팅방 생성/조회`와 동일한 구조 (`messages` 포함)

**참고사항**:
- `messages`에는 최근 **30개** 메시지가 포함됩니다 (내림차순).

**Error Responses**:
- `404 Not Found`: `"채팅방을 찾을 수 없습니다."`
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 4. 메시지 전송 (텍스트 또는 이미지)

**Endpoint**: `POST /api/chat/messages`

**Content-Type**: `multipart/form-data`

**Form Fields**:
| 필드 | 타입 | 필수 | 설명 |
|------|------|------|------|
| roomId | integer | O | 채팅방 ID |
| message | string | X | 텍스트 메시지 |
| image | file | X | 이미지 파일 |

**참고사항**:
- `message` 또는 `image` 중 하나는 반드시 필요합니다.

**Success Response**:
```json
{
  "message": "메시지 전송 성공",
  "data": {
    "messageId": 999,
    "roomId": 10,
    "message": "안녕하세요",
    "imageUrl": null,
    "createdAt": "2026-01-10T10:01:00Z",
    "success": true
  },
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:
- `400 Bad Request`: `"메시지 또는 이미지를 입력해주세요."`
- `403 Forbidden`: `"이 채팅방은 더 이상 메시지를 보낼 수 없습니다."`
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `404 Not Found`: `"채팅방을 찾을 수 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 5. 메시지 목록 조회 (페이지네이션)

**Endpoint**: `GET /api/chat/rooms/{roomId}/messages`

**Query Parameters**:
| 파라미터 | 타입 | 필수 | 설명 |
|---------|------|------|------|
| lastMessageId | integer | X | 기준 메시지 ID (이전 메시지 조회용) |
| limit | integer | X | 조회 개수 (기본값: 50) |

**Success Response**:
```json
{
  "message": "메시지 목록 조회 성공",
  "data": [
    {
      "messageId": 999,
      "roomId": 10,
      "senderId": 2,
      "senderNickname": "buyer1",
      "senderProfileImage": null,
      "message": "안녕하세요",
      "imageUrl": null,
      "createdAt": "2026-01-10T10:01:00Z",
      "isMyMessage": true
    }
  ],
  "statusCode": 200,
  "success": true
}
```

**참고사항**:
- 기본 정렬은 **최근 메시지부터 내림차순**입니다.
- 이전 메시지를 더 가져오려면 `lastMessageId`를 전달합니다.
- 상세 조회 응답의 `messages`를 기준으로 추가 페이징 호출합니다.

**Error Responses**:
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 6. 메시지 읽음 처리

**Endpoint**: `POST /api/chat/rooms/{roomId}/read`

**Success Response**:
```json
{
  "message": "메시지 읽음 처리 완료",
  "data": null,
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:
- `404 Not Found`: `"채팅방을 찾을 수 없습니다."`
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 7. 결제 요청 (판매자 → 구매자)

**Endpoint**: `POST /api/chat/rooms/{roomId}/request-payment`

**Request Body**:
```json
{
  "transactionId": 555
}
```

**Success Response**:
```json
{
  "message": "결제 요청이 전송되었습니다",
  "data": {
    "paymentUrl": "/payment/555",
    "transactionId": 555,
    "amount": 150000
  },
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:
- `403 Forbidden`: `"판매자만 결제를 요청할 수 있습니다."`
- `404 Not Found`: `"채팅방을 찾을 수 없습니다."`
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 8. 구매 확정 (구매자 확정)

**Endpoint**: `POST /api/chat/rooms/{roomId}/confirm-purchase`

**Request Body**:
```json
{
  "transactionId": 555
}
```

**Success Response**:
```json
{
  "message": "구매 확정이 완료되었습니다",
  "data": {
    "transactionId": 555,
    "confirmedAt": "2026-01-10T10:10:00Z",
    "success": true
  },
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:
- `403 Forbidden`: `"구매자만 구매를 확정할 수 있습니다."`
- `400 Bad Request`: `"거래 정보가 일치하지 않습니다."`
- `404 Not Found`: `"채팅방을 찾을 수 없습니다."`
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

### 9. 거래 취소

**Endpoint**: `POST /api/chat/rooms/{roomId}/cancel`

**Request Body**:
```json
{
  "transactionId": 555,
  "cancelReason": "구매자 변심"
}
```

**Success Response**:
```json
{
  "message": "거래가 취소되었습니다",
  "data": null,
  "statusCode": 200,
  "success": true
}
```

**Error Responses**:
- `403 Forbidden`: `"거래 당사자만 취소할 수 있습니다."`
- `400 Bad Request`: `"거래 정보가 일치하지 않습니다."`
- `404 Not Found`: `"채팅방을 찾을 수 없습니다."`
- `403 Forbidden`: `"이 채팅방에 접근할 권한이 없습니다."`
- `401 Unauthorized`: `"인증 정보가 없습니다."`

---

## 실시간(SignalR) 연동

**Hub URL**: `/hubs/chat`  
**인증**: JWT 필요

### 클라이언트 → 서버 호출

- `JoinRoom(roomId)` : 특정 채팅방 참여
- `LeaveRoom(roomId)` : 채팅방 나가기
- `UserTyping(roomId)` : 타이핑 중 알림
- `UserStoppedTyping(roomId)` : 타이핑 중지 알림

### 서버 → 클라이언트 이벤트

1. **ReceiveMessage** (메시지 수신)
```json
{
  "messageId": 999,
  "roomId": 10,
  "senderId": 2,
  "senderNickname": "",
  "senderProfileImage": null,
  "message": "안녕하세요",
  "imageUrl": null,
  "createdAt": "2026-01-10T10:01:00Z"
}
```

2. **RoomUpdated** (채팅방 상태 변경 알림)
```json
{
  "roomId": 10,
  "event": "PaymentRequested",
  "transactionId": 555,
  "statusCode": null,
  "message": "결제가 요청되었습니다."
}
```

3. **UserJoined / UserLeft / UserTyping / UserStoppedTyping**
```json
{
  "userId": 2,
  "roomId": 10,
  "timestamp": "2026-01-10T10:02:00Z"
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
