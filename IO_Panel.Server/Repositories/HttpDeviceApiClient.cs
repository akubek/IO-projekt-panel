using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using IO_Panel.Server.Models;
using Microsoft.Extensions.Logging;

namespace IO_Panel.Server.Repositories
{
    public class HttpDeviceApiClient : IDeviceApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpDeviceApiClient> _logger;

        public HttpDeviceApiClient(HttpClient http, ILogger<HttpDeviceApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<IEnumerable<ApiDevice>> GetAllAsync(CancellationToken cancellation = default)
        {
            var url = new Uri(_http.BaseAddress!, "api/devices");
            _logger.LogInformation("Calling external API: GET {Url}", url);
            try
            {
                using var resp = await _http.GetAsync("api/devices", cancellation);
                _logger.LogInformation("External API returned {StatusCode}", resp.StatusCode);
                if (resp.StatusCode == HttpStatusCode.NotFound) return Enumerable.Empty<ApiDevice>();
                resp.EnsureSuccessStatusCode();
                var list = await resp.Content.ReadFromJsonAsync<IEnumerable<ApiDevice>>(cancellationToken : cancellation);
                _logger.LogInformation("Deserialized {Count} devices from external API", list?.Count() ?? 0);
                return list ?? Enumerable.Empty<ApiDevice>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling external API /api/devices");
                throw;
            }
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