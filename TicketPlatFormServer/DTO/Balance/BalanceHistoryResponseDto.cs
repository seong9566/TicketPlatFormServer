namespace TicketPlatFormServer.DTO.Balance;

public class BalanceHistoryResponseDto
{
    public List<BalanceTransactionDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
