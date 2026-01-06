# Repository 패턴 개선 계획 (v2.1 FINAL - Codex 검증 완료)

**작성일**: 2026-01-06
**작성자**: Claude Code
**1차 검증**: Codex (2026-01-06) ✅
**2차 검증**: Codex (2026-01-06) ✅
**최종 승인**: 대기 중

---

## 📋 목표 (Objectives)

현재 Dapper + EF Core 혼용 방식을 **유지**하면서, Codex 2차 피드백까지 반영하여 다음을 개선:

### 우선순위 높음 (Critical)
1. ✅ **보안 강화**: EnableSensitiveDataLogging 환경별 처리 + EF 로깅 파이프라인 점검
2. ✅ **동시성 안전성**: IDbConnection 동시 사용 가이드라인 강화
3. ✅ **Namespace 일관성**: UserRepository namespace 추가

### 우선순위 중간 (Important)
4. ✅ **계층 분리**: DTO → ReadModel 이동 (`Repository/ReadModels/`)
5. ✅ **예외 처리 전략**: Service에서 경계 예외만 AppException 변환
6. ✅ **트랜잭션 전략**: EF + Dapper 혼용 시 정합성 보장 + Factory 호환성

### 우선순위 낮음 (Nice to have)
7. ✅ **코드 중복 제거**: Primary Constructor + Partial 클래스 규칙
8. ✅ **디렉토리 구조**: EventRepo → Event 통일
9. ✅ **명명 규칙**: 메서드명 통일

---

## 🔴 Phase 0: 보안 및 안전성 수정 (CRITICAL - 즉시 적용)

### 0.1 EnableSensitiveDataLogging 환경별 처리 (Codex v2 반영)

**파일**: `Program.cs:34-40`

**변경 전**:
```csharp
.LogTo(Console.WriteLine, ...)
.EnableSensitiveDataLogging()  // ❌ 항상 켜짐!
```

**변경 후 (Codex 최종 권장)**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// ... EF Core 설정
builder.Services.AddDbContext<TicketContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
            mySqlOptions.CommandTimeout(60);
            mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

    // ✅ 환경별 로깅 설정
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, new[]
        {
            DbLoggerCategory.Database.Command.Name,
            DbLoggerCategory.Database.Transaction.Name,
            DbLoggerCategory.Database.Connection.Name
        }, LogLevel.Warning);

        options.EnableSensitiveDataLogging();  // 개발 환경에서만
    }
});
```

**Codex 피드백 반영**:
- ❌ `#if DEBUG` (빌드 구성) 대신
- ✅ `builder.Environment.IsDevelopment()` (런타임 환경) 사용

**추가 점검 항목**:
- [ ] Serilog/NLog 사용 시 SQL 파라미터 로깅 비활성화 확인
- [ ] SQL Interceptor 사용 시 민감 정보 필터링 확인
- [ ] 프로덕션 로그에 `@Email`, `@Phone` 같은 파라미터 미노출 확인

**예상 시간**: 10분
**테스트**:
- Development: SQL 파라미터 로그 출력 확인
- Production: SQL 파라미터 로그 미출력 확인

---

### 0.2 IDbConnection 동시성 가이드라인 강화 (Codex v2 반영)

**파일**: `Repository/README.md` (신규 생성)

```markdown
# Repository 사용 가이드

## ⚠️ IDbConnection 동시성 주의사항 (CRITICAL)

### 현재 구성
`IDbConnection`은 **Scoped**로 등록되어 **요청당 1개**만 존재합니다.

\```csharp
// Program.cs
builder.Services.AddScoped<System.Data.IDbConnection>(sp =>
    new MySqlConnector.MySqlConnection(connectionString));
\```

### ❌ 금지 패턴 (동시성 충돌!)

\```csharp
// Service에서 병렬 Repository 호출 금지!
var tasks = new[]
{
    _eventRepository.GetEventsByCategoryId(1),
    _ticketRepository.GetTicketsByEventId(100)
};
await Task.WhenAll(tasks);  // ❌ 같은 IDbConnection 동시 접근!
\```

### ✅ 권장 패턴 (순차 호출)

\```csharp
// 순차적으로 호출
var events = await _eventRepository.GetEventsByCategoryId(1);
var tickets = await _ticketRepository.GetTicketsByEventId(100);
\```

### 🔒 Repository 작성 규칙

1. **Connection 보관 금지**: Repository는 IDbConnection을 필드로만 받고, 열거나 닫지 않음
2. **Dispose 금지**: DI 컨테이너가 자동으로 Dispose 처리
3. **병렬 호출 금지**: 같은 요청 내에서 Repository를 동시에 호출하지 않음

### 🔄 향후 Factory 패턴 전환 시 (병렬 호출 필요 시)

IDbConnectionFactory 패턴으로 전환하면 Repository마다 새 연결을 생성하므로 병렬 호출이 안전해집니다.
단, **EF 트랜잭션 공유가 복잡해지므로** Phase 2.1 참고.

\```csharp
// 향후 패턴 (현재는 미적용)
using var connection = _connectionFactory.CreateConnection();
var result = await connection.QueryAsync<T>(...);
\```

---

## 리소스 Dispose 책임

| 패턴 | Connection 생성 | Connection Dispose | 비고 |
|------|----------------|-------------------|------|
| **현재 (Scoped)** | DI Container | DI Container | Repository는 관여 안 함 |
| **Factory (향후)** | Repository | Repository (`using`) | Repository가 직접 Dispose |
```

**Codex 피드백 반영**:
- "동일 요청 내 동시 Repository 호출 금지" 명문화
- "Repository는 connection을 보관/공유하지 말 것" 추가
- Dispose 책임 명확화

**예상 시간**: 30분
**리스크**: 낮음 (문서화만)

---

### 0.3 UserRepository Namespace 추가

**파일**: `Repository/User/UserRepository.cs`, `Repository/User/IUserRepository.cs`

**변경 전**:
```csharp
// UserRepository.cs (namespace 없음 - GLOBAL!)
using System.Data;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

public class UserRepository : IUserRepository
{
    // ...
}
```

**변경 후**:
```csharp
using System.Data;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.User;  // ✅ 추가

public class UserRepository : IUserRepository
{
    // ...
}
```

**IUserRepository.cs도 동일 변경**:
```csharp
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.User;  // ✅ 추가

public interface IUserRepository
{
    // ...
}
```

**관련 파일 수정**:
- `Program.cs`: `using TicketPlatFormServer.Repository.User;` 추가
- `UserService.cs`: using 구문 확인

**예상 시간**: 15분
**테스트**: 컴파일 성공 확인

---

## Phase 1: 계층 분리 (Layer Separation)

### 1.1 DTO → ReadModel 이동 (Codex v2 반영 - 위치 변경)

**목적**: Repository와 프레젠테이션 계층 결합 제거

**Codex 피드백**:
> `DTO/ReadModels/`는 프레젠테이션 DTO와 혼동 가능.
> `Repository/ReadModels/`가 더 명확함.

**변경 대상**:
| 기존 이름 | 새 이름 | 기존 위치 | 새 위치 |
|----------|---------|----------|---------|
| `EventListRespDto` | `EventListReadModel` | `DTO/` | `Repository/ReadModels/` |
| `EventDetailRespDto` | `EventDetailReadModel` | `DTO/` | `Repository/ReadModels/` |
| `TicketListRespDto` | `TicketListReadModel` | `DTO/` | `Repository/ReadModels/` |

**디렉토리 구조 (최종)**:
```
Repository/
├── ReadModels/       ← 신규! Repository 전용 반환 모델
│   ├── EventListReadModel.cs
│   ├── EventDetailReadModel.cs
│   ├── TicketListReadModel.cs
│   └── SellerInfoReadModel.cs
├── User/
├── Event/
├── Ticket/
├── Home/
└── README.md

DTO/
├── Request/          # API 요청 DTO
│   ├── RegisterUserReqDto.cs
│   └── LoginReqDto.cs
└── Response/         # API 응답 DTO
    ├── RegisterUserRespDto.cs
    ├── LoginUserRespDto.cs
    └── ApiResponse.cs
```

**변경 절차**:
1. `Repository/ReadModels/` 디렉토리 생성
2. 기존 `*RespDto` 클래스를 `Repository/ReadModels/`로 이동하고 `*ReadModel`로 이름 변경
3. namespace 수정: `namespace TicketPlatFormServer.Repository.ReadModels;`
4. Repository 인터페이스 및 구현체 수정
5. Service 계층에서 ReadModel → RespDto 변환 로직 추가 (필요 시)
6. 컴파일 확인 및 테스트

**Codex 시간 조정 반영**: 1.5h → **2-3시간** (영향 범위 고려)
**리스크**: 중간 (Service 계층 수정 필요, 충분한 버퍼 확보)

---

### 1.2 AppException InnerException 보존 + 원칙 명확화 (Codex v2 반영)

**파일**: `Common/Exception/AppException.cs`

```csharp
using System.Net;

namespace TicketPlatFormServer.Common;

public class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }

    // 기존 생성자
    public AppException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
    }

    // InnerException 보존 생성자 (신규)
    public AppException(
        string message,
        Exception innerException,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
```

**Service 예외 처리 원칙 (Codex v2 명확화)**:

```csharp
// EventService.cs
public async Task<EventDetailRespDto> GetEventDetail(int eventId)
{
    // 1. 입력 검증 - AppException (InnerException 없음)
    if (eventId <= 0)
    {
        throw new AppException(
            "유효하지 않은 이벤트 ID입니다.",
            HttpStatusCode.BadRequest
        );
    }

    try
    {
        var eventDetail = await _repo.GetEventDetailById(eventId);

        // 2. 비즈니스 예외 (경계) - AppException (InnerException 없음)
        if (eventDetail == null)
        {
            throw new AppException(
                "이벤트를 찾을 수 없습니다.",
                HttpStatusCode.NotFound
            );
        }

        return MapToRespDto(eventDetail);
    }
    catch (AppException)
    {
        // 3. 비즈니스 예외는 그대로 전파
        throw;
    }
    catch (Exception ex)
    {
        // 4. 인프라 예외는 InnerException 보존하며 래핑
        throw new AppException(
            "이벤트 조회 중 오류가 발생했습니다.",
            ex,  // ✅ InnerException 보존
            HttpStatusCode.InternalServerError
        );
    }
}
```

**GlobalExceptionMiddleware 개선**:

```csharp
// GlobalExceptionMiddleware.cs
catch (AppException e)
{
    _logger.LogWarning(e, "[AppException] {Message} | Path: {Path}",
        e.Message,
        context.Request.Path);

    // InnerException 로깅 (있을 경우)
    if (e.InnerException != null)
    {
        _logger.LogWarning("[InnerException] Type: {Type}, Message: {Message}",
            e.InnerException.GetType().Name,
            e.InnerException.Message);
    }

    context.Response.StatusCode = (int)e.StatusCode;
    var response = new ApiResponse<object>(
        message: e.Message,  // ✅ 사용자에게는 비즈니스 메시지만
        data: null,
        statusCode: e.StatusCode
    );

    await context.Response.WriteAsJsonAsync(response);
}
catch (Exception e)
{
    _logger.LogError(e, "[Exception] {Message} | Path: {Path}",
        e.Message,
        context.Request.Path);

    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    var response = new ApiResponse<object>(
        message: "서버 내부 오류가 발생했습니다.",  // ✅ 내부 상세는 숨김
        data: null,
        statusCode: HttpStatusCode.InternalServerError
    );

    await context.Response.WriteAsJsonAsync(response);
}
```

**Codex 원칙 명확화**:
- ✅ Service에서 **경계(입력검증/NotFound)** 만 AppException으로 생성
- ✅ 나머지는 원 예외를 보존 (래핑 시 inner 포함)
- ✅ Middleware는 **클라이언트에 내부 메시지 절대 노출 금지**
- ✅ 로그에는 예외 객체(`LogError(ex, ...)`) 전체 기록

**예상 시간**: 1시간

---

## Phase 2: 트랜잭션 전략 (Transaction Strategy)

### 2.1 EF + Dapper 트랜잭션 공유 전략 (Codex v2 Factory 호환성 추가)

**파일**: `Repository/README.md` (추가)

```markdown
## EF Core + Dapper 트랜잭션 전략

### 기본 원칙
- **단일 Repository 쿼리**: 각자 사용 (EF 또는 Dapper)
- **여러 작업의 정합성 필요 시**: EF 트랜잭션 공유 (Service 계층에서 관리)

### 트랜잭션 공유 방법 (현재 Scoped IDbConnection 패턴)

\```csharp
// Service 계층에서 트랜잭션 관리
public async Task CreateTransactionWithTicketUpdate(...)
{
    using var efTransaction = await _db.Database.BeginTransactionAsync();

    try
    {
        // 1. EF Core 작업
        var transaction = new Transaction { ... };
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        // 2. Dapper 작업 - EF 트랜잭션 공유
        var connection = _db.Database.GetDbConnection();
        await connection.ExecuteAsync(
            "UPDATE tickets SET remaining_quantity = remaining_quantity - @Quantity WHERE id = @TicketId",
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
\```

### ⚠️ Factory 패턴 전환 시 주의사항 (Codex v2 추가)

**IDbConnectionFactory로 전환할 경우**, 매번 새 Connection을 만들면:
- ❌ **EF 트랜잭션과 별개의 Connection이 되어 트랜잭션 공유 불가**
- ✅ 해결: Factory에서도 **EF의 GetDbConnection()을 반환**하도록 구성

\```csharp
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
\```

### 주의사항
- 트랜잭션은 **Service 계층**에서 관리
- Repository는 트랜잭션을 받지 않음 (책임 분리)
- Factory 전환 시 **트랜잭션 공유 메커니즘 재검증 필수**
```

**Codex 피드백 반영**:
- Factory 전환 시 EF 트랜잭션 공유 어려움 명시
- 같은 Connection/DbTransaction 공유 필요성 강조
- 해결 방안 (EF의 GetDbConnection 사용) 제시

**예상 시간**: 40분
**현재 적용**: 문서화만 (실제 구현은 티켓 구매 기능 개발 시)

---

## Phase 3: 코드 중복 제거 (DRY)

### 3.1 Primary Constructor + Partial 클래스 규칙 (Codex v2 반영)

**목적**: BaseRepository 강제 의존성 주입 문제 해결

**선택**: **Primary Constructor 패턴** (C# 12)

**기본 패턴**:
```csharp
// UserRepository.cs - EF만 사용
namespace TicketPlatFormServer.Repository.User;

public class UserRepository(TicketContext db) : IUserRepository
{
    public async Task<User?> GetByEmail(string email)
    {
        return await db.Users  // ✅ db 직접 사용
            .Include(u => u.Provider)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Email == email && x.IsDeleted == false);
    }
}
```

**Partial 클래스와의 결합 규칙 (Codex v2 명확화)**:

#### ✅ 옵션 A: SQL 상수를 Static Class로 분리 (권장)

```csharp
// EventRepository.cs
namespace TicketPlatFormServer.Repository.Event;

public class EventRepository(IDbConnection dapper) : IEventRepository
{
    public async Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId)
    {
        var result = await dapper.QueryAsync<EventListReadModel>(
            EventQueries.GetEventsByCategoryId,  // ✅ Static class 참조
            new { CategoryId = categoryId }
        );
        return result.ToList();
    }
}

// EventQueries.cs (별도 파일)
namespace TicketPlatFormServer.Repository.Event;

internal static class EventQueries
{
    internal const string GetEventsByCategoryId = @"
        SELECT
            e.id AS EventId,
            e.title AS EventTitle,
            ...
        FROM events e
        LEFT JOIN artists a ON e.artist_id = a.id
        WHERE e.category_id = @CategoryId
          AND e.is_active = 1
        ORDER BY e.sort_order ASC, e.start_at ASC";

    internal const string GetEventDetailById = @"
        SELECT ...";
}
```

**장점**:
- Primary Constructor 파라미터와 SQL 상수가 분리됨
- 쿼리만 모아서 보기 쉬움
- Partial 클래스 불필요

---

#### ✅ 옵션 B: 명시적 필드로 저장 (Partial 유지)

```csharp
// EventRepository.cs
namespace TicketPlatFormServer.Repository.Event;

public partial class EventRepository(IDbConnection dapper) : IEventRepository
{
    // ✅ Primary Constructor 파라미터를 명시적 필드로 저장
    private readonly IDbConnection _dapper = dapper;

    public async Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId)
    {
        var result = await _dapper.QueryAsync<EventListReadModel>(
            SqlGetEventsByCategoryId,
            new { CategoryId = categoryId }
        );
        return result.ToList();
    }
}

// EventRepository.Sql.cs
namespace TicketPlatFormServer.Repository.Event;

public partial class EventRepository
{
    private const string SqlGetEventsByCategoryId = @"...";
    private const string SqlGetEventDetailById = @"...";
}
```

**장점**:
- 기존 Partial 패턴 유지
- `_dapper` 필드명으로 명확성 확보

---

**팀 규칙 제안 (Codex 피드백 반영)**:
1. **SQL 쿼리가 2개 이상 또는 20줄 이상**: 분리 (옵션 A 또는 B)
2. **SQL 쿼리가 1개이고 짧음**: 메서드 내부 또는 상수로 유지
3. **Primary Constructor 사용 시**: 옵션 A (Static Class) 권장

**적용 대상**:
- `UserRepository`: Primary Constructor, Partial 불필요
- `EventRepository`: Primary Constructor + 옵션 A (Static Class)
- `TicketRepository`: Primary Constructor + 옵션 A (Static Class)
- `HomeRepository`: 확인 후 결정

**예상 시간**: 2시간 (Static Class 생성 + 모든 Repository 적용)

---

### 3.2 Partial 클래스 사용 기준 재정의

**새로운 기준** (Codex 피드백 반영):
- ❌ **모든 Repository에 강제 적용하지 않음**
- ✅ **SQL 쿼리가 많거나 길 때만** 분리
- ✅ **Primary Constructor와 함께 사용 시** Static Class 권장

**예상 시간**: 20분 (기준 문서화)

---

## Phase 4: 디렉토리 및 명명 규칙 (Structure & Naming)

### 4.1 디렉토리 구조 통일

**변경 전**:
```
Repository/
├── User/
├── EventRepo/  ← 불일치
├── Ticket/
└── Home/
```

**변경 후**:
```
Repository/
├── ReadModels/     ← 신규 (Phase 1.1)
├── User/
├── Event/          ← 통일
├── Ticket/
├── Home/
└── README.md       ← 신규 (Phase 0.2, 2.1)
```

**변경 절차**:
1. `Repository/EventRepo/` → `Repository/Event/`로 이름 변경
2. Namespace 수정: `TicketPlatFormServer.Repository.EventRepo` → `.Event`
3. `Program.cs` using 구문 수정
4. `EventService.cs` using 구문 수정
5. 컴파일 확인

**예상 시간**: 30분

---

### 4.2 메서드명 통일 (낮은 우선순위)

**변경 대상**:
- `GetByEmail` → `GetUserByEmail`

**변경 위치**:
- `IUserRepository.cs`
- `UserRepository.cs`
- `UserService.cs` (3곳)

**예상 시간**: 20분

---

## Phase 5: 유틸리티 및 문서화 (Utilities & Documentation)

### 5.1 JSON 파싱 헬퍼 + 로깅 보안 강화 (Codex v2 반영)

**파일**: `Common/Helpers/JsonHelper.cs`

```csharp
namespace TicketPlatFormServer.Common.Helpers;

public static class JsonHelper
{
    public static T? SafeDeserialize<T>(
        string? json,
        T? defaultValue = default,
        ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return defaultValue;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            // ✅ Codex v2: JSON 전체 로깅 금지 (보안)
            // ❌ logger?.LogWarning(ex, "JSON 파싱 실패: {Json}", json);

            // ✅ 길이와 타입만 로깅
            logger?.LogWarning(ex,
                "JSON 파싱 실패: Type={Type}, Length={Length}, Error={Error}",
                typeof(T).Name,
                json.Length,
                ex.Message);

            return defaultValue;
        }
    }

    /// <summary>
    /// JSON 해시값 생성 (로깅용)
    /// </summary>
    public static string GetJsonHash(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "empty";

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash)[..16];  // 처음 16자만
    }
}
```

**Codex 피드백 반영**:
- ❌ `{Json}` 전체 로깅 금지 (운영 환경에서 개인정보 노출 위험)
- ✅ 타입, 길이, 에러 메시지만 로깅
- ✅ 디버깅 필요 시 해시값 사용

**사용 예시**:
```csharp
// TicketRepository에서
var seatFeatures = JsonHelper.SafeDeserialize<List<string>>(
    row.SeatFeatures?.ToString(),
    new List<string>(),
    _logger
);

// 디버깅 필요 시
_logger.LogDebug("SeatFeatures Hash: {Hash}",
    JsonHelper.GetJsonHash(row.SeatFeatures?.ToString()));
```

**예상 시간**: 50분

---

### 5.2 Repository 가이드 문서 통합

**파일**: `Repository/README.md` (Phase 0.2, 2.1, 3.1 통합)

전체 내용:
- Repository 계층 책임 및 원칙
- IDbConnection 동시성 주의사항 (Phase 0.2)
- EF Core vs Dapper 선택 기준
- EF + Dapper 트랜잭션 공유 방법 (Phase 2.1)
- Primary Constructor + Partial/Static Class 규칙 (Phase 3.1)
- 예외 처리 원칙
- CancellationToken 사용 가이드 (Codex v2 추가)
- 새로운 Repository 추가 방법

**예상 시간**: 통합 작업 30분 (각 Phase에서 이미 작성)

---

## 🚀 실행 계획 (우선순위 재정렬 - Codex v2 시간 조정)

### Step 0: 긴급 보안 수정 (45분)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 0.1 | EnableSensitiveDataLogging 환경별 처리 | 🔴 긴급 | 낮음 | 10분 |
| 0.2 | IDbConnection 가이드라인 강화 | 🔴 긴급 | 낮음 | 20분 |
| 0.3 | UserRepository Namespace 추가 | 🔴 긴급 | 낮음 | 15분 |

**즉시 실행 필요!**

---

### Step 1: 계층 분리 (3.5-4시간, Codex 시간 조정)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 1.1 | ReadModel 이동 (DTO → Repository/ReadModels/) | 높음 | 중간 | **2-3시간** ⬆️ |
| 1.2 | AppException InnerException + 원칙 명확화 | 높음 | 낮음 | 1시간 |
| 2.1 | 트랜잭션 전략 + Factory 호환성 문서화 | 높음 | 낮음 | 30분 |

---

### Step 2: 코드 품질 개선 (3시간)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 3.1 | Primary Constructor + Static Class 패턴 | 중간 | 중간 | 2시간 |
| 4.1 | 디렉토리 구조 통일 (EventRepo → Event) | 중간 | 중간 | 30분 |
| 5.1 | JSON 헬퍼 + 로깅 보안 강화 | 중간 | 낮음 | 30분 |

---

### Step 3: 정리 작업 (Optional, 50분)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 3.2 | Partial 클래스 기준 문서화 | 낮음 | 낮음 | 20분 |
| 4.2 | 메서드명 통일 | 낮음 | 낮음 | 20분 |
| 5.2 | Repository README 통합 정리 | 낮음 | 낮음 | 10분 |

---

**총 예상 시간**: 약 **7.5-8.5시간** (Step 3 제외 시 6.5-7.5시간)
**Codex 조정 반영**: Step 1.1 시간 +0.5-1.5h 증가

---

## ⚠️ 추가 리스크 및 대응 (Codex v2 신규 항목)

| 리스크 | 확률 | 영향 | 대응 방안 (Codex 제안) |
|--------|------|------|---------------------|
| Factory + 트랜잭션 호환성 | 낮음 | 높음 | Phase 2.1에 EF GetDbConnection 사용 명시 |
| IDbConnection Dispose 책임 불명확 | 중간 | 중간 | Phase 0.2에 책임 표 추가 |
| CancellationToken 부재 | 낮음 | 중간 | Repository README에 가이드 추가 (향후 적용) |
| JSON 로깅 개인정보 노출 | 중간 | 높음 | Phase 5.1에서 전체 JSON 로깅 금지 |
| ReadModel 이름 변경 범위 과소평가 | 높음 | 중간 | Step 1.1 시간 버퍼 +1h |
| #if DEBUG vs Environment 혼동 | 낮음 | 높음 | Phase 0.1에서 Environment 사용 명시 |

---

## ✅ 완료 기준 (Definition of Done)

### Step 0 (긴급)
- [ ] `EnableSensitiveDataLogging`이 `IsDevelopment()` 조건 내에만 있음
- [ ] EF Core 로깅 파이프라인에서 파라미터 노출 없음 (프로덕션)
- [ ] UserRepository에 `namespace TicketPlatFormServer.Repository.User;` 추가
- [ ] `Repository/README.md`에 IDbConnection 동시성 금지 규칙 명시
- [ ] Dispose 책임 표 작성
- [ ] 컴파일 성공

### Step 1 (계층 분리)
- [ ] `Repository/ReadModels/` 디렉토리 생성 및 파일 이동
- [ ] Repository 인터페이스가 `*ReadModel` 반환
- [ ] `AppException`에 InnerException 생성자 추가
- [ ] Service 예외 처리 원칙 적용 (경계만 AppException)
- [ ] Middleware InnerException 로깅 추가
- [ ] 트랜잭션 전략 + Factory 호환성 문서 작성
- [ ] 기존 API 엔드포인트 테스트 통과

### Step 2 (코드 품질)
- [ ] 모든 Repository가 Primary Constructor 사용
- [ ] SQL 쿼리는 Static Class로 분리 (EventQueries, TicketQueries)
- [ ] `Repository/EventRepo/` → `Repository/Event/`로 변경
- [ ] JsonHelper에 보안 로깅 적용 (전체 JSON 로깅 금지)
- [ ] Repository README 통합 완료

### 검증
- [ ] 컴파일 에러 0개
- [ ] Swagger UI에서 모든 API 정상 동작
- [ ] Development 환경: SQL 파라미터 로그 출력
- [ ] Production 빌드: SQL 파라미터 로그 미출력
- [ ] JSON 파싱 실패 시 타입/길이만 로깅 (JSON 내용 미로깅)

---

## 📝 Codex 피드백 반영 요약 (v2 → v2.1)

| Codex v2 지적사항 | 반영 여부 | v2.1 변경 내용 |
|------------------|-----------|---------------|
| `#if DEBUG` → `Environment.IsDevelopment()` | ✅ 반영 | Phase 0.1 코드 수정 |
| EF 로깅 파이프라인 전체 점검 | ✅ 반영 | Phase 0.1 체크리스트 추가 |
| IDbConnection 가이드라인 강화 | ✅ 반영 | Phase 0.2 문서 강화 |
| ReadModels 위치 변경 | ✅ 반영 | Phase 1.1 (DTO/ → Repository/ReadModels/) |
| 예외 처리 원칙 명확화 | ✅ 반영 | Phase 1.2 원칙 추가 |
| Primary Constructor + Partial 규칙 | ✅ 반영 | Phase 3.1 Static Class 패턴 추가 |
| Step 1.1 시간 조정 | ✅ 반영 | 1.5h → 2-3h |
| Factory + 트랜잭션 호환성 | ✅ 반영 | Phase 2.1 주의사항 추가 |
| IDbConnection Dispose 책임 | ✅ 반영 | Phase 0.2 표 추가 |
| CancellationToken 가이드 | ✅ 반영 | Phase 5.2 README에 추가 |
| JSON 로깅 보안 | ✅ 반영 | Phase 5.1 전체 JSON 로깅 금지 |

---

## 🎯 최종 승인 후 실행 순서

1. **Step 0 (긴급)**: 보안 수정 → 즉시 적용 및 테스트
2. **Step 1 (계층)**: ReadModel 이동 + 예외 처리 → 기능 테스트
3. **Step 2 (품질)**: Primary Constructor + Static Class → 리팩토링 완료
4. **Step 3 (선택)**: 필요 시 추가 정리

**예상 완료 시간**: 1-2일 (실제 개발 시간 6.5-8.5시간 + 테스트)

---

**계획 문서 v2.1 FINAL - 구현 준비 완료 ✅**
