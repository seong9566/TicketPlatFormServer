# 거래 내역 조회 API 성능 최적화 가이드

## 개요
구매/판매 내역 조회 API의 성능을 향상시키기 위한 최적화 작업 문서입니다.

## 적용된 최적화 사항

### 1. 데이터베이스 인덱스 추가

#### 1.1 추가된 인덱스 목록

```sql
-- 구매 내역 조회 최적화
CREATE INDEX idx_trans_buyer_created_id
ON transactions (buyer_id, created_at DESC, id DESC);

-- 판매 내역 조회 최적화
CREATE INDEX idx_trans_seller_created_id
ON transactions (seller_id, created_at DESC, id DESC);

-- 상태별 필터링 최적화
CREATE INDEX idx_trans_status_created
ON transactions (status_id, created_at DESC);
```

#### 1.2 인덱스 설계 근거

**복합 인덱스의 컬럼 순서:**
1. **필터 컬럼 (buyer_id/seller_id)**: WHERE 절에서 사용
2. **정렬 컬럼 (created_at DESC)**: ORDER BY 절에서 사용
3. **고유성 컬럼 (id DESC)**: 페이지네이션 커서에서 사용

**정렬 방향 명시:**
- `DESC` 명시로 인덱스 역순 스캔 방지
- 최신 데이터 조회 시 인덱스 효율성 극대화

#### 1.3 인덱스 적용 방법

**운영 환경 적용:**
```bash
# 1. 마이그레이션 스크립트 실행
mysql -u username -p TicketPlatFormDB < 20260209_001_add_transaction_history_indexes.sql

# 2. 인덱스 생성 확인
mysql -u username -p TicketPlatFormDB -e "SHOW INDEX FROM transactions WHERE Key_name IN ('idx_trans_buyer_created_id', 'idx_trans_seller_created_id', 'idx_trans_status_created');"
```

**대용량 테이블의 경우 (다운타임 최소화):**
```sql
-- 온라인 인덱스 생성 (MySQL 8.0+)
CREATE INDEX idx_trans_buyer_created_id
ON transactions (buyer_id, created_at DESC, id DESC)
ALGORITHM=INPLACE, LOCK=NONE;
```

#### 1.4 롤백 방법

```bash
# 롤백 스크립트 실행
mysql -u username -p TicketPlatFormDB < 20260209_001_add_transaction_history_indexes_rollback.sql
```

---

### 2. COUNT 쿼리 최적화

#### 2.1 변경 전
```csharp
// 매 요청마다 전체 건수 조회
var totalCount = await db.ExecuteScalarAsync<int>(countQuery, parameters);
```

**문제점:**
- 11개 테이블 조인된 복잡한 쿼리에서 COUNT(DISTINCT) 실행
- 페이지네이션 시 매번 전체 데이터 스캔
- 데이터가 많을수록 성능 저하

#### 2.2 변경 후
```csharp
// 첫 페이지에서만 전체 건수 조회
var isFirstPage = string.IsNullOrWhiteSpace(cursor);
var (items, totalCount) = await repository.GetPurchaseHistoryAsync(
    userId, status, period, sortBy, cursorId, cursorCreatedAt,
    actualLimit + 1,
    includeTotalCount: isFirstPage  // 첫 페이지만 true
);
```

**개선 효과:**
- 첫 페이지: TotalCount 반환 (예: `"totalCount": 150`)
- 이후 페이지: TotalCount null 반환 (예: `"totalCount": null`)
- 페이지네이션 시 COUNT 쿼리 생략으로 약 50% 성능 향상 예상

#### 2.3 API 응답 변경

**응답 DTO 변경:**
```csharp
public class TransactionHistoryRespDto
{
    public List<TransactionHistoryItemDto> Items { get; set; }
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public int? TotalCount { get; set; }  // int -> int? (nullable)
}
```

**응답 예시:**

첫 페이지:
```json
{
  "items": [...],
  "nextCursor": "eyJpZCI6MTAsImNyZWF0ZWRBdCI6IjIwMjYtMDItMDVUMTA6MzA6MDBaIn0=",
  "hasMore": true,
  "totalCount": 150
}
```

두 번째 페이지 이후:
```json
{
  "items": [...],
  "nextCursor": "eyJpZCI6MzAsImNyZWF0ZWRBdCI6IjIwMjYtMDItMDRUMDk6MjA6MDBaIn0=",
  "hasMore": true,
  "totalCount": null
}
```

---

### 3. 쿼리 성능 분석 가이드

#### 3.1 EXPLAIN을 사용한 실행 계획 분석

**구매 내역 조회 쿼리 분석:**
```sql
EXPLAIN SELECT
    t.id, t.created_at, t.status_id,
    ti.ticket_id, ti.quantity, ti.unit_price
FROM transactions t
INNER JOIN transaction_statuses ts ON t.status_id = ts.id
INNER JOIN transaction_items ti ON t.id = ti.transaction_id
WHERE t.buyer_id = 1
  AND t.deleted_at IS NULL
ORDER BY t.created_at DESC, t.id DESC
LIMIT 20;
```

**기대되는 실행 계획:**
| id | select_type | table | type | key | rows | Extra |
|----|-------------|-------|------|-----|------|-------|
| 1 | SIMPLE | t | ref | idx_trans_buyer_created_id | ~20 | Using index condition |
| 1 | SIMPLE | ts | eq_ref | PRIMARY | 1 | Using where |
| 1 | SIMPLE | ti | ref | idx_trans_items_trans | 1 | NULL |

**주요 확인 사항:**
- ✅ `type: ref` (인덱스 사용)
- ✅ `key: idx_trans_buyer_created_id` (우리가 만든 인덱스 사용)
- ✅ `rows: ~20` (실제 필요한 건수만 스캔)
- ❌ `type: ALL` (전체 테이블 스캔 - 문제)
- ❌ `Extra: Using filesort` (정렬 위해 추가 작업 - 문제)

#### 3.2 쿼리 프로파일링

```sql
-- 프로파일링 활성화
SET profiling = 1;

-- 쿼리 실행
SELECT ... FROM transactions WHERE buyer_id = 1 ORDER BY created_at DESC LIMIT 20;

-- 실행 시간 확인
SHOW PROFILES;

-- 상세 분석
SHOW PROFILE FOR QUERY 1;
```

#### 3.3 인덱스 효율성 확인

```sql
-- 인덱스 카디널리티 확인 (높을수록 좋음)
SELECT
    table_name,
    index_name,
    cardinality,
    CASE
        WHEN cardinality IS NULL THEN 'Not Analyzed'
        WHEN cardinality < 100 THEN 'Low Selectivity'
        WHEN cardinality < 1000 THEN 'Medium Selectivity'
        ELSE 'High Selectivity'
    END AS selectivity_level
FROM information_schema.statistics
WHERE table_schema = 'TicketPlatFormDB'
  AND table_name = 'transactions'
  AND index_name IN ('idx_trans_buyer_created_id', 'idx_trans_seller_created_id');
```

#### 3.4 슬로우 쿼리 모니터링

**슬로우 쿼리 로그 설정:**
```sql
-- 슬로우 쿼리 로그 활성화 (2초 이상 걸리는 쿼리 기록)
SET GLOBAL slow_query_log = 'ON';
SET GLOBAL long_query_time = 2;
SET GLOBAL slow_query_log_file = '/var/log/mysql/slow-query.log';

-- 인덱스 미사용 쿼리도 기록
SET GLOBAL log_queries_not_using_indexes = 'ON';
```

**슬로우 쿼리 로그 분석:**
```bash
# 가장 느린 쿼리 Top 10
mysqldumpslow -s t -t 10 /var/log/mysql/slow-query.log

# 가장 많이 실행된 슬로우 쿼리 Top 10
mysqldumpslow -s c -t 10 /var/log/mysql/slow-query.log
```

---

### 4. 성능 측정 결과 (예상)

#### 4.1 인덱스 적용 전후 비교

| 시나리오 | 인덱스 적용 전 | 인덱스 적용 후 | 개선율 |
|---------|--------------|--------------|--------|
| 첫 페이지 조회 (COUNT 포함) | ~300ms | ~80ms | 73% ↓ |
| 두 번째 페이지 이후 (COUNT 제외) | ~250ms | ~30ms | 88% ↓ |
| 상태 필터링 포함 조회 | ~400ms | ~100ms | 75% ↓ |

**측정 조건:**
- 데이터: transactions 10,000건
- 환경: MySQL 8.0, 16GB RAM
- 동시 접속: 100명

#### 4.2 COUNT 쿼리 최적화 효과

```
첫 페이지:
- 변경 전: COUNT(150ms) + 데이터 조회(80ms) = 230ms
- 변경 후: COUNT(150ms) + 데이터 조회(80ms) = 230ms
- 개선율: 0% (변화 없음)

두 번째 페이지 이후:
- 변경 전: COUNT(150ms) + 데이터 조회(80ms) = 230ms
- 변경 후: 데이터 조회(80ms) = 80ms
- 개선율: 65% ↓
```

---

### 5. 추가 최적화 고려 사항

#### 5.1 커버링 인덱스 (Covering Index)

현재 쿼리가 많은 컬럼을 조회하므로 커버링 인덱스 적용은 어려움.
일부 단순 조회의 경우 커버링 인덱스 고려:

```sql
-- 예: 트랜잭션 ID와 상태만 조회하는 경우
CREATE INDEX idx_trans_buyer_status_id
ON transactions (buyer_id, status_id, id, created_at);
```

#### 5.2 파티셔닝

데이터가 수백만 건 이상으로 증가할 경우 파티셔닝 고려:

```sql
-- 월별 파티셔닝 예시
ALTER TABLE transactions
PARTITION BY RANGE (YEAR(created_at) * 100 + MONTH(created_at)) (
    PARTITION p202601 VALUES LESS THAN (202602),
    PARTITION p202602 VALUES LESS THAN (202603),
    PARTITION p202603 VALUES LESS THAN (202604),
    ...
    PARTITION pmax VALUES LESS THAN MAXVALUE
);
```

#### 5.3 읽기 전용 복제 서버 활용

대량 조회 트래픽 분산:
```
Master (쓰기) ───┐
                 ├─> 애플리케이션
Slave (읽기)  ───┘
```

#### 5.4 쿼리 캐싱 (Application Level)

```csharp
// IMemoryCache를 사용한 캐싱 예시
public async Task<TransactionHistoryRespDto> GetPurchaseHistoryAsync(...)
{
    var cacheKey = $"purchase_history:{userId}:{status}:{period}:{sortBy}:{cursor}";

    if (_cache.TryGetValue(cacheKey, out TransactionHistoryRespDto? cached))
    {
        logger.LogDebug("캐시에서 구매 내역 조회 - CacheKey: {CacheKey}", cacheKey);
        return cached;
    }

    var result = await repository.GetPurchaseHistoryAsync(...);

    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

    return result;
}
```

**주의사항:**
- 실시간성이 중요한 데이터는 캐싱 시간 최소화
- 캐시 무효화 전략 필수 (거래 상태 변경 시)

---

### 6. 모니터링 및 유지보수

#### 6.1 성능 모니터링 체크리스트

- [ ] 슬로우 쿼리 로그 주기적 확인 (주 1회)
- [ ] 인덱스 카디널리티 확인 (월 1회)
- [ ] 인덱스 크기 모니터링 (월 1회)
- [ ] EXPLAIN 실행 계획 검증 (분기 1회)
- [ ] 평균 응답 시간 모니터링 (실시간)

#### 6.2 인덱스 통계 갱신

```sql
-- 인덱스 통계 갱신 (데이터 변화가 많을 경우 주기적 실행)
ANALYZE TABLE transactions;

-- 인덱스 최적화
OPTIMIZE TABLE transactions;
```

#### 6.3 성능 저하 시 대응

**증상별 대응 방안:**

| 증상 | 원인 | 해결 방법 |
|-----|------|----------|
| 응답 시간 점진적 증가 | 데이터 증가 | 인덱스 통계 갱신, 파티셔닝 검토 |
| 특정 시간대 성능 저하 | 동시 접속 증가 | 읽기 복제 서버 추가, 커넥션 풀 조정 |
| COUNT 쿼리 느림 | 데이터 과다 | COUNT 캐싱, 근사치 사용 검토 |
| JOIN 성능 저하 | 관련 테이블 인덱스 부족 | 외래 키 컬럼 인덱스 확인 |

---

### 7. 참고 자료

**MySQL 인덱스 최적화:**
- [MySQL 8.0 Reference Manual - Optimization](https://dev.mysql.com/doc/refman/8.0/en/optimization.html)
- [High Performance MySQL, 4th Edition](https://www.oreilly.com/library/view/high-performance-mysql/9781492080503/)

**ASP.NET Core 성능 최적화:**
- [Performance Best Practices for ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)
- [Dapper Performance Tips](https://github.com/DapperLib/Dapper#performance)

---

## 적용 체크리스트

### 배포 전
- [ ] 스테이징 환경에서 인덱스 생성 테스트
- [ ] EXPLAIN으로 쿼리 실행 계획 확인
- [ ] 성능 측정 (인덱스 적용 전후 비교)
- [ ] 롤백 스크립트 준비 및 테스트

### 배포 시
- [ ] 트래픽이 적은 시간대 선택
- [ ] 데이터베이스 백업
- [ ] 인덱스 생성 스크립트 실행
- [ ] 인덱스 생성 완료 확인
- [ ] 애플리케이션 배포 (코드 변경 사항)

### 배포 후
- [ ] API 응답 시간 모니터링 (30분간)
- [ ] 슬로우 쿼리 로그 확인
- [ ] 에러 로그 확인
- [ ] 사용자 피드백 수집

---

**작성일:** 2026-02-09
**작성자:** ASP.NET MySQL Expert Agent
**버전:** 1.0
