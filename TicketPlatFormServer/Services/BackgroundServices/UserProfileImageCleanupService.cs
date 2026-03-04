using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Services.Storage;

namespace TicketPlatFormServer.Services.BackgroundServices;

/// <summary>
/// 고아 프로필 이미지 자동 정리 백그라운드 서비스.
/// DB에 참조되지 않는 Supabase 스토리지 파일을 24시간마다 정리한다.
/// </summary>
public class UserProfileImageCleanupService(
    IServiceProvider serviceProvider,
    ILogger<UserProfileImageCleanupService> logger,
    SupabaseStorageSettings storageSettings) : BackgroundService
{
    /// <summary>
    /// 백그라운드 서비스 실행
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[UserProfileImageCleanupService] 서비스 시작. 실행 주기: 24시간");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOrphanProfileImages(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[UserProfileImageCleanupService] 정리 중 오류 발생");
            }

            logger.LogInformation("[UserProfileImageCleanupService] 다음 실행까지 24시간 대기");
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    /// <summary>
    /// 고아 프로필 이미지 정리.
    /// Supabase에 존재하지만 DB에 참조되지 않는 파일 중 생성 후 1시간이 초과된 것을 삭제한다.
    /// </summary>
    private async Task CleanupOrphanProfileImages(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var storageUploader = scope.ServiceProvider.GetRequiredService<IStorageUploader>();
        var context = scope.ServiceProvider.GetRequiredService<TicketContext>();

        logger.LogInformation("[UserProfileImageCleanupService] 고아 파일 정리 시작");
        var startTime = DateTime.UtcNow;

        // 1. Supabase 파일 목록 조회
        var storageObjects = await storageUploader.ListObjectsAsync(
            "",
            bucketName: storageSettings.BucketNames.ProfileImage,
            ct: ct);

        // 2. DB 참조 목록 조회 (object key로 저장됨)
        var dbImageKeys = await context.UserProfiles
            .Where(p => p.ProfileImageUrl != null)
            .Select(p => p.ProfileImageUrl!)
            .ToListAsync(ct);
        var dbImageKeySet = new HashSet<string>(dbImageKeys, StringComparer.OrdinalIgnoreCase);

        // 3. 고아 파일 필터링: Supabase에 있지만 DB에 없는 파일
        //    안전 임계값: 업로드 후 1시간 이내 파일은 삭제 대상에서 제외
        var safetyThreshold = DateTime.UtcNow.AddHours(-1);
        var orphans = storageObjects
            .Where(o => !dbImageKeySet.Contains(o.Name) && o.CreatedAt < safetyThreshold)
            .ToList();

        if (orphans.Count == 0)
        {
            logger.LogInformation("[UserProfileImageCleanupService] 정리할 고아 파일 없음");
            return;
        }

        logger.LogInformation("[UserProfileImageCleanupService] {Count}개 고아 파일 발견", orphans.Count);

        var successCount = 0;
        var failCount = 0;

        // 4. 순차 처리 (Task.WhenAll 금지 — 커넥션 충돌 방지)
        foreach (var orphan in orphans)
        {
            try
            {
                await storageUploader.DeleteAsync(
                    orphan.Name,
                    bucketName: storageSettings.BucketNames.ProfileImage,
                    ct: ct);
                successCount++;
                logger.LogInformation("[UserProfileImageCleanupService] 삭제 완료: {Name}", orphan.Name);
            }
            catch (Exception ex)
            {
                failCount++;
                logger.LogError(ex, "[UserProfileImageCleanupService] 삭제 실패: {Name}", orphan.Name);
            }
        }

        var elapsed = DateTime.UtcNow - startTime;
        logger.LogInformation(
            "[UserProfileImageCleanupService] 정리 완료 - 성공: {Success}, 실패: {Fail}, 소요: {Elapsed}초",
            successCount, failCount, elapsed.TotalSeconds);
    }

    /// <summary>
    /// 서비스 중지 시
    /// </summary>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[UserProfileImageCleanupService] 서비스 중지");
        return base.StopAsync(cancellationToken);
    }
}
