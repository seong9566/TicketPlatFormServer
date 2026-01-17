# GetFavoriteTicketsByUserId 리뷰 기반 방향 제안

## 배경
- 대상 메서드: `FavoriteRepository.GetFavoriteTicketsByUserId`
- DB 사실: `tickets.created_at`, `user_profile.total_trade_count`는 NOT NULL 보장
- 사용 제약: 사용자당 즐겨찾기 최대 20개

## 핵심 방향
1) 모델/매핑을 NOT NULL 보장에 맞게 정리
- Dapper 매핑과 ReadModel/DBModel 타입 불일치를 해소해 런타임 변환 실패 위험을 줄임.
- `Ticket.CreatedAt` 및 `UserProfile.TotalTradeCount`를 nullable에서 non-nullable로 정리.
- Dapper 매핑에서 `Convert`/null 체크를 단순화해 코드 가독성 향상.

2) 쿼리 성능은 현 구조 유지 가능하나 최소한의 안전장치 권장
- 즐겨찾기 20개 제한이면 서브쿼리(응답률 계산)는 현재도 허용 가능.
- 다만 응답률 계산이 행 단위로 수행되므로, 필요 시 인덱스 최적화를 검토.

## 구체적 제안
### A. 모델/타입 정합성 정리 (권장)
- `TicketPlatFormServer/DBModel/Ticket.cs`:
  - `CreatedAt`을 `DateTime`으로 정리(필드 NOT NULL 기준).
- `TicketPlatFormServer/DBModel/UserProfile.cs`:
  - `TotalTradeCount`를 `int`로 정리(필드 NOT NULL 기준).
- `TicketPlatFormServer/Repository/ReadModels/SellerInfoReadModel.cs`:
  - `TotalTradeCount`가 `int`이므로, Dapper 매핑에서 null 변환 로직 제거.

### B. Dapper 매핑 개선 (선택)
- `QueryAsync<dynamic>` 대신 명시적 DTO로 매핑해 타입 안정성 강화.
- 예: `FavoriteTicketRow`(쿼리 결과 전용)를 정의 후 read model로 변환.
- 컬럼명 변경 시 런타임 오류를 조기에 탐지 가능.

### C. 응답률 계산 성능 (필요 시)
- 현재는 즐겨찾기 20개 제한으로 실무적으로 허용 가능.
- 트래픽 증가 시 아래 중 하나 고려:
  - 판매자별 응답률을 별도 테이블에 주기적으로 집계.
  - `chat_rooms.seller_id`, `chat_messages.room_id`, `chat_messages.sender_id` 인덱스 확인.

## 결정 기준 제안
- 당장 적용: A (NOT NULL 반영, 타입 정합성 정리)
- 여유 있을 때: B (타입 안정성 개선)
- 트래픽 증가 시: C (응답률 집계/인덱스)

## 오픈 포인트
- NOT NULL 보장된 컬럼에 대한 스키마/EF 스캐폴딩 동기화 방식 확인 필요
  - 수동 수정 vs. 스캐폴딩 재생성 중 선택
