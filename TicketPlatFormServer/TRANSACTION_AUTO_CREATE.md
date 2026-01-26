# Transaction 자동 생성 기능 구현 문서

## 문제 상황

### 증상
- `GetChatRoomDetail` API 호출 시 `ChatRoomDetailRespDto.Transaction`이 항상 `null`로 반환됨
- 결제 요청을 위해 필요한 `TransactionId`가 없어 결제 진행 불가능

### 근본 원인
- Transaction은 결제 요청 시점에 자동 생성되지 않았음
- 기존 `RequestPayment` 메서드는 클라이언트로부터 `transactionId`를 받아서 연결만 수행
- Transaction 생성 로직이 코드베이스에 존재하지 않음

---

## 해결 방안

### 핵심 변경사항
판매자가 "결제 요청" 버튼을 클릭할 때 Transaction을 **자동으로 생성**하도록 변경

1. ✅ TransactionRepository에 생성 메서드 추가
2. ✅ TransactionItemRepository 신규 생성
3. ✅ ChatService.RequestPayment 로직 변경 (transactionId 파라미터 제거 → 자동 생성)
4. ✅ API 시그니처 변경 (RequestPaymentReqDto에서 TransactionId 필드 제거)

---

## 구현 상세

### 1. TransactionRepository 확장

#### 파일: `Repository/Transaction/ITransactionRepository.cs`

```csharp
/// <summary>
/// 거래 생성
/// </summary>
Task<DBModel.Transaction> CreateTransactionAsync(DBModel.Transaction transaction);

/// <summary>
/// Code로 TransactionStatus 조회
/// </summary>
Task<DBModel.TransactionStatus?> GetTransactionStatusByCodeAsync(string code);
```

#### 파일: `Repository/Transaction/TransactionRepository.cs`

```csharp
/// <summary>
/// 거래 생성
/// </summary>
public async Task<DBModel.Transaction> CreateTransactionAsync(DBModel.Transaction transaction)
{
    transaction.CreatedAt = DateTime.UtcNow;
    context.Transactions.Add(transaction);
    await context.SaveChangesAsync();
    return transaction;
}

/// <summary>
/// Code로 TransactionStatus 조회
/// </summary>
public async Task<DBModel.TransactionStatus?> GetTransactionStatusByCodeAsync(string code)
{
    return await context.TransactionStatuses
        .Where(ts => ts.Code == code && ts.IsActive == true)
        .FirstOrDefaultAsync();
}
```

---

### 2. TransactionItemRepository 신규 생성

#### 파일: `Repository/Transaction/ITransactionItemRepository.cs`

```csharp
namespace TicketPlatFormServer.Repository.Transactions;

public interface ITransactionItemRepository
{
    /// <summary>
    /// 거래 항목 생성
    /// </summary>
    Task<TransactionItem> CreateTransactionItemAsync(TransactionItem item);
}
```

#### 파일: `Repository/Transaction/TransactionItemRepository.cs`

```csharp
namespace TicketPlatFormServer.Repository.Transactions;

public class TransactionItemRepository(TicketContext context) : ITransactionItemRepository
{
    public async Task<TransactionItem> CreateTransactionItemAsync(TransactionItem item)
    {
        item.CreatedAt = DateTime.UtcNow;
        context.TransactionItems.Add(item);
        await context.SaveChangesAsync();
        return item;
    }
}
```

---

### 3. ChatService.RequestPayment 메서드 변경

#### 변경 전
```csharp
public async Task<PaymentUrlRespDto> RequestPayment(long roomId, long transactionId, int userId)
{
    // transactionId를 클라이언트로부터 받아서 검증만 수행
    // Transaction 생성 로직 없음
}
```

#### 변경 후
```csharp
public async Task<PaymentUrlRespDto> RequestPayment(long roomId, int userId)
{
    // 1. 권한 및 상태 검증
    await ValidateUserInRoom(roomId, userId);

    var room = await chatRepo.GetChatRoomById(roomId);
    if (room.SellerId != userId)
        throw new AppException("판매자만 결제를 요청할 수 있습니다.", HttpStatusCode.Forbidden);

    if (room.TransactionId != null)
        throw new AppException("이미 결제 요청된 거래입니다.", HttpStatusCode.BadRequest);

    // 2. Transaction 자동 생성 (pending_payment 상태)
    var pendingStatus = await transactionRepo.GetTransactionStatusByCodeAsync("pending_payment");
    var transaction = new DBModel.Transaction
    {
        BuyerId = room.BuyerId,
        SellerId = room.SellerId,
        StatusId = pendingStatus.Id,
        ReservedAt = DateTime.UtcNow,
        ReservationExpiresAt = DateTime.UtcNow.AddHours(24), // 24시간 후 만료
    };
    var createdTransaction = await transactionRepo.CreateTransactionAsync(transaction);

    // 3. TransactionItem 생성 (티켓 정보)
    var transactionItem = new TransactionItem
    {
        TransactionId = createdTransaction.Id,
        TicketId = room.TicketId,
        Quantity = 1,
        UnitPrice = room.Ticket.Price,
        TotalPrice = room.Ticket.Price,
    };
    await transactionItemRepo.CreateTransactionItemAsync(transactionItem);

    // 4. ChatRoom에 Transaction 연결
    await chatRepo.SetTransactionId(roomId, createdTransaction.Id);

    // 5. 결제 URL 생성 및 반환
    // ... (생략)
}
```

---

### 4. API 변경사항

#### RequestPaymentReqDto 변경

**변경 전:**
```csharp
public class RequestPaymentReqDto
{
    public long RoomId { get; set; }
    public long TransactionId { get; set; }  // ❌ 제거됨
}
```

**변경 후:**
```csharp
public class RequestPaymentReqDto
{
    public long RoomId { get; set; }
    // TransactionId 필드 제거됨
}
```

#### API 요청 예시

**변경 전:**
```json
POST /api/chat/rooms/request-payment
{
  "roomId": 123,
  "transactionId": 456  // ❌ 더 이상 필요 없음
}
```

**변경 후:**
```json
POST /api/chat/rooms/request-payment
{
  "roomId": 123
}
```

---

### 5. DI 등록 (Program.cs)

```csharp
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionItemRepository, TransactionItemRepository>();  // 추가
```

---

## 데이터 흐름

### 1. 채팅방 생성 시점 (구매자)
```
GET /api/chat/rooms (ticketId=1)
→ ChatRoom 생성
→ TransactionInfo: null ✅ (정상)
```

### 2. 결제 요청 시점 (판매자)
```
POST /api/chat/rooms/request-payment
{
  "roomId": 123
}

백엔드 처리:
1. Transaction 생성 (status_id = pending_payment)
2. TransactionItem 생성 (ticket_id, quantity=1, price)
3. ChatRoom.transaction_id 업데이트
4. OrderId 생성 및 Payment 초기화
5. 시스템 메시지 전송 ("결제가 요청되었습니다.")

응답:
{
  "message": "결제 요청 성공",
  "data": {
    "paymentUrl": "/payment/checkout?orderId=TXN_456_...",
    "transactionId": 456,
    "amount": 50000
  }
}
```

### 3. 채팅방 재조회 (구매자)
```
GET /api/chat/rooms/detail?roomId=123
→ TransactionInfo: {
    "transactionId": 456,
    "statusCode": "pending_payment",
    "statusName": "결제대기",
    "confirmedAt": null,
    "cancelledAt": null
  } ✅
```

### 4. 결제 진행 (구매자)
```
POST /api/payment/confirm
→ Transaction.status_id → "paid" (결제완료)
```

### 5. 구매 확정 (구매자)
```
POST /api/chat/rooms/confirm-purchase
→ Transaction.status_id → "confirmed" (구매확정)
→ Escrow 해제 → 판매자에게 정산
```

---

## Transaction 상태 코드

구현에 사용되는 상태 코드 (transaction_statuses 테이블):

| ID | Code | Name (KR) | 설명 |
|----|------|-----------|------|
| 1 | reserved | 예약중 | 예약 단계 |
| 2 | **pending_payment** | **결제대기** | **결제 요청 후 대기 상태** ✅ |
| 3 | paid | 결제완료 | 결제 성공 |
| 4 | confirmed | 구매확정 | 구매자가 확정 |
| 5 | completed | 거래완료 | 모든 프로세스 완료 |
| 6 | cancelled | 취소됨 | 거래 취소 |
| 7 | refunded | 환불됨 | 환불 처리 |

### 상태 전이 다이어그램
```
[채팅방 생성]
     ↓
[결제 요청] → pending_payment (자동 생성)
     ↓
[결제 진행] → paid
     ↓
[구매 확정] → confirmed
     ↓
[정산 완료] → completed

[취소 요청] → cancelled
     ↓
[환불 처리] → refunded
```

---

## 중복 방지 로직

동일 채팅방에서 중복 결제 요청 방지:

```csharp
if (room.TransactionId != null)
{
    throw new AppException("이미 결제 요청된 거래입니다.", HttpStatusCode.BadRequest);
}
```

**테스트 시나리오:**
1. 판매자가 결제 요청 → Transaction 생성 성공 ✅
2. 판매자가 다시 결제 요청 → 400 Bad Request ("이미 결제 요청된 거래입니다.") ✅

---

## 권한 검증

### 판매자만 결제 요청 가능
```csharp
if (room.SellerId != userId)
{
    throw new AppException("판매자만 결제를 요청할 수 있습니다.", HttpStatusCode.Forbidden);
}
```

**테스트 시나리오:**
- 판매자가 요청 → 성공 ✅
- 구매자가 요청 → 403 Forbidden ✅

---

## 데이터베이스 변화

### Transaction 테이블
```sql
INSERT INTO transactions (buyer_id, seller_id, status_id, reserved_at, reservation_expires_at, created_at)
VALUES (10, 20, 2, NOW(), DATE_ADD(NOW(), INTERVAL 24 HOUR), NOW());
```

### TransactionItem 테이블
```sql
INSERT INTO transaction_items (transaction_id, ticket_id, quantity, unit_price, total_price, created_at)
VALUES (456, 1, 1, 50000, 50000, NOW());
```

### ChatRoom 테이블
```sql
UPDATE chat_rooms
SET transaction_id = 456
WHERE id = 123;
```

---

## 검증 방법

### 1. API 테스트 (Postman)

```bash
# Step 1: 채팅방 생성 (구매자 토큰)
POST /api/chat/rooms
Authorization: Bearer {buyer_token}
{
  "ticketId": 1
}

# 확인: TransactionInfo가 null인지 확인

# Step 2: 결제 요청 (판매자 토큰)
POST /api/chat/rooms/request-payment
Authorization: Bearer {seller_token}
{
  "roomId": 123
}

# 확인: 응답에 transactionId와 paymentUrl 포함되는지 확인

# Step 3: 채팅방 재조회 (구매자 토큰)
GET /api/chat/rooms/detail?roomId=123
Authorization: Bearer {buyer_token}

# 확인: TransactionInfo가 null이 아니고 데이터가 있는지 확인
```

### 2. DB 직접 확인

```sql
-- Transaction 생성 확인
SELECT * FROM transactions WHERE id = 456;

-- TransactionItem 생성 확인
SELECT * FROM transaction_items WHERE transaction_id = 456;

-- ChatRoom 연결 확인
SELECT * FROM chat_rooms WHERE id = 123 AND transaction_id = 456;
```

---

## 프론트엔드 수정 필요사항

### 변경 전 (클라이언트 코드)
```javascript
// ❌ 더 이상 사용하지 않음
const requestPayment = async (roomId, transactionId) => {
  await api.post('/chat/rooms/request-payment', {
    roomId,
    transactionId  // 제거 필요
  });
};
```

### 변경 후 (클라이언트 코드)
```javascript
// ✅ 새로운 방식
const requestPayment = async (roomId) => {
  const response = await api.post('/chat/rooms/request-payment', {
    roomId
  });

  // 응답에서 새로 생성된 transactionId 사용
  const { transactionId, paymentUrl, amount } = response.data.data;

  // 결제 페이지로 이동
  window.location.href = paymentUrl;
};
```

---

## 향후 개선 사항

### 1. 예약 만료 처리
현재 24시간 후 만료 설정되어 있으나, 자동 취소 로직은 미구현
→ Background Service로 만료된 예약 자동 취소 처리 필요

### 2. 수량 선택 기능
현재 티켓 1개 고정
→ 향후 수량 선택 기능 추가 시 `Quantity` 파라미터 추가 필요

### 3. 트랜잭션 처리
현재 Transaction 생성과 TransactionItem 생성이 별도 호출
→ 데이터베이스 트랜잭션으로 묶어서 원자성 보장 필요

### 4. 상태 코드 캐싱
`GetTransactionStatusByCodeAsync` 메서드는 매번 DB 조회
→ Memory Cache 적용으로 성능 개선 필요

---

## 관련 파일 목록

### 신규 생성 (2개)
- `Repository/Transaction/ITransactionItemRepository.cs`
- `Repository/Transaction/TransactionItemRepository.cs`

### 수정 (7개)
- `Repository/Transaction/ITransactionRepository.cs`
- `Repository/Transaction/TransactionRepository.cs`
- `Services/Chat/IChatService.cs`
- `Services/Chat/ChatService.cs`
- `Controllers/ChatController.cs`
- `DTO/Chat/RequestPaymentReqDto.cs`
- `Program.cs`

---

## 참고 문서
- [결제 API 문서](./PAYMENT_API_DOCS.md)
- [채팅 API 문서](./CHAT_API.postman_collection.json)
