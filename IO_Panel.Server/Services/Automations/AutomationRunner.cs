using IO_projekt_symulator.Server.Contracts;
using IO_Panel.Server.Models;
using IO_Panel.Server.Repositories;
using MassTransit;

namespace IO_Panel.Server.Services.Automations;

public sealed class AutomationRunner : IAutomationRunner
{
    private readonly ILogger<AutomationRunner> _logger;
    private readonly IAutomationRepository _automationRepository;
    private readonly ISceneRepository _sceneRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public AutomationRunner(
        ILogger<AutomationRunner> logger,
        IAutomationRepository automationRepository,
        ISceneRepository sceneRepository,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _automationRepository = automationRepository;
        _sceneRepository = sceneRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task HandleDeviceUpdatedAsync(DeviceUpdatedEvent message, CancellationToken cancellationToken)
    {
        var all = await _automationRepository.GetAllAsync();

        var deviceId = message.DeviceId.ToString();

        foreach (var automation in all)
        {
            if (!automation.IsEnabled)
            {
                continue;
            }

            if (!ReferencesDevice(automation.Trigger, deviceId))
            {
                continue;
            }

            if (!IsTimeWindowSatisfied(automation.Trigger.TimeWindow, DateTimeOffset.Now.TimeOfDay))
            {
                continue;
            }

            if (!TryGetComparableValue(automation.Trigger.Conditions, deviceId, message.Value, message.Unit, out var comparableValue))
            {
                continue;
            }

            if (!AreConditionsSatisfied(automation.Trigger.Conditions, deviceId, comparableValue, message.Unit))
            {
                continue;
            }

            await ExecuteAsync(automation, cancellationToken);
        }
    }

    private static bool ReferencesDevice(AutomationTrigger trigger, string deviceId)
        => (trigger.Conditions ?? Array.Empty<AutomationCondition>())
            .Any(c => string.Equals(c.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase));

    private static bool IsTimeWindowSatisfied(TimeOfDayWindow? window, TimeSpan nowLocalTimeOfDay)
    {
        if (window is null)
        {
            return true;
        }

        var now = TimeOnly.FromTimeSpan(nowLocalTimeOfDay);

        var from = window.From;
        var to = window.To;

        if (!window.WrapMidnight || from <= to)
        {
            return now >= from && now <= to;
        }

        return now >= from || now <= to;
    }

    private static bool TryGetComparableValue(
        AutomationCondition[]? conditions,
        string updatedDeviceId,
        double? updatedValue,
        string? updatedUnit,
        out double comparableValue)
    {
        comparableValue = default;

        if (updatedValue.HasValue)
        {
            comparableValue = updatedValue.Value;
            return true;
        }

        // If the incoming event doesn't include a value, allow boolean triggers to still work.
        // We infer "On" as 1 if either the event unit or the condition unit is "bool".
        var cond = (conditions ?? Array.Empty<AutomationCondition>())
            .FirstOrDefault(c => string.Equals(c.DeviceId, updatedDeviceId, StringComparison.OrdinalIgnoreCase));

        var expectsBool = IsBoolUnit(updatedUnit) || IsBoolUnit(cond?.Unit);

        if (!expectsBool)
        {
            return false;
        }

        comparableValue = 1;
        return true;
    }

    private static bool IsBoolUnit(string? unit)
        => string.Equals(unit?.Trim(), "bool", StringComparison.OrdinalIgnoreCase);

    private static bool AreConditionsSatisfied(
        AutomationCondition[]? conditions,
        string updatedDeviceId,
        double updatedValue,
        string? updatedUnit)
    {
        var conds = conditions ?? Array.Empty<AutomationCondition>();
        if (conds.Length == 0)
        {
            return false;
        }

        foreach (var condition in conds)
        {
            if (!string.Equals(condition.DeviceId, updatedDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!IsUnitCompatible(condition.Unit, updatedUnit))
            {
                return false;
            }

            if (!Compare(updatedValue, condition.Operator, condition.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsUnitCompatible(string? expected, string? actual)
    {
        if (string.IsNullOrWhiteSpace(expected))
        {
            return true;
        }

        // Allow "bool" triggers even if the event forgets to send the unit.
        if (string.Equals(expected.Trim(), "bool", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(actual))
        {
            return true;
        }

        return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
    }

    private static bool Compare(double left, AutomationComparisonOperator op, double right)
    {
        const double eps = 1e-9;

        return op switch
        {
            AutomationComparisonOperator.Equal => Math.Abs(left - right) <= eps,
            AutomationComparisonOperator.GreaterThan => left > right,
            AutomationComparisonOperator.GreaterThanOrEqual => left > right || Math.Abs(left - right) <= eps,
            AutomationComparisonOperator.LessThan => left < right,
            AutomationComparisonOperator.LessThanOrEqual => left < right || Math.Abs(left - right) <= eps,
            _ => false
        };
    }

    private async Task ExecuteAsync(Automation automation, CancellationToken cancellationToken)
    {
        switch (automation.Action.Kind)
        {
            case AutomationActionKind.SetDeviceState:
            {
                if (string.IsNullOrWhiteSpace(automation.Action.DeviceId) ||
                    !Guid.TryParse(automation.Action.DeviceId, out var deviceId) ||
                    !automation.Action.TargetValue.HasValue)
                {
                    _logger.LogWarning("Automation {AutomationId} has invalid SetDeviceState action.", automation.Id);
                    return;
                }

                var command = new SetDeviceStateCommand
                {
                    DeviceId = deviceId,
                    Value = automation.Action.TargetValue.Value,
                    Unit = automation.Action.TargetUnit
                };

                await _publishEndpoint.Publish(command, cancellationToken);
                _logger.LogInformation("Automation {AutomationId} executed: SetDeviceState {DeviceId}={Value}{Unit}.",
                    automation.Id, automation.Action.DeviceId, command.Value, command.Unit);

                break;
            }

            case AutomationActionKind.RunScene:
            {
                if (!automation.Action.SceneId.HasValue)
                {
                    _logger.LogWarning("Automation {AutomationId} has invalid RunScene action.", automation.Id);
                    return;
                }

                var scene = await _sceneRepository.GetByIdAsync(automation.Action.SceneId.Value);
                if (scene is null)
                {
                    _logger.LogWarning("Automation {AutomationId} references missing SceneId={SceneId}.",
                        automation.Id, automation.Action.SceneId.Value);
                    return;
                }

                foreach (var action in scene.Actions)
                {
                    if (!Guid.TryParse(action.DeviceId, out var deviceId))
                    {
                        _logger.LogWarning("Scene {SceneId} contains invalid DeviceId={DeviceId}.", scene.Id, action.DeviceId);
                        continue;
                    }

                    await _publishEndpoint.Publish(new SetDeviceStateCommand
                    {
                        DeviceId = deviceId,
                        Value = action.TargetState.Value,
                        Unit = action.TargetState.Unit
                    }, cancellationToken);
                }

                _logger.LogInformation("Automation {AutomationId} executed: RunScene {SceneId}.", automation.Id, scene.Id);
                break;
            }

            default:
                _logger.LogWarning("Automation {AutomationId} has unknown action kind {Kind}.", automation.Id, automation.Action.Kind);
                break;
        }
    }
}