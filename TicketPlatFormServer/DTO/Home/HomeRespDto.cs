namespace TicketPlatFormServer.DTO.Home;

/// <summary>
/// 홈 화면 응답 DTO
/// </summary>
public class HomeRespDto
{
    /// <summary>
    /// 인기 티켓 목록
    /// </summary>
    public List<PopularTicketDto> PopularTickets { get; set; } = new();
    
    /// <summary>
    /// 추천 이벤트 목록 (Just for you)
    /// </summary>
    public List<RecommendedEventDto> RecommendedEvents { get; set; } = new();
}

