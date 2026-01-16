using System;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 거래 방법 코드 테이블
/// </summary>
public partial class TradeMethod
{
    public int Id { get; set; }

    /// <summary>
    /// 거래 방법 코드 (예: DIRECT, DELIVERY, ELECTRONIC)
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
    /// 거래 방법 설명
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 정렬 순서
    /// </summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
