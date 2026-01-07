namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 카테고리 DTO
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// 카테고리 고유 ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 카테고리 이름 (예: "콘서트", "뮤지컬")
    /// </summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>
    /// Material Icons 이름
    /// </summary>
    public string IconName { get; set; } = null!;

    /// <summary>
    /// 표시 순서
    /// </summary>
    public int DisplayOrder { get; set; }
}
