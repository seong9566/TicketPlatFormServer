using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketPlatFormServer.Config;
using TicketPlatFormServer.Services.Settlements;

namespace TicketPlatFormServer.Services.BackgroundServices;

public class SettlementProcessingService(
    IServiceProvider serviceProvider,
    TossPaymentsSettings settings,
    ILogger<SettlementProcessingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = settings.SettlementProcessingIntervalMinutes > 0
            ? settings.SettlementProcessingIntervalMinutes
            : 5;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingSettlementsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[SettlementProcessingService] 정산 배치 실행 중 오류가 발생했습니다.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task ProcessPendingSettlementsAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var settlementService = scope.ServiceProvider.GetRequiredService<ISettlementService>();
        await settlementService.ProcessPendingSettlementsAsync();
    }
}
