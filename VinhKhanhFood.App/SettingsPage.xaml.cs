using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VinhKhanhFood.App;

public partial class SettingsPage : ContentPage , INotifyPropertyChanged
{
    // Biến static để DetailPage bốc dữ liệu dùng ngay
    public static float CurrentSpeechRate { get; set; } = 1.0f;

    // --- Thuộc tính Binding cho Account ---
    public bool IsLoggedIn => Preferences.Default.Get("IsLoggedIn", false);
    public bool IsNotLoggedIn => !IsLoggedIn;
    public string CurrentUserName => Preferences.Default.Get("UserName", "Guest");
    public string CurrentUserRole => Preferences.Default.Get("UserRole", "Visitor");

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = this; // Rất quan trọng để IsVisible hoạt động
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. Đồng bộ Ngôn ngữ
        string currentLanguage = App.CurrentLanguage ?? "vi";
        RefreshLanguageDisplay(currentLanguage);

        // 2. Đồng bộ Tốc độ đọc
        float savedRate = Preferences.Default.Get("UserSpeechSpeed", 1.0f);
        SpeedSlider.Value = savedRate;
        LblSpeedValue.Text = $"{savedRate:F1}x";
        CurrentSpeechRate = savedRate;

        // 3. Cập nhật trạng thái User (Đăng nhập/Đăng xuất)
        RefreshUserStatus();
    }

    // ==========================================
    // LOGIC: AUDIO & LOCALIZATION
    // ==========================================

    private async void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        LanguageModal.IsVisible = true;
        LanguageModal.Opacity = 0;
        await LanguageModal.FadeTo(1, 200, Easing.SinOut);
    }

    private void OnSpeedSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        float speed = (float)Math.Round(e.NewValue, 1);
        LblSpeedValue.Text = $"{speed:F1}x";
        CurrentSpeechRate = speed;
        Preferences.Default.Set("UserSpeechSpeed", speed);
    }

    private async void OnCloseModalTapped(object sender, EventArgs e)
    {
        await LanguageModal.FadeTo(0, 150, Easing.SinIn);
        LanguageModal.IsVisible = false;
    }

    private void OnLanguageSelected(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string langCode)
        {
            App.CurrentLanguage = langCode;
            Services.LocalizationService.SetLanguage(langCode);
            Preferences.Default.Set("Language", langCode);

            RefreshLanguageDisplay(langCode);
            OnCloseModalTapped(this, EventArgs.Empty);
        }
    }

    private void RefreshLanguageDisplay(string langCode)
    {
        // Cập nhật text hiển thị trên Setting chính
        LblCurrentLanguage.Text = langCode switch
        {
            "en" => "GB English",
            "zh" => "CN 中文",
            _ => "VN Tiếng Việt"
        };

        // Highlight màu nền cho các Option trong Modal
        var highlightColor = Color.FromArgb("#33FF9800"); // Màu cam mờ
        OptVietnamese.BackgroundColor = langCode == "vi" ? highlightColor : Colors.Transparent;
        OptEnglish.BackgroundColor = langCode == "en" ? highlightColor : Colors.Transparent;
        OptChinese.BackgroundColor = langCode == "zh" ? highlightColor : Colors.Transparent;
    }

    // ==========================================
    // LOGIC: ACCOUNT (USER THẬT)
    // ==========================================

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Điều hướng đến trang Đăng nhập (LoginPage)
        // Dùng PushModalAsync để hiện đè lên trang hiện tại
        await Navigation.PushModalAsync(new LoginPage());
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        // Xóa sạch thông tin lưu trong máy
        Preferences.Default.Set("IsLoggedIn", false);
        Preferences.Default.Remove("UserName");
        Preferences.Default.Remove("UserRole");
        Preferences.Default.Remove("UserToken");

        RefreshUserStatus();
    }

    private void RefreshUserStatus()
    {
        // Thông báo cho UI cập nhật lại các vùng IsVisible
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsNotLoggedIn));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentUserRole));
    }

    // ==========================================
    // NOTIFY PROPERTY CHANGED HELPER
    // ==========================================
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}