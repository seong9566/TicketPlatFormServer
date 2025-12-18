using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 찜 유형 코드 테이블
/// </summary>
public partial class FavoriteType
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameKo { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();
}
