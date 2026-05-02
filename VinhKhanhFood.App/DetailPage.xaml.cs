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
        PlayAudioButton.Text = LocalizationService.GetString("Play");
        CancelAudioButton.Text = LocalizationService.GetString("Cancel");
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
}
