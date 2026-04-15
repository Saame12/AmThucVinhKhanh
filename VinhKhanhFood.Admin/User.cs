namespace VinhKhanhFood.Admin.Models
{
    public class UsageHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }

        public string? Action { get; set; }

        public int PoiId { get; set; }
        public string? PoiName { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? Role { get; set; } // 🔥 NEW
    }
}
