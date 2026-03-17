using InventoryService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Services;

/// <summary>
/// Background service that runs periodically to automatically update expired batches
/// </summary>
public class ExpiredBatchUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredBatchUpdateService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

    public ExpiredBatchUpdateService(IServiceProvider serviceProvider, ILogger<ExpiredBatchUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredBatchUpdateService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var productBatchService = scope.ServiceProvider.GetRequiredService<IProductBatchService>();
                    var updatedCount = await productBatchService.UpdateExpiredBatchesAsync();
                    
                    if (updatedCount > 0)
                    {
                        _logger.LogInformation("Expired batch update completed: {Count} batches updated", updatedCount);
                    }
                    else
                    {
                        _logger.LogDebug("Expired batch update completed: no batches needed updating");
                    }
                }

                // Wait for the interval before the next execution
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ExpiredBatchUpdateService cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExpiredBatchUpdateService");
                // Wait before retrying
                await Task.Delay(_interval, stoppingToken);
            }
        }

        _logger.LogInformation("ExpiredBatchUpdateService stopped");
    }
}
