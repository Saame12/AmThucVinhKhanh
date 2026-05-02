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
        public DateTime? LastSeenUtc { get; set; }
        public double? LastLatitude { get; set; }
        public double? LastLongitude { get; set; }
        public int? CurrentPoiId { get; set; }
        public string? CurrentPoiName { get; set; }
        public int? SecondaryPoiId { get; set; }
        public string? SecondaryPoiName { get; set; }
        public string? LocationZoneStatus { get; set; }
        public DateTime? LastAudioHeartbeatUtc { get; set; }
        public int? CurrentAudioPoiId { get; set; }
        public string? CurrentAudioPoiName { get; set; }
    }
}
