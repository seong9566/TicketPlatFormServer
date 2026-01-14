# 채팅 API 스펙

## 개요
실시간 채팅 및 거래 기능을 위한 API 명세서입니다.

**Base URL**: `http://localhost:5224/api/chat`

**인증**: 모든 API는 JWT Bearer Token 인증이 필요합니다.

**실시간 통신**: SignalR Hub를 통한 실시간 메시지 전송 지원 (`/hubs/chat`)

---

## 1. 채팅방 생성 또는 조회

### `POST /api/chat/rooms`

**설명**: 특정 티켓에 대한 채팅방을 생성하거나 이미 존재하는 경우 조회합니다.

**Request**
```
POST /api/chat/rooms
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**
```json
{
  "ticketId": 123
}
```

**Response** `200 OK`
```json
{
  "message": "채팅방 조회 성공",
  "data": {
    "roomId": 1,
    "ticketId": 123,
    "ticketTitle": "아이유 콘서트 티켓",
    "sellerId": 10,
    "sellerNickname": "판매자",
    "buyerId": 20,
    "buyerNickname": "구매자",
    "status": "active",
    "lastMessage": "안녕하세요",
    "lastMessageAt": "2026-01-14T10:30:00",
    "unreadCount": 0
  },
  "statusCode": 200
}
```

---

## 2. 내 채팅방 목록 조회

### `GET /api/chat/rooms`

**설명**: 내가 참여 중인 채팅방 목록을 페이징하여 조회합니다.

**Request**
```
GET /api/chat/rooms?page={page}&pageSize={pageSize}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| page | int | X | 페이지 번호 (default: 1) |
| pageSize | int | X | 페이지 크기 (default: 20) |

**Response** `200 OK`
```json
{
  "message": "채팅방 목록 조회 성공",
  "data": [
    {
      "roomId": 1,
      "ticketId": 123,
      "ticketTitle": "아이유 콘서트 티켓",
      "otherUserNickname": "판매자",
      "otherUserProfileImage": "https://...",
      "lastMessage": "안녕하세요",
      "lastMessageAt": "2026-01-14T10:30:00",
      "unreadCount": 2,
      "status": "active"
    }
  ],
  "statusCode": 200
}
```

---

## 3. 채팅방 상세 조회

### `GET /api/chat/rooms/detail`

**설명**: 특정 채팅방의 상세 정보를 조회합니다.

**Request**
```
GET /api/chat/rooms/detail?roomId={roomId}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| roomId | long | O | 채팅방 ID |

**Response** `200 OK`
```json
{
  "message": "채팅방 상세 조회 성공",
  "data": {
    "roomId": 1,
    "ticketId": 123,
    "ticketTitle": "아이유 콘서트 티켓",
    "ticketPrice": 150000,
    "ticketImageUrl": "https://...",
    "sellerId": 10,
    "sellerNickname": "판매자",
    "sellerProfileImage": "https://...",
    "buyerId": 20,
    "buyerNickname": "구매자",
    "buyerProfileImage": "https://...",
    "status": "active",
    "transactionId": 456,
    "transactionStatus": "pending"
  },
  "statusCode": 200
}
```

**Error Response** `401 Unauthorized`
```json
{
  "message": "인증 정보가 없습니다.",
  "data": null,
  "statusCode": 401
}
```

---

## 4. 메시지 전송

### `POST /api/chat/messages`

**설명**: 채팅방에 텍스트 또는 이미지 메시지를 전송합니다.

**Request**
```
POST /api/chat/messages
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Request Body (Form Data)**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| roomId | long | O | 채팅방 ID |
| message | string | X | 메시지 내용 (텍스트 메시지인 경우) |
| image | File | X | 이미지 파일 (이미지 메시지인 경우) |

**참고**: `message`와 `image` 중 하나는 반드시 제공되어야 합니다.

**Response** `200 OK`
```json
{
  "message": "메시지 전송 성공",
  "data": {
    "messageId": 789,
    "roomId": 1,
    "message": "안녕하세요",
    "imageUrl": null,
    "createdAt": "2026-01-14T10:30:00"
  },
  "statusCode": 200
}
```

**SignalR 실시간 알림**
메시지 전송 시 해당 채팅방의 모든 참여자에게 실시간으로 알림:
```json
Event: "ReceiveMessage"
{
  "messageId": 789,
  "roomId": 1,
  "senderId": 20,
  "senderNickname": "구매자",
  "senderProfileImage": "https://...",
  "message": "안녕하세요",
  "imageUrl": null,
  "createdAt": "2026-01-14T10:30:00"
}
```

---

## 5. 메시지 목록 조회

### `GET /api/chat/messages`

**설명**: 채팅방의 메시지 목록을 페이징하여 조회합니다.

**Request**
```
GET /api/chat/messages?roomId={roomId}&lastMessageId={lastMessageId}&limit={limit}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| roomId | long | O | 채팅방 ID |
| lastMessageId | long | X | 마지막으로 조회한 메시지 ID (커서 페이징) |
| limit | int | X | 조회할 메시지 수 (default: 50) |

**Response** `200 OK`
```json
{
  "message": "메시지 목록 조회 성공",
  "data": [
    {
      "messageId": 789,
      "senderId": 20,
      "senderNickname": "구매자",
      "message": "안녕하세요",
      "imageUrl": null,
      "createdAt": "2026-01-14T10:30:00",
      "isRead": true
    },
    {
      "messageId": 788,
      "senderId": 10,
      "senderNickname": "판매자",
      "message": null,
      "imageUrl": "https://...",
      "createdAt": "2026-01-14T10:25:00",
      "isRead": true
    }
  ],
  "statusCode": 200
}
```

**참고**:
- 커서 기반 페이징을 사용합니다.
- `lastMessageId`를 제공하지 않으면 최신 메시지부터 조회됩니다.
- 메시지는 최신순으로 정렬됩니다.

---

## 6. 메시지 읽음 처리

### `POST /api/chat/rooms/read`

**설명**: 채팅방의 읽지 않은 메시지를 모두 읽음 처리합니다.

**Request**
```
POST /api/chat/rooms/read
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**
```json
{
  "roomId": 1
}
```

**Response** `200 OK`
```json
{
  "message": "메시지 읽음 처리 완료",
  "data": null,
  "statusCode": 200
}
```

---

## 7. 결제 요청

### `POST /api/chat/rooms/request-payment`

**설명**: 판매자가 구매자에게 결제를 요청합니다.

**Request**
```
POST /api/chat/rooms/request-payment
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**
```json
{
  "roomId": 1,
  "transactionId": 456
}
```

**Response** `200 OK`
```json
{
  "message": "결제 요청이 전송되었습니다",
  "data": {
    "paymentUrl": "https://payment.example.com/...",
    "transactionId": 456
  },
  "statusCode": 200
}
```

**SignalR 실시간 알림**
결제 요청 시 채팅방 참여자에게 실시간으로 알림:
```json
Event: "RoomUpdated"
{
  "roomId": 1,
  "event": "PaymentRequested",
  "transactionId": 456,
  "message": "결제가 요청되었습니다."
}
```

**참고**: 판매자만 결제 요청을 할 수 있습니다.

---

## 8. 구매 확정

### `POST /api/chat/rooms/confirm-purchase`

**설명**: 구매자가 거래를 확정합니다.

**Request**
```
POST /api/chat/rooms/confirm-purchase
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**
```json
{
  "roomId": 1,
  "transactionId": 456
}
```

**Response** `200 OK`
```json
{
  "message": "구매 확정이 완료되었습니다",
  "data": {
    "transactionId": 456,
    "confirmedAt": "2026-01-14T10:35:00"
  },
  "statusCode": 200
}
```

**SignalR 실시간 알림**
구매 확정 시 채팅방 참여자에게 실시간으로 알림:
```json
Event: "RoomUpdated"
{
  "roomId": 1,
  "event": "PurchaseConfirmed",
  "transactionId": 456,
  "message": "구매가 확정되었습니다."
}
```

**참고**:
- 구매자만 구매 확정을 할 수 있습니다.
- 구매 확정 후 판매자에게 정산금이 지급됩니다.

---

## 9. 거래 취소

### `POST /api/chat/rooms/cancel`

**설명**: 거래를 취소합니다.

**Request**
```
POST /api/chat/rooms/cancel
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**
```json
{
  "roomId": 1,
  "transactionId": 456,
  "cancelReason": "구매 의사 취소"
}
```

**Response** `200 OK`
```json
{
  "message": "거래가 취소되었습니다",
  "data": null,
  "statusCode": 200
}
```

**SignalR 실시간 알림**
거래 취소 시 채팅방 참여자에게 실시간으로 알림:
```json
Event: "RoomUpdated"
{
  "roomId": 1,
  "event": "TransactionCancelled",
  "transactionId": 456,
  "statusCode": "cancelled",
  "message": "거래가 취소되었습니다. 사유: 구매 의사 취소"
}
```

**참고**:
- 판매자와 구매자 모두 거래를 취소할 수 있습니다.
- 결제 전에는 자유롭게 취소 가능합니다.
- 결제 후 취소 시 환불 처리됩니다.

---

## 10. 메시지 이미지 URL 재발급

### `GET /api/chat/messages/image-url`

**설명**: 만료된 이미지 URL을 재발급합니다.

**Request**
```
GET /api/chat/messages/image-url?messageId={messageId}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| messageId | long | O | 메시지 ID |

**Response** `200 OK`
```json
{
  "message": "이미지 URL 재발급 성공",
  "data": {
    "imageUrl": "https://storage.example.com/...",
    "expiresAt": "2026-01-14T11:30:00"
  },
  "statusCode": 200
}
```

**참고**:
- Signed URL 방식으로 생성된 이미지 URL은 일정 시간 후 만료됩니다.
- 만료된 이미지를 다시 조회할 때 이 API를 사용합니다.

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
| 400 | Bad Request - 잘못된 요청 |
| 401 | Unauthorized - 인증 필요 (토큰 없음 또는 만료) |
| 403 | Forbidden - 권한 없음 (채팅방 참여자 아님) |
| 404 | Not Found - 리소스 없음 |
| 500 | Internal Server Error - 서버 오류 |

**에러 응답 형식**
```json
{
  "message": "에러 메시지",
  "data": null,
  "statusCode": 400
}
```

### 채팅방 상태 코드

| Code | Description |
|------|-------------|
| `active` | 활성 상태 |
| `closed` | 종료된 상태 |
| `deleted` | 삭제된 상태 |

### SignalR Hub 연결

**Hub URL**: `ws://localhost:5224/hubs/chat`

**연결 방법** (JavaScript 예시)
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", {
    accessTokenFactory: () => "YOUR_JWT_TOKEN"
  })
  .build();

// 메시지 수신 이벤트
connection.on("ReceiveMessage", (message) => {
  console.log("새 메시지:", message);
});

// 채팅방 업데이트 이벤트
connection.on("RoomUpdated", (update) => {
  console.log("채팅방 업데이트:", update);
});

// 연결 시작
await connection.start();

// 채팅방 그룹 참여
await connection.invoke("JoinRoom", roomId);
```

### 파일 업로드

이미지 메시지 전송 시:
- **허용 형식**: jpg, jpeg, png, gif
- **최대 크기**: 5MB
- **저장소**: Supabase Storage

---

## API 플로우

### 1. 채팅 시작 플로우
```
1. POST /api/chat/rooms (채팅방 생성/조회)
2. SignalR 연결 및 JoinRoom
3. GET /api/chat/messages (기존 메시지 조회)
4. POST /api/chat/messages (메시지 전송)
```

### 2. 거래 플로우
```
1. 채팅방에서 협상
2. POST /api/chat/rooms/request-payment (판매자가 결제 요청)
3. 구매자가 결제 진행 (외부 결제 시스템)
4. POST /api/chat/rooms/confirm-purchase (구매자가 구매 확정)
5. 거래 완료
```

### 3. 거래 취소 플로우
```
1. POST /api/chat/rooms/cancel (판매자 또는 구매자가 취소)
2. 결제 완료 건인 경우 환불 처리
3. 거래 취소 완료
```

---

**작성일**: 2026-01-14
**버전**: v2.0
**최종 업데이트**: Query Parameter 및 Request Body 방식으로 리팩토링 완료
