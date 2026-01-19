namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 거래 방식 응답 DTO
/// </summary>
public class TradeMethodRespDto
{
    /// <summary>
    /// 거래 방식 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 거래 방식 코드 (예: pin_trade, delivery)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 거래 방식 한글 이름
    /// </summary>
    public string NameKo { get; set; } = string.Empty;

    /// <summary>
    /// 거래 방식 영어 이름
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// 거래 방식 설명
    /// </summary>
    public string? Description { get; set; }
}
