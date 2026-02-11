using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Notifications;

public interface INotificationRepository
{
    Task<NotificationType?> GetTypeByCodeAsync(string typeCode);
    Task SeedTypesIfMissingAsync();
    Task<DBModel.Notification> CreateAsync(DBModel.Notification notification);
    Task<List<DBModel.Notification>> GetByUserCursorAsync(long userId, long? cursorId, int limitPlusOne);
    Task<DBModel.Notification?> GetByIdAsync(long id);
    Task MarkAsReadAsync(long id, DateTime readAtUtc);
    Task<int> MarkAllAsReadAsync(long userId, DateTime readAtUtc);
    Task<int> GetUnreadCountAsync(long userId);
}
