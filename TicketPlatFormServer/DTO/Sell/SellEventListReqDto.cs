namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 판매용 공연 목록 조회 요청 DTO
/// </summary>
public class SellEventListReqDto
{
    /// <summary>
    /// 카테고리 ID (필수)
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 검색 키워드 (선택)
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 페이지 번호 (1부터 시작, 기본값: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 페이지 크기 (기본값: 20)
    /// </summary>
    public int Size { get; set; } = 20;
}
