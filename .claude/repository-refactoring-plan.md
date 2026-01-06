# Repository 패턴 개선 계획

**작성일**: 2026-01-06
**작성자**: Claude Code
**검증자**: Codex (예정)

---

## 📋 목표 (Objectives)

현재 Dapper + EF Core 혼용 방식을 **유지**하면서, 다음을 개선:

1. **일관성 향상**: Repository 구조와 패턴 통일
2. **유지보수성 개선**: 중복 코드 제거 및 공통 로직 추상화
3. **가독성 향상**: SQL 쿼리 분리 및 명명 규칙 통일
4. **에러 처리 강화**: 예외 처리 및 리소스 관리 개선
5. **테스트 용이성**: 의존성 주입 및 인터페이스 설계 개선

---

## 🔍 현재 상태 분석 (Current State Analysis)

### 현재 Repository 구조

```
Repository/
├── User/
│   ├── IUserRepository.cs
│   └── UserRepository.cs
├── EventRepo/
│   ├── IEventRepository.cs
│   ├── EventRepository.cs
│   └── EventRepository.Sql.cs (Partial)
├── Ticket/
│   ├── ITicketRepository.cs
│   ├── TicketRepository.cs
│   └── TicketRepository.Sql.cs (Partial)
├── Home/
│   ├── IHomeRepository.cs
│   └── HomeRepository.cs
└── TicketContext.cs
```

### 현재 사용 패턴

| Repository | EF Core | Dapper | Partial SQL |
|------------|---------|--------|-------------|
| UserRepository | ✅ (주로) | ❌ (미사용) | ❌ |
| EventRepository | ✅ | ✅ | ✅ |
| TicketRepository | ✅ | ✅ | ✅ |
| HomeRepository | ? | ? | ? |

### 발견된 문제점

#### 1. **구조적 불일치**
- UserRepository: Partial 클래스 미사용
- EventRepository/TicketRepository: Partial 클래스로 SQL 분리 ✅
- 디렉토리명 불일치 (`EventRepo` vs `User`, `Ticket`)

#### 2. **중복 코드**
```csharp
// 모든 Repository에 반복되는 코드
private readonly TicketContext _db;
private readonly IDbConnection _dapper;

public XxxRepository(TicketContext db, IDbConnection dapper)
{
    _db = db;
    _dapper = dapper;
}
```

#### 3. **DTO 누출**
```csharp
// EventRepository.cs:22 - Repository가 DTO 반환 (위반!)
public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)

// IUserRepository 주석에서는 명시적으로 금지:
// "Repository는 DB와 1:1로 맞닿아 있는 계층이다."
// "그래서 파라미터 값이 DTO가 되면 안됀다."
```

#### 4. **비즈니스 로직 포함**
```csharp
// TicketRepository.cs:156-199 - Repository에서 비즈니스 로직 수행
private string? ExtractSeatType(string? ticketTitle, object? seatFeatures)
{
    // 좌석 타입 추출 로직 - Service 계층에 있어야 함
}
```

#### 5. **JSON 파싱 중복**
```csharp
// TicketRepository에서 여러 번 반복되는 JSON 파싱 코드
try
{
    var features = JsonSerializer.Deserialize<List<string>>(row.SeatFeatures.ToString() ?? "[]");
    // ...
}
catch
{
    // JSON 파싱 실패 시 무시
}
```

#### 6. **에러 처리 부재**
- UpdateLastLoginAt: user가 null일 때 조용히 실패
- JSON 파싱 실패 시 빈 catch 블록
- IDbConnection 리소스 해제 관리 불명확

#### 7. **명명 규칙 불일치**
- SQL 상수: `SqlGetEventsByCategoryId` (✅ 좋음)
- 메서드명: `GetByEmail` vs `GetEventsByCategoryId` (일관성 부족)

---

## 🎯 개선 계획 (Improvement Plan)

### Phase 1: 기본 구조 정리 (Foundation)

#### 1.1 Base Repository 생성
**목적**: 공통 코드 중복 제거

**파일**: `Repository/Base/BaseRepository.cs`

```csharp
namespace TicketPlatFormServer.Repository.Base;

/// <summary>
/// 모든 Repository의 기본 클래스
/// EF Core + Dapper 혼용을 위한 공통 인프라 제공
/// </summary>
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
```

**적용 대상**:
- UserRepository
- EventRepository
- TicketRepository
- HomeRepository

**예상 결과**: 각 Repository에서 4-6줄의 중복 코드 제거

---

#### 1.2 디렉토리 구조 통일

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
├── Base/
│   └── BaseRepository.cs
├── User/
├── Event/  ← 통일
├── Ticket/
└── Home/
```

**변경 사항**:
1. `Repository/EventRepo/` → `Repository/Event/`로 이름 변경
2. namespace 수정: `TicketPlatFormServer.Repository.EventRepo` → `TicketPlatFormServer.Repository.Event`
3. using 구문 수정 (Program.cs, 관련 Service 파일들)

---

#### 1.3 모든 Repository에 Partial 클래스 패턴 적용

**현재**: EventRepository, TicketRepository만 Partial + SQL 분리 사용
**목표**: 모든 Repository에 일관되게 적용

**User Repository에 적용**:
```
Repository/User/
├── IUserRepository.cs
├── UserRepository.cs
└── UserRepository.Sql.cs  ← 신규 생성
```

**UserRepository.Sql.cs 예시**:
```csharp
namespace TicketPlatFormServer.Repository.User;

public partial class UserRepository
{
    // 향후 복잡한 SQL 쿼리가 추가될 경우를 대비
    // 현재는 EF Core LINQ만 사용하므로 빈 파일로 준비
}
```

---

### Phase 2: 계층 책임 분리 (Layer Responsibility)

#### 2.1 DTO 반환 문제 해결

**현재 문제**:
```csharp
// IEventRepository.cs - DTO 반환 중!
Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId);
Task<EventDetailRespDto?> GetEventDetailById(int eventId);
```

**해결 방안**:

**옵션 A**: DBModel로 변경 (권장 ❌)
- 이유: 복잡한 JOIN 쿼리 결과를 단일 DBModel로 표현 불가능
- EventListRespDto는 Event + Artist JOIN 결과

**옵션 B**: 현재 방식 유지 + 명확한 문서화 (권장 ✅)
- Repository가 DTO를 반환하는 것을 **예외적으로 허용**
- 조건: 복잡한 JOIN 쿼리로 여러 테이블 결합 시에만
- IUserRepository 주석 업데이트:

```csharp
/// <summary>
/// Repository 계층 책임:
/// 1. 단순 CRUD: DBModel 사용 (원칙)
/// 2. 복잡한 JOIN: DTO 허용 (예외)
///
/// 예외 허용 조건:
/// - 여러 테이블 조인 필요
/// - 집계 함수 사용
/// - 성능 최적화를 위한 단일 쿼리 필요
/// </summary>
```

**선택 근거**:
- 사용자가 "현재 혼용 방식 유지" 선택
- 실용적인 접근: 성능과 유지보수성 균형

---

#### 2.2 비즈니스 로직 Service로 이동

**문제 코드**:
```csharp
// TicketRepository.cs:156-199
private string? ExtractSeatType(string? ticketTitle, object? seatFeatures)
{
    // 좌석 타입 추출 로직
}
```

**해결 방안**:
1. `TicketService.cs` 또는 새로운 `SeatTypeExtractor` 유틸리티 클래스로 이동
2. Repository는 데이터 조회만 담당
3. Service에서 후처리 수행

**변경 후**:
```csharp
// TicketRepository.cs - 데이터 조회만
public async Task<List<TicketListRespDto>> GetTicketsByEventId(int eventId)
{
    var ticketRows = await _dapper.QueryAsync<dynamic>(
        SqlGetTicketsByEventId,
        new { EventId = eventId }
    );
    return MapToTicketListDto(ticketRows); // 단순 매핑만
}

// TicketService.cs - 비즈니스 로직
public async Task<List<TicketListRespDto>> GetTicketsByEventId(int eventId)
{
    var tickets = await _ticketRepository.GetTicketsByEventId(eventId);

    foreach (var ticket in tickets)
    {
        ticket.SeatType = ExtractSeatType(ticket.TicketTitle, ticket.SeatFeatures);
    }

    return tickets;
}
```

---

### Phase 3: 유틸리티 및 헬퍼 (Utilities)

#### 3.1 JSON 파싱 헬퍼 생성

**파일**: `Common/Helpers/JsonHelper.cs`

```csharp
namespace TicketPlatFormServer.Common.Helpers;

public static class JsonHelper
{
    public static T? SafeDeserialize<T>(string? json, T? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(json))
            return defaultValue;

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            // 로깅 추가
            Console.WriteLine($"JSON 파싱 실패: {ex.Message}");
            return defaultValue;
        }
    }
}
```

**사용처**:
- TicketRepository의 SeatFeatures 파싱
- 향후 다른 JSON 필드 파싱

---

#### 3.2 좌석 타입 추출 유틸리티

**파일**: `Common/Helpers/SeatTypeExtractor.cs`

```csharp
namespace TicketPlatFormServer.Common.Helpers;

public static class SeatTypeExtractor
{
    private static readonly string[] SeatTypeKeywords = { "VIP", "R", "S", "A" };

    public static string? Extract(string? ticketTitle, object? seatFeatures)
    {
        // 제목에서 추출
        if (!string.IsNullOrEmpty(ticketTitle))
        {
            foreach (var keyword in SeatTypeKeywords)
            {
                if (ticketTitle.Contains(keyword))
                    return keyword == "VIP" ? "VIP석" : $"{keyword}석";
            }
        }

        // SeatFeatures JSON에서 추출
        if (seatFeatures != null)
        {
            var features = JsonHelper.SafeDeserialize<List<string>>(
                seatFeatures.ToString()
            );

            if (features != null)
            {
                foreach (var keyword in SeatTypeKeywords)
                {
                    if (features.Any(f => f.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                        return keyword == "VIP" ? "VIP석" : $"{keyword}석";
                }
            }
        }

        return null;
    }
}
```

---

### Phase 4: 에러 처리 및 로깅 (Error Handling)

#### 4.1 Repository 메서드 예외 처리 강화

**현재 문제**:
```csharp
// UserRepository.cs:50-58
public async Task UpdateLastLoginAt(int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user != null)  // 조용히 실패
    {
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
```

**개선안**:
```csharp
public async Task UpdateLastLoginAt(int userId)
{
    var user = await _db.Users.FindAsync(userId);
    if (user == null)
    {
        throw new AppException($"사용자를 찾을 수 없습니다. (ID: {userId})", 404);
    }

    user.LastLoginAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();
}
```

---

#### 4.2 Dapper 쿼리 예외 처리

**추가 사항**:
```csharp
public async Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId)
{
    try
    {
        var result = await _dapper.QueryAsync<EventListRespDto>(
            SqlGetEventsByCategoryId,
            new { CategoryId = categoryId }
        );
        return result.ToList();
    }
    catch (Exception ex)
    {
        // 로깅 후 재전송
        Console.WriteLine($"DB 쿼리 실패 - GetEventsByCategoryId: {ex.Message}");
        throw new AppException("이벤트 목록 조회 중 오류가 발생했습니다.", 500);
    }
}
```

---

### Phase 5: 명명 규칙 통일 (Naming Conventions)

#### 5.1 Repository 메서드 명명 규칙

**현재 혼재**:
- `GetByEmail(string email)` ← 간결
- `GetEventsByCategoryId(int categoryId)` ← 상세
- `GetTicketDetailById(int ticketId)` ← 상세

**표준화 제안**:
```
Get{Entity}By{Criteria}
Get{Entity}DetailBy{Criteria}
Get{EntityPlural}By{Criteria}

예시:
- GetByEmail → GetUserByEmail
- GetEventsByCategoryId → (유지)
- GetTicketDetailById → (유지)
```

**변경 계획**:
1. `GetByEmail` → `GetUserByEmail`
2. 기존 메서드는 Obsolete 표시 + 새 메서드로 위임
3. Service 계층에서 새 메서드 사용

---

#### 5.2 SQL 상수 명명 규칙

**현재**: 일관성 있음 ✅
```csharp
private const string SqlGetEventsByCategoryId = @"...";
private const string SqlGetEventDetailById = @"...";
```

**유지**: 현재 패턴 그대로 사용

---

### Phase 6: 문서화 (Documentation)

#### 6.1 XML 주석 표준화

**모든 Repository 메서드에 추가**:
```csharp
/// <summary>
/// 카테고리별 이벤트 목록 조회
/// </summary>
/// <param name="categoryId">티켓 카테고리 ID</param>
/// <returns>이벤트 목록 (Event + Artist 조인 결과)</returns>
/// <exception cref="AppException">DB 쿼리 실패 시</exception>
Task<List<EventListRespDto>> GetEventsByCategoryId(int categoryId);
```

---

#### 6.2 Repository 설계 문서 작성

**파일**: `Repository/README.md`

내용:
- Repository 계층 책임 및 원칙
- EF Core vs Dapper 선택 기준
- Partial 클래스 사용 가이드
- 예외 처리 가이드
- 새로운 Repository 추가 방법

---

## 🚀 실행 계획 (Execution Plan)

### 우선순위

| Phase | 작업 | 중요도 | 난이도 | 예상 시간 |
|-------|------|--------|--------|-----------|
| 1.1 | BaseRepository 생성 | 높음 | 낮음 | 30분 |
| 1.2 | 디렉토리 구조 통일 | 중간 | 중간 | 1시간 |
| 1.3 | Partial 클래스 적용 | 낮음 | 낮음 | 30분 |
| 2.1 | DTO 문서화 | 높음 | 낮음 | 20분 |
| 2.2 | 비즈니스 로직 이동 | 높음 | 중간 | 1.5시간 |
| 3.1 | JSON 헬퍼 | 중간 | 낮음 | 30분 |
| 3.2 | SeatType 유틸리티 | 중간 | 낮음 | 40분 |
| 4.1 | Repository 예외 처리 | 높음 | 중간 | 1시간 |
| 4.2 | Dapper 예외 처리 | 중간 | 낮음 | 40분 |
| 5.1 | 메서드명 통일 | 낮음 | 낮음 | 30분 |
| 6.1 | XML 주석 | 낮음 | 낮음 | 1시간 |

**총 예상 시간**: 약 7-8시간

---

### 단계별 실행 순서

#### Step 1: 기초 작업 (1-2시간)
1. BaseRepository 생성
2. JSON 헬퍼 생성
3. SeatType 유틸리티 생성
4. DTO 사용 정책 문서화

#### Step 2: 구조 개선 (2-3시간)
1. EventRepo → Event 디렉토리 변경
2. 모든 Repository에 BaseRepository 적용
3. UserRepository에 Partial 클래스 적용
4. 비즈니스 로직 Service로 이동

#### Step 3: 안정성 강화 (2-3시간)
1. Repository 예외 처리 추가
2. Dapper 쿼리 예외 처리 추가
3. 메서드명 통일 (Obsolete 패턴 사용)
4. XML 주석 완성

---

## 🔍 검증 포인트 (Validation Points)

### Codex에게 검증받을 항목

1. **BaseRepository 설계**:
   - 의존성 주입 패턴이 올바른가?
   - null 체크가 적절한가?
   - 향후 확장 가능성이 있는가?

2. **계층 책임 분리**:
   - DTO 사용 정책이 합리적인가?
   - 비즈니스 로직 분리가 명확한가?

3. **예외 처리**:
   - AppException 사용이 적절한가?
   - 예외 메시지가 명확한가?
   - 보안 이슈는 없는가? (스택 트레이스 노출 등)

4. **성능 영향**:
   - BaseRepository 상속이 성능에 영향을 주는가?
   - 추가된 try-catch가 성능에 영향을 주는가?

5. **코드 품질**:
   - SOLID 원칙 위반은 없는가?
   - 중복 코드 제거가 효과적인가?
   - 명명 규칙이 .NET 표준에 부합하는가?

---

## 📝 가정 및 제약사항 (Assumptions & Constraints)

### 가정
1. 현재 코드는 프로덕션 환경에서 정상 동작 중
2. DB 스키마는 변경하지 않음
3. API 스펙은 변경하지 않음 (하위 호환성 유지)
4. 단위 테스트는 없음 (수동 테스트 필요)

### 제약사항
1. **Breaking Change 금지**: 기존 Service 계층 코드가 깨지면 안 됨
2. **성능 저하 금지**: 리팩토링으로 성능이 나빠지면 안 됨
3. **Dapper + EF Core 혼용 유지**: 이 패턴은 변경하지 않음

---

## ⚠️ 리스크 및 대응 (Risks & Mitigation)

| 리스크 | 확률 | 영향 | 대응 방안 |
|--------|------|------|-----------|
| BaseRepository 적용 시 컴파일 에러 | 중간 | 높음 | 단계적 적용, 한 개씩 테스트 |
| EventRepo → Event 변경 시 누락 파일 | 낮음 | 중간 | IDE 찾기/바꾸기 사용, 컴파일 확인 |
| 비즈니스 로직 이동 시 동작 변경 | 낮음 | 높음 | 기존 로직 그대로 복사, 수동 테스트 |
| 예외 처리 추가로 기존 동작 변경 | 중간 | 중간 | Service 계층에서 catch 추가 |
| 메서드명 변경으로 Service 코드 깨짐 | 낮음 | 중간 | Obsolete 패턴 사용 |

---

## ✅ 완료 기준 (Definition of Done)

### 코드 레벨
- [ ] 모든 Repository가 BaseRepository 상속
- [ ] EventRepo → Event 디렉토리 변경 완료 및 컴파일 성공
- [ ] 모든 Repository에 Partial 클래스 적용
- [ ] 비즈니스 로직이 Service 계층으로 이동
- [ ] JsonHelper, SeatTypeExtractor 생성 및 사용
- [ ] 모든 Repository 메서드에 예외 처리 추가
- [ ] 모든 public 메서드에 XML 주석 추가

### 검증 레벨
- [ ] Codex 코드 리뷰 통과
- [ ] 컴파일 에러 0개
- [ ] 기존 API 엔드포인트 수동 테스트 통과
- [ ] Swagger UI에서 모든 API 정상 동작 확인

---

## 📌 다음 단계 (Next Steps)

이 계획을 Codex에게 검증받은 후:

1. **사용자 승인**: 계획 내용 확인 및 우선순위 조정
2. **Phase별 구현**: Step 1부터 순차적으로 진행
3. **중간 검증**: 각 Phase 완료 후 Codex 리뷰
4. **최종 검증**: 전체 완료 후 Codex 종합 리뷰

---

**계획 문서 끝**
