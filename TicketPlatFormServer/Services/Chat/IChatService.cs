using TicketPlatFormServer.DTO.Chat;

namespace TicketPlatFormServer.Services.Chat;

public interface IChatService
{
    // Room Management
    Task<ChatRoomDetailRespDto> GetOrCreateChatRoom(long ticketId, long userId);
    Task<List<ChatRoomListRespDto>> GetChatRooms(long userId, int page, int pageSize);
    Task<ChatRoomDetailRespDto> GetChatRoomDetail(long roomId, long userId);

    // Messaging
    Task<SendMessageRespDto> SendMessage(SendMessageReqDto req);
    Task<List<ChatMessageRespDto>> GetMessages(GetMessagesReqDto req);
    Task MarkMessagesAsRead(long roomId, long userId);

    // Transaction Actions
    Task<PaymentUrlRespDto> RequestPayment(long roomId, long transactionId, long userId);
    Task<PurchaseConfirmRespDto> ConfirmPurchase(ConfirmPurchaseReqDto req);
    Task CancelTransaction(CancelTransactionReqDto req);
}
