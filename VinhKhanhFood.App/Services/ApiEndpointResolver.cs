namespace VinhKhanhFood.App.Services;

public static class ApiEndpointResolver
{
    private const string DevApiOverrideKey = "DevApiBaseUrl";

    public static string BaseServerUrl
    {
        get
        {
            var overrideUrl = Preferences.Default.Get(DevApiOverrideKey, string.Empty)?.Trim();
            if (!string.IsNullOrWhiteSpace(overrideUrl))
            {
                return NormalizeBaseUrl(overrideUrl);
            }

            if (DeviceInfo.Platform != DevicePlatform.Android)
            {
                return "http://localhost:5020";
            }

#if ANDROID
            return IsAndroidEmulator()
                ? "http://10.0.2.2:5020"
                : "http://127.0.0.1:5020";
#else
            return "http://localhost:5020";
#endif
        }
    }

    public static string BaseApiUrl => $"{BaseServerUrl}/api";

    public static string FoodEndpoint => $"{BaseApiUrl}/Food";
    public static string UserEndpoint => $"{BaseApiUrl}/User";
    public static string PaymentEndpoint => $"{BaseApiUrl}/Payment";

    public static void SetDevelopmentBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            Preferences.Default.Remove(DevApiOverrideKey);
            return;
        }

        Preferences.Default.Set(DevApiOverrideKey, NormalizeBaseUrl(baseUrl));
    }

    public static string ResolveAssetUrl(string relativeOrAbsolutePath, string defaultFolder = "audio")
    {
        if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(relativeOrAbsolutePath, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var normalizedPath = relativeOrAbsolutePath.Trim().TrimStart('/');
        if (!normalizedPath.Contains('/'))
        {
            normalizedPath = $"{defaultFolder}/{normalizedPath}";
        }

        return $"{BaseServerUrl}/{normalizedPath}";
    }

    private static string NormalizeBaseUrl(string baseUrl) =>
        baseUrl.Trim().TrimEnd('/').Replace("/api", string.Empty, StringComparison.OrdinalIgnoreCase);

#if ANDROID
    private static bool IsAndroidEmulator()
    {
        var fingerprint = Android.OS.Build.Fingerprint?.ToLowerInvariant() ?? string.Empty;
        var model = Android.OS.Build.Model?.ToLowerInvariant() ?? string.Empty;
        var manufacturer = Android.OS.Build.Manufacturer?.ToLowerInvariant() ?? string.Empty;
        var brand = Android.OS.Build.Brand?.ToLowerInvariant() ?? string.Empty;
        var device = Android.OS.Build.Device?.ToLowerInvariant() ?? string.Empty;
        var product = Android.OS.Build.Product?.ToLowerInvariant() ?? string.Empty;
        var hardware = Android.OS.Build.Hardware?.ToLowerInvariant() ?? string.Empty;

        return fingerprint.Contains("generic") ||
               fingerprint.Contains("emulator") ||
               model.Contains("emulator") ||
               model.Contains("sdk") ||
               manufacturer.Contains("genymotion") ||
               brand.StartsWith("generic", StringComparison.Ordinal) ||
               device.StartsWith("generic", StringComparison.Ordinal) ||
               product.Contains("sdk") ||
               hardware.Contains("ranchu") ||
               hardware.Contains("goldfish");
    }
#endif
}
