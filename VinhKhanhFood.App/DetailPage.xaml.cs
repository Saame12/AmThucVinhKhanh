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
        App.Auth.SessionChanged += OnSessionChanged;
        _audioGuideService.StateChanged += OnAudioStateChanged;

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
        App.Auth.SessionChanged -= OnSessionChanged;
        _audioGuideService.StateChanged -= OnAudioStateChanged;
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
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocalizedTexts();
            UpdateContent();
        });
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
        UpdateProfessionalAudioUi();
        UpdateAudioStateUi();
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
        ProfessionalAudioTitleLabel.Text = GetProfessionalAudioTitle();
        ProfessionalAudioSubtitleLabel.Text = GetProfessionalAudioSubtitle();
        ProfessionalAudioBadgeLabel.Text = GetProfessionalAudioBadge();
        UnlockProfessionalAudioButton.Text = GetProfessionalAudioButtonText();
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
        var isVip = App.Auth.CurrentSession?.IsVip == true;
        UnlockProfessionalAudioButton.IsEnabled = _currentLocation.HasProfessionalAudio;
        UnlockProfessionalAudioButton.BackgroundColor = Color.FromArgb(isVip ? "#E85D2A" : "#FEF3C7");
        UnlockProfessionalAudioButton.TextColor = isVip ? Colors.White : Color.FromArgb("#92400E");
        ProfessionalAudioSubtitleLabel.Text = GetProfessionalAudioSubtitle();
        ProfessionalAudioBadgeLabel.Text = GetProfessionalAudioBadge();
        UnlockProfessionalAudioButton.Text = GetProfessionalAudioButtonText();
    }

    private async Task PlayCurrentPoiAudioAsync()
    {
        var started = await _audioGuideService.PlayPoiAsync(_currentLocation);
        if (!started)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("AudioUnavailable"), LocalizationService.GetString("OK"));
        }
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        await PlayCurrentPoiAudioAsync();
    }

    private void OnCancelAudioClicked(object sender, EventArgs e)
    {
        _audioGuideService.Cancel();
    }

    private async void OnProfessionalAudioClicked(object sender, EventArgs e)
    {
        if (!_currentLocation.HasProfessionalAudio)
        {
            return;
        }

        if (App.Auth.CurrentSession?.IsVip != true)
        {
            var shouldOpenSettings = await DisplayAlert(
                GetProfessionalAudioTitle(),
                GetProfessionalAudioLockedMessage(),
                GetOpenSettingsText(),
                LocalizationService.GetString("Cancel"));

            if (shouldOpenSettings && Shell.Current is not null)
            {
                await Shell.Current.GoToAsync("//SettingsTab");
            }

            return;
        }

        var started = await _audioGuideService.PlayProfessionalAudioAsync(_currentLocation);
        if (!started)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), GetProfessionalAudioUnavailableText(), LocalizationService.GetString("OK"));
        }
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

    private static string GetProfessionalAudioTitle() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Professional audio",
        "zh" => "\u4E13\u4E1A\u97F3\u9891",
        _ => "Audio chuyen nghiep"
    };

    private static string GetProfessionalAudioSubtitle() => LocalizationService.CurrentLanguage switch
    {
        "en" => App.Auth.CurrentSession?.IsVip == true
            ? "Your VIP plan is active. You can now play the owner-uploaded professional audio."
            : "Owner-uploaded professional audio is available after upgrading this account to VIP.",
        "zh" => App.Auth.CurrentSession?.IsVip == true
            ? "\u60A8\u7684 VIP \u5957\u9910\u5DF2\u751F\u6548\uff0c\u73B0\u5728\u53EF\u4EE5\u64AD\u653E\u5E97\u4E3B\u4E0A\u4F20\u7684\u4E13\u4E1A\u97F3\u9891\u3002"
            : "\u5347\u7EA7\u6B64\u8D26\u6237\u4E3A VIP \u540E\uff0c\u5373\u53EF\u64AD\u653E\u5E97\u4E3B\u4E0A\u4F20\u7684\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => App.Auth.CurrentSession?.IsVip == true
            ? "Goi VIP da hoat dong. Ban co the nghe audio chuyen nghiep do owner tai len."
            : "Audio chuyen nghiep do owner tai len se duoc mo khoa sau khi nang cap tai khoan len VIP."
    };

    private static string GetProfessionalAudioBadge() => LocalizationService.CurrentLanguage switch
    {
        "en" => App.Auth.CurrentSession?.IsVip == true ? "VIP active" : "Premium",
        "zh" => App.Auth.CurrentSession?.IsVip == true ? "VIP \u5DF2\u5F00\u901A" : "\u4ED8\u8D39",
        _ => App.Auth.CurrentSession?.IsVip == true ? "VIP dang mo" : "Tra phi"
    };

    private static string GetProfessionalAudioButtonText() => LocalizationService.CurrentLanguage switch
    {
        "en" => App.Auth.CurrentSession?.IsVip == true ? "Play professional audio" : "Buy VIP in Settings",
        "zh" => App.Auth.CurrentSession?.IsVip == true ? "\u64AD\u653E\u4E13\u4E1A\u97F3\u9891" : "\u524D\u5F80\u8BBE\u7F6E\u5F00\u901A VIP",
        _ => App.Auth.CurrentSession?.IsVip == true ? "Phat audio chuyen nghiep" : "Mo VIP trong Cai dat"
    };

    private static string GetProfessionalAudioLockedMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Open Settings to activate VIP and unlock this professional audio track.",
        "zh" => "\u8BF7\u524D\u5F80\u8BBE\u7F6E\u5F00\u901A VIP\uff0C\u518D\u89E3\u9501\u8FD9\u6761\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => "Hay mo Cai dat de kich hoat VIP va nghe audio chuyen nghiep nay."
    };

    private static string GetProfessionalAudioUnavailableText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This POI does not have a professional audio file yet.",
        "zh" => "\u8BE5 POI \u6682\u65F6\u6CA1\u6709\u4E13\u4E1A\u97F3\u9891\u6587\u4EF6\u3002",
        _ => "POI nay chua co file audio chuyen nghiep."
    };

    private static string GetOpenSettingsText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Open Settings",
        "zh" => "\u6253\u5F00\u8BBE\u7F6E",
        _ => "Mo Cai dat"
    };
}
