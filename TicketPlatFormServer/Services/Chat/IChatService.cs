using TicketPlatFormServer.DTO.Chat;

namespace TicketPlatFormServer.Services.Chat;

public interface IChatService
{
    // Room Management
    Task<ChatRoomDetailRespDto> GetOrCreateChatRoom(int ticketId, int userId);
    Task<List<ChatRoomListRespDto>> GetChatRooms(int userId, int page, int pageSize);
    Task<ChatRoomDetailRespDto> GetChatRoomDetail(long roomId, int userId);
    Task<ChatRoomDetailRespDto?> GetChatRoomByTicket(int ticketId, int userId);
    Task<DBModel.ChatRoom?> GetChatRoomById(long roomId);

    // Messaging
    Task<SendMessageRespDto> SendMessage(SendMessageReqDto req);
    Task<List<ChatMessageRespDto>> GetMessages(GetMessagesReqDto req);
    Task MarkMessagesAsRead(long roomId, int userId);

    // Transaction Actions
    Task<TransactionCreatedRespDto> RequestPayment(long roomId, int userId, int quantity);
    Task<PurchaseConfirmRespDto> ConfirmPurchase(ConfirmPurchaseReqDto req);
    Task CancelTransaction(CancelTransactionReqDto req);
    Task LeaveChatRoom(LeaveChatRoomReqDto req);

    // Image URL Refresh
    Task<RefreshImageUrlRespDto> RefreshImageUrl(long messageId, int userId);

    // SignalR Support
    Task<(string Nickname, string? ProfileImageUrl)> GetSenderInfoForSignalR(long messageId);
}
