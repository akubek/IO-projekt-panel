using IO_Panel.Server.Repositories;
using Microsoft.Extensions.Hosting;

namespace IO_Panel.Server.Services.Automations;

public sealed class AutomationPeriodicEvaluator : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationPeriodicEvaluator> _logger;

    // in-memory last-fired; good enough for dev; for production persist to DB
    private readonly Dictionary<Guid, DateTimeOffset> _lastFiredAt = new();

    public AutomationPeriodicEvaluator(IServiceScopeFactory scopeFactory, ILogger<AutomationPeriodicEvaluator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Periodic automation evaluator tick ({IntervalSeconds}s).", TickInterval.TotalSeconds);
                await EvaluateOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Periodic automation evaluation failed.");
            }
        }
    }

    private async Task EvaluateOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var automationRepository = scope.ServiceProvider.GetRequiredService<IAutomationRepository>();
        var deviceRepository = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
        var runner = scope.ServiceProvider.GetRequiredService<IAutomationRunner>();

        var automations = await automationRepository.GetAllAsync();

        foreach (var automation in automations)
        {
            if (!automation.IsEnabled)
            {
                continue;
            }

            if (!TryEnterCooldown(automation.Id))
            {
                continue;
            }

            // Evaluate conditions using latest device state(s).
            // Current AutomationRunner only evaluates against an updated-device event,
            // so we simulate an event per referenced device.
            var conditions = automation.Trigger?.Conditions ?? Array.Empty<Models.AutomationCondition>();
            if (conditions.Length == 0)
            {
                continue;
            }

            foreach (var condition in conditions)
            {
                var device = await deviceRepository.GetByIdAsync(condition.DeviceId);
                if (device is null)
                {
                    continue;
                }

                // Use latest known state from the device entity (or history if you store it separately)
                var value = device.State?.Value;
                var unit = device.State?.Unit;

                if (!value.HasValue && !string.Equals(unit, "bool", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Let the runner apply the same evaluation/execute path by emulating a device update.
                await runner.HandleDeviceUpdatedAsync(new IO_projekt_symulator.Server.Contracts.DeviceUpdatedEvent
                {
                    DeviceId = Guid.Parse(device.Id),
                    Value = value,
                    Unit = unit,
                    Malfunctioning = device.Malfunctioning
                }, cancellationToken);
            }
        }
    }

    private bool TryEnterCooldown(Guid automationId)
    {
        var now = DateTimeOffset.UtcNow;

        if (_lastFiredAt.TryGetValue(automationId, out var last) && now - last < Cooldown)
        {
            return false;
        }

        _lastFiredAt[automationId] = now;
        return true;
    }
}