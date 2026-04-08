using System.Globalization;

namespace VinhKhanhFood.App.Services;

public class LocalizationService
{
    public static string CurrentLanguage { get; set; } = "vi"; // Default: Vietnamese

    // Event để thông báo khi ngôn ngữ thay đổi
    public static event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        {
            "vi", new Dictionary<string, string>
            {
                // Tab titles
                { "Map", "Bản đồ" },
                { "Explore", "Khám phá" },
                { "Settings", "Cài đặt" },
                
                // Main page
                { "Discover", "Khám phá" },
                { "Vinh Khanh", "Vĩnh Khánh" },
                { "Find amazing local food experiences near you", "Tìm các trải nghiệm ẩm thực địa phương tuyệt vời gần bạn" },
                { "Search places...", "Tìm kiếm địa điểm..." },
                
                // Explore page
                { "Discover Places", "Khám phá địa điểm" },
                { "Explore amazing food locations around you", "Khám phá các địa điểm ẩm thực tuyệt vời xung quanh bạn" },
                { "All", "Tất cả" },
                { "Seafood", "Hải sản" },
                { "Snails", "Ốc" },
                { "Soup", "Canh" },
                
                // Settings page
                { "Language", "Ngôn ngữ" },
                { "Vietnamese", "Tiếng Việt" },
                { "English", "English" },
                { "Chinese", "中文" },
                { "About", "Về chúng tôi" },
                { "Version", "Phiên bản" },
                
                // Detail page
                { "Details", "Chi tiết" },
                { "Location", "Địa điểm" },
                { "Call", "Gọi" },
                { "Direction", "Chỉ đường" },
                { "Share", "Chia sẻ" },
                
                // Common
                { "Loading...", "Đang tải..." },
                { "Error", "Lỗi" },
                { "Cancel", "Hủy" },
                { "OK", "Đồng ý" },
                { "No results found", "Không tìm thấy kết quả" }
            }
        },
        {
            "en", new Dictionary<string, string>
            {
                // Tab titles
                { "Map", "Map" },
                { "Explore", "Explore" },
                { "Settings", "Settings" },
                
                // Main page
                { "Discover", "Discover" },
                { "Vinh Khanh", "Vinh Khanh" },
                { "Find amazing local food experiences near you", "Find amazing local food experiences near you" },
                { "Search places...", "Search places..." },
                
                // Explore page
                { "Discover Places", "Discover Places" },
                { "Explore amazing food locations around you", "Explore amazing food locations around you" },
                { "All", "All" },
                { "Seafood", "Seafood" },
                { "Snails", "Snails" },
                { "Soup", "Soup" },
                
                // Settings page
                { "Language", "Language" },
                { "Vietnamese", "Vietnamese" },
                { "English", "English" },
                { "Chinese", "Chinese" },
                { "About", "About" },
               
                { "Version", "Version" },
                
                // Detail page
                { "Details", "Details" },
                { "Location", "Location" },
                { "Call", "Call" },
                { "Direction", "Direction" },
                { "Share", "Share" },
                
                // Common
                { "Loading...", "Loading..." },
                { "Error", "Error" },
                { "Cancel", "Cancel" },
                { "OK", "OK" },
                { "No results found", "No results found" }
            }
        },
        {
            "zh", new Dictionary<string, string>
            {
                // Tab titles
                { "Map", "地图" },
                { "Explore", "探索" },
                { "Settings", "设置" },
                
                // Main page
                { "Discover", "探索" },
                { "Vinh Khanh", "荣香" },
                { "Find amazing local food experiences near you", "找到您附近的本地美食体验" },
                { "Search places...", "搜索地点..." },
                
                // Explore page
                { "Discover Places", "探索地点" },
                { "Explore amazing food locations around you", "探索您周围的美妙美食地点" },
                { "All", "全部" },
                { "Seafood", "海鲜" },
                { "Snails", "螺蛳" },
                { "Soup", "汤" },
                
                // Settings page
                { "Language", "语言" },
                { "Vietnamese", "越南语" },
                { "English", "English" },
                { "Chinese", "中文" },
                { "About", "关于" },
             
                { "Version", "版本" },
                
                // Detail page
                { "Details", "详细信息" },
                { "Location", "位置" },
                { "Call", "拨打" },
                { "Direction", "方向" },
                { "Share", "分享" },
                
                // Common
                { "Loading...", "加载中..." },
                { "Error", "错误" },
                { "Cancel", "取消" },
                { "OK", "确定" },
                { "No results found", "未找到结果" }
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
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(GetCultureCode(language));

            // Trigger event để thông báo ngôn ngữ đã thay đổi
            LanguageChanged?.Invoke(null, new LanguageChangedEventArgs { Language = language });
        }
    }

    public static string GetCultureCode(string language) => language switch
    {
        "vi" => "vi-VN",
        "en" => "en-US",
        "zh" => "zh-CN",
        _ => "vi-VN"
    };

    public static List<(string Code, string Name)> GetAvailableLanguages() => new()
    {
        ("vi", "Tiếng Việt"),
        ("en", "English"),
        ("zh", "中文")
    };
}

// Event args cho LanguageChanged event
public class LanguageChangedEventArgs : EventArgs
{
    public string Language { get; set; } = "vi";
}
