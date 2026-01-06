using Application.BackgroundJobs.Options;
using Application.UseCases.Admin.Roulette;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.BackgroundJobs;

public sealed class RouletteWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RouletteWorker> _logger;
    private readonly IOptions<WorkersOptions> _options;

    private readonly SemaphoreSlim _mutex = new(1, 1);

    public RouletteWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<RouletteWorker> logger,
        IOptions<WorkersOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.Roulette.Enabled)
        {
            _logger.LogInformation("RouletteWorker disabled");
            return;
        }

        var tick = TimeSpan.FromSeconds(Math.Max(1, _options.Value.Roulette.TickSeconds));
        var guildId = string.IsNullOrWhiteSpace(_options.Value.DefaultGuildId) ? "default" : _options.Value.DefaultGuildId;

        _logger.LogInformation("RouletteWorker started (tick={Tick}s, guildId={GuildId})",
            tick.TotalSeconds, guildId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(tick, stoppingToken);

                if (!await _mutex.WaitAsync(0, stoppingToken))
                    continue;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var ban = scope.ServiceProvider.GetRequiredService<RunBanRouletteHandler>();
                    var ticket = scope.ServiceProvider.GetRequiredService<RunTicketRouletteHandler>();

                    var banRes = await ban.HandleAsync(new RunBanRouletteCommand(guildId, "System"), stoppingToken);
                    if (banRes.Ran)
                        _logger.LogInformation("Ban roulette ran: bucket={Bucket}, picked={Picked}", banRes.Bucket, banRes.PickedCount);

                    var ticketRes = await ticket.HandleAsync(new RunTicketRouletteCommand(guildId, "System"), stoppingToken);
                    if (ticketRes.Ran)
                        _logger.LogInformation("Ticket roulette ran: bucket={Bucket}, picked={Picked}", ticketRes.Bucket, ticketRes.PickedCount);
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
                _logger.LogError(ex, "RouletteWorker iteration failed");
            }
        }
    }

    public override void Dispose()
    {
        _mutex.Dispose();
        base.Dispose();
    }
}
