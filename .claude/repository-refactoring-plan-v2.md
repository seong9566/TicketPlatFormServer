# Repository 패턴 개선 계획 (v2 - Codex 검증 반영)

**작성일**: 2026-01-06
**작성자**: Claude Code
**1차 검증**: Codex (2026-01-06) ✅
**2차 검증**: Codex (예정)

---

## 📋 목표 (Objectives)

현재 Dapper + EF Core 혼용 방식을 **유지**하면서, Codex 피드백을 반영하여 다음을 개선:

### 우선순위 높음 (Critical)
1. ✅ **보안 강화**: EnableSensitiveDataLogging 조건부 처리
2. ✅ **동시성 안전성**: IDbConnection 동시 사용 문제 해결
3. ✅ **Namespace 일관성**: UserRepository namespace 추가

### 우선순위 중간 (Important)
4. ✅ **계층 분리**: DTO → ReadModel/QueryModel 이름 변경
5. ✅ **예외 처리 전략**: Repository는 예외를 그대로 throw
6. ✅ **트랜잭션 전략**: EF + Dapper 혼용 시 정합성 보장

### 우선순위 낮음 (Nice to have)
7. ✅ **코드 중복 제거**: BaseRepository 또는 Composition 패턴
8. ✅ **디렉토리 구조**: EventRepo → Event 통일
9. ✅ **명명 규칙**: 메서드명 통일

---

## 🔍 현재 상태 분석 (Codex 피드백 반영)

### Codex가 발견한 심각한 문제 (Critical Issues)

#### ⚠️ **1. UserRepository Namespace 누락**
```csharp
// 현재: UserRepository.cs (GLOBAL NAMESPACE!)
public class UserRepository : IUserRepository
{
    // ...
}

// 다른 Repository들
namespace TicketPlatFormServer.Repository.EventRepo;
namespace TicketPlatFormServer.Repository.Ticket;
namespace TicketPlatFormServer.Repository.Home;
```

**영향**:
- 컴파일은 되지만 일관성 깨짐
- 디렉토리 리팩토링 시 충돌 위험
- IDE 자동완성 혼란

---

#### ⚠️ **2. IDbConnection 동시성 문제**
```csharp
// Program.cs - Scoped 등록
builder.Services.AddScoped<System.Data.IDbConnection>(sp =>
    new MySqlConnector.MySqlConnection(connectionString));
```

**문제**: 한 요청 내에서 병렬 호출 시 동일 연결 동시 사용
```csharp
// 위험한 코드 (현재 프로젝트에는 없지만 향후 발생 가능)
var tasks = new[]
{
    _eventRepository.GetEventsByCategoryId(1),
    _ticketRepository.GetTicketsByEventId(100)
};
await Task.WhenAll(tasks); // ❌ 같은 IDbConnection 동시 접근!
```

---

#### 🔒 **3. EnableSensitiveDataLogging 프로덕션 노출**
```csharp
// Program.cs:40 - 환경 구분 없이 항상 켜짐!
.EnableSensitiveDataLogging()
```

**보안 리스크**:
- SQL 파라미터 값이 로그에 노출
- 개인정보(email, phone) 평문 로깅
- 프로덕션 환경에서 **심각한 보안 위반**

---

### Codex가 지적한 설계 문제 (Design Issues)

#### 4. **DTO vs ReadModel 혼동**
```csharp
// 현재: API 응답 DTO를 Repository가 반환
public interface IEventRepository
{
    Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId);
    //              ^^^^^ 프레젠테이션 계층 모델
}
```

**Codex 제안**: `*RespDto` → `*ReadModel` 또는 `*QueryRow`로 이름 변경하여 계층 분리

---

#### 5. **예외 처리 전략 문제**
```csharp
// 원래 계획 (Phase 4.2) - Codex가 비추천!
public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)
{
    try
    {
        var result = await _dapper.QueryAsync<EventListRespDto>(...);
        return result.ToList();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB 쿼리 실패: {ex.Message}"); // ❌ 로그 분산
        throw new AppException("조회 중 오류 발생", 500);  // ❌ 원인 추적 약화
    }
}
```

**Codex 권장**:
- Repository는 예외를 그대로 throw
- Service에서 도메인 예외만 `AppException`으로 변환
- `GlobalExceptionMiddleware`가 모든 예외 로깅

---

#### 6. **트랜잭션 전략 부재**
현재 계획에서 EF + Dapper를 **같은 트랜잭션**으로 묶는 방법이 명시되지 않음.

**향후 필요 시나리오**:
```csharp
// 예: 티켓 구매 - EF로 Transaction 생성, Dapper로 재고 차감
using var transaction = await _db.Database.BeginTransactionAsync();
try
{
    // EF Core 작업
    _db.Transactions.Add(newTransaction);
    await _db.SaveChangesAsync();

    // Dapper 작업 (같은 트랜잭션 공유 필요!)
    await _dapper.ExecuteAsync("UPDATE tickets SET remaining_quantity = ...");

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Codex 제안**: `Db.Database.GetDbConnection()` + `CurrentTransaction` 연동 전략 필요

---

#### 7. **BaseRepository 강제 의존성 주입**
```csharp
// 제안했던 BaseRepository
public abstract class BaseRepository
{
    protected readonly TicketContext Db;
    protected readonly IDbConnection Dapper;
    // 모든 Repository가 EF + Dapper 둘 다 강제로 받음
}

// 문제: EventRepository는 _db를 안 씀, HomeRepository는 Dapper만 씀
```

**Codex 제안**: Composition 방식 또는 선택적 의존성 주입 고려

---

## 🎯 개선 계획 (Codex 피드백 반영 - 우선순위 재정렬)

---

### 🔴 Phase 0: 보안 및 안전성 수정 (CRITICAL - 즉시 적용)

#### 0.1 EnableSensitiveDataLogging 조건부 처리

**파일**: `Program.cs:40`

**변경 전**:
```csharp
.EnableSensitiveDataLogging()
```

**변경 후**:
```csharp
#if DEBUG
        .EnableSensitiveDataLogging()
#endif
```

또는 더 명시적으로:
```csharp
if (builder.Environment.IsDevelopment())
{
    options.EnableSensitiveDataLogging();
}
```

**예상 시간**: 5분
**테스트**: 프로덕션 빌드 시 민감 정보 로그 미노출 확인

---

#### 0.2 IDbConnection 동시성 안전성 확보

**옵션 A: 가이드라인 문서화** (권장 - 빠르고 안전)

**파일**: `Repository/README.md` (신규 생성)

```markdown
## Repository 사용 가이드

### IDbConnection 사용 시 주의사항

**중요**: 현재 `IDbConnection`은 Scoped로 등록되어 요청당 1개만 존재합니다.
따라서 **병렬 Repository 호출을 피해야** 합니다.

❌ **금지 패턴**:
\```csharp
var tasks = new[]
{
    _eventRepository.GetEventsByCategoryId(1),
    _ticketRepository.GetTicketsByEventId(100)
};
await Task.WhenAll(tasks); // 동시성 충돌!
\```

✅ **권장 패턴**:
\```csharp
var events = await _eventRepository.GetEventsByCategoryId(1);
var tickets = await _ticketRepository.GetTicketsByEventId(100);
\```
```

**예상 시간**: 30분
**리스크**: 낮음 (문서화만)

---

**옵션 B: IDbConnectionFactory 패턴** (향후 확장 시)

**파일**: `Repository/Base/IDbConnectionFactory.cs`

```csharp
namespace TicketPlatFormServer.Repository.Base;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string required");
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
```

**Program.cs 변경**:
```csharp
// 기존
// builder.Services.AddScoped<IDbConnection>(...);

// 신규
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
```

**Repository 변경**:
```csharp
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

**예상 시간**: 2시간
**리스크**: 중간 (모든 Repository 수정 필요)
**판단**: 현재는 옵션 A만, 향후 병렬 호출 필요 시 옵션 B

---

#### 0.3 UserRepository Namespace 추가

**파일**: `Repository/User/UserRepository.cs`, `Repository/User/IUserRepository.cs`

**변경 전**:
```csharp
// UserRepository.cs (namespace 없음)
using System.Data;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.Repository;

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

namespace TicketPlatFormServer.Repository.User;

public class UserRepository : IUserRepository
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

### Phase 1: 계층 분리 및 명명 (Layer Separation)

#### 1.1 DTO → ReadModel 이름 변경

**목적**: Repository와 프레젠테이션 계층 결합 제거

**변경 대상**:
| 기존 이름 | 새 이름 | 위치 |
|----------|---------|------|
| `EventListRespDto` | `EventListReadModel` | `DTO/ReadModels/` |
| `EventDetailRespDto` | `EventDetailReadModel` | `DTO/ReadModels/` |
| `TicketListRespDto` | `TicketListReadModel` | `DTO/ReadModels/` |

**디렉토리 구조**:
```
DTO/
├── Request/          # API 요청 DTO
│   ├── RegisterUserReqDto.cs
│   └── LoginReqDto.cs
├── Response/         # API 응답 DTO
│   ├── RegisterUserRespDto.cs
│   └── ApiResponse.cs
└── ReadModels/       # Repository 반환 모델 (신규!)
    ├── EventListReadModel.cs
    ├── EventDetailReadModel.cs
    └── TicketListReadModel.cs
```

**변경 절차**:
1. `DTO/ReadModels/` 디렉토리 생성
2. 기존 `*RespDto` 클래스를 복사하여 `*ReadModel`로 이름 변경
3. Repository 인터페이스 및 구현체 수정
4. Service 계층에서 ReadModel → RespDto 변환 로직 추가 (필요 시)
5. 기존 `*RespDto` 삭제

**예상 시간**: 1.5시간
**리스크**: 중간 (Service 계층 수정 필요)

---

#### 1.2 Repository 예외 처리 전략 변경

**기존 계획 (Phase 4.2) 삭제**

**새로운 원칙**:
1. **Repository**: 예외를 그대로 throw (변경 없음)
2. **Service**: 비즈니스 예외만 `AppException`으로 변환
3. **GlobalExceptionMiddleware**: 모든 예외 로깅 및 응답 생성

**AppException에 InnerException 보존 추가**:

**파일**: `Common/Exception/AppException.cs`

```csharp
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

**Service 예시**:
```csharp
// EventService.cs
public async Task<EventDetailRespDto> GetEventDetail(int eventId)
{
    if (eventId <= 0)
    {
        throw new AppException("유효하지 않은 이벤트 ID입니다.", HttpStatusCode.BadRequest);
    }

    try
    {
        var eventDetail = await _repo.GetEventDetailById(eventId);

        if (eventDetail == null)
        {
            throw new AppException("이벤트를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        return MapToRespDto(eventDetail);
    }
    catch (AppException)
    {
        throw; // 비즈니스 예외는 그대로
    }
    catch (Exception ex)
    {
        // DB 예외 등은 InnerException으로 보존
        throw new AppException(
            "이벤트 조회 중 오류가 발생했습니다.",
            ex,
            HttpStatusCode.InternalServerError
        );
    }
}
```

**GlobalExceptionMiddleware 개선**:

```csharp
// GlobalExceptionMiddleware.cs
catch (Exception e)
{
    _logger.LogError(e, "[Exception] {Message} | Path: {Path}",
        e.Message,
        context.Request.Path);

    // InnerException도 로깅
    if (e.InnerException != null)
    {
        _logger.LogError("[InnerException] {Message}", e.InnerException.Message);
    }

    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    var response = new ApiResponse<object>(
        message: "서버 내부 오류가 발생했습니다.",
        data: null,
        statusCode: HttpStatusCode.InternalServerError
    );

    await context.Response.WriteAsJsonAsync(response);
}
```

**예상 시간**: 1시간
**장점**:
- 원인 추적 가능 (InnerException 보존)
- 로그 중앙화
- 디버깅 용이

---

### Phase 2: 트랜잭션 전략 (Transaction Strategy)

#### 2.1 EF + Dapper 트랜잭션 공유 전략 문서화

**파일**: `Repository/README.md`

```markdown
## EF Core + Dapper 트랜잭션 전략

### 기본 원칙
- **단일 데이터 소스 쿼리**: 각자 사용 (EF 또는 Dapper)
- **여러 작업의 정합성 필요 시**: EF 트랜잭션 공유

### 트랜잭션 공유 방법

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
            transaction: efTransaction.GetDbTransaction() // 중요!
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

### 주의사항
- 트랜잭션은 **Service 계층**에서 관리
- Repository는 트랜잭션을 받지 않음 (책임 분리)
- 트랜잭션 필요 시 Service에서 직접 DbConnection 사용
```

**예상 시간**: 30분
**현재 적용**: 문서화만 (실제 구현은 티켓 구매 기능 개발 시)

---

### Phase 3: 코드 중복 제거 (DRY - Optional)

#### 3.1 BaseRepository vs Composition 선택

**Codex 피드백**:
- BaseRepository는 모든 Repo에 EF + Dapper 강제
- EventRepository는 `_db` 미사용, HomeRepository는 `_context` 미사용

**대안 A: BaseRepository 유지 + 선택적 상속**

```csharp
// Base/BaseRepository.cs - EF + Dapper
public abstract class BaseRepository
{
    protected readonly TicketContext Db;
    protected readonly IDbConnection Dapper;

    protected BaseRepository(TicketContext db, IDbConnection dapper)
    {
        Db = db ?? throw new ArgumentNullException(nameof(db));
        Dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    }
}

// Base/EfRepository.cs - EF만 (신규)
public abstract class EfRepository
{
    protected readonly TicketContext Db;

    protected EfRepository(TicketContext db)
    {
        Db = db ?? throw new ArgumentNullException(nameof(db));
    }
}

// Base/DapperRepository.cs - Dapper만 (신규)
public abstract class DapperRepository
{
    protected readonly IDbConnection Dapper;

    protected DapperRepository(IDbConnection dapper)
    {
        Dapper = dapper ?? throw new ArgumentNullException(nameof(dapper));
    }
}
```

**적용**:
- `UserRepository`: `EfRepository` 상속
- `EventRepository`, `TicketRepository`: `DapperRepository` 상속 (또는 `BaseRepository`)
- `HomeRepository`: `DapperRepository` 상속

**예상 시간**: 1시간

---

**대안 B: C# 12 Primary Constructor 사용** (가장 깔끔)

```csharp
// UserRepository.cs
namespace TicketPlatFormServer.Repository.User;

public class UserRepository(TicketContext db) : IUserRepository
{
    // db는 자동으로 필드가 됨

    public async Task<User?> GetByEmail(string email)
    {
        return await db.Users
            .Include(u => u.Provider)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Email == email && x.IsDeleted == false);
    }
}

// EventRepository.cs
namespace TicketPlatFormServer.Repository.Event;

public partial class EventRepository(IDbConnection dapper) : IEventRepository
{
    public async Task<List<EventListReadModel>> GetEventsByCategoryId(int categoryId)
    {
        var result = await dapper.QueryAsync<EventListReadModel>(
            SqlGetEventsByCategoryId,
            new { CategoryId = categoryId }
        );
        return result.ToList();
    }
}
```

**장점**:
- BaseRepository 불필요
- 필요한 의존성만 주입
- 코드 간결

**예상 시간**: 1.5시간

---

**권장**: **대안 B (Primary Constructor)** - 현대적이고 간결

---

#### 3.2 Partial 클래스 사용 기준 명확화

**Codex 피드백**: 빈 `.Sql.cs`는 유지보수 가치 낮음

**새로운 기준**:
- SQL 쿼리가 **20줄 이상** 또는 **2개 이상**일 때만 Partial로 분리
- 그 외에는 메서드 내부 또는 상수로 유지

**적용**:
- `EventRepository`: Partial 유지 ✅ (2개 SQL 존재)
- `TicketRepository`: Partial 유지 ✅ (3개 SQL 존재)
- `UserRepository`: Partial 불필요 ❌ (EF LINQ만 사용)
- `HomeRepository`: 확인 후 결정

**예상 시간**: 30분 (UserRepository.Sql.cs 생성 취소)

---

### Phase 4: 디렉토리 및 명명 규칙 (Structure & Naming)

#### 4.1 디렉토리 구조 통일

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
├── Base/           ← 신규 (선택 사항)
│   ├── EfRepository.cs
│   └── DapperRepository.cs
├── User/
├── Event/  ← 통일
├── Ticket/
├── Home/
└── README.md       ← 신규 (가이드 문서)
```

**변경 절차**:
1. `Repository/EventRepo/` → `Repository/Event/`로 이름 변경
2. Namespace 수정: `TicketPlatFormServer.Repository.EventRepo` → `.Event`
3. Program.cs using 구문 수정
4. EventService.cs using 구문 수정
5. 컴파일 확인

**예상 시간**: 30분

---

#### 4.2 메서드명 통일 (낮은 우선순위)

**변경 대상**:
- `GetByEmail` → `GetUserByEmail`

**변경 위치**:
- `IUserRepository.cs`
- `UserRepository.cs`
- `UserService.cs` (3곳)

**방법**: Obsolete 패턴 사용하지 않고 직접 변경

**예상 시간**: 20분

---

### Phase 5: 유틸리티 및 문서화 (Utilities & Documentation)

#### 5.1 JSON 파싱 헬퍼 (Codex 피드백 반영)

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
            // Codex 피드백: Console.WriteLine 대신 Logger 사용
            logger?.LogWarning(ex, "JSON 파싱 실패: {Json}", json);
            return defaultValue;
        }
    }
}
```

**사용 예시**:
```csharp
// TicketRepository에서
private readonly ILogger<TicketRepository> _logger;

public TicketRepository(IDbConnection dapper, ILogger<TicketRepository> logger)
{
    _dapper = dapper;
    _logger = logger;
}

// JSON 파싱 시
var seatFeatures = JsonHelper.SafeDeserialize<List<string>>(
    row.SeatFeatures?.ToString(),
    new List<string>(),
    _logger
);
```

**예상 시간**: 40분

---

#### 5.2 Repository 가이드 문서 작성

**파일**: `Repository/README.md`

내용:
- Repository 계층 책임
- EF Core vs Dapper 선택 기준
- IDbConnection 동시성 주의사항
- EF + Dapper 트랜잭션 공유 방법
- Partial 클래스 사용 기준
- 예외 처리 원칙
- 새로운 Repository 추가 방법

**예상 시간**: 1시간

---

## 🚀 실행 계획 (우선순위 재정렬)

### Step 0: 긴급 보안 수정 (30분)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 0.1 | EnableSensitiveDataLogging 조건부 처리 | 🔴 긴급 | 낮음 | 5분 |
| 0.2 | IDbConnection 가이드라인 문서화 | 🔴 긴급 | 낮음 | 15분 |
| 0.3 | UserRepository Namespace 추가 | 🔴 긴급 | 낮음 | 10분 |

**즉시 실행 필요!**

---

### Step 1: 계층 분리 (2.5시간)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 1.1 | DTO → ReadModel 이름 변경 | 높음 | 중간 | 1.5시간 |
| 1.2 | AppException InnerException 추가 | 높음 | 낮음 | 30분 |
| 2.1 | 트랜잭션 전략 문서화 | 높음 | 낮음 | 30분 |

---

### Step 2: 코드 품질 개선 (3시간)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 3.1 | Primary Constructor 적용 | 중간 | 중간 | 1.5시간 |
| 4.1 | 디렉토리 구조 통일 (EventRepo → Event) | 중간 | 중간 | 30분 |
| 5.1 | JSON 헬퍼 + 로깅 | 중간 | 낮음 | 40분 |
| 5.2 | Repository README 작성 | 중간 | 낮음 | 20분 |

---

### Step 3: 정리 작업 (Optional, 50분)

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 3.2 | Partial 클래스 기준 적용 | 낮음 | 낮음 | 30분 |
| 4.2 | 메서드명 통일 | 낮음 | 낮음 | 20분 |

---

**총 예상 시간**: 약 6.5시간 (Step 3 제외 시 5.5시간)

---

## ✅ 완료 기준 (Definition of Done)

### Step 0 (긴급)
- [ ] EnableSensitiveDataLogging이 Development 환경에서만 활성화
- [ ] UserRepository에 `namespace TicketPlatFormServer.Repository.User;` 추가
- [ ] IDbConnection 사용 가이드라인 문서화
- [ ] 컴파일 성공

### Step 1 (계층 분리)
- [ ] `DTO/ReadModels/` 디렉토리 생성 및 파일 이동
- [ ] Repository 인터페이스가 `*ReadModel` 반환
- [ ] `AppException`에 InnerException 생성자 추가
- [ ] 트랜잭션 전략 문서 작성
- [ ] 기존 API 엔드포인트 테스트 통과

### Step 2 (코드 품질)
- [ ] 모든 Repository가 Primary Constructor 사용
- [ ] `Repository/EventRepo/` → `Repository/Event/`로 변경
- [ ] JsonHelper 생성 및 Logger 통합
- [ ] Repository README.md 작성 완료

### 검증
- [ ] Codex 2차 검증 통과
- [ ] 컴파일 에러 0개
- [ ] Swagger UI에서 모든 API 정상 동작
- [ ] 프로덕션 빌드 시 민감 정보 로그 미노출

---

## 📝 Codex 피드백 반영 요약

| Codex 지적사항 | 반영 여부 | 변경 내용 |
|---------------|-----------|----------|
| UserRepository namespace 누락 | ✅ 반영 | Phase 0.3 추가 |
| IDbConnection 동시성 문제 | ✅ 반영 | Phase 0.2 (가이드라인) + 향후 Factory 패턴 |
| EnableSensitiveDataLogging 보안 | ✅ 반영 | Phase 0.1 (조건부 처리) |
| DTO → ReadModel 이름 변경 | ✅ 반영 | Phase 1.1 |
| Phase 4.2 예외 처리 재설계 | ✅ 반영 | Phase 1.2 (InnerException 보존) |
| 트랜잭션 전략 부재 | ✅ 반영 | Phase 2.1 (문서화) |
| BaseRepository 강제 주입 | ✅ 반영 | Phase 3.1 (Primary Constructor) |
| Partial 클래스 강제 | ✅ 반영 | Phase 3.2 (기준 명확화) |
| JSON 헬퍼 Console.WriteLine | ✅ 반영 | Phase 5.1 (Logger 사용) |

---

## 🔄 Codex 2차 검증 포인트

다음 항목에 대해 Codex 재검증 요청:

1. **Step 0 긴급 수정이 충분한가?**
   - EnableSensitiveDataLogging 조건부 처리
   - IDbConnection 가이드라인 vs Factory 패턴 선택

2. **계층 분리 전략이 올바른가?**
   - DTO → ReadModel 이름 변경 및 위치
   - AppException InnerException 보존 방식

3. **Primary Constructor 선택이 적절한가?**
   - BaseRepository vs Primary Constructor
   - 각 Repository의 의존성 주입 방식

4. **우선순위가 합리적인가?**
   - Step 0 → Step 1 → Step 2 순서
   - Step 3은 선택 사항

5. **누락된 리스크가 있는가?**
   - 추가 보안 이슈
   - 성능 문제
   - 엣지 케이스

---

**계획 문서 v2 끝**
