using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        UpdateTabTitles();
        LocalizationService.LanguageChanged += OnLanguageChanged;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            LocalizationService.LanguageChanged -= OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateTabTitles);
    }

    private void UpdateTabTitles()
    {
        MapTab.Title = LocalizationService.GetString("Map");
        ExploreTab.Title = LocalizationService.GetString("Explore");
        ScanTab.Title = GetScanTabTitle();
        SettingsTab.Title = LocalizationService.GetString("Settings");
    }

    private static string GetScanTabTitle() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Scan QR",
        "zh" => "扫码",
        _ => "Quét QR"
    };
}
