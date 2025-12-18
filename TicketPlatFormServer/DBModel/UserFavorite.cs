using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

/// <summary>
/// 사용자 찜 테이블
/// </summary>
public partial class UserFavorite
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int FavoriteTypeId { get; set; }

    public int TargetId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual FavoriteType FavoriteType { get; set; } = null!;
}
