using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 좌석 등급 코드 테이블
/// </summary>
public partial class SeatGrade
{
    public int Id { get; set; }

    /// <summary>
    /// 좌석 등급 코드 (예: VIP, R, S, A)
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
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
