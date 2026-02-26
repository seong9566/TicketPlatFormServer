using System.Net;
using System.Text.Json;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Notification;
using TicketPlatFormServer.Repository.Notifications;

namespace TicketPlatFormServer.Services.Notification;

public class NotificationService(
    INotificationRepository notificationRepository,
    INotificationTokenRepository notificationTokenRepository,
    IFcmService fcmService,
    ILogger<NotificationService> logger) : INotificationService
{
    private const int DefaultLimit = 20;
    private const int MaxLimit = 50;

    public async Task<RegisterTokenRespDto> RegisterTokenAsync(long userId, RegisterTokenReqDto req)
    {
        await notificationTokenRepository.SeedPlatformsIfMissingAsync();

        var platformCode = req.Platform.Trim().ToUpperInvariant();
        var platform = await notificationTokenRepository.GetPlatformByCodeAsync(platformCode);
        if (platform == null)
        {
            throw new AppException("지원하지 않는 platform입니다", HttpStatusCode.BadRequest);
        }

        var token = req.DeviceToken.Trim();
        var saved = await notificationTokenRepository.UpsertAsync(userId, token, platform.Id);

        return new RegisterTokenRespDto
        {
            Id = saved.Id,
            DeviceToken = saved.DeviceToken,
            Platform = platform.Code
        };
    }

    public async Task DeleteTokenAsync(long userId, DeleteTokenReqDto req)
    {
        var deleted = await notificationTokenRepository.DeleteAsync(userId, req.DeviceToken.Trim());
        if (!deleted)
        {
            throw new AppException("해당 토큰이 존재하지 않습니다.", HttpStatusCode.NotFound);
        }
    }

    public async Task<NotificationListRespDto> GetNotificationsAsync(long userId, string? cursor, int? limit)
    {
        long? cursorId = null;
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (!long.TryParse(cursor, out var parsed))
            {
                throw new AppException("유효하지 않은 cursor 형식입니다.", HttpStatusCode.BadRequest);
            }
            cursorId = parsed;
        }

        var actualLimit = Math.Min(limit ?? DefaultLimit, MaxLimit);
        var list = await notificationRepository.GetByUserCursorAsync(userId, cursorId, actualLimit + 1);
        var hasMore = list.Count > actualLimit;
        if (hasMore)
        {
            list = list.Take(actualLimit).ToList();
        }

        var items = list.Select(x => new NotificationItemRespDto
        {
            Id = x.Id,
            TypeCode = x.Type.Code,
            TypeName = x.Type.NameKo ?? x.Type.Code,
            Title = x.Title,
            Body = x.Body,
            Data = x.Data,
            ReadFlag = x.ReadFlag ?? false,
            ReadAt = x.ReadAt,
            CreatedAt = x.CreatedAt ?? DateTime.UtcNow
        }).ToList();

        var nextCursor = hasMore && items.Count > 0 ? items[^1].Id.ToString() : null;

        return new NotificationListRespDto
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }

    public async Task MarkAsReadAsync(long userId, long notificationId)
    {
        var target = await notificationRepository.GetByIdAsync(notificationId);
        if (target == null)
        {
            throw new AppException("알림이 존재하지 않습니다.", HttpStatusCode.NotFound);
        }

        if (target.UserId != userId)
        {
            throw new AppException("본인의 알림이 아닙니다.", HttpStatusCode.Forbidden);
        }

        if (target.ReadFlag == true)
        {
            return;
        }

        await notificationRepository.MarkAsReadAsync(notificationId, DateTime.UtcNow);
    }

    public async Task<ReadAllRespDto> MarkAllAsReadAsync(long userId)
    {
        var updated = await notificationRepository.MarkAllAsReadAsync(userId, DateTime.UtcNow);
        return new ReadAllRespDto { UpdatedCount = updated };
    }

    public async Task<UnreadCountRespDto> GetUnreadCountAsync(long userId)
    {
        var unreadCount = await notificationRepository.GetUnreadCountAsync(userId);
        return new UnreadCountRespDto { UnreadCount = unreadCount };
    }

    public async Task CreateAndSendAsync(long userId, string typeCode, string title, string body, Dictionary<string, string>? data = null)
    {
        await notificationRepository.SeedTypesIfMissingAsync();

        var type = await notificationRepository.GetTypeByCodeAsync(typeCode);
        if (type == null)
        {
            throw new AppException($"알림 타입을 찾을 수 없습니다: {typeCode}", HttpStatusCode.InternalServerError);
        }

        var payload = data == null ? null : JsonSerializer.Serialize(data);
        await notificationRepository.CreateAsync(new DBModel.Notification
        {
            UserId = (int)userId,
            TypeId = type.Id,
            Title = title,
            Body = body,
            Data = payload,
            ReadFlag = false,
            ReadAt = null,
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await fcmService.SendToUserAsync(userId, title, body, data);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FCM 발송 실패. UserId={UserId}, TypeCode={TypeCode}", userId, typeCode);
        }
    }
}
