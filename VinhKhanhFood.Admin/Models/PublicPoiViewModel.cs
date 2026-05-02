namespace VinhKhanhFood.Admin.Models;

public class PublicPoiViewModel
{
    public int PoiId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool HasPaid { get; set; }
    public string PaymentQrCode { get; set; } = string.Empty;
    public string PaymentQrImageUrl { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 50000m;
}

public class PaymentVerificationRequest
{
    public int PoiId { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
}
