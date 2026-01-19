namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 정가 조회 요청 DTO
/// </summary>
public class GetOriginalPriceReqDto
{
    /// <summary>
    /// 공연 ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 좌석 등급 ID
    /// </summary>
    public int GradeId { get; set; }

    /// <summary>
    /// 좌석 위치 ID (옵션)
    /// </summary>
    public int? LocationId { get; set; }

    /// <summary>
    /// 좌석 구역 ID (옵션)
    /// </summary>
    public int? AreaId { get; set; }
}
