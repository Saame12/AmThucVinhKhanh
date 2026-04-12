using System.Globalization;

namespace VinhKhanhFood.App.Services;

public static class LocalizationService
{
    private const string LanguagePreferenceKey = "Language";

    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["vi"] = new()
        {
            ["Map"] = "Bản đồ",
            ["Explore"] = "Khám phá",
            ["Settings"] = "Cài đặt",
            ["Discover"] = "Khám phá",
            ["Vinh Khanh"] = "Vĩnh Khánh",
            ["Discover Places"] = "Khám phá địa điểm",
            ["Places"] = "Địa điểm",
            ["Hot POIs"] = "POIs nổi bật",
            ["FeaturedPoiWidgetTitle"] = "Gợi ý POI nổi bật",
            ["FeaturedPoiWidgetSubtitle"] = "Khối đề xuất nhanh các quán đáng ghé, đáng nghe audio và đáng quét QR.",
            ["NearbyPoiListTitle"] = "Tất cả POIs gần bạn",
            ["Find amazing local food experiences near you"] = "Khám phá trải nghiệm ẩm thực địa phương tuyệt vời gần bạn",
            ["Explore amazing food locations around you"] = "Khám phá những điểm ăn uống hấp dẫn quanh bạn",
            ["Search places..."] = "Tìm địa điểm...",
            ["All"] = "Tất cả",
            ["Seafood"] = "Hải sản",
            ["Snails"] = "Ốc",
            ["Soup"] = "Canh",
            ["Language"] = "Ngôn ngữ",
            ["LanguageDescription"] = "Ngôn ngữ giao diện và thuyết minh",
            ["SettingsTitle"] = "Cài đặt",
            ["SettingsSubtitle"] = "Cấu hình trải nghiệm Vĩnh Khánh Foodie",
            ["AudioLocalizationTitle"] = "Âm thanh và ngôn ngữ",
            ["AudioLocalizationSubtitle"] = "Thiết lập hướng dẫn viên và giao diện",
            ["AboutAccountTitle"] = "Tài khoản và thông tin",
            ["LoginToYourAccount"] = "Đăng nhập hoặc đăng ký",
            ["Logout"] = "Đăng xuất",
            ["Version Info:"] = "Phiên bản:",
            ["VersionValue"] = "v1.0.0 - Bản ứng dụng",
            ["Select Language"] = "Chọn ngôn ngữ",
            ["Choose your preferred audio language"] = "Chọn ngôn ngữ giao diện và audio",
            ["More languages coming soon in future updates"] = "Sẽ có thêm ngôn ngữ trong các bản cập nhật tiếp theo",
            ["Vietnamese"] = "Tiếng Việt",
            ["English"] = "Tiếng Anh",
            ["Chinese"] = "Tiếng Trung",
            ["Details"] = "Chi tiết",
            ["Location"] = "Vị trí",
            ["Opening Hours"] = "Giờ mở cửa",
            ["Phone"] = "Điện thoại",
            ["Audio Guide"] = "Thuyết minh âm thanh",
            ["About"] = "Giới thiệu",
            ["Highlights"] = "Điểm nổi bật",
            ["Get Directions"] = "Chỉ đường",
            ["Call Store"] = "Gọi cửa hàng",
            ["Listen to Introduction"] = "Nghe thuyết minh",
            ["AudioPlaying"] = "Đang phát thuyết minh",
            ["AudioPaused"] = "Đã tạm dừng",
            ["AudioIdle"] = "Sẵn sàng phát",
            ["AudioUnavailable"] = "POI này chưa có nội dung audio",
            ["Play"] = "Phát",
            ["Pause"] = "Dừng",
            ["Resume"] = "Tiếp tục",
            ["Cancel"] = "Hủy",
            ["Fresh seafood daily"] = "Hải sản tươi mỗi ngày",
            ["Traditional recipes since 1995"] = "Công thức gia truyền từ năm 1995",
            ["Family-friendly atmosphere"] = "Không gian thân thiện cho gia đình",
            ["MapPinTapHint"] = "Nhấn để xem chi tiết",
            ["View Details"] = "Xem chi tiết",
            ["AwayDistance"] = "gần đây",
            ["LoginTitle"] = "Đăng nhập",
            ["RegisterTitle"] = "Đăng ký",
            ["AuthSubtitle"] = "Tài khoản người dùng cho trải nghiệm cá nhân hóa",
            ["FullName"] = "Họ và tên",
            ["Username"] = "Tên đăng nhập",
            ["Password"] = "Mật khẩu",
            ["ConfirmPassword"] = "Xác nhận mật khẩu",
            ["SwitchToLogin"] = "Đã có tài khoản? Đăng nhập",
            ["SwitchToRegister"] = "Chưa có tài khoản? Đăng ký",
            ["SubmitLogin"] = "Đăng nhập",
            ["SubmitRegister"] = "Tạo tài khoản",
            ["AuthValidationRequired"] = "Vui lòng nhập đầy đủ thông tin",
            ["AuthPasswordMismatch"] = "Mật khẩu xác nhận chưa khớp",
            ["AuthUsernameExists"] = "Tên đăng nhập đã tồn tại",
            ["AuthRequestFailed"] = "Yêu cầu không thành công",
            ["AuthUnexpectedResponse"] = "Máy chủ trả về dữ liệu không hợp lệ",
            ["AuthConnectionError"] = "Không thể kết nối tới máy chủ",
            ["AuthLoginSuccess"] = "Đăng nhập thành công",
            ["AuthRegisterSuccess"] = "Đăng ký thành công",
            ["AuthBlocked"] = "Tài khoản đã bị khóa",
            ["Error"] = "Lỗi",
            ["OK"] = "Đồng ý",
            ["Info"] = "Thông báo",
            ["Notification"] = "Thông báo",
            ["No results found"] = "Không tìm thấy kết quả",
            ["AutoGuideActivated"] = "Chế độ thuyết minh tự động đã bật. Hệ thống sẽ phát audio POI khi bạn đến gần."
        },
        ["en"] = new()
        {
            ["Map"] = "Map",
            ["Explore"] = "Explore",
            ["Settings"] = "Settings",
            ["Discover"] = "Discover",
            ["Vinh Khanh"] = "Vinh Khanh",
            ["Discover Places"] = "Discover Places",
            ["Places"] = "Places",
            ["Hot POIs"] = "Hot POIs",
            ["FeaturedPoiWidgetTitle"] = "Featured POI Picks",
            ["FeaturedPoiWidgetSubtitle"] = "A curated highlight widget for the best spots to hear, scan, and explore.",
            ["NearbyPoiListTitle"] = "All nearby POIs",
            ["Find amazing local food experiences near you"] = "Find amazing local food experiences near you",
            ["Explore amazing food locations around you"] = "Explore amazing food locations around you",
            ["Search places..."] = "Search places...",
            ["All"] = "All",
            ["Seafood"] = "Seafood",
            ["Snails"] = "Snails",
            ["Soup"] = "Soup",
            ["Language"] = "Language",
            ["LanguageDescription"] = "App and narration language",
            ["SettingsTitle"] = "Settings",
            ["SettingsSubtitle"] = "Configure your Vinh Khanh Foodie experience",
            ["AudioLocalizationTitle"] = "Audio and localization",
            ["AudioLocalizationSubtitle"] = "Guide and interface preferences",
            ["AboutAccountTitle"] = "Account and about",
            ["LoginToYourAccount"] = "Login or register",
            ["Logout"] = "Logout",
            ["Version Info:"] = "Version:",
            ["VersionValue"] = "v1.0.0 - App release",
            ["Select Language"] = "Select language",
            ["Choose your preferred audio language"] = "Choose your preferred app and audio language",
            ["More languages coming soon in future updates"] = "More languages are coming in future updates",
            ["Vietnamese"] = "Vietnamese",
            ["English"] = "English",
            ["Chinese"] = "Chinese",
            ["Details"] = "Details",
            ["Location"] = "Location",
            ["Opening Hours"] = "Opening hours",
            ["Phone"] = "Phone",
            ["Audio Guide"] = "Audio guide",
            ["About"] = "About",
            ["Highlights"] = "Highlights",
            ["Get Directions"] = "Get directions",
            ["Call Store"] = "Call store",
            ["Listen to Introduction"] = "Listen to the guide",
            ["AudioPlaying"] = "Narration is playing",
            ["AudioPaused"] = "Narration paused",
            ["AudioIdle"] = "Ready to play",
            ["AudioUnavailable"] = "This POI does not have audio content yet",
            ["Play"] = "Play",
            ["Pause"] = "Pause",
            ["Resume"] = "Resume",
            ["Cancel"] = "Cancel",
            ["Fresh seafood daily"] = "Fresh seafood daily",
            ["Traditional recipes since 1995"] = "Traditional recipes since 1995",
            ["Family-friendly atmosphere"] = "Family-friendly atmosphere",
            ["MapPinTapHint"] = "Tap to view details",
            ["View Details"] = "View details",
            ["AwayDistance"] = "nearby",
            ["LoginTitle"] = "Login",
            ["RegisterTitle"] = "Register",
            ["AuthSubtitle"] = "User account for a personalized experience",
            ["FullName"] = "Full name",
            ["Username"] = "Username",
            ["Password"] = "Password",
            ["ConfirmPassword"] = "Confirm password",
            ["SwitchToLogin"] = "Already have an account? Login",
            ["SwitchToRegister"] = "Need an account? Register",
            ["SubmitLogin"] = "Login",
            ["SubmitRegister"] = "Create account",
            ["AuthValidationRequired"] = "Please complete all required information",
            ["AuthPasswordMismatch"] = "Password confirmation does not match",
            ["AuthUsernameExists"] = "This username is already in use",
            ["AuthRequestFailed"] = "Request failed",
            ["AuthUnexpectedResponse"] = "The server returned an invalid response",
            ["AuthConnectionError"] = "Unable to connect to the server",
            ["AuthLoginSuccess"] = "Login successful",
            ["AuthRegisterSuccess"] = "Registration successful",
            ["AuthBlocked"] = "This account is blocked",
            ["Error"] = "Error",
            ["OK"] = "OK",
            ["Info"] = "Info",
            ["Notification"] = "Notification",
            ["No results found"] = "No results found",
            ["AutoGuideActivated"] = "Automatic guide mode is on. The app will play POI audio when you get close."
        },
        ["zh"] = new()
        {
            ["Map"] = "地图",
            ["Explore"] = "探索",
            ["Settings"] = "设置",
            ["Discover"] = "探索",
            ["Vinh Khanh"] = "荣庆",
            ["Discover Places"] = "探索地点",
            ["Places"] = "地点",
            ["Hot POIs"] = "热门 POIs",
            ["FeaturedPoiWidgetTitle"] = "精选热门 POI",
            ["FeaturedPoiWidgetSubtitle"] = "精选展示区，快速查看值得听、值得逛、值得扫码体验的店铺。",
            ["NearbyPoiListTitle"] = "附近全部 POI",
            ["Find amazing local food experiences near you"] = "发现您附近精彩的本地美食体验",
            ["Explore amazing food locations around you"] = "探索您周围精彩的美食地点",
            ["Search places..."] = "搜索地点...",
            ["All"] = "全部",
            ["Seafood"] = "海鲜",
            ["Snails"] = "螺类",
            ["Soup"] = "汤类",
            ["Language"] = "语言",
            ["LanguageDescription"] = "界面与语音语言",
            ["SettingsTitle"] = "设置",
            ["SettingsSubtitle"] = "配置您的荣庆美食体验",
            ["AudioLocalizationTitle"] = "音频与语言",
            ["AudioLocalizationSubtitle"] = "讲解与界面偏好",
            ["AboutAccountTitle"] = "账户与信息",
            ["LoginToYourAccount"] = "登录或注册",
            ["Logout"] = "退出登录",
            ["Version Info:"] = "版本：",
            ["VersionValue"] = "v1.0.0 - 应用版本",
            ["Select Language"] = "选择语言",
            ["Choose your preferred audio language"] = "选择您偏好的界面与音频语言",
            ["More languages coming soon in future updates"] = "未来版本将支持更多语言",
            ["Vietnamese"] = "越南语",
            ["English"] = "英语",
            ["Chinese"] = "中文",
            ["Details"] = "详情",
            ["Location"] = "位置",
            ["Opening Hours"] = "营业时间",
            ["Phone"] = "电话",
            ["Audio Guide"] = "语音导览",
            ["About"] = "介绍",
            ["Highlights"] = "亮点",
            ["Get Directions"] = "导航",
            ["Call Store"] = "拨打电话",
            ["Listen to Introduction"] = "收听讲解",
            ["AudioPlaying"] = "正在播放讲解",
            ["AudioPaused"] = "已暂停",
            ["AudioIdle"] = "准备播放",
            ["AudioUnavailable"] = "该 POI 暂无音频内容",
            ["Play"] = "播放",
            ["Pause"] = "暂停",
            ["Resume"] = "继续",
            ["Cancel"] = "取消",
            ["Fresh seafood daily"] = "每日新鲜海鲜",
            ["Traditional recipes since 1995"] = "自 1995 年传承至今的传统配方",
            ["Family-friendly atmosphere"] = "适合家庭的友好环境",
            ["MapPinTapHint"] = "点击查看详情",
            ["View Details"] = "查看详情",
            ["AwayDistance"] = "附近",
            ["LoginTitle"] = "登录",
            ["RegisterTitle"] = "注册",
            ["AuthSubtitle"] = "用于个性化体验的用户账户",
            ["FullName"] = "姓名",
            ["Username"] = "用户名",
            ["Password"] = "密码",
            ["ConfirmPassword"] = "确认密码",
            ["SwitchToLogin"] = "已有账户？登录",
            ["SwitchToRegister"] = "还没有账户？注册",
            ["SubmitLogin"] = "登录",
            ["SubmitRegister"] = "创建账户",
            ["AuthValidationRequired"] = "请填写完整信息",
            ["AuthPasswordMismatch"] = "两次输入的密码不一致",
            ["AuthUsernameExists"] = "该用户名已存在",
            ["AuthRequestFailed"] = "请求失败",
            ["AuthUnexpectedResponse"] = "服务器返回了无效数据",
            ["AuthConnectionError"] = "无法连接到服务器",
            ["AuthLoginSuccess"] = "登录成功",
            ["AuthRegisterSuccess"] = "注册成功",
            ["AuthBlocked"] = "该账户已被禁用",
            ["Error"] = "错误",
            ["OK"] = "确定",
            ["Info"] = "提示",
            ["Notification"] = "通知",
            ["No results found"] = "未找到结果",
            ["AutoGuideActivated"] = "自动导览已开启，当您靠近 POI 时应用会自动播放音频。"
        }
    };

    public static event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    public static string CurrentLanguage { get; private set; } = "vi";

    public static void Initialize()
    {
        var savedLanguage = Preferences.Default.Get(LanguagePreferenceKey, "vi");
        SetLanguage(savedLanguage, false);
    }

    public static string GetString(string key, string? language = null)
    {
        var targetLanguage = language ?? CurrentLanguage;

        if (Translations.TryGetValue(targetLanguage, out var languageMap) &&
            languageMap.TryGetValue(key, out var translated))
        {
            return translated;
        }

        if (Translations["vi"].TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    public static IReadOnlyList<(string Code, string Name)> GetAvailableLanguages() =>
        new List<(string Code, string Name)>
        {
            ("vi", GetString("Vietnamese", "vi")),
            ("en", GetString("English", "en")),
            ("zh", GetString("Chinese", "zh"))
        };

    public static void SetLanguage(string language, bool persist = true)
    {
        if (!Translations.ContainsKey(language))
        {
            language = "vi";
        }

        CurrentLanguage = language;

        var culture = new CultureInfo(GetCultureCode(language));
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        if (persist)
        {
            Preferences.Default.Set(LanguagePreferenceKey, language);
        }

        LanguageChanged?.Invoke(null, new LanguageChangedEventArgs { Language = language });
    }

    public static string GetCultureCode(string language) => language switch
    {
        "en" => "en-US",
        "zh" => "zh-CN",
        _ => "vi-VN"
    };
}

public sealed class LanguageChangedEventArgs : EventArgs
{
    public string Language { get; init; } = "vi";
}
