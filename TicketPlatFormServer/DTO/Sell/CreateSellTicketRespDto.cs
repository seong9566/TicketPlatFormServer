namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 티켓 판매 등록 응답 DTO
/// </summary>
public class CreateSellTicketRespDto
{
    /// <summary>
    /// 생성된 티켓 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 티켓 상태
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 메시지
    /// </summary>
    public string Message { get; set; } = null!;
}
