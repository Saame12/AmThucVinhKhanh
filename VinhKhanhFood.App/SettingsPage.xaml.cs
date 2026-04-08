namespace VinhKhanhFood.App;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Đọc lại ngôn ngữ và tốc độ từ Preferences nếu có
        var savedLang = App.CurrentLanguage;
        // Cập nhật màu sắc highlight cho các Option dựa trên App.CurrentLanguage
        UpdateLanguageUI(savedLang);
        // 1. Đồng bộ Tốc độ đọc
        var savedRate = Preferences.Default.Get("UserSpeechSpeed", 1.0f);
        SpeedSlider.Value = savedRate;
        LblSpeedValue.Text = $"{savedRate:F1}x";
        CurrentSpeechRate = savedRate;

        // Highlight ngôn ngữ hiện tại khi trang mở
        string currentLanguage = App.CurrentLanguage ?? "vi";
        RefreshLanguageDisplay(currentLanguage);

    }

    // ==========================================
    // LOGIC: AUDIO & LOCALIZATION
    // ==========================================
   
    // SỬA LỖI: Đảm bảo đúng tên và tham số (object sender, TappedEventArgs e)
    private async void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        LanguageModal.IsVisible = true;
        LanguageModal.Opacity = 0;
        await LanguageModal.FadeTo(1, 200, Easing.SinOut);
    }
    public static float CurrentSpeechRate { get; set; } = 1.0f;
    // Khi kéo thanh Slider tốc độ đọc
    private void OnSpeedSliderValueChanged(object sender, ValueChangedEventArgs e)
    {    
        // 1. Làm tròn và hiển thị UI
        float speed = (float)Math.Round(e.NewValue, 1);
        LblSpeedValue.Text = $"{speed:F1}x";

        // 2. Cập nhật biến static để các trang khác (DetailPage) bốc dữ liệu dùng ngay
        CurrentSpeechRate = speed;

        // 3. Lưu lại vào bộ nhớ máy (để khi tắt app mở lại vẫn còn 1.2x hay 1.5x)
        Preferences.Default.Set("UserSpeechSpeed", speed);
    }

    // Đóng bảng Modal chọn ngôn ngữ
    private async void OnCloseModalTapped(object sender, EventArgs e)
    {
        await LanguageModal.FadeTo(0, 200, Easing.SinIn);
        LanguageModal.IsVisible = false;
    }
    private void RefreshLanguageDisplay(string langCode)
    {
        // Cập nhật Text hiển thị
        LblCurrentLanguage.Text = langCode switch
        {
            "en" => "GB English",
            "zh" => "CN 中文",
            _ => "VN Tiếng Việt"
        };

        // Cập nhật màu sắc Highlight
        OptVietnamese.BackgroundColor = langCode == "vi" ? Color.FromArgb("#33FF9800") : Colors.Transparent;
        OptEnglish.BackgroundColor = langCode == "en" ? Color.FromArgb("#33FF9800") : Colors.Transparent;
        OptChinese.BackgroundColor = langCode == "zh" ? Color.FromArgb("#33FF9800") : Colors.Transparent;
    }
    // Khi chọn 1 ngôn ngữ trong danh sách
    private void OnLanguageSelected(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string langCode)
        {
            // Cập nhật logic hệ thống
            App.CurrentLanguage = langCode;
            Services.LocalizationService.SetLanguage(langCode);
            Preferences.Default.Set("Language", langCode);

            // Cập nhật giao diện
            RefreshLanguageDisplay(langCode);

            // Đóng modal
            OnCloseModalTapped(this, EventArgs.Empty);
        }
    }

    private void UpdateLanguageUI(string langCode)
    {
        OptVietnamese.BackgroundColor = langCode == "vi" ? Color.FromArgb("#33FF9800") : Colors.Transparent;
        OptEnglish.BackgroundColor = langCode == "en" ? Color.FromArgb("#33FF9800") : Colors.Transparent;
        OptChinese.BackgroundColor = langCode == "zh" ? Color.FromArgb("#33FF9800") : Colors.Transparent;
    }

    // ==========================================
    // LOGIC: APP PREFERENCES
    // ==========================================
}