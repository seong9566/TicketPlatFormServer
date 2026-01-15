namespace TicketPlatFormServer.DTO.Sell;

/// <summary>
/// 티켓 이미지 URL 재발급 응답 DTO
/// </summary>
public class RefreshTicketImageUrlRespDto
{
    /// <summary>
    /// 이미지 목록 (Signed URL 갱신됨)
    /// </summary>
    public List<TicketImageDto> Images { get; set; } = new();
}
