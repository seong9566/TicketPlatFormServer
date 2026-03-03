using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Settlement;
using TicketPlatFormServer.Repository.Payment;
using TicketPlatFormServer.Repository.Settlements;
using TicketPlatFormServer.Repository.ReadModels;
using TicketPlatFormServer.Services.Balance;
using TicketPlatFormServer.Services.Notification;

namespace TicketPlatFormServer.Services.Settlements;

public class SettlementService(
    ISettlementRepository settlementRepository,
    IPaymentRepository paymentRepository,
    IBalanceService balanceService,
    INotificationService notificationService,
    ILogger<SettlementService> logger) : ISettlementService
{
    private const int MaxSettlementRetryCount = 5;

    public async Task ProcessPendingSettlementsAsync()
    {
        var pendingStatus = await paymentRepository.GetSettlementStatusByCodeAsync("pending");
        var processingStatus = await paymentRepository.GetSettlementStatusByCodeAsync("processing");
        var completedStatus = await paymentRepository.GetSettlementStatusByCodeAsync("completed");
        var failedStatus = await paymentRepository.GetSettlementStatusByCodeAsync("failed");

        if (pendingStatus == null || processingStatus == null || completedStatus == null || failedStatus == null)
        {
            logger.LogWarning("[SettlementService.ProcessPendingSettlementsAsync] 정산 상태 코드가 누락되어 배치를 건너뜁니다.");
            return;
        }

        var dueSettlements = await settlementRepository.GetDuePendingSettlementsAsync(DateTime.UtcNow);
        if (dueSettlements.Count == 0)
        {
            return;
        }

        foreach (var settlement in dueSettlements)
        {
            var now = DateTime.UtcNow;
            var data = BuildNotificationData(settlement);

            try
            {
                if (await settlementRepository.HasBalanceTransactionAsync(settlement.Id))
                {
                    settlement.StatusId = completedStatus.Id;
                    settlement.ProcessedAt = now;
                    settlement.FailureReason = null;
                    settlement.UpdatedAt = now;
                    await settlementRepository.UpdateSettlementAsync(settlement);

                    await SendSettlementCompletedNotificationAsync(settlement, data);
                    continue;
                }

                settlement.StatusId = processingStatus.Id;
                settlement.UpdatedAt = now;
                await settlementRepository.UpdateSettlementAsync(settlement);

                if (settlement.BankAccountId == null || settlement.BankAccountId == 0)
                {
                    settlement.StatusId = failedStatus.Id;
                    settlement.FailureReason = "인증된 정산 계좌가 없습니다";
                    settlement.UpdatedAt = DateTime.UtcNow;
                    await settlementRepository.UpdateSettlementAsync(settlement);

                    await SendSettlementFailedNotificationAsync(settlement, data);
                    continue;
                }

                await balanceService.CreditAsync(
                    (int)settlement.SellerId,
                    settlement.NetAmount,
                    "SETTLEMENT",
                    settlement.Id,
                    $"정산 완료 - 거래 #{settlement.TransactionId}");

                settlement.StatusId = completedStatus.Id;
                settlement.ProcessedAt = DateTime.UtcNow;
                settlement.FailureReason = null;
                settlement.UpdatedAt = DateTime.UtcNow;
                await settlementRepository.UpdateSettlementAsync(settlement);

                await SendSettlementCompletedNotificationAsync(settlement, data);
            }
            catch (Exception ex)
            {
                var retryCount = (settlement.RetryCount ?? 0) + 1;
                settlement.RetryCount = retryCount;
                settlement.FailureReason = ex.Message;
                settlement.UpdatedAt = DateTime.UtcNow;

                if (retryCount >= MaxSettlementRetryCount)
                {
                    settlement.StatusId = failedStatus.Id;
                }
                else
                {
                    settlement.StatusId = pendingStatus.Id;
                }

                await settlementRepository.UpdateSettlementAsync(settlement);

                if (retryCount >= MaxSettlementRetryCount)
                {
                    await SendSettlementFailedNotificationAsync(settlement, data);
                }
            }
        }
    }

    public async Task<SettlementListResponseDto> GetBySellerAsync(long sellerId, int page, int pageSize, string? statusFilter)
    {
        var normalizedPage = page > 0 ? page : 1;
        var normalizedPageSize = pageSize > 0 ? pageSize : 20;

        var readModels = (await settlementRepository.GetBySellerIdAsync(
            sellerId,
            normalizedPage,
            normalizedPageSize,
            statusFilter)).ToList();

        var totalCount = await settlementRepository.CountBySellerIdAsync(sellerId, statusFilter);
        var totalNetAmount = await settlementRepository.GetTotalCompletedNetAmountAsync(sellerId);

        var items = readModels.Select(ToResponse).ToList();

        return new SettlementListResponseDto
        {
            Settlements = items,
            TotalCount = totalCount,
            TotalNetAmount = totalNetAmount,
            Summary = new SettlementSummaryDto
            {
                TotalAmount = items.Sum(x => x.Amount),
                TotalFee = items.Sum(x => x.Fee),
                TotalNetAmount = items.Sum(x => x.NetAmount),
                PendingCount = items.Count(x => x.StatusCode == "pending"),
                OnHoldCount = items.Count(x => x.StatusCode == "on_hold"),
                ProcessingCount = items.Count(x => x.StatusCode == "processing"),
                CompletedCount = items.Count(x => x.StatusCode == "completed"),
                FailedCount = items.Count(x => x.StatusCode == "failed")
            }
        };
    }

    public async Task<SettlementDetailRespDto> GetDetailAsync(long settlementId, long sellerId)
    {
        var detail = await settlementRepository.GetDetailByIdAndSellerIdAsync(settlementId, sellerId);
        if (detail == null)
        {
            throw new AppException("정산 내역을 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        return new SettlementDetailRespDto
        {
            Id = detail.Id,
            TransactionId = detail.TransactionId,
            Amount = detail.Amount,
            Fee = detail.Fee,
            NetAmount = detail.NetAmount,
            StatusCode = detail.StatusCode,
            StatusName = detail.StatusName,
            ScheduledAt = detail.ScheduledAt,
            ProcessedAt = detail.ProcessedAt,
            FailureReason = detail.FailureReason,
            RetryCount = detail.RetryCount,
            EventTitle = detail.EventTitle,
            SeatInfo = detail.SeatInfo,
            BuyerNickname = detail.BuyerNickname,
            BankName = detail.BankName,
            AccountNumber = MaskAccountNumber(detail.AccountNumber),
            AccountHolder = detail.AccountHolder
        };
    }

    public async Task<SettlementListResponseDto> GetMySettlementsAsync(long sellerId)
    {
        var settlements = await settlementRepository.GetSettlementsBySellerIdAsync(sellerId);

        var items = settlements.Select(ToResponse).ToList();
        var summary = new SettlementSummaryDto
        {
            TotalAmount = settlements.Sum(x => x.Amount),
            TotalFee = settlements.Sum(x => x.Fee),
            TotalNetAmount = settlements.Sum(x => x.NetAmount),
            PendingCount = settlements.Count(x => x.Status.Code == "pending"),
            OnHoldCount = settlements.Count(x => x.Status.Code == "on_hold"),
            ProcessingCount = settlements.Count(x => x.Status.Code == "processing"),
            CompletedCount = settlements.Count(x => x.Status.Code == "completed"),
            FailedCount = settlements.Count(x => x.Status.Code == "failed")
        };

        return new SettlementListResponseDto
        {
            Settlements = items,
            TotalCount = items.Count,
            Summary = summary
        };
    }

    public async Task<SettlementResponseDto?> GetSettlementByIdAsync(long id, long sellerId)
    {
        var settlement = await settlementRepository.GetSettlementByIdAsync(id, sellerId);
        return settlement == null ? null : ToResponse(settlement);
    }

    private async Task SendSettlementCompletedNotificationAsync(DBModel.Settlement settlement, Dictionary<string, string> data)
    {
        try
        {
            await notificationService.CreateAndSendAsync(
                settlement.SellerId,
                "SETTLEMENT_COMPLETED",
                "정산 완료",
                $"정산금 {settlement.NetAmount:N0}원이 잔고에 적립되었습니다.",
                data);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[SettlementService.ProcessPendingSettlementsAsync] 정산 완료 알림 발송 실패: SettlementId={SettlementId}",
                settlement.Id);
        }
    }

    private async Task SendSettlementFailedNotificationAsync(DBModel.Settlement settlement, Dictionary<string, string> data)
    {
        try
        {
            await notificationService.CreateAndSendAsync(
                settlement.SellerId,
                "SETTLEMENT_FAILED",
                "정산 실패",
                "정산 처리에 실패했습니다. 계좌 정보를 확인해주세요.",
                data);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[SettlementService.ProcessPendingSettlementsAsync] 정산 실패 알림 발송 실패: SettlementId={SettlementId}",
                settlement.Id);
        }
    }

    private static Dictionary<string, string> BuildNotificationData(DBModel.Settlement settlement)
    {
        return new Dictionary<string, string>
        {
            ["settlementId"] = settlement.Id.ToString(),
            ["transactionId"] = settlement.TransactionId.ToString(),
            ["netAmount"] = settlement.NetAmount.ToString()
        };
    }

    private static string? MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            return accountNumber;
        }

        return accountNumber.Length > 4
            ? new string('*', accountNumber.Length - 4) + accountNumber[^4..]
            : accountNumber;
    }

    private static SettlementResponseDto ToResponse(SettlementListReadModel settlement)
    {
        return new SettlementResponseDto
        {
            Id = settlement.Id,
            TransactionId = settlement.TransactionId,
            Amount = settlement.Amount,
            Fee = settlement.Fee,
            NetAmount = settlement.NetAmount,
            StatusCode = settlement.StatusCode,
            StatusName = settlement.StatusName,
            ScheduledAt = settlement.ScheduledAt,
            ProcessedAt = settlement.ProcessedAt,
            FailureReason = settlement.FailureReason,
            EventTitle = settlement.EventTitle,
            SeatInfo = settlement.SeatInfo
        };
    }

    private static SettlementResponseDto ToResponse(DBModel.Settlement settlement)
    {
        return new SettlementResponseDto
        {
            Id = settlement.Id,
            TransactionId = settlement.TransactionId,
            Amount = settlement.Amount,
            Fee = settlement.Fee,
            NetAmount = settlement.NetAmount,
            StatusCode = settlement.Status.Code,
            StatusName = settlement.Status.NameKo ?? settlement.Status.Code,
            ScheduledAt = settlement.ScheduledAt,
            ProcessedAt = settlement.ProcessedAt,
            FailureReason = settlement.FailureReason
        };
    }
}
