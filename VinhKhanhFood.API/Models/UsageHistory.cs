namespace VinhKhanhFood.API.Models;

public class UsageHistory
{
    public int Id { get; set; }

    // 🔥 Ai thực hiện
    public int UserId { get; set; }
    public string UserName { get; set; }

    // 🔥 Hành động
    public string Action { get; set; } = string.Empty;

    // 🔥 POI liên quan
    public int PoiId { get; set; }
    public string PoiName { get; set; }

    // 🔥 Thời gian
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string Role { get; set; } = "Owner"; // 🔥 NEW
}
