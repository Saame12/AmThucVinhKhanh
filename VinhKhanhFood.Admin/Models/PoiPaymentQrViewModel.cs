using ApiFoodLocation = VinhKhanhFood.API.Models.FoodLocation;

namespace VinhKhanhFood.Admin.Models;

public class PoiPaymentQrViewModel
{
    public ApiFoodLocation Poi { get; set; } = new();
    public decimal FixedAmount { get; set; } = 10000m;
    public string PublicPaymentUrl { get; set; } = string.Empty;
    public bool IsLocalDemoUrl { get; set; }
    public string QrImageUrl { get; set; } = string.Empty;
    public string QrCodeLabel { get; set; } = string.Empty;
}
