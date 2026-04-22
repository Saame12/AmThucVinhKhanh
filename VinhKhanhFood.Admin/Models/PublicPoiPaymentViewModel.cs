using ApiFoodLocation = VinhKhanhFood.API.Models.FoodLocation;

namespace VinhKhanhFood.Admin.Models;

public class PublicPoiPaymentViewModel
{
    public ApiFoodLocation Poi { get; set; } = new();
    public decimal FixedAmount { get; set; } = 10000m;
    public bool HasPaidAccess { get; set; }
    public string GuestId { get; set; } = string.Empty;
    public string AssetBaseUrl { get; set; } = string.Empty;
    public string? PaymentMessage { get; set; }

    public string PoiImageUrl =>
        !string.IsNullOrWhiteSpace(Poi.ImageUrl)
            ? $"{AssetBaseUrl}/images/{Poi.ImageUrl}"
            : "https://placehold.co/960x720/FFF7ED/C2410C?text=POI";

    public string? VietnameseAudioUrl =>
        !string.IsNullOrWhiteSpace(Poi.AudioUrl)
            ? $"{AssetBaseUrl}/audio/{Poi.AudioUrl}"
            : null;

    public string? EnglishAudioUrl =>
        !string.IsNullOrWhiteSpace(Poi.AudioUrl_EN)
            ? $"{AssetBaseUrl}/audio/{Poi.AudioUrl_EN}"
            : null;

    public string? ChineseAudioUrl =>
        !string.IsNullOrWhiteSpace(Poi.AudioUrl_ZH)
            ? $"{AssetBaseUrl}/audio/{Poi.AudioUrl_ZH}"
            : null;
}
