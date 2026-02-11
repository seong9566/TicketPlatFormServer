using TicketPlatFormServer.DTO.Notification;

namespace TicketPlatFormServer.Services.Notification;

public interface INotificationService
{
    Task<RegisterTokenRespDto> RegisterTokenAsync(long userId, RegisterTokenReqDto req);
    Task DeleteTokenAsync(long userId, DeleteTokenReqDto req);
    Task<NotificationListRespDto> GetNotificationsAsync(long userId, string? cursor, int? limit);
    Task MarkAsReadAsync(long userId, long notificationId);
    Task<ReadAllRespDto> MarkAllAsReadAsync(long userId);
    Task<UnreadCountRespDto> GetUnreadCountAsync(long userId);
    Task CreateAndSendAsync(long userId, string typeCode, string title, string body, Dictionary<string, string>? data = null);
}
