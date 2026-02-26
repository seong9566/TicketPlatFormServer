using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.DTO.Withdrawal;

namespace TicketPlatFormServer.Services.Withdrawal;

public interface IWithdrawalService
{
    Task<WithdrawalResponseDto> RequestWithdrawalAsync(long userId, WithdrawalRequestDto request, string idempotencyKey);

    Task<WithdrawalResponseDto> CancelWithdrawalAsync(long userId, long withdrawalId);

    Task<WithdrawalListResponseDto> GetWithdrawalHistoryAsync(long userId, int page, int pageSize);

    Task<BalanceResponseDto> GetBalanceAsync(long userId);

    Task<BalanceHistoryResponseDto> GetBalanceHistoryAsync(long userId, int page, int pageSize);
}
