# 채팅 API 수정 사항 - Lazy Room Creation

## 📌 수정 목적

채팅방 진입 시 즉시 방을 생성하지 않고, **첫 메시지 전송 시에만 방을 생성**하도록 변경합니다.

### 현재 문제점
- 사용자가 "채팅하기" 버튼을 클릭하면 즉시 채팅방이 생성됨
- 메시지를 보내지 않고 뒤로가기해도 빈 채팅방이 목록에 표시됨
- 불필요한 빈 채팅방이 DB에 누적됨

### 해결 방안
- 채팅방 진입 시에는 방을 생성하지 않음
- 기존 채팅방이 있는지만 확인
- 첫 메시지 전송 시점에 방 생성
- 메시지가 없는 채팅방은 목록에 표시하지 않음

---

## 🔄 API 수정 사항

### 1. 새 API 추가 (권장)

#### `GET /api/chat/rooms/by-ticket`

**설명**: 특정 티켓에 대한 기존 채팅방을 조회합니다 (생성하지 않음).

**Request**
```http
GET /api/chat/rooms/by-ticket?ticketId={ticketId}
Authorization: Bearer {token}
```

**Query Parameters**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| ticketId | int | O | 티켓 ID |

**Response** `200 OK` (채팅방 존재)
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
    "unreadCount": 2
  },
  "statusCode": 200
}
```

**Response** `404 Not Found` (채팅방 없음)
```json
{
  "message": "채팅방이 존재하지 않습니다",
  "data": null,
  "statusCode": 404
}
```

**구현 가이드**
```csharp
// ChatService.cs
public async Task<ChatRoomReadModel?> GetChatRoomByTicketAsync(int ticketId, int userId)
{
    var room = await _chatRepository.GetChatRoomByTicketAndUserAsync(ticketId, userId);
    
    if (room == null)
    {
        return null; // 또는 throw new NotFoundException()
    }
    
    return new ChatRoomReadModel
    {
        RoomId = room.RoomId,
        TicketId = room.TicketId,
        // ... 매핑
    };
}

// ChatRepository.cs
public async Task<ChatRoom?> GetChatRoomByTicketAndUserAsync(int ticketId, int userId)
{
    return await _context.ChatRooms
        .Where(cr => cr.TicketId == ticketId 
                  && (cr.SellerId == userId || cr.BuyerId == userId)
                  && cr.Status != ChatRoomStatus.Deleted)
        .FirstOrDefaultAsync();
}
```

---

### 2. 기존 API 수정

#### `GET /api/chat/rooms` (채팅방 목록 조회)

**수정 내용**: 메시지가 하나 이상 있는 채팅방만 반환

**Before**
```csharp
// 모든 채팅방 반환
var rooms = await _context.ChatRooms
    .Where(cr => cr.SellerId == userId || cr.BuyerId == userId)
    .OrderByDescending(cr => cr.LastMessageAt)
    .ToListAsync();
```

**After**
```csharp
// 메시지가 있는 채팅방만 반환
var rooms = await _context.ChatRooms
    .Where(cr => (cr.SellerId == userId || cr.BuyerId == userId)
              && cr.LastMessage != null)  // ⭐ 추가
    .OrderByDescending(cr => cr.LastMessageAt)
    .ToListAsync();
```

**또는 메시지 테이블과 JOIN**
```csharp
var rooms = await _context.ChatRooms
    .Where(cr => cr.SellerId == userId || cr.BuyerId == userId)
    .Where(cr => _context.ChatMessages
        .Any(m => m.RoomId == cr.RoomId))  // ⭐ 메시지가 있는 방만
    .OrderByDescending(cr => cr.LastMessageAt)
    .ToListAsync();
```

---

### 3. 유지되는 API

#### `POST /api/chat/rooms` (채팅방 생성/조회)

**변경 없음** - 기존 동작 유지
- Flutter에서 첫 메시지 전송 시에만 호출됨
- 이미 존재하면 조회, 없으면 생성

---

## 🔍 데이터베이스 고려사항

### 인덱스 확인
```sql
-- ChatRooms 테이블
CREATE INDEX IX_ChatRooms_TicketId_UserId 
ON ChatRooms(TicketId, SellerId, BuyerId);

CREATE INDEX IX_ChatRooms_LastMessageAt 
ON ChatRooms(LastMessageAt DESC);
```

### 기존 빈 채팅방 처리
```sql
-- 메시지가 없는 오래된 채팅방 삭제 (선택사항)
DELETE FROM ChatRooms
WHERE LastMessage IS NULL
  AND CreatedAt < DATEADD(DAY, -7, GETDATE());
```

---

## 📋 테스트 시나리오

### 시나리오 1: 새 채팅 시작 (메시지 전송)
1. Flutter: `GET /api/chat/rooms/by-ticket?ticketId=123` → 404
2. Flutter: 사용자가 메시지 입력 후 전송
3. Flutter: `POST /api/chat/rooms` → roomId=1 반환
4. Flutter: `POST /api/chat/messages` → 메시지 저장
5. Flutter: 채팅 목록 조회 → 방 표시됨 ✅

### 시나리오 2: 새 채팅 시작 (메시지 없이 뒤로가기)
1. Flutter: `GET /api/chat/rooms/by-ticket?ticketId=123` → 404
2. Flutter: 사용자가 메시지 입력 안 함
3. Flutter: 뒤로가기
4. Flutter: 채팅 목록 조회 → 방 표시 안 됨 ✅

### 시나리오 3: 기존 채팅방 재진입
1. Flutter: `GET /api/chat/rooms/by-ticket?ticketId=123` → roomId=1 반환
2. Flutter: `GET /api/chat/messages?roomId=1` → 기존 메시지 로드
3. 정상 채팅 가능 ✅

---

## 📊 마이그레이션 영향도

| 항목 | 영향 |
|------|------|
| 기존 API | `POST /api/chat/rooms` 동작 변경 없음 |
| 신규 API | `GET /api/chat/rooms/by-ticket` 추가 |
| DB 스키마 | 변경 없음 |
| 기존 데이터 | 영향 없음 (빈 방은 목록에서 숨김) |
| Flutter 앱 | 로직 변경 필요 |

---

## ✅ 개발 체크리스트

### Backend (C# .NET Core)
- [ ] `ChatService.cs` - `GetChatRoomByTicketAsync()` 메서드 추가
- [ ] `ChatRepository.cs` - `GetChatRoomByTicketAndUserAsync()` 메서드 추가
- [ ] `ChatController.cs` - `GET /rooms/by-ticket` 엔드포인트 추가
- [ ] `ChatQueries.cs` - 채팅방 목록 조회 쿼리 수정 (메시지 필터링)
- [ ] DTO 확인 - 기존 `ChatRoomRespDto` 재사용 가능
- [ ] 단위 테스트 작성
- [ ] Swagger 문서 업데이트
- [ ] Postman 테스트

### 배포
- [ ] dev 환경 배포 및 테스트
- [ ] test 환경 배포
- [ ] production 배포

---

## 🚀 배포 순서

1. **Backend API 배포** (먼저)
   - 신규 API 추가
   - 기존 API 수정
   
2. **Flutter 앱 업데이트** (나중)
   - Backend API 배포 후 진행
   - 호환성 유지

---

**작성일**: 2026-01-23  
**작성자**: Development Team  
**관련 문서**: `docs/api/chat_api_spec.md`
