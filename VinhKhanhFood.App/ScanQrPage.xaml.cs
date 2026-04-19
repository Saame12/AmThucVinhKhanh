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
        _ = InitializeScannerAsync();
        ScanStatusLabel.Text = string.Empty;
    }

    private async Task InitializeScannerAsync()
    {
        var cameraPermission = await Permissions.RequestAsync<Permissions.Camera>();
        if (cameraPermission != PermissionStatus.Granted)
        {
            BarcodeReader.IsDetecting = false;
            ScanStatusLabel.Text = GetCameraPermissionRequiredText();
            return;
        }

        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
        BarcodeReader.IsDetecting = true;
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
        ManualQrButton.Text = GetManualQrButtonText();

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

        _ = HandleQrValueAsync(value);
    }

    private async void OnManualQrClicked(object sender, EventArgs e)
    {
        if (_isHandlingScan)
        {
            return;
        }

        var enteredValue = await DisplayPromptAsync(
            GetManualQrPromptTitle(),
            GetManualQrPromptMessage(),
            accept: GetConfirmText(),
            cancel: LocalizationService.GetString("Cancel"),
            placeholder: "VK-POI-0001 / vinhkhanhfood://poi/1",
            maxLength: 200,
            keyboard: Keyboard.Url);

        if (string.IsNullOrWhiteSpace(enteredValue))
        {
            return;
        }

        await HandleQrValueAsync(enteredValue.Trim());
    }

    private async Task HandleQrValueAsync(string value)
    {
        if (_isHandlingScan)
        {
            return;
        }

        _isHandlingScan = true;
        BarcodeReader.IsDetecting = false;

        try
        {
            ScanStatusLabel.Text = GetOpeningPoiText();
            var result = await App.OpenQrAsync(value);
            ScanStatusLabel.Text = result switch
            {
                App.QrOpenResult.Success or App.QrOpenResult.Pending => GetOpeningPoiText(),
                App.QrOpenResult.NotFound => GetPoiNotFoundText(),
                App.QrOpenResult.Unavailable => GetServerUnavailableText(),
                _ => GetInvalidQrText()
            };

            await Task.Delay(1200);
        }
        finally
        {
            _isHandlingScan = false;
            BarcodeReader.IsDetecting = true;
            ScanStatusLabel.Text = string.Empty;
        }
    }

    private static string GetManualQrButtonText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Enter QR manually",
        "zh" => "\u624B\u52A8\u8F93\u5165 QR",
        _ => "Nhap ma QR thu cong"
    };

    private static string GetManualQrPromptTitle() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Manual QR test",
        "zh" => "\u624B\u52A8 QR \u6D4B\u8BD5",
        _ => "Test QR thu cong"
    };

    private static string GetManualQrPromptMessage() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Paste the POI QR link or enter the POI code to simulate a scan on the emulator.",
        "zh" => "\u7C98\u8D34 POI QR \u94FE\u63A5\u6216\u8F93\u5165 POI \u4EE3\u7801\uff0C\u5728\u6A21\u62DF\u5668\u4E0A\u6A21\u62DF\u626B\u7801\u3002",
        _ => "Dan lien ket QR hoac nhap ma POI de mo phong quet tren gia lap."
    };

    private static string GetConfirmText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Open POI",
        "zh" => "\u6253\u5F00 POI",
        _ => "Mo POI"
    };

    private static string GetOpeningPoiText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Opening POI and starting audio...",
        "zh" => "\u6B63\u5728\u6253\u5F00 POI \u5E76\u64AD\u653E\u97F3\u9891...",
        _ => "Dang mo POI va phat audio..."
    };

    private static string GetInvalidQrText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Invalid POI QR",
        "zh" => "POI QR \u65E0\u6548",
        _ => "Ma QR POI khong hop le"
    };

    private static string GetPoiNotFoundText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "POI not found",
        "zh" => "\u672A\u627E\u5230 POI",
        _ => "Khong tim thay POI"
    };

    private static string GetServerUnavailableText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Server is unavailable",
        "zh" => "\u670D\u52A1\u5668\u65E0\u6CD5\u8FDE\u63A5",
        _ => "Server dang khong san sang"
    };

    private static string GetCameraPermissionRequiredText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Allow camera access to scan QR codes.",
        "zh" => "\u8BF7\u5141\u8BB8\u76F8\u673A\u6743\u9650\u4EE5\u626B\u63CF QR \u7801\u3002",
        _ => "Hay cap quyen camera de quet ma QR."
    };
}
