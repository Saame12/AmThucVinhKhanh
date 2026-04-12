using System.Net.Http.Json;
using VinhKhanhFood.App.Models;

namespace VinhKhanhFood.App.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        _httpClient = new HttpClient(handler);
    }

    public async Task<List<FoodLocation>> GetFoodLocationsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<FoodLocation>>(ApiEndpointResolver.FoodEndpoint);
            return response ?? new List<FoodLocation>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi kết nối API: {ex.Message}");
            return new List<FoodLocation>();
        }
    }
}
