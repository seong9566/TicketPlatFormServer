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
        var existingCodes = await db.NotificationTypes.Select(x => x.Code).ToListAsync();
        var toInsert = RequiredTypes
            .Where(x => !existingCodes.Contains(x.Code))
            .Select(x => new DBModel.NotificationType
            {
                Id = x.Id,
                Code = x.Code,
                NameKo = x.NameKo,
                IsActive = true,
                SortOrder = x.SortOrder
            })
            .ToList();

        if (toInsert.Count == 0)
        {
            return;
        }

        await db.NotificationTypes.AddRangeAsync(toInsert);
        await db.SaveChangesAsync();
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
