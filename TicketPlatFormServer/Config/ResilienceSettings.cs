namespace TicketPlatFormServer.Config;

public class ResilienceSettings
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int InitialRetryDelayMs { get; set; } = 1000;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSec { get; set; } = 30;
}
