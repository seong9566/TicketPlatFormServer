using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 티켓 검증 방법 코드 테이블
/// </summary>
public partial class TicketVerificationMethod
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<TicketVerification> TicketVerifications { get; set; } = new List<TicketVerification>();
}
