namespace TicketPlatFormServer.DTO.Withdrawal;

public class WithdrawalListResponseDto
{
    public List<WithdrawalResponseDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
