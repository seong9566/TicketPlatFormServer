namespace TicketPlatFormServer.DBModel;

public partial class ChatRoom
{
    public virtual Ticket Ticket { get; set; } = null!;
    public virtual User Buyer { get; set; } = null!;
    public virtual User Seller { get; set; } = null!;
}
