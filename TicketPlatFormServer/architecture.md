# 아키텍처
architecture.md 업데이트 주기: 월간 또는 아키텍처 수정 사항이 있을 경우

## 시스템 구조
- **모바일 앱**: Flutter
- **백엔드**: ASP.NET Core 9 (C#, net9.0)
- **데이터 모델 구조**:
  - 사용자/인증: `users` 1:1 `user_profile`, `refresh_tokens`, `auth_providers`, `auth_roles`, `user_verification`, `bank_account`
  - 이벤트/티켓: `ticket_category` 1:N `events`, `artists` 1:N `events`, `events` 1:N `event_schedules`, `events` 1:N `seat_locations`(전역 포함), `tickets` N:1 `events`, `tickets` N:1 `ticket_statuses`, `tickets` 1:N `ticket_images`/`ticket_price_history`
  - 거래/결제/정산: `transactions`(buyer_id/seller_id) 1:N `transaction_items`, `transaction_statuses`, `payments`/`payment_statuses`/`payment_methods`, `escrow` 1:1 `transactions`, `settlements`/`settlement_statuses`, `refunds`/`refund_statuses`/`refund_reasons`
  - 채팅/실시간: `chat_rooms` 1:N `chat_messages`, `chat_room_statuses`
  - 분쟁: `disputes`(transaction_id), `dispute_types`/`dispute_statuses`, `dispute_evidence`
  - 알림/평판: `notifications`/`notification_types`/`notification_platforms`/`notification_token`, `user_reputation`/`reputation_rating_types`
  - 기타: `user_favorites` + `favorite_types`, `artist_followers`, 관리자 로그(`admin_actions`, `admin_action_types`, `admin_target_types`)

## 모듈 구성
1. 사용자/인증 모듈: 회원/로그인/JWT/토큰 갱신
2. 이벤트/티켓 모듈: 공연, 판매/구매 티켓, 이미지
3. 거래/결제/정산 모듈: 거래 상태, 결제, 에스크로, 정산, 환불
4. 채팅/실시간 모듈: 채팅방/메시지, SignalR
5. 알림/평판 모듈: 푸시 토큰/알림, 유저 평판
6. 인프라 모듈: 파일 업로드/스토리지, 공통 예외 처리

## 데이터 흐름
Client → API Controller → Service → Repository/DB
 → (Storage Upload) → Object Storage
 → (Real-time) → SignalR Hub → Client UI Update

```
[Client]
   |
   v
[API Controller]
   |
   v
[Service] ---> [Object Storage] (file upload)
   |
   v
[Repository] ---> [DB]
   |
   v
[SignalR Hub] ---> [Client UI Update]
```
