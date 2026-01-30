using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

        // Basic Authentication 설정 (SecretKey를 Base64 인코딩)
        var authToken = "dGVzdF9nc2tfZG9jc19PYVB6OEw1S2RtUVhrelJ6M3k0N0JNdzY6";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
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
}
