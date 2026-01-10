using IO_projekt_symulator.Server.Contracts;

namespace IO_Panel.Server.Services.Automations;

public interface IAutomationRunner
{
    Task HandleDeviceUpdatedAsync(DeviceUpdatedEvent message, CancellationToken cancellationToken);
}