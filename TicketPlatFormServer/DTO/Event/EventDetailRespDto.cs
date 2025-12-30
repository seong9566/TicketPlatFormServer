namespace TicketPlatFormServer.DTO;

/// <summary>
/// 이벤트 상세 정보 조회 RespDto
/// </summary>
public class EventDetailRespDto
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public string? EventPosterImageUrl { get; set; }
    
    /// <summary>
    /// 공연 시작 날짜/시간
    /// </summary>
    public DateTime? StartAt { get; set; }
    
    /// <summary>
    /// 공연 종료 날짜/시간
    /// </summary>
    public DateTime? EndAt { get; set; }
    
    /// <summary>
    /// 장소명
    /// </summary>
    public string? VenueName { get; set; }
    
    /// <summary>
    /// 장소 주소
    /// </summary>
    public string? VenueAddress { get; set; }
    
    /// <summary>
    /// 아티스트 ID (콘서트인 경우)
    /// </summary>
    public int? ArtistId { get; set; }
    
    /// <summary>
    /// 아티스트명 (콘서트인 경우)
    /// </summary>
    public string? ArtistName { get; set; }
    
    /// <summary>
    /// 매진 임박 여부 (remaining_quantity가 5개 이하인 티켓이 있는지)
    /// </summary>
    public bool IsSoldOutImminent { get; set; }
    
    /// <summary>
    /// 좌석 타입별 필터 정보
    /// </summary>
    public List<SeatTypeFilterDto> SeatTypeFilters { get; set; } = new();
    
    /// <summary>
    /// 판매 티켓 목록
    /// </summary>
    public List<TicketListRespDto> Tickets { get; set; } = new();
}
