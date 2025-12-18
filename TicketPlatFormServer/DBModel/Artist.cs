using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class Artist
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ArtistFollower> ArtistFollowers { get; set; } = new List<ArtistFollower>();

    public virtual TicketCategory Category { get; set; } = null!;

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
