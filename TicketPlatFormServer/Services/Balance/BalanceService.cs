using System.Net;
using TicketPlatFormServer.Common;
using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.Repository.Balance;

namespace TicketPlatFormServer.Services.Balance;

public class BalanceService(
    IBalanceRepository balanceRepository,
    ILogger<BalanceService> logger) : IBalanceService
{
    public async Task<DBModel.UserBalance> GetBalanceAsync(int userId)
    {
        return await balanceRepository.GetOrCreateByUserIdAsync(userId);
    }

    public async Task CreditAsync(int userId, long amount, string referenceType, long referenceId, string description)
    {
        if (amount <= 0)
        {
            throw new AppException("적립 금액은 0보다 커야 합니다.", HttpStatusCode.BadRequest);
        }

        await balanceRepository.GetOrCreateByUserIdAsync(userId);
        await balanceRepository.AtomicCreditAsync(userId, amount);

        var balance = await balanceRepository.GetByUserIdAsync(userId);
        if (balance == null)
        {
            throw new AppException("잔고 정보를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        balance.TotalEarned += amount;

        await balanceRepository.AddTransactionAsync(new DBModel.BalanceTransaction
        {
            UserId = userId,
            Type = "CREDIT",
            Amount = amount,
            BalanceAfter = balance.Available,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Description = description
        });

        await balanceRepository.SaveChangesAsync();

        logger.LogInformation("[BalanceService.CreditAsync] UserId={UserId}, Amount={Amount}", userId, amount);
    }

    public async Task DebitAsync(int userId, long amount, string referenceType, long referenceId, string description)
    {
        if (amount <= 0)
        {
            throw new AppException("출금 금액은 0보다 커야 합니다.", HttpStatusCode.BadRequest);
        }

        var affectedRows = await balanceRepository.AtomicDebitAsync(userId, amount);
        if (affectedRows == 0)
        {
            throw new AppException("잔고가 부족합니다.", HttpStatusCode.BadRequest);
        }

        var balance = await balanceRepository.GetByUserIdAsync(userId);
        if (balance == null)
        {
            throw new AppException("잔고 정보를 찾을 수 없습니다.", HttpStatusCode.InternalServerError);
        }

        balance.TotalWithdrawn += amount;

        await balanceRepository.AddTransactionAsync(new DBModel.BalanceTransaction
        {
            UserId = userId,
            Type = "DEBIT",
            Amount = amount,
            BalanceAfter = balance.Available,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Description = description
        });

        await balanceRepository.SaveChangesAsync();

        logger.LogInformation("[BalanceService.DebitAsync] UserId={UserId}, Amount={Amount}", userId, amount);
    }

    public async Task<BalanceResponseDto> AdminAdjustBalanceAsync(int userId, long amount, string reason)
    {
        if (amount == 0)
        {
            throw new AppException("조정 금액은 0이 아니어야 합니다.", HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new AppException("조정 사유를 입력해주세요.", HttpStatusCode.BadRequest);
        }

        if (amount > 0)
        {
            await CreditAsync(userId, amount, "ADMIN_ADJUST", 0, $"관리자 조정: {reason}");
        }
        else
        {
            await DebitAsync(userId, Math.Abs(amount), "ADMIN_ADJUST", 0, $"관리자 조정: {reason}");
        }

        var balance = await GetBalanceAsync(userId);
        return new BalanceResponseDto
        {
            Available = balance.Available,
            Pending = balance.Pending,
            TotalEarned = balance.TotalEarned,
            TotalWithdrawn = balance.TotalWithdrawn,
        };
    }
}
