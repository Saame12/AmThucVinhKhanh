namespace VinhKhanhFood.API.Models;

public class Subscription
{
    public int Id { get; set; }
    public string GuestId { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string ClaimToken { get; set; } = string.Empty;
    public string? ClaimedGuestId { get; set; }
    public DateTime? ClaimedAtUtc { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Expired
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
