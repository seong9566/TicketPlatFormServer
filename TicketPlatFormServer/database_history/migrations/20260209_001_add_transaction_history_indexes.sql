-- =====================================================
-- Migration: 거래 내역 조회 성능 최적화를 위한 인덱스 추가
-- Created: 2026-02-09
-- Description: 구매/판매 내역 API의 성능 향상을 위한 복합 인덱스 추가
-- =====================================================

-- =====================================================
-- FORWARD MIGRATION (UP)
-- =====================================================

-- 1. transactions 테이블: 구매 내역 조회 최적화
-- 용도: buyer_id로 필터링하고 created_at DESC, id DESC로 정렬하는 쿼리 최적화
-- 커버하는 쿼리: SELECT * FROM transactions WHERE buyer_id = ? ORDER BY created_at DESC, id DESC
CREATE INDEX IF NOT EXISTS idx_trans_buyer_created_id
ON transactions (buyer_id, created_at DESC, id DESC);

-- 2. transactions 테이블: 판매 내역 조회 최적화
-- 용도: seller_id로 필터링하고 created_at DESC, id DESC로 정렬하는 쿼리 최적화
-- 커버하는 쿼리: SELECT * FROM transactions WHERE seller_id = ? ORDER BY created_at DESC, id DESC
CREATE INDEX IF NOT EXISTS idx_trans_seller_created_id
ON transactions (seller_id, created_at DESC, id DESC);

-- 3. transactions 테이블: 상태별 필터링 및 날짜 범위 조회 최적화
-- 용도: status_id와 created_at으로 필터링하는 쿼리 최적화
-- 커버하는 쿼리: WHERE status_id IN (...) AND created_at >= ?
CREATE INDEX IF NOT EXISTS idx_trans_status_created
ON transactions (status_id, created_at DESC);

-- 4. transaction_items 테이블: transaction_id로 JOIN 최적화
-- 용도: transactions와 transaction_items 간의 JOIN 성능 향상
-- 참고: 이미 idx_trans_items_trans 인덱스가 존재하므로 별도 추가 불필요
-- 기존 인덱스 확인: SHOW INDEX FROM transaction_items WHERE Key_name = 'idx_trans_items_trans';

-- 5. chat_rooms 테이블: transaction_id로 JOIN 최적화
-- 용도: transactions와 chat_rooms 간의 LEFT JOIN 성능 향상
-- 참고: 이미 idx_chat_transaction 인덱스가 존재하므로 별도 추가 불필요
-- 기존 인덱스 확인: SHOW INDEX FROM chat_rooms WHERE Key_name = 'idx_chat_transaction';

-- =====================================================
-- 인덱스 생성 확인 쿼리
-- =====================================================
-- 생성된 인덱스 확인:
-- SHOW INDEX FROM transactions WHERE Key_name IN ('idx_trans_buyer_created_id', 'idx_trans_seller_created_id', 'idx_trans_status_created');

-- 인덱스 크기 확인:
-- SELECT
--     table_name,
--     index_name,
--     ROUND(stat_value * @@innodb_page_size / 1024 / 1024, 2) AS size_mb
-- FROM mysql.innodb_index_stats
-- WHERE database_name = 'TicketPlatFormDB'
--   AND table_name = 'transactions'
--   AND index_name IN ('idx_trans_buyer_created_id', 'idx_trans_seller_created_id', 'idx_trans_status_created')
--   AND stat_name = 'size';

-- =====================================================
-- BACKWARD MIGRATION (DOWN)
-- =====================================================

-- Rollback 시 실행할 스크립트:
-- DROP INDEX IF EXISTS idx_trans_buyer_created_id ON transactions;
-- DROP INDEX IF EXISTS idx_trans_seller_created_id ON transactions;
-- DROP INDEX IF EXISTS idx_trans_status_created ON transactions;

-- =====================================================
-- EXPLAIN 분석 예시
-- =====================================================

-- 구매 내역 조회 쿼리 실행 계획 분석:
/*
EXPLAIN SELECT
    t.id, t.created_at, t.status_id
FROM transactions t
WHERE t.buyer_id = 1
  AND t.deleted_at IS NULL
ORDER BY t.created_at DESC, t.id DESC
LIMIT 20;

-- 기대 결과:
-- type: ref (인덱스 사용)
-- possible_keys: idx_trans_buyer_created_id
-- key: idx_trans_buyer_created_id
-- rows: 실제 데이터 건수만큼만 스캔
-- Extra: Using index condition (인덱스만 사용하여 조회)
*/

-- 판매 내역 조회 쿼리 실행 계획 분석:
/*
EXPLAIN SELECT
    t.id, t.created_at, t.status_id
FROM transactions t
WHERE t.seller_id = 1
  AND t.deleted_at IS NULL
ORDER BY t.created_at DESC, t.id DESC
LIMIT 20;

-- 기대 결과:
-- type: ref
-- key: idx_trans_seller_created_id
-- Extra: Using index condition
*/

-- 상태 필터링 쿼리 실행 계획 분석:
/*
EXPLAIN SELECT
    t.id, t.created_at
FROM transactions t
INNER JOIN transaction_statuses ts ON t.status_id = ts.id
WHERE ts.code IN ('paid', 'confirmed')
  AND t.created_at >= DATE_SUB(NOW(), INTERVAL 1 MONTH)
  AND t.deleted_at IS NULL
ORDER BY t.created_at DESC
LIMIT 20;

-- 기대 결과:
-- type: range 또는 ref
-- key: idx_trans_status_created
-- Extra: Using index condition; Using where
*/

-- =====================================================
-- 성능 측정 쿼리
-- =====================================================

-- 인덱스 사용 전후 비교:
/*
-- 1. 쿼리 실행 시간 측정
SET profiling = 1;

-- 구매 내역 조회 쿼리 실행
SELECT ... FROM transactions WHERE buyer_id = 1 ORDER BY created_at DESC, id DESC LIMIT 20;

-- 실행 시간 확인
SHOW PROFILES;

-- 2. 인덱스 효율성 확인
SELECT
    table_name,
    index_name,
    cardinality,
    CASE
        WHEN cardinality IS NULL THEN 'Low'
        WHEN cardinality < 100 THEN 'Low'
        WHEN cardinality < 1000 THEN 'Medium'
        ELSE 'High'
    END AS selectivity
FROM information_schema.statistics
WHERE table_schema = 'TicketPlatFormDB'
  AND table_name = 'transactions'
  AND index_name IN ('idx_trans_buyer_created_id', 'idx_trans_seller_created_id', 'idx_trans_status_created');
*/

-- =====================================================
-- 주의사항
-- =====================================================
-- 1. 인덱스는 쓰기 성능에 영향을 줄 수 있으므로, 필요한 것만 추가
-- 2. 복합 인덱스의 컬럼 순서가 중요 (WHERE 절에 사용되는 컬럼을 앞에 배치)
-- 3. 정렬 방향(ASC/DESC)을 명시하여 인덱스 효율성 극대화
-- 4. 운영 환경에 적용 전 스테이징 환경에서 충분히 테스트
-- 5. 인덱스 생성 시 테이블 락이 발생할 수 있으므로, 트래픽이 적은 시간대에 실행 권장
--    (MySQL 8.0의 경우 ALGORITHM=INPLACE, LOCK=NONE 사용 가능)

-- =====================================================
-- 대용량 테이블의 경우 온라인 인덱스 생성 옵션
-- =====================================================
/*
-- 온라인으로 인덱스 생성 (다운타임 최소화):
CREATE INDEX idx_trans_buyer_created_id
ON transactions (buyer_id, created_at DESC, id DESC)
ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_trans_seller_created_id
ON transactions (seller_id, created_at DESC, id DESC)
ALGORITHM=INPLACE, LOCK=NONE;

CREATE INDEX idx_trans_status_created
ON transactions (status_id, created_at DESC)
ALGORITHM=INPLACE, LOCK=NONE;
*/
