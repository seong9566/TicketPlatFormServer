# TicketHub 개발 목표

## 최근 완료 (2026-01-16)

### Phase 1: 채팅 이미지 첨부 기능 개선
- ✅ Supabase 다중 버킷 지원 (profile-image, chat-image, ticket-image)
- ✅ 프로필 이미지 Signed URL 배치 처리 (성능 최적화)
- ✅ SignalR 실시간 메시지에 발신자 정보 추가
- ✅ Object Key 패턴 기반 버킷 자동 추론

**성능 개선**: 프로필 이미지 N번 조회 → 버킷별 1-3번 배치 조회

### Phase 2: 티켓 등록 스키마 개선 (DB 마이그레이션)
- ✅ 마스터 테이블 생성 및 기본 데이터 삽입
  - `seat_grades`: VIP, 일반, 지정석, 입장권 (4개)
  - `ticket_features`: 예매처 ID로 전달, 현장발권, 모바일티켓 등 (7개)
  - `trade_methods`: PIN거래, 배송거래, 현장거래, 기타거래 (4개)
  - `ticket_ticket_features`: 티켓-특징 다대다 관계 테이블
- ✅ `tickets` 테이블 스키마 변경
  - 추가: `seat_grade_id`, `trade_method_id`, `trade_description`, `has_ticket`
  - 제거: `seat_info`, `seat_features` (비구조화 데이터 정규화)
- ✅ 성능 인덱스 추가 (seat_grade, trade_method, has_ticket)
- ✅ 비즈니스 규칙 CHECK 제약: `price <= original_price` (원가 이하 판매 보장)

**개선 효과**: 비구조화 → 구조화, FK 데이터 무결성, DB 레벨 가격 검증, 쿼리 성능 향상

### 이전 완료
- ✅ Swagger 파일 업로드 에러 수정 (UpdateUserProfileReqDto 개선)
- ✅ 티켓 상세 조회 시 이미지 URL을 Signed URL로 변환
- ✅ Supabase 스토리지 구조 및 Signed URL 규칙 정립

## 다음 계획

### Phase 3: 티켓 등록 애플리케이션 코드 마이그레이션 ✅
- [x] **Entity 클래스 생성** (기존 확인)
  - `DBModel/SeatGrade.cs`, `DBModel/TradeMethod.cs`
  - `DBModel/TicketFeature.cs`, `DBModel/TicketTicketFeature.cs`
- [x] **Ticket Entity 업데이트** (`DBModel/Ticket.cs`) (기존 확인)
  - 제거: `SeatInfo`, `SeatFeatures` 속성 (완료)
  - 추가: `SeatGradeId`, `TradeMethodId`, `TradeDescription`, `HasTicket` 속성
  - Navigation 속성 추가: `SeatGrade`, `TradeMethod`, `TicketFeatures` (Many-to-Many)
- [x] **TicketContext 업데이트** (`Repository/TicketContext.cs`) (기존 확인)
  - 새 Entity DbSet 추가
  - Entity Configuration 추가 (FK 관계 설정)
- [x] **Dapper 쿼리 수정**
  - `TicketQueries.cs`: seat_info, seat_features 제거, 새 컬럼 추가
  - `FavoriteQueries.cs`: seat_info 제거, seat_grade/trade_method JOIN 추가
- [x] **Repository 로직 업데이트**
  - `TicketRepository.cs`: SeatFeatures JSON 파싱 로직 제거, 새 속성 매핑
  - `FavoriteRepository.cs`: SeatInfo 매핑 제거
- [x] **ReadModel 및 DTO 업데이트**
  - `TicketListReadModel`, `FavoriteTicketReadModel`
  - `TicketListRespDto`, `FavoriteTicketListRespDto`
  - `CreateSellTicketReqDto`, `MyTicketListRespDto`
- [x] **Service 로직 업데이트**
  - `TicketService.cs`, `EventService.cs`, `FavoriteService.cs`, `SellService.cs`
- [x] **테스트 및 검증**
  - 빌드 확인 ✅

### 향후 계획
- [ ] Phase 4: 안정성 및 리질리언스 강화 (파일 삭제 기능, 에러 처리 개선)
- [ ] 채팅방 정리 시 이미지 자동 삭제
- [ ] Redis 캐시 마이그레이션 검토 (다중 서버 배포 시)
