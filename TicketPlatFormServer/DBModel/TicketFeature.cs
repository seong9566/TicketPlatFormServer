using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 특징 코드 테이블
/// </summary>
public partial class TicketFeature
{
    public int Id { get; set; }

    /// <summary>
    /// 특징 코드 (예: CONSECUTIVE_SEATS, AISLE_SEAT, GOOD_VIEW)
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 한글 이름
    /// </summary>
    public string NameKo { get; set; } = null!;

    /// <summary>
    /// 영문 이름
    /// </summary>
    public string NameEn { get; set; } = null!;

    /// <summary>
    /// 특징 설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 아이콘 이름 (UI용)
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<TicketTicketFeature> TicketTicketFeatures { get; set; } = new List<TicketTicketFeature>();
}
