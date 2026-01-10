using IO_projekt_symulator.Server.Contracts;
using IO_Panel.Server.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace IO_Panel.Server.Consumers;

public sealed class DeviceUpdatedEventConsumer : IConsumer<DeviceUpdatedEvent>
{
    private readonly ILogger<DeviceUpdatedEventConsumer> _logger;
    private readonly IHubContext<DeviceUpdatesHub> _hubContext;

    public DeviceUpdatedEventConsumer(
        ILogger<DeviceUpdatedEventConsumer> logger,
        IHubContext<DeviceUpdatesHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
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

        await _hubContext.Clients.All.SendAsync("deviceUpdated", message, context.CancellationToken);
    }
}