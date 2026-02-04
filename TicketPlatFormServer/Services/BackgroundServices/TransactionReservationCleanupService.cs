using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketPlatFormServer.Repository.Chat;
using TicketPlatFormServer.Repository.Ticket;
using TicketPlatFormServer.Repository.Transactions;

namespace TicketPlatFormServer.Services.BackgroundServices;

/// <summary>
/// 만료된 결제 예약 자동 정리 백그라운드 서비스
/// </summary>
public class TransactionReservationCleanupService(
    IServiceProvider serviceProvider,
    ILogger<TransactionReservationCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    /// <summary>
    /// 백그라운드 서비스 실행ㅋ
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[TransactionReservationCleanupService] 예약 정리 서비스 시작. 실행 주기: {Hours}시간",
            CleanupInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredReservations();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TransactionReservationCleanupService] 예약 정리 중 오류 발생");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredReservations()
    {
        using var scope = serviceProvider.CreateScope();
        var transactionRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var ticketRepo = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        var now = DateTime.UtcNow;
        var expiredTransactions = await transactionRepo.GetExpiredPendingTransactionsAsync(now);

        if (expiredTransactions.Count == 0)
        {
            logger.LogInformation("[TransactionReservationCleanupService] 만료된 예약 거래 없음");
            return;
        }

        logger.LogInformation("[TransactionReservationCleanupService] 만료된 예약 거래 발견: {Count}", expiredTransactions.Count);

        foreach (var transaction in expiredTransactions)
        {
            try
            {
                foreach (var item in transaction.TransactionItems)
                {
                    await ticketRepo.ReleaseTicketQuantityAsync((int)item.TicketId, item.Quantity);
                }

                var cancelledStatus = await transactionRepo.GetTransactionStatusByCodeAsync("cancelled");
                if (cancelledStatus == null)
                {
                    logger.LogWarning("[TransactionReservationCleanupService] 거래 상태 코드 없음: cancelled");
                    continue;
                }

                await transactionRepo.UpdateTransactionStatusAsync(transaction.Id, cancelledStatus.Id);
                await transactionRepo.UpdateTransactionCancelledAtAsync(transaction.Id, now);

                var room = await chatRepo.GetChatRoomByTransactionId(transaction.Id);
                if (room != null)
                {
                    var message = await chatRepo.CreateMessage(
                        room.Id,
                        (int)room.SellerId,
                        "결제 요청이 만료되었습니다.",
                        null
                    );

                    await chatRepo.UpdateLastMessageAt(room.Id, message.CreatedAt ?? now);
                    await chatRepo.IncrementUnreadCount(room.Id, true);
                    await chatRepo.ClearTransactionId(room.Id);
                }

                logger.LogInformation("[TransactionReservationCleanupService] 예약 만료 처리 완료 - TransactionId: {TransactionId}",
                    transaction.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TransactionReservationCleanupService] 예약 만료 처리 실패 - TransactionId: {TransactionId}",
                    transaction.Id);
            }
        }
    }
}
