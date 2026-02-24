namespace TicketPlatFormServer.Repository.ReadModels;

public class PaymentPreviewReadModel
{
    public int? TicketId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? SeatInfo { get; set; }
    public int? Quantity { get; set; }
    public int? UnitPrice { get; set; }
    public int? TotalAmount { get; set; }
    public int? EventId { get; set; }
    public string? EventTitle { get; set; }
    public DateTime? EventDateTime { get; set; }
    public string? VenueName { get; set; }
}
