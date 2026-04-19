namespace VinhKhanhFood.API.Models;

public class PaymentTransaction
{
    public int Id { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string PaymentType { get; set; } = "QR_PAYMENT";
    public string Provider { get; set; } = "MockQR";
    public string Status { get; set; } = "Pending";
    public string CustomerLabel { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? PaidAt { get; set; }
    public DateTime? ReconciledAt { get; set; }
}
