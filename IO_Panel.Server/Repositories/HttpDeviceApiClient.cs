using IO_Panel.Server.Repositories.Entities;
using System.Net;
using System.Net.Http.Json;

namespace IO_Panel.Server.Repositories
{
    public class HttpDeviceApiClient : IDeviceApiClient
    {
        private readonly HttpClient _http;

        public HttpDeviceApiClient(HttpClient http) => _http = http;

        public async Task<IEnumerable<ApiDevice>> GetAllAsync(CancellationToken cancellation = default)
        {
            var list = await _http.GetFromJsonAsync<IEnumerable<ApiDevice>>("api/devices", cancellation);
            return list ?? Enumerable.Empty<ApiDevice>();
        }

        public async Task<ApiDevice?> GetByIdAsync(string id, CancellationToken cancellation = default)
        {
            using var resp = await _http.GetAsync($"api/devices/{id}", cancellation);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ApiDevice>(cancellationToken: cancellation);
        }

        public async Task SetStateAsync(string id, ApiDeviceState state, CancellationToken cancellation = default)
        {
            var body = new { value = state.Value, unit = state.Unit };
            using var resp = await _http.PostAsJsonAsync($"api/devices/{id}/state", body, cancellation);

            // Accept 200 OK or 202 Accepted (server may queue)
            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted)
                return;

            resp.EnsureSuccessStatusCode();
        }

        public async Task SendCommandAsync(string id, object command, CancellationToken cancellation = default)
        {
            using var resp = await _http.PostAsJsonAsync($"api/devices/{id}/command", command, cancellation);

            // Accept 200 OK or 202 Accepted (server may queue)
            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted)
                return;

            resp.EnsureSuccessStatusCode();
        }
    }
}