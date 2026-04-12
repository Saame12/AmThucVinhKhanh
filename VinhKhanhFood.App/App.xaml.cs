using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class App : Application
{
    private static Uri? _pendingUri;

    public static AuthService Auth { get; } = new();
    public static AudioGuideService AudioGuide { get; } = new();
    public static string CurrentLanguage => LocalizationService.CurrentLanguage;

    public App()
    {
        InitializeComponent();
        LocalizationService.Initialize();
        MainPage = new AppShell();

        if (_pendingUri is not null)
        {
            var pendingUri = _pendingUri;
            _pendingUri = null;
            _ = HandleIncomingUriAsync(pendingUri);
        }
    }

    public static void ReceiveQrUri(string rawUri)
    {
        if (!Uri.TryCreate(rawUri, UriKind.Absolute, out var uri))
        {
            return;
        }

        if (Current is App app)
        {
            _ = app.HandleIncomingUriAsync(uri);
            return;
        }

        _pendingUri = uri;
    }

    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        _ = HandleIncomingUriAsync(uri);
        base.OnAppLinkRequestReceived(uri);
    }

    private async Task HandleIncomingUriAsync(Uri uri)
    {
        if (!TryExtractPoiId(uri, out var poiId))
        {
            return;
        }

        var apiService = new ApiService();
        var locations = await apiService.GetFoodLocationsAsync();
        var poi = locations.FirstOrDefault(item => item.Id == poiId);
        if (poi is null)
        {
            return;
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
}
