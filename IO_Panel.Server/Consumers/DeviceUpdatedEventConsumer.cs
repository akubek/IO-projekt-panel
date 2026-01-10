using IO_projekt_symulator.Server.Contracts;
using IO_Panel.Server.Hubs;
using IO_Panel.Server.Repositories;
using IO_Panel.Server.Services.Automations;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace IO_Panel.Server.Consumers;

public sealed class DeviceUpdatedEventConsumer : IConsumer<DeviceUpdatedEvent>
{
    private readonly ILogger<DeviceUpdatedEventConsumer> _logger;
    private readonly IHubContext<DeviceUpdatesHub> _hubContext;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAutomationRunner _automationRunner;

    public DeviceUpdatedEventConsumer(
        ILogger<DeviceUpdatedEventConsumer> logger,
        IHubContext<DeviceUpdatesHub> hubContext,
        IDeviceRepository deviceRepository,
        IAutomationRunner automationRunner)
    {
        _logger = logger;
        _hubContext = hubContext;
        _deviceRepository = deviceRepository;
        _automationRunner = automationRunner;
    }

    public async Task Consume(ConsumeContext<DeviceUpdatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Device update received: DeviceId={DeviceId}, Value={Value}, Unit={Unit}, Malfunctioning={Malfunctioning}",
            message.DeviceId,
            message.Value,
            message.Unit,
            message.Malfunctioning);

        if (message.Value.HasValue)
        {
            await _deviceRepository.AddDeviceHistoryPointAsync(
                deviceId: message.DeviceId.ToString(),
                value: message.Value.Value,
                unit: message.Unit,
                recordedAt: DateTimeOffset.UtcNow,
                cancellationToken: context.CancellationToken);
        }

        await _automationRunner.HandleDeviceUpdatedAsync(message, context.CancellationToken);

        await _hubContext.Clients.All.SendAsync("deviceUpdated", message, context.CancellationToken);
    }
}