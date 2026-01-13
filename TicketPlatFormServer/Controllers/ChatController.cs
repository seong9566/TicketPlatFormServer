using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO;
using TicketPlatFormServer.DTO.Chat;
using TicketPlatFormServer.Hubs;
using TicketPlatFormServer.Services.Chat;

namespace TicketPlatFormServer.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController(IChatService chatService, IHubContext<ChatHub> hubContext) : ControllerBase
{
    /// <summary>
    /// 채팅방 생성 또는 조회
    /// </summary>
    [HttpPost("rooms")]
    public async Task<IActionResult> CreateOrGetChatRoom([FromBody] CreateChatRoomReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.GetOrCreateChatRoom(req.TicketId, userId.Value);
        var resp = new ApiResponse<ChatRoomDetailRespDto>(
            message: "채팅방 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 내 채팅방 목록 조회
    /// </summary>
    [HttpGet("rooms")]
    public async Task<IActionResult> GetChatRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.GetChatRooms(userId.Value, page, pageSize);
        var resp = new ApiResponse<List<ChatRoomListRespDto>>(
            message: "채팅방 목록 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 채팅방 상세 조회
    /// </summary>
    [HttpGet("rooms/{roomId}")]
    public async Task<IActionResult> GetChatRoomDetail(long roomId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.GetChatRoomDetail(roomId, userId.Value);
        var resp = new ApiResponse<ChatRoomDetailRespDto>(
            message: "채팅방 상세 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 메시지 전송 (텍스트 또는 이미지)
    /// </summary>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromForm] SendMessageReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        req.UserId = userId.Value;

        var result = await chatService.SendMessage(req);

        // SignalR을 통해 실시간으로 메시지 브로드캐스트
        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("ReceiveMessage", new NewMessageSignalDto
            {
                MessageId = result.MessageId,
                RoomId = result.RoomId,
                SenderId = userId.Value,
                SenderNickname = "", // TODO: 사용자 정보 추가
                SenderProfileImage = null,
                Message = result.Message,
                ImageUrl = result.ImageUrl,
                CreatedAt = result.CreatedAt
            });

        var resp = new ApiResponse<SendMessageRespDto>(
            message: "메시지 전송 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 메시지 목록 조회 (페이지네이션)
    /// </summary>
    [HttpGet("rooms/{roomId}/messages")]
    public async Task<IActionResult> GetMessages(
        long roomId,
        [FromQuery] long? lastMessageId = null,
        [FromQuery] int limit = 50)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var req = new GetMessagesReqDto
        {
            RoomId = roomId,
            UserId = userId.Value,
            LastMessageId = lastMessageId,
            Limit = limit
        };

        var result = await chatService.GetMessages(req);
        var resp = new ApiResponse<List<ChatMessageRespDto>>(
            message: "메시지 목록 조회 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 메시지 읽음 처리
    /// </summary>
    [HttpPost("rooms/{roomId}/read")]
    public async Task<IActionResult> MarkMessagesAsRead(long roomId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        await chatService.MarkMessagesAsRead(roomId, userId.Value);
        var resp = new ApiResponse<object>(
            message: "메시지 읽음 처리 완료",
            data: null,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 결제 요청 (판매자가 구매자에게)
    /// </summary>
    [HttpPost("rooms/{roomId}/request-payment")]
    public async Task<IActionResult> RequestPayment(long roomId, [FromBody] RequestPaymentReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.RequestPayment(roomId, req.TransactionId, userId.Value);

        // SignalR로 채팅방에 결제 요청 알림
        await hubContext.Clients.Group($"room_{roomId}")
            .SendAsync("RoomUpdated", new RoomUpdatedSignalDto
            {
                RoomId = roomId,
                Event = "PaymentRequested",
                TransactionId = req.TransactionId,
                Message = "결제가 요청되었습니다."
            });

        var resp = new ApiResponse<PaymentUrlRespDto>(
            message: "결제 요청이 전송되었습니다",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 구매 확정 (구매자가 확정)
    /// </summary>
    [HttpPost("rooms/{roomId}/confirm-purchase")]
    public async Task<IActionResult> ConfirmPurchase(long roomId, [FromBody] ConfirmPurchaseReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        req.UserId = userId.Value;
        req.RoomId = roomId;

        var result = await chatService.ConfirmPurchase(req);

        // SignalR로 채팅방에 구매 확정 알림
        await hubContext.Clients.Group($"room_{roomId}")
            .SendAsync("RoomUpdated", new RoomUpdatedSignalDto
            {
                RoomId = roomId,
                Event = "PurchaseConfirmed",
                TransactionId = result.TransactionId,
                Message = "구매가 확정되었습니다."
            });

        var resp = new ApiResponse<PurchaseConfirmRespDto>(
            message: "구매 확정이 완료되었습니다",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 거래 취소
    /// </summary>
    [HttpPost("rooms/{roomId}/cancel")]
    public async Task<IActionResult> CancelTransaction(long roomId, [FromBody] CancelTransactionReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        req.UserId = userId.Value;
        req.RoomId = roomId;

        await chatService.CancelTransaction(req);

        // SignalR로 채팅방에 거래 취소 알림
        await hubContext.Clients.Group($"room_{roomId}")
            .SendAsync("RoomUpdated", new RoomUpdatedSignalDto
            {
                RoomId = roomId,
                Event = "TransactionCancelled",
                TransactionId = req.TransactionId,
                StatusCode = "cancelled",
                Message = $"거래가 취소되었습니다. 사유: {req.CancelReason}"
            });

        var resp = new ApiResponse<object>(
            message: "거래가 취소되었습니다",
            data: null,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 메시지 이미지 URL 재발급
    /// </summary>
    [HttpGet("messages/{messageId}/image-url")]
    public async Task<IActionResult> RefreshImageUrl(long messageId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.RefreshImageUrl(messageId, userId.Value);
        var resp = new ApiResponse<RefreshImageUrlRespDto>(
            message: "이미지 URL 재발급 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }
}
