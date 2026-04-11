namespace VinhKhanhFood.Admin.Services;

public class LocalizationService
{
    public static string CurrentLanguage { get; set; } = "vi"; // Default: Vietnamese

    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        {
            "vi", new Dictionary<string, string>
            {
                // Navigation
                { "Home", "Trang chủ" },
                { "Privacy", "Chính sách riêng tư" },
                { "Admin", "Quản trị viên" },
                
                // Home page
                { "Welcome", "Chào mừng" },
                { "Welcome to Vinh Khanh Food Admin", "Chào mừng đến Quản lý Vinh Khánh" },
                { "Manage your food locations and content", "Quản lý các địa điểm ẩm thực của bạn" },
                
                // Account
                { "Login", "Đăng nhập" },
                { "Logout", "Đăng xuất" },
                { "Username", "Tên đăng nhập" },
                { "Password", "Mật khẩu" },
                { "Remember me", "Ghi nhớ tôi" },
                
                // Messages
                { "Loading...", "Đang tải..." },
                { "Error", "Lỗi" },
                { "Success", "Thành công" },
                { "Cancel", "Hủy" },
                { "Save", "Lưu" },
                { "Delete", "Xóa" },
                { "Edit", "Sửa" },
                { "Add", "Thêm" },

                // Thêm vào Dictionary "vi"
{ "AutoTranslate", "Dịch tự động" },
{ "GenerateAudio", "Tạo giọng đọc (TTS)" },
{ "Vietnamese", "Tiếng Việt" },
{ "English", "Tiếng Anh" },
{ "Chinese", "Tiếng Trung" },
{ "Processing", "Đang xử lý..." },

            }
        },
        {
            "en", new Dictionary<string, string>
            {
                // Navigation
                { "Home", "Home" },
                { "Privacy", "Privacy" },
                { "Admin", "Admin" },
                
                // Home page
                { "Welcome", "Welcome" },
                { "Welcome to Vinh Khanh Food Admin", "Welcome to Vinh Khanh Food Admin" },
                { "Manage your food locations and content", "Manage your food locations and content" },
                
                // Account
                { "Login", "Login" },
                { "Logout", "Logout" },
                { "Username", "Username" },
                { "Password", "Password" },
                { "Remember me", "Remember me" },
                
                // Messages
                { "Loading...", "Loading..." },
                { "Error", "Error" },
                { "Success", "Success" },
                { "Cancel", "Cancel" },
                { "Save", "Save" },
                { "Delete", "Delete" },
                { "Edit", "Edit" },
                { "Add", "Add" },

                // Thêm vào Dictionary "en"
{ "AutoTranslate", "Auto Translate" },
{ "GenerateAudio", "Generate Voice" },
{ "Vietnamese", "Vietnamese" },
{ "English", "English" },
{ "Chinese", "Chinese" },
{ "Processing", "Processing..." },
            }
        },
        {
            "zh", new Dictionary<string, string>
            {
                // Navigation
                { "Home", "主页" },
                { "Privacy", "隐私" },
                { "Admin", "管理员" },
                
                // Home page
                { "Welcome", "欢迎" },
                { "Welcome to Vinh Khanh Food Admin", "欢迎来到荣香美食管理员" },
                { "Manage your food locations and content", "管理您的美食地点和内容" },
                
                // Account
                { "Login", "登录" },
                { "Logout", "登出" },
                { "Username", "用户名" },
                { "Password", "密码" },
                { "Remember me", "记住我" },
                
                // Messages
                { "Loading...", "加载中..." },
                { "Error", "错误" },
                { "Success", "成功" },
                { "Cancel", "取消" },
                { "Save", "保存" },
                { "Delete", "删除" },
                { "Edit", "编辑" },
                { "Add", "添加" },

                // Thêm vào Dictionary "zh"
{ "AutoTranslate", "自动翻译" },
{ "GenerateAudio", "生成语音" },
{ "Vietnamese", "越南语" },
{ "English", "英语" },
{ "Chinese", "中文" },
{ "Processing", "正在处理..." },
            }
        }
    };

    public static string GetString(string key, string? language = null)
    {
        language ??= CurrentLanguage;

        if (Translations.TryGetValue(language, out var langDict))
        {
            if (langDict.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        // Fallback to Vietnamese
        if (Translations["vi"].TryGetValue(key, out var viValue))
        {
            return viValue;
        }

        return key; // Return key if translation not found
    }

    public static void SetLanguage(string language)
    {
        if (Translations.ContainsKey(language))
        {
            CurrentLanguage = language;
        }
    }

    public static List<(string Code, string Name)> GetAvailableLanguages() => new()
    {
        ("vi", "Tiếng Việt"),
        ("en", "English"),
        ("zh", "中文")
    };
}
