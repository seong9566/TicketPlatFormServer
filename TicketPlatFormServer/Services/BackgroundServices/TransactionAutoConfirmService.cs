using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.Repository;
using TicketPlatFormServer.Repository.Disputes;
using TicketPlatFormServer.Repository.Transactions;
using TicketPlatFormServer.Services.Notification;
using TicketPlatFormServer.Services.Payment;

namespace TicketPlatFormServer.Services.BackgroundServices;

public class TransactionAutoConfirmService(
    IServiceProvider serviceProvider,
    ILogger<TransactionAutoConfirmService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[TransactionAutoConfirmService] 자동 구매확정 서비스 시작. 실행 주기: {Minutes}분", CheckInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAutoConfirmAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TransactionAutoConfirmService] 자동 구매확정 처리 중 오류 발생");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessAutoConfirmAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var disputeRepository = scope.ServiceProvider.GetRequiredService<IDisputeRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var context = scope.ServiceProvider.GetRequiredService<TicketContext>();

        var dueTransactions = await transactionRepository.GetAutoConfirmDueTransactionsAsync(DateTime.UtcNow);
        if (dueTransactions.Count == 0)
        {
            return;
        }

        var pendingStatus = await disputeRepository.GetDisputeStatusByCodeAsync("PENDING");
        var inReviewStatus = await disputeRepository.GetDisputeStatusByCodeAsync("IN_REVIEW");
        if (pendingStatus == null || inReviewStatus == null)
        {
            logger.LogWarning("[TransactionAutoConfirmService] 분쟁 상태 코드를 찾지 못해 자동확정을 건너뜁니다.");
            return;
        }

        foreach (var transaction in dueTransactions)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var hasOpenDispute = await disputeRepository.HasActiveDisputeAsync(transaction.Id, [pendingStatus.Id, inReviewStatus.Id]);
                if (hasOpenDispute)
                {
                    logger.LogInformation("[TransactionAutoConfirmService] 분쟁 진행 중 거래 자동확정 건너뜀. TransactionId={TransactionId}", transaction.Id);
                    continue;
                }

                await paymentService.ReleaseEscrowAsync(transaction.Id);

                var sellerNickname = await context.UserProfiles
                    .Where(x => x.UserId == transaction.SellerId)
                    .Select(x => x.Nickname)
                    .FirstOrDefaultAsync() ?? "판매자";

                await notificationService.CreateAndSendAsync(
                    transaction.BuyerId,
                    "REVIEW_REQUEST",
                    "거래는 어떠셨나요?",
                    $"{sellerNickname} 판매자에 대한 리뷰를 남겨주세요.",
                    new Dictionary<string, string>
                    {
                        ["type"] = "REVIEW_REQUEST",
                        ["transactionId"] = transaction.Id.ToString()
                    });

                logger.LogInformation("[TransactionAutoConfirmService] 자동 구매확정 및 리뷰요청 완료. TransactionId={TransactionId}", transaction.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[TransactionAutoConfirmService] 자동 구매확정 실패. TransactionId={TransactionId}", transaction.Id);
            }
        }
    }
}
