using LushThreads.Application.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Implementation of IIpLocationService using ip-api.com.
    /// </summary>
    public class IpLocationService : IIpLocationService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IpLocationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetLocationFromIpAsync(string ipAddress)
        {
            if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress == "Unknown")
                return "Local Development";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetFromJsonAsync<IpApiResponse>($"http://ip-api.com/json/{ipAddress}");

                return response?.Status == "success" ? $"{response.City}, {response.Country}" : "Unknown Location";
            }
            catch
            {
                return "Location Unavailable";
            }
        }

        private record IpApiResponse(
            [property: JsonPropertyName("status")] string Status,
            [property: JsonPropertyName("country")] string Country,
            [property: JsonPropertyName("city")] string City);
    }
}
