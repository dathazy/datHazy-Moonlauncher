using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using datHazy_Moonlauncher.Models;

namespace datHazy_Moonlauncher.Services
{
    public static class ConfigService
    {
        private static readonly HttpClient _http = new HttpClient();

        static ConfigService()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "datHazy-Moonlauncher/1.0");

            _http.Timeout = TimeSpan.FromSeconds(10);
        }

        public static async Task<AppConfig> LoadAsync(string url)
        {
            var response = await _http.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var config = JsonSerializer.Deserialize<AppConfig>(json, options);

            if (config == null)
                throw new Exception("Config deserialized as null");

            return config;
        }
    }
}