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
        "en" => "Language and app information",
        "zh" => "\u8BED\u8A00\u4E0E\u5E94\u7528\u4FE1\u606F",
        _ => "Ngon ngu va thong tin ung dung"
    };
}
