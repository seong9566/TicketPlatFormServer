namespace TicketPlatFormServer.DTO.Balance;

public class BalanceResponseDto
{
    public long Available { get; set; }

    public long Pending { get; set; }

    public long TotalEarned { get; set; }

    public long TotalWithdrawn { get; set; }
}
