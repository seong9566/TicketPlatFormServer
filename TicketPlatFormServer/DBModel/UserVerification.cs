using System;
using System.Collections.Generic;

namespace TicketPlatFormServer.DBModel;

public partial class UserVerification
{
    public long UserId { get; set; }

    public string? Name { get; set; }

    public DateOnly? Birth { get; set; }

    public bool? IdentityVerified { get; set; }

    public bool? PhoneVerified { get; set; }

    public bool? AccountVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
