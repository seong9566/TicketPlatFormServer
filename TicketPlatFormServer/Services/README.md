# Service 계층 가이드

## 예외 처리 패턴

Service 계층에서는 `AppException`을 사용하여 비즈니스 로직 예외를 처리합니다.

### 기본 패턴 (InnerException 없음)

단순한 비즈니스 로직 검증 실패 시 사용:

```csharp
public async Task<EventDetailRespDto> GetEventDetailWithTickets(int eventId)
{
    if (eventId <= 0)
    {
        throw new AppException(
            message: "유효하지 않은 이벤트 ID입니다.",
            statusCode: HttpStatusCode.BadRequest
        );
    }

    var eventDetail = await _eventRepo.GetEventDetailById(eventId);

    if (eventDetail == null)
    {
        throw new AppException(
            message: "이벤트를 찾을 수 없습니다.",
            statusCode: HttpStatusCode.NotFound
        );
    }

    return eventDetail;
}
```

### InnerException 패턴 (원본 예외 보존)

DB 예외, 외부 API 호출 실패 등 **원본 예외 정보를 보존**해야 할 때 사용:

#### 예제 1: DB 예외 처리

```csharp
public async Task<User> CreateUser(RegisterUserReqDto dto)
{
    try
    {
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        return await _repo.CreateUser(user);
    }
    catch (DbUpdateException ex)
    {
        // DB 제약 조건 위반 등의 예외를 비즈니스 예외로 변환
        // InnerException으로 원본 예외 정보 보존
        throw new AppException(
            message: "사용자 등록 중 오류가 발생했습니다.",
            statusCode: HttpStatusCode.InternalServerError,
            innerException: ex
        );
    }
}
```

#### 예제 2: 외부 API 호출 실패

```csharp
public async Task<PaymentResult> ProcessPayment(PaymentReqDto dto)
{
    try
    {
        // 외부 결제 API 호출
        var response = await _paymentApiClient.ChargeAsync(dto.Amount, dto.CardToken);
        return MapToPaymentResult(response);
    }
    catch (HttpRequestException ex)
    {
        // 외부 API 호출 실패를 사용자에게 친절한 메시지로 변환
        // InnerException으로 원본 네트워크 오류 보존
        throw new AppException(
            message: "결제 처리 중 오류가 발생했습니다. 잠시 후 다시 시도해주세요.",
            statusCode: HttpStatusCode.BadGateway,
            innerException: ex
        );
    }
    catch (TaskCanceledException ex)
    {
        // 타임아웃 예외 처리
        throw new AppException(
            message: "결제 요청 시간이 초과되었습니다.",
            statusCode: HttpStatusCode.RequestTimeout,
            innerException: ex
        );
    }
}
```

#### 예제 3: 복잡한 비즈니스 로직 오류

```csharp
public async Task<TransactionRespDto> CreateTicketTransaction(TransactionReqDto dto)
{
    try
    {
        // 티켓 재고 확인
        var ticket = await _ticketRepo.GetByIdAsync(dto.TicketId);
        if (ticket.RemainingQuantity < dto.Quantity)
        {
            throw new AppException(
                message: "티켓 재고가 부족합니다.",
                statusCode: HttpStatusCode.Conflict
            );
        }

        // 트랜잭션 생성 (DB 작업)
        var transaction = await _transactionRepo.CreateAsync(dto);

        // 재고 감소 (DB 작업)
        await _ticketRepo.DecreaseQuantityAsync(dto.TicketId, dto.Quantity);

        return MapToRespDto(transaction);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // 동시성 충돌 (다른 사용자가 동시에 구매)
        throw new AppException(
            message: "다른 사용자가 동시에 구매하여 처리할 수 없습니다. 다시 시도해주세요.",
            statusCode: HttpStatusCode.Conflict,
            innerException: ex
        );
    }
    catch (AppException)
    {
        // AppException은 그대로 전파
        throw;
    }
    catch (Exception ex)
    {
        // 예상치 못한 예외 - InnerException 보존
        throw new AppException(
            message: "거래 처리 중 오류가 발생했습니다.",
            statusCode: HttpStatusCode.InternalServerError,
            innerException: ex
        );
    }
}
```

## InnerException 사용 시기

### ✅ InnerException을 사용해야 하는 경우

1. **DB 예외 변환**: `DbUpdateException`, `DbUpdateConcurrencyException` 등
2. **외부 API 호출 실패**: `HttpRequestException`, `TaskCanceledException` 등
3. **파일 I/O 오류**: `IOException`, `FileNotFoundException` 등
4. **직렬화 오류**: `JsonException`, `XmlException` 등
5. **예상치 못한 예외**: 일반 `Exception` catch 후 재던지기

**이유**: 로그에서 원본 예외의 StackTrace와 상세 정보를 확인할 수 있어 디버깅에 유용

### ❌ InnerException이 필요 없는 경우

1. **단순 입력 검증**: `if (id <= 0)`, `if (string.IsNullOrEmpty(email))`
2. **비즈니스 규칙 위반**: 재고 부족, 권한 없음, 중복 데이터 등
3. **null 체크**: `if (user == null)`, `if (result == null)`

**이유**: 이미 충분히 명확한 비즈니스 로직 오류이므로 추가 정보 불필요

## GlobalExceptionMiddleware 로깅 동작

### InnerException이 있는 경우

```
[AppException] 결제 처리 중 오류가 발생했습니다. | Path: /api/payment
InnerException: HttpRequestException - Connection refused
```

### InnerException이 없는 경우

```
[AppException] 유효하지 않은 이벤트 ID입니다. | Path: /api/events/0
```

## 참고 자료

- [AppException.cs](../Common/Exception/AppException.cs): 예외 클래스 정의
- [GlobalExceptionMiddleware.cs](../Common/Exception/GlobalExceptionMiddleware.cs): 예외 처리 미들웨어
