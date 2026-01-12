namespace TicketPlatFormServer.Config;

public class ChatSettings
{
    public int MessageRetentionDays { get; set; } = 90;
    public int CleanupIntervalHours { get; set; } = 24;
}
