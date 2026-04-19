namespace VinhKhanhFood.Admin.Models
{
    public class FoodLocation
    {
        public int Id { get; set; }

        // Tiếng Việt (Mặc định)
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
        // 🔥 NEW: trạng thái duyệt (PHẢI CÓ để match API)
        public string? Status { get; set; }
        // Cập nhật logic thuộc tính thông minh hỗ trợ 3 ngôn ngữ
        public bool HasDefaultAudio =>
            !string.IsNullOrWhiteSpace(Description) ||
            !string.IsNullOrWhiteSpace(Description_EN) ||
            !string.IsNullOrWhiteSpace(Description_ZH);

        public bool HasProfessionalAudio =>
            !string.IsNullOrWhiteSpace(AudioUrl) ||
            !string.IsNullOrWhiteSpace(AudioUrl_EN) ||
            !string.IsNullOrWhiteSpace(AudioUrl_ZH);

        public string QrAudioUri => $"vinhkhanhfood://poi/{Id}";
        public string QrCodeLabel => $"VK-POI-{Id:D4}";
        public string QrCodeImageUrl => $"https://quickchart.io/qr?size=260&text={Uri.EscapeDataString(QrAudioUri)}";
        public string BuildPaymentQrUri(decimal amount) =>
            $"vinhkhanhpay://payment/poi/{Id}?amount={decimal.Truncate(amount)}&name={Uri.EscapeDataString(Name)}";
        public string BuildPaymentQrCode(decimal amount) =>
            $"VK-PAY-{Id:D4}-{decimal.Truncate(amount):0000000}";
        public string BuildPaymentQrImageUrl(decimal amount) =>
            $"https://quickchart.io/qr?size=260&text={Uri.EscapeDataString(BuildPaymentQrUri(amount))}";

    }

}
