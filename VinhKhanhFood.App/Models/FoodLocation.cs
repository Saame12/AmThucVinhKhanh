namespace VinhKhanhFood.App.Models;

public class FoodLocation
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public string? Name_EN { get; set; }
    public string? Description_EN { get; set; }
    public string? AudioUrl_EN { get; set; }
    public string? Name_ZH { get; set; }
    public string? Description_ZH { get; set; }
    public string? AudioUrl_ZH { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public int? OwnerId { get; set; }

    public string DisplayName => App.CurrentLanguage switch
    {
        "en" => !string.IsNullOrWhiteSpace(Name_EN) ? Name_EN : Name,
        "zh" => !string.IsNullOrWhiteSpace(Name_ZH) ? Name_ZH : Name,
        _ => Name
    };

    public string DisplayDescription => App.CurrentLanguage switch
    {
        "en" => !string.IsNullOrWhiteSpace(Description_EN) ? Description_EN : (Description ?? string.Empty),
        "zh" => !string.IsNullOrWhiteSpace(Description_ZH) ? Description_ZH : (Description ?? string.Empty),
        _ => Description ?? string.Empty
    };

    public string? DisplayAudioUrl => App.CurrentLanguage switch
    {
        "en" => !string.IsNullOrWhiteSpace(AudioUrl_EN) ? AudioUrl_EN : AudioUrl,
        "zh" => !string.IsNullOrWhiteSpace(AudioUrl_ZH) ? AudioUrl_ZH : AudioUrl,
        _ => AudioUrl
    };

    public string QrAudioUri => $"vinhkhanhfood://poi/{Id}";

    public string QrCodeLabel => $"VK-POI-{Id:D4}";
    public string QrManualCode => QrCodeLabel;

    public string QrCodeImageUrl => $"https://quickchart.io/qr?size=220&text={Uri.EscapeDataString(QrAudioUri)}";

    public string BuildPaymentQrUri(decimal amount) =>
        $"vinhkhanhpay://payment/poi/{Id}?amount={decimal.Truncate(amount)}&name={Uri.EscapeDataString(Name)}";

    public string BuildPaymentQrCode(decimal amount) =>
        $"VK-PAY-{Id:D4}-{decimal.Truncate(amount):0000000}";

    public string BuildPaymentQrImageUrl(decimal amount) =>
        $"https://quickchart.io/qr?size=220&text={Uri.EscapeDataString(BuildPaymentQrUri(amount))}";

    public bool HasDefaultAudio =>
        !string.IsNullOrWhiteSpace(Description) ||
        !string.IsNullOrWhiteSpace(Description_EN) ||
        !string.IsNullOrWhiteSpace(Description_ZH);

    public bool HasProfessionalAudio =>
        !string.IsNullOrWhiteSpace(AudioUrl) ||
        !string.IsNullOrWhiteSpace(AudioUrl_EN) ||
        !string.IsNullOrWhiteSpace(AudioUrl_ZH);
}
