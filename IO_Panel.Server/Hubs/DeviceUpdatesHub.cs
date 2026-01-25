using Microsoft.AspNetCore.SignalR;

namespace IO_Panel.Server.Hubs;

/// <summary>
/// SignalR hub used by the UI to receive real-time device updates.
/// </summary>
/// <remarks>
/// The server broadcasts messages (e.g., <c>deviceUpdated</c>) when external updates arrive via RabbitMQ consumers.
/// This hub currently defines no server-callable methods; it acts as a broadcast endpoint.
/// </remarks>
public sealed class DeviceUpdatesHub : Hub
{
}