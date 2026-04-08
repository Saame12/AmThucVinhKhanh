namespace VinhKhanhFood.Admin.Models
{
    public class AddFoodLocationViewModel
    {
        public int OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Name_EN { get; set; }
        public string? Description_EN { get; set; }
        public string? Name_ZH { get; set; }
        public string? Description_ZH { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }
    }
}
