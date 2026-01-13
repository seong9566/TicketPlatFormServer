namespace TicketPlatFormServer.Config;

public class StorageProviderSettings
{
    public string ActiveProvider { get; set; } = "Supabase";
    public bool EnableFallback { get; set; } = true;
}
