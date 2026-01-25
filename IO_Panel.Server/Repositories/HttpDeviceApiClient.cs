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
    /// <summary>
    /// Typed <see cref="HttpClient"/> wrapper for the external simulator device API.
    /// Provides basic logging and tolerant status handling (e.g., treating 404 as "no devices").
    /// </summary>
    public class HttpDeviceApiClient : IDeviceApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<HttpDeviceApiClient> _logger;

        public HttpDeviceApiClient(HttpClient http, ILogger<HttpDeviceApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Calls GET <c>/api/devices</c> and returns the deserialized device list.
        /// </summary>
        public async Task<IEnumerable<ApiDevice>> GetAllAsync(CancellationToken cancellation = default)
        {
            var url = new Uri(_http.BaseAddress!, "api/devices");
            _logger.LogInformation("Calling external API: GET {Url}", url);

            try
            {
                using var resp = await _http.GetAsync("api/devices", cancellation);
                _logger.LogInformation("External API returned {StatusCode}", resp.StatusCode);

                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    return Enumerable.Empty<ApiDevice>();
                }

                resp.EnsureSuccessStatusCode();

                var list = await resp.Content.ReadFromJsonAsync<IEnumerable<ApiDevice>>(cancellationToken: cancellation);
                _logger.LogInformation("Deserialized {Count} devices from external API", list?.Count() ?? 0);

                return list ?? Enumerable.Empty<ApiDevice>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling external API /api/devices");
                throw;
            }
        }

        /// <summary>
        /// Calls GET <c>/api/devices/{id}</c> and returns the deserialized device or null when not found.
        /// </summary>
        public async Task<ApiDevice?> GetByIdAsync(string id, CancellationToken cancellation = default)
        {
            using var resp = await _http.GetAsync($"api/devices/{id}", cancellation);
            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ApiDevice>(cancellationToken: cancellation);
        }

        /// <summary>
        /// Calls POST <c>/api/devices/{id}/state</c> to set device state on the simulator.
        /// Accepts 200 OK and 202 Accepted (when the simulator queues the request).
        /// </summary>
        public async Task SetStateAsync(string id, ApiDeviceState state, CancellationToken cancellation = default)
        {
            var body = new { value = state.Value, unit = state.Unit };
            using var resp = await _http.PostAsJsonAsync($"api/devices/{id}/state", body, cancellation);

            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            resp.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Calls POST <c>/api/devices/{id}/command</c> to send a generic command payload to the simulator.
        /// Accepts 200 OK and 202 Accepted (when the simulator queues the request).
        /// </summary>
        public async Task SendCommandAsync(string id, object command, CancellationToken cancellation = default)
        {
            using var resp = await _http.PostAsJsonAsync($"api/devices/{id}/command", command, cancellation);

            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Accepted)
            {
                return;
            }

            resp.EnsureSuccessStatusCode();
        }
    }
}