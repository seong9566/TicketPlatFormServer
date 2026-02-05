백엔드 요청: 판매내역/구매내역 API
날짜: 2026-02-05  
우선순위: 일반  
요청자: Flutter 팀
---
📋 요청 개요
사용자의 판매내역과 구매내역을 조회하는 API 2개를 요청합니다.
---
1. 판매내역 조회 API
   GET /api/transactions/sales
   로그인한 사용자가 판매자로 참여한 거래 목록을 조회합니다.
   Request
   Headers
   Authorization: Bearer {access_token}
   Query Parameters
   | 파라미터 | 타입 | 필수 | 설명 |
   |---------|------|------|------|
   | status | string | N | 거래 상태 필터 (콤마 구분 가능) |
   | period | string | N | 기간 필터: 1w, 1m, 3m, 6m, all (기본값: all) |
   | sortBy | string | N | 정렬 기준: latest, oldest (기본값: latest) |
   | cursor | string | N | 페이지네이션 커서 (다음 페이지 조회 시) |
   | limit | int | N | 조회 개수 (기본값: 20, 최대: 50) |
   status 값
- reserved - 예약됨
- pending_payment - 결제 대기
- paid - 결제 완료
- confirmed - 구매 확정
- completed - 거래 완료
- cancelled - 취소됨
- refunded - 환불됨
  period 값
- 1w - 최근 1주일
- 1m - 최근 1개월
- 3m - 최근 3개월
- 6m - 최근 6개월
- all - 전체 기간 (기본값)
  Request 예시
  GET /api/transactions/sales?status=paid,confirmed&period=1m&sortBy=latest&limit=20
  Response
  성공 (200 OK)
  {
  items: [
  {
  transactionId: 123,
  ticketId: 456,
  ticketTitle: Bunnies Camp 2024,
  ticketThumbnailUrl: https://...,
  eventDateTime: 2024-03-15T19:00:00Z,
  venueName: 올림픽공원 체조경기장,
  seatInfo: VIP석 A구역 3열 15번,
  quantity: 2,
  unitPrice: 90000,
  totalAmount: 180000,
  statusCode: paid,
  statusName: 결제 완료,
  buyer: {
  userId: 789,
  nickname: 구매자닉네임,
  profileImageUrl: https://...
  },
  roomId: 101,
  createdAt: 2024-02-01T10:30:00Z,
  paidAt: 2024-02-01T11:00:00Z,
  confirmedAt: null,
  cancelledAt: null
  }
  ],
  nextCursor: eyJpZCI6MTIzLCJjcmVhdGVkQXQiOiIyMDI0LTAyLTAxIn0=,
  hasMore: true,
  totalCount: 45
  }
---
2. 구매내역 조회 API
   GET /api/transactions/purchases
   로그인한 사용자가 구매자로 참여한 거래 목록을 조회합니다.
   Request
   Headers
   Authorization: Bearer {access_token}
   Query Parameters
   | 파라미터 | 타입 | 필수 | 설명 |
   |---------|------|------|------|
   | status | string | N | 거래 상태 필터 (콤마 구분 가능) |
   | period | string | N | 기간 필터: 1w, 1m, 3m, 6m, all (기본값: all) |
   | sortBy | string | N | 정렬 기준: latest, oldest (기본값: latest) |
   | cursor | string | N | 페이지네이션 커서 (다음 페이지 조회 시) |
   | limit | int | N | 조회 개수 (기본값: 20, 최대: 50) |
   Response
   성공 (200 OK)
   {
   items: [
   {
   transactionId: 124,
   ticketId: 457,
   ticketTitle: NewJeans Fan Meeting,
   ticketThumbnailUrl: https://...,
   eventDateTime: 2024-04-20T18:00:00Z,
   venueName: KSPO DOME,
   seatInfo: 스탠딩 A구역,
   quantity: 1,
   unitPrice: 150000,
   totalAmount: 150000,
   statusCode: confirmed,
   statusName: 구매 확정,
   seller: {
   userId: 456,
   nickname: 판매자닉네임,
   profileImageUrl: https://...
   },
   roomId: 102,
   createdAt: 2024-02-05T14:00:00Z,
   paidAt: 2024-02-05T14:30:00Z,
   confirmedAt: 2024-02-06T10:00:00Z,
   cancelledAt: null
   }
   ],
   nextCursor: eyJpZCI6MTI0LCJjcmVhdGVkQXQiOiIyMDI0LTAyLTA1In0=,
   hasMore: false,
   totalCount: 12
   }
---
3. 응답 필드 상세 설명
   Transaction Item
   | 필드 | 타입 | 설명 |
   |------|------|------|
   | transactionId | int | 거래 고유 ID |
   | ticketId | int | 티켓 고유 ID (상세 화면 이동용) |
   | ticketTitle | string | 티켓 제목 (공연/이벤트명) |
   | ticketThumbnailUrl | string? | 티켓 썸네일 이미지 URL |
   | eventDateTime | string | 공연/이벤트 일시 (ISO 8601) |
   | venueName | string? | 공연장/장소명 |
   | seatInfo | string? | 좌석 정보 |
   | quantity | int | 거래 수량 |
   | unitPrice | int | 티켓 단가 (원) |
   | totalAmount | int | 총 거래 금액 (unitPrice × quantity) |
   | statusCode | string | 거래 상태 코드 |
   | statusName | string | 거래 상태 표시명 |
   | buyer | object | 구매자 정보 (판매내역에서만) |
   | seller | object | 판매자 정보 (구매내역에서만) |
   | roomId | int | 채팅방 ID (채팅방 이동용) |
   | createdAt | string | 거래 생성 일시 |
   | paidAt | string? | 결제 완료 일시 |
   | confirmedAt | string? | 구매 확정 일시 |
   | cancelledAt | string? | 취소 일시 |
   User 정보 (buyer/seller)
   | 필드 | 타입 | 설명 |
   |------|------|------|
   | userId | int | 사용자 고유 ID |
   | nickname | string | 닉네임 |
   | profileImageUrl | string? | 프로필 이미지 URL |
   Pagination 정보
   | 필드 | 타입 | 설명 |
   |------|------|------|
   | nextCursor | string? | 다음 페이지 커서 (없으면 null) |
   | hasMore | bool | 다음 페이지 존재 여부 |
   | totalCount | int | 전체 거래 수 (필터 적용 후) |
---
4. 에러 응답
   | HTTP Status | 코드 | 메시지 | 설명 |
   |-------------|------|--------|------|
   | 400 | INVALID_PARAMETER | 잘못된 파라미터입니다. | status, period, sortBy 값 오류 |
   | 401 | UNAUTHORIZED | 인증이 필요합니다. | 토큰 없음/만료 |
   | 500 | INTERNAL_ERROR | 서버 오류가 발생했습니다. | 서버 내부 오류 |
---
5. Flutter UI 사용 시나리오
   5.1 목록 화면 구조
   [마이페이지]
   ├── [판매내역] → GET /api/transactions/sales
   └── [구매내역] → GET /api/transactions/purchases
   5.2 상태 그룹화 (탭 UI)
   | 탭 | 포함 상태 |
   |----|----------|
   | 전체 | 모든 상태 |
   | 진행중 | reserved, pending_payment, paid |
   | 완료 | confirmed, completed |
   | 취소/환불 | cancelled, refunded |
   5.3 상세 화면 이동
- 목록 아이템 탭 → ticketId로 티켓 상세 화면 이동
- 채팅 버튼 탭 → roomId로 채팅방 이동
---
6. 질문/확인 사항
1. 거래 상태별 조회 가능한가요? (status 파라미터 콤마 구분)
2. 기간 필터 기준은 createdAt인가요?
3. totalCount는 필터 적용 후의 총 개수인가요?
4. 커서 방식의 페이지네이션 가능한가요?
5. 거래가 없는 경우 빈 배열 반환인가요?
6. 예상 배포 일정을 알려주시면 Flutter 구현 일정을 맞추겠습니다.