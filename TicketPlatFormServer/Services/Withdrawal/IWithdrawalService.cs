using TicketPlatFormServer.DTO.Balance;
using TicketPlatFormServer.DTO.Withdrawal;

namespace TicketPlatFormServer.Services.Withdrawal;

public interface IWithdrawalService
{
    Task<WithdrawalResponseDto> RequestWithdrawalAsync(int userId, WithdrawalRequestDto request, string idempotencyKey);

    Task<WithdrawalResponseDto> CancelWithdrawalAsync(int userId, long withdrawalId);

    Task<WithdrawalListResponseDto> GetWithdrawalHistoryAsync(int userId, int page, int pageSize);

    Task<BalanceResponseDto> GetBalanceAsync(int userId);

    Task<BalanceHistoryResponseDto> GetBalanceHistoryAsync(int userId, int page, int pageSize);
}
