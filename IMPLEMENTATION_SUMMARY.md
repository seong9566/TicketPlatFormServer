# API /api/events/tickets 리팩토링 완료 보고서

**작성일**: 2026-01-20
**작성자**: Claude Code (Sonnet 4.5)
**버전**: 1.0

---

## 📋 작업 개요

새로운 DB 스키마 (event_seat_locations, event_seat_areas, event_seat_grades)를 반영하여 티켓 조회 API를 성공적으로 리팩토링하였습니다.

---

## ✅ 완료된 작업

### Phase 1: 쿼리 개선 및 ReadModel 업데이트

#### 1.1 TicketQueries.cs 업데이트 ✅
**파일**: `/Repository/Ticket/TicketQueries.cs`

**변경 내용**:
- `GetTicketsByEventId` 쿼리 개선
  - `event_seat_locations` 테이블 LEFT JOIN 추가
  - 위치 정보 필드 추가: `LocationId`, `LocationName`, `LocationSortOrder`
  - 좌석 등급 정보 확장: `SeatGradeCode`, `SeatGradeNameEn`, `SeatGradeSortOrder`
  - 구역 정보 확장: `AreaId`, `AreaSortOrder`
  - 정렬 순서 개선: 위치 → 등급 → 구역 → 가격 → 생성일

- `GetTicketDetailById` 쿼리에도 동일한 로직 적용

**정렬 순서 변경**:
```sql
-- BEFORE
ORDER BY t.price ASC, t.created_at DESC

-- AFTER
ORDER BY
    COALESCE(esl.sort_order, 999) ASC,  -- 위치 우선
    COALESCE(esg.sort_order, 999) ASC,  -- 등급 다음
    COALESCE(sa.sort_order, 999) ASC,   -- 구역 다음
    t.price ASC,                         -- 가격
    t.created_at DESC                    -- 최신순
```

#### 1.2 TicketListReadModel.cs 업데이트 ✅
**파일**: `/Repository/ReadModels/TicketListReadModel.cs`

**추가된 필드** (총 9개):
```csharp
// 좌석 등급 확장
public string? SeatGradeCode { get; set; }
public string? SeatGradeNameEn { get; set; }
public int? SeatGradeSortOrder { get; set; }

// 구역 정보 확장
public int? AreaId { get; set; }
public int? AreaSortOrder { get; set; }

// 위치 정보 (NEW)
public int? LocationId { get; set; }
public string? LocationName { get; set; }
public int? LocationSortOrder { get; set; }
```

**하위 호환성**: 모든 새 필드는 nullable로 선언하여 레거시 데이터 대응

#### 1.3 TicketRepository.cs 매핑 로직 업데이트 ✅
**파일**: `/Repository/Ticket/TicketRepository.cs`

**변경 내용**:
- `GetTicketsByEventId` 메서드: 새 필드 매핑 추가
- `GetTicketDetailById` 메서드: 동일한 로직 적용
- Long → Int 명시적 형변환 적용
- Null 처리 강화

**주요 코드**:
```csharp
SeatGradeCode = row.SeatGradeCode,
SeatGradeNameEn = row.SeatGradeNameEn,
SeatGradeSortOrder = row.SeatGradeSortOrder != null ? (int?)Convert.ToInt32(row.SeatGradeSortOrder) : null,
AreaId = row.AreaId != null ? (int?)Convert.ToInt32(row.AreaId) : null,
LocationId = row.LocationId != null ? (int?)Convert.ToInt32(row.LocationId) : null,
LocationName = row.LocationName,
```

---

### Phase 2: DTO 및 Service 레이어 업데이트

#### 2.1 SeatLocationDto.cs 생성 ✅
**파일**: `/DTO/Event/SeatLocationDto.cs` (신규 파일)

```csharp
public class SeatLocationDto
{
    public int LocationId { get; set; }
    public string LocationName { get; set; } = null!;
    public int TicketCount { get; set; }
    public int SortOrder { get; set; }
}
```

#### 2.2 TicketListRespDto.cs 확장 ✅
**파일**: `/DTO/Event/TicketListRespDto.cs`

**추가된 필드** (총 5개):
```csharp
public string? SeatGradeCode { get; set; }        // 좌석 등급 코드
public string? SeatGradeNameEn { get; set; }      // 좌석 등급 영문명
public int? AreaId { get; set; }                  // 구역 ID
public int? LocationId { get; set; }              // 위치 ID
public string? LocationName { get; set; }         // 위치명
```

#### 2.3 EventDetailRespDto.cs 확장 ✅
**파일**: `/DTO/Event/EventDetailRespDto.cs`

**추가된 필드**:
```csharp
public List<SeatLocationDto> SeatLocationFilters { get; set; } = new();
```

#### 2.4 EventService.cs 로직 업데이트 ✅
**파일**: `/Services/Event/EventService.cs`

**변경 내용**:
1. **위치별 티켓 카운트 집계 로직 추가**:
```csharp
var locationCounts = new Dictionary<int, (string name, int count, int sortOrder)>();

foreach (var ticket in ticketReadModels)
{
    if (ticket.LocationId.HasValue)
    {
        var locId = ticket.LocationId.Value;
        if (!locationCounts.ContainsKey(locId))
        {
            locationCounts[locId] = (
                ticket.LocationName ?? "미분류",
                0,
                ticket.LocationSortOrder ?? 999
            );
        }
        var (name, count, sort) = locationCounts[locId];
        locationCounts[locId] = (name, count + 1, sort);
    }
}
```

2. **SeatLocationFilters 생성**:
```csharp
var seatLocationFilters = locationCounts
    .OrderBy(x => x.Value.sortOrder)
    .Select(x => new SeatLocationDto
    {
        LocationId = x.Key,
        LocationName = x.Value.name,
        TicketCount = x.Value.count,
        SortOrder = x.Value.sortOrder
    })
    .ToList();
```

3. **DTO 매핑에 새 필드 추가**:
```csharp
SeatLocationFilters = seatLocationFilters,
Tickets = ticketReadModels.Select(tm => new TicketListRespDto
{
    SeatGradeCode = tm.SeatGradeCode,
    SeatGradeNameEn = tm.SeatGradeNameEn,
    AreaId = tm.AreaId,
    LocationId = tm.LocationId,
    LocationName = tm.LocationName,
    // ... 기존 필드들
}).ToList()
```

---

## 🔧 빌드 및 테스트 결과

### 빌드 결과 ✅
```
빌드했습니다.
    경고 0개
    오류 0개
경과 시간: 00:00:04.40
```

### SQL 쿼리 테스트 ✅
- ✅ 쿼리 정상 실행 확인
- ✅ 새 필드들 올바르게 조회됨
- ✅ 레거시 티켓 (위치 정보 없음) 정상 처리 (NULL 반환)
- ✅ 정렬 순서 올바르게 적용됨

**테스트 데이터 결과**:
```
총 티켓: 39개
위치 정보 있는 티켓: 4개
레거시 티켓: 35개
```

### EXPLAIN 분석 결과
```
tickets:            type = ref, key = fk_tickets_event, rows = 15
user_profile:       type = eq_ref (효율적)
event_seat_locations: type = eq_ref (효율적)
trade_methods:      type = eq_ref (효율적)

주의: event_seat_grades, event_seat_areas는 ALL (개선 필요)
```

---

## 🚀 성능 최적화 권고사항

MySQL DB Expert Agent 분석 결과, 다음과 같은 최적화가 권장됩니다:

### 즉시 적용 권장 (DB_OPTIMIZATION.sql 참조)

#### 1. 복합 인덱스 추가 ⭐ (가장 중요)
```sql
CREATE INDEX idx_tickets_event_status_filter
ON tickets(event_id, status_id, deleted_at, remaining_quantity);
```
**효과**: WHERE 조건 4개를 모두 커버하여 스캔 rows 15 → 2-5로 감소

#### 2. sort_order 기본값 설정
```sql
ALTER TABLE event_seat_locations
MODIFY COLUMN sort_order INT NOT NULL DEFAULT 999;

ALTER TABLE event_seat_grades
MODIFY COLUMN sort_order INT NOT NULL DEFAULT 999;

ALTER TABLE event_seat_areas
MODIFY COLUMN sort_order INT NOT NULL DEFAULT 999;

UPDATE event_seat_locations SET sort_order = 999 WHERE sort_order IS NULL;
UPDATE event_seat_grades SET sort_order = 999 WHERE sort_order IS NULL;
UPDATE event_seat_areas SET sort_order = 999 WHERE sort_order IS NULL;
```
**효과**:
- COALESCE 함수 제거 가능
- Using filesort 제거
- 인덱스 활용 가능

#### 3. 통계 정보 업데이트 ✅ (완료)
```sql
ANALYZE TABLE tickets;
ANALYZE TABLE event_seat_grades;
ANALYZE TABLE event_seat_areas;
ANALYZE TABLE event_seat_locations;
```

### 예상 성능 개선 효과

| 항목 | 현재 | 최적화 후 |
|------|------|-----------|
| tickets 스캔 | 15 rows | 2-5 rows |
| event_seat_grades | ALL (2 rows) | eq_ref (1 row) |
| event_seat_areas | ALL (2 rows) | eq_ref (1 row) |
| Using filesort | YES | NO |
| Using temporary | YES | NO |
| **전체 성능** | 기준 | **3-5배 개선** |

---

## 📊 API 응답 변화

### Before (기존)
```json
{
  "message": "이벤트 상세 정보 조회 성공",
  "data": {
    "eventId": 1,
    "eventTitle": "아이유 콘서트",
    "seatTypeFilters": [
      {"seatTypeName": "전체좌석", "ticketCount": 10},
      {"seatTypeName": "VIP석", "ticketCount": 3}
    ],
    "tickets": [{
      "ticketId": 101,
      "seatGradeName": "VIP석",
      "area": "A구역",
      "price": 150000
    }]
  }
}
```

### After (개선)
```json
{
  "message": "이벤트 상세 정보 조회 성공",
  "data": {
    "eventId": 1,
    "eventTitle": "아이유 콘서트",
    "seatTypeFilters": [
      {"seatTypeName": "전체좌석", "ticketCount": 10},
      {"seatTypeName": "VIP석", "ticketCount": 3}
    ],
    "seatLocationFilters": [
      {
        "locationId": 1,
        "locationName": "1층",
        "ticketCount": 7,
        "sortOrder": 1
      },
      {
        "locationId": 2,
        "locationName": "2층",
        "ticketCount": 3,
        "sortOrder": 2
      }
    ],
    "tickets": [{
      "ticketId": 101,
      "seatGradeId": 5,
      "seatGradeCode": "VIP",
      "seatGradeName": "VIP석",
      "seatGradeNameEn": "VIP Seat",
      "areaId": 10,
      "area": "A구역",
      "locationId": 1,
      "locationName": "1층",
      "price": 150000
    }]
  }
}
```

**추가된 필드**:
- ✅ `seatLocationFilters`: 위치별 필터 배열
- ✅ `seatGradeCode`: 좌석 등급 코드
- ✅ `seatGradeNameEn`: 좌석 등급 영문명
- ✅ `areaId`: 구역 ID
- ✅ `locationId`, `locationName`: 위치 정보

---

## 📁 변경된 파일 목록

### 수정된 파일 (7개)
1. `/Repository/Ticket/TicketQueries.cs` - SQL 쿼리 수정
2. `/Repository/ReadModels/TicketListReadModel.cs` - 9개 필드 추가
3. `/Repository/Ticket/TicketRepository.cs` - 매핑 로직 업데이트
4. `/DTO/Event/TicketListRespDto.cs` - 5개 필드 추가
5. `/DTO/Event/EventDetailRespDto.cs` - SeatLocationFilters 추가
6. `/Services/Event/EventService.cs` - 비즈니스 로직 업데이트
7. `/Controllers/EventController.cs` - (변경 없음, 검증만 수행)

### 신규 파일 (3개)
1. `/DTO/Event/SeatLocationDto.cs` - 위치 필터 DTO
2. `/REFACTORING_PLAN.md` - 상세 작업 계획서
3. `/DB_OPTIMIZATION.sql` - DB 최적화 스크립트
4. `/IMPLEMENTATION_SUMMARY.md` - 본 문서

---

## ✅ 하위 호환성 체크

- ✅ 모든 새 필드는 nullable
- ✅ 기존 필드 모두 유지
- ✅ 기존 클라이언트에 영향 없음
- ✅ 레거시 티켓 (위치 정보 없음) 정상 응답
- ✅ NULL 값 처리 안전

---

## 🎯 다음 단계

### 즉시 실행 권장
1. **DB 최적화 스크립트 실행** (`DB_OPTIMIZATION.sql`)
   - 복합 인덱스 추가
   - sort_order 기본값 설정
   - 통계 정보 업데이트

2. **EXPLAIN 재검증**
   - 최적화 후 성능 개선 확인
   - 실행 계획 변화 확인

3. **부하 테스트**
   - 대량 티켓 데이터 시나리오 테스트
   - 응답 시간 측정

### 향후 고려사항
1. **쿼리 최적화 2단계**:
   - COALESCE 제거 (sort_order DEFAULT 설정 후)
   - 애플리케이션 레벨 정렬 고려

2. **API 버전 관리**:
   - 향후 대규모 변경 시 `/v2/` 엔드포인트 고려

3. **모니터링**:
   - 쿼리 실행 시간 로깅
   - Slow Query Log 모니터링

---

## 📝 참고 문서

- **상세 계획서**: `REFACTORING_PLAN.md`
- **DB 최적화 스크립트**: `DB_OPTIMIZATION.sql`
- **MySQL Expert 분석 결과**: Agent ID a9d2386

---

## 🎉 완료 요약

✅ **Phase 1 & 2 통합 구현 완료**
✅ **빌드 성공 (경고 0, 오류 0)**
✅ **SQL 쿼리 테스트 통과**
✅ **하위 호환성 유지**
✅ **성능 최적화 가이드 작성**

**추가 작업 필요**: DB 최적화 스크립트 실행 및 검증

---

**작성일**: 2026-01-20
**최종 업데이트**: 2026-01-20 (KST)
