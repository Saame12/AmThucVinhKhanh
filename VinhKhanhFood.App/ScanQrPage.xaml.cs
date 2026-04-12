using VinhKhanhFood.App.Services;
using ZXing.Net.Maui;

namespace VinhKhanhFood.App;

public partial class ScanQrPage : ContentPage
{
    private bool _isHandlingScan;

    public ScanQrPage()
    {
        InitializeComponent();
        UpdateLocalizedTexts();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalizationService.LanguageChanged += OnLanguageChanged;
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
        BarcodeReader.IsDetecting = true;
        ScanStatusLabel.Text = GetReadyText();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationService.LanguageChanged -= OnLanguageChanged;
        BarcodeReader.IsDetecting = false;
    }

    private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateLocalizedTexts);
    }

    private void UpdateLocalizedTexts()
    {
        ScanTitleLabel.Text = GetTitleText();
        ScanSubtitleLabel.Text = GetSubtitleText();
        HintTitleLabel.Text = GetHintTitleText();
        HintBodyLabel.Text = GetHintBodyText();

        if (!_isHandlingScan)
        {
            ScanStatusLabel.Text = GetReadyText();
        }
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_isHandlingScan)
        {
            return;
        }

        var value = e.Results.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        _isHandlingScan = true;
        BarcodeReader.IsDetecting = false;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!value.StartsWith("vinhkhanhfood://poi/", StringComparison.OrdinalIgnoreCase))
            {
                ScanStatusLabel.Text = GetInvalidQrText();
                await Task.Delay(1500);
                _isHandlingScan = false;
                BarcodeReader.IsDetecting = true;
                ScanStatusLabel.Text = GetReadyText();
                return;
            }

            ScanStatusLabel.Text = GetOpeningPoiText();
            App.ReceiveQrUri(value);

            await Task.Delay(1200);
            _isHandlingScan = false;
            BarcodeReader.IsDetecting = true;
            ScanStatusLabel.Text = GetReadyText();
        });
    }

    private static string GetTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Scan Venue QR",
        "zh" => "扫描店铺二维码",
        _ => "Quét QR tại quán"
    };

    private static string GetSubtitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Point the camera at the venue QR to open its POI page and start the audio guide.",
        "zh" => "将镜头对准店铺二维码，应用会打开对应的 POI 页面并自动播放语音导览。",
        _ => "Hướng camera vào mã QR của quán để mở đúng trang POI và tự phát audio guide."
    };

    private static string GetHintTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Instant venue access",
        "zh" => "快速进入店铺内容",
        _ => "Mở nhanh nội dung của quán"
    };

    private static string GetHintBodyText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Each venue has a dedicated QR. Once scanned, the app jumps to that POI and starts narration automatically.",
        "zh" => "每个店铺都有专属二维码。扫描后，应用会打开对应的 POI 并自动播放语音介绍。",
        _ => "Mỗi quán có một mã QR riêng. Quét xong, app sẽ mở đúng POI và tự động phát phần thuyết minh."
    };

    private static string GetReadyText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Ready to scan",
        "zh" => "已准备好扫描",
        _ => "Sẵn sàng quét"
    };

    private static string GetOpeningPoiText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Opening POI and starting audio...",
        "zh" => "正在打开 POI 并播放音频...",
        _ => "Đang mở POI và phát audio..."
    };

    private static string GetInvalidQrText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This QR code is not a valid Vinh Khanh POI QR.",
        "zh" => "该二维码不是有效的 Vinh Khanh POI 二维码。",
        _ => "Đây không phải mã QR POI hợp lệ của Vĩnh Khánh."
    };
}
