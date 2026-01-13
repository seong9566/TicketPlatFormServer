namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 내 판매 티켓 목록 조회 요청 DTO
/// </summary>
public class MyTicketListReqDto
{
    /// <summary>
    /// 상태 필터 (선택)
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
