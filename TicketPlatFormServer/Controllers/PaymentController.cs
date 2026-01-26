using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Payment;
using TicketPlatFormServer.Services.Payment;

namespace TicketPlatFormServer.Controllers;

/// <summary>
/// 결제 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController(
    IPaymentService paymentService,
    ILogger<PaymentController> logger) : ControllerBase
{
    // 토스페이먼츠 Webhook IP 화이트리스트
    private static readonly string[] TossWebhookIPs =
    {
        "52.79.60.235",
        "13.124.227.214"
    };

    /// <summary>
    /// 결제 요청 준비 (OrderId 생성)
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> RequestPayment([FromBody] PaymentRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        logger.LogInformation("[PaymentController.RequestPayment] UserId: {UserId}, TransactionId: {TransactionId}",
            userId.Value, request.TransactionId);

        var result = await paymentService.InitiatePaymentAsync(request, userId.Value);

        var response = new ApiResponse<PaymentRequestResponseDto>(
            message: "결제 요청 준비 완료",
            data: result,
            statusCode: 200
        );

        return Ok(response);
    }

    /// <summary>
    /// 결제 승인 (토스페이먼츠 결제창에서 성공 후 호출)
    /// </summary>
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmRequestDto request)
    {
        logger.LogInformation("[PaymentController.ConfirmPayment] OrderId: {OrderId}, PaymentKey: {PaymentKey}",
            request.OrderId, request.PaymentKey);

        var result = await paymentService.ConfirmPaymentAsync(request);

        var response = new ApiResponse<TossPaymentResponseDto>(
            message: "결제 승인 완료",
            data: result,
            statusCode: 200
        );

        return Ok(response);
    }

    /// <summary>
    /// 결제 취소 (환불)
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelPayment([FromBody] PaymentCancelRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        logger.LogInformation("[PaymentController.CancelPayment] UserId: {UserId}, TransactionId: {TransactionId}",
            userId.Value, request.TransactionId);

        var result = await paymentService.CancelPaymentAsync(request, userId.Value);

        var response = new ApiResponse<PaymentCancelResponseDto>(
            message: "결제 취소 완료",
            data: result,
            statusCode: 200
        );

        return Ok(response);
    }

    /// <summary>
    /// OrderId로 결제 조회
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetPaymentByOrderId(string orderId)
    {
        logger.LogInformation("[PaymentController.GetPaymentByOrderId] OrderId: {OrderId}", orderId);

        var result = await paymentService.GetPaymentByOrderIdAsync(orderId);

        var response = new ApiResponse<TossPaymentResponseDto>(
            message: "결제 조회 완료",
            data: result,
            statusCode: 200
        );

        return Ok(response);
    }

    /// <summary>
    /// 토스페이먼츠 Webhook 수신
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook([FromBody] TossWebhookDto webhook)
    {
        // IP 화이트리스트 검증
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        if (remoteIp != null && !TossWebhookIPs.Contains(remoteIp) && remoteIp != "::1" && remoteIp != "127.0.0.1")
        {
            logger.LogWarning("[PaymentController.HandleWebhook] 허용되지 않은 IP: {RemoteIp}", remoteIp);
            return Unauthorized(new { message = "Unauthorized IP address" });
        }

        logger.LogInformation("[PaymentController.HandleWebhook] EventType: {EventType}, PaymentKey: {PaymentKey}",
            webhook.EventType, webhook.Data?.PaymentKey);

        await paymentService.HandleWebhookAsync(webhook);

        return Ok(new TossWebhookResponse
        {
            Success = true,
            Message = "Webhook processed successfully"
        });
    }
}
