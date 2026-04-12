namespace VinhKhanhFood.App.Services;

public static class ApiEndpointResolver
{
    public static string BaseApiUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5020/api"
            : "http://localhost:5020/api";

    public static string FoodEndpoint => $"{BaseApiUrl}/Food";
    public static string UserEndpoint => $"{BaseApiUrl}/User";
}
