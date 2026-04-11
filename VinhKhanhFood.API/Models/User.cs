namespace VinhKhanhFood.API.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        // Trong thực tế nên lưu PasswordHash, nhưng để test MVP bạn có thể để tạm Password
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Vendor";
        public string Status { get; set; } = "Active";
    }

    // Dùng để nhận dữ liệu từ App gửi lên
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Dùng để trả kết quả về cho App (Không trả lại Password)
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // Chuỗi định danh phiên đăng nhập
    }
}