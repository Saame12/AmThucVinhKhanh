    namespace VinhKhanhFood.API.Models
    {
        public class FoodLocation
        {
            public int Id { get; set; }

        // --- TIẾNG VIỆT (Mặc định) ---
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? AudioUrl { get; set; }

        // --- TIẾNG ANH ---
            public string? Name_EN { get; set; }
            public string? Description_EN { get; set; }
            public string? AudioUrl_EN { get; set; } 

        // --- TIẾNG TRUNG ---
            public string? Name_ZH { get; set; }        
            public string? Description_ZH { get; set; }
            public string? AudioUrl_ZH { get; set; }

        // Các thông số kỹ thuật dùng chung
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string? ImageUrl { get; set; }

        // Thông tin quản lý (Phân quyền Vendor)
            public int? OwnerId { get; set; }
        // 🔥 NEW: trạng thái duyệt
        public string Status { get; set; } = "Pending";
    }
    }