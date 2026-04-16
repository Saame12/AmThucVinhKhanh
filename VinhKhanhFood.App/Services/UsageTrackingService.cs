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
            var identity = App.Auth.GetUsageIdentity();
            var payload = new
            {
                UserId = identity.UserId,
                UserName = identity.UserName,
                Role = identity.Role,
                GuestId = identity.GuestId,
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
