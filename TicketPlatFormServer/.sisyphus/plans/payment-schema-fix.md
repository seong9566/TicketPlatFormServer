# Payment 스키마 불일치 수정

## TL;DR

> **핵심 요약**: EF Core의 Payment 엔티티 매핑 설정에서 누락된 10개 컬럼 매핑을 추가하여 "Unknown column" MySQL 오류 해결
> 
> **제공물**: 
> - TicketContext.cs의 Payment 엔티티 설정 수정 (10개 컬럼 매핑 추가)
> - 4개 결제 엔드포인트 수동 검증 완료
> - 단일 커밋
> 
> **예상 작업량**: Quick (30분 이내)
> **병렬 실행**: NO - 순차 실행
> **핵심 경로**: 코드 수정 → 빌드 → API 실행 → 검증

---

## Context

### 원본 요청
사용자가 결제 확인 API 호출 시 다음 오류 발생:
```
MySqlConnector.MySqlException: Unknown column 'p.ApiVersion' in 'field list'
```

오류 위치:
- `PaymentRepository.GetPaymentByOrderIdAsync()` (line 38)
- 호출 경로: `POST /api/payment/confirm` → `PaymentService.ConfirmPaymentAsync()` (line 86)

### 조사 요약
**핵심 발견사항**:
1. **데이터베이스 스키마는 정상** - `payments` 테이블에 모든 컬럼 존재 확인
   - merchant_id ✓
   - api_version ✓
   - country ✓
   - culture_expense ✓
   - discount_info ✓
   - is_partial_cancelable ✓
   - last_transaction_key ✓
   - metadata ✓
   - payment_type ✓
   - use_escrow ✓

2. **Payment 엔티티 (DBModel/Payment.cs)도 정상** - 모든 속성 정의됨

3. **문제 위치**: `TicketContext.OnModelCreating()` (lines 1187-1249)
   - 현재 9개 컬럼만 매핑 (id, amount, method_id, order_id, paid_at, payment_key, pg_provider, status_id, transaction_id)
   - **10개 컬럼 매핑 누락**
   - EF Core가 누락된 속성을 컨벤션으로 처리 시 잘못된 컬럼명 생성
     - `ApiVersion` 속성 → SQL에서 `p.ApiVersion` 생성 (올바른 형식: `p.api_version`)

### 연구 결과
- EF Core는 명시적 매핑이 없으면 속성명을 그대로 사용 (PascalCase)
- MySQL 테이블은 snake_case 컬럼명 사용
- 불일치로 인해 SELECT 쿼리 실패

---

## 작업 목표

### 핵심 목표
EF Core의 Payment 엔티티 설정에 누락된 10개 컬럼의 명시적 매핑을 추가하여 데이터베이스 스키마와 일치시킴

### 구체적인 제공물
- `Repository/TicketContext.cs` 파일의 `OnModelCreating` 메서드 내 Payment 엔티티 설정 수정

### 완료 조건
- [ ] TicketContext.cs에 10개 컬럼 매핑 추가 완료
- [ ] `dotnet build` 성공 (컴파일 오류 없음)
- [ ] `dotnet run` 성공 (API 서버 정상 기동)
- [ ] POST /api/payment/initiate - 200 OK 응답
- [ ] POST /api/payment/confirm - 정상 처리 (Unknown column 오류 없음)
- [ ] POST /api/payment/cancel - 정상 처리
- [ ] GET /api/payment/{orderId} - 정상 조회
- [ ] 단일 커밋 완료

### 반드시 포함해야 할 것
- 10개 컬럼 모두에 대한 명시적 `.HasColumnName()` 매핑
- 기존 매핑 스타일과 일관성 유지 (들여쓰기, 주석 형식)
- Comment 속성 유지 (한글 설명)

### 절대 포함하지 말아야 할 것 (가드레일)
- ❌ 데이터베이스 스키마 변경 (마이그레이션 생성 금지)
- ❌ Payment 엔티티 클래스 수정 (DBModel/Payment.cs 변경 금지)
- ❌ 다른 엔티티 설정 변경
- ❌ PaymentRepository, PaymentService 로직 수정
- ❌ EF Core 컨벤션 변경 (ModelBuilder 전역 설정 금지)

---

## 검증 전략

> 사용자 결정: 수동 검증만 수행 (테스트 프로젝트 없음)

### 검증 결정사항
- **인프라 존재 여부**: NO (테스트 프로젝트 없음)
- **사용자가 원하는 테스트**: 수동 검증만
- **QA 접근 방식**: Swagger UI 또는 curl을 통한 수동 엔드포인트 테스트

### 자동화된 검증 (에이전트 실행 가능)

**빌드 검증**:
```bash
# 에이전트가 실행할 명령:
cd /Users/stecdev/Desktop/workspace/dotnet_server/TicketPlatFormServer/TicketPlatFormServer
dotnet build
# Assert: 빌드 성공 (exit code 0)
# Assert: "Build succeeded" 문구 출력 확인
```

**API 기동 검증**:
```bash
# 에이전트가 실행할 명령:
dotnet run --project TicketPlatFormServer &
sleep 10  # API 초기화 대기
curl -s http://localhost:5224/swagger/index.html | grep -q "Swagger UI"
# Assert: Swagger UI 정상 로드
# Assert: HTTP 200 응답
```

**결제 초기화 엔드포인트 검증**:
```bash
# 에이전트가 실행할 명령:
curl -s -X POST http://localhost:5224/api/payment/initiate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -d '{
    "transactionId": 1,
    "amount": 50000,
    "orderName": "테스트 티켓 구매",
    "customerName": "홍길동",
    "customerEmail": "test@example.com"
  }' \
  | jq '.success'
# Assert: JSON 응답에서 .success == true
# Assert: .data.orderId 필드 존재
# Assert: HTTP 200 응답
```

**결제 승인 엔드포인트 검증** (핵심 오류 지점):
```bash
# 에이전트가 실행할 명령:
curl -s -X POST http://localhost:5224/api/payment/confirm \
  -H "Content-Type: application/json" \
  -d '{
    "paymentKey": "test_payment_key_123",
    "orderId": "TXN_1_abc123",
    "amount": 50000
  }' \
  2>&1 | grep -v "Unknown column"
# Assert: "Unknown column" 오류 문자열 없음
# Assert: 응답이 JSON 형식 (Toss API 통신 실패는 예상됨, 스키마 오류만 체크)
```

**결제 취소 엔드포인트 검증**:
```bash
# 에이전트가 실행할 명령:
curl -s -X POST http://localhost:5224/api/payment/cancel \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -d '{
    "transactionId": 1,
    "cancelReason": "단순 변심",
    "cancelAmount": 50000
  }' \
  2>&1
# Assert: "Unknown column" 오류 문자열 없음
# Assert: HTTP 응답 수신 (400/404는 OK, 500 Internal Server Error는 NG)
```

**결제 조회 엔드포인트 검증**:
```bash
# 에이전트가 실행할 명령:
curl -s -X GET http://localhost:5224/api/payment/TXN_1_abc123 \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  2>&1
# Assert: "Unknown column" 오류 문자열 없음
# Assert: HTTP 응답 수신 (404 Not Found는 OK, 500 Internal Server Error는 NG)
```

**프로세스 정리**:
```bash
# 검증 완료 후 API 서버 종료:
pkill -f "dotnet run"
```

**증거 자료 수집**:
- [ ] 각 curl 명령의 터미널 출력 (실제 응답)
- [ ] "Unknown column" 오류가 더 이상 발생하지 않음을 확인
- [ ] HTTP 상태 코드 확인 (500 에러가 스키마 문제에서 비즈니스 로직 문제로 변경됨)

---

## 실행 전략

### 병렬 실행 파동

> 모든 작업이 순차적으로 진행되어야 함 (단일 파일 수정)

```
Wave 1 (즉시 시작):
└── Task 1: TicketContext.cs 수정 (10개 컬럼 매핑 추가)

Wave 2 (Wave 1 완료 후):
└── Task 2: 빌드 및 실행 검증

Wave 3 (Wave 2 완료 후):
├── Task 3: 결제 초기화 엔드포인트 검증
├── Task 4: 결제 승인 엔드포인트 검증 (핵심)
├── Task 5: 결제 취소 엔드포인트 검증
└── Task 6: 결제 조회 엔드포인트 검증

Wave 4 (Wave 3 완료 후):
└── Task 7: Git 커밋

핵심 경로: Task 1 → Task 2 → Task 4 → Task 7
병렬 가속: 없음 (순차 실행)
```

### 의존성 매트릭스

| Task | 의존 대상 | 차단 대상 | 병렬 실행 가능 |
|------|----------|----------|--------------|
| 1 | 없음 | 2 | 없음 |
| 2 | 1 | 3,4,5,6 | 없음 |
| 3 | 2 | 7 | 4,5,6 |
| 4 | 2 | 7 | 3,5,6 |
| 5 | 2 | 7 | 3,4,6 |
| 6 | 2 | 7 | 3,4,5 |
| 7 | 1,2,3,4,5,6 | 없음 | 없음 (최종) |

### 에이전트 배치 요약

| Wave | Tasks | 권장 에이전트 |
|------|-------|--------------|
| 1 | 1 | quick (단순 코드 수정) |
| 2 | 2 | quick (빌드 검증) |
| 3 | 3,4,5,6 | quick (curl 명령 실행) - 병렬 가능 |
| 4 | 7 | git-master (커밋 작성) |

---

## TODOs

### 1. TicketContext.cs 수정 - 10개 컬럼 매핑 추가

**해야 할 일**:
- `Repository/TicketContext.cs` 파일 열기
- `OnModelCreating` 메서드 내부의 `modelBuilder.Entity<PaymentEntity>` 구성 블록 찾기 (lines 1187-1249)
- 기존 9개 컬럼 매핑 뒤에 10개 추가 컬럼 매핑 삽입
- 기존 코드 스타일 유지 (4칸 들여쓰기, `.HasComment()` 한글 주석)

**절대 하지 말아야 할 것**:
- ❌ Payment 엔티티 클래스 (DBModel/Payment.cs) 수정 금지
- ❌ 데이터베이스 마이그레이션 생성 금지
- ❌ 다른 엔티티 설정 변경 금지
- ❌ 기존 9개 컬럼 매핑 삭제/수정 금지

**권장 에이전트 프로필**:
- **카테고리**: `quick`
  - 이유: 단순 코드 추가 작업, 복잡한 로직 없음
- **스킬**: 없음
  - 이유: 일반적인 C# 코드 편집, 특수 도메인 지식 불필요
- **평가했지만 제외한 스킬**:
  - `git-master`: 커밋은 Task 7에서 별도 수행

**병렬화**:
- **병렬 실행 가능 여부**: NO
- **병렬 그룹**: Sequential (Wave 1)
- **차단 대상**: Task 2 (빌드는 코드 수정 후 가능)
- **차단 당함**: 없음 (즉시 시작 가능)

**참조 자료** (실행자가 참고할 파일):

**패턴 참조** (따라야 할 기존 코드):
- `Repository/TicketContext.cs:1187-1249` - Payment 엔티티 기존 매핑 패턴
  - 이유: 동일한 엔티티 내에서 일관된 스타일 유지 필요
  - 패턴: `entity.Property(e => e.PropertyName).HasComment("한글설명").HasColumnName("snake_case");`
  
**API/타입 참조** (구현 대상 계약):
- `DBModel/Payment.cs:11-103` - 매핑할 10개 속성 정의
  - 이유: 각 속성의 타입과 주석 확인
  - 추출할 정보: 속성명 (PascalCase), XML 주석 (한글 설명)

**문서 참조**:
- `database_history/TicketPlatFormDB_dump.sql` - payments 테이블 스키마 정의
  - 이유: 정확한 컬럼명 (snake_case)과 타입 확인
  - 확인 항목: merchant_id, api_version, country, culture_expense, discount_info, is_partial_cancelable, last_transaction_key, metadata, payment_type, use_escrow

**각 참조가 중요한 이유**:
- TicketContext.cs 기존 패턴: 코드 일관성 유지, 리뷰어가 이질감 없이 이해 가능
- Payment.cs 엔티티: 속성명 오타 방지, 주석 내용 복사
- SQL dump: 컬럼명 정확성 보장, snake_case 변환 규칙 확인

**인수 조건** (자동화된 검증):

```csharp
// 추가할 정확한 코드 (lines 1234-1249 사이에 삽입):

entity.Property(e => e.MerchantId)
    .HasMaxLength(50)
    .HasComment("토스 가맹점 ID (mId)")
    .HasColumnName("merchant_id");
entity.Property(e => e.ApiVersion)
    .HasMaxLength(20)
    .HasComment("토스 API 버전")
    .HasColumnName("api_version");
entity.Property(e => e.Country)
    .HasMaxLength(2)
    .IsFixedLength()
    .HasDefaultValue("KR")
    .HasComment("국가 코드 (ISO-3166-1 alpha-2)")
    .HasColumnName("country");
entity.Property(e => e.UseEscrow)
    .HasDefaultValue(false)
    .HasComment("에스크로 사용 여부")
    .HasColumnName("use_escrow");
entity.Property(e => e.IsPartialCancelable)
    .HasDefaultValue(false)
    .HasComment("부분 취소 가능 여부")
    .HasColumnName("is_partial_cancelable");
entity.Property(e => e.PaymentType)
    .HasMaxLength(20)
    .HasComment("결제 타입 (NORMAL, BILLING)")
    .HasColumnName("payment_type");
entity.Property(e => e.LastTransactionKey)
    .HasMaxLength(255)
    .HasComment("최종 거래 키 (deprecated: use payment_transactions)")
    .HasColumnName("last_transaction_key");
entity.Property(e => e.CultureExpense)
    .HasDefaultValue(false)
    .HasComment("문화비 소득공제 여부")
    .HasColumnName("culture_expense");
entity.Property(e => e.Metadata)
    .HasColumnType("json")
    .HasComment("커스텀 메타데이터")
    .HasColumnName("metadata");
entity.Property(e => e.DiscountInfo)
    .HasColumnType("json")
    .HasComment("할인 정보")
    .HasColumnName("discount_info");
```

**검증 기준**:
- [ ] TicketContext.cs 파일에 정확히 10개 속성 매핑 추가됨
- [ ] 각 `.HasColumnName()` 값이 데이터베이스 컬럼명과 정확히 일치 (snake_case)
- [ ] 모든 `.HasComment()` 값이 한글로 작성됨
- [ ] 들여쓰기가 기존 코드와 일치 (4칸 스페이스)
- [ ] 세미콜론(`;`) 위치 정확함

**커밋**: NO (Task 7에서 일괄 커밋)

---

### 2. 빌드 및 실행 검증

**해야 할 일**:
- 프로젝트 빌드 실행
- API 서버 기동 확인
- Swagger UI 접근 가능 확인

**절대 하지 말아야 할 것**:
- ❌ 빌드 오류 발생 시 임의로 코드 수정 (Task 1로 돌아가서 재검토)
- ❌ API 서버를 백그라운드로 실행한 채 방치 (검증 완료 후 종료)

**권장 에이전트 프로필**:
- **카테고리**: `quick`
  - 이유: 단순 명령 실행 및 결과 확인
- **스킬**: 없음

**병렬화**:
- **병렬 실행 가능 여부**: NO
- **병렬 그룹**: Sequential (Wave 2)
- **차단 대상**: Tasks 3,4,5,6 (API 실행 후 엔드포인트 테스트 가능)
- **차단 당함**: Task 1 (코드 수정 완료 후 빌드 가능)

**참조 자료**:

**명령 실행 참조**:
- `AGENTS.md:### Building and Running` - 빌드 및 실행 명령어
  - 이유: 프로젝트 표준 명령어 사용
  - 명령어: `dotnet build`, `dotnet run --project TicketPlatFormServer`

**인수 조건**:

**빌드 검증**:
```bash
# 에이전트 실행 명령:
cd /Users/stecdev/Desktop/workspace/dotnet_server/TicketPlatFormServer/TicketPlatFormServer
dotnet build
# Assert: Exit code 0
# Assert: 출력에 "Build succeeded" 포함
# Assert: 출력에 "0 Error(s)" 포함
```

**API 실행 검증**:
```bash
# 에이전트 실행 명령:
dotnet run --project TicketPlatFormServer &
sleep 10  # 초기화 대기
curl -s -o /dev/null -w "%{http_code}" http://localhost:5224/swagger/index.html
# Assert: HTTP 응답 코드 200
```

**Swagger UI 접근 검증**:
```bash
# 에이전트 실행 명령:
curl -s http://localhost:5224/swagger/index.html | head -20
# Assert: HTML 응답에 "<title>Swagger UI</title>" 포함
# Assert: "TicketPlatFormServer" 문자열 포함
```

**증거 자료**:
- [ ] dotnet build 출력 전문
- [ ] API 서버 콘솔 출력 (첫 20줄)
- [ ] Swagger UI HTTP 응답 헤더

**커밋**: NO

---

### 3. 결제 초기화 엔드포인트 검증

**해야 할 일**:
- POST /api/payment/initiate 엔드포인트 호출
- 정상 응답 확인 (orderId 생성 확인)

**절대 하지 말아야 할 것**:
- ❌ 실제 거래 데이터 생성 시도 (테스트용 transactionId 사용)
- ❌ 인증 토큰 없이 호출 시 401 오류를 실패로 간주

**권장 에이전트 프로필**:
- **카테고리**: `quick`
- **스킬**: 없음

**병렬화**:
- **병렬 실행 가능 여부**: YES
- **병렬 그룹**: Wave 3 (Tasks 4, 5, 6과 함께)
- **차단 대상**: Task 7
- **차단 당함**: Task 2

**참조 자료**:

**API 스펙 참조**:
- `api_spec/` 디렉토리 - 결제 API 명세
  - 이유: 요청/응답 형식 확인

**구현 참조**:
- `Services/Payment/PaymentService.cs:29-69` - InitiatePaymentAsync 로직
  - 이유: 예상 응답 구조 이해

**인수 조건**:

```bash
# 에이전트 실행 명령:
curl -s -X POST http://localhost:5224/api/payment/initiate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -d '{
    "transactionId": 999,
    "amount": 10000,
    "orderName": "테스트 결제",
    "customerName": "테스터",
    "customerEmail": "test@test.com"
  }' | jq '.'
# Assert: JSON 응답 수신
# Assert: .data.orderId 필드 존재
# Assert: .data.orderId가 "TXN_999_" 로 시작
# Assert: HTTP 응답 코드 200 또는 400 (401은 인증 문제로 예상 가능)
```

**증거 자료**:
- [ ] curl 명령 전체 출력
- [ ] 응답 JSON에서 orderId 필드 추출 결과

**커밋**: NO

---

### 4. 결제 승인 엔드포인트 검증 (핵심)

**해야 할 일**:
- POST /api/payment/confirm 엔드포인트 호출
- **"Unknown column" 오류가 발생하지 않음을 확인** (핵심 검증 항목)
- Toss API 호출 실패는 예상되지만, 스키마 오류는 발생하지 않아야 함

**절대 하지 말아야 할 것**:
- ❌ Toss API 실패를 수정하려고 시도 (스코프 외부)
- ❌ 실제 결제 승인 시도 (테스트 paymentKey 사용)

**권장 에이전트 프로필**:
- **카테고리**: `quick`
- **스킬**: 없음

**병렬화**:
- **병렬 실행 가능 여부**: YES
- **병렬 그룹**: Wave 3 (Tasks 3, 5, 6과 함께)
- **차단 대상**: Task 7
- **차단 당함**: Task 2

**참조 자료**:

**오류 발생 위치**:
- `Repository/Payment/PaymentRepository.cs:36-44` - GetPaymentByOrderIdAsync
  - 이유: 원본 오류 발생 지점, 수정 후 이 메서드가 정상 동작해야 함

**호출 경로**:
- `Services/Payment/PaymentService.cs:74-313` - ConfirmPaymentAsync
  - 이유: line 86에서 GetPaymentByOrderIdAsync 호출

**인수 조건**:

```bash
# 에이전트 실행 명령:
curl -s -X POST http://localhost:5224/api/payment/confirm \
  -H "Content-Type: application/json" \
  -d '{
    "paymentKey": "test_key_12345",
    "orderId": "TXN_999_abcdef",
    "amount": 10000
  }' 2>&1 | tee /tmp/payment_confirm_output.txt

# Assert: 출력에 "Unknown column" 문자열 없음
grep -q "Unknown column" /tmp/payment_confirm_output.txt && exit 1 || exit 0

# Assert: MySQL 오류 코드 없음
grep -q "MySqlConnector.MySqlException" /tmp/payment_confirm_output.txt && exit 1 || exit 0

# Assert: 응답 수신 (JSON 또는 에러 메시지, 단 스키마 오류 제외)
test -s /tmp/payment_confirm_output.txt
```

**예상 동작**:
- ✅ GetPaymentByOrderIdAsync가 null 반환 (데이터 없음) - 정상
- ✅ Toss API 호출 실패 (유효하지 않은 paymentKey) - 예상됨
- ❌ "Unknown column 'p.ApiVersion'" 오류 - 발생하면 안됨

**증거 자료**:
- [ ] curl 전체 출력 (에러 스택 트레이스 포함)
- [ ] grep 결과 ("Unknown column" 없음 확인)

**커밋**: NO

---

### 5. 결제 취소 엔드포인트 검증

**해야 할 일**:
- POST /api/payment/cancel 엔드포인트 호출
- 스키마 오류 없이 로직 실행 확인

**절대 하지 말아야 할 것**:
- ❌ 실제 결제 취소 시도
- ❌ 404/400 응답을 실패로 간주

**권장 에이전트 프로필**:
- **카테고리**: `quick`
- **스킬**: 없음

**병렬화**:
- **병렬 실행 가능 여부**: YES
- **병렬 그룹**: Wave 3 (Tasks 3, 4, 6과 함께)
- **차단 대상**: Task 7
- **차단 당함**: Task 2

**참조 자료**:

**구현 참조**:
- `Services/Payment/PaymentService.cs:384-491` - CancelPaymentAsync
  - 이유: line 390에서 GetPaymentByTransactionIdAsync 호출 (Payment 엔티티 조회)

**인수 조건**:

```bash
# 에이전트 실행 명령:
curl -s -X POST http://localhost:5224/api/payment/cancel \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -d '{
    "transactionId": 999,
    "cancelReason": "테스트 취소",
    "cancelAmount": 10000
  }' 2>&1 | tee /tmp/payment_cancel_output.txt

# Assert: "Unknown column" 문자열 없음
grep -q "Unknown column" /tmp/payment_cancel_output.txt && exit 1 || exit 0

# Assert: HTTP 500 에러가 스키마 문제로 발생하지 않음
# (404/403은 비즈니스 로직 오류로 허용)
```

**증거 자료**:
- [ ] curl 출력
- [ ] HTTP 상태 코드 확인

**커밋**: NO

---

### 6. 결제 조회 엔드포인트 검증

**해야 할 일**:
- GET /api/payment/{orderId} 엔드포인트 호출
- Payment 엔티티 조회 시 스키마 오류 없음 확인

**절대 하지 말아야 할 것**:
- ❌ 존재하지 않는 orderId로 404 응답을 실패로 간주

**권장 에이전트 프로필**:
- **카테고리**: `quick`
- **스킬**: 없음

**병렬화**:
- **병렬 실행 가능 여부**: YES
- **병렬 그룹**: Wave 3 (Tasks 3, 4, 5와 함께)
- **차단 대상**: Task 7
- **차단 당함**: Task 2

**참조 자료**:

**구현 참조**:
- `Services/Payment/PaymentService.cs:536-548` - GetPaymentByOrderIdAsync
  - 이유: line 540에서 GetPaymentByOrderIdAsync (Repository) 호출

**인수 조건**:

```bash
# 에이전트 실행 명령:
curl -s -X GET http://localhost:5224/api/payment/TXN_999_test \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  2>&1 | tee /tmp/payment_get_output.txt

# Assert: "Unknown column" 문자열 없음
grep -q "Unknown column" /tmp/payment_get_output.txt && exit 1 || exit 0
```

**증거 자료**:
- [ ] curl 출력
- [ ] 스키마 오류 없음 확인

**커밋**: NO

---

### 7. Git 커밋 생성

**해야 할 일**:
- 변경 사항 스테이징 (TicketContext.cs만)
- Conventional Commits 형식으로 커밋 메시지 작성
- 커밋 실행

**절대 하지 말아야 할 것**:
- ❌ 다른 파일 포함 (TicketContext.cs만 커밋)
- ❌ 임시 파일/로그 파일 커밋
- ❌ 커밋 메시지에 AI-slop 패턴 사용 (예: "이 커밋은...", "모든 것을...")

**권장 에이전트 프로필**:
- **카테고리**: `quick`
- **스킬**: `git-master`
  - 이유: Git 커밋 작성 및 히스토리 관리 전문
  - 도메인 중복: 커밋 메시지 품질, Conventional Commits 준수

**병렬화**:
- **병렬 실행 가능 여부**: NO
- **병렬 그룹**: Sequential (Wave 4, 최종)
- **차단 대상**: 없음
- **차단 당함**: Tasks 1,2,3,4,5,6 (모든 검증 완료 후 커밋)

**참조 자료**:

**커밋 가이드라인**:
- `AGENTS.md:## Commit Guidelines` - Conventional Commits 형식
  - 이유: 프로젝트 커밋 메시지 표준 준수

**Git 히스토리**:
```bash
# 최근 커밋 메시지 스타일 참고:
git log --oneline -10
```

**인수 조건**:

**커밋 메시지 (정확히 이 형식 사용)**:
```
fix: add missing Payment entity column mappings in TicketContext

EF Core의 Payment 엔티티 설정에서 누락된 10개 컬럼의 명시적 매핑을 추가하여
"Unknown column 'p.ApiVersion' in 'field list'" MySQL 오류 해결

추가된 컬럼 매핑:
- merchant_id (토스 가맹점 ID)
- api_version (토스 API 버전)
- country (국가 코드)
- use_escrow (에스크로 사용 여부)
- is_partial_cancelable (부분 취소 가능 여부)
- payment_type (결제 타입)
- last_transaction_key (최종 거래 키)
- culture_expense (문화비 소득공제 여부)
- metadata (커스텀 메타데이터)
- discount_info (할인 정보)

Resolves: PaymentRepository.GetPaymentByOrderIdAsync() 실행 시 컬럼 누락 오류
```

**Git 명령어**:
```bash
# 에이전트 실행 명령:
cd /Users/stecdev/Desktop/workspace/dotnet_server/TicketPlatFormServer/TicketPlatFormServer

# 변경 파일 확인
git status

# TicketContext.cs만 스테이징
git add Repository/TicketContext.cs

# 커밋 (메시지는 위 형식 사용)
git commit -F- <<'EOF'
fix: add missing Payment entity column mappings in TicketContext

EF Core의 Payment 엔티티 설정에서 누락된 10개 컬럼의 명시적 매핑을 추가하여
"Unknown column 'p.ApiVersion' in 'field list'" MySQL 오류 해결

추가된 컬럼 매핑:
- merchant_id (토스 가맹점 ID)
- api_version (토스 API 버전)
- country (국가 코드)
- use_escrow (에스크로 사용 여부)
- is_partial_cancelable (부분 취소 가능 여부)
- payment_type (결제 타입)
- last_transaction_key (최종 거래 키)
- culture_expense (문화비 소득공제 여부)
- metadata (커스텀 메타데이터)
- discount_info (할인 정보)

Resolves: PaymentRepository.GetPaymentByOrderIdAsync() 실행 시 컬럼 누락 오류
EOF

# 커밋 확인
git log -1 --stat
```

**검증 기준**:
- [ ] `git status`에서 TicketContext.cs만 staged
- [ ] 커밋 메시지가 Conventional Commits 형식 준수
- [ ] 커밋 메시지에 "fix:" prefix 사용
- [ ] 커밋 완료 후 `git log -1`에서 변경 내역 확인

**커밋**: YES (이 Task가 커밋 생성 담당)
- 메시지: 위 형식 사용
- 파일: `Repository/TicketContext.cs`
- 사전 검증: `dotnet build` 성공

---

## 커밋 전략

| Task 완료 후 | 메시지 | 파일 | 검증 |
|------------|--------|------|------|
| 7 | `fix: add missing Payment entity column mappings in TicketContext` | Repository/TicketContext.cs | `dotnet build` 성공, 모든 엔드포인트 검증 완료 |

---

## 성공 기준

### 검증 명령어
```bash
# 최종 확인 1: 빌드 성공
dotnet build

# 최종 확인 2: API 실행
dotnet run --project TicketPlatFormServer &
sleep 10

# 최종 확인 3: 결제 승인 오류 없음
curl -s -X POST http://localhost:5224/api/payment/confirm \
  -H "Content-Type: application/json" \
  -d '{"paymentKey":"test","orderId":"TXN_1_test","amount":1000}' \
  2>&1 | grep -v "Unknown column"

# 최종 확인 4: API 서버 종료
pkill -f "dotnet run"
```

### 최종 체크리스트
- [ ] TicketContext.cs에 10개 컬럼 매핑 모두 추가됨
- [ ] "Unknown column" 오류 더 이상 발생하지 않음
- [ ] POST /api/payment/confirm이 스키마 오류 없이 실행됨
- [ ] 다른 결제 엔드포인트도 정상 동작 (비즈니스 로직 오류는 허용)
- [ ] Git 커밋 1개 생성됨
- [ ] 커밋 메시지가 Conventional Commits 형식 준수
