namespace TicketPlatFormServer.Services.Notification;

public interface IFcmService
{
    Task SendToUserAsync(long userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default);
}
