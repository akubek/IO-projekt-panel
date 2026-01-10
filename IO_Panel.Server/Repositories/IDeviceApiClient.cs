using IO_Panel.Server.Models;

namespace IO_Panel.Server.Repositories
{
    public interface IDeviceApiClient
    {
        Task<IEnumerable<ApiDevice>> GetAllAsync(CancellationToken cancellation = default);
        Task<ApiDevice?> GetByIdAsync(string id, CancellationToken cancellation = default);
        Task SendCommandAsync(string id, object command, CancellationToken cancellation = default);

        // POST /api/devices/{id}/state
        Task SetStateAsync(string id, ApiDeviceState state, CancellationToken cancellation = default);
    }
}