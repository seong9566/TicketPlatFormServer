using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 이벤트/공연 정보 테이블
/// </summary>
public partial class Event
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    /// <summary>
    /// 아티스트 FK (콘서트 카테고리만)
    /// </summary>
    public int? ArtistId { get; set; }

    /// <summary>
    /// 공연/이벤트 제목
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 포스터 이미지 URL
    /// </summary>
    public string? PosterImageUrl { get; set; }

    /// <summary>
    /// 장소명
    /// </summary>
    public string? VenueName { get; set; }

    /// <summary>
    /// 장소 주소
    /// </summary>
    public string? VenueAddress { get; set; }

    /// <summary>
    /// 공연 시작 시간
    /// </summary>
    public DateTime? StartAt { get; set; }

    /// <summary>
    /// 공연 종료 시간
    /// </summary>
    public DateTime? EndAt { get; set; }

    public int? CreatedByAdminId { get; set; }

    /// <summary>
    /// 활성화 여부
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Artist? Artist { get; set; }

    public virtual TicketCategory Category { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
