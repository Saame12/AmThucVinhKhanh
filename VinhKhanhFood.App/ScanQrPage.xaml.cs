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
        ScanStatusLabel.Text = string.Empty;
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
        if (!_isHandlingScan)
        {
            ScanStatusLabel.Text = string.Empty;
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
                ScanStatusLabel.Text = string.Empty;
                return;
            }

            ScanStatusLabel.Text = GetOpeningPoiText();
            App.ReceiveQrUri(value);

            await Task.Delay(1200);
            _isHandlingScan = false;
            BarcodeReader.IsDetecting = true;
            ScanStatusLabel.Text = string.Empty;
        });
    }

    private static string GetOpeningPoiText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Opening POI and starting audio...",
        "zh" => "正在打开 POI 并播放音频...",
        _ => "Đang mở POI và phát audio..."
    };

    private static string GetInvalidQrText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Invalid POI QR",
        "zh" => "POI QR 无效",
        _ => "Mã QR POI không hợp lệ"
    };
}
