using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication; // Thư viện để gọi điện thoại
using Microsoft.Maui.Media;
using VinhKhanhFood.App.Models;

namespace VinhKhanhFood.App;

public partial class DetailPage : ContentPage
{
    private readonly FoodLocation _currentLocation;
    private bool _isPlaying = false;
    private CancellationTokenSource _cts;

    public DetailPage(FoodLocation location)
    {
        InitializeComponent();
        _currentLocation = location;
        BindingContext = _currentLocation;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Đảm bảo tắt âm thanh khi người dùng thoát trang bằng bất kỳ cách nào
        StopAudio();
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnGetDirectionsClicked(object sender, EventArgs e)
    {
        try
        {
            var location = new Location(_currentLocation.Latitude, _currentLocation.Longitude);
            // Sử dụng DisplayName để hiển thị đúng ngôn ngữ trên bản đồ
            var options = new MapLaunchOptions { Name = _currentLocation.DisplayName };
            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not open maps.", "OK");
        }
    }

    private void OnCallStoreClicked(object sender, EventArgs e)
    {
        if (PhoneDialer.Default.IsSupported)
            PhoneDialer.Default.Open("0904567788");
        else
            DisplayAlert("Notification", "Dialer not supported on this device.", "OK");
    }

    // ==========================================
    // LOGIC THUYẾT MINH (TEXT TO SPEECH)
    // ==========================================

    private async void OnToggleAudioClicked(object sender, EventArgs e)
    {
        if (_isPlaying) StopAudio();
        else await PlayAudio();
    }

    private async Task PlayAudio()
    {
        // 1. Lấy nội dung theo ngôn ngữ hiện tại
        string textToRead = _currentLocation.DisplayDescription;

        if (string.IsNullOrWhiteSpace(textToRead))
        {
            string msg = App.CurrentLanguage == "vi" ? "Không có nội dung thuyết minh." : "No description available for this language.";
            await DisplayAlert("Info", msg, "OK");
            return;
        }

        _isPlaying = true;
        BtnPlayAudio.Text = "■";
        LblAudioStatus.Text = App.CurrentLanguage switch
        {
            "en" => "Playing Introduction...",
            "zh" => "正在播放...",
            _ => "Đang thuyết minh..."
        };

        _cts = new CancellationTokenSource();

        // Tính toán thời gian chạy ProgressBar dựa trên độ dài văn bản và tốc độ đọc
        int durationMs = (int)((textToRead.Length * 100) / SettingsPage.CurrentSpeechRate);
        AnimateProgressBar(durationMs, _cts.Token);

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            // 2. Tìm đúng giọng đọc khớp với mã ngôn ngữ (vi, en, zh)
            var currentLocale = locales.FirstOrDefault(l =>
                l.Language.StartsWith(App.CurrentLanguage, StringComparison.OrdinalIgnoreCase));

            var options = new SpeechOptions()
            {
                Volume = 1.0f,
                Locale = currentLocale,
                Pitch = 1.0f // Bạn có thể chỉnh độ trầm bổng ở đây nếu muốn
            };

            await TextToSpeech.Default.SpeakAsync(textToRead, options, cancelToken: _cts.Token);
        }
        catch (TaskCanceledException) { }
        finally
        {
            if (_isPlaying) StopAudio();
        }
    }

    private void StopAudio()
    {
        if (!_isPlaying) return;
        _cts?.Cancel();
        _isPlaying = false;
        BtnPlayAudio.Text = "▶";
        LblAudioStatus.Text = App.CurrentLanguage == "vi" ? "Nghe thuyết minh" : "Listen to Introduction";
        AudioProgressBar.Progress = 0;
    }

    private async void AnimateProgressBar(int totalDurationMs, CancellationToken token)
    {
        int delay = 100;
        int elapsed = 0;
        while (elapsed < totalDurationMs && !token.IsCancellationRequested)
        {
            await Task.Delay(delay, token);
            elapsed += delay;
            double progress = (double)elapsed / totalDurationMs;
            MainThread.BeginInvokeOnMainThread(() => AudioProgressBar.Progress = progress);
        }
    }
}