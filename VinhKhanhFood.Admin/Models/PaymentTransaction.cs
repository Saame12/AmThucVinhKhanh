namespace VinhKhanhFood.Admin.Models;

public class PaymentTransaction
{
    public int Id { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string PaymentType { get; set; } = "QR_PAYMENT";
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CustomerLabel { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ReconciledAt { get; set; }
}

public class PaymentDashboardViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? SelectedPoiId { get; set; }
    public string? SelectedStatus { get; set; }
    public int TotalTransactions { get; set; }
    public int PaidTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int ReconciledTransactions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<FoodLocation> Pois { get; set; } = [];
    public List<PaymentPoiStat> TopPois { get; set; } = [];
    public List<PaymentProviderStat> ProviderBreakdown { get; set; } = [];
    public List<PaymentTransaction> Transactions { get; set; } = [];
}

public class PaymentPoiStat
{
    public int PoiId { get; set; }
    public string PoiName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}

public class PaymentProviderStat
{
    public string Provider { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public int PaidCount { get; set; }
    public decimal Revenue { get; set; }
}
