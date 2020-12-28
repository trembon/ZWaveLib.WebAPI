using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZWaveLib.WebAPI.Services
{
    public interface IWebhookService
    {
        Task SendHealProgressWebHook(DateTime timestamp, HealStatus status);

        Task SendNodeUpdateWebHook(byte nodeId, DateTime timestamp, EventParameter parameter, object value);

        Task SendNodeOperationProgressWebHook(byte nodeId, DateTime timestamp, NodeQueryStatus status);

        Task SendDiscoveryProgressWebHook(DateTime timestamp, DiscoveryStatus status);

        Task SendControllerStatusWebHook(DateTime timestamp, ControllerStatus status);
    }

    public class WebhookService : IWebhookService, IDisposable
    {
        private HttpClient httpClient;

        private IConfiguration configuration;
        private ILogger<WebhookService> logger;

        public WebhookService(IConfiguration configuration, ILogger<WebhookService> logger)
        {
            httpClient = new HttpClient();

            this.logger = logger;
            this.configuration = configuration;
        }

        public void Dispose()
        {
            try
            {
                httpClient?.Dispose();
                httpClient = null;
            }
            catch { }
        }

        public Task SendHealProgressWebHook(DateTime timestamp, HealStatus status)
        {
            string jsonData = JsonSerializer.Serialize(new { status, timestamp });
            return SendWebHooks(jsonData, "Webhooks:HealProgress");
        }

        public Task SendNodeUpdateWebHook(byte nodeId, DateTime timestamp, EventParameter valueType, object value)
        {
            string jsonData = JsonSerializer.Serialize(new { nodeId, timestamp, valueType, value });
            return SendWebHooks(jsonData, "Webhooks:NodeUpdate");
        }

        public Task SendNodeOperationProgressWebHook(byte nodeId, DateTime timestamp, NodeQueryStatus status)
        {
            string jsonData = JsonSerializer.Serialize(new { nodeId, timestamp, status });
            return SendWebHooks(jsonData, "Webhooks:NodeOperationProgress");
        }

        public Task SendDiscoveryProgressWebHook(DateTime timestamp, DiscoveryStatus status)
        {
            string jsonData = JsonSerializer.Serialize(new { timestamp, status });
            return SendWebHooks(jsonData, "Webhooks:DiscoveryProgress");
        }

        public Task SendControllerStatusWebHook(DateTime timestamp, ControllerStatus status)
        {
            string jsonData = JsonSerializer.Serialize(new { timestamp, status });
            return SendWebHooks(jsonData, "Webhooks:ControllerStatus");
        }

        private async Task SendWebHooks(string jsonData, string configurationKey)
        {
            foreach (string url in configuration.GetSection(configurationKey).Get<string[]>())
            {
                try
                {
                    bool result = await SendData(url, jsonData);
                    if (!result)
                        throw new Exception("Invalid response from url");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to send sensor data to webhook: {url}");
                }
            }
        }

        private async Task<bool> SendData(string url, string jsonData)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}
