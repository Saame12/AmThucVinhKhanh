using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class DetailPage : ContentPage
{
    private readonly FoodLocation _currentLocation;
    private readonly bool _autoPlayAudio;
    private readonly AudioGuideService _audioGuideService = App.AudioGuide;

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
        ScanBadgeLabel.Text = "QR";
        PlayAudioButton.Text = LocalizationService.GetString("Play");
        CancelAudioButton.Text = LocalizationService.GetString("Cancel");
        CopyQrButton.Text = GetQrCopyLabel();
        QrTitleLabel.Text = GetQrTitle();
        QrInstructionLabel.Text = GetQrInstruction();
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

    private async void OnCopyQrClicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(_currentLocation.QrAudioUri);
        await DisplayAlert(LocalizationService.GetString("Info"), GetQrCopiedMessage(), LocalizationService.GetString("OK"));
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private static string GetHeaderEyebrowText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "POI detail",
        "zh" => "POI 详情",
        _ => "Chi tiết POI"
    };

    private static string GetHeaderTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Audio and QR",
        "zh" => "音频与 QR",
        _ => "Audio và QR"
    };

    private static string GetGuideBadgeText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Guide",
        "zh" => "导览",
        _ => "Guide"
    };

    private static string GetQrTitle() => LocalizationService.CurrentLanguage switch
    {
        "en" => "POI QR audio",
        "zh" => "POI 音频二维码",
        _ => "Mã QR audio của quán"
    };

    private static string GetQrInstruction() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Each venue can use this QR. When scanned, the app opens this POI and starts the audio guide automatically.",
        "zh" => "每个店铺都可以使用这个二维码。扫描后，应用会打开对应 POI 并自动播放语音介绍。",
        _ => "Mỗi quán có thể dùng mã QR này. Khi quét, app sẽ mở đúng POI và tự phát phần thuyết minh."
    };

    private static string GetQrCopyLabel() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Copy QR link",
        "zh" => "复制二维码链接",
        _ => "Sao chép link QR"
    };

    private static string GetQrCopiedMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "The QR link has been copied.",
        "zh" => "二维码链接已复制。",
        _ => "Đã sao chép link QR."
    };
}
