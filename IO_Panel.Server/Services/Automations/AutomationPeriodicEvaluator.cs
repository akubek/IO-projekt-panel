using IO_Panel.Server.Repositories;
using Microsoft.Extensions.Hosting;

namespace IO_Panel.Server.Services.Automations;

/// <summary>
/// Background worker that periodically evaluates enabled automations using the latest device snapshots.
/// </summary>
/// <remarks>
/// This is a polling-based evaluator (interval ticking) that reuses <see cref="IAutomationRunner"/> by emulating
/// device update events for devices referenced by automation conditions.
/// Cooldown tracking is in-memory and resets on application restart.
/// </remarks>
public sealed class AutomationPeriodicEvaluator : BackgroundService
{
    /// <summary>
    /// Interval between evaluation ticks.
    /// </summary>
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Minimum time between two evaluations/executions of the same automation (in-memory throttle).
    /// </summary>
    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AutomationPeriodicEvaluator> _logger;

    private readonly Dictionary<Guid, DateTimeOffset> _lastFiredAt = new();

    public AutomationPeriodicEvaluator(IServiceScopeFactory scopeFactory, ILogger<AutomationPeriodicEvaluator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Main worker loop. Ticks at a fixed interval until the host shuts down.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TickInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
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

    /// <summary>
    /// Evaluates all enabled automations once by reading the current device snapshots from the repository.
    /// </summary>
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

            var conditions = automation.Trigger?.Conditions ?? Array.Empty<Models.AutomationCondition>();
            if (conditions.Length == 0)
            {
                continue;
            }

            // Current AutomationRunner evaluates on "device updated" events.
            // To reuse that logic with periodic polling, emulate a device update per referenced device/condition.
            foreach (var condition in conditions)
            {
                var device = await deviceRepository.GetByIdAsync(condition.DeviceId);
                if (device is null)
                {
                    continue;
                }

                var value = device.State?.Value;
                var unit = device.State?.Unit;

                if (!value.HasValue && !string.Equals(unit, "bool", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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

    /// <summary>
    /// Returns <see langword="true"/> if the automation can execute now; enforces an in-memory cooldown otherwise.
    /// </summary>
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