namespace TicketPlatFormServer.DTO;

/// <summary>
/// 공연/이벤트 목록 조회 RespDto
/// </summary>
public class EventListRespDto
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public string? EventPosterImageUrl { get; set; }
    
    /// <summary>
    /// 공연 시작 날짜
    /// </summary>
    public DateTime? StartAt { get; set; }
    
    /// <summary>
    /// 공연 종료 날짜
    /// </summary>
    public DateTime? EndAt { get; set; }
    
    /// <summary>
    /// 장소명
    /// </summary>
    public string? VenueName { get; set; }
    
    public int? ArtistId { get; set; }
    public string? ArtistName { get; set; }
    public string? ArtistProfileImageUrl { get; set; }
    public DateTime EventCreatedAt { get; set; }
    public bool IsNew { get; set; }
}

