## claude의 context을 위한 파일
## 작업 내용을 저장 하고 다음 세션에서 이어서 할 수 있도록 함

---

# 최근 작업 내역 (2026-01-29)

---

## ✅ 완료: 토스페이먼츠 API 응답 DTO 완전 대응 및 DB 스키마 동기화

### 작업 개요
토스페이먼츠 결제 성공 시 반환되는 실제 API 응답 형식에 완전히 대응하도록 DTO, DB 스키마, Entity 모델, 서비스 로직을 전면 개선. Codex(gpt-5.2-codex)를 활용한 DB 설계 검증 완료.

### 구현 완료 항목 (2026-01-29)

#### 1. DTO 필드 확장 (TossPaymentResponseDto.cs)
**추가된 최상위 필드 (16개)**:
- `mId` (가맹점 ID), `version` (API 버전), `lastTransactionKey` (최종 거래 키)
- `useEscrow` (에스크로 사용 여부), `cultureExpense` (문화비 지출 여부)
- `type` (결제 타입: NORMAL, BILLING), `country` (국가 코드)
- `isPartialCancelable` (부분 취소 가능 여부), `secret` (비밀 정보)
- `metadata` (메타데이터), `discount` (할인 정보)
- `checkout` (결제창 URL), `mobilePhone`, `giftCertificate`, `cashReceipt`, `cashReceipts`

**TossCardDto 추가 필드 (5개)**:
- `issuerCode` (발급사 코드), `acquirerCode` (매입사 코드)
- `interestPayer` (무이자 할부 부담자), `useCardPoint` (포인트 사용), `amount` (카드 결제 금액)

**TossVirtualAccountDto 추가 필드 (2개)**:
- `accountType` (계좌 타입), `refundReceiveAccount` (환불 계좌 정보)

**새로운 결제 수단 DTO (4개)**:
- `TossCheckoutDto` (결제창 정보)
- `TossMobilePhoneDto` (휴대폰 결제)
- `TossGiftCertificateDto` (상품권 결제)
- `TossCashReceiptDto` (현금영수증)

#### 2. MySQL DB 마이그레이션 (Codex 검증 완료)
**파일**: `migrations/001_improved_toss_payments_integration.sql`

**Phase 1: payments 테이블 확장**
- PK 타입 변경: `id` BIGINT → BIGINT UNSIGNED
- Amount 타입 변경: INT → BIGINT UNSIGNED
- 11개 새 컬럼 추가:
  - `use_escrow`, `is_partial_cancelable`, `payment_type`, `last_transaction_key`
  - `merchant_id`, `api_version`, `country` (CHAR(2))
  - `culture_expense`, `metadata` (JSON), `discount_info` (JSON)
- Collation 변경: `payment_key`, `order_id` → utf8mb4_0900_as_cs (대소문자 구분)
- UNIQUE 인덱스 추가: `payment_key`, `order_id`
- 일반 인덱스 추가: `transaction_id`, `payment_type`, `merchant_id`, `paid_at`, `status_id`

**Phase 2: 결제 수단별 상세 테이블 (4개)**
- `payment_card_details` (15개 필드) - 카드 결제 상세
- `payment_virtual_account_details` (12개 필드) - 가상계좌 상세
- `payment_easy_pay_details` (5개 필드) - 간편결제 상세
- `payment_cash_receipts` (8개 필드) - 현금영수증

**Phase 3: 거래 히스토리 테이블**
- `payment_transactions` (13개 필드) - 결제 거래 이벤트 로그
  - `balance_amount`, `tax_free_amount`, `currency`, `event_at` 포함
  - 복합 인덱스: `(payment_id, created_at)`

**Codex 검증 결과 반영**:
- ✅ Online DDL 최적화 (`ALGORITHM=INSTANT, LOCK=NONE`)
- ✅ 중복 데이터 사전 정리 (ROW_NUMBER() 윈도우 함수)
- ✅ FK 타입 일관성 (모든 payment_id → BIGINT UNSIGNED)
- ✅ JSON + 암호화 타입 불일치 해결 (TEXT 타입 사용)
- ✅ 1:1 관계 강제 (UNIQUE INDEX on payment_id)
- ✅ ON DELETE RESTRICT (하드 딜리트 방지)

#### 3. EF Core Entity 모델 업데이트 (6개 파일)
**신규 엔티티 생성 (5개)**:
- `DBModel/PaymentCardDetail.cs` - 카드 결제 상세
- `DBModel/PaymentVirtualAccountDetail.cs` - 가상계좌 상세
- `DBModel/PaymentEasyPayDetail.cs` - 간편결제 상세
- `DBModel/PaymentCashReceipt.cs` - 현금영수증
- `DBModel/PaymentTransaction.cs` - 결제 거래 이벤트

**Payment.cs 수정**:
- 11개 새 필드 추가 (useEscrow, isPartialCancelable, paymentType 등)
- Navigation Properties 추가 (CardDetail, VirtualAccountDetail, Transactions 등)
- `Id` 타입 변경: long → ulong

#### 4. 암호화 서비스 구현
**파일**: `Services/Common/EncryptionService.cs`

**기능**:
- AES-256-GCM 암호화/복호화
- Base64 인코딩 자동 적용
- PBKDF2 키 파생 (100,000 반복)
- Nullable 헬퍼 메서드 (`EncryptNullable`, `DecryptNullable`)

**암호화 대상 필드**:
- `payment_virtual_account_details.secret`
- `payment_virtual_account_details.refund_receive_account`
- `payment_transactions.toss_response`

**설정**: `appsettings.json`
```json
{
  "Encryption": {
    "MasterKey": "TicketPlatform-AES256-Encryption-Master-Key-2026-Secure-Payment-Data-Protection"
  }
}
```

#### 5. Repository 메서드 확장 (11개 메서드 추가)
**IPaymentRepository.cs & PaymentRepository.cs**:

**생성 메서드 (5개)**:
- `CreateCardDetailAsync`
- `CreateVirtualAccountDetailAsync`
- `CreateEasyPayDetailAsync`
- `CreateCashReceiptAsync`
- `CreateTransactionAsync`

**조회 메서드 (4개)**:
- `GetCardDetailByPaymentIdAsync`
- `GetVirtualAccountDetailByPaymentIdAsync`
- `GetEasyPayDetailByPaymentIdAsync`
- `GetTransactionsByPaymentIdAsync`

**기타**:
- `UpdatePaymentStatusAsync` 시그니처 변경 (long → ulong)

#### 6. PaymentService 로직 개선
**파일**: `Services/Payment/PaymentService.cs`

**ConfirmPaymentAsync 메서드 수정**:
- TossPaymentResponseDto의 모든 새 필드 → Payment 엔티티 매핑
- 카드/가상계좌/간편결제/현금영수증 detail 테이블 저장 로직 추가
- PaymentTransaction 이벤트 로그 자동 생성
- 암호화 필요 필드 자동 암호화 (EncryptionService 사용)
- JSON 필드 직렬화 (`metadata`, `discount_info`)

**수정 내용**:
```csharp
// 기존
var payment = new Payment {
    TransactionId = transactionId,
    PgProvider = "toss",
    Amount = tossResponse.TotalAmount,
    // ...
};

// 개선 후
var payment = new Payment {
    TransactionId = transactionId,
    PgProvider = "toss",
    MerchantId = tossResponse.MId,
    ApiVersion = tossResponse.Version,
    Country = tossResponse.Country ?? "KR",
    Amount = tossResponse.TotalAmount,
    UseEscrow = tossResponse.UseEscrow,
    IsPartialCancelable = tossResponse.IsPartialCancelable,
    Metadata = JsonSerializer.Serialize(tossResponse.Metadata),
    // ... + 카드/가상계좌/간편결제 detail 저장
};
```

### 보안 강화

#### 암호화 필드 (AES-256-GCM + Base64)
1. **가상계좌 시크릿**: `payment_virtual_account_details.secret`
2. **환불 계좌 정보**: `payment_virtual_account_details.refund_receive_account`
3. **토스 API 응답 전문**: `payment_transactions.toss_response`

#### PCI DSS 준수
- 카드번호는 **마스킹된 값만 저장** (예: 1234-****-****-5678)
- CVV, 전체 PAN 저장 금지
- 민감 컬럼 접근 로그 감사 권장

#### 데이터 무결성
- UNIQUE 제약으로 중복 방지 (`payment_key`, `order_id`, `receipt_key`)
- FK RESTRICT로 데이터 삭제 방지
- 1:1 관계 강제 (detail 테이블)

### 기술적 의사결정

#### DB 설계 검증 (Codex gpt-5.2-codex)
- **검증 단계**: 2회 (초기 설계 → 피드백 반영 → 재검증)
- **주요 피드백**:
  1. Online DDL 안전성 (AFTER 제거, ALGORITHM=INSTANT)
  2. Amount 데이터 타입 (INT → BIGINT UNSIGNED)
  3. Idempotency 보장 (UNIQUE 인덱스)
  4. JSON + 암호화 타입 불일치 (JSON → TEXT)
  5. Case-sensitive collation (utf8mb4_0900_as_cs)

#### 타입 변환 전략
- MySQL `BIGINT UNSIGNED` ↔ C# `ulong`
- MySQL `JSON` ↔ C# `string` (JsonSerializer 사용)
- MySQL `CHAR(2)` ↔ C# `string` (국가 코드)

#### 마이그레이션 순서
1. FK 제거 (`refunds.payment_id`)
2. payments.id 타입 변경 (BIGINT → BIGINT UNSIGNED)
3. refunds.payment_id 타입 변경
4. FK 재생성
5. 나머지 컬럼 추가 및 인덱스 생성

### 변경된 파일 목록

**신규 생성 (8개)**:
1. `DBModel/PaymentCardDetail.cs`
2. `DBModel/PaymentVirtualAccountDetail.cs`
3. `DBModel/PaymentEasyPayDetail.cs`
4. `DBModel/PaymentCashReceipt.cs`
5. `DBModel/PaymentTransaction.cs`
6. `Services/Common/EncryptionService.cs`
7. `migrations/001_improved_toss_payments_integration.sql`

**수정 (8개)**:
1. `DTO/Payment/TossPaymentResponseDto.cs` - 27개 필드 추가
2. `DBModel/Payment.cs` - 11개 필드 + Navigation Properties
3. `Repository/Payment/IPaymentRepository.cs` - 11개 메서드 시그니처
4. `Repository/Payment/PaymentRepository.cs` - 11개 메서드 구현
5. `Services/Payment/PaymentService.cs` - ConfirmPaymentAsync 로직 개선
6. `Program.cs` - EncryptionService DI 등록
7. `appsettings.json` - Encryption 설정 추가

### 빌드 및 테스트 상태
- ✅ **빌드 성공** (0 errors, 0 warnings)
- ✅ **DB 마이그레이션 적용 완료** (MySQL MCP 사용)
- ✅ **검증 쿼리 실행 완료**
  - payments 테이블: 11개 새 컬럼 확인
  - UNIQUE 인덱스: payment_key, order_id 확인
  - 5개 새 테이블 생성 확인

### 다음 작업 (권장)

#### 필수 작업
1. **End-to-End 테스트**
   - 실제 토스페이먼츠 API로 결제 승인 테스트
   - 카드/가상계좌/간편결제 각 결제 수단별 테스트
   - detail 테이블 데이터 저장 확인
   - 암호화된 필드 복호화 테스트

2. **로그 모니터링**
   - ConfirmPaymentAsync 실행 시 detail 저장 로그 확인
   - 암호화/복호화 성공 여부 확인

3. **에러 처리 개선**
   - Detail 저장 실패 시 롤백 로직 확인
   - 암호화 실패 시 에러 핸들링

#### 선택 작업
1. **EF Core 마이그레이션 생성** (코드 우선 접근):
   ```bash
   dotnet ef migrations add AddTossPaymentsIntegrationFields
   ```

2. **Swagger 문서 업데이트**
   - TossPaymentResponseDto 새 필드 문서화

3. **프론트엔드 연동**
   - 새로운 필드 활용 방안 논의
   - 결제 상세 정보 표시

### 기술 문서 링크
- 토스페이먼츠 Payment 객체: https://docs.tosspayments.com/reference#payment-객체
- MySQL 8.0 Online DDL: https://dev.mysql.com/doc/refman/8.0/en/innate-online-ddl.html
- AES-GCM 암호화: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm

---

**마지막 업데이트**: 2026-01-29
**상태**:
- ✅ TossPaymentResponseDto 27개 필드 추가 완료
- ✅ MySQL DB 스키마 완전 동기화 완료 (Codex 검증)
- ✅ EF Core Entity 모델 업데이트 완료
- ✅ AES-256-GCM 암호화 서비스 구현 완료
- ✅ PaymentService 로직 개선 완료
- 🔄 다음: End-to-End 결제 테스트 및 암호화 검증

---

# 이전 작업 내역 (2026-01-26)

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

### ✅ 최근 작업 추가 (2026-01-30)
1. **Payment/Status 시드 정리**
   - `database_history/seed_payment_statuses.sql` 추가
   - `database_history/seed_payment_methods.sql` 추가
   - `database_history/seed_escrow_statuses.sql` 추가
   - `db_restore.sh`, `db_restore.bat`에서 시드 자동 적용 추가

2. **EF 매핑/트랜잭션 안정화**
   - `Repository/TicketContext.cs`: `payment_transactions` 매핑 추가
   - `Repository/Transaction/TransactionRepository.cs`: EF 트랜잭션 공유 처리 (Dapper Update)

3. **결제 상태/수단 자동 생성 제거**
   - `Services/Payment/PaymentService.cs`: 상태/수단 시드 기반 조회로 복원

### ✅ 결제 완료 이후 해야 할 일
1. **거래 상태 변경 확인**
   - `transactions.status_id`가 `paid`로 업데이트되었는지 확인

2. **에스크로 생성 확인**
   - `escrow` 레코드 생성 여부 확인
   - `escrow.status_id`가 `holding`인지 확인

3. **결제 상세 저장 확인**
   - `payment_transactions`에 PAYMENT 로그가 쌓였는지 확인
   - 결제 수단별 detail 테이블 저장 여부 확인
     - 카드: `payment_card_details`
     - 가상계좌: `payment_virtual_account_details`
     - 간편결제: `payment_easy_pay_details`
     - 현금영수증: `payment_cash_receipts`

4. **암호화 필드 검증**
   - `payment_virtual_account_details.secret`
   - `payment_virtual_account_details.refund_receive_account`
   - `payment_transactions.toss_response`

5. **Idempotency 확인**
   - 동일 `order_id`로 재호출 시 기존 결제 반환 여부 확인

6. **결제 후 구매 확정 플로우**
   - `ReleaseEscrowAsync` 호출 → escrow `released`, transaction `confirmed`

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
