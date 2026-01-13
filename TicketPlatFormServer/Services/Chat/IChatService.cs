using TicketPlatFormServer.DTO.Chat;

namespace TicketPlatFormServer.Services.Chat;

public interface IChatService
{
    // Room Management
    Task<ChatRoomDetailRespDto> GetOrCreateChatRoom(int ticketId, int userId);
    Task<List<ChatRoomListRespDto>> GetChatRooms(int userId, int page, int pageSize);
    Task<ChatRoomDetailRespDto> GetChatRoomDetail(long roomId, int userId);

    // Messaging
    Task<SendMessageRespDto> SendMessage(SendMessageReqDto req);
    Task<List<ChatMessageRespDto>> GetMessages(GetMessagesReqDto req);
    Task MarkMessagesAsRead(long roomId, int userId);

    // Transaction Actions
    Task<PaymentUrlRespDto> RequestPayment(long roomId, long transactionId, int userId);
    Task<PurchaseConfirmRespDto> ConfirmPurchase(ConfirmPurchaseReqDto req);
    Task CancelTransaction(CancelTransactionReqDto req);

    // Image URL Refresh
    Task<RefreshImageUrlRespDto> RefreshImageUrl(long messageId, int userId);
}
