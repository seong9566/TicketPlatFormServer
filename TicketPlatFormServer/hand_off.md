## claude의 context을 위한 파일
## 작업 내용을 저장 하고 다음 세션에서 이어서 할 수 있도록 함

---

# 최근 작업 내역 (2026-02-02)

---

## ✅ 완료: 구매확정 API 개선 (Settlement 자동 생성 + 메시지 타입 추가 + 티켓 소유권 이전)

### 작업 개요
구매확정 시 Settlement(정산) 레코드 자동 생성, PURCHASE_CONFIRMED 메시지 타입 추가, 티켓 소유권 이전(RemainingQuantity 감소) 기능을 모두 구현하여 구매확정 플로우를 완성했습니다.

---

## Phase 1: Settlement 자동 생성

### 배경
기존에는 구매확정 시 Escrow만 해제되고 Settlement 레코드가 생성되지 않아 실제 정산 처리가 불가능했습니다.

### 구현 내용

#### 1. 정산 상태 시드 데이터 생성
**파일**: `database_history/seed_settlement_statuses.sql`

```sql
INSERT INTO settlement_statuses (code, name_ko, is_active, sort_order) VALUES
('pending', '정산 대기', true, 1),
('processing', '정산 처리중', true, 2),
('completed', '정산 완료', true, 3),
('failed', '정산 실패', true, 4);
```

#### 2. Repository 메서드 추가
**파일**: `Repository/Payment/IPaymentRepository.cs`, `PaymentRepository.cs`

새 메서드: `GetSettlementStatusByCodeAsync(string code)`
- 1시간 캐싱 적용 (IMemoryCache)
- code로 SettlementStatus 조회

#### 3. PaymentService.ReleaseEscrowAsync 개선
**파일**: `Services/Payment/PaymentService.cs`

구매확정 시 자동으로 수행되는 작업:
```csharp
// 3-5. Settlement 레코드 생성
var settlement = new Settlement
{
    TransactionId = transactionId,
    SellerId = transaction.SellerId,
    Amount = escrow.Amount,              // 총 금액
    Fee = escrow.FeeAmount,              // 수수료
    NetAmount = escrow.SellerAmount,     // 순 정산 금액
    BankAccountId = defaultBankAccount?.Id ?? 0,
    StatusId = settlementStatusPending.Id,
    ScheduledAt = DateTime.UtcNow.AddDays(1),  // D+1 정산
    CreatedAt = DateTime.UtcNow
};
await context.Settlements.AddAsync(settlement);
```

**주요 로직**:
- 판매자의 인증된(Verified) 계좌를 BankAccount에서 조회
- 계좌 없으면 `BankAccountId = 0`으로 설정, WARNING 로그 남김
- 정산 예정일은 D+1 (내일)
- 정산 상태는 `pending`

#### 4. DB 복원 스크립트 업데이트
**파일**: `database_history/db_restore.sh`, `db_restore.bat`

settlement_statuses 시드 자동 적용 추가:
```bash
mysql ... < seed_settlement_statuses.sql
```

---

## Phase 2: 구매확정 메시지 타입 추가

### 배경
결제 완료 시 PAYMENT_SUCCESS 메시지가 자동 생성되는 것과 동일하게, 구매확정 시에도 PURCHASE_CONFIRMED 메시지를 자동 생성하여 일관된 UX 제공.

### 구현 내용

#### 1. MessageType enum 확장
**파일**: `Enum/MessageType.cs`

```csharp
public enum MessageType
{
    TEXT,
    IMAGE,
    PAYMENT_REQUEST,
    PAYMENT_SUCCESS,
    PURCHASE_CONFIRMED  // 신규 추가
}
```

#### 2. ChatService.ConfirmPurchase 개선
**파일**: `Services/Chat/ChatService.cs`

**변경 전**:
```csharp
var systemMessage = await chatRepo.CreateMessage(req.RoomId, req.UserId, "구매가 확정되었습니다.", null);
```

**변경 후**:
```csharp
// PURCHASE_CONFIRMED 메시지 생성
var message = await chatRepo.CreateMessage(
    roomId: req.RoomId,
    senderId: req.UserId,
    message: null,  // 프론트엔드가 UI 카드 렌더링
    imageUrl: null,
    type: MessageType.PURCHASE_CONFIRMED
);

// SignalR 실시간 브로드캐스트
var signalDto = new NewMessageSignalDto
{
    MessageId = message.Id,
    RoomId = req.RoomId,
    SenderId = req.UserId,
    SenderNickname = room.Buyer?.UserProfile?.Nickname ?? "구매자",
    Message = null,
    Type = MessageType.PURCHASE_CONFIRMED.ToString(),
    CreatedAt = message.CreatedAt ?? DateTime.UtcNow
};

// 1. 채팅방 참여자들에게
await hubContext.Clients.Group($"ChatRoom_{req.RoomId}")
    .SendAsync("ReceiveMessage", signalDto);

// 2. 구매자/판매자 개인 알림
await hubContext.Clients.Group($"User_{room.BuyerId}")
    .SendAsync("NewMessage", signalDto);
await hubContext.Clients.Group($"User_{room.SellerId}")
    .SendAsync("NewMessage", signalDto);
```

**추가 수정**:
- ChatService에 `IHubContext<ChatHub>` DI 추가
- 관련 using 문 추가 (`Microsoft.AspNetCore.SignalR`, `TicketPlatFormServer.Enum`, `TicketPlatFormServer.Hubs`)

#### 3. ChatController 중복 코드 제거
**파일**: `Controllers/ChatController.cs`

ConfirmPurchase 엔드포인트에서 SignalR 브로드캐스트 코드 제거 (Service에서 처리하므로)

---

## Phase 3: 티켓 소유권 이전 (RemainingQuantity 감소)

### 배경
구매확정 시 티켓의 `remaining_quantity`를 감소시켜야 실제 판매된 티켓 수가 반영됩니다.

### 구현 내용

**파일**: `Services/Payment/PaymentService.cs`의 `ReleaseEscrowAsync` 메서드

```csharp
// 3-4-1. 티켓 소유권 이전 (RemainingQuantity 감소)
foreach (var item in transaction.TransactionItems)
{
    var ticket = await context.Tickets.FindAsync((int)item.TicketId);
    if (ticket != null)
    {
        ticket.RemainingQuantity -= item.Quantity;
        
        if (ticket.RemainingQuantity < 0)
        {
            logger.LogWarning("Ticket RemainingQuantity < 0 - TicketId: {TicketId}, RemainingQuantity: {RemainingQuantity}",
                ticket.Id, ticket.RemainingQuantity);
            ticket.RemainingQuantity = 0;
        }
        
        logger.LogInformation("Ticket 소유권 이전 - TicketId: {TicketId}, Quantity: {Quantity}, RemainingQuantity: {RemainingQuantity}",
            ticket.Id, item.Quantity, ticket.RemainingQuantity);
    }
}
await context.SaveChangesAsync();
```

**안전장치**:
- RemainingQuantity가 음수가 되면 0으로 설정하고 WARNING 로그 남김
- TransactionItems를 순회하며 모든 티켓의 수량 감소

---

## 완성된 구매확정 플로우

```
1. 판매 등록
   └─ Transaction + ChatRoom 생성

2. 결제 요청
   └─ PAYMENT_REQUEST 메시지 생성

3. 결제 완료
   └─ Payment, Escrow 생성
   └─ PAYMENT_SUCCESS 메시지 자동 생성

4. 구매 확정 ⭐ (이번 작업)
   ├─ Escrow 해제 (status: released)
   ├─ Transaction 상태 변경 (status: confirmed, confirmed_at: NOW)
   ├─ Settlement 자동 생성 ⭐
   │  ├─ Amount: 총 금액
   │  ├─ Fee: 수수료
   │  ├─ NetAmount: 순 정산 금액
   │  ├─ BankAccountId: 판매자 인증된 계좌
   │  ├─ StatusId: pending
   │  └─ ScheduledAt: D+1
   ├─ Ticket.RemainingQuantity 감소 ⭐
   ├─ PURCHASE_CONFIRMED 메시지 자동 생성 ⭐
   ├─ SignalR 실시간 알림 (채팅방 + 개인)
   └─ ChatRoom 잠금
```

---

## 변경된 파일 목록

### 신규 생성 (1개)
1. `database_history/seed_settlement_statuses.sql`

### 수정 (9개)
1. `Enum/MessageType.cs` - PURCHASE_CONFIRMED 추가
2. `Repository/Payment/IPaymentRepository.cs` - GetSettlementStatusByCodeAsync 추가
3. `Repository/Payment/PaymentRepository.cs` - 메서드 구현
4. `Services/Payment/PaymentService.cs` - Settlement 생성 + 티켓 소유권 이전
5. `Services/Chat/ChatService.cs` - IHubContext 주입 + PURCHASE_CONFIRMED 메시지 생성
6. `Controllers/ChatController.cs` - 중복 SignalR 코드 제거
7. `database_history/db_restore.sh` - settlement_statuses 시드 적용
8. `database_history/db_restore.bat` - settlement_statuses 시드 적용

---

## 빌드 및 검증 상태

✅ **빌드 성공** (0 errors, 0 warnings)
- 경과 시간: 00:00:02.31

---

## 다음 작업 (권장)

### End-to-End 테스트
```bash
# 구매확정 API 호출
POST /api/chat/rooms/confirm-purchase
{
  "RoomId": 1,
  "TransactionId": 1
}

# 확인 사항:
# 1. settlements 테이블에 레코드 생성되었는지
# 2. settlement.status_id = pending (정산 대기)
# 3. settlement.scheduled_at = D+1
# 4. tickets.remaining_quantity 감소되었는지
# 5. chat_messages에 PURCHASE_CONFIRMED 타입 메시지 생성되었는지
# 6. SignalR로 실시간 알림 전송되었는지
```

### DB 덤프 업데이트 (선택)
```bash
cd database_history

# DB 복원 테스트 (settlement_statuses 시드 자동 적용)
./db_restore.sh

# 새 덤프 생성
mysqldump -h 127.0.0.1 -P 3306 -u root -p'stecdev1234!' \
  --databases TicketPlatFormDB \
  --routines --triggers --events \
  --single-transaction \
  --set-gtid-purged=OFF \
  > TicketPlatFormDB_dump_new.sql
```

---

## 주요 설계 결정

### 1. Settlement 자동 생성
- **생성 시점**: 구매확정 시 (결제 완료 시 ✗)
- **이유**: 실제 거래 확정 후에만 정산 필요
- **판매자 계좌 없을 시**: BankAccountId=0, WARNING 로그, 정산 레코드는 생성

### 2. PURCHASE_CONFIRMED 메시지
- **message=null**: 프론트엔드가 type 보고 UI 카드 렌더링
- **일관성**: PAYMENT_SUCCESS와 동일한 패턴
- **SignalR**: 채팅방 그룹 + 개인 알림 동시 전송

### 3. 티켓 소유권 이전
- **TransactionItems 기반**: 여러 티켓 동시 구매 지원
- **음수 방지**: RemainingQuantity < 0이면 0으로 설정 + WARNING
- **트랜잭션 내**: DB 트랜잭션 내에서 원자적 처리

---

**마지막 업데이트**: 2026-02-02
**상태**: ✅ Phase 1, 2, 3 모두 완료
