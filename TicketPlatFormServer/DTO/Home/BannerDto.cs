namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 배너 DTO
/// </summary>
public class BannerDto
{
    /// <summary>
    /// 배너 고유 ID
    /// </summary>
    public int BannerId { get; set; }

    /// <summary>
    /// 배너 제목
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 배너 이미지 URL (권장 비율: 16:9)
    /// </summary>
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// 클릭 시 이동할 URL (null이면 클릭 불가)
    /// </summary>
    public string? LinkUrl { get; set; }

    /// <summary>
    /// 표시 순서 (오름차순)
    /// </summary>
    public int DisplayOrder { get; set; }
}
