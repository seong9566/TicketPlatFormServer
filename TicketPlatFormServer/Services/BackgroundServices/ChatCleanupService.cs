using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Repository.Chat;

namespace TicketPlatFormServer.Services.BackgroundServices;

/// <summary>
/// 만료된 채팅방 자동 정리 백그라운드 서비스
/// </summary>
public class ChatCleanupService(
    IServiceProvider serviceProvider,
    ILogger<ChatCleanupService> logger,
    ChatSettings chatSettings) : BackgroundService
{
    /// <summary>
    /// 백그라운드 서비스 실행
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[ChatCleanupService] 채팅 정리 서비스가 시작되었습니다. 실행 주기: {Hours}시간, 보관 기간: {Days}일",
            chatSettings.CleanupIntervalHours, chatSettings.MessageRetentionDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredChatRooms();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[ChatCleanupService] 채팅 정리 중 오류 발생");
            }

            // 다음 실행까지 대기
            logger.LogInformation("[ChatCleanupService] 다음 실행까지 {Hours}시간 대기", chatSettings.CleanupIntervalHours);
            await Task.Delay(
                TimeSpan.FromHours(chatSettings.CleanupIntervalHours),
                stoppingToken);
        }
    }

    /// <summary>
    /// 만료된 채팅방 정리
    /// </summary>
    private async Task CleanupExpiredChatRooms()
    {
        using var scope = serviceProvider.CreateScope();
        var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        logger.LogInformation("[ChatCleanupService] 채팅 정리 프로세스 시작");

        var startTime = DateTime.UtcNow;

        // 만료된 채팅방 조회
        var expiredRoomIds = await chatRepo.GetExpiredChatRooms(chatSettings.MessageRetentionDays);

        if (expiredRoomIds.Count == 0)
        {
            logger.LogInformation("[ChatCleanupService] 정리할 채팅방이 없습니다");
            return;
        }

        logger.LogInformation("[ChatCleanupService] {Count}개의 만료된 채팅방 발견", expiredRoomIds.Count);

        var successCount = 0;
        var failCount = 0;
        var totalDeletedMessages = 0;

        foreach (var roomId in expiredRoomIds)
        {
            try
            {
                // 메시지 삭제
                var deletedCount = await chatRepo.DeleteMessagesForRoom(roomId);
                totalDeletedMessages += deletedCount;

                // 채팅방 소프트 삭제
                await chatRepo.SoftDeleteChatRoom(roomId);

                successCount++;

                logger.LogInformation("[ChatCleanupService] 채팅방 정리 완료: RoomId={RoomId}, 삭제된 메시지={MessageCount}",
                    roomId, deletedCount);
            }
            catch (Exception ex)
            {
                failCount++;
                logger.LogError(ex, "[ChatCleanupService] 채팅방 정리 실패: RoomId={RoomId}", roomId);
            }
        }

        var elapsed = DateTime.UtcNow - startTime;

        logger.LogInformation(
            "[ChatCleanupService] 채팅 정리 완료 - 성공: {Success}, 실패: {Fail}, 총 삭제 메시지: {TotalMessages}, 소요 시간: {Elapsed}초",
            successCount, failCount, totalDeletedMessages, elapsed.TotalSeconds);
    }

    /// <summary>
    /// 서비스 중지 시
    /// </summary>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[ChatCleanupService] 채팅 정리 서비스가 중지되었습니다");
        return base.StopAsync(cancellationToken);
    }
}
