namespace TicketPlatFormServer.Repository.ReadModels;

/// <summary>
/// 정산 상세 조회 ReadModel (Dapper 매핑용)
/// </summary>
public class SettlementDetailReadModel : SettlementListReadModel
{
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountHolder { get; set; }
    public string? BuyerNickname { get; set; }
}
