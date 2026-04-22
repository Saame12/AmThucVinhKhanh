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
        App.Auth.SessionChanged += OnSessionChanged;
        UpdateLocalizedTexts();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        App.Auth.SessionChanged -= OnSessionChanged;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
    }

    private void OnSessionChanged(object? sender, Models.UserSession? e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
    }

    private void UpdateLocalizedTexts()
    {
        var session = App.Auth.CurrentSession;

        TitleLabel.Text = LocalizationService.GetString("SettingsTitle");
        SubtitleLabel.Text = GetSettingsSubtitleText();
        LanguageSectionTitleLabel.Text = LocalizationService.GetString("AudioLocalizationTitle");
        LanguageSectionSubtitleLabel.Text = LocalizationService.GetString("AudioLocalizationSubtitle");
        CurrentLanguageCaptionLabel.Text = LocalizationService.GetString("Language");
        ChangeLanguageButton.Text = LocalizationService.GetString("Select Language");
        CurrentLanguageValueLabel.Text = GetCurrentLanguageDisplayName();

        AccountSectionTitleLabel.Text = GetAccountSectionTitleText();
        AccountSectionSubtitleLabel.Text = GetAccountSectionSubtitleText();
        AccountStatusLabel.Text = session is null ? GetGuestAccountStatusText() : GetLoggedInStatusText(session.FullName);
        AccountNameLabel.Text = session is null ? GetGuestAccountHintText() : GetLoggedInHintText(session.Username, session.Role);
        LoginButton.Text = GetLoginButtonText(session is null);
        LogoutButton.Text = LocalizationService.GetString("Logout");
        LoginButton.IsVisible = session is null;
        LogoutButton.IsVisible = session is not null;

        VipSectionTitleLabel.Text = GetVipSectionTitleText();
        VipSectionSubtitleLabel.Text = GetVipSectionSubtitleText();
        VipStatusLabel.Text = GetVipStatusText(session);
        VipHintLabel.Text = GetVipHintText(session);
        BuyVipButton.Text = GetVipButtonText(session);
        BuyVipButton.IsEnabled = session is not null && !session.IsVip;

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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            LocalizationService.GetString("SettingsTitle"),
            GetLogoutConfirmText(),
            LocalizationService.GetString("OK"),
            LocalizationService.GetString("Cancel"));

        if (!confirm)
        {
            return;
        }

        App.Auth.Logout();
        UpdateLocalizedTexts();
    }

    private async void OnBuyVipClicked(object sender, EventArgs e)
    {
        var session = App.Auth.CurrentSession;
        if (session is null)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("LoginToYourAccount"), LocalizationService.GetString("OK"));
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            return;
        }

        BuyVipButton.IsEnabled = false;
        try
        {
            var result = await App.Auth.PurchaseVipAsync();
            if (!result.Success)
            {
                await DisplayAlert(LocalizationService.GetString("Error"), result.ErrorMessage ?? LocalizationService.GetString("AuthRequestFailed"), LocalizationService.GetString("OK"));
                return;
            }

            await DisplayAlert(LocalizationService.GetString("Info"), GetVipPurchasedText(), LocalizationService.GetString("OK"));
            UpdateLocalizedTexts();
        }
        finally
        {
            BuyVipButton.IsEnabled = App.Auth.CurrentSession is not null && !(App.Auth.CurrentSession?.IsVip ?? false);
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
        "en" => "Language, account, VIP access, and traveler session information",
        "zh" => "\u8BED\u8A00\u3001\u8D26\u6237\u3001VIP \u6743\u76CA\u4E0E\u6E38\u5BA2\u4F1A\u8BDD\u4FE1\u606F",
        _ => "Tuy chinh ngon ngu, tai khoan, VIP va thong tin phien khach du lich"
    };

    private static string GetAccountSectionTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Account",
        "zh" => "\u8D26\u6237",
        _ => "Tai khoan"
    };

    private static string GetAccountSectionSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Sign in to sync your profile and unlock VIP audio.",
        "zh" => "\u767B\u5F55\u540E\u53EF\u540C\u6B65\u60A8\u7684\u8D44\u6599\u5E76\u89E3\u9501 VIP \u97F3\u9891\u3002",
        _ => "Dang nhap de dong bo thong tin va mo khoa audio VIP."
    };

    private static string GetGuestAccountStatusText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Guest mode is active",
        "zh" => "\u6E38\u5BA2\u6A21\u5F0F\u5DF2\u542F\u7528",
        _ => "Dang o che do khach"
    };

    private static string GetGuestAccountHintText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "You can keep using QR audio normally, or sign in for VIP features.",
        "zh" => "\u60A8\u4ECD\u53EF\u6B63\u5E38\u4F7F\u7528 QR \u8BED\u97F3\uff0c\u6216\u767B\u5F55\u4EE5\u4F53\u9A8C VIP \u529F\u80FD\u3002",
        _ => "Ban van dung QR audio binh thuong, hoac dang nhap de dung them tinh nang VIP."
    };

    private static string GetLoggedInStatusText(string fullName) => LocalizationService.CurrentLanguage switch
    {
        "en" => $"Signed in as {fullName}",
        "zh" => $"\u5DF2\u767B\u5F55\u4E3A {fullName}",
        _ => $"Da dang nhap voi {fullName}"
    };

    private static string GetLoggedInHintText(string username, string role) => LocalizationService.CurrentLanguage switch
    {
        "en" => $"Username: {username} | Role: {role}",
        "zh" => $"\u7528\u6237\u540D\uff1A{username} | \u89D2\u8272\uff1A{role}",
        _ => $"Ten dang nhap: {username} | Vai tro: {role}"
    };

    private static string GetLoginButtonText(bool isGuest) => LocalizationService.CurrentLanguage switch
    {
        "en" => isGuest ? "Login or register" : "Account ready",
        "zh" => isGuest ? "\u767B\u5F55\u6216\u6CE8\u518C" : "\u8D26\u6237\u5DF2\u5C31\u7EEA",
        _ => isGuest ? "Dang nhap / Dang ky" : "Tai khoan san sang"
    };

    private static string GetVipSectionTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "VIP audio plan",
        "zh" => "VIP \u97F3\u9891\u8BA1\u5212",
        _ => "Goi audio VIP"
    };

    private static string GetVipSectionSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Default POI audio remains free. VIP unlocks professional uploaded audio.",
        "zh" => "\u9ED8\u8BA4 POI \u97F3\u9891\u4ECD\u7136\u514D\u8D39\uff0cVIP \u53EF\u89E3\u9501\u4E13\u4E1A\u4E0A\u4F20\u97F3\u9891\u3002",
        _ => "Audio mac dinh van mien phi. VIP mo khoa audio chuyen nghiep do owner upload."
    };

    private static string GetVipStatusText(Models.UserSession? session) => LocalizationService.CurrentLanguage switch
    {
        "en" when session is null => "Sign in to purchase VIP",
        "en" when session.IsVip => "VIP is active",
        "en" => "VIP has not been purchased",
        "zh" when session is null => "\u767B\u5F55\u540E\u624D\u53EF\u8D2D\u4E70 VIP",
        "zh" when session.IsVip => "VIP \u5DF2\u542F\u7528",
        "zh" => "\u5C1A\u672A\u8D2D\u4E70 VIP",
        _ when session is null => "Can dang nhap de mua VIP",
        _ when session.IsVip => "VIP da kich hoat",
        _ => "Chua mua VIP"
    };

    private static string GetVipHintText(Models.UserSession? session) => LocalizationService.CurrentLanguage switch
    {
        "en" when session is null => "You are currently using the app as a guest.",
        "en" when session.IsVip => "This account can play professional POI audio files.",
        "en" => "Purchase VIP to unlock uploaded premium POI audio.",
        "zh" when session is null => "\u60A8\u76EE\u524D\u4EE5\u6E38\u5BA2\u8EAB\u4EFD\u4F7F\u7528 App\u3002",
        "zh" when session.IsVip => "\u8BE5\u8D26\u6237\u53EF\u64AD\u653E POI \u4E13\u4E1A\u97F3\u9891\u6587\u4EF6\u3002",
        "zh" => "\u8D2D\u4E70 VIP \u4EE5\u89E3\u9501\u4E0A\u4F20\u7684\u9AD8\u7EA7 POI \u97F3\u9891\u3002",
        _ when session is null => "Ban dang dung app voi vai tro khach.",
        _ when session.IsVip => "Tai khoan nay co the nghe audio chuyen nghiep cua POI.",
        _ => "Mua VIP de mo khoa audio cao cap cua tung POI."
    };

    private static string GetVipButtonText(Models.UserSession? session) => LocalizationService.CurrentLanguage switch
    {
        "en" when session is null => "Login to buy VIP",
        "en" when session.IsVip => "VIP already active",
        "en" => "Buy VIP",
        "zh" when session is null => "\u767B\u5F55\u540E\u8D2D\u4E70 VIP",
        "zh" when session.IsVip => "VIP \u5DF2\u5F00\u542F",
        "zh" => "\u8D2D\u4E70 VIP",
        _ when session is null => "Dang nhap de mua VIP",
        _ when session.IsVip => "VIP da kich hoat",
        _ => "Mua VIP"
    };

    private static string GetLogoutConfirmText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Do you want to sign out and return to guest mode?",
        "zh" => "\u60A8\u8981\u9000\u51FA\u5E76\u8FD4\u56DE\u6E38\u5BA2\u6A21\u5F0F\u5417\uff1F",
        _ => "Ban co muon dang xuat va quay ve che do khach khong?"
    };

    private static string GetVipPurchasedText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "VIP has been activated for this account.",
        "zh" => "\u8BE5\u8D26\u6237\u7684 VIP \u5DF2\u542F\u7528\u3002",
        _ => "VIP da duoc kich hoat cho tai khoan nay."
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
        "zh" => "\u5F53\u60A8\u4F7F\u7528 App \u65F6\uff0c\u7CFB\u7EDF\u4F1A\u81EA\u52A8\u66F4\u65B0\u6B64 guid \u7684\u5728\u7EBF\u72B6\u6001\u3002",
        _ => "Guid nay duoc cap nhat online tu dong trong luc ban su dung app."
    };
}
