using VinhKhanhFood.App.Models;
using VinhKhanhFood.App.Services;

namespace VinhKhanhFood.App;

public partial class PaymentCheckoutPage : ContentPage
{
    private readonly FoodLocation _poi;
    private readonly decimal _amount;
    private readonly PaymentService _paymentService = new();

    public PaymentCheckoutPage(FoodLocation poi, decimal amount)
    {
        InitializeComponent();
        _poi = poi;
        _amount = amount;
        UpdateUi();
    }

    private void UpdateUi()
    {
        Title = GetTitleText();
        HeaderEyebrowLabel.Text = GetEyebrowText();
        HeaderTitleLabel.Text = GetTitleText();
        PoiNameLabel.Text = _poi.DisplayName;
        DescriptionLabel.Text = GetDescriptionText();
        AmountLabel.Text = $"{_amount:N0} VND";
        PaymentQrImage.Source = _poi.BuildPaymentQrImageUrl(_amount);
        QrCodeLabel.Text = _poi.BuildPaymentQrCode(_amount);
        HintLabel.Text = GetHintText();
        ConfirmPaymentButton.Text = GetConfirmButtonText();
    }

    private async void OnConfirmPaymentClicked(object sender, EventArgs e)
    {
        ConfirmPaymentButton.IsEnabled = false;
        try
        {
            var result = await _paymentService.MockCheckoutAsync(_poi.Id, _amount);
            if (!result.Success)
            {
                await DisplayAlert(LocalizationService.GetString("Error"), result.ErrorMessage ?? LocalizationService.GetString("AuthRequestFailed"), LocalizationService.GetString("OK"));
                return;
            }

            await DisplayAlert(LocalizationService.GetString("Info"), GetPaymentSuccessText(), LocalizationService.GetString("OK"));
            await Navigation.PushAsync(new DetailPage(_poi));
            Navigation.RemovePage(this);
        }
        finally
        {
            ConfirmPaymentButton.IsEnabled = true;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private static string GetEyebrowText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "QR payment",
        "zh" => "QR \u652F\u4ED8",
        _ => "Thanh toan QR"
    };

    private static string GetTitleText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Unlock professional audio",
        "zh" => "\u89E3\u9501\u4E13\u4E1A\u97F3\u9891",
        _ => "Mo khoa audio chuyen nghiep"
    };

    private static string GetDescriptionText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "This mock QR payment unlocks the uploaded professional audio for this POI.",
        "zh" => "\u6B64 mock QR \u652F\u4ED8\u5C06\u89E3\u9501\u8BE5 POI \u7684\u4E0A\u4F20\u4E13\u4E1A\u97F3\u9891\u3002",
        _ => "QR payment mock nay se mo khoa audio chuyen nghiep da upload cua POI nay."
    };

    private static string GetHintText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "For the thesis demo, press the button below to simulate a successful QR payment and save the transaction to the payment dashboard.",
        "zh" => "\u5728\u6BD5\u4E1A\u8BBA\u6587 Demo \u4E2D\uff0C\u8BF7\u70B9\u51FB\u4E0B\u65B9\u6309\u94AE\u6A21\u62DF QR \u652F\u4ED8\u6210\u529F\uff0C\u5E76\u5C06\u4EA4\u6613\u4FDD\u5B58\u5230 Payment Dashboard\u3002",
        _ => "Trong demo do an, bam nut duoi day de gia lap thanh toan QR thanh cong va luu giao dich vao Payment Dashboard."
    };

    private static string GetConfirmButtonText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Simulate successful payment",
        "zh" => "\u6A21\u62DF\u652F\u4ED8\u6210\u529F",
        _ => "Gia lap thanh toan thanh cong"
    };

    private static string GetPaymentSuccessText() => LocalizationService.CurrentLanguage switch
    {
        "en" => "Payment saved and professional audio has been unlocked for this POI.",
        "zh" => "\u652F\u4ED8\u5DF2\u8BB0\u5F55\uff0C\u8BE5 POI \u7684\u4E13\u4E1A\u97F3\u9891\u5DF2\u89E3\u9501\u3002",
        _ => "Giao dich da duoc luu va audio chuyen nghiep cua POI nay da duoc mo khoa."
    };
}
