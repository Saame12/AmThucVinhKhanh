namespace VinhKhanhFood.Admin.Models
{
    public class User
    {
        public int Id { get; set; }
        public string DisplayId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Vendor";
        public string? Status { get; set; }
        public string OnlineStatus { get; set; } = "Offline";
        public bool IsVirtual { get; set; }
    }
}
