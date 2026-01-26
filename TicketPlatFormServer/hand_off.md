## claude의 context을 위한 파일
## 작업 내용을 저장 하고 다음 세션에서 이어서 할 수 있도록 함

---

# 최근 작업 내역 (2026-01-26)

---

## ✅ 완료: 토스페이먼츠 결제 시스템 통합

### 작업 개요
중고 티켓 거래 플랫폼에 토스페이먼츠 결제 시스템 및 에스크로 기반 안전 거래 기능 구현 완료

### 구현 완료 항목 (2026-01-26)

#### 1. 신규 파일 생성 (5개)
- **`Repository/Payment/IPaymentRepository.cs`** - Payment/Escrow 레포지토리 인터페이스
  - Payment CRUD 메서드
  - Escrow 관리 메서드 (생성, 해제, 환불)
  - 상태 코드 매핑 메서드 (캐싱 적용)

- **`Repository/Payment/PaymentRepository.cs`** - DB 작업 구현체
  - EF Core + Dapper 하이브리드 패턴
  - IMemoryCache로 상태 코드 1시간 캐싱
  - TransactionRepository 패턴 준수

- **`Services/Payment/IPaymentService.cs`** - 결제 비즈니스 로직 인터페이스

- **`Services/Payment/PaymentService.cs`** - 핵심 결제 로직 구현
  - InitiatePaymentAsync: OrderId 생성 (`TXN_{TransactionId}_{Guid}`)
  - ConfirmPaymentAsync: Toss API 승인 + DB 트랜잭션 처리
  - ReleaseEscrowAsync: 구매 확정 시 에스크로 해제
  - CancelPaymentAsync: 결제 취소 및 환불
  - HandleWebhookAsync: Webhook 이벤트 처리
  - 중복 결제 방지 (Idempotency)
  - 금액 검증 로직

- **`Controllers/PaymentController.cs`** - 결제 API 엔드포인트
  - POST `/api/payment/request` - 결제 요청 준비
  - POST `/api/payment/confirm` - 결제 승인
  - POST `/api/payment/cancel` - 결제 취소/환불
  - GET `/api/payment/order/{orderId}` - 결제 조회
  - POST `/api/payment/webhook` - Toss Webhook 수신 (IP 화이트리스트)

#### 2. 기존 파일 수정 (6개)
- **`Config/TossPaymentsSettings.cs`**
  - EscrowFeePercentage 속성 추가 (기본값 3.5%)

- **`Repository/Transaction/ITransactionRepository.cs`**
  - UpdateTransactionStatusAsync 메서드 추가
  - GetTransactionWithDetailsAsync 메서드 추가

- **`Repository/Transaction/TransactionRepository.cs`**
  - 인터페이스 메서드 구현 (Dapper 사용)

- **`Services/Chat/ChatService.cs`**
  - IPaymentService 의존성 추가
  - RequestPayment: PaymentService.InitiatePaymentAsync 호출
  - ConfirmPurchase: PaymentService.ReleaseEscrowAsync 호출
  - Placeholder URL 제거

- **`Program.cs`**
  - TossPaymentsSettings 등록
  - HttpClient "TossPayments" 등록 (Polly 정책 적용)
  - PaymentRepository, TossPaymentsService, PaymentService DI 등록

- **`appsettings.json`**
  - TossPayments 설정 섹션 추가 (테스트 키, URL, 수수료율)

#### 3. 빌드 에러 수정
- **`Repository/TicketContext.cs`**
  - Payment 네임스페이스 충돌 해결
  - PaymentEntity alias 추가
  - DbSet<Payment> → DbSet<PaymentEntity>
  - modelBuilder.Entity<Payment> → modelBuilder.Entity<PaymentEntity>

- **`Services/Chat/ChatService.cs`**
  - room.Buyer?.Nickname → room.Buyer?.UserProfile?.Nickname

### 핵심 설계 결정

#### OrderId 생성 전략
- **형식**: `TXN_{TransactionId}_{Guid}`
- **예시**: `TXN_123_a7f3b2c14d5e6f7g8h9i0j1k2l3m4n5o`
- **장점**: Transaction ID 역추적 가능, Guid로 충돌 방지

#### Escrow 생성 시점
- **시점**: 결제 완료 시 생성 (결제 요청 시 ✗)
- **이유**: 실제 입금 확인 후에만 에스크로 보관, 실패한 결제 고아 레코드 방지

#### 수수료 계산
- **비율**: 3.5% (appsettings.json에서 설정 가능)
- **공식**:
  ```
  FeeAmount = Amount * 0.035
  SellerAmount = Amount - FeeAmount
  ```

#### 결제 흐름
```
채팅방에서 결제 요청 (판매자)
  ↓
OrderId 생성 → 프론트엔드에 전달
  ↓
사용자가 토스 위젯에서 결제
  ↓
프론트엔드 → /api/payment/confirm 호출
  ↓
Toss API 승인 요청 → Payment + Escrow 생성
  ↓
구매자가 "구매 확정" → Escrow 해제 (정산)
```

### 보안 기능
1. **Webhook IP 화이트리스트**: `52.79.60.235`, `13.124.227.214` (+ localhost)
2. **중복 결제 방지**: OrderId 기반 Idempotency 체크
3. **금액 검증**: Toss API 응답 금액과 요청 금액 비교
4. **권한 검증**: 결제 취소는 Buyer/Seller만 가능

### 에러 처리
- AppException 사용 (HttpStatusCode 포함)
- DB 트랜잭션 롤백 (결제 처리 실패 시)
- Polly 정책 (Retry + Circuit Breaker)
- 모든 주요 작업 로깅

### 현재 설정 (appsettings.json)
```json
{
  "TossPayments": {
    "SecretKey": "test_sk_YyZqmkKeP8gplRvEeEk3bQRxB9lG",
    "ClientKey": "test_ck_O6BYq7GWPVvglqzGdNwrNE5vbo1d",
    "ApiBaseUrl": "https://api.tosspayments.com",
    "IsTestMode": true,
    "SuccessUrl": "http://localhost:5173/payment/success",
    "FailUrl": "http://localhost:5173/payment/fail",
    "TimeoutSeconds": 30,
    "EscrowFeePercentage": 3.5
  }
}
#### 4. Supabase Storage 설정 및 검증 로직 개선
- **`appsettings.SupabaseStorage.json`**: `AllowedExtensions` 와일드카드(`*`) 제거 및 실제 허용 확장자 리스트(`.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.heic`) 명시
- **`FileUploadService.cs`** & **`MagicBytesValidator.cs`**:
  - 확장자 검증 시 대소문자 구분 없이 처리하도록 개선
  - `MagicBytesValidator`에 허용된 모든 확장자에 대한 검증 로직과 설정이 일치하도록 보장

---

## 🔄 다음 작업 (결제 시스템)

### Phase 1: 배포 전 필수 작업
1. **DB 상태 코드 확인 및 추가**
   ```sql
   -- payment_statuses 테이블
   INSERT INTO payment_statuses (code, name_ko, is_active, sort_order) VALUES
   ('pending', '결제 대기', true, 1),
   ('paid', '결제 완료', true, 2),
   ('cancelled', '결제 취소', true, 3);

   -- transaction_statuses 테이블
   INSERT INTO transaction_statuses (code, name_ko, is_active, sort_order) VALUES
   ('pending', '거래 대기', true, 1),
   ('payment_requested', '결제 요청됨', true, 2),
   ('paid', '결제 완료', true, 3),
   ('confirmed', '구매 확정', true, 4),
   ('cancelled', '거래 취소', true, 5);

   -- escrow_statuses 테이블
   INSERT INTO escrow_statuses (code, name_ko, is_active, sort_order) VALUES
   ('holding', '보관 중', true, 1),
   ('released', '정산 완료', true, 2),
   ('refunded', '환불 완료', true, 3);

   -- payment_methods 테이블
   INSERT INTO payment_methods (code, name_ko, is_active, sort_order) VALUES
   ('card', '카드', true, 1),
   ('virtual_account', '가상계좌', true, 2),
   ('transfer', '계좌이체', true, 3),
   ('mobile', '휴대폰', true, 4),
   ('easy_pay', '간편결제', true, 5);
   ```

2. **환경별 설정 분리**
   - appsettings.Development.json - 테스트 키
   - appsettings.Production.json - 운영 키
   - 운영 환경: 환경변수로 SecretKey 관리 (보안)

3. **프론트엔드 통합 가이드 작성**
   - 토스 결제 위젯 연동 방법
   - API 호출 시퀀스 다이어그램
   - 에러 처리 가이드

### Phase 2: End-to-End 테스트
1. **정상 결제 흐름 테스트**
   - 채팅방 생성 → 결제 요청 → 결제 승인 → 구매 확정
   - DB 상태 확인 (Payment, Escrow, Transaction)
   - 테스트 카드: `5123-4567-8901-2346`

2. **결제 취소/환불 테스트**
   - 결제 후 즉시 취소
   - 부분 취소 (향후)
   - DB 환불 상태 확인

3. **Webhook 테스트**
   - 토스 개발자센터 Webhook 시뮬레이터 사용
   - PAYMENT_STATUS_CHANGED 이벤트 처리 확인
   - 로그 및 DB 상태 검증

4. **오류 시나리오 테스트**
   - 잘못된 OrderId → 404 오류
   - 금액 불일치 → 400 오류
   - 중복 결제 시도 → 기존 결제 반환
   - 권한 없는 취소 → 403 오류
   - 네트워크 오류 → Polly Retry 동작 확인

### Phase 3: 고급 기능 (향후)
1. **Settlement 자동 생성**
   - Escrow 해제 시 Settlement 레코드 생성
   - 정산 스케줄러 구현 (매일 자정)

2. **부분 취소 지원**
   - 토스페이먼츠 부분 취소 API 연동
   - 다중 취소 내역 관리

3. **결제 관리 기능**
   - 관리자 대시보드 (결제 통계)
   - 결제 내역 조회 API
   - 정산 내역 조회 API

4. **알림 기능**
   - 결제 완료 이메일/SMS
   - 정산 완료 알림
   - Webhook 이벤트 알림

### Phase 4: 최적화
1. **캐싱 전략 강화**
   - PaymentMethod 매핑 영구 캐싱 (데이터 변경 시만 갱신)
   - Redis 도입 검토 (분산 캐싱)

2. **Webhook 안정성**
   - Webhook 재시도 로직 (실패 시)
   - Dead Letter Queue 구현

3. **성능 최적화**
   - DB 인덱스 추가 (payment_key, order_id)
   - 쿼리 성능 프로파일링

---

## 🔗 관련 문서
- 토스페이먼츠 공식 문서: https://docs.tosspayments.com
- 결제 승인 API: https://docs.tosspayments.com/reference#payment-confirm
- Webhook 가이드: https://docs.tosspayments.com/guides/webhook
- 테스트 카드: https://docs.tosspayments.com/reference#test-card

---

# 이전 작업 내역 (2026-01-22)

---

## ✅ 완료: ChatService 쿼리 방식 분석 및 아키텍처 리뷰

### 문제
- `ChatQueries.cs`에 Raw SQL 정의, `ChatRepository.cs`는 EF Core 사용
- 주석과 실제 코드 불일치 (주석: "Dapper 사용", 실제: EF Core)
- 어떤 방식을 채택해야 하는지 의문

### 분석 결과
**현재 상태**:
- `ChatQueries.cs`: Raw SQL 3개 정의 → **미사용**
- `ChatRepository.cs`: EF Core LINQ → **실제 사용 중**
- 혼재가 아닌 EF Core 단일 방식

**두 방식 비교**:
```
Raw SQL (Dapper)
  장점: 성능 최적화, 쿼리 명시적 제어
  단점: 타입 안정성 부족, 유지보수 어려움

EF Core LINQ
  장점: 타입 안전성, 생산성, DBMS 독립적
  단점: 성능 오버헤드 (미미함, <50ms 차이)
```

### 결론 및 추천
**✅ EF Core 방식 유지** (현재 방식)

**이유**:
1. 팀이 이미 EF Core 9 전면 사용 중 (일관성)
2. 타입 안전성으로 장기 유지보수 비용 절감
3. 성능 차이 미미 (사용자 체감 불가)
4. AsNoTracking, Include 최적화 잘 적용됨

### 액션 플랜
1. **ChatQueries.cs 삭제** (미사용 코드 제거)
2. **잘못된 주석 수정** (`ChatRepository.cs:49` - "Dapper 사용" 제거)
3. **Include 중복 제거** (확장 메서드로 추출)
4. **선택적 프로젝션 적용** (필요한 필드만 조회)
5. **인덱스 최적화** (가장 큰 성능 향상 기대)

### 하이브리드 접근 (향후)
- **기본**: EF Core 90%
- **예외**: Raw SQL 10% (대량 배치, 복잡한 통계 쿼리만)

---

## 🔄 다음 작업 (ChatService 최적화 - 보류)
- ChatQueries.cs 삭제 (미사용 코드 제거)
- 잘못된 주석 수정
- Include 중복 제거 (확장 메서드로 추출)

---

## 📊 아키텍처 결정 사항

### 결제 시스템 설계
- **PG사**: 토스페이먼츠
- **에스크로**: 결제 완료 시 생성 (3.5% 수수료)
- **OrderId 형식**: `TXN_{TransactionId}_{Guid}`
- **중복 방지**: Idempotency 체크
- **보안**: Webhook IP 화이트리스트, 금액 검증, 권한 검증
- **에러 처리**: DB 트랜잭션 롤백, Polly Retry + Circuit Breaker

### Repository 패턴 쿼리 전략 (기존)
- **채택**: EF Core LINQ (AsNoTracking + Include)
- **제외**: Raw SQL (Dapper) - 필요 시 선택적 사용만
- **근거**: 타입 안전성, 팀 숙련도, 일관성 > 미미한 성능 차이

---

**마지막 업데이트**: 2026-01-26
**상태**:
- ✅ 토스페이먼츠 결제 시스템 통합 완료 (Phase 1)
- ✅ 빌드 에러 수정 완료 (Payment 네임스페이스 충돌, User.Nickname)
- ✅ Supabase Storage 허용 확장자 및 Magic Bytes 검증 로직 고도화
- 🔄 다음: DB 상태 코드 확인 및 End-to-End 테스트
