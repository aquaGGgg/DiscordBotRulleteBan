using Application.BackgroundJobs.Options;
using Application.UseCases.Internal.Punishments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.BackgroundJobs;

public sealed class ExpirePunishmentsWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpirePunishmentsWorker> _logger;
    private readonly IOptions<WorkersOptions> _options;

    private readonly SemaphoreSlim _mutex = new(1, 1);

    public ExpirePunishmentsWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpirePunishmentsWorker> logger,
        IOptions<WorkersOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.ExpirePunishments.Enabled)
        {
            _logger.LogInformation("ExpirePunishmentsWorker disabled");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.Value.ExpirePunishments.IntervalSeconds));
        var batchSize = _options.Value.ExpirePunishments.BatchSize;

        _logger.LogInformation("ExpirePunishmentsWorker started (interval={Interval}s, batch={Batch})",
            interval.TotalSeconds, batchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken);

                if (!await _mutex.WaitAsync(0, stoppingToken))
                    continue;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<ExpirePunishmentsHandler>();

                    var res = await handler.HandleAsync(new ExpirePunishmentsCommand(batchSize), stoppingToken);
                    if (res.ExpiredCount > 0)
                        _logger.LogInformation("Expired punishments processed: {Count}", res.ExpiredCount);
                }
                finally
                {
                    _mutex.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExpirePunishmentsWorker iteration failed");
                // не падаем, at-least-once по следующему тикающему циклу
            }
        }
    }

    public override void Dispose()
    {
        _mutex.Dispose();
        base.Dispose();
    }
}
