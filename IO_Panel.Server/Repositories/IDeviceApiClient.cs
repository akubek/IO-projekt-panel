using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    /// <summary>
    /// Wraps HTTP access to the external simulator device API (enumerate devices, fetch details, send commands/state).
    /// </summary>
    public interface IDeviceApiClient
    {
        /// <summary>
        /// Returns all devices from the simulator API.
        /// </summary>
        Task<IEnumerable<ApiDevice>> GetAllAsync(CancellationToken cancellation = default);

        /// <summary>
        /// Returns a single device from the simulator API, or null when not found.
        /// </summary>
        Task<ApiDevice?> GetByIdAsync(string id, CancellationToken cancellation = default);

        /// <summary>
        /// Sends a generic command payload to the simulator API.
        /// </summary>
        Task SendCommandAsync(string id, object command, CancellationToken cancellation = default);

        /// <summary>
        /// Sets device state via simulator API (POST /api/devices/{id}/state).
        /// </summary>
        Task SetStateAsync(string id, ApiDeviceState state, CancellationToken cancellation = default);
    }
}