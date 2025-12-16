using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 이벤트/공연 정보 테이블
/// </summary>
public partial class Event
{
    public long Id { get; set; }

    /// <summary>
    /// 카테고리 FK
    /// </summary>
    public long CategoryId { get; set; }

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
    /// 등록 관리자 FK
    /// </summary>
    public long? CreatedByAdminId { get; set; }

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

    public virtual TicketCategory Category { get; set; } = null!;

    public virtual User? CreatedByAdmin { get; set; }

    public virtual ICollection<EventSession> EventSessions { get; set; } = new List<EventSession>();
}
