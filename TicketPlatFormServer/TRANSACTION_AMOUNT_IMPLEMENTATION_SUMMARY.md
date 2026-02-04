# Transaction Amount 필드 추가 구현 완료

**날짜**: 2026-02-03  
**요청자**: Flutter 팀  
**우선순위**: 🚨 긴급 (결제 기능 블로킹)

---

## 📋 요청 사항

Flutter 팀의 결제 금액 불일치 문제 해결을 위해 `GET /api/chat/rooms/detail` 응답의 **Transaction 객체에 `amount` 필드를 추가**했습니다.

---

## ✅ 구현 완료 내용

### 1. **DB 모델 변경** (`DBModel/Transaction.cs`)
```csharp
/// <summary>
/// 총 거래 금액 (TransactionItem의 TotalPrice 합계)
/// </summary>
public int? Amount { get; set; }
```

### 2. **DTO 변경** (`DTO/Chat/ChatRoomDetailRespDto.cs`)
```csharp
public class TransactionInfo
{
    public long TransactionId { get; set; }
    public string StatusCode { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public int? Amount { get; set; }  // ✅ 추가
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
```

### 3. **결제 요청 로직 수정** (`Services/Chat/ChatService.cs`)

#### RequestPayment 메서드 (Line 458-469)
```csharp
var totalAmount = room.Ticket.Price * quantity;

var transaction = new DBModel.Transaction
{
    BuyerId = room.BuyerId,
    SellerId = room.SellerId,
    StatusId = pendingStatus.Id,
    Amount = totalAmount,  // ✅ Transaction 생성 시 Amount 저장
    ReservedAt = DateTime.UtcNow,
    ReservationExpiresAt = DateTime.UtcNow.AddHours(24)
};
```

#### MapToRoomDetailDto 메서드 (Line 955-962)
```csharp
Transaction = transaction != null ? new TransactionInfo
{
    TransactionId = transaction.Id,
    StatusCode = transaction.Status?.Code ?? "",
    StatusName = transaction.Status?.NameKo ?? "",
    Amount = transaction.Amount,  // ✅ DTO 매핑 시 Amount 포함
    ConfirmedAt = transaction.ConfirmedAt,
    CancelledAt = transaction.CancelledAt
} : null,
```

### 4. **DB 마이그레이션 스크립트** (`database_history/migration_add_transaction_amount_20260203.sql`)
```sql
-- 1. Add Amount column
ALTER TABLE transaction 
ADD COLUMN Amount INT NULL COMMENT '총 거래 금액 (TransactionItem의 TotalPrice 합계)';

-- 2. Migrate existing data
UPDATE transaction t
SET t.Amount = (
    SELECT SUM(ti.TotalPrice)
    FROM transaction_item ti
    WHERE ti.TransactionId = t.Id
)
WHERE t.Amount IS NULL;

-- 3. Verification query included
```

---

## 🔧 변경된 파일 목록

| 파일 | 변경 내용 |
|------|----------|
| `DBModel/Transaction.cs` | `Amount` 필드 추가 (Line 66-69) |
| `DTO/Chat/ChatRoomDetailRespDto.cs` | `TransactionInfo.Amount` 추가 (Line 66) |
| `Services/Chat/ChatService.cs` | `RequestPayment`: Amount 저장 (Line 458)<br>`MapToRoomDetailDto`: Amount 매핑 (Line 959) |
| `database_history/migration_add_transaction_amount_20260203.sql` | DB 마이그레이션 스크립트 |

---

## ✅ 빌드 검증

```bash
dotnet build --no-restore
```

**결과**: ✅ 빌드 성공 (경고 0개, 오류 0개)

---

## 📊 API 응답 예시

### Before (문제)
```json
{
  "transaction": {
    "transactionId": 9,
    "statusCode": "pending_payment",
    "statusName": "결제 대기",
    "confirmedAt": null,
    "cancelledAt": null
  }
}
```

### After (해결) ✅
```json
{
  "transaction": {
    "transactionId": 9,
    "statusCode": "pending_payment",
    "statusName": "결제 대기",
    "amount": 180000,  // ✅ 추가됨
    "confirmedAt": null,
    "cancelledAt": null
  }
}
```

---

## 🚀 배포 전 체크리스트

### Backend 팀
- [x] 코드 변경 완료
- [x] 빌드 검증 완료 (에러 0개)
- [ ] **DB 마이그레이션 스크립트 실행**
  ```bash
  mysql -u [user] -p TicketPlatFormDB < database_history/migration_add_transaction_amount_20260203.sql
  ```
- [ ] 마이그레이션 검증 (스크립트 내 검증 쿼리 실행)
- [ ] 배포
- [ ] Flutter 팀에 배포 완료 알림

### Flutter 팀 후속 작업
- [ ] `transaction.amount` 필드 사용하도록 코드 수정
- [ ] 결제 요청 시 `amount` 값 전송
- [ ] 테스트 (단일 수량 / 복수 수량)

---

## 🎯 기대 효과

### 시나리오 1: 단일 수량 결제
```
판매자: 결제 요청 (quantity=1)
→ Transaction.Amount = 90000

구매자: GET /api/chat/rooms/detail
→ transaction.amount = 90000

구매자: POST /api/payment/request (amount=90000)
→ ✅ 성공 (90000 == 90000)
```

### 시나리오 2: 복수 수량 결제
```
판매자: 결제 요청 (quantity=2)
→ Transaction.Amount = 180000

구매자: GET /api/chat/rooms/detail
→ transaction.amount = 180000

구매자: POST /api/payment/request (amount=180000)
→ ✅ 성공 (180000 == 180000)
```

---

## 📞 문의

추가 질문이나 이슈 발생 시 백엔드 팀에 문의해주세요.

**구현 완료일**: 2026-02-03
