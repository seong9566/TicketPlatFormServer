# SignalR 실시간 채팅 알림 API 명세서

**버전**: v2.2
**작성일**: 2026-01-23
**변경 사유**: 홈 화면 및 채팅 목록 화면에서 새 메시지 실시간 알림 기능 추가

---

## 📋 변경 개요

### 문제점
- 사용자가 채팅방 밖에 있을 때 (홈 화면, 채팅 목록 화면 등) 새 메시지 알림을 받지 못함
- BottomNav의 "NEW" 배지가 실시간으로 표시되지 않음
- 채팅방 목록(ChatRoomCard)이 실시간으로 업데이트되지 않음

### 해결 방안
- 메시지 전송 시 **수신자별 그룹** (`user_{userId}`)으로 추가 브로드캐스트
- 기존 채팅방 그룹 (`room_{roomId}`) 브로드캐스트는 유지
- 사용자가 어느 화면에 있든 메시지 수신 가능

### 영향 범위
- **API 엔드포인트**: `POST /api/chat/messages` (내부 로직 변경)
- **SignalR 이벤트**: `ReceiveMessage` (브로드캐스트 대상 추가)
- **하위 호환성**: ✅ 기존 기능 모두 유지, 추가 기능만 구현

---

## 🔧 API 엔드포인트 변경

### `POST /api/chat/messages` - 메시지 전송

#### 기본 정보
- **URL**: `/api/chat/messages`
- **Method**: `POST`
- **Content-Type**: `multipart/form-data`
- **Auth**: JWT Bearer Token (Required)

#### Request Body
```
roomId: long (required)
message: string (optional, 메시지 또는 이미지 중 하나 필수)
images: IFormFile[] (optional, 최대 3개)
```

#### Response (200 OK)
```json
{
  "message": "메시지 전송 성공",
  "data": {
    "messageId": 123,
    "roomId": 1,
    "senderId": 20,
    "senderNickname": "판매자",
    "senderProfileImage": "https://...",
    "message": "안녕하세요",
    "images": [
      {
        "url": "https://...",
        "expiresAt": "2026-01-24T10:00:00Z"
      }
    ],
    "createdAt": "2026-01-23T10:00:00Z",
    "success": true
  },
  "statusCode": 200
}
```

#### ⚠️ 내부 로직 변경사항

**변경 전 (v2.1)**:
```csharp
// 채팅방 그룹으로만 전송
await hubContext.Clients.Group($"room_{roomId}")
    .SendAsync("ReceiveMessage", messageDto);
```

**변경 후 (v2.2)**:
```csharp
// 1. 채팅방 정보 조회하여 수신자 파악
var room = await chatService.GetChatRoomById(roomId);
var receiverId = room.BuyerId == senderId ? room.SellerId : room.BuyerId;

// 2. 채팅방 그룹으로 전송 (기존)
await hubContext.Clients.Group($"room_{roomId}")
    .SendAsync("ReceiveMessage", messageDto);

// 3. 수신자 그룹으로 전송 (신규) ⭐
await hubContext.Clients.Group($"user_{receiverId}")
    .SendAsync("ReceiveMessage", messageDto);
```

**주요 차이점**:
1. 채팅방 조회 추가 (`GetChatRoomById`)
2. 수신자 ID 식별 로직 추가
3. `user_{receiverId}` 그룹으로 추가 브로드캐스트

---

## 📡 SignalR 이벤트 상세

### `ReceiveMessage` 이벤트

#### 이벤트명
```
ReceiveMessage
```

#### 브로드캐스트 대상 (v2.2 변경)

| 그룹명 | 수신 대상 | 목적 | 버전 |
|--------|----------|------|------|
| `room_{roomId}` | 채팅방 안에 있는 모든 사용자 | 실시간 메시지 표시 | v1.0 (기존) |
| `user_{receiverId}` | 수신자 (어느 화면에 있든) | 전역 알림, 배지 표시 | v2.2 (신규) ⭐ |

#### Payload
```typescript
{
  messageId: number;
  roomId: number;
  senderId: number;
  senderNickname: string;
  senderProfileImage: string | null;
  message: string | null;
  images: Array<{
    url: string;
    expiresAt: string;
  }> | null;
  createdAt: string; // ISO 8601
}
```

#### Flutter 앱 처리 권장사항

**중복 수신 처리**:
- 채팅방 안에 있는 사용자는 `room_{roomId}`와 `user_{userId}` 두 그룹 모두에서 메시지 수신
- **해결**: `messageId`로 중복 제거 필요

```dart
// 예시: Flutter에서 중복 제거
final _receivedMessageIds = <int>{};

void _handleReceiveMessage(Map<String, dynamic> data) {
  final messageId = data['messageId'] as int;

  // 중복 체크
  if (_receivedMessageIds.contains(messageId)) {
    print('📌 Duplicate message ignored: $messageId');
    return;
  }

  _receivedMessageIds.add(messageId);

  // 메시지 처리 로직
  _processNewMessage(data);
}
```

---

## 🔍 로그 포맷

### 메시지 전송 시 로그

#### Before (v2.1)
```
[ChatController.SendMessage] Broadcasting message to room_1. MessageId: 123, SenderId: 20
[ChatController.SendMessage] SignalR broadcast completed for room_1
```

#### After (v2.2)
```
[ChatController.SendMessage] Broadcasting message to room_1. MessageId: 123, SenderId: 20, ReceiverId: 15
[ChatController.SendMessage] ✅ SignalR broadcast completed: room_1 and user_15
```

### 로그 레벨
- **Information**: 정상 동작 (메시지 전송, 브로드캐스트 완료)
- **Warning**: 비정상이지만 처리 가능한 상황
- **Error**: 예외 발생

---

## 🧪 테스트 시나리오

### 시나리오 1: 홈 화면에서 메시지 수신
```
Given: 사용자 A가 앱 실행 후 홈 화면에 있음
  And: SignalR 연결 완료 (user_15 그룹 자동 가입)
When: 사용자 B가 사용자 A에게 메시지 전송
Then:
  - Backend 로그 확인: "✅ SignalR broadcast completed: room_1 and user_15"
  - Flutter 앱: "🔔 SignalR ReceiveMessage event fired!"
  - BottomNav: "NEW" 배지 즉시 표시됨
```

### 시나리오 2: 채팅 목록 화면에서 메시지 수신
```
Given: 사용자 A가 ChatView (채팅 목록) 화면에 있음
When: 사용자 B가 메시지 전송
Then:
  - ChatRoomCard 실시간 업데이트
  - lastMessage 업데이트
  - lastMessageAt 업데이트
  - unreadCount 증가
```

### 시나리오 3: 채팅방 안에서 메시지 수신 (회귀 테스트)
```
Given: 사용자 A가 ChatRoomView 안에 있음
  And: room_1 그룹과 user_15 그룹 모두 구독 중
When: 사용자 B가 메시지 전송
Then:
  - 메시지 즉시 표시 (기존 동작 유지)
  - 중복 수신되지 않음 (messageId로 필터링)
```

### 시나리오 4: 수신자 식별 로직 검증
```
Given: 채팅방 1 (BuyerId: 15, SellerId: 20)
When: 사용자 20(판매자)이 메시지 전송
Then:
  - receiverId = 15 (구매자)
  - SignalR 전송: room_1, user_15

When: 사용자 15(구매자)가 메시지 전송
Then:
  - receiverId = 20 (판매자)
  - SignalR 전송: room_1, user_20
```

---

## 🔧 내부 구현 상세

### 새로운 서비스 메서드

#### `IChatService.GetChatRoomById`
```csharp
Task<ChatRoom?> GetChatRoomById(long roomId);
```

**목적**: 채팅방 정보 조회 (권한 검증 없음)
**반환**: `ChatRoom` 엔티티 또는 `null`
**사용처**: `ChatController.SendMessage`에서 수신자 식별

#### `ChatService.GetChatRoomById`
```csharp
public async Task<ChatRoom?> GetChatRoomById(long roomId)
{
    return await chatRepo.GetChatRoomById(roomId);
}
```

**구현**: Repository 메서드 위임
**파일**: `Services/Chat/ChatService.cs:155-161`

---

## ⚠️ 주의사항

### 1. 중복 메시지 처리
- **문제**: 채팅방 안에 있는 사용자는 `room_{roomId}`와 `user_{userId}` 두 그룹에서 메시지 수신
- **해결**: Flutter 앱에서 `messageId`로 중복 제거 필수
- **권장**: 클라이언트에서 Set 자료구조로 수신한 messageId 관리

### 2. 성능 고려
- **추가 쿼리**: 메시지 전송마다 채팅방 조회 발생 (1회 SELECT)
- **영향**: 미미 (단일 쿼리, 인덱스 최적화됨, EF Core 1차 캐시 활용)
- **쿼리 예시**:
  ```sql
  SELECT * FROM chat_rooms WHERE id = ?
  ```

### 3. 네트워크 오버헤드
- **추가 브로드캐스트**: 메시지당 1회 추가 SignalR 전송
- **페이로드 크기**: 동일 (DTO 재사용)
- **영향**: 무시 가능 (SignalR은 WebSocket 사용, 효율적)

### 4. 하위 호환성
- **Breaking Change**: 없음 ✅
- **기존 기능**: 모두 유지 ✅
- **추가 기능**: 전역 알림만 추가 ✅

---

## 📊 메시지 흐름 비교

### Before (v2.1)
```
사용자 B → POST /api/chat/messages
            ↓
        ChatController.SendMessage
            ↓
        SignalR Broadcast
            ↓
        room_{roomId} 그룹
            ↓
    채팅방 안에 있는 사용자만 수신
```

### After (v2.2)
```
사용자 B → POST /api/chat/messages
            ↓
        ChatController.SendMessage
            ↓
    채팅방 정보 조회 (GetChatRoomById)
            ↓
        수신자 ID 파악
            ↓
        SignalR Broadcast (병렬)
        ├─→ room_{roomId} 그룹 (기존)
        └─→ user_{receiverId} 그룹 (신규) ⭐
            ↓
    어느 화면에 있든 수신 가능
```

---

## 📚 관련 파일

### Backend
| 파일 | 변경 내용 | Line |
|------|----------|------|
| `Controllers/ChatController.cs` | SendMessage 메서드 수정 | 108-164 |
| `Services/Chat/IChatService.cs` | GetChatRoomById 인터페이스 추가 | 12 |
| `Services/Chat/ChatService.cs` | GetChatRoomById 구현 추가 | 155-161 |
| `Hubs/ChatHub.cs` | 변경 없음 (이미 user 그룹 구현됨) | - |

### Flutter (권장 수정사항)
| 파일 | 권장 변경 | 목적 |
|------|----------|------|
| `ChatListViewModel` | messageId 중복 제거 로직 추가 | 중복 메시지 방지 |
| `ChatRoomViewModel` | messageId 중복 제거 로직 추가 | 중복 메시지 방지 |
| `BottomNavProvider` | `user_{userId}` 그룹 구독 확인 | 전역 알림 수신 |

---

## 🚀 배포 체크리스트

- [x] 코드 수정 완료
- [x] 빌드 성공 확인 (0 errors, 0 warnings)
- [ ] 로컬 테스트 (Swagger)
- [ ] SignalR Hub 연결 확인
- [ ] 시나리오 1-4 테스트 완료
- [ ] Flutter 앱 중복 제거 로직 확인
- [ ] dev 환경 배포
- [ ] 통합 테스트 (Flutter + Backend)
- [ ] production 배포

---

## 📞 문의 및 이슈

**작성자**: Claude Code
**리뷰어**: -
**승인자**: -
**배포일**: TBD

**관련 이슈**:
- 홈 화면 메시지 알림 미수신 문제 해결
- BottomNav "NEW" 배지 실시간 표시
- ChatView 실시간 업데이트

---

**Last Updated**: 2026-01-23
