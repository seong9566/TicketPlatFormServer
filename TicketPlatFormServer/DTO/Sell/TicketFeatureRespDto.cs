namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 티켓 특이사항 응답 DTO
/// </summary>
public class TicketFeatureRespDto
{
    /// <summary>
    /// 특이사항 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 특이사항 코드
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 한글명
    /// </summary>
    public string NameKo { get; set; } = null!;

    /// <summary>
    /// 설명
    /// </summary>
    public string? Description { get; set; }
}
