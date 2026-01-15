namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 티켓 이미지 DTO
/// </summary>
public class TicketImageDto
{
    /// <summary>
    /// 이미지 ID
    /// </summary>
    public long ImageId { get; set; }

    /// <summary>
    /// 이미지 Signed URL
    /// </summary>
    public string ImageUrl { get; set; } = null!;

    /// <summary>
    /// URL 만료 시간
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}
