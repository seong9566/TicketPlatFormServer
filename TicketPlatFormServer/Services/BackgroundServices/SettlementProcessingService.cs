using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DTO.Payment;
using TicketPlatFormServer.Repository.Payment;
using TicketPlatFormServer.Repository.Settlements;
using TicketPlatFormServer.Services.Payment;

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
        var tossPaymentsService = scope.ServiceProvider.GetRequiredService<ITossPaymentsService>();

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

                var transfer = await tossPaymentsService.RequestTransferAsync(new TransferRequestDto
                {
                    RefPayoutId = $"SETTLEMENT_{settlement.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                    Destination = settlement.SellerId.ToString(),
                    ScheduleType = "EXPRESS",
                    Amount = settlement.NetAmount,
                    TransactionDescription = "정산",
                    Metadata = new Dictionary<string, string>
                    {
                        ["settlementId"] = settlement.Id.ToString(),
                        ["transactionId"] = settlement.TransactionId.ToString()
                    }
                });

                if (string.Equals(transfer.Status, "DONE", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(transfer.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(transfer.Status))
                {
                    settlement.StatusId = completedStatus.Id;
                    settlement.ProcessedAt = DateTime.UtcNow;
                    settlement.FailureReason = null;
                    settlement.UpdatedAt = DateTime.UtcNow;
                    await settlementRepository.UpdateSettlementAsync(settlement);
                }
                else
                {
                    await HandleRetryOrFailureAsync(settlementRepository, settlement, pendingStatus.Id, failedStatus.Id, $"정산 상태: {transfer.Status}");
                }
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
