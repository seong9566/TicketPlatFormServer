using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Chat;

/// <summary>
/// 채팅 Repository 구현체 (Primary Constructor 사용)
/// </summary>
public class ChatRepository(TicketContext db, IDbConnection dapper, ILogger<ChatRepository> logger) : IChatRepository
{
    /// <summary>
    /// 채팅방 ID로 조회
    /// </summary>
    public async Task<ChatRoom?> GetChatRoomById(long roomId)
    {
        return await db.ChatRooms
            .Include(cr => cr.Ticket)
            .Include(cr => cr.Buyer).ThenInclude(u => u.UserProfile)
            .Include(cr => cr.Seller).ThenInclude(u => u.UserProfile)
            .Include(cr => cr.Status)
            .Include(cr => cr.Transaction).ThenInclude(t => t.Status)
            .FirstOrDefaultAsync(cr => cr.Id == roomId && cr.DeletedAt == null);
    }

    /// <summary>
    /// 티켓과 구매자로 채팅방 조회
    /// </summary>
    public async Task<ChatRoom?> GetChatRoomByTicketAndBuyer(long ticketId, long buyerId)
    {
        return await db.ChatRooms
            .Include(cr => cr.Ticket)
            .Include(cr => cr.Buyer).ThenInclude(u => u.UserProfile)
            .Include(cr => cr.Seller).ThenInclude(u => u.UserProfile)
            .Include(cr => cr.Status)
            .Include(cr => cr.Transaction).ThenInclude(t => t.Status)
            .FirstOrDefaultAsync(cr => cr.TicketId == ticketId && cr.BuyerId == buyerId && cr.DeletedAt == null);
    }

    /// <summary>
    /// 사용자의 채팅방 목록 조회 (Dapper 사용)
    /// </summary>
    public async Task<List<ChatRoom>> GetChatRoomsByUserId(long userId, int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;

        var rooms = await db.ChatRooms
            .Include(cr => cr.Ticket)
            .Include(cr => cr.Buyer).ThenInclude(u => u.UserProfile)
            .Include(cr => cr.Seller).ThenInclude(u => u.UserProfile)
            .Include(cr => cr.Status)
            .Include(cr => cr.Transaction).ThenInclude(t => t.Status)
            .Where(cr => (cr.BuyerId == userId || cr.SellerId == userId) && cr.DeletedAt == null)
            .OrderByDescending(cr => cr.LastMessageAt)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync();

        logger.LogInformation("[ChatRepository.GetChatRoomsByUserId] UserId: {UserId}, Count: {Count}", userId, rooms.Count);
        return rooms;
    }

    /// <summary>
    /// 채팅방 생성
    /// </summary>
    public async Task<ChatRoom> CreateChatRoom(long ticketId, long buyerId, long sellerId, long statusId)
    {
        var chatRoom = new ChatRoom
        {
            TicketId = ticketId,
            BuyerId = buyerId,
            SellerId = sellerId,
            StatusId = statusId,
            LastMessageAt = DateTime.UtcNow,
            UnreadCountBuyer = 0,
            UnreadCountSeller = 0,
            CreatedAt = DateTime.UtcNow
        };

        db.ChatRooms.Add(chatRoom);
        await db.SaveChangesAsync();

        logger.LogInformation("[ChatRepository.CreateChatRoom] RoomId: {RoomId}, TicketId: {TicketId}, BuyerId: {BuyerId}, SellerId: {SellerId}",
            chatRoom.Id, ticketId, buyerId, sellerId);

        return await GetChatRoomById(chatRoom.Id) ?? chatRoom;
    }

    /// <summary>
    /// 채팅방 상태 변경
    /// </summary>
    public async Task UpdateChatRoomStatus(long roomId, long statusId)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            chatRoom.StatusId = statusId;
            await db.SaveChangesAsync();
            logger.LogInformation("[ChatRepository.UpdateChatRoomStatus] RoomId: {RoomId}, StatusId: {StatusId}", roomId, statusId);
        }
    }

    /// <summary>
    /// 마지막 메시지 시간 업데이트
    /// </summary>
    public async Task UpdateLastMessageAt(long roomId, DateTime lastMessageAt)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            chatRoom.LastMessageAt = lastMessageAt;
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 읽지 않은 메시지 수 증가
    /// </summary>
    public async Task IncrementUnreadCount(long roomId, bool isBuyer)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            if (isBuyer)
            {
                chatRoom.UnreadCountBuyer++;
            }
            else
            {
                chatRoom.UnreadCountSeller++;
            }
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 읽지 않은 메시지 수 초기화
    /// </summary>
    public async Task ResetUnreadCount(long roomId, bool isBuyer)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            if (isBuyer)
            {
                chatRoom.UnreadCountBuyer = 0;
            }
            else
            {
                chatRoom.UnreadCountSeller = 0;
            }
            await db.SaveChangesAsync();
            logger.LogInformation("[ChatRepository.ResetUnreadCount] RoomId: {RoomId}, IsBuyer: {IsBuyer}", roomId, isBuyer);
        }
    }

    /// <summary>
    /// 채팅방 잠금
    /// </summary>
    public async Task LockChatRoom(long roomId)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            chatRoom.LockedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            logger.LogInformation("[ChatRepository.LockChatRoom] RoomId: {RoomId}", roomId);
        }
    }

    /// <summary>
    /// 채팅방 종료
    /// </summary>
    public async Task CloseChatRoom(long roomId)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            chatRoom.ClosedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            logger.LogInformation("[ChatRepository.CloseChatRoom] RoomId: {RoomId}", roomId);
        }
    }

    /// <summary>
    /// 사용자가 채팅방에 속해 있는지 확인 (권한 체크)
    /// </summary>
    public async Task<bool> IsUserInChatRoom(long roomId, long userId)
    {
        return await db.ChatRooms
            .AnyAsync(cr => cr.Id == roomId &&
                           (cr.BuyerId == userId || cr.SellerId == userId) &&
                           cr.DeletedAt == null);
    }

    /// <summary>
    /// 채팅방에 거래 ID 설정
    /// </summary>
    public async Task SetTransactionId(long roomId, long transactionId)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            chatRoom.TransactionId = transactionId;
            await db.SaveChangesAsync();
            logger.LogInformation("[ChatRepository.SetTransactionId] RoomId: {RoomId}, TransactionId: {TransactionId}", roomId, transactionId);
        }
    }

    /// <summary>
    /// 메시지 생성
    /// </summary>
    public async Task<ChatMessage> CreateMessage(long roomId, long senderId, string? message, string? imageUrl)
    {
        var chatMessage = new ChatMessage
        {
            RoomId = roomId,
            SenderId = senderId,
            Message = message,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };

        db.ChatMessages.Add(chatMessage);
        await db.SaveChangesAsync();

        logger.LogInformation("[ChatRepository.CreateMessage] MessageId: {MessageId}, RoomId: {RoomId}, SenderId: {SenderId}",
            chatMessage.Id, roomId, senderId);

        return chatMessage;
    }

    /// <summary>
    /// 채팅방의 메시지 목록 조회
    /// </summary>
    public async Task<List<ChatMessage>> GetMessagesByRoomId(long roomId, long? lastMessageId, int limit)
    {
        var query = db.ChatMessages
            .Include(cm => cm.Sender).ThenInclude(u => u.UserProfile)
            .Where(cm => cm.RoomId == roomId);

        if (lastMessageId.HasValue)
        {
            query = query.Where(cm => cm.Id < lastMessageId.Value);
        }

        var messages = await query
            .OrderByDescending(cm => cm.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return messages;
    }

    /// <summary>
    /// 읽지 않은 메시지 수 조회
    /// </summary>
    public async Task<int> GetUnreadCount(long roomId, long userId)
    {
        var chatRoom = await db.ChatRooms
            .FirstOrDefaultAsync(cr => cr.Id == roomId && cr.DeletedAt == null);

        if (chatRoom == null) return 0;

        return chatRoom.BuyerId == userId ? (chatRoom.UnreadCountBuyer ?? 0) : (chatRoom.UnreadCountSeller ?? 0);
    }

    /// <summary>
    /// 만료된 채팅방 조회
    /// </summary>
    public async Task<List<long>> GetExpiredChatRooms(int retentionDays)
    {
        var expiryDate = DateTime.UtcNow.AddDays(-retentionDays);

        var expiredRooms = await db.ChatRooms
            .Where(cr => cr.DeletedAt == null &&
                        (cr.ClosedAt != null && cr.ClosedAt < expiryDate ||
                         cr.Transaction != null && cr.Transaction.ConfirmedAt != null && cr.Transaction.ConfirmedAt < expiryDate))
            .Select(cr => cr.Id)
            .ToListAsync();

        logger.LogInformation("[ChatRepository.GetExpiredChatRooms] Found {Count} expired rooms", expiredRooms.Count);
        return expiredRooms;
    }

    /// <summary>
    /// 채팅방 소프트 삭제
    /// </summary>
    public async Task SoftDeleteChatRoom(long roomId)
    {
        var chatRoom = await db.ChatRooms.FindAsync(roomId);
        if (chatRoom != null)
        {
            chatRoom.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            logger.LogInformation("[ChatRepository.SoftDeleteChatRoom] RoomId: {RoomId}", roomId);
        }
    }

    /// <summary>
    /// 채팅방의 모든 메시지 삭제
    /// </summary>
    public async Task<int> DeleteMessagesForRoom(long roomId)
    {
        var messages = await db.ChatMessages
            .Where(cm => cm.RoomId == roomId)
            .ToListAsync();

        db.ChatMessages.RemoveRange(messages);
        await db.SaveChangesAsync();

        logger.LogInformation("[ChatRepository.DeleteMessagesForRoom] RoomId: {RoomId}, Deleted: {Count}", roomId, messages.Count);
        return messages.Count;
    }

    /// <summary>
    /// 상태 코드로 상태 ID 조회
    /// </summary>
    public async Task<long> GetStatusIdByCode(string code)
    {
        var status = await db.ChatRoomStatuses
            .FirstOrDefaultAsync(s => s.Code == code);

        if (status == null)
        {
            logger.LogWarning("[ChatRepository.GetStatusIdByCode] Status not found: {Code}", code);
            throw new InvalidOperationException($"Chat room status '{code}' not found");
        }

        return status.Id;
    }
}
