using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Jose;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DTO.Payment;

namespace TicketPlatFormServer.Services.Payment;

/// <summary>
/// 토스페이먼츠 API 서비스 구현
/// </summary>
public class TossPaymentsService : ITossPaymentsService
{
    private readonly HttpClient _httpClient;
    private readonly TossPaymentsSettings _settings;
    private readonly ILogger<TossPaymentsService> _logger;

    public TossPaymentsService(
        IHttpClientFactory httpClientFactory,
        TossPaymentsSettings settings,
        ILogger<TossPaymentsService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TossPayments");
        _settings = settings;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            _logger.LogError("[TossPaymentsService] SecretKey is null or empty! Settings: ClientKey={ClientKeyEmpty}, ApiBaseUrl={ApiBaseUrl}", 
                string.IsNullOrWhiteSpace(_settings.ClientKey), _settings.ApiBaseUrl);
            throw new InvalidOperationException("TossPayments SecretKey is not configured.");
        }

        var rawToken = $"{_settings.SecretKey}:";
        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawToken));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
        
        _logger.LogInformation("[TossPaymentsService] Initialized successfully - BaseUrl={BaseUrl}, TestMode={TestMode}, SecretKeyLength={SecretKeyLength}", 
            _settings.ApiBaseUrl, _settings.IsTestMode, _settings.SecretKey?.Length ?? 0);
    }

    /// <summary>
    /// 결제 승인
    /// POST https://api.tosspayments.com/v1/payments/confirm
    /// </summary>
    public async Task<TossPaymentResponseDto> ConfirmPaymentAsync(string paymentKey, string orderId, int amount)
    {
        _logger.LogInformation("[TossPaymentsService.ConfirmPaymentAsync] Start: PaymentKey={PaymentKey}, OrderId={OrderId}, Amount={Amount}",
            paymentKey, orderId, amount);

        var requestBody = new TossPaymentConfirmRequest
        {
            PaymentKey = paymentKey,
            OrderId = orderId,
            Amount = amount
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync("/v1/payments/confirm", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[TossPaymentsService.ConfirmPaymentAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode, responseContent);

                var errorResponse = JsonSerializer.Deserialize<TossPaymentResponseDto>(responseContent);
                throw new AppException(
                    errorResponse?.Failure?.Message ?? "결제 승인 중 오류가 발생했습니다.",
                    HttpStatusCode.BadRequest
                );
            }

            var result = JsonSerializer.Deserialize<TossPaymentResponseDto>(responseContent);
            if (result == null)
            {
                throw new AppException("결제 승인 응답을 처리할 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            // Success: PaymentKey=tgen_20260128100231dfOG2, Status=DONE
            _logger.LogInformation("[TossPaymentsService.ConfirmPaymentAsync] Success: PaymentKey={PaymentKey}, Status={Status}",
                paymentKey, result.Status);
        
            

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[TossPaymentsService.ConfirmPaymentAsync] HTTP Request failed");
            throw new AppException("결제 승인 요청 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// 결제 조회 (PaymentKey로)
    /// GET https://api.tosspayments.com/v1/payments/{paymentKey}
    /// </summary>
    public async Task<TossPaymentResponseDto> GetPaymentAsync(string paymentKey)
    {
        _logger.LogInformation("[TossPaymentsService.GetPaymentAsync] Start: PaymentKey={PaymentKey}", paymentKey);

        try
        {
            var response = await _httpClient.GetAsync($"/v1/payments/{paymentKey}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[TossPaymentsService.GetPaymentAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode, responseContent);
                throw new AppException("결제 정보를 조회할 수 없습니다.", HttpStatusCode.NotFound);
            }

            var result = JsonSerializer.Deserialize<TossPaymentResponseDto>(responseContent);
            if (result == null)
            {
                throw new AppException("결제 정보 응답을 처리할 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[TossPaymentsService.GetPaymentAsync] HTTP Request failed");
            throw new AppException("결제 정보 조회 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// 결제 조회 (OrderId로)
    /// GET https://api.tosspayments.com/v1/payments/orders/{orderId}
    /// </summary>
    public async Task<TossPaymentResponseDto> GetPaymentByOrderIdAsync(string orderId)
    {
        _logger.LogInformation("[TossPaymentsService.GetPaymentByOrderIdAsync] Start: OrderId={OrderId}", orderId);

        try
        {
            var response = await _httpClient.GetAsync($"/v1/payments/orders/{orderId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[TossPaymentsService.GetPaymentByOrderIdAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode, responseContent);
                throw new AppException("결제 정보를 조회할 수 없습니다.", HttpStatusCode.NotFound);
            }

            var result = JsonSerializer.Deserialize<TossPaymentResponseDto>(responseContent);
            if (result == null)
            {
                throw new AppException("결제 정보 응답을 처리할 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[TossPaymentsService.GetPaymentByOrderIdAsync] HTTP Request failed");
            throw new AppException("결제 정보 조회 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// 결제 취소 (환불)
    /// POST https://api.tosspayments.com/v1/payments/{paymentKey}/cancel
    /// </summary>
    public async Task<TossPaymentResponseDto> CancelPaymentAsync(string paymentKey, string cancelReason, int? cancelAmount = null)
    {
        _logger.LogInformation("[TossPaymentsService.CancelPaymentAsync] Start: PaymentKey={PaymentKey}, CancelAmount={CancelAmount}, Reason={Reason}",
            paymentKey, cancelAmount, cancelReason);

        var requestBody = new TossPaymentCancelRequest
        {
            CancelReason = cancelReason,
            CancelAmount = cancelAmount
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync($"/v1/payments/{paymentKey}/cancel", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[TossPaymentsService.CancelPaymentAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode, responseContent);

                var errorResponse = JsonSerializer.Deserialize<TossPaymentResponseDto>(responseContent);
                throw new AppException(
                    errorResponse?.Failure?.Message ?? "결제 취소 중 오류가 발생했습니다.",
                    HttpStatusCode.BadRequest
                );
            }

            var result = JsonSerializer.Deserialize<TossPaymentResponseDto>(responseContent);
            if (result == null)
            {
                throw new AppException("결제 취소 응답을 처리할 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            _logger.LogInformation("[TossPaymentsService.CancelPaymentAsync] Success: PaymentKey={PaymentKey}, Status={Status}",
                paymentKey, result.Status);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[TossPaymentsService.CancelPaymentAsync] HTTP Request failed");
            throw new AppException("결제 취소 요청 중 오류가 발생했습니다.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// 지급대행 잔액 조회 (GET /v2/balances)
    /// ENCRYPTION 보안 불필요 (GET 요청)
    /// </summary>
    public async Task<PayoutBalanceDto> GetPayoutBalanceAsync()
    {
        var endpoint = $"{_settings.SettlementApiBaseUrl.TrimEnd('/')}/v2/balances";
        var response = await _httpClient.GetAsync(endpoint);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[TossPaymentsService.GetPayoutBalanceAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                (int)response.StatusCode, responseContent);
            throw new AppException($"지급대행 잔액 조회에 실패했습니다. StatusCode={(int)response.StatusCode}, Response={responseContent}", HttpStatusCode.BadGateway);
        }

        _logger.LogInformation("[TossPaymentsService.GetPayoutBalanceAsync] Response: {Response}", responseContent);

        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;

        // v2 응답: entityBody 안에 데이터가 있을 수 있음
        var dataRoot = root.TryGetProperty("entityBody", out var entityBody) ? entityBody : root;

        var result = new PayoutBalanceDto();

        if (dataRoot.TryGetProperty("availableAmount", out var available))
        {
            result.AvailableAmount = new PayoutBalanceAmountDto
            {
                Currency = available.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "KRW" : "KRW",
                Value = available.TryGetProperty("value", out var val) ? val.GetInt64() : 0
            };
        }

        if (dataRoot.TryGetProperty("pendingAmount", out var pending))
        {
            result.PendingAmount = new PayoutBalanceAmountDto
            {
                Currency = pending.TryGetProperty("currency", out var cur) ? cur.GetString() ?? "KRW" : "KRW",
                Value = pending.TryGetProperty("value", out var val) ? val.GetInt64() : 0
            };
        }

        return result;
    }

    public async Task<TransferResponseDto> RequestTransferAsync(TransferRequestDto request)
    {
        var endpoint = $"{_settings.SettlementApiBaseUrl.TrimEnd('/')}/v2/payouts";
        var payload = new
        {
            refPayoutId = request.RefPayoutId,
            destination = request.Destination,
            scheduleType = request.ScheduleType,
            payoutDate = request.PayoutDate,
            amount = new
            {
                currency = request.Currency,
                value = request.Amount
            },
            transactionDescription = request.TransactionDescription,
            metadata = request.Metadata
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        _logger.LogInformation("[TossPaymentsService.RequestTransferAsync] Payload: {Payload}", jsonPayload);

        // JWE 암호화
        string requestBody;
        if (!string.IsNullOrWhiteSpace(_settings.PayoutEncryptionKey))
        {
            requestBody = EncryptJwe(jsonPayload);
            _logger.LogInformation("[TossPaymentsService.RequestTransferAsync] JWE 암호화 적용");
        }
        else
        {
            _logger.LogWarning("[TossPaymentsService.RequestTransferAsync] PayoutEncryptionKey 미설정 - 암호화 없이 전송");
            requestBody = jsonPayload;
        }

        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        // ENCRYPTION 보안 헤더 추가
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
        requestMessage.Content = content;
        if (!string.IsNullOrWhiteSpace(_settings.PayoutEncryptionKey))
        {
            requestMessage.Headers.Add("TossPayments-api-security-mode", "ENCRYPTION");
        }

        var response = await _httpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();

        // 응답 복호화 (ENCRYPTION 보안이 적용된 경우 응답도 JWE)
        string decryptedResponse;
        if (!string.IsNullOrWhiteSpace(_settings.PayoutEncryptionKey) && !responseContent.TrimStart().StartsWith("{"))
        {
            try
            {
                decryptedResponse = DecryptJwe(responseContent);
                _logger.LogInformation("[TossPaymentsService.RequestTransferAsync] JWE 응답 복호화 성공");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TossPaymentsService.RequestTransferAsync] JWE 응답 복호화 실패. RawResponse={Response}", responseContent);
                decryptedResponse = responseContent;
            }
        }
        else
        {
            decryptedResponse = responseContent;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[TossPaymentsService.RequestTransferAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                (int)response.StatusCode, decryptedResponse);
            throw new AppException($"정산 이체 요청에 실패했습니다. StatusCode={(int)response.StatusCode}, Response={decryptedResponse}", HttpStatusCode.BadGateway);
        }

        _logger.LogInformation("[TossPaymentsService.RequestTransferAsync] Success: Response={Response}", decryptedResponse);

        string? payoutId = null;
        string? status = null;
        string? refPayoutId = request.RefPayoutId;

        using (var document = JsonDocument.Parse(decryptedResponse))
        {
            // v2 응답: entityBody.items[0] 구조
            if (document.RootElement.TryGetProperty("entityBody", out var entityBody)
                && entityBody.TryGetProperty("items", out var items)
                && items.ValueKind == JsonValueKind.Array && items.GetArrayLength() > 0)
            {
                var first = items[0];
                payoutId = first.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                status = first.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                refPayoutId = first.TryGetProperty("refPayoutId", out var refProp) ? refProp.GetString() : refPayoutId;
            }
            else if (document.RootElement.TryGetProperty("payouts", out var payouts)
                     && payouts.ValueKind == JsonValueKind.Array && payouts.GetArrayLength() > 0)
            {
                var first = payouts[0];
                payoutId = first.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                status = first.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                refPayoutId = first.TryGetProperty("refPayoutId", out var refProp) ? refProp.GetString() : refPayoutId;
            }
            else
            {
                payoutId = document.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                status = document.RootElement.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                refPayoutId = document.RootElement.TryGetProperty("refPayoutId", out var refProp) ? refProp.GetString() : refPayoutId;
            }
        }

        return new TransferResponseDto
        {
            PayoutId = payoutId,
            RefPayoutId = refPayoutId,
            Status = status,
            RawResponse = decryptedResponse
        };
    }

    public async Task<TransferStatusDto> GetTransferStatusAsync(string transferId)
    {
        var endpoint = $"{_settings.SettlementApiBaseUrl.TrimEnd('/')}/v2/payouts/{transferId}";
        var response = await _httpClient.GetAsync(endpoint);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[TossPaymentsService.GetTransferStatusAsync] Failed: StatusCode={StatusCode}, Response={Response}",
                (int)response.StatusCode, responseContent);
            throw new AppException($"정산 이체 상태 조회에 실패했습니다. StatusCode={(int)response.StatusCode}, Response={responseContent}", HttpStatusCode.BadGateway);
        }

        string? payoutId;
        string? status;
        string? failureReason;

        using (var document = JsonDocument.Parse(responseContent))
        {
            payoutId = document.RootElement.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            status = document.RootElement.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
            failureReason = document.RootElement.TryGetProperty("failureReason", out var reasonProp) ? reasonProp.GetString() : null;
        }

        return new TransferStatusDto
        {
            PayoutId = payoutId,
            Status = status,
            FailureReason = failureReason,
            RawResponse = responseContent
        };
    }

    public async Task<bool> ValidateBankAccountAsync(string bankCode, string accountNumber)
    {
        var endpoint = "/v2/bank-accounts/validate";
        var payload = new
        {
            bankCode,
            accountNumber
        };

        var requestJson = JsonSerializer.Serialize(payload);
        _logger.LogInformation(
            "[TossPaymentsService.ValidateBankAccountAsync] Request: BankCode={BankCode}, Endpoint={Endpoint}",
            bankCode, endpoint);

        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation(
            "[TossPaymentsService.ValidateBankAccountAsync] Response: StatusCode={StatusCode}, Body={Body}",
            (int)response.StatusCode, responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "[TossPaymentsService.ValidateBankAccountAsync] Failed: StatusCode={StatusCode}, BankCode={BankCode}, Body={Body}",
                (int)response.StatusCode, bankCode, responseContent);
            throw new AppException($"계좌 유효성 확인에 실패했습니다. {response.StatusCode}", HttpStatusCode.BadGateway);
        }

        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;
        // Toss v2: isValid is nested inside entityBody
        if (root.TryGetProperty("entityBody", out var entityBody))
        {
            return entityBody.TryGetProperty("isValid", out var isValidNested) && isValidNested.GetBoolean();
        }
        // fallback: v1 flat response shape
        return root.TryGetProperty("isValid", out var isValidFlat) && isValidFlat.GetBoolean();
    }

    public async Task<bool> VerifyBankAccountHolderNameAsync(string bankCode, string accountNumber, string holderName)
    {
        var endpoint = "/v2/bank-accounts/verify-holder-name";
        var payload = new
        {
            bankCode,
            accountNumber,
            holderName
        };

        var requestJson = JsonSerializer.Serialize(payload);
        _logger.LogInformation(
            "[TossPaymentsService.VerifyBankAccountHolderNameAsync] Request: BankCode={BankCode}, Endpoint={Endpoint}",
            bankCode, endpoint);

        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger.LogInformation(
            "[TossPaymentsService.VerifyBankAccountHolderNameAsync] Response: StatusCode={StatusCode}, Body={Body}",
            (int)response.StatusCode, responseContent);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "[TossPaymentsService.VerifyBankAccountHolderNameAsync] Failed: StatusCode={StatusCode}, BankCode={BankCode}, Body={Body}",
                (int)response.StatusCode, bankCode, responseContent);
            throw new AppException($"예금주명 검증에 실패했습니다. {response.StatusCode}", HttpStatusCode.BadGateway);
        }

        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;
        // Toss v2: isValid is nested inside entityBody
        if (root.TryGetProperty("entityBody", out var entityBody))
        {
            return entityBody.TryGetProperty("isValid", out var isValidNested) && isValidNested.GetBoolean();
        }
        // fallback: v1 flat response shape
        return root.TryGetProperty("isValid", out var isValidFlat) && isValidFlat.GetBoolean();
    }

    // ==================== JWE 암호화 (지급대행 ENCRYPTION 보안) ====================

    /// <summary>
    /// 보안 키 (64자 Hex) → 32바이트 배열 변환
    /// </summary>
    private static byte[] ConvertHexToBytes(string hex)
    {
        return Enumerable.Range(0, hex.Length / 2)
            .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
            .ToArray();
    }

    /// <summary>
    /// JWE 암호화 (dir + A256GCM)
    /// TossPayments ENCRYPTION 보안 방식
    /// </summary>
    private string EncryptJwe(string jsonPayload)
    {
        if (string.IsNullOrWhiteSpace(_settings.PayoutEncryptionKey))
        {
            throw new InvalidOperationException("PayoutEncryptionKey(보안 키)가 설정되지 않았습니다.");
        }

        var keyBytes = ConvertHexToBytes(_settings.PayoutEncryptionKey);
        var headers = new Dictionary<string, object>
        {
            { "iat", DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:sszzz") },
            { "nonce", Guid.NewGuid().ToString() }
        };

        return JWT.Encode(jsonPayload, keyBytes, JweAlgorithm.DIR, JweEncryption.A256GCM, extraHeaders: headers);
    }

    /// <summary>
    /// JWE 복호화 (dir + A256GCM)
    /// </summary>
    private string DecryptJwe(string jweToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.PayoutEncryptionKey))
        {
            throw new InvalidOperationException("PayoutEncryptionKey(보안 키)가 설정되지 않았습니다.");
        }

        var keyBytes = ConvertHexToBytes(_settings.PayoutEncryptionKey);
        return JWT.Decode(jweToken, keyBytes, JweAlgorithm.DIR, JweEncryption.A256GCM);
    }
}
