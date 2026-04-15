using System.Net.Http;

namespace VinhKhanhFood.App.Services;

public static class ApiHealthService
{
    public static string GetServerUnavailableMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Server is not running. Please start the API and try again.",
        "zh" => "服务器尚未启动。请先启动 API 后再试。",
        _ => "Server chưa khởi động. Hãy bật API rồi thử lại."
    };

    public static async Task<bool> IsServerAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(3)
            };

            using var response = await httpClient.GetAsync(ApiEndpointResolver.BaseApiUrl, cancellationToken);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }
}
