using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 인증 제공자 코드 테이블
/// </summary>
public partial class AuthProvider
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
