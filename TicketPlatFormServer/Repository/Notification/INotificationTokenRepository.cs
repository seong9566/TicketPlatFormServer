using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Notifications;

public interface INotificationTokenRepository
{
    Task SeedPlatformsIfMissingAsync();
    Task<NotificationPlatform?> GetPlatformByCodeAsync(string platformCode);
    Task<NotificationToken> UpsertAsync(long userId, string deviceToken, long platformId);
    Task<bool> DeleteAsync(long userId, string deviceToken);
    Task<List<NotificationToken>> GetByUserIdAsync(long userId);
    Task<bool> DeleteByDeviceTokenAsync(string deviceToken);
}
