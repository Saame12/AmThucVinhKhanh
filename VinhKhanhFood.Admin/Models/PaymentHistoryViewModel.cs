namespace VinhKhanhFood.Admin.Models;

public sealed class PaymentHistoryViewModel
{
    public string SelectedStatus { get; set; } = "all";
    public int TotalPayments { get; set; }
    public int ActivePayments { get; set; }
    public int WebPayments { get; set; }
    public int AppPayments { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<PaymentHistoryItem> Items { get; set; } = new();
}

public sealed class PaymentHistoryItem
{
    public int Id { get; set; }
    public string GuestId { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
