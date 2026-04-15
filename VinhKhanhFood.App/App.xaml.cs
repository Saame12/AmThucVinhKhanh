using VinhKhanhFood.App.Services;

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
        if (!TryCreatePoiUri(rawValue, out var uri))
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
        Auth.StopPresenceHeartbeat();
        await Auth.SetPresenceAsync(false);
    }

    private async void OnWindowDestroying(object? sender, EventArgs e)
    {
        Auth.StopPresenceHeartbeat();
        await Auth.SetPresenceAsync(false);
    }

    private async Task<QrOpenResult> HandleIncomingUriAsync(Uri uri)
    {
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

        if (!string.Equals(uri.Scheme, "vinhkhanhfood", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(uri.Host, "poi", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return int.TryParse(uri.AbsolutePath.Trim('/'), out poiId);
    }

    private static bool TryCreatePoiUri(string rawValue, out Uri uri)
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

        if (int.TryParse(normalized, out var numericPoiId))
        {
            uri = new Uri($"vinhkhanhfood://poi/{numericPoiId}");
            return true;
        }

        return false;
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
