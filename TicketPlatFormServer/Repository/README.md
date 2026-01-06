# Repository 사용 가이드

## ⚠️ IDbConnection 동시성 주의사항 (CRITICAL)

### 현재 구성

`IDbConnection`은 **Scoped**로 등록되어 **요청당 1개**만 존재합니다.

```csharp
// Program.cs
builder.Services.AddScoped<System.Data.IDbConnection>(sp =>
    new MySqlConnector.MySqlConnection(connectionString));
```

### ❌ 금지 패턴 (동시성 충돌!)

```csharp
// Service에서 병렬 Repository 호출 금지!
var tasks = new[]
{
    _eventRepository.GetEventsByCategoryId(1),
    _ticketRepository.GetTicketsByEventId(100)
};
await Task.WhenAll(tasks);  // ❌ 같은 IDbConnection 동시 접근!
```

**문제점**:
- 하나의 MySQL Connection을 여러 쿼리가 동시에 사용하려고 시도
- Connection state 충돌 발생 가능
- 예측 불가능한 에러 또는 데이터 손상 위험

### ✅ 권장 패턴 (순차 호출)

```csharp
// 순차적으로 호출
var events = await _eventRepository.GetEventsByCategoryId(1);
var tickets = await _ticketRepository.GetTicketsByEventId(100);
```

**장점**:
- Connection 안전하게 재사용
- 에러 없이 안정적인 동작

### 🔒 Repository 작성 규칙

1. **Connection 보관 금지**: Repository는 `IDbConnection`을 필드로만 받고, 열거나 닫지 않음
2. **Dispose 금지**: DI 컨테이너가 자동으로 Dispose 처리
3. **병렬 호출 금지**: 같은 요청 내에서 Repository를 동시에 호출하지 않음
4. **State 변경 금지**: Connection의 `State`, `Database` 등을 직접 변경하지 않음

### 🔄 향후 Factory 패턴 전환 시 (병렬 호출 필요 시)

`IDbConnectionFactory` 패턴으로 전환하면 Repository마다 새 연결을 생성하므로 병렬 호출이 안전해집니다.

**단, EF 트랜잭션 공유가 복잡해지므로** 아래 "EF + Dapper 트랜잭션 전략" 참고.

```csharp
// 향후 패턴 (현재는 미적용)
public class EventRepository : IEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public async Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<EventListReadModel>(...);
        return result.ToList();
    }
}
```

---

## 리소스 Dispose 책임

| 패턴 | Connection 생성 | Connection Dispose | 비고 |
|------|----------------|-------------------|------|
| **현재 (Scoped)** | DI Container | DI Container | Repository는 관여 안 함 |
| **Factory (향후)** | Repository | Repository (`using`) | Repository가 직접 Dispose |

---

## EF Core + Dapper 트랜잭션 전략

### 기본 원칙

- **단일 Repository 쿼리**: 각자 사용 (EF 또는 Dapper)
- **여러 작업의 정합성 필요 시**: EF 트랜잭션 공유 (Service 계층에서 관리)

### 트랜잭션 공유 방법 (현재 Scoped IDbConnection 패턴)

```csharp
// Service 계층에서 트랜잭션 관리
public class TransactionService
{
    private readonly TicketContext _db;
    private readonly ITicketRepository _ticketRepository;

    public async Task CreateTransactionWithTicketUpdate(
        TransactionDto dto,
        int ticketId,
        int quantity)
    {
        using var efTransaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1. EF Core 작업
            var transaction = new Transaction
            {
                UserId = dto.UserId,
                TotalAmount = dto.Amount,
                CreatedAt = DateTime.UtcNow
            };
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            // 2. Dapper 작업 - EF 트랜잭션 공유
            var connection = _db.Database.GetDbConnection();
            await connection.ExecuteAsync(
                @"UPDATE tickets
                  SET remaining_quantity = remaining_quantity - @Quantity
                  WHERE id = @TicketId",
                new { TicketId = ticketId, Quantity = quantity },
                transaction: efTransaction.GetDbTransaction()  // ✅ 중요!
            );

            await efTransaction.CommitAsync();
        }
        catch
        {
            await efTransaction.RollbackAsync();
            throw;
        }
    }
}
```

### ⚠️ Factory 패턴 전환 시 주의사항

**IDbConnectionFactory로 전환할 경우**, 매번 새 Connection을 만들면:
- ❌ **EF 트랜잭션과 별개의 Connection이 되어 트랜잭션 공유 불가**
- ✅ 해결: Factory에서도 **EF의 `GetDbConnection()`을 반환**하도록 구성

```csharp
// Factory 패턴 + 트랜잭션 공유 (향후 적용 시)
public class TicketContextConnectionFactory : IDbConnectionFactory
{
    private readonly TicketContext _context;

    public TicketContextConnectionFactory(TicketContext context)
    {
        _context = context;
    }

    public IDbConnection CreateConnection()
    {
        // ✅ EF Context의 Connection 반환 (트랜잭션 공유 가능)
        return _context.Database.GetDbConnection();
    }
}
```

### 주의사항

- 트랜잭션은 **Service 계층**에서 관리
- Repository는 트랜잭션을 받지 않음 (책임 분리)
- Factory 전환 시 **트랜잭션 공유 메커니즘 재검증 필수**

---

## Repository 계층 책임

### Repository가 해야 할 일 ✅

1. **데이터 조회**: DB에서 데이터를 가져옴
2. **데이터 저장**: DB에 데이터를 저장/수정/삭제
3. **쿼리 최적화**: 성능을 위한 SQL 작성 (Dapper 사용 가능)
4. **데이터 매핑**: DB Row → DTO/Entity 변환

### Repository가 하지 말아야 할 일 ❌

1. **비즈니스 로직**: 계산, 검증, 상태 변경 로직 (Service 담당)
2. **트랜잭션 관리**: BeginTransaction, Commit, Rollback (Service 담당)
3. **예외 변환**: `AppException`으로 래핑 (Service 담당)
4. **Connection 관리**: Open, Close, Dispose (DI Container 담당)

---

## EF Core vs Dapper 선택 기준

| 케이스 | 권장 도구 | 이유 |
|--------|----------|------|
| **단순 CRUD** | EF Core | 타입 안전성, 변경 추적, 관계 자동 로드 |
| **복잡한 JOIN (3개 이상 테이블)** | Dapper | SQL 제어, 성능 최적화 |
| **집계 쿼리 (SUM, COUNT, GROUP BY)** | Dapper | SQL 직접 작성이 간결 |
| **대량 데이터 조회** | Dapper | 메모리 효율, AsNoTracking 불필요 |
| **관계 포함 조회 (Include)** | EF Core | 자동 매핑 편리 |

---

## 새로운 Repository 추가 방법

### 1. 인터페이스 정의

```csharp
// Repository/Payment/IPaymentRepository.cs
namespace TicketPlatFormServer.Repository.Payment;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment> CreateAsync(Payment payment);
}
```

### 2. 구현 클래스 작성

**EF Core만 사용하는 경우**:

```csharp
// Repository/Payment/PaymentRepository.cs
namespace TicketPlatFormServer.Repository.Payment;

public class PaymentRepository(TicketContext db) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await db.Payments
            .Include(p => p.Method)
            .Include(p => p.Status)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }
}
```

**Dapper도 사용하는 경우**:

```csharp
// Repository/Payment/PaymentRepository.cs
namespace TicketPlatFormServer.Repository.Payment;

public class PaymentRepository(
    TicketContext db,
    IDbConnection dapper) : IPaymentRepository
{
    // EF Core 사용
    public async Task<Payment> CreateAsync(Payment payment)
    {
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    // Dapper 사용 (복잡한 쿼리)
    public async Task<List<PaymentSummaryReadModel>> GetPaymentSummaryByUser(int userId)
    {
        var result = await dapper.QueryAsync<PaymentSummaryReadModel>(
            PaymentQueries.GetPaymentSummaryByUser,
            new { UserId = userId }
        );
        return result.ToList();
    }
}

// Repository/Payment/PaymentQueries.cs (SQL 상수 분리)
namespace TicketPlatFormServer.Repository.Payment;

internal static class PaymentQueries
{
    internal const string GetPaymentSummaryByUser = @"
        SELECT
            p.id AS PaymentId,
            p.amount AS Amount,
            pm.name AS MethodName,
            ps.code AS StatusCode
        FROM payments p
        INNER JOIN payment_methods pm ON p.method_id = pm.id
        INNER JOIN payment_statuses ps ON p.status_id = ps.id
        WHERE p.user_id = @UserId
        ORDER BY p.created_at DESC";
}
```

### 3. DI 등록

```csharp
// Program.cs
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
```

---

## 참고 자료

- [EF Core 문서](https://learn.microsoft.com/ko-kr/ef/core/)
- [Dapper 문서](https://github.com/DapperLib/Dapper)
- [트랜잭션 관리 Best Practices](https://learn.microsoft.com/ko-kr/ef/core/saving/transactions)
