using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Chat;

public interface IChatRepository
{
    // Chat Room Operations
    Task<ChatRoom?> GetChatRoomById(long roomId);
    Task<ChatRoom?> GetChatRoomByTicketAndBuyer(long ticketId, long buyerId);
    Task<List<ChatRoom>> GetChatRoomsByUserId(long userId, int page, int pageSize);
    Task<ChatRoom> CreateChatRoom(long ticketId, long buyerId, long sellerId, long statusId);
    Task UpdateChatRoomStatus(long roomId, long statusId);
    Task UpdateLastMessageAt(long roomId, DateTime lastMessageAt);
    Task IncrementUnreadCount(long roomId, bool isBuyer);
    Task ResetUnreadCount(long roomId, bool isBuyer);
    Task LockChatRoom(long roomId);
    Task CloseChatRoom(long roomId);
    Task<bool> IsUserInChatRoom(long roomId, long userId);
    Task SetTransactionId(long roomId, long transactionId);

    // Message Operations
    Task<ChatMessage> CreateMessage(long roomId, long senderId, string? message, string? imageUrl);
    Task<List<ChatMessage>> GetMessagesByRoomId(long roomId, long? lastMessageId, int limit);
    Task<int> GetUnreadCount(long roomId, long userId);

    // Cleanup Operations
    Task<List<long>> GetExpiredChatRooms(int retentionDays);
    Task SoftDeleteChatRoom(long roomId);
    Task<int> DeleteMessagesForRoom(long roomId);

    // Status Operations
    Task<long> GetStatusIdByCode(string code);
}
