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
            VipOfferBorder.IsVisible = false;
            return;
        }

        AccountStatusLabel.Text = $"{session.FullName} ({session.Username})";
        AccountActionButton.Text = LocalizationService.GetString("Logout");

        VipOfferBorder.IsVisible = true;
        VipTitleLabel.Text = GetVipTitleText();
        VipSubtitleLabel.Text = GetVipSubtitleText();
        VipStatusLabel.Text = session.IsVip ? GetVipStatusActiveText() : GetVipStatusInactiveText();
        VipActionButton.Text = session.IsVip ? GetVipActionActiveText() : GetVipActionBuyText();
        VipActionButton.BackgroundColor = Color.FromArgb(session.IsVip ? "#0F766E" : "#E85D2A");
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

    private async void OnVipActionClicked(object sender, EventArgs e)
    {
        var session = App.Auth.CurrentSession;
        if (session is null)
        {
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            return;
        }

        if (session.IsVip)
        {
            await DisplayAlert(GetVipTitleText(), GetVipAlreadyPurchasedText(), LocalizationService.GetString("OK"));
            return;
        }

        VipActionButton.IsEnabled = false;
        try
        {
            var (success, errorMessage) = await App.Auth.PurchaseVipAsync();
            if (!success)
            {
                await DisplayAlert(LocalizationService.GetString("Error"), errorMessage ?? GetVipPurchaseFailedText(), LocalizationService.GetString("OK"));
                return;
            }

            await DisplayAlert(GetVipTitleText(), GetVipPurchaseSuccessText(), LocalizationService.GetString("OK"));
            UpdateAccountSection();
        }
        finally
        {
            VipActionButton.IsEnabled = true;
        }
    }

    private static string GetCurrentLanguageDisplayName() => LocalizationService.CurrentLanguage switch
    {
        "en" => "English",
        "zh" => "\u4E2D\u6587",
        _ => "Tieng Viet"
    };

    private static string GetVipTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "VIP audio plan",
        "zh" => "VIP \u97F3\u9891\u5957\u9910",
        _ => "Goi VIP audio"
    };

    private static string GetVipSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Upgrade this account to unlock owner-uploaded professional audio in POI details.",
        "zh" => "\u5347\u7EA7\u6B64\u8D26\u6237\u540E\uff0c\u53EF\u89E3\u9501 POI \u8BE6\u60C5\u4E2D\u7531\u5E97\u4E3B\u4E0A\u4F20\u7684\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => "Nang cap tai khoan de mo khoa audio chuyen nghiep do owner tai len trong trang chi tiet POI."
    };

    private static string GetVipStatusInactiveText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Current plan: Standard",
        "zh" => "\u5F53\u524D\u5957\u9910\uff1A\u6807\u51C6",
        _ => "Goi hien tai: Tieu chuan"
    };

    private static string GetVipStatusActiveText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Current plan: VIP unlocked",
        "zh" => "\u5F53\u524D\u5957\u9910\uff1AVIP \u5DF2\u5F00\u901A",
        _ => "Goi hien tai: VIP da mo khoa"
    };

    private static string GetVipActionBuyText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Buy VIP now",
        "zh" => "\u7ACB\u5373\u5F00\u901A VIP",
        _ => "Mua goi VIP ngay"
    };

    private static string GetVipActionActiveText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "VIP is active",
        "zh" => "VIP \u5DF2\u751F\u6548",
        _ => "VIP dang hoat dong"
    };

    private static string GetVipAlreadyPurchasedText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This account is already VIP and can access professional audio.",
        "zh" => "\u8D26\u6237\u5DF2\u662F VIP\uff0C\u53EF\u4EE5\u542C\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => "Tai khoan nay da la VIP va co the nghe audio chuyen nghiep."
    };

    private static string GetVipPurchaseSuccessText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "VIP has been activated for this account.",
        "zh" => "\u6B64\u8D26\u6237\u5DF2\u5F00\u901A VIP\u3002",
        _ => "Tai khoan nay da duoc kich hoat VIP."
    };

    private static string GetVipPurchaseFailedText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Unable to activate VIP right now.",
        "zh" => "\u6682\u65F6\u65E0\u6CD5\u5F00\u901A VIP\u3002",
        _ => "Hien khong the kich hoat VIP."
    };
}
