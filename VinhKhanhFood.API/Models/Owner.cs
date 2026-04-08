namespace VinhKhanhFood.API.Models
{
    public class Owner
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string? BusinessDescription { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedDate { get; set; }
        public string? RejectionReason { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? TaxNumber { get; set; }
        public bool IsActive { get; set; } = true;

        // Relationship
        public ICollection<FoodLocation>? FoodLocations { get; set; }
    }
}
