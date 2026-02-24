namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 판매 대시보드 조회 요청 DTO
/// </summary>
public class SalesDashboardReqDto
{
    /// <summary>
    /// 상태 필터 (all, on_sale, completed, settling)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 페이지 번호 (1부터 시작, 기본값: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 페이지 크기 (기본값: 20)
    /// </summary>
    public int Size { get; set; } = 20;
}
