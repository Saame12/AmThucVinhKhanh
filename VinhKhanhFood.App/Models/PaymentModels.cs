namespace VinhKhanhFood.App.Models;

public sealed class PaymentAccessResponse
{
    public int PoiId { get; set; }
    public bool HasProfessionalAudio { get; set; }
    public bool HasAccess { get; set; }
    public string AccessType { get; set; } = "NONE";
}

public sealed class MockCheckoutRequest
{
    public int PoiId { get; set; }
    public decimal Amount { get; set; }
    public int? UserId { get; set; }
    public string? GuestId { get; set; }
    public string? Provider { get; set; }
}

public sealed class MockCheckoutResult
{
    public int TransactionId { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool HasAccess { get; set; }
    public string AccessType { get; set; } = "NONE";
}
