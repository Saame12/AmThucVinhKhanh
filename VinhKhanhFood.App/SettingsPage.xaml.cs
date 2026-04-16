using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        UpdateLocalizedTexts();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedTexts();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
    }

    private void UpdateLocalizedTexts()
    {
        TitleLabel.Text = LocalizationService.GetString("SettingsTitle");
        SubtitleLabel.Text = GetSettingsSubtitleText();
        LanguageSectionTitleLabel.Text = LocalizationService.GetString("AudioLocalizationTitle");
        LanguageSectionSubtitleLabel.Text = LocalizationService.GetString("AudioLocalizationSubtitle");
        CurrentLanguageCaptionLabel.Text = LocalizationService.GetString("Language");
        ChangeLanguageButton.Text = LocalizationService.GetString("Select Language");
        CurrentLanguageValueLabel.Text = GetCurrentLanguageDisplayName();

        TravelerSectionTitleLabel.Text = GetTravelerSectionTitleText();
        TravelerSectionSubtitleLabel.Text = GetTravelerSectionSubtitleText();
        GuidCaptionLabel.Text = GetGuidCaptionText();
        GuidValueLabel.Text = $"guid-{App.Auth.GuestId}";
        GuidHintLabel.Text = GetGuidHintText();

        VersionTitleLabel.Text = LocalizationService.GetString("Version Info:");
        VersionValueLabel.Text = LocalizationService.GetString("VersionValue");
    }

    private async void OnChangeLanguageClicked(object sender, EventArgs e)
    {
        var languages = LocalizationService.GetAvailableLanguages();
        var labels = languages.Select(static item => item.Name).ToArray();
        var selection = await DisplayActionSheet(
            LocalizationService.GetString("Select Language"),
            LocalizationService.GetString("Cancel"),
            null,
            labels);

        if (string.IsNullOrWhiteSpace(selection) ||
            string.Equals(selection, LocalizationService.GetString("Cancel"), StringComparison.Ordinal))
        {
            return;
        }

        var language = languages.FirstOrDefault(item => item.Name == selection);
        if (!string.IsNullOrWhiteSpace(language.Code))
        {
            LocalizationService.SetLanguage(language.Code);
        }
    }

    private static string GetCurrentLanguageDisplayName() => LocalizationService.CurrentLanguage switch
    {
        "en" => "English",
        "zh" => "\u4E2D\u6587",
        _ => "Tieng Viet"
    };

    private static string GetSettingsSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Language preferences and traveler session information",
        "zh" => "\u8BED\u8A00\u504F\u597D\u4E0E\u6E38\u5BA2\u4F1A\u8BDD\u4FE1\u606F",
        _ => "Tuy chinh ngon ngu va thong tin phien khach du lich"
    };

    private static string GetTravelerSectionTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Traveler session",
        "zh" => "\u6E38\u5BA2\u4F1A\u8BDD",
        _ => "Phien khach du lich"
    };

    private static string GetTravelerSectionSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Each active visitor is tracked with a temporary guid for load monitoring.",
        "zh" => "\u6BCF\u4E2A\u6D3B\u52A8\u6E38\u5BA2\u90FD\u4F1A\u4F7F\u7528\u4E34\u65F6 guid \u8FDB\u884C\u8D1F\u8F7D\u7EDF\u8BA1\u3002",
        _ => "Moi khach dang dung app se duoc gan guid tam de thong ke tai server."
    };

    private static string GetGuidCaptionText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Current guest guid",
        "zh" => "\u5F53\u524D\u6E38\u5BA2 guid",
        _ => "Guid hien tai"
    };

    private static string GetGuidHintText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This guid is refreshed online automatically while the app is being used.",
        "zh" => "\u5F53\u60A8\u4F7F\u7528 App \u65F6\uff0C\u7CFB\u7EDF\u4F1A\u81EA\u52A8\u66F4\u65B0\u6B64 guid \u7684\u5728\u7EBF\u72B6\u6001\u3002",
        _ => "Guid nay duoc cap nhat online tu dong trong luc ban su dung app."
    };
}
