using VinhKhanhFood.API.Models;

namespace VinhKhanhFood.API.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedDatabase(AppDbContext context)
        {
            // Check if data already exists
            if (context.Owners.Any() || context.FoodLocations.Any())
            {
                return;
            }

            // Create 3 owners
            var owner1 = new Owner
            {
                Username = "owner_pho_vinh",
                Password = "password123", // TODO: Hash in production
                Email = "owner1@phuvinh.com",
                PhoneNumber = "0123456789",
                FullName = "Nguyễn Văn An",
                BusinessName = "Phở Vĩnh Khánh",
                BusinessDescription = "Quán phở truyền thống hơn 20 năm",
                Address = "123 Đường Lê Lợi, TP Hồ Chí Minh",
                Latitude = 10.7769,
                Longitude = 106.6966,
                IdentificationNumber = "123456789",
                TaxNumber = "TAX123456789",
                Status = "Approved",
                RegistrationDate = DateTime.UtcNow.AddDays(-30),
                ApprovedDate = DateTime.UtcNow.AddDays(-25),
                IsActive = true
            };

            var owner2 = new Owner
            {
                Username = "owner_com_tam",
                Password = "password123",
                Email = "owner2@comtam.com",
                PhoneNumber = "0987654321",
                FullName = "Trần Thị Bình",
                BusinessName = "Cơm Tấm Sài Gòn",
                BusinessDescription = "Cơm tấm nổi tiếng với sườn nướng",
                Address = "456 Đường Nguyễn Hữu Cảnh, TP Hồ Chí Minh",
                Latitude = 10.8050,
                Longitude = 106.7300,
                IdentificationNumber = "987654321",
                TaxNumber = "TAX987654321",
                Status = "Approved",
                RegistrationDate = DateTime.UtcNow.AddDays(-20),
                ApprovedDate = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            };

            var owner3 = new Owner
            {
                Username = "owner_banh_mi",
                Password = "password123",
                Email = "owner3@banhmi.com",
                PhoneNumber = "0912345678",
                FullName = "Lê Minh Khoa",
                BusinessName = "Bánh Mì Thọ",
                BusinessDescription = "Bánh mì ba tế ngon nhất quận 1",
                Address = "789 Đường Hồ Tùng Mậu, TP Hồ Chí Minh",
                Latitude = 10.7610,
                Longitude = 106.6790,
                IdentificationNumber = "456789123",
                TaxNumber = "TAX456789123",
                Status = "Pending", // Chờ duyệt
                RegistrationDate = DateTime.UtcNow.AddDays(-5),
                IsActive = true
            };

            context.Owners.AddRange(owner1, owner2, owner3);
            context.SaveChanges();

            // Create 2 food locations linked to owners
            var foodLocation1 = new FoodLocation
            {
                Name = "Phở Đặc Biệt",
                Description = "Phở bò tươi ngon nhất Sài Gòn",
                Name_EN = "Special Beef Pho",
                Description_EN = "The best fresh beef pho in Ho Chi Minh City",
                Name_ZH = "特殊牛肉河粉",
                Description_ZH = "胡志明市最新鲜的牛肉河粉",
                Latitude = 10.7769,
                Longitude = 106.6966,
                ImageUrl = "pho.jpg",
                AudioUrl = "pho_vi.mp3",
                AudioUrl_EN = "pho_en.mp3",
                AudioUrl_ZH = "pho_zh.mp3",
                OwnerId = owner1.Id,
                Status = "Approved",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                ApprovedDate = DateTime.UtcNow.AddDays(-18)
            };

            var foodLocation2 = new FoodLocation
            {
                Name = "Sườn Nướng Cơm Tấm",
                Description = "Cơm tấm với sườn nướng tuyệt vời",
                Name_EN = "Grilled Ribs Broken Rice",
                Description_EN = "Broken rice with perfectly grilled ribs",
                Name_ZH = "烤排骨碎米",
                Description_ZH = "完美烤排骨的碎米",
                Latitude = 10.8050,
                Longitude = 106.7300,
                ImageUrl = "comtam.jpg",
                AudioUrl = "comtam_vi.mp3",
                AudioUrl_EN = "comtam_en.mp3",
                AudioUrl_ZH = "comtam_zh.mp3",
                OwnerId = owner2.Id,
                Status = "Approved",
                CreatedDate = DateTime.UtcNow.AddDays(-15),
                ApprovedDate = DateTime.UtcNow.AddDays(-13)
            };

            context.FoodLocations.AddRange(foodLocation1, foodLocation2);
            context.SaveChanges();
        }
    }
}
