using Microsoft.EntityFrameworkCore;

namespace TicketPlatFormServer.Repository.Notifications;

public class NotificationRepository(TicketContext db) : INotificationRepository
{
    private static readonly (long Id, string Code, string NameKo, int SortOrder)[] RequiredTypes =
    [
        (1, "CHAT_MESSAGE", "채팅 메시지", 1),
        (2, "TRANSACTION_REQUEST", "거래 요청", 2),
        (4, "PURCHASE_CONFIRMED", "구매 확정", 4),
        (5, "DISPUTE_OPENED", "신고 접수", 5),
        (6, "DISPUTE_RESOLVED", "신고 해결", 6),
        (7, "REVIEW_REQUEST", "리뷰 요청", 7)
    ];

    public async Task<DBModel.NotificationType?> GetTypeByCodeAsync(string typeCode)
    {
        return await db.NotificationTypes.FirstOrDefaultAsync(x => x.Code == typeCode && x.IsActive == true);
    }

    public async Task SeedTypesIfMissingAsync()
    {
        foreach (var (id, code, nameKo, sortOrder) in RequiredTypes)
        {
            await db.Database.ExecuteSqlRawAsync(
                @"INSERT INTO notification_types (id, code, name_ko, is_active, sort_order)
                  VALUES ({0}, {1}, {2}, 1, {3})
                  ON DUPLICATE KEY UPDATE code = VALUES(code), name_ko = VALUES(name_ko), sort_order = VALUES(sort_order)",
                id, code, nameKo, sortOrder);
        }
    }

    public async Task<DBModel.Notification> CreateAsync(DBModel.Notification notification)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
        await db.Entry(notification).Reference(x => x.Type).LoadAsync();
        return notification;
    }

    public async Task<List<DBModel.Notification>> GetByUserCursorAsync(long userId, long? cursorId, int limitPlusOne)
    {
        var query = db.Notifications
            .AsNoTracking()
            .Include(x => x.Type)
            .Where(x => x.UserId == userId);

        if (cursorId.HasValue)
        {
            query = query.Where(x => x.Id < cursorId.Value);
        }

        return await query
            .OrderByDescending(x => x.Id)
            .Take(limitPlusOne)
            .ToListAsync();
    }

    public async Task<DBModel.Notification?> GetByIdAsync(long id)
    {
        return await db.Notifications
            .Include(x => x.Type)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task MarkAsReadAsync(long id, DateTime readAtUtc)
    {
        await db.Notifications
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReadFlag, true)
                .SetProperty(x => x.ReadAt, readAtUtc));
    }

    public async Task<int> MarkAllAsReadAsync(long userId, DateTime readAtUtc)
    {
        return await db.Notifications
            .Where(x => x.UserId == userId && x.ReadFlag != true)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReadFlag, true)
                .SetProperty(x => x.ReadAt, readAtUtc));
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await db.Notifications.CountAsync(x => x.UserId == userId && x.ReadFlag != true);
    }
}
