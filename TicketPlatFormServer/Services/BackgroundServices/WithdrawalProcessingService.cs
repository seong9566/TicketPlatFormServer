using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DTO.Payment;
using TicketPlatFormServer.Repository.Balance;
using TicketPlatFormServer.Repository.Withdrawal;
using TicketPlatFormServer.Services.Balance;
using TicketPlatFormServer.Services.Payment;

namespace TicketPlatFormServer.Services.BackgroundServices;

public class WithdrawalProcessingService(
    IServiceProvider serviceProvider,
    TossPaymentsSettings settings,
    ILogger<WithdrawalProcessingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = settings.WithdrawalProcessingIntervalMinutes > 0
            ? settings.WithdrawalProcessingIntervalMinutes
            : (settings.SettlementProcessingIntervalMinutes > 0 ? settings.SettlementProcessingIntervalMinutes : 5);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingWithdrawalsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[WithdrawalProcessingService] 출금 배치 실행 중 오류가 발생했습니다.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task ProcessPendingWithdrawalsAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var withdrawalRepository = scope.ServiceProvider.GetRequiredService<IWithdrawalRepository>();
        var tossPaymentsService = scope.ServiceProvider.GetRequiredService<ITossPaymentsService>();
        var balanceRepository = scope.ServiceProvider.GetRequiredService<IBalanceRepository>();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalanceService>();

        var requestedStatus = await withdrawalRepository.GetStatusByCodeAsync("requested");
        var processingStatus = await withdrawalRepository.GetStatusByCodeAsync("processing");
        var completedStatus = await withdrawalRepository.GetStatusByCodeAsync("completed");
        var failedStatus = await withdrawalRepository.GetStatusByCodeAsync("failed");

        if (requestedStatus == null || processingStatus == null || completedStatus == null || failedStatus == null)
        {
            logger.LogWarning("[WithdrawalProcessingService] 출금 상태 코드가 누락되어 배치를 건너뜁니다.");
            return;
        }

        var pendingWithdrawals = await withdrawalRepository.GetPendingWithdrawalsAsync();
        if (pendingWithdrawals.Count == 0)
        {
            return;
        }

        foreach (var withdrawal in pendingWithdrawals)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (withdrawal.BankAccount == null
                    || string.IsNullOrWhiteSpace(withdrawal.BankAccount.BankCode)
                    || string.IsNullOrWhiteSpace(withdrawal.BankAccount.AccountNumber))
                {
                    throw new InvalidOperationException("출금 계좌 정보가 유효하지 않습니다.");
                }

                withdrawal.StatusId = processingStatus.Id;
                withdrawal.Status = processingStatus;
                withdrawal.UpdatedAt = DateTime.UtcNow;
                await withdrawalRepository.UpdateAsync(withdrawal);

                var transferRequest = new TransferRequestDto
                {
                    RefPayoutId = $"WD-{withdrawal.Id}",
                    Destination = $"{withdrawal.BankAccount.BankCode}:{withdrawal.BankAccount.AccountNumber}",
                    Amount = (int)withdrawal.NetAmount,
                    TransactionDescription = "출금"
                };

                var transferResponse = await tossPaymentsService.RequestTransferAsync(transferRequest);

                withdrawal.StatusId = completedStatus.Id;
                withdrawal.Status = completedStatus;
                withdrawal.PayoutId = transferResponse.PayoutId;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.FailureReason = null;
                withdrawal.UpdatedAt = DateTime.UtcNow;

                var pendingDebitedRows = await balanceRepository.AtomicPendingDebitAsync(withdrawal.UserId, withdrawal.Amount);
                if (pendingDebitedRows == 0)
                {
                    throw new InvalidOperationException("출금 보류 금액 차감에 실패했습니다.");
                }

                await withdrawalRepository.UpdateAsync(withdrawal);
            }
            catch (Exception ex)
            {
                await HandleRetryOrFailureAsync(
                    withdrawalRepository,
                    balanceRepository,
                    balanceService,
                    withdrawal,
                    requestedStatus.Id,
                    failedStatus.Id,
                    ex.Message);
            }
        }
    }

    private async Task HandleRetryOrFailureAsync(
        IWithdrawalRepository withdrawalRepository,
        IBalanceRepository balanceRepository,
        IBalanceService balanceService,
        DBModel.Withdrawal withdrawal,
        long requestedStatusId,
        long failedStatusId,
        string reason)
    {
        var maxRetry = settings.MaxWithdrawalRetryCount > 0 ? settings.MaxWithdrawalRetryCount : 3;
        var retryCount = (withdrawal.RetryCount ?? 0) + 1;

        withdrawal.RetryCount = retryCount;
        withdrawal.FailureReason = reason;
        withdrawal.UpdatedAt = DateTime.UtcNow;

        if (retryCount >= maxRetry)
        {
            withdrawal.StatusId = failedStatusId;
            withdrawal.ProcessedAt = DateTime.UtcNow;

            var creditRows = await balanceRepository.AtomicCreditAsync(withdrawal.UserId, withdrawal.Amount);
            if (creditRows > 0)
            {
                var pendingRows = await balanceRepository.AtomicPendingDebitAsync(withdrawal.UserId, withdrawal.Amount);
                if (pendingRows == 0)
                {
                    await balanceRepository.AtomicDebitAsync(withdrawal.UserId, withdrawal.Amount);
                }
                else
                {
                    var balance = await balanceRepository.GetByUserIdAsync(withdrawal.UserId);
                    if (balance != null)
                    {
                        await balanceRepository.AddTransactionAsync(new DBModel.BalanceTransaction
                        {
                            UserId = withdrawal.UserId,
                            Type = "REFUND",
                            Amount = withdrawal.Amount,
                            BalanceAfter = balance.Available,
                            ReferenceType = "WITHDRAWAL",
                            ReferenceId = withdrawal.Id,
                            Description = "출금 실패 환불"
                        });
                    }
                }
            }

            logger.LogWarning("[WithdrawalProcessingService] 출금 실패 확정 UserId={UserId}, WithdrawalId={WithdrawalId}",
                withdrawal.UserId, withdrawal.Id);
        }
        else
        {
            withdrawal.StatusId = requestedStatusId;
        }

        await withdrawalRepository.SaveChangesAsync();

        _ = balanceService;
    }
}
