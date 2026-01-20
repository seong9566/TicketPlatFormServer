# API /api/events/tickets 리팩토링 작업 계획서

## 📋 작업 개요

**목표**: 새로운 DB 스키마(event_seat_locations, event_seat_areas, event_seat_grades)를 반영하여 티켓 조회 API를 리팩토링

**영향받는 엔드포인트**: `GET /api/events/tickets?eventId={eventId}`

**우선순위**: 높음 (Phase 1 + 2 통합 구현)

---

## 🗄️ 데이터베이스 스키마 분석

### 새로 추가된 테이블

#### 1. `event_seat_locations` (좌석 위치/층 정보)
```sql
- id (PK, int)
- event_id (FK, int)
- location_name (varchar(50)) -- 예: "1층", "2층", "플로어석"
- is_active (tinyint(1))
- sort_order (int) -- UI 정렬 순서
- created_at (timestamp)
```

#### 2. `event_seat_areas` (좌석 구역 정보)
```sql
- id (PK, int)
- event_id (FK, int)
- area_name (varchar(50)) -- 예: "A구역", "B구역"
- is_active (tinyint(1))
- sort_order (int) -- UI 정렬 순서
- created_at (timestamp)
```

#### 3. `event_seat_grades` (좌석 등급 정보)
```sql
- id (PK, int)
- event_id (FK, int)
- seat_grade_id (FK, int) -- 글로벌 seat_grades 테이블 참조
- code (varchar(50)) -- 시스템 코드 (예: "VIP", "R", "S")
- name_ko (varchar(100)) -- 한글명 (예: "VIP석", "R석")
- name_en (varchar(100)) -- 영문명 (예: "VIP Seat")
- original_price (int) -- 정가
- is_active (tinyint(1))
- sort_order (int) -- UI 정렬 순서
- created_at (timestamp)
```

### 업데이트된 테이블

#### `tickets` 테이블 변경사항
- **추가된 FK**:
  - `seat_location_id` (int, nullable) → event_seat_locations.id
  - `area_id` (int, nullable) → event_seat_areas.id
  - `seat_grade_id` (int, nullable) → event_seat_grades.id

---

## 🔍 현재 구현 분석

### 문제점

1. **위치 데이터 누락**
   - `event_seat_locations` 테이블을 조인하지 않음
   - 티켓 응답에 층/위치 정보가 없음
   - 위치별 필터링 불가능

2. **불완전한 좌석 메타데이터**
   - `event_seat_grades`에서 `code`, `name_en` 필드를 가져오지 않음
   - 영문 좌석 등급명이 제공되지 않음

3. **정렬 순서 미적용**
   - `sort_order` 필드를 사용하지 않아 일관된 UI 정렬 안 됨
   - 현재: `ORDER BY t.price ASC, t.created_at DESC`
   - 개선 필요: 위치 → 등급 → 구역 → 가격 순으로 정렬

4. **필터 옵션 부족**
   - 좌석 타입 필터만 제공 (`SeatTypeFilterDto`)
   - 위치/층 필터가 없음

5. **타입 변환 오류**
   - MySQL의 `BIGINT`가 C#의 `long`으로 반환되지만 모델은 `int` 사용
   - Dynamic 매핑 시 명시적 형변환 필요

---

## 📐 Phase 1: 쿼리 개선 및 ReadModel 업데이트

### 1.1 TicketQueries.cs 업데이트

**파일 경로**: `/Repository/Ticket/TicketQueries.cs`

**작업 내용**:

#### GetTicketsByEventId 쿼리 수정

```sql
SELECT
    t.id AS TicketId,

    -- 좌석 등급 정보 (확장)
    t.seat_grade_id AS SeatGradeId,
    esg.code AS SeatGradeCode,
    esg.name_ko AS SeatGradeName,
    esg.name_en AS SeatGradeNameEn,
    esg.sort_order AS SeatGradeSortOrder,

    -- 구역 정보 (확장)
    t.area_id AS AreaId,
    sa.area_name AS Area,
    sa.sort_order AS AreaSortOrder,

    -- 위치 정보 (NEW)
    t.seat_location_id AS LocationId,
    esl.location_name AS LocationName,
    esl.sort_order AS LocationSortOrder,

    -- 기존 필드
    t.`row` AS `Row`,
    t.price AS Price,
    COALESCE(esg.original_price, t.price) AS OriginalPrice,
    t.quantity AS Quantity,
    t.remaining_quantity AS RemainingQuantity,
    t.is_consecutive AS IsConsecutive,
    t.trade_method_id AS TradeMethodId,
    tm.name_ko AS TradeMethodName,
    t.has_ticket AS HasTicket,
    t.description AS Description,
    t.created_at AS CreatedAt,

    -- 판매자 정보
    up.user_id AS UserId,
    up.nickname AS Nickname,
    up.profile_image_url AS ProfileImageUrl,
    up.manner_temperature AS MannerTemperature

FROM tickets t
INNER JOIN user_profile up ON t.seller_id = up.user_id
LEFT JOIN event_seat_grades esg ON t.seat_grade_id = esg.id
LEFT JOIN event_seat_areas sa ON t.area_id = sa.id
LEFT JOIN event_seat_locations esl ON t.seat_location_id = esl.id  -- NEW JOIN
LEFT JOIN trade_methods tm ON t.trade_method_id = tm.id
WHERE t.event_id = @EventId
  AND t.status_id = 1
  AND t.deleted_at IS NULL
  AND t.remaining_quantity > 0
ORDER BY
    COALESCE(esl.sort_order, 999) ASC,  -- 위치 정렬 (NULL은 맨 뒤)
    COALESCE(esg.sort_order, 999) ASC,  -- 등급 정렬
    COALESCE(sa.sort_order, 999) ASC,   -- 구역 정렬
    t.price ASC,                         -- 가격 정렬
    t.created_at DESC;                   -- 최신순
```

**변경 사항**:
- ✅ `event_seat_locations` 조인 추가
- ✅ 위치 정보 필드 추가 (`LocationId`, `LocationName`, `LocationSortOrder`)
- ✅ 구역 정보 확장 (`AreaId`, `AreaSortOrder`)
- ✅ 좌석 등급 정보 확장 (`SeatGradeCode`, `SeatGradeNameEn`, `SeatGradeSortOrder`)
- ✅ 정렬 순서 개선 (위치 → 등급 → 구역 → 가격)
- ✅ NULL 값 처리 (레거시 티켓 대응)

#### GetTicketDetailById 쿼리 수정

동일한 로직 적용 + 기존 상세 정보 필드 유지

---

### 1.2 ReadModel 업데이트

**파일 경로**: `/Repository/ReadModels/TicketListReadModel.cs`

**작업 내용**:

```csharp
public class TicketListReadModel
{
    public int TicketId { get; set; }

    // 좌석 등급 정보 (확장)
    public int? SeatGradeId { get; set; }
    public string? SeatGradeCode { get; set; }        // NEW
    public string? SeatGradeName { get; set; }
    public string? SeatGradeNameEn { get; set; }      // NEW
    public int? SeatGradeSortOrder { get; set; }      // NEW

    // 구역 정보 (확장)
    public int? AreaId { get; set; }                  // NEW
    public string? Area { get; set; }
    public int? AreaSortOrder { get; set; }           // NEW

    // 위치 정보 (NEW)
    public int? LocationId { get; set; }              // NEW
    public string? LocationName { get; set; }         // NEW
    public int? LocationSortOrder { get; set; }       // NEW

    // 기존 필드
    public string? Row { get; set; }
    public int Price { get; set; }
    public int OriginalPrice { get; set; }
    public bool? IsConsecutive { get; set; }
    public int? TradeMethodId { get; set; }
    public string? TradeMethodName { get; set; }
    public bool? HasTicket { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Quantity { get; set; }
    public int RemainingQuantity { get; set; }
    public bool IsSingleTicket { get; set; }
    public List<string> TicketImages { get; set; } = new();
    public SellerInfoReadModel Seller { get; set; } = null!;
}
```

**변경 사항**:
- ✅ 9개 새 필드 추가 (위치 3개, 구역 2개, 좌석 등급 4개)
- ✅ 모두 nullable로 선언 (레거시 데이터 대응)
- ✅ 기존 필드 유지 (하위 호환성)

---

### 1.3 TicketRepository.cs 매핑 로직 업데이트

**파일 경로**: `/Repository/Ticket/TicketRepository.cs`

**작업 내용**:

#### GetTicketsByEventId 메서드

```csharp
foreach (var row in ticketRows)
{
    tickets.Add(new TicketListReadModel
    {
        TicketId = (int)row.TicketId,

        // 좌석 등급 정보
        SeatGradeId = row.SeatGradeId != null ? (int?)Convert.ToInt32(row.SeatGradeId) : null,
        SeatGradeCode = row.SeatGradeCode,                                              // NEW
        SeatGradeName = row.SeatGradeName,
        SeatGradeNameEn = row.SeatGradeNameEn,                                          // NEW
        SeatGradeSortOrder = row.SeatGradeSortOrder != null ? (int?)Convert.ToInt32(row.SeatGradeSortOrder) : null, // NEW

        // 구역 정보
        AreaId = row.AreaId != null ? (int?)Convert.ToInt32(row.AreaId) : null,        // NEW
        Area = row.Area,
        AreaSortOrder = row.AreaSortOrder != null ? (int?)Convert.ToInt32(row.AreaSortOrder) : null, // NEW

        // 위치 정보 (NEW)
        LocationId = row.LocationId != null ? (int?)Convert.ToInt32(row.LocationId) : null,
        LocationName = row.LocationName,
        LocationSortOrder = row.LocationSortOrder != null ? (int?)Convert.ToInt32(row.LocationSortOrder) : null,

        // 기존 필드
        Row = row.Row,
        Price = (int)row.Price,
        OriginalPrice = (int)row.OriginalPrice,
        IsConsecutive = row.IsConsecutive,
        TradeMethodId = row.TradeMethodId != null ? (int?)Convert.ToInt32(row.TradeMethodId) : null,
        TradeMethodName = row.TradeMethodName,
        HasTicket = row.HasTicket,
        Description = row.Description,
        CreatedAt = row.CreatedAt,
        Quantity = (int)row.Quantity,
        IsSingleTicket = row.Quantity == 1,
        RemainingQuantity = (int)row.RemainingQuantity,
        TicketImages = new List<string>(),

        Seller = new SellerInfoReadModel
        {
            UserId = (int)row.UserId,
            Nickname = row.Nickname,
            ProfileImageUrl = row.ProfileImageUrl,
            MannerTemperature = row.MannerTemperature != null ? (float?)Convert.ToDouble(row.MannerTemperature) : null,
            TotalTradeCount = 0,
            ResponseRate = null,
            IsSecurePayment = false
        }
    });
}
```

**변경 사항**:
- ✅ 새 필드 매핑 추가
- ✅ Long → Int 명시적 형변환
- ✅ Null 처리 강화

#### GetTicketDetailById 메서드

동일한 매핑 로직 적용

---

## 📤 Phase 2: DTO 및 Service 레이어 업데이트

### 2.1 새로운 DTO 생성

#### SeatLocationDto.cs (NEW)

**파일 경로**: `/DTO/Event/SeatLocationDto.cs`

```csharp
namespace TicketPlatFormServer.DTO;

/// <summary>
/// 좌석 위치 필터 정보 Dto
/// </summary>
public class SeatLocationDto
{
    /// <summary>
    /// 위치 ID
    /// </summary>
    public int LocationId { get; set; }

    /// <summary>
    /// 위치명 (예: "1층", "2층", "플로어석")
    /// </summary>
    public string LocationName { get; set; } = null!;

    /// <summary>
    /// 해당 위치의 티켓 개수
    /// </summary>
    public int TicketCount { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }
}
```

---

### 2.2 기존 DTO 확장

#### TicketListRespDto.cs 업데이트

**파일 경로**: `/DTO/Event/TicketListRespDto.cs`

**추가할 필드**:

```csharp
// 기존 필드 유지...

/// <summary>
/// 구역 ID (NEW)
/// </summary>
public int? AreaId { get; set; }

/// <summary>
/// 좌석 등급 코드 (NEW - 예: "VIP", "R", "S")
/// </summary>
public string? SeatGradeCode { get; set; }

/// <summary>
/// 좌석 등급 영문명 (NEW - 예: "VIP Seat")
/// </summary>
public string? SeatGradeNameEn { get; set; }

/// <summary>
/// 위치 ID (NEW)
/// </summary>
public int? LocationId { get; set; }

/// <summary>
/// 위치명 (NEW - 예: "1층", "2층")
/// </summary>
public string? LocationName { get; set; }
```

**하위 호환성**:
- ✅ 기존 필드 모두 유지
- ✅ 새 필드는 nullable로 추가
- ✅ 기존 클라이언트에 영향 없음

---

#### EventDetailRespDto.cs 업데이트

**파일 경로**: `/DTO/Event/EventDetailRespDto.cs`

**추가할 필드**:

```csharp
// 기존 필드 유지...

/// <summary>
/// 좌석 위치별 필터 정보 (NEW)
/// </summary>
public List<SeatLocationDto> SeatLocationFilters { get; set; } = new();
```

**변경 사항**:
- ✅ `SeatLocationFilters` 추가
- ✅ 기존 `SeatTypeFilters`와 동일한 패턴
- ✅ UI에서 위치별 필터링 가능

---

### 2.3 EventService.cs 업데이트

**파일 경로**: `/Services/Event/EventService.cs`

**작업 내용**:

#### GetEventDetailWithTickets 메서드 수정

```csharp
public async Task<EventDetailRespDto> GetEventDetailWithTickets(int eventId, int? userId = null)
{
    // 기존 로직...
    var ticketReadModels = await _ticketRepo.GetTicketsByEventId(eventId);

    // 찜 목록 조회 로직 (기존 유지)
    // ...

    // 좌석 등급 필터 생성 (기존)
    var seatGradeCounts = new Dictionary<string, int>();

    // 위치 필터 생성 (NEW)
    var locationCounts = new Dictionary<int, (string name, int count, int sortOrder)>();

    bool isSoldOutImminent = false;

    foreach (var ticket in ticketReadModels)
    {
        // 좌석 등급별 카운트 (기존)
        if (!string.IsNullOrEmpty(ticket.SeatGradeName))
        {
            if (!seatGradeCounts.ContainsKey(ticket.SeatGradeName))
            {
                seatGradeCounts[ticket.SeatGradeName] = 0;
            }
            seatGradeCounts[ticket.SeatGradeName]++;
        }

        // 위치별 카운트 (NEW)
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

        // 매진 임박 체크 (기존)
        if (ticket.RemainingQuantity <= 5)
        {
            isSoldOutImminent = true;
        }
    }

    // 좌석 등급 필터 생성 (기존)
    var seatTypeFilters = new List<SeatTypeFilterDto>();
    seatTypeFilters.Add(new SeatTypeFilterDto
    {
        SeatTypeName = "전체좌석",
        TicketCount = ticketReadModels.Count
    });
    foreach (var kvp in seatGradeCounts.OrderBy(x => x.Key))
    {
        seatTypeFilters.Add(new SeatTypeFilterDto
        {
            SeatTypeName = kvp.Key,
            TicketCount = kvp.Value
        });
    }

    // 위치 필터 생성 (NEW)
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

    // DTO 매핑
    return new EventDetailRespDto
    {
        EventId = eventReadModel.EventId,
        EventTitle = eventReadModel.EventTitle,
        EventPosterImageUrl = eventReadModel.EventPosterImageUrl,
        StartAt = eventReadModel.StartAt,
        EndAt = eventReadModel.EndAt,
        VenueName = eventReadModel.VenueName,
        VenueAddress = eventReadModel.VenueAddress,
        ArtistId = eventReadModel.ArtistId,
        ArtistName = eventReadModel.ArtistName,
        IsSoldOutImminent = isSoldOutImminent,
        SeatTypeFilters = seatTypeFilters,
        SeatLocationFilters = seatLocationFilters,  // NEW
        Tickets = ticketReadModels.Select(tm => new TicketListRespDto
        {
            TicketId = tm.TicketId,
            SeatGradeId = tm.SeatGradeId,
            SeatGradeName = tm.SeatGradeName,
            SeatGradeCode = tm.SeatGradeCode,          // NEW
            SeatGradeNameEn = tm.SeatGradeNameEn,      // NEW
            AreaId = tm.AreaId,                        // NEW
            Area = tm.Area,
            LocationId = tm.LocationId,                // NEW
            LocationName = tm.LocationName,            // NEW
            Row = tm.Row,
            Price = tm.Price,
            OriginalPrice = tm.OriginalPrice,
            IsConsecutive = tm.IsConsecutive,
            TradeMethodId = tm.TradeMethodId,
            TradeMethodName = tm.TradeMethodName,
            HasTicket = tm.HasTicket,
            Description = tm.Description,
            CreatedAt = tm.CreatedAt,
            Quantity = tm.Quantity,
            RemainingQuantity = tm.RemainingQuantity,
            IsSingleTicket = tm.IsSingleTicket,
            TicketImages = tm.TicketImages,
            IsFavorited = userId.HasValue ? favoritedTicketIds.Contains(tm.TicketId) : null,
            Seller = new SellerInfoDto
            {
                UserId = tm.Seller.UserId,
                Nickname = tm.Seller.Nickname,
                ProfileImageUrl = tm.Seller.ProfileImageUrl,
                MannerTemperature = tm.Seller.MannerTemperature,
                TotalTradeCount = tm.Seller.TotalTradeCount,
                ResponseRate = tm.Seller.ResponseRate,
                IsSecurePayment = tm.Seller.IsSecurePayment
            }
        }).ToList()
    };
}
```

**변경 사항**:
- ✅ 위치별 카운트 집계 로직 추가
- ✅ `SeatLocationFilters` 생성
- ✅ DTO 매핑에 새 필드 추가
- ✅ sort_order 기반 정렬 적용

---

## 🧪 테스트 전략

### 단위 테스트

1. **TicketRepository 테스트**
   - ✅ 새 필드가 올바르게 매핑되는지 확인
   - ✅ Null 값 처리 검증 (레거시 티켓)
   - ✅ Long → Int 변환 검증

2. **EventService 테스트**
   - ✅ 위치 필터 생성 로직 검증
   - ✅ 빈 위치 정보 처리
   - ✅ SortOrder 정렬 검증

### 통합 테스트

1. **API 엔드포인트 테스트**
   ```bash
   GET /api/events/tickets?eventId=1
   ```

   **검증 항목**:
   - ✅ HTTP 200 응답
   - ✅ `SeatLocationFilters` 배열 존재
   - ✅ 각 티켓에 `LocationName`, `LocationId` 필드 존재
   - ✅ 정렬 순서 올바름 (location → grade → area → price)
   - ✅ 레거시 티켓(위치 정보 없음) 정상 응답

2. **데이터베이스 쿼리 성능 테스트**
   - ✅ 기존 대비 성능 저하 없음 (<100ms 목표)
   - ✅ 인덱스 활용 확인 (EXPLAIN ANALYZE)

---

## 🔄 마이그레이션 및 배포 전략

### 레거시 데이터 처리

**현재 상황**:
- 일부 티켓은 `seat_location_id`, `area_id`가 NULL일 수 있음

**처리 방법**:
1. ✅ Nullable 필드로 설계 (이미 적용)
2. ✅ NULL 체크 로직 (쿼리 및 매핑에 포함)
3. ✅ UI에서 "미분류" 처리 가능

**향후 개선**:
- 레거시 티켓 데이터 마이그레이션 스크립트 작성 (Optional)

### 배포 순서

1. **데이터베이스 변경 확인** (이미 완료됨)
2. **백엔드 코드 배포** (본 작업)
3. **API 문서 업데이트** (Swagger)
4. **프론트엔드 업데이트** (새 필드 활용)

---

## 📊 예상 API 응답 예시

### Before (현재)

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
    "tickets": [
      {
        "ticketId": 101,
        "seatGradeName": "VIP석",
        "area": "A구역",
        "price": 150000
      }
    ]
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
    "tickets": [
      {
        "ticketId": 101,
        "seatGradeId": 5,
        "seatGradeName": "VIP석",
        "seatGradeCode": "VIP",
        "seatGradeNameEn": "VIP Seat",
        "areaId": 10,
        "area": "A구역",
        "locationId": 1,
        "locationName": "1층",
        "price": 150000
      }
    ]
  }
}
```

**추가된 필드**:
- ✅ `seatLocationFilters` (위치 필터 배열)
- ✅ `seatGradeCode`, `seatGradeNameEn` (좌석 등급 상세)
- ✅ `areaId`, `locationId`, `locationName` (구조화된 위치 정보)

---

## ⚠️ 주의사항 및 위험 요소

### 1. 타입 변환 주의
- MySQL의 `INT` → C# `long` → 명시적 `int` 캐스팅 필요
- Dynamic 객체 매핑 시 런타임 에러 가능성

**완화 방법**:
- ✅ `Convert.ToInt32()` 사용
- ✅ Nullable 처리로 예외 방지
- ✅ 단위 테스트로 검증

### 2. NULL 데이터 처리
- 레거시 티켓에 위치 정보 없을 수 있음

**완화 방법**:
- ✅ 모든 새 필드를 nullable로 설계
- ✅ 쿼리에서 `LEFT JOIN` 사용
- ✅ COALESCE로 정렬 순서 기본값 설정

### 3. 성능 영향
- 추가 JOIN으로 쿼리 복잡도 증가

**완화 방법**:
- ✅ 인덱스 확인 (event_id, seat_location_id, area_id)
- ✅ EXPLAIN 분석
- ✅ 성능 테스트 실시

### 4. API 하위 호환성
- 기존 클라이언트가 새 필드 무시하는지 확인 필요

**완화 방법**:
- ✅ 새 필드는 모두 선택적(optional)
- ✅ 기존 필드 유지
- ✅ API 버전 관리 고려 (향후)

---

## ✅ 작업 체크리스트

### Phase 1: 쿼리 및 ReadModel
- [ ] `TicketQueries.GetTicketsByEventId` 수정
- [ ] `TicketQueries.GetTicketDetailById` 수정
- [ ] `TicketListReadModel` 필드 추가
- [ ] `TicketRepository.GetTicketsByEventId` 매핑 로직 수정
- [ ] `TicketRepository.GetTicketDetailById` 매핑 로직 수정

### Phase 2: DTO 및 Service
- [ ] `SeatLocationDto` 생성
- [ ] `TicketListRespDto` 필드 추가
- [ ] `EventDetailRespDto` 필드 추가
- [ ] `EventService.GetEventDetailWithTickets` 로직 수정

### 테스트 및 검증
- [ ] 빌드 성공 확인
- [ ] 단위 테스트 작성 및 실행
- [ ] API 수동 테스트 (Swagger)
- [ ] 레거시 데이터 테스트 (위치 정보 없는 티켓)
- [ ] 성능 테스트 (쿼리 실행 시간)

### 문서화
- [ ] API 문서 업데이트 (Swagger 주석)
- [ ] 변경 로그 작성
- [ ] 배포 가이드 작성

---

## 📝 구현 순서

1. **DB 스키마 확인** ✅
2. **TicketQueries.cs 수정** → SQL 쿼리 업데이트
3. **TicketListReadModel.cs 수정** → 새 필드 추가
4. **TicketRepository.cs 수정** → 매핑 로직 업데이트
5. **SeatLocationDto.cs 생성** → 새 DTO 파일
6. **TicketListRespDto.cs 수정** → 기존 DTO 확장
7. **EventDetailRespDto.cs 수정** → 필터 필드 추가
8. **EventService.cs 수정** → 로직 업데이트
9. **빌드 및 테스트**
10. **Codex 검증 요청**

---

## 📌 참고 정보

- **프로젝트명**: TicketHub
- **기술 스택**: ASP.NET Core 9, MySQL, Dapper
- **패턴**: Repository + Service + DTO
- **API 문서**: Swagger (Swashbuckle)

---

## 🚀 완료 기준

1. ✅ 모든 체크리스트 항목 완료
2. ✅ 빌드 에러 없음
3. ✅ API 응답에 새 필드 포함
4. ✅ 레거시 데이터 정상 처리
5. ✅ 성능 기준 충족 (<100ms)
6. ✅ Codex 검증 통과

---

**작성일**: 2026-01-20
**작성자**: Claude Code (Sonnet 4.5)
**검토자**: Codex GPT-5.2 (예정)
