namespace TicketPlatFormServer.DTO.Balance;

public class BalanceTransactionDto
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public long Amount { get; set; }

    public long BalanceAfter { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }
}
