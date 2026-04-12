using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        UpdateLocalizedTexts();
        App.Auth.SessionChanged += OnSessionChanged;
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

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is null)
        {
            App.Auth.SessionChanged -= OnSessionChanged;
        }
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
    }

    private void OnSessionChanged(object? sender, Models.UserSession? e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateAccountSection);
    }

    private void UpdateLocalizedTexts()
    {
        TitleLabel.Text = LocalizationService.GetString("SettingsTitle");
        SubtitleLabel.Text = LocalizationService.GetString("SettingsSubtitle");
        LanguageSectionTitleLabel.Text = LocalizationService.GetString("AudioLocalizationTitle");
        LanguageSectionSubtitleLabel.Text = LocalizationService.GetString("AudioLocalizationSubtitle");
        CurrentLanguageCaptionLabel.Text = LocalizationService.GetString("Language");
        ChangeLanguageButton.Text = LocalizationService.GetString("Select Language");
        AccountSectionTitleLabel.Text = LocalizationService.GetString("AboutAccountTitle");
        VersionTitleLabel.Text = LocalizationService.GetString("Version Info:");
        VersionValueLabel.Text = LocalizationService.GetString("VersionValue");

        CurrentLanguageValueLabel.Text = GetCurrentLanguageDisplayName();
        UpdateAccountSection();
    }

    private void UpdateAccountSection()
    {
        var session = App.Auth.CurrentSession;
        if (session is null)
        {
            AccountStatusLabel.Text = LocalizationService.GetString("LoginToYourAccount");
            AccountActionButton.Text = LocalizationService.GetString("LoginTitle");
            return;
        }

        AccountStatusLabel.Text = $"{session.FullName} ({session.Username})";
        AccountActionButton.Text = LocalizationService.GetString("Logout");
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

    private async void OnAccountActionClicked(object sender, EventArgs e)
    {
        if (App.Auth.IsLoggedIn)
        {
            App.Auth.Logout();
            return;
        }

        await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
    }

    private static string GetCurrentLanguageDisplayName() => LocalizationService.CurrentLanguage switch
    {
        "en" => "English",
        "zh" => "中文",
        _ => "Tiếng Việt"
    };
}
