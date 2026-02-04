using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Chat;

public interface IChatRepository
{
    // Chat Room Operations
    Task<ChatRoom?> GetChatRoomById(long roomId);
    Task<ChatRoom?> GetChatRoomByTransactionId(long transactionId);
    Task<ChatRoom?> GetChatRoomByTicketAndBuyer(int ticketId, int buyerId);
    Task<ChatRoom?> GetChatRoomByTicketAndUser(int ticketId, int userId);
    Task<List<ChatRoom>> GetChatRoomsByUserId(int userId, int page, int pageSize);
    Task<ChatRoom> CreateChatRoom(int ticketId, int buyerId, int sellerId, long statusId);
    Task UpdateChatRoomStatus(long roomId, long statusId);
    Task UpdateLastMessageAt(long roomId, DateTime lastMessageAt);
    Task IncrementUnreadCount(long roomId, bool isBuyer);
    Task ResetUnreadCount(long roomId, bool isBuyer);
    Task LockChatRoom(long roomId);
    Task CloseChatRoom(long roomId);
    Task<bool> IsUserInChatRoom(long roomId, int userId);
    Task SetTransactionId(long roomId, long transactionId);
    Task ClearTransactionId(long roomId);

    // Message Operations
    Task<ChatMessage> CreateMessage(long roomId, int senderId, string? message, string? imageUrl, Enum.MessageType type = Enum.MessageType.TEXT);
    Task<ChatMessage> CreateMessageWithImages(long roomId, int senderId, string? message, List<string> imageObjectKeys, Enum.MessageType type = Enum.MessageType.IMAGE);
    Task<ChatMessage?> GetMessageById(long messageId);
    Task<List<ChatMessage>> GetMessagesByRoomId(long roomId, long? lastMessageId, int limit);
    Task<Dictionary<long, string?>> GetLastMessagesForRooms(IEnumerable<long> roomIds);
    Task<int> GetUnreadCount(long roomId, int userId);
    Task DeleteMessage(long messageId);

    // Cleanup Operations
    Task<List<long>> GetExpiredChatRooms(int retentionDays);
    Task SoftDeleteChatRoom(long roomId);
    Task<int> DeleteMessagesForRoom(long roomId);

    // Status Operations
    Task<long> GetStatusIdByCode(string code);
}
