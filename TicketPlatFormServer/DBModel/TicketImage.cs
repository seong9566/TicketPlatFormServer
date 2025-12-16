using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 이미지 테이블
/// </summary>
public partial class TicketImage
{
    public long Id { get; set; }

    public long TicketId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
