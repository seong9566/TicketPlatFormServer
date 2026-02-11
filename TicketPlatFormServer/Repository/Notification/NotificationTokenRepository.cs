using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;

namespace TicketPlatFormServer.Repository.Notifications;

public class NotificationTokenRepository(TicketContext db) : INotificationTokenRepository
{
    private static readonly (long Id, string Code, string NameKo, int SortOrder)[] RequiredPlatforms =
    [
        (1, "ANDROID", "안드로이드", 1),
        (2, "IOS", "iOS", 2)
    ];

    public async Task SeedPlatformsIfMissingAsync()
    {
        var existingCodes = await db.NotificationPlatforms.Select(x => x.Code).ToListAsync();
        var toInsert = RequiredPlatforms
            .Where(x => !existingCodes.Contains(x.Code))
            .Select(x => new NotificationPlatform
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

        await db.NotificationPlatforms.AddRangeAsync(toInsert);
        await db.SaveChangesAsync();
    }

    public async Task<NotificationPlatform?> GetPlatformByCodeAsync(string platformCode)
    {
        return await db.NotificationPlatforms.FirstOrDefaultAsync(x => x.Code == platformCode && x.IsActive == true);
    }

    public async Task<NotificationToken> UpsertAsync(long userId, string deviceToken, long platformId)
    {
        var existing = await db.NotificationTokens.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
        if (existing == null)
        {
            var entity = new NotificationToken
            {
                UserId = userId,
                DeviceToken = deviceToken,
                PlatformId = platformId,
                CreatedAt = DateTime.UtcNow
            };
            db.NotificationTokens.Add(entity);
            await db.SaveChangesAsync();
            await db.Entry(entity).Reference(x => x.Platform).LoadAsync();
            return entity;
        }

        existing.UserId = userId;
        existing.PlatformId = platformId;
        await db.SaveChangesAsync();
        await db.Entry(existing).Reference(x => x.Platform).LoadAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(long userId, string deviceToken)
    {
        var target = await db.NotificationTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceToken == deviceToken);
        if (target == null)
        {
            return false;
        }

        db.NotificationTokens.Remove(target);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<NotificationToken>> GetByUserIdAsync(long userId)
    {
        return await db.NotificationTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> DeleteByDeviceTokenAsync(string deviceToken)
    {
        var target = await db.NotificationTokens.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
        if (target == null)
        {
            return false;
        }

        db.NotificationTokens.Remove(target);
        await db.SaveChangesAsync();
        return true;
    }
}
