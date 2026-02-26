namespace TicketPlatFormServer.DTO.Admin;

public class AdminAdjustBalanceRequestDto
{
    public long Amount { get; set; }

    public string Reason { get; set; } = null!;
}
