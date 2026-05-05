using VinhKhanhFood.App.Services;
using Microsoft.Maui.ApplicationModel;

namespace VinhKhanhFood.App;

public partial class App : Application
{
    private static Uri? _pendingUri;
    private bool _isReloadingShell;

    public static AuthService Auth { get; } = new();
    public static AudioGuideService AudioGuide { get; } = new();
    public static string CurrentLanguage => LocalizationService.CurrentLanguage;

    public App()
    {
        InitializeComponent();
        LocalizationService.Initialize();
        LocalizationService.LanguageChanged += OnLanguageChanged;
        AudioGuide.StateChanged += OnAudioGuideStateChanged;
        MainPage = new AppShell();

        if (_pendingUri is not null)
        {
            var pendingUri = _pendingUri;
            _pendingUri = null;
            _ = HandleIncomingUriAsync(pendingUri);
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Resumed += OnWindowResumed;
        window.Stopped += OnWindowStopped;
        window.Destroying += OnWindowDestroying;
        return window;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        if (_isReloadingShell)
        {
            return;
        }

        _ = ReloadShellAsync();
    }

    public static async Task<QrOpenResult> OpenQrAsync(string rawValue)
    {
        if (!TryCreateSupportedQrUri(rawValue, out var uri))
        {
            return QrOpenResult.Invalid;
        }

        if (Current is App app)
        {
            return await app.HandleIncomingUriAsync(uri);
        }

        _pendingUri = uri;
        return QrOpenResult.Pending;
    }

    public static void ReceiveQrUri(string rawUri)
    {
        _ = OpenQrAsync(rawUri);
    }

    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        _ = HandleIncomingUriAsync(uri);
        base.OnAppLinkRequestReceived(uri);
    }

    private async void OnWindowResumed(object? sender, EventArgs e)
    {
        await Auth.SetPresenceAsync(true);
        Auth.StartPresenceHeartbeat();
    }

    private async void OnWindowStopped(object? sender, EventArgs e)
    {
        Auth.StopAudioHeartbeat();
        Auth.StopPresenceHeartbeat();
        await Auth.SetPresenceAsync(false);
    }

    private async void OnWindowDestroying(object? sender, EventArgs e)
    {
        Auth.StopAudioHeartbeat();
        Auth.StopPresenceHeartbeat();
        await Auth.SetPresenceAsync(false);
    }

    private void OnAudioGuideStateChanged(object? sender, AudioGuideStateChangedEventArgs e)
    {
        if (e.State == AudioGuidePlaybackState.Playing && e.CurrentPoi is not null)
        {
            Auth.StartAudioHeartbeat(e.CurrentPoi);
            return;
        }

        Auth.StopAudioHeartbeat();
    }

    private async Task<QrOpenResult> HandleIncomingUriAsync(Uri uri)
    {
        if (await TryHandleUnlockUriAsync(uri))
        {
            return QrOpenResult.Success;
        }

        if (!TryExtractPoiId(uri, out var poiId))
        {
            return QrOpenResult.Invalid;
        }

        var apiService = new ApiService();
        var locations = await apiService.GetFoodLocationsAsync();
        if (locations.Count == 0)
        {
            return QrOpenResult.Unavailable;
        }

        var poi = locations.FirstOrDefault(item => item.Id == poiId);
        if (poi is null)
        {
            return QrOpenResult.NotFound;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync("//ExploreTab");
            await Shell.Current.Navigation.PushAsync(new DetailPage(poi, autoPlayAudio: true));
        });

        return QrOpenResult.Success;
    }

    private async Task<bool> TryHandleUnlockUriAsync(Uri uri)
    {
        if (!string.Equals(uri.Scheme, "vinhkhanhfood", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, "unlock", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var token = GetQueryParameter(uri, "token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var claimed = await Auth.ClaimUnlockAsync(token);
        if (!claimed)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Current?.MainPage?.DisplayAlert("Thong bao", "Khong the kich hoat mo khoa tu lien ket nay.", "OK"));
            return true;
        }

        if (int.TryParse(GetQueryParameter(uri, "poiId"), out var poiId))
        {
            var apiService = new ApiService();
            var locations = await apiService.GetFoodLocationsAsync();
            var poi = locations.FirstOrDefault(item => item.Id == poiId);
            if (poi is not null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Shell.Current is null)
                    {
                        return;
                    }

                    await Shell.Current.GoToAsync("//ExploreTab");
                    await Shell.Current.Navigation.PushAsync(new DetailPage(poi, autoPlayAudio: true));
                });

                return true;
            }
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Current!.MainPage!.DisplayAlert("Thong bao", "Da mo khoa thanh cong cho thiet bi nay.", "OK");
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("//ExploreTab");
            }
        });

        return true;
    }

    private async Task ReloadShellAsync()
    {
        try
        {
            _isReloadingShell = true;

            string? currentTabRoute = null;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                currentTabRoute = Shell.Current?.CurrentItem?.Route;
                MainPage = new AppShell();
            });

            if (!string.IsNullOrWhiteSpace(currentTabRoute) && Shell.Current is not null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync($"//{currentTabRoute}");
                });
            }
        }
        finally
        {
            _isReloadingShell = false;
        }
    }

    private static bool TryExtractPoiId(Uri uri, out int poiId)
    {
        poiId = 0;

        if (string.Equals(uri.Scheme, "vinhkhanhfood", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(uri.Host, "poi", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(uri.AbsolutePath.Trim('/'), out poiId);
        }

        if ((string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)) &&
            TryExtractPoiIdFromPublicPath(uri.AbsolutePath, out poiId))
        {
            return true;
        }

        return false;
    }

    private static bool TryCreateSupportedQrUri(string rawValue, out Uri uri)
    {
        uri = null!;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var normalized = rawValue.Trim();

        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
        {
            uri = absoluteUri;
            return true;
        }

        if (normalized.StartsWith("VK-POI-", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(normalized["VK-POI-".Length..], out var codePoiId))
        {
            uri = new Uri($"vinhkhanhfood://poi/{codePoiId}");
            return true;
        }

        if (normalized.StartsWith("VK-PAY-", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(normalized["VK-PAY-".Length..], out var paymentPoiId))
        {
            uri = new Uri($"vinhkhanhfood://poi/{paymentPoiId}");
            return true;
        }

        if (int.TryParse(normalized, out var numericPoiId))
        {
            uri = new Uri($"vinhkhanhfood://poi/{numericPoiId}");
            return true;
        }

        return false;
    }

    private static string? GetQueryParameter(Uri uri, string key)
    {
        var query = uri.Query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return null;
    }

    private static bool TryExtractPoiIdFromPublicPath(string absolutePath, out int poiId)
    {
        poiId = 0;

        var segments = absolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3)
        {
            return false;
        }

        return string.Equals(segments[0], "public", StringComparison.OrdinalIgnoreCase) &&
               string.Equals(segments[1], "poi", StringComparison.OrdinalIgnoreCase) &&
               int.TryParse(segments[2], out poiId);
    }

    public enum QrOpenResult
    {
        Success,
        Invalid,
        NotFound,
        Unavailable,
        Pending
    }
}
