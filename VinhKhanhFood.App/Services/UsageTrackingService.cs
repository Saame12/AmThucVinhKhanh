using System.Net.Http.Json;

namespace VinhKhanhFood.App.Services;

public sealed class UsageTrackingService
{
    private readonly HttpClient _httpClient;

    public UsageTrackingService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler);
    }

    public async Task TrackPoiDetailViewAsync(int poiId)
    {
        try
        {
            var session = App.Auth.CurrentSession;
            var payload = new
            {
                UserId = session?.Id ?? 0,
                UserName = session?.FullName ?? "Guest",
                Role = session?.Role ?? "Traveler",
                PoiId = poiId
            };

            await _httpClient.PostAsJsonAsync($"{ApiEndpointResolver.FoodEndpoint}/history/view", payload);
        }
        catch
        {
            // Tracking should not interrupt navigation or detail rendering.
        }
    }
}
