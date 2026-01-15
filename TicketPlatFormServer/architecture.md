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

## 7. 스토리지 아키텍처 (Supabase)

### 버킷 구조
- **Bucket Name**: `chat-images` (단일 버킷 통합 사용)
- **접근 제어**: Signed URL 사용 (Private Bucket 권장)

### 디렉토리 구조 (Object Key)
| 용도 | 경로 패턴 | 예시 |
|------|-----------|------|
| 채팅 이미지 | `chat/{roomId}/{filename}` | `chat/101/abc_123.jpg` |
| 프로필 이미지 | `profiles/{userId}/{filename}` | `profiles/12/def_456.jpg` |
| 티켓 이미지 | `tickets/{ticketId}/{filename}` | `tickets/55/ghi_789.jpg` |

> **Note**: `profiles/` 경로는 기존 `user-profile-images/`에서 단순화됨 (2026.01.15)

### Signed URL 처리 규칙
1. **URL 생성**: Supabase API (`/storage/v1/object/sign/{bucket}/{key}`) 사용
2. **Access URL 수정**:
   - API 반환값: `/object/sign/{bucket}/...`
   - 실제 접근값: `/storage/v1/object/sign/{bucket}/...` (`/storage/v1` 접두사 필수)
3. **만료 정책**:
   - 업로드 직후: 1시간
   - 조회 시 (갱신): 30분
   - 클라이언트 캐싱 적극 활용 권장

