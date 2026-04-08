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
        // Cập nhật Slider
        var savedRate = Preferences.Default.Get("SpeechRate", 1.0f);
        SpeedSlider.Value = savedRate;

        // Highlight ngôn ngữ hiện tại khi trang mở
        string currentLanguage = App.CurrentLanguage ?? "vi";

        if (currentLanguage == "vi")
        {
            OptVietnamese.BackgroundColor = Color.FromArgb("#33FF9800");
            OptEnglish.BackgroundColor = Colors.Transparent;
            OptChinese.BackgroundColor = Colors.Transparent;
            LblCurrentLanguage.Text = "VN Tiếng Việt";
        }
        else if (currentLanguage == "en")
        {
            OptEnglish.BackgroundColor = Color.FromArgb("#33FF9800");
            OptVietnamese.BackgroundColor = Colors.Transparent;
            OptChinese.BackgroundColor = Colors.Transparent;
            LblCurrentLanguage.Text = "GB English";
        }
        else if (currentLanguage == "zh")
        {
            OptChinese.BackgroundColor = Color.FromArgb("#33FF9800");
            OptVietnamese.BackgroundColor = Colors.Transparent;
            OptEnglish.BackgroundColor = Colors.Transparent;
            LblCurrentLanguage.Text = "CN 中文";
        }

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

    // Khi chọn 1 ngôn ngữ trong danh sách
    private void OnLanguageSelected(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.GestureRecognizers[0] is TapGestureRecognizer tap && tap.CommandParameter is string selectedLang)
        {
            LblCurrentLanguage.Text = selectedLang;

            // Xác định mã ngôn ngữ dựa trên CommandParameter
            string langCode = selectedLang.Contains("VN") ? "vi" :
                              selectedLang.Contains("GB") ? "en" : "zh";

            // Cập nhật trạng thái App
            App.CurrentLanguage = langCode;
            Services.LocalizationService.SetLanguage(langCode);

            // Cập nhật màu nền highlight một cách tự động
            UpdateLanguageUI(langCode);

            // Gọi hàm đóng bảng
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