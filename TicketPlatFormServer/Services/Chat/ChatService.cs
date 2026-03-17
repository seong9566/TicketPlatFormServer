using System.Net;
using Microsoft.AspNetCore.SignalR;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.DTO.Chat;
using TicketPlatFormServer.Enum;
using TicketPlatFormServer.Hubs;
using TicketPlatFormServer.Repository.Chat;
using TicketPlatFormServer.Repository.Reputation;
using TicketPlatFormServer.Repository.Sell;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Repository.Transactions;
using TicketPlatFormServer.Services.FileUpload;
using TicketPlatFormServer.Services.Notification;

namespace TicketPlatFormServer.Services.Chat;

public class ChatService(
    IChatRepository chatRepo,
    ISellRepository sellRepository,
    ITicketRepository ticketRepo,
    ITransactionRepository transactionRepo,
    IReputationRepository reputationRepository,
    ITransactionItemRepository transactionItemRepo,
    IFileUploadService fileUploadService,
    INotificationService notificationService,
    IHubContext<ChatHub> hubContext,
    SupabaseStorageSettings supabaseSettings,
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
            // 닫힌 방이면 soft delete 후 새 방 생성
            if (existingRoom.ClosedAt != null)
            {
                await chatRepo.SoftDeleteChatRoom(existingRoom.Id);
                logger.LogInformation("[ChatService.GetOrCreateChatRoom] 닫힌 채팅방 삭제 후 새 방 생성: OldRoomId={OldRoomId}, TicketId={TicketId}, BuyerId={BuyerId}",
                    existingRoom.Id, ticketId, userId);
            }
            else
            {
                // 활성 방이면 재사용
                var messages = await GetRecentMessages(existingRoom.Id, userId);
                return await MapToRoomDetailDto(existingRoom, userId, messages);
            }
        }

        // 새 채팅방 생성
        var activeStatusId = await chatRepo.GetStatusIdByCode("active");
        var newRoom = await chatRepo.CreateChatRoom(ticketId, userId, ticket.Seller.UserId, activeStatusId);

        logger.LogInformation("[ChatService.GetOrCreateChatRoom] 새 채팅방 생성: RoomId={RoomId}, TicketId={TicketId}, BuyerId={BuyerId}, SellerId={SellerId}",
            newRoom.Id, ticketId, userId, ticket.Seller.UserId);

        var newRoomMessages = await GetRecentMessages(newRoom.Id, userId);
        return await MapToRoomDetailDto(newRoom, userId, newRoomMessages);
    }

    /// <summary>
    /// 내 채팅방 목록 조회
    /// </summary>
    public async Task<List<ChatRoomListRespDto>> GetChatRooms(int userId, int page, int pageSize)
    {
        var rooms = await chatRepo.GetChatRoomsByUserId(userId, page, pageSize);

        // 1. 프로필 이미지 Object Key 수집
        var profileKeys = rooms
            .Select(room =>
            {
                var otherUserProfile = room.BuyerId == userId
                    ? room.Seller?.UserProfile?.ProfileImageUrl
                    : room.Buyer?.UserProfile?.ProfileImageUrl;
                return otherUserProfile;
            })
            .Where(url => !string.IsNullOrEmpty(url) &&
                          !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                          !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .Select(url => url!)
            .Distinct()
            .ToList();

        var posterKeys = rooms
            .Select(room => room.Ticket?.Event?.PosterImageUrl)
            .Where(url => !string.IsNullOrEmpty(url) &&
                          !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                          !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .Select(url => url!)
            .Distinct()
            .ToList();

        var allKeys = profileKeys.Concat(posterKeys).Distinct().ToList();

        // 2. 배치로 Signed URL 발급
        var signedUrls = allKeys.Count > 0
            ? await fileUploadService.RefreshSignedUrlsBatchAsync(allKeys)
            : new Dictionary<string, SignedUrlResult>();

        // 3. 마지막 메시지 조회
        var roomIds = rooms.Select(r => r.Id).ToList();
        var lastMessages = await chatRepo.GetLastMessagesForRooms(roomIds);

        // 4. 매핑
        return rooms.Select(room =>
        {
            var otherUserProfileKey = room.BuyerId == userId
                ? room.Seller?.UserProfile?.ProfileImageUrl
                : room.Buyer?.UserProfile?.ProfileImageUrl;

            string? otherUserProfileUrl = otherUserProfileKey;
            if (!string.IsNullOrEmpty(otherUserProfileKey) &&
                !otherUserProfileKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !otherUserProfileKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (signedUrls.TryGetValue(otherUserProfileKey, out var result))
                {
                    otherUserProfileUrl = result.SignedUrl;
                }
            }

            lastMessages.TryGetValue(room.Id, out var lastMsg);

            string? ticketThumbnailUrl = room.Ticket?.Event?.PosterImageUrl;
            if (!string.IsNullOrEmpty(ticketThumbnailUrl) &&
                !ticketThumbnailUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !ticketThumbnailUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (signedUrls.TryGetValue(ticketThumbnailUrl, out var ticketImageResult))
                {
                    ticketThumbnailUrl = ticketImageResult.SignedUrl;
                }
            }

            return new ChatRoomListRespDto
            {
                RoomId = room.Id,
                TicketId = (int)room.TicketId,
                TicketTitle = room.Ticket?.Event?.Title ?? "",
                TicketThumbnailUrl = ticketThumbnailUrl,
                OtherUser = new OtherUserInfo
                {
                    UserId = (int)(room.BuyerId == userId ? room.SellerId : room.BuyerId),
                    Nickname = room.BuyerId == userId
                        ? room.Seller?.UserProfile?.Nickname ?? "Unknown"
                        : room.Buyer?.UserProfile?.Nickname ?? "Unknown",
                    ProfileImageUrl = otherUserProfileUrl
                },
                LastMessage = lastMsg,
                LastMessageAt = room.LastMessageAt,
                UnreadCount = room.BuyerId == userId ? (room.UnreadCountBuyer ?? 0) : (room.UnreadCountSeller ?? 0),
                RoomStatusCode = room.Status?.Code ?? "",
                RoomStatusName = room.Status?.NameKo ?? "",
                TransactionId = room.TransactionId,
                TransactionStatusCode = room.Transaction?.Status?.Code,
                TransactionStatusName = room.Transaction?.Status?.NameKo
            };
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
        return await MapToRoomDetailDto(room, userId, messages);
    }

    /// <summary>
    /// 채팅방 ID로 조회 (권한 검증 없음)
    /// </summary>
    public async Task<ChatRoom?> GetChatRoomById(long roomId)
    {
        return await chatRepo.GetChatRoomById(roomId);
    }

    /// <summary>
    /// 티켓으로 채팅방 조회 (생성하지 않음)
    /// </summary>
    public async Task<ChatRoomDetailRespDto?> GetChatRoomByTicket(int ticketId, int userId)
    {
        // 티켓 조회
        var ticket = await ticketRepo.GetTicketDetailById(ticketId);
        if (ticket == null)
        {
            throw new AppException("티켓을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 채팅방 조회 (생성하지 않음)
        var existingRoom = await chatRepo.GetChatRoomByTicketAndUser(ticketId, userId);

        if (existingRoom == null)
        {
            return null; // Controller에서 404 처리
        }

        if (existingRoom.ClosedAt != null || string.Equals(existingRoom.Status?.Code, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return null; // Controller에서 404 처리
        }
        // 메시지 로드 및 매핑
        var messages = await GetRecentMessages(existingRoom.Id, userId);
        return await MapToRoomDetailDto(existingRoom, userId, messages);
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

        // 이미지 개수 제한 검증
        var maxImages = supabaseSettings.MaxChatImagesPerMessage;
        if (req.Images != null && req.Images.Count > maxImages)
        {
            throw new AppException($"이미지는 최대 {maxImages}개까지 전송할 수 있습니다.", HttpStatusCode.BadRequest);
        }

        // 메시지 또는 이미지 중 하나는 필수
        if (string.IsNullOrWhiteSpace(req.Message) && (req.Images == null || req.Images.Count == 0))
        {
            throw new AppException("메시지 또는 이미지를 입력해주세요.", HttpStatusCode.BadRequest);
        }

        // 다중 이미지 업로드 (롤백 지원)
        var uploadResults = new List<(string ObjectKey, string SignedUrl, DateTime ExpiresAt)>();
        var uploadedKeys = new List<string>();
        ChatMessage? message = null;

        try
        {
            // 1단계: 이미지 업로드 (스토리지)
            if (req.Images != null && req.Images.Count > 0)
            {
                foreach (var image in req.Images)
                {
                    var result = await fileUploadService.UploadChatImageAsync(image, req.UserId, req.RoomId);
                    uploadResults.Add((result.ObjectKey, result.SignedUrl, result.ExpiresAt));
                    uploadedKeys.Add(result.ObjectKey);
                }
            }

            // 2단계: 메시지 저장 (DB에는 object key 저장)
            var imageKeys = uploadResults.Select(r => r.ObjectKey).ToList();
            var messageType = imageKeys.Count > 0 ? Enum.MessageType.IMAGE : Enum.MessageType.TEXT;
            message = await chatRepo.CreateMessageWithImages(req.RoomId, req.UserId, req.Message, imageKeys, messageType);

            // 3단계: 메타데이터 업데이트 (이 단계 실패 시 메시지도 롤백 필요)
            try
            {
                // 마지막 메시지 시간 업데이트
                await chatRepo.UpdateLastMessageAt(req.RoomId, message.CreatedAt ?? DateTime.UtcNow);

                // 상대방 읽지 않은 메시지 수 증가
                var isSenderBuyer = room.BuyerId == req.UserId;
                await chatRepo.IncrementUnreadCount(req.RoomId, !isSenderBuyer);

                if (messageType == Enum.MessageType.TEXT || messageType == Enum.MessageType.IMAGE)
                {
                    var receiverId = isSenderBuyer ? room.SellerId : room.BuyerId;
                    var ticketTitle = room.Ticket?.Event?.Title ?? "새 채팅 메시지";
                    var messagePreview = messageType == Enum.MessageType.IMAGE
                        ? "[이미지]"
                        : (req.Message?.Trim() ?? string.Empty);
                    var ticketImageUrl = await GetTicketImageUrlForNotificationAsync(room.TicketId);

                    await notificationService.CreateAndSendAsync(
                        receiverId,
                        "CHAT_MESSAGE",
                        ticketTitle,
                        messagePreview,
                        new Dictionary<string, string>
                        {
                            ["type"] = "CHAT_MESSAGE",
                            ["title"] = ticketTitle,
                            ["body"] = messagePreview,
                            ["roomId"] = req.RoomId.ToString(),
                            ["message"] = messagePreview,
                            ["messageType"] = messageType.ToString(),
                            ["ticketTitle"] = ticketTitle,
                            ["ticketImageUrl"] = ticketImageUrl,
                            ["senderId"] = req.UserId.ToString(),
                            ["messageId"] = message.Id.ToString()
                        });
                }
            }
            catch (Exception metadataEx)
            {
                // 메타데이터 업데이트 실패 시 메시지 삭제 (보상 트랜잭션)
                logger.LogError(metadataEx, "[ChatService.SendMessage] 메타데이터 업데이트 실패, 메시지 롤백: MessageId={MessageId}", message.Id);
                await chatRepo.DeleteMessage(message.Id);
                throw;
            }

            // 발신자 정보 조회
            var (senderNickname, senderProfileImage) = await GetSenderInfoForSignalR(message.Id);

            logger.LogInformation("[ChatService.SendMessage] 메시지 전송 성공: MessageId={MessageId}, RoomId={RoomId}, SenderId={SenderId}, ImageCount={ImageCount}",
                message.Id, req.RoomId, req.UserId, uploadResults.Count);

            return new SendMessageRespDto
            {
                MessageId = message.Id,
                RoomId = req.RoomId,
                SenderId = req.UserId,
                ClientMessageId = req.ClientMessageId,
                SenderNickname = senderNickname,
                SenderProfileImage = senderProfileImage,
                Message = req.Message,
                Type = message.Type.ToString(),
                Images = uploadResults.Select(r => new ImageInfo
                {
                    Url = r.SignedUrl,
                    ExpiresAt = r.ExpiresAt
                }).ToList(),
                CreatedAt = message.CreatedAt ?? DateTime.UtcNow,
                Success = true
            };
        }
        catch (Exception ex)
        {
            // 전체 롤백: 메시지 삭제 (이미 시도했을 수도 있지만 안전하게 재시도)
            if (message != null)
            {
                try
                {
                    await chatRepo.DeleteMessage(message.Id);
                    logger.LogInformation("[ChatService.SendMessage] 롤백 완료: 메시지 삭제 MessageId={MessageId}", message.Id);
                }
                catch (Exception deleteEx)
                {
                    logger.LogError(deleteEx, "[ChatService.SendMessage] 메시지 삭제 실패 (이미 삭제되었을 수 있음): MessageId={MessageId}", message.Id);
                }
            }

            // 업로드된 파일 삭제
            if (uploadedKeys.Count > 0)
            {
                logger.LogWarning(ex, "[ChatService.SendMessage] 메시지 전송 실패, 업로드된 이미지 롤백: {Count}개", uploadedKeys.Count);
                foreach (var key in uploadedKeys)
                {
                    try
                    {
                        await fileUploadService.DeleteFileAsync(key);
                    }
                    catch (Exception fileDeleteEx)
                    {
                        logger.LogError(fileDeleteEx, "[ChatService.SendMessage] 파일 삭제 실패: {Key}", key);
                    }
                }
            }

            throw;
        }
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
    /// Transaction 자동 생성 및 결제 요청
    /// </summary>
    public async Task<TransactionCreatedRespDto> RequestPayment(long roomId, int userId, int quantity)
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

        // Transaction이 이미 존재하는지 확인
        if (room.TransactionId != null)
        {
            throw new AppException("이미 거래가 진행중입니다.", HttpStatusCode.BadRequest);
        }

        if (quantity <= 0)
        {
            throw new AppException("유효하지 않은 수량입니다.", HttpStatusCode.BadRequest);
        }

        // Ticket 정보 확인
        if (room.Ticket == null)
        {
            throw new AppException("티켓 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        // 재고 예약 (원자적 감소)
        var isReserved = await ticketRepo.TryReserveTicketQuantityAsync((int)room.TicketId, quantity);
        if (!isReserved)
        {
            throw new AppException("판매 가능한 수량이 부족합니다.", HttpStatusCode.BadRequest);
        }

        try
        {
            // 1. Transaction 생성
            var pendingStatus = await transactionRepo.GetTransactionStatusByCodeAsync("pending_payment");
            if (pendingStatus == null)
            {
                throw new AppException("거래 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
            }

            var totalAmount = room.Ticket.Price * quantity;

            var transaction = new DBModel.Transaction
            {
                BuyerId = room.BuyerId,
                SellerId = room.SellerId,
                StatusId = pendingStatus.Id,
                Amount = totalAmount,
                ReservedAt = DateTime.UtcNow,
                ReservationExpiresAt = DateTime.UtcNow.AddHours(24) // 24시간 후 만료
            };

            var createdTransaction = await transactionRepo.CreateTransactionAsync(transaction);

            // 2. TransactionItem 생성 (티켓 정보)
            var transactionItem = new TransactionItem
            {
                TransactionId = createdTransaction.Id,
                TicketId = (int)room.TicketId,
                Quantity = quantity,
                UnitPrice = room.Ticket.Price,
                TotalPrice = totalAmount
            };

            await transactionItemRepo.CreateTransactionItemAsync(transactionItem);

            // 3. ChatRoom에 Transaction 연결
            await chatRepo.SetTransactionId(roomId, createdTransaction.Id);

            // 4. 시스템 메시지 전송
            var paymentMessage = $"거래가 요청되었습니다. 판매자에게 직접 송금해주세요. (수량: {quantity}장, 총 금액: {totalAmount:N0}원)";
            var systemMessage = await chatRepo.CreateMessage(roomId, userId, paymentMessage, null, Enum.MessageType.TRANSACTION_REQUEST);

            // LastMessageAt 업데이트
            await chatRepo.UpdateLastMessageAt(roomId, systemMessage.CreatedAt ?? DateTime.UtcNow);

            // 상대방 읽지 않은 수 증가
            var isSenderBuyer = room.BuyerId == userId;
            await chatRepo.IncrementUnreadCount(roomId, !isSenderBuyer);

            var receiverId = isSenderBuyer ? room.SellerId : room.BuyerId;
            await notificationService.CreateAndSendAsync(
                receiverId,
                "TRANSACTION_REQUEST",
                "거래 요청이 도착했습니다",
                "판매자가 거래를 요청했습니다.",
                new Dictionary<string, string>
                {
                    ["type"] = "TRANSACTION_REQUEST",
                    ["transactionId"] = createdTransaction.Id.ToString(),
                    ["roomId"] = roomId.ToString()
                });

            logger.LogInformation("[ChatService.RequestPayment] RoomId={RoomId}, TransactionId={TransactionId}, UserId={UserId}, Quantity={Quantity}",
                roomId, createdTransaction.Id, userId, quantity);

            return new TransactionCreatedRespDto
            {
                TransactionId = (int)createdTransaction.Id,
                Amount = totalAmount
            };
        }
        catch (Exception ex)
        {
            await ticketRepo.ReleaseTicketQuantityAsync((int)room.TicketId, quantity);
            logger.LogError(ex, "[ChatService.RequestPayment] 결제 요청 실패 - 재고 복구 완료 (RoomId={RoomId}, TicketId={TicketId}, Quantity={Quantity})",
                roomId, room.TicketId, quantity);
            throw;
        }
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

        var confirmedStatus = await transactionRepo.GetTransactionStatusByCodeAsync("confirmed");
        if (confirmedStatus == null)
        {
            throw new AppException("거래 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        await transactionRepo.UpdateTransactionStatusAsync(req.TransactionId, confirmedStatus.Id);

        // 채팅방 잠금
        await chatRepo.LockChatRoom(req.RoomId);

        // PURCHASE_CONFIRMED 메시지 생성
        var message = await chatRepo.CreateMessage(
            roomId: req.RoomId,
            senderId: req.UserId,
            message: null,
            imageUrl: null,
            type: MessageType.PURCHASE_CONFIRMED
        );

        // ChatRoom 업데이트
        await chatRepo.UpdateLastMessageAt(req.RoomId, message.CreatedAt ?? DateTime.UtcNow);
        await chatRepo.IncrementUnreadCount(req.RoomId, false);

        await notificationService.CreateAndSendAsync(
            room.SellerId,
            "PURCHASE_CONFIRMED",
            "구매가 확정되었습니다",
            "구매자가 거래를 확정했습니다.",
            new Dictionary<string, string>
            {
                ["type"] = "PURCHASE_CONFIRMED",
                ["transactionId"] = req.TransactionId.ToString(),
                ["roomId"] = req.RoomId.ToString()
            });

        var sellerNickname = room.Seller?.UserProfile?.Nickname ?? "판매자";
        await notificationService.CreateAndSendAsync(
            room.BuyerId,
            "REVIEW_REQUEST",
            "거래는 어떠셨나요?",
            $"{sellerNickname} 판매자에 대한 리뷰를 남겨주세요.",
            new Dictionary<string, string>
            {
                ["type"] = "REVIEW_REQUEST",
                ["transactionId"] = req.TransactionId.ToString(),
                ["roomId"] = req.RoomId.ToString()
            });

        // SignalR 실시간 브로드캐스트
        var signalDto = new NewMessageSignalDto
        {
            MessageId = message.Id,
            RoomId = req.RoomId,
            SenderId = req.UserId,
            SenderNickname = room.Buyer?.UserProfile?.Nickname ?? "구매자",
            Message = null,
            Type = MessageType.PURCHASE_CONFIRMED.ToString(),
            CreatedAt = message.CreatedAt ?? DateTime.UtcNow
        };

        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("ReceiveMessage", signalDto);

        await hubContext.Clients.Group($"user_{room.BuyerId}")
            .SendAsync("NewMessage", signalDto);
        await hubContext.Clients.Group($"user_{room.SellerId}")
            .SendAsync("NewMessage", signalDto);

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

        var transaction = await transactionRepo.GetTransactionWithDetailsAsync(req.TransactionId);
        if (transaction == null)
        {
            throw new AppException("거래를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        foreach (var item in transaction.TransactionItems)
        {
            await ticketRepo.ReleaseTicketQuantityAsync((int)item.TicketId, item.Quantity);
        }

        var cancelledStatus = await transactionRepo.GetTransactionStatusByCodeAsync("cancelled");
        if (cancelledStatus == null)
        {
            throw new AppException("거래 상태 코드를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        await transactionRepo.UpdateTransactionStatusAsync(req.TransactionId, cancelledStatus.Id);
        await transactionRepo.UpdateTransactionCancelledAtAsync(req.TransactionId, DateTime.UtcNow);
        await chatRepo.ClearTransactionId(req.RoomId);

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
    /// 채팅방 나가기
    /// </summary>
    public async Task LeaveChatRoom(LeaveChatRoomReqDto req)
    {
        await ValidateUserInRoom(req.RoomId, req.UserId);

        var room = await chatRepo.GetChatRoomById(req.RoomId);
        if (room == null)
        {
            throw new AppException("채팅방을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (room.ClosedAt != null || string.Equals(room.Status?.Code, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var isBuyer = room.BuyerId == req.UserId;
        var leaveMessage = isBuyer
            ? "구매자가 채팅방을 나갔습니다."
            : "판매자가 채팅방을 나갔습니다.";

        var message = await chatRepo.CreateMessage(req.RoomId, req.UserId, leaveMessage, null, MessageType.TEXT);

        await chatRepo.UpdateLastMessageAt(req.RoomId, message.CreatedAt ?? DateTime.UtcNow);
        await chatRepo.IncrementUnreadCount(req.RoomId, !isBuyer);

        var closedStatusId = await chatRepo.GetStatusIdByCode("closed");
        await chatRepo.UpdateChatRoomStatus(req.RoomId, closedStatusId);
        await chatRepo.CloseChatRoom(req.RoomId);

        var (senderNickname, senderProfileImage) = await GetSenderInfoForSignalR(message.Id);

        var signalDto = new NewMessageSignalDto
        {
            MessageId = message.Id,
            RoomId = req.RoomId,
            SenderId = req.UserId,
            SenderNickname = senderNickname,
            SenderProfileImage = senderProfileImage,
            Message = leaveMessage,
            Type = MessageType.TEXT.ToString(),
            CreatedAt = message.CreatedAt ?? DateTime.UtcNow
        };

        await hubContext.Clients.Group($"room_{req.RoomId}")
            .SendAsync("ReceiveMessage", signalDto);

        var receiverId = isBuyer ? room.SellerId : room.BuyerId;
        await hubContext.Clients.Group($"user_{receiverId}")
            .SendAsync("ReceiveMessage", signalDto);

        logger.LogInformation("[ChatService.LeaveChatRoom] RoomId={RoomId}, UserId={UserId}", req.RoomId, req.UserId);
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

        // 1. 이미지 Object Key 수집
        var messageImageKeys = messageList
            .SelectMany(m => 
            {
                var keys = m.Images.Select(i => i.ImageUrl).ToList();
                // 하위 호환성: Images가 비어있고 ImageUrl이 있으면 추가
                if (keys.Count == 0 && !string.IsNullOrEmpty(m.ImageUrl))
                {
                    keys.Add(m.ImageUrl);
                }
                return keys;
            })
            .Distinct()
            .ToList();

        // 2. 프로필 이미지 Object Key 수집
        var profileImageKeys = messageList
            .Where(m => m.Sender?.UserProfile?.ProfileImageUrl != null)
            .Select(m => m.Sender!.UserProfile!.ProfileImageUrl!)
            .Where(url => !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                          !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();

        // 3. 모든 Object Key를 하나로 합침
        var allKeys = messageImageKeys.Concat(profileImageKeys).Distinct().ToList();

        // 4. 배치로 signed URL 발급
        var signedUrls = allKeys.Count > 0
            ? await fileUploadService.RefreshSignedUrlsBatchAsync(allKeys)
            : new Dictionary<string, SignedUrlResult>();

        // 5. 매핑
        return messageList.Select(m =>
        {
            // 메시지 이미지 처리
            var imagesInfo = new List<ImageInfo>();
            
            // 신규 다중 이미지 처리
            if (m.Images != null && m.Images.Count > 0)
            {
                foreach (var img in m.Images.OrderBy(i => i.SortOrder))
                {
                    if (signedUrls.TryGetValue(img.ImageUrl, out var result))
                    {
                        imagesInfo.Add(new ImageInfo { Url = result.SignedUrl, ExpiresAt = result.ExpiresAt });
                    }
                }
            }
            // 하위 호환성: Images가 없을 때 기존 ImageUrl 사용
            else if (!string.IsNullOrWhiteSpace(m.ImageUrl))
            {
                if (signedUrls.TryGetValue(m.ImageUrl, out var result))
                {
                    imagesInfo.Add(new ImageInfo { Url = result.SignedUrl, ExpiresAt = result.ExpiresAt });
                }
            }

            // 프로필 이미지 처리
            var profileImageUrl = m.Sender?.UserProfile?.ProfileImageUrl;
            string? finalProfileImageUrl = profileImageUrl;

            if (!string.IsNullOrEmpty(profileImageUrl) &&
                !profileImageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !profileImageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (signedUrls.TryGetValue(profileImageUrl, out var profileResult))
                {
                    finalProfileImageUrl = profileResult.SignedUrl;
                }
            }

            return new ChatMessageRespDto
            {
                MessageId = m.Id,
                RoomId = m.RoomId,
                SenderId = (int)m.SenderId,
                SenderNickname = m.Sender?.UserProfile?.Nickname ?? "Unknown",
                SenderProfileImage = finalProfileImageUrl,
                Message = m.Message,
                Type = m.Type.ToString(),
                Images = imagesInfo.Count > 0 ? imagesInfo : null,
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow,
                IsMyMessage = m.SenderId == userId
            };
        }).ToList();
    }

    private async Task<ChatRoomDetailRespDto> MapToRoomDetailDto(DBModel.ChatRoom room, int userId, List<ChatMessageRespDto> messages)
    {
        var isBuyer = room.BuyerId == userId;
        var transaction = room.Transaction;
        var hasReviewedSeller = false;
        var canWriteReview = false;

        // 프로필 이미지 및 이벤트 포스터 배치 처리
        var imageKeys = new List<string>();

        var buyerProfileKey = room.Buyer?.UserProfile?.ProfileImageUrl;
        var sellerProfileKey = room.Seller?.UserProfile?.ProfileImageUrl;
        var eventPosterKey = room.Ticket?.Event?.PosterImageUrl;

        if (!string.IsNullOrEmpty(buyerProfileKey) && !buyerProfileKey.StartsWith("http"))
            imageKeys.Add(buyerProfileKey);

        if (!string.IsNullOrEmpty(sellerProfileKey) && !sellerProfileKey.StartsWith("http"))
            imageKeys.Add(sellerProfileKey);

        if (!string.IsNullOrEmpty(eventPosterKey) && !eventPosterKey.StartsWith("http"))
            imageKeys.Add(eventPosterKey);

        var signedUrls = imageKeys.Count > 0
            ? await fileUploadService.RefreshSignedUrlsBatchAsync(imageKeys)
            : new Dictionary<string, SignedUrlResult>();

        string? buyerProfileUrl = buyerProfileKey;
        if (!string.IsNullOrEmpty(buyerProfileKey) && signedUrls.TryGetValue(buyerProfileKey, out var buyerResult))
        {
            buyerProfileUrl = buyerResult.SignedUrl;
        }

        string? sellerProfileUrl = sellerProfileKey;
        if (!string.IsNullOrEmpty(sellerProfileKey) && signedUrls.TryGetValue(sellerProfileKey, out var sellerResult))
        {
            sellerProfileUrl = sellerResult.SignedUrl;
        }

        // 이벤트 포스터 URL 처리
        string? eventPosterUrl = eventPosterKey;
        if (!string.IsNullOrEmpty(eventPosterKey) && signedUrls.TryGetValue(eventPosterKey, out var posterResult))
        {
            eventPosterUrl = posterResult.SignedUrl;
        }

        if (isBuyer && transaction != null)
        {
            hasReviewedSeller = await reputationRepository.ExistsAsync(transaction.Id, userId);

            var within7Days = transaction.ConfirmedAt != null
                              && (DateTime.UtcNow - transaction.ConfirmedAt.Value).TotalDays <= 7;

            canWriteReview = transaction.CancelledAt == null
                             && transaction.ConfirmedAt != null
                             && !hasReviewedSeller
                             && within7Days;
        }

        return new ChatRoomDetailRespDto
        {
            RoomId = room.Id,
            Ticket = new TicketInfo
            {
                TicketId = (int)room.TicketId,
                Title = room.Ticket?.Event?.Title ?? "",
                Price = room.Ticket?.Price ?? 0,
                UnitPrice = room.Ticket?.SeatGrade?.OriginalPrice ?? room.Ticket?.Price ?? 0,
                TotalQuantity = room.Ticket?.Quantity ?? 0,
                RemainingQuantity = room.Ticket?.RemainingQuantity ?? 0,
                ThumbnailUrl = eventPosterUrl,
                SeatInfo = BuildSeatInfo(room.Ticket),
                EventDateTime = room.Ticket?.EventDatetime,
                VenueName = room.Ticket?.Event?.VenueName
            },
            Buyer = new UserInfo
            {
                UserId = (int)room.BuyerId,
                Nickname = room.Buyer?.UserProfile?.Nickname ?? "Unknown",
                ProfileImageUrl = buyerProfileUrl,
                MannerTemperature = room.Buyer?.UserProfile?.MannerTemperature ?? 36.5
            },
            Seller = new UserInfo
            {
                UserId = (int)room.SellerId,
                Nickname = room.Seller?.UserProfile?.Nickname ?? "Unknown",
                ProfileImageUrl = sellerProfileUrl,
                MannerTemperature = room.Seller?.UserProfile?.MannerTemperature ?? 36.5
            },
            StatusCode = room.Status?.Code ?? "",
            StatusName = room.Status?.NameKo ?? "",
            Transaction = transaction != null ? new TransactionInfo
            {
                TransactionId = transaction.Id,
                StatusCode = transaction.Status?.Code ?? "",
                StatusName = transaction.Status?.NameKo ?? "",
                Amount = transaction.Amount,
                ConfirmedAt = transaction.ConfirmedAt,
                CancelledAt = transaction.CancelledAt
            } : null,
            CanSendMessage = room.LockedAt == null && room.ClosedAt == null,
            CanRequestPayment = !isBuyer && room.LockedAt == null && room.ClosedAt == null,
            CanConfirmPurchase = isBuyer && transaction != null && transaction.ConfirmedAt == null,
            CanCancelTransaction = transaction != null && transaction.ConfirmedAt == null && transaction.CancelledAt == null,
            CanWriteReview = canWriteReview,
            HasReviewedSeller = hasReviewedSeller,
            Messages = messages
        };
    }

    /// <summary>
    /// SignalR용 발신자 정보 조회 (닉네임, 프로필 이미지 Signed URL)
    /// </summary>
    public async Task<(string Nickname, string? ProfileImageUrl)> GetSenderInfoForSignalR(long messageId)
    {
        var message = await chatRepo.GetMessageById(messageId);
        if (message?.Sender?.UserProfile == null)
        {
            logger.LogWarning("[ChatService.GetSenderInfoForSignalR] Sender info not found for messageId: {MessageId}", messageId);
            return ("Unknown", null);
        }

        var nickname = message.Sender.UserProfile.Nickname ?? "Unknown";
        var profileImageKey = message.Sender.UserProfile.ProfileImageUrl;

        // Signed URL 변환
        string? profileImageUrl = profileImageKey;
        if (!string.IsNullOrEmpty(profileImageKey) &&
            !profileImageKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !profileImageKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var signedUrlResult = await fileUploadService.RefreshSignedUrlAsync(profileImageKey);
            profileImageUrl = signedUrlResult.SignedUrl;
        }

        return (nickname, profileImageUrl);
    }

    /// <summary>
    /// 티켓의 좌석 정보를 조합하여 문자열로 반환
    /// 예: "1층 VIP A구역 3열"
    /// </summary>
    private static string? BuildSeatInfo(DBModel.Ticket? ticket)
    {
        if (ticket == null) return null;

        var parts = new List<string>();

        // 좌석 위치 (1층, 2층, 플로어석 등)
        if (!string.IsNullOrEmpty(ticket.SeatLocation?.LocationName))
            parts.Add(ticket.SeatLocation.LocationName);

        // 좌석 등급 (VIP, R석 등)
        if (!string.IsNullOrEmpty(ticket.SeatGrade?.NameKo))
            parts.Add(ticket.SeatGrade.NameKo);

        // 구역 (A구역, B구역 등)
        if (!string.IsNullOrEmpty(ticket.Area?.AreaName))
            parts.Add(ticket.Area.AreaName);

        // 열 (3열, 5열 등) - "열" 이미 포함된 경우 중복 방지
        if (!string.IsNullOrEmpty(ticket.Row))
        {
            var rowValue = ticket.Row.Trim();
            parts.Add(rowValue.EndsWith("열") ? rowValue : $"{rowValue}열");
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }

    private async Task<string> GetTicketImageUrlForNotificationAsync(long ticketId)
    {
        try
        {
            var images = await sellRepository.GetTicketImagesByTicketIdAsync(ticketId);
            var objectKey = images.FirstOrDefault()?.ImageUrl;

            if (string.IsNullOrWhiteSpace(objectKey))
            {
                return string.Empty;
            }

            if (objectKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                objectKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return objectKey;
            }

            var signedUrl = await fileUploadService.RefreshSignedUrlAsync(objectKey);
            return signedUrl.SignedUrl;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[ChatService.GetTicketImageUrlForNotificationAsync] 티켓 이미지 URL 생성 실패: TicketId={TicketId}", ticketId);
            return string.Empty;
        }
    }
}
