-- =====================================================
-- Rollback Migration: 거래 내역 조회 인덱스 제거
-- Created: 2026-02-09
-- Description: 20260209_001_add_transaction_history_indexes.sql의 롤백 스크립트
-- =====================================================

-- 추가된 인덱스 제거
DROP INDEX IF EXISTS idx_trans_buyer_created_id ON transactions;
DROP INDEX IF EXISTS idx_trans_seller_created_id ON transactions;
DROP INDEX IF EXISTS idx_trans_status_created ON transactions;

-- 롤백 확인
-- SHOW INDEX FROM transactions;
