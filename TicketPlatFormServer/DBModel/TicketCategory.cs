using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 카테고리 코드 테이블
/// </summary>
public partial class TicketCategory
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameKo { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
