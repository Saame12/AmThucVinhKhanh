using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class DetailPage : ContentPage
{
    private readonly FoodLocation _currentLocation;
    private readonly bool _autoPlayAudio;
    private readonly AudioGuideService _audioGuideService = App.AudioGuide;
    private readonly UsageTrackingService _usageTrackingService = new();
    private bool _hasTrackedView;

    public DetailPage(FoodLocation location, bool autoPlayAudio = false)
    {
        InitializeComponent();
        _currentLocation = location;
        _autoPlayAudio = autoPlayAudio;
        BindingContext = _currentLocation;

        HeroImage.Source = _currentLocation.ImageUrl;
        UpdateLocalizedTexts();
        UpdateContent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LocalizationService.LanguageChanged += OnLanguageChanged;
        _audioGuideService.StateChanged += OnAudioStateChanged;
        App.Auth.SessionChanged += OnSessionChanged;

        if (!_hasTrackedView)
        {
            _hasTrackedView = true;
            await _usageTrackingService.TrackPoiDetailViewAsync(_currentLocation.Id);
        }

        if (_autoPlayAudio)
        {
            await PlayCurrentPoiAudioAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        _audioGuideService.StateChanged -= OnAudioStateChanged;
        App.Auth.SessionChanged -= OnSessionChanged;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocalizedTexts();
            UpdateContent();
        });
    }

    private void OnSessionChanged(object? sender, UserSession? e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateContent);
    }

    private void OnAudioStateChanged(object? sender, AudioGuideStateChangedEventArgs e)
    {
        if (e.CurrentPoi?.Id != _currentLocation.Id && e.State == AudioGuidePlaybackState.Playing)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(UpdateAudioStateUi);
    }

    private void UpdateContent()
    {
        NameLabel.Text = _currentLocation.DisplayName;
        DescriptionLabel.Text = _currentLocation.DisplayDescription;
        ProfessionalAudioCard.IsVisible = _currentLocation.HasProfessionalAudio;
        UpdateAudioStateUi();
        UpdateProfessionalAudioUi();
    }

    private void UpdateLocalizedTexts()
    {
        Title = LocalizationService.GetString("Details");
        HeaderEyebrowLabel.Text = GetHeaderEyebrowText();
        HeaderTitleLabel.Text = GetHeaderTitleText();
        AudioSectionTitleLabel.Text = LocalizationService.GetString("Audio Guide");
        GuideBadgeLabel.Text = GetGuideBadgeText();
        PlayAudioButton.Text = LocalizationService.GetString("Play");
        CancelAudioButton.Text = LocalizationService.GetString("Cancel");
        ProfessionalAudioTitleLabel.Text = GetProfessionalAudioTitleText();
        VipBadgeLabel.Text = "VIP";
        UpdateProfessionalAudioUi();
    }

    private void UpdateAudioStateUi()
    {
        var isCurrentPoiPlaying = _audioGuideService.CurrentPoi?.Id == _currentLocation.Id && _audioGuideService.IsPlaying;
        AudioStatusLabel.Text = isCurrentPoiPlaying
            ? LocalizationService.GetString("AudioPlaying")
            : LocalizationService.GetString("AudioIdle");

        PlayAudioButton.IsEnabled = !isCurrentPoiPlaying;
        CancelAudioButton.IsEnabled = _audioGuideService.CurrentPoi?.Id == _currentLocation.Id;
    }

    private void UpdateProfessionalAudioUi()
    {
        if (!_currentLocation.HasProfessionalAudio)
        {
            return;
        }

        var session = App.Auth.CurrentSession;
        if (session is null)
        {
            ProfessionalAudioStatusLabel.Text = GetProfessionalGuestText();
            PlayProfessionalAudioButton.Text = GetProfessionalLoginButtonText();
            return;
        }

        if (!session.IsVip)
        {
            ProfessionalAudioStatusLabel.Text = GetProfessionalLockedText();
            PlayProfessionalAudioButton.Text = GetProfessionalBuyVipText();
            return;
        }

        ProfessionalAudioStatusLabel.Text = GetProfessionalReadyText();
        PlayProfessionalAudioButton.Text = GetProfessionalPlayText();
    }

    private async Task PlayCurrentPoiAudioAsync()
    {
        var started = await _audioGuideService.PlayPoiAsync(_currentLocation);
        if (!started)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("AudioUnavailable"), LocalizationService.GetString("OK"));
            return;
        }

        await _usageTrackingService.TrackAudioPlayAsync(_currentLocation.Id);
    }

    private async Task PlayProfessionalAudioAsync()
    {
        var started = await _audioGuideService.PlayProfessionalAudioAsync(_currentLocation);
        if (!started)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), GetProfessionalMissingText(), LocalizationService.GetString("OK"));
            return;
        }

        await _usageTrackingService.TrackAudioPlayAsync(_currentLocation.Id);
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        await PlayCurrentPoiAudioAsync();
    }

    private async void OnPlayProfessionalAudioClicked(object sender, EventArgs e)
    {
        var session = App.Auth.CurrentSession;
        if (session is null)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("LoginToYourAccount"), LocalizationService.GetString("OK"));
            await Navigation.PushModalAsync(new NavigationPage(new LoginPage()));
            return;
        }

        if (!session.IsVip)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), GetProfessionalNeedVipText(), LocalizationService.GetString("OK"));
            await Shell.Current.GoToAsync("//SettingsTab");
            return;
        }

        await PlayProfessionalAudioAsync();
    }

    private void OnCancelAudioClicked(object sender, EventArgs e)
    {
        _audioGuideService.Cancel();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private static string GetHeaderEyebrowText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "POI detail",
        "zh" => "POI \u8BE6\u60C5",
        _ => "Chi tiet POI"
    };

    private static string GetHeaderTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Audio guide",
        "zh" => "\u8BED\u97F3\u8BB2\u89E3",
        _ => "Thuyet minh audio"
    };

    private static string GetGuideBadgeText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Guide",
        "zh" => "\u5BFC\u89C8",
        _ => "Guide"
    };

    private static string GetProfessionalAudioTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Professional audio",
        "zh" => "\u4E13\u4E1A\u97F3\u9891",
        _ => "Audio chuyen nghiep"
    };

    private static string GetProfessionalGuestText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Sign in to unlock owner-uploaded professional audio.",
        "zh" => "\u767B\u5F55\u540E\u53EF\u89E3\u9501 owner \u4E0A\u4F20\u7684\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => "Dang nhap de mo khoa audio chuyen nghiep do owner upload."
    };

    private static string GetProfessionalLockedText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This POI has a VIP audio version.",
        "zh" => "\u8BE5 POI \u63D0\u4F9B VIP \u97F3\u9891\u7248\u672C\u3002",
        _ => "POI nay co ban audio VIP."
    };

    private static string GetProfessionalReadyText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Your VIP account can play the uploaded professional track.",
        "zh" => "\u60A8\u7684 VIP \u8D26\u6237\u53EF\u64AD\u653E\u4E0A\u4F20\u7684\u4E13\u4E1A\u97F3\u8F68\u3002",
        _ => "Tai khoan VIP cua ban co the phat audio chuyen nghiep da upload."
    };

    private static string GetProfessionalLoginButtonText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Login to unlock",
        "zh" => "\u767B\u5F55\u540E\u89E3\u9501",
        _ => "Dang nhap de mo khoa"
    };

    private static string GetProfessionalBuyVipText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Buy VIP in Settings",
        "zh" => "\u524D\u5F80\u8BBE\u7F6E\u8D2D\u4E70 VIP",
        _ => "Mua VIP trong Cai dat"
    };

    private static string GetProfessionalPlayText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Play professional audio",
        "zh" => "\u64AD\u653E\u4E13\u4E1A\u97F3\u9891",
        _ => "Phat audio chuyen nghiep"
    };

    private static string GetProfessionalNeedVipText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Please purchase VIP in Settings to listen to this professional audio.",
        "zh" => "\u8BF7\u5148\u5728\u8BBE\u7F6E\u4E2D\u8D2D\u4E70 VIP \u540E\u518D\u6536\u542C\u6B64\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => "Hay mua VIP trong Cai dat de nghe audio chuyen nghiep nay."
    };

    private static string GetProfessionalMissingText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "The uploaded professional audio file is unavailable.",
        "zh" => "\u5DF2\u4E0A\u4F20\u7684\u4E13\u4E1A\u97F3\u9891\u6587\u4EF6\u6682\u65F6\u4E0D\u53EF\u7528\u3002",
        _ => "File audio chuyen nghiep da upload hien khong kha dung."
    };
}
