using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;
using Microsoft.Maui.ApplicationModel;

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
        App.Auth.UnlockStatusChanged += OnUnlockStatusChanged;

        if (!_hasTrackedView)
        {
            _hasTrackedView = true;
            await _usageTrackingService.TrackPoiDetailViewAsync(_currentLocation.Id);
        }

        await App.Auth.RefreshUnlockStatusAsync();
        UpdateUnlockUi();

        if (_autoPlayAudio && App.Auth.HasFullAccess)
        {
            await PlayCurrentPoiAudioAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        _audioGuideService.StateChanged -= OnAudioStateChanged;
        App.Auth.UnlockStatusChanged -= OnUnlockStatusChanged;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
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

    private void OnUnlockStatusChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateUnlockUi);
    }

    private void UpdateContent()
    {
        NameLabel.Text = _currentLocation.DisplayName;
        DescriptionLabel.Text = _currentLocation.DisplayDescription;
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
        UnlockButton.Text = GetUnlockButtonText();
    }

    private void UpdateAudioStateUi()
    {
        if (!App.Auth.HasFullAccess)
        {
            AudioStatusLabel.Text = GetLockedAudioStatusText();
            PlayAudioButton.IsEnabled = false;
            CancelAudioButton.IsEnabled = false;
            return;
        }

        var isCurrentPoiPlaying = _audioGuideService.CurrentPoi?.Id == _currentLocation.Id && _audioGuideService.IsPlaying;
        AudioStatusLabel.Text = isCurrentPoiPlaying
            ? LocalizationService.GetString("AudioPlaying")
            : LocalizationService.GetString("AudioIdle");

        PlayAudioButton.IsEnabled = !isCurrentPoiPlaying;
        CancelAudioButton.IsEnabled = _audioGuideService.CurrentPoi?.Id == _currentLocation.Id;
    }

    private void UpdateUnlockUi()
    {
        var hasFullAccess = App.Auth.HasFullAccess;
        UnlockPanel.IsVisible = !hasFullAccess;
        UnlockStatusLabel.Text = hasFullAccess
            ? string.Empty
            : GetUnlockStatusText();
        UpdateAudioStateUi();
    }

    private async Task PlayCurrentPoiAudioAsync()
    {
        if (!App.Auth.HasFullAccess)
        {
            UpdateUnlockUi();
            await DisplayAlert(LocalizationService.GetString("Info"), GetUnlockRequiredMessage(), "OK");
            return;
        }

        var started = await _audioGuideService.PlayPoiAsync(_currentLocation);
        if (!started)
        {
            await DisplayAlert(LocalizationService.GetString("Info"), LocalizationService.GetString("AudioUnavailable"), LocalizationService.GetString("OK"));
            return;
        }

        await _usageTrackingService.TrackAudioPlayAsync(_currentLocation.Id);
    }

    private async void OnPlayAudioClicked(object sender, EventArgs e)
    {
        await PlayCurrentPoiAudioAsync();
    }

    private void OnCancelAudioClicked(object sender, EventArgs e)
    {
        _audioGuideService.Cancel();
    }

    private async void OnUnlockClicked(object sender, EventArgs e)
    {
        var confirmed = await DisplayAlert(
            LocalizationService.GetString("Info"),
            GetInAppUnlockPromptText(),
            GetUnlockButtonText(),
            LocalizationService.GetString("Cancel"));

        if (!confirmed)
        {
            return;
        }

        UnlockButton.IsEnabled = false;

        try
        {
            var unlocked = await App.Auth.PurchaseUnlockAsync();
            if (!unlocked)
            {
                await DisplayAlert(LocalizationService.GetString("Info"), GetUnlockFailedMessage(), LocalizationService.GetString("OK"));
                return;
            }

            await App.Auth.RefreshUnlockStatusAsync();
            UpdateUnlockUi();

            await DisplayAlert(LocalizationService.GetString("Info"), GetUnlockSuccessMessage(), LocalizationService.GetString("OK"));
            await PlayCurrentPoiAudioAsync();
        }
        finally
        {
            UnlockButton.IsEnabled = true;
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

    private static string GetUnlockButtonText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Unlock 10k",
        "zh" => "\u89E3\u9501 10k",
        _ => "Mo khoa 10k"
    };

    private static string GetUnlockStatusText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This POI is in preview mode. Pay 10k in the app to unlock full features on this device.",
        "zh" => "\u6B64 POI \u76EE\u524D\u4E3A\u9884\u89C8\u6A21\u5F0F\uff0C\u8BF7\u5728 App \u5185\u652F\u4ED8 10k \u540E\u89E3\u9501\u6B64\u8BBE\u5907\u7684\u5168\u90E8\u529F\u80FD\u3002",
        _ => "POI nay dang o che do xem thu. Thanh toan 10k ngay trong app de mo khoa day du chuc nang cho thiet bi nay."
    };

    private static string GetLockedAudioStatusText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Audio is locked until the 10k in-app unlock is completed.",
        "zh" => "\u5B8C\u6210 App \u5185 10k \u89E3\u9501\u524D\uff0c\u97F3\u9891\u6682\u65F6\u88AB\u9501\u5B9A\u3002",
        _ => "Audio tam khoa cho den khi hoan tat mo khoa 10k ngay trong app."
    };

    private static string GetUnlockRequiredMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Please unlock this device in the app before using audio.",
        "zh" => "\u8BF7\u5148\u5728 App \u5185\u4E3A\u8BBE\u5907\u5B8C\u6210\u89E3\u9501\uff0C\u518D\u4F7F\u7528\u97F3\u9891\u3002",
        _ => "Hay mo khoa thiet bi nay ngay trong app truoc khi dung audio."
    };

    private static string GetInAppUnlockPromptText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Confirm the demo 10k payment to unlock full app features on this device?",
        "zh" => "\u786E\u8BA4\u6A21\u62DF\u652F\u4ED8 10k\uff0c\u4E3A\u6B64\u8BBE\u5907\u89E3\u9501 App \u5168\u90E8\u529F\u80FD\uff1F",
        _ => "Xac nhan thanh toan demo 10k de mo khoa day du chuc nang app cho thiet bi nay?"
    };

    private static string GetUnlockSuccessMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Unlock completed. Full app features are now available on this device.",
        "zh" => "\u89E3\u9501\u5DF2\u5B8C\u6210\uff0C\u6B64\u8BBE\u5907\u73B0\u5728\u53EF\u4F7F\u7528 App \u5168\u90E8\u529F\u80FD\u3002",
        _ => "Mo khoa thanh cong. Thiet bi nay da duoc su dung day du chuc nang app."
    };

    private static string GetUnlockFailedMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "The unlock request could not be completed right now. Please try again.",
        "zh" => "\u5F53\u524D\u65E0\u6CD5\u5B8C\u6210\u89E3\u9501\u8BF7\u6C42\uff0C\u8BF7\u7A0D\u540E\u518D\u8BD5\u3002",
        _ => "Khong the hoan tat yeu cau mo khoa luc nay. Hay thu lai sau."
    };
}
