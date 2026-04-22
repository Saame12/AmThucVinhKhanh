using System.Net.Http.Json;
using VinhKhanhFood.App.Models;

namespace VinhKhanhFood.App.Services;

public sealed class PaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler);
    }

    public async Task<PaymentAccessResponse> GetProfessionalAccessAsync(int poiId, CancellationToken cancellationToken = default)
    {
        var session = App.Auth.CurrentSession;
        var guestId = App.Auth.GuestId;
        var query = $"{ApiEndpointResolver.PaymentEndpoint}/access?poiId={poiId}";

        if (session is not null && session.Id > 0)
        {
            query += $"&userId={session.Id}";
        }

        if (!string.IsNullOrWhiteSpace(guestId))
        {
            query += $"&guestId={Uri.EscapeDataString(guestId)}";
        }

        try
        {
            return await _httpClient.GetFromJsonAsync<PaymentAccessResponse>(query, cancellationToken)
                   ?? new PaymentAccessResponse { PoiId = poiId };
        }
        catch
        {
            return new PaymentAccessResponse { PoiId = poiId };
        }
    }

    public async Task<(bool Success, string? ErrorMessage, MockCheckoutResult? Result)> MockCheckoutAsync(int poiId, decimal amount, CancellationToken cancellationToken = default)
    {
        var session = App.Auth.CurrentSession;
        var request = new MockCheckoutRequest
        {
            PoiId = poiId,
            Amount = amount,
            UserId = session?.Id > 0 ? session.Id : null,
            GuestId = session is null ? App.Auth.GuestId : null,
            Provider = "MockQR"
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiEndpointResolver.PaymentEndpoint}/mock-checkout", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, LocalizationService.GetString("AuthRequestFailed"), null);
            }

            var result = await response.Content.ReadFromJsonAsync<MockCheckoutResult>(cancellationToken: cancellationToken);
            if (result is null)
            {
                return (false, LocalizationService.GetString("AuthUnexpectedResponse"), null);
            }

            return (true, null, result);
        }
        catch (HttpRequestException)
        {
            return (false, ApiHealthService.GetServerUnavailableMessage(), null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }
}
