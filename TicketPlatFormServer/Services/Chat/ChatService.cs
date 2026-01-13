using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Chat;
using TicketPlatFormServer.Repository.Chat;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Repository.Transactions;
using TicketPlatFormServer.Services.FileUpload;

namespace TicketPlatFormServer.Services.Chat;

public class ChatService(
    IChatRepository chatRepo,
    ITicketRepository ticketRepo,
    ITransactionRepository transactionRepo,
    IFileUploadService fileUploadService,
    ILogger<ChatService> logger) : IChatService
{
    private const int DetailMessageLimit = 30;
    /// <summary>
    /// 채팅방 조회 또는 생성
    /// </summary>
    public async Task<ChatRoomDetailRespDto> GetOrCreateChatRoom(int ticketId, int userId)
    {
        // 티켓 조회
        var ticket = await ticketRepo.GetTicketDetailById((int)ticketId);
        if (ticket == null)
        {
            throw new AppException("티켓을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 자기 자신과 채팅 방지
        if (ticket.Seller.UserId == userId)
        {
            throw new AppException("본인의 티켓과는 채팅할 수 없습니다.", HttpStatusCode.BadRequest);
        }

        // 기존 채팅방 조회
        var existingRoom = await chatRepo.GetChatRoomByTicketAndBuyer(ticketId, userId);

        if (existingRoom != null)
        {
            var messages = await GetRecentMessages(existingRoom.Id, userId);
            return MapToRoomDetailDto(existingRoom, userId, messages);
        }

        // 새 채팅방 생성
        var activeStatusId = await chatRepo.GetStatusIdByCode("active");
        var newRoom = await chatRepo.CreateChatRoom(ticketId, userId, ticket.Seller.UserId, activeStatusId);

        logger.LogInformation("[ChatService.GetOrCreateChatRoom] 새 채팅방 생성: RoomId={RoomId}, TicketId={TicketId}, BuyerId={BuyerId}, SellerId={SellerId}",
            newRoom.Id, ticketId, userId, ticket.Seller.UserId);

        var newRoomMessages = await GetRecentMessages(newRoom.Id, userId);
        return MapToRoomDetailDto(newRoom, userId, newRoomMessages);
    }

    /// <summary>
    /// 내 채팅방 목록 조회
    /// </summary>
    public async Task<List<ChatRoomListRespDto>> GetChatRooms(int userId, int page, int pageSize)
    {
        var rooms = await chatRepo.GetChatRoomsByUserId(userId, page, pageSize);

        return rooms.Select(room => new ChatRoomListRespDto
        {
            RoomId = room.Id,
            TicketId = room.TicketId,
            TicketTitle = room.Ticket?.Title ?? "",
            OtherUser = new OtherUserInfo
            {
                UserId = room.BuyerId == userId ? room.SellerId : room.BuyerId,
                Nickname = room.BuyerId == userId
                    ? room.Seller?.UserProfile?.Nickname ?? "Unknown"
                    : room.Buyer?.UserProfile?.Nickname ?? "Unknown",
                ProfileImageUrl = room.BuyerId == userId
                    ? room.Seller?.UserProfile?.ProfileImageUrl
                    : room.Buyer?.UserProfile?.ProfileImageUrl
            },
            LastMessage = null, // 별도 쿼리 필요
            LastMessageAt = room.LastMessageAt,
            UnreadCount = room.BuyerId == userId ? (room.UnreadCountBuyer ?? 0) : (room.UnreadCountSeller ?? 0),
            RoomStatusCode = room.Status?.Code ?? "",
            RoomStatusName = room.Status?.NameKo ?? "",
            TransactionId = room.TransactionId,
            TransactionStatusCode = room.Transaction?.Status?.Code,
            TransactionStatusName = room.Transaction?.Status?.NameKo
        }).ToList();
    }

    /// <summary>
    /// 채팅방 상세 조회
    /// </summary>
    public async Task<ChatRoomDetailRespDto> GetChatRoomDetail(long roomId, int userId)
    {
        // 채팅방 존재 확인 (404 우선)
        var room = await chatRepo.GetChatRoomById(roomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 권한 확인 (403)
        await ValidateUserInRoom(roomId, userId);

        var messages = await GetRecentMessages(roomId, userId);
        return MapToRoomDetailDto(room, userId, messages);
    }

    /// <summary>
    /// 메시지 전송
    /// </summary>
    public async Task<SendMessageRespDto> SendMessage(SendMessageReqDto req)
    {
        // 채팅방 존재 확인 (404 우선)
        var room = await chatRepo.GetChatRoomById(req.RoomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 권한 확인 (403)
        await ValidateUserInRoom(req.RoomId, req.UserId);

        if (room.LockedAt != null || room.ClosedAt != null)
        {
            throw new AppException("이 채팅방은 더 이상 메시지를 보낼 수 없습니다.", HttpStatusCode.Forbidden);
        }

        // 이미지 업로드 (있는 경우)
        string? imageObjectKey = null;
        string? imageSignedUrl = null;
        DateTime? imageExpiresAt = null;

        if (req.Image != null)
        {
            var uploadResult = await fileUploadService.UploadChatImageAsync(req.Image, req.UserId, req.RoomId);
            imageObjectKey = uploadResult.ObjectKey;
            imageSignedUrl = uploadResult.SignedUrl;
            imageExpiresAt = uploadResult.ExpiresAt;
        }

        // 메시지 또는 이미지 중 하나는 필수
        if (string.IsNullOrWhiteSpace(req.Message) && string.IsNullOrWhiteSpace(imageObjectKey))
        {
            throw new AppException("메시지 또는 이미지를 입력해주세요.", HttpStatusCode.BadRequest);
        }

        // 메시지 저장 (DB에는 object key 저장)
        var message = await chatRepo.CreateMessage(req.RoomId, req.UserId, req.Message, imageObjectKey);

        // 마지막 메시지 시간 업데이트
        await chatRepo.UpdateLastMessageAt(req.RoomId, message.CreatedAt ?? DateTime.UtcNow);

        // 상대방 읽지 않은 메시지 수 증가
        var isSenderBuyer = room.BuyerId == req.UserId;
        await chatRepo.IncrementUnreadCount(req.RoomId, !isSenderBuyer);

        logger.LogInformation("[ChatService.SendMessage] 메시지 전송: MessageId={MessageId}, RoomId={RoomId}, SenderId={SenderId}",
            message.Id, req.RoomId, req.UserId);

        return new SendMessageRespDto
        {
            MessageId = message.Id,
            RoomId = req.RoomId,
            Message = req.Message,
            ImageUrl = imageSignedUrl,
            ImageUrlExpiresAt = imageExpiresAt,
            CreatedAt = message.CreatedAt ?? DateTime.UtcNow,
            Success = true
        };
    }

    /// <summary>
    /// 메시지 목록 조회
    /// </summary>
    public async Task<List<ChatMessageRespDto>> GetMessages(GetMessagesReqDto req)
    {
        // 권한 확인
        await ValidateUserInRoom(req.RoomId, req.UserId);

        var messages = await chatRepo.GetMessagesByRoomId(req.RoomId, req.LastMessageId, req.Limit);

        return await MapMessagesWithSignedUrls(messages, req.UserId);
    }

    /// <summary>
    /// 메시지 읽음 처리
    /// </summary>
    public async Task MarkMessagesAsRead(long roomId, int userId)
    {
        // 채팅방 존재 확인 (404 우선)
        var room = await chatRepo.GetChatRoomById(roomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 권한 확인 (403)
        await ValidateUserInRoom(roomId, userId);

        var isBuyer = room.BuyerId == userId;
        await chatRepo.ResetUnreadCount(roomId, isBuyer);

        logger.LogInformation("[ChatService.MarkMessagesAsRead] RoomId={RoomId}, UserId={UserId}", roomId, userId);
    }

    /// <summary>
    /// 결제 요청 (판매자가 구매자에게)
    /// </summary>
    public async Task<PaymentUrlRespDto> RequestPayment(long roomId, long transactionId, int userId)
    {
        // 권한 확인
        await ValidateUserInRoom(roomId, userId);

        var room = await chatRepo.GetChatRoomById(roomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 판매자인지 확인
        if (room.SellerId != userId)
        {
            throw new AppException("판매자만 결제를 요청할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        // Transaction 검증
        var transaction = await transactionRepo.GetTransactionById(transactionId);
        if (transaction == null)
        {
            throw new AppException("거래 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // Transaction 소유권 검증 (BuyerId, SellerId 일치)
        if (transaction.BuyerId != room.BuyerId || transaction.SellerId != room.SellerId)
        {
            throw new AppException("거래 정보가 이 채팅방과 일치하지 않습니다.", HttpStatusCode.Forbidden);
        }

        // Transaction 연결 (없는 경우)
        if (room.TransactionId == null)
        {
            await chatRepo.SetTransactionId(roomId, transactionId);
        }

        // 시스템 메시지 전송
        var systemMessage = await chatRepo.CreateMessage(roomId, userId, "결제가 요청되었습니다.", null);

        // LastMessageAt 업데이트
        await chatRepo.UpdateLastMessageAt(roomId, systemMessage.CreatedAt ?? DateTime.UtcNow);

        // 상대방 읽지 않은 수 증가
        var isSenderBuyer = room.BuyerId == userId;
        await chatRepo.IncrementUnreadCount(roomId, !isSenderBuyer);

        logger.LogInformation("[ChatService.RequestPayment] RoomId={RoomId}, TransactionId={TransactionId}, UserId={UserId}",
            roomId, transactionId, userId);

        // TODO: 실제 결제 시스템과 연동하여 결제 URL 생성
        var paymentUrl = $"/payment/{transactionId}";

        return new PaymentUrlRespDto
        {
            PaymentUrl = paymentUrl,
            TransactionId = transactionId,
            Amount = room.Ticket?.Price ?? 0
        };
    }

    /// <summary>
    /// 구매 확정 (구매자가 확정)
    /// </summary>
    public async Task<PurchaseConfirmRespDto> ConfirmPurchase(ConfirmPurchaseReqDto req)
    {
        // 권한 확인
        await ValidateUserInRoom(req.RoomId, req.UserId);

        var room = await chatRepo.GetChatRoomById(req.RoomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 구매자인지 확인
        if (room.BuyerId != req.UserId)
        {
            throw new AppException("구매자만 구매를 확정할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        // Transaction 확인
        if (room.TransactionId == null || room.TransactionId != req.TransactionId)
        {
            throw new AppException("거래 정보가 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        // TODO: Transaction 상태 업데이트 (별도 Transaction Repository 필요)
        // await transactionRepo.ConfirmTransaction(req.TransactionId, userId, DateTime.UtcNow);

        // 채팅방 잠금
        await chatRepo.LockChatRoom(req.RoomId);

        // 시스템 메시지 전송
        var systemMessage = await chatRepo.CreateMessage(req.RoomId, req.UserId, "구매가 확정되었습니다.", null);

        // LastMessageAt 업데이트
        await chatRepo.UpdateLastMessageAt(req.RoomId, systemMessage.CreatedAt ?? DateTime.UtcNow);

        // 상대방 읽지 않은 수 증가
        var isSenderBuyer = room.BuyerId == req.UserId;
        await chatRepo.IncrementUnreadCount(req.RoomId, !isSenderBuyer);

        logger.LogInformation("[ChatService.ConfirmPurchase] RoomId={RoomId}, TransactionId={TransactionId}, UserId={UserId}",
            req.RoomId, req.TransactionId, req.UserId);

        return new PurchaseConfirmRespDto
        {
            TransactionId = req.TransactionId,
            ConfirmedAt = DateTime.UtcNow,
            Success = true
        };
    }

    /// <summary>
    /// 거래 취소
    /// </summary>
    public async Task CancelTransaction(CancelTransactionReqDto req)
    {
        // 권한 확인
        await ValidateUserInRoom(req.RoomId, req.UserId);

        var room = await chatRepo.GetChatRoomById(req.RoomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 구매자 또는 판매자인지 확인
        if (room.BuyerId != req.UserId && room.SellerId != req.UserId)
        {
            throw new AppException("거래 당사자만 취소할 수 있습니다.", HttpStatusCode.Forbidden);
        }

        // Transaction 확인
        if (room.TransactionId == null || room.TransactionId != req.TransactionId)
        {
            throw new AppException("거래 정보가 일치하지 않습니다.", HttpStatusCode.BadRequest);
        }

        // TODO: Transaction 취소 및 Refund 처리 (별도 Transaction Repository 필요)
        // await transactionRepo.CancelTransaction(req.TransactionId, DateTime.UtcNow);
        // await refundRepo.CreateRefund(req.TransactionId, req.CancelReason, req.UserId);

        // 채팅방 상태 변경 (CANCELLED)
        var cancelledStatusId = await chatRepo.GetStatusIdByCode("cancelled");
        await chatRepo.UpdateChatRoomStatus(req.RoomId, cancelledStatusId);

        // 시스템 메시지 전송
        var systemMessage = await chatRepo.CreateMessage(req.RoomId, req.UserId, $"거래가 취소되었습니다. 사유: {req.CancelReason}", null);

        // LastMessageAt 업데이트
        await chatRepo.UpdateLastMessageAt(req.RoomId, systemMessage.CreatedAt ?? DateTime.UtcNow);

        // 상대방 읽지 않은 수 증가
        var isSenderBuyer = room.BuyerId == req.UserId;
        await chatRepo.IncrementUnreadCount(req.RoomId, !isSenderBuyer);

        logger.LogInformation("[ChatService.CancelTransaction] RoomId={RoomId}, TransactionId={TransactionId}, UserId={UserId}, Reason={Reason}",
            req.RoomId, req.TransactionId, req.UserId, req.CancelReason);
    }

    /// <summary>
    /// 사용자가 채팅방에 속해 있는지 검증 (권한 체크)
    /// </summary>
    private async Task ValidateUserInRoom(long roomId, int userId)
    {
        var isInRoom = await chatRepo.IsUserInChatRoom(roomId, userId);
        if (!isInRoom)
        {
            throw new AppException("이 채팅방에 접근할 권한이 없습니다.", HttpStatusCode.Forbidden);
        }
    }

    /// <summary>
    /// 메시지 이미지 URL 재발급
    /// </summary>
    public async Task<RefreshImageUrlRespDto> RefreshImageUrl(long messageId, int userId)
    {
        var message = await chatRepo.GetMessageById(messageId);
        if (message == null)
        {
            throw new AppException("메시지를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        await ValidateUserInRoom(message.RoomId, userId);

        if (string.IsNullOrEmpty(message.ImageUrl))
        {
            throw new AppException("이미지가 없는 메시지입니다.", HttpStatusCode.BadRequest);
        }

        var result = await fileUploadService.RefreshSignedUrlAsync(message.ImageUrl);

        return new RefreshImageUrlRespDto
        {
            MessageId = messageId,
            ImageUrl = result.SignedUrl,
            ExpiresAt = result.ExpiresAt
        };
    }

    /// <summary>
    /// ChatRoom 엔티티를 ChatRoomDetailRespDto로 매핑
    /// </summary>
    private async Task<List<ChatMessageRespDto>> GetRecentMessages(long roomId, int userId)
    {
        var messages = await chatRepo.GetMessagesByRoomId(roomId, null, DetailMessageLimit);
        return await MapMessagesWithSignedUrls(messages, userId);
    }

    private async Task<List<ChatMessageRespDto>> MapMessagesWithSignedUrls(IEnumerable<DBModel.ChatMessage> messages, int userId)
    {
        var messageList = messages.ToList();

        // 이미지가 있는 메시지의 object key 수집
        var imageKeys = messageList
            .Where(m => !string.IsNullOrEmpty(m.ImageUrl))
            .Select(m => m.ImageUrl!)
            .Distinct()
            .ToList();

        // 배치로 signed URL 발급
        var signedUrls = imageKeys.Count > 0
            ? await fileUploadService.RefreshSignedUrlsBatchAsync(imageKeys)
            : new Dictionary<string, SignedUrlResult>();

        return messageList.Select(m =>
        {
            var hasImage = !string.IsNullOrEmpty(m.ImageUrl);
            SignedUrlResult? signedUrlResult = null;
            if (hasImage && signedUrls.TryGetValue(m.ImageUrl!, out var result))
            {
                signedUrlResult = result;
            }

            return new ChatMessageRespDto
            {
                MessageId = m.Id,
                RoomId = m.RoomId,
                SenderId = m.SenderId,
                SenderNickname = m.Sender?.UserProfile?.Nickname ?? "Unknown",
                SenderProfileImage = m.Sender?.UserProfile?.ProfileImageUrl,
                Message = m.Message,
                ImageUrl = signedUrlResult?.SignedUrl,
                ImageUrlExpiresAt = signedUrlResult?.ExpiresAt,
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow,
                IsMyMessage = m.SenderId == userId
            };
        }).ToList();
    }

    private ChatRoomDetailRespDto MapToRoomDetailDto(DBModel.ChatRoom room, int userId, List<ChatMessageRespDto> messages)
    {
        var isBuyer = room.BuyerId == userId;
        var transaction = room.Transaction;

        return new ChatRoomDetailRespDto
        {
            RoomId = room.Id,
            Ticket = new TicketInfo
            {
                TicketId = room.TicketId,
                Title = room.Ticket?.Title ?? "",
                Price = room.Ticket?.Price ?? 0,
                ThumbnailUrl = null // TODO: 티켓 이미지 추가
            },
            Buyer = new UserInfo
            {
                UserId = room.BuyerId,
                Nickname = room.Buyer?.UserProfile?.Nickname ?? "Unknown",
                ProfileImageUrl = room.Buyer?.UserProfile?.ProfileImageUrl,
                MannerTemperature = room.Buyer?.UserProfile?.MannerTemperature ?? 36.5
            },
            Seller = new UserInfo
            {
                UserId = room.SellerId,
                Nickname = room.Seller?.UserProfile?.Nickname ?? "Unknown",
                ProfileImageUrl = room.Seller?.UserProfile?.ProfileImageUrl,
                MannerTemperature = room.Seller?.UserProfile?.MannerTemperature ?? 36.5
            },
            StatusCode = room.Status?.Code ?? "",
            StatusName = room.Status?.NameKo ?? "",
            Transaction = transaction != null ? new TransactionInfo
            {
                TransactionId = transaction.Id,
                StatusCode = transaction.Status?.Code ?? "",
                StatusName = transaction.Status?.NameKo ?? "",
                ConfirmedAt = transaction.ConfirmedAt,
                CancelledAt = transaction.CancelledAt
            } : null,
            CanSendMessage = room.LockedAt == null && room.ClosedAt == null,
            CanRequestPayment = !isBuyer && room.LockedAt == null && room.ClosedAt == null,
            CanConfirmPurchase = isBuyer && transaction != null && transaction.ConfirmedAt == null,
            CanCancelTransaction = transaction != null && transaction.ConfirmedAt == null && transaction.CancelledAt == null,
            Messages = messages
        };
    }
}
