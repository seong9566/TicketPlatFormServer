namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 카테고리 응답 DTO
/// </summary>
public class CategoryRespDto
{
    /// <summary>
    /// 카테고리 ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 카테고리 코드
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 카테고리 한글명
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 아이콘 URL
    /// </summary>
    public string? IconUrl { get; set; }
}
