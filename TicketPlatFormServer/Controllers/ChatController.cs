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
public class ChatController(IChatService chatService, IHubContext<ChatHub> hubContext, ILogger<ChatController> logger) : ControllerBase
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
    [HttpGet("rooms/detail")]
    public async Task<IActionResult> GetChatRoomDetail([FromQuery] long roomId)
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
    /// 티켓으로 채팅방 조회 (생성하지 않음)
    /// </summary>
    [HttpGet("rooms/by-ticket")]
    public async Task<IActionResult> GetChatRoomByTicket([FromQuery] int ticketId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.GetChatRoomByTicket(ticketId, userId.Value);

        if (result == null)
        {
            var notFoundResp = new ApiResponse<object>(
                message: "채팅방이 존재하지 않습니다",
                data: null,
                statusCode: 404
            );
            return NotFound(notFoundResp);
        }

        var resp = new ApiResponse<ChatRoomDetailRespDto>(
            message: "채팅방 조회 성공",
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

        // 채팅방 정보 조회하여 수신자 파악
        var room = await chatService.GetChatRoomById(req.RoomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 수신자 ID 결정 (발신자가 아닌 다른 사람)
        var receiverId = room.BuyerId == userId.Value
            ? room.SellerId
            : room.BuyerId;

        var messageDto = new NewMessageSignalDto
        {
            MessageId = result.MessageId,
            RoomId = result.RoomId,
            SenderId = userId.Value,
            SenderNickname = result.SenderNickname,
            SenderProfileImage = result.SenderProfileImage,
            Message = result.Message,
            Type = result.Type,
            Images = result.Images,
            CreatedAt = result.CreatedAt
        };

        logger.LogInformation(
            "[ChatController.SendMessage] Broadcasting message to room_{RoomId}. MessageId: {MessageId}, SenderId: {SenderId}, ReceiverId: {ReceiverId}",
            req.RoomId, result.MessageId, userId.Value, receiverId);

        // 채팅방 안에 있는 사람들에게 전송 (기존)
        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("ReceiveMessage", messageDto);

        // 수신자에게 직접 전송 (어느 화면에 있든 수신 가능)
        await hubContext.Clients.Group($"user_{receiverId}")
            .SendAsync("ReceiveMessage", messageDto);

        logger.LogInformation(
            "[ChatController.SendMessage]  SignalR broadcast completed: room_{RoomId} and user_{ReceiverId}",
            req.RoomId, receiverId);

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
    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(
        [FromQuery] long roomId,
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
    [HttpPost("rooms/read")]
    public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkMessagesAsReadReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        await chatService.MarkMessagesAsRead(req.RoomId, userId.Value);
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
    [HttpPost("rooms/request-payment")]
    public async Task<IActionResult> RequestPayment([FromBody] RequestPaymentReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        var result = await chatService.RequestPayment(req.RoomId, userId.Value, req.Quantity);

        // SignalR로 채팅방에 결제 요청 알림
        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("RoomUpdated", new RoomUpdatedSignalDto
            {
                RoomId = req.RoomId,
                Event = "PaymentRequested",
                TransactionId = result.TransactionId,
                Message = "결제가 요청되었습니다."
            });

        var resp = new ApiResponse<PaymentUrlRespDto>(
            message: "결제 요청 성공",
            data: result,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 구매 확정 (구매자가 확정)
    /// </summary>
    [HttpPost("rooms/confirm-purchase")]
    public async Task<IActionResult> ConfirmPurchase([FromBody] ConfirmPurchaseReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        req.UserId = userId.Value;

        var result = await chatService.ConfirmPurchase(req);

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
    [HttpPost("rooms/cancel")]
    public async Task<IActionResult> CancelTransaction([FromBody] CancelTransactionReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        req.UserId = userId.Value;

        await chatService.CancelTransaction(req);

        // SignalR로 채팅방에 거래 취소 알림
        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("RoomUpdated", new RoomUpdatedSignalDto
            {
                RoomId = req.RoomId,
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
    /// 채팅방 나가기
    /// </summary>
    [HttpPost("rooms/leave")]
    public async Task<IActionResult> LeaveChatRoom([FromBody] LeaveChatRoomReqDto req)
    {
        var userId = User.GetUserId();
        if (userId == null)
            throw new AppException("인증 정보가 없습니다.", HttpStatusCode.Unauthorized);

        req.UserId = userId.Value;

        await chatService.LeaveChatRoom(req);

        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("RoomUpdated", new RoomUpdatedSignalDto
            {
                RoomId = req.RoomId,
                Event = "RoomClosed",
                StatusCode = "closed",
                Message = "채팅방이 종료되었습니다."
            });

        var resp = new ApiResponse<object>(
            message: "채팅방 나가기 완료",
            data: null,
            statusCode: 200
        );
        return Ok(resp);
    }

    /// <summary>
    /// 메시지 이미지 URL 재발급
    /// </summary>
    [HttpGet("messages/image-url")]
    public async Task<IActionResult> RefreshImageUrl([FromQuery] long messageId)
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
