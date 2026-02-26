using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.DTO.Withdrawal;
using TicketPlatFormServer.Repository.Balance;
using TicketPlatFormServer.Repository.BankAccounts;
using TicketPlatFormServer.Repository.Withdrawal;
using TicketPlatFormServer.Services.Balance;

namespace TicketPlatFormServer.Services.Withdrawal;

public class WithdrawalService(
    IWithdrawalRepository withdrawalRepository,
    IBalanceService balanceService,
    IBalanceRepository balanceRepository,
    IBankAccountRepository bankAccountRepository,
    TossPaymentsSettings settings,
    ILogger<WithdrawalService> logger) : IWithdrawalService
{
    private readonly TossPaymentsSettings _settings = settings;
    private readonly long _minWithdrawalAmount = settings.MinWithdrawalAmount > 0 ? settings.MinWithdrawalAmount : 1_000;
    private readonly int _maxDailyWithdrawals = settings.MaxDailyWithdrawals > 0 ? settings.MaxDailyWithdrawals : 3;
    private readonly long _maxDailyWithdrawalAmount = settings.MaxDailyWithdrawalAmount > 0 ? settings.MaxDailyWithdrawalAmount : 5_000_000;

    public async Task<WithdrawalResponseDto> RequestWithdrawalAsync(int userId, WithdrawalRequestDto request, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new AppException("멱등성 키가 필요합니다.", HttpStatusCode.BadRequest);
        }

        var existing = await withdrawalRepository.GetByIdempotencyKeyAsync(idempotencyKey);
        if (existing != null)
        {
            return ToResponse(existing);
        }

        DBModel.BankAccount? bankAccount;
        if (request.BankAccountId.HasValue)
        {
            bankAccount = await bankAccountRepository.GetByIdAndUserIdAsync(request.BankAccountId.Value, userId);
            if (bankAccount == null)
            {
                throw new AppException("출금 계좌를 찾을 수 없습니다.", HttpStatusCode.NotFound);
            }
        }
        else
        {
            bankAccount = await bankAccountRepository.GetVerifiedBankAccountByUserIdAsync(userId);
            if (bankAccount == null)
            {
                throw new AppException("인증된 대표 계좌를 찾을 수 없습니다.", HttpStatusCode.NotFound);
            }
        }

        if (bankAccount.Verified != true)
        {
            throw new AppException("인증된 계좌만 출금할 수 있습니다.", HttpStatusCode.BadRequest);
        }

        if (request.Amount < _minWithdrawalAmount)
        {
            throw new AppException($"최소 출금 금액은 {_minWithdrawalAmount:N0}원입니다.", HttpStatusCode.BadRequest);
        }

        var todayCount = await withdrawalRepository.GetTodayCountByUserIdAsync(userId);
        if (todayCount >= _maxDailyWithdrawals)
        {
            throw new AppException("일일 출금 횟수를 초과했습니다.", HttpStatusCode.BadRequest);
        }

        var todaySum = await withdrawalRepository.GetTodaySumByUserIdAsync(userId);
        if (todaySum + request.Amount > _maxDailyWithdrawalAmount)
        {
            throw new AppException("일일 출금 한도를 초과했습니다.", HttpStatusCode.BadRequest);
        }

        var requestedStatus = await withdrawalRepository.GetStatusByCodeAsync("requested");
        if (requestedStatus == null)
        {
            throw new AppException("출금 상태 코드가 올바르지 않습니다.", HttpStatusCode.InternalServerError);
        }

        var fee = (long)_settings.WithdrawalFee;
        var netAmount = request.Amount - fee;

        await balanceService.DebitAsync(userId, request.Amount, "WITHDRAWAL", 0, "출금 요청");

        var pendingCreditRows = await balanceRepository.AtomicPendingCreditAsync(userId, request.Amount);
        if (pendingCreditRows == 0)
        {
            await balanceRepository.AtomicCreditAsync(userId, request.Amount);
            throw new AppException("출금 보류 금액 반영에 실패했습니다.", HttpStatusCode.InternalServerError);
        }

        var withdrawal = new DBModel.Withdrawal
        {
            UserId = userId,
            BankAccountId = bankAccount.Id,
            Amount = request.Amount,
            Fee = fee,
            NetAmount = netAmount,
            StatusId = requestedStatus.Id,
            IdempotencyKey = idempotencyKey,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RetryCount = 0
        };

        await withdrawalRepository.AddAsync(withdrawal);
        await withdrawalRepository.SaveChangesAsync();

        withdrawal.Status = requestedStatus;
        withdrawal.BankAccount = bankAccount;

        logger.LogInformation("[WithdrawalService.RequestWithdrawalAsync] UserId={UserId}, WithdrawalId={WithdrawalId}, Amount={Amount}",
            userId, withdrawal.Id, withdrawal.Amount);

        return ToResponse(withdrawal);
    }

    public async Task<WithdrawalResponseDto> CancelWithdrawalAsync(int userId, long withdrawalId)
    {
        var withdrawal = await withdrawalRepository.GetByIdAndUserIdAsync(withdrawalId, userId);
        if (withdrawal == null)
        {
            throw new AppException("출금 정보를 찾을 수 없습니다.", HttpStatusCode.NotFound);
        }

        if (!string.Equals(withdrawal.Status.Code, "requested", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("요청 상태의 출금만 취소할 수 있습니다.", HttpStatusCode.BadRequest);
        }

        var cancelledStatus = await withdrawalRepository.GetStatusByCodeAsync("cancelled");
        if (cancelledStatus == null)
        {
            throw new AppException("출금 상태 코드가 올바르지 않습니다.", HttpStatusCode.InternalServerError);
        }

        var amount = withdrawal.Amount;
        var creditedRows = await balanceRepository.AtomicCreditAsync(userId, amount);
        if (creditedRows == 0)
        {
            throw new AppException("출금 취소 처리에 실패했습니다.", HttpStatusCode.InternalServerError);
        }

        var pendingDebitedRows = await balanceRepository.AtomicPendingDebitAsync(userId, amount);
        if (pendingDebitedRows == 0)
        {
            await balanceRepository.AtomicDebitAsync(userId, amount);
            throw new AppException("출금 취소 처리에 실패했습니다.", HttpStatusCode.InternalServerError);
        }

        var balance = await balanceRepository.GetByUserIdAsync(userId);
        if (balance == null)
        {
            throw new AppException("잔고 정보를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        await balanceRepository.AddTransactionAsync(new DBModel.BalanceTransaction
        {
            UserId = userId,
            Type = "REFUND",
            Amount = amount,
            BalanceAfter = balance.Available,
            ReferenceType = "WITHDRAWAL",
            ReferenceId = withdrawal.Id,
            Description = "출금 취소 환불"
        });

        withdrawal.StatusId = cancelledStatus.Id;
        withdrawal.Status = cancelledStatus;
        withdrawal.ProcessedAt = DateTime.UtcNow;
        withdrawal.UpdatedAt = DateTime.UtcNow;
        await withdrawalRepository.SaveChangesAsync();

        logger.LogInformation("[WithdrawalService.CancelWithdrawalAsync] UserId={UserId}, WithdrawalId={WithdrawalId}",
            userId, withdrawal.Id);

        return ToResponse(withdrawal);
    }

    public async Task<WithdrawalListResponseDto> GetWithdrawalHistoryAsync(int userId, int page, int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var withdrawals = await withdrawalRepository.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await withdrawalRepository.GetCountByUserIdAsync(userId);

        return new WithdrawalListResponseDto
        {
            Items = withdrawals.Select(ToResponse).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BalanceResponseDto> GetBalanceAsync(int userId)
    {
        var balance = await balanceService.GetBalanceAsync(userId);
        return new BalanceResponseDto
        {
            Available = balance.Available,
            Pending = balance.Pending,
            TotalEarned = balance.TotalEarned,
            TotalWithdrawn = balance.TotalWithdrawn
        };
    }

    public async Task<BalanceHistoryResponseDto> GetBalanceHistoryAsync(int userId, int page, int pageSize)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var items = await balanceRepository.GetByUserIdAsync(userId, page, pageSize);
        var totalCount = await balanceRepository.GetTransactionCountByUserIdAsync(userId);

        return new BalanceHistoryResponseDto
        {
            Items = items.Select(x => new BalanceTransactionDto
            {
                Id = x.Id,
                Type = x.Type,
                Amount = x.Amount,
                BalanceAfter = x.BalanceAfter,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static WithdrawalResponseDto ToResponse(DBModel.Withdrawal withdrawal)
    {
        return new WithdrawalResponseDto
        {
            Id = withdrawal.Id,
            Amount = withdrawal.Amount,
            Fee = withdrawal.Fee,
            NetAmount = withdrawal.NetAmount,
            StatusCode = withdrawal.Status.Code,
            StatusName = withdrawal.Status.NameKo,
            BankName = withdrawal.BankAccount.BankName,
            RequestedAt = withdrawal.RequestedAt,
            ProcessedAt = withdrawal.ProcessedAt,
            FailureReason = withdrawal.FailureReason
        };
    }
}
