using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Repository.Payment;
using TicketPlatFormServer.Repository.Settlements;
using TicketPlatFormServer.Services.Balance;

namespace TicketPlatFormServer.Services.BackgroundServices;

public class SettlementProcessingService(
    IServiceProvider serviceProvider,
    TossPaymentsSettings settings,
    ILogger<SettlementProcessingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = settings.SettlementProcessingIntervalMinutes > 0
            ? settings.SettlementProcessingIntervalMinutes
            : 5;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingSettlementsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[SettlementProcessingService] 정산 배치 실행 중 오류가 발생했습니다.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task ProcessPendingSettlementsAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var settlementRepository = scope.ServiceProvider.GetRequiredService<ISettlementRepository>();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalanceService>();

        var pendingStatus = await paymentRepository.GetSettlementStatusByCodeAsync("pending");
        var processingStatus = await paymentRepository.GetSettlementStatusByCodeAsync("processing");
        var completedStatus = await paymentRepository.GetSettlementStatusByCodeAsync("completed");
        var failedStatus = await paymentRepository.GetSettlementStatusByCodeAsync("failed");

        if (pendingStatus == null || processingStatus == null || completedStatus == null || failedStatus == null)
        {
            logger.LogWarning("[SettlementProcessingService] 정산 상태 코드가 누락되어 배치를 건너뜁니다.");
            return;
        }

        var dueSettlements = await settlementRepository.GetDuePendingSettlementsAsync(DateTime.UtcNow);
        if (dueSettlements.Count == 0)
        {
            return;
        }

        foreach (var settlement in dueSettlements)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (settlement.BankAccountId == null || settlement.BankAccount == null)
                {
                    settlement.StatusId = failedStatus.Id;
                    settlement.FailureReason = "정산 계좌가 설정되지 않았습니다.";
                    settlement.UpdatedAt = DateTime.UtcNow;
                    await settlementRepository.UpdateSettlementAsync(settlement);
                    continue;
                }

                settlement.StatusId = processingStatus.Id;
                settlement.UpdatedAt = DateTime.UtcNow;
                await settlementRepository.UpdateSettlementAsync(settlement);

                await balanceService.CreditAsync(
                    userId: (int)settlement.SellerId,
                    amount: settlement.NetAmount,
                    referenceType: "SETTLEMENT",
                    referenceId: settlement.Id,
                    description: $"티켓 판매 정산 (거래 #{settlement.TransactionId})");

                settlement.StatusId = completedStatus.Id;
                settlement.ProcessedAt = DateTime.UtcNow;
                settlement.FailureReason = null;
                settlement.UpdatedAt = DateTime.UtcNow;
                await settlementRepository.UpdateSettlementAsync(settlement);
            }
            catch (Exception ex)
            {
                await HandleRetryOrFailureAsync(settlementRepository, settlement, pendingStatus.Id, failedStatus.Id, ex.Message);
            }
        }
    }

    private async Task HandleRetryOrFailureAsync(
        ISettlementRepository settlementRepository,
        DBModel.Settlement settlement,
        long pendingStatusId,
        long failedStatusId,
        string reason)
    {
        var maxRetry = settings.MaxSettlementRetryCount > 0 ? settings.MaxSettlementRetryCount : 3;
        var retryCount = (settlement.RetryCount ?? 0) + 1;

        settlement.RetryCount = retryCount;
        settlement.FailureReason = reason;
        settlement.UpdatedAt = DateTime.UtcNow;

        if (retryCount >= maxRetry)
        {
            settlement.StatusId = failedStatusId;
        }
        else
        {
            settlement.StatusId = pendingStatusId;
            settlement.ScheduledAt = DateTime.UtcNow.AddMinutes(settings.SettlementProcessingIntervalMinutes > 0
                ? settings.SettlementProcessingIntervalMinutes
                : 5);
        }

        await settlementRepository.UpdateSettlementAsync(settlement);
    }
}
