using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 알림 플랫폼 코드 테이블
/// </summary>
public partial class NotificationPlatform
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<NotificationToken> NotificationTokens { get; set; } = new List<NotificationToken>();
}
