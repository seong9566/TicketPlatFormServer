using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 평판 평가 유형 코드 테이블
/// </summary>
public partial class ReputationRatingType
{
    public long Id { get; set; }

    public string Code { get; set; } = null!;

    public string? NameKo { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<UserReputation> UserReputations { get; set; } = new List<UserReputation>();
}
