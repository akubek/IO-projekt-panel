using IO_projekt_symulator.Server.Contracts;

namespace IO_Panel.Server.Services.Automations;

/// <summary>
/// Executes automations in response to device updates.
/// </summary>
public interface IAutomationRunner
{
    /// <summary>
    /// Handles an incoming device update event and evaluates any enabled automations that reference the device.
    /// </summary>
    Task HandleDeviceUpdatedAsync(DeviceUpdatedEvent message, CancellationToken cancellationToken);
}