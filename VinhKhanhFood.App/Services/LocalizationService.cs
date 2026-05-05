using System.Globalization;

namespace VinhKhanhFood.App.Services;

public static class LocalizationService
{
    private const string LanguagePreferenceKey = "Language";

    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        ["vi"] = new()
        {
            ["Map"] = "Ban do",
            ["Explore"] = "Kham pha",
            ["Settings"] = "Cai dat",
            ["FeaturedPoiWidgetTitle"] = "Goi y POI noi bat",
            ["FeaturedPoiWidgetSubtitle"] = "Khoi de xuat nhanh cac quan dang ghe, dang nghe audio va dang quet QR.",
            ["NearbyPoiListTitle"] = "Tat ca POI gan ban",
            ["Language"] = "Ngon ngu",
            ["AudioLocalizationTitle"] = "Am thanh va ngon ngu",
            ["AudioLocalizationSubtitle"] = "Thiet lap huong dan vien va giao dien",
            ["SettingsTitle"] = "Cai dat",
            ["Select Language"] = "Chon ngon ngu",
            ["Vietnamese"] = "Tieng Viet",
            ["English"] = "Tieng Anh",
            ["Chinese"] = "Tieng Trung",
            ["Version Info:"] = "Phien ban:",
            ["VersionValue"] = "v1.0.0 - Ban ung dung",
            ["Details"] = "Chi tiet",
            ["Audio Guide"] = "Thuyet minh am thanh",
            ["AudioPlaying"] = "Dang phat thuyet minh",
            ["AudioIdle"] = "San sang phat",
            ["Play"] = "Phat",
            ["Cancel"] = "Huy",
            ["View Details"] = "Xem chi tiet",
            ["MapPinTapHint"] = "Nhan de xem chi tiet",
            ["Info"] = "Thong bao"
        },
        ["en"] = new()
        {
            ["Map"] = "Map",
            ["Explore"] = "Explore",
            ["Settings"] = "Settings",
            ["FeaturedPoiWidgetTitle"] = "Featured POI picks",
            ["FeaturedPoiWidgetSubtitle"] = "A quick highlight widget for the best places to hear, scan, and explore.",
            ["NearbyPoiListTitle"] = "All nearby POIs",
            ["Language"] = "Language",
            ["AudioLocalizationTitle"] = "Audio and localization",
            ["AudioLocalizationSubtitle"] = "Guide and interface preferences",
            ["SettingsTitle"] = "Settings",
            ["Select Language"] = "Select language",
            ["Vietnamese"] = "Vietnamese",
            ["English"] = "English",
            ["Chinese"] = "Chinese",
            ["Version Info:"] = "Version:",
            ["VersionValue"] = "v1.0.0 - App release",
            ["Details"] = "Details",
            ["Audio Guide"] = "Audio guide",
            ["AudioPlaying"] = "Narration is playing",
            ["AudioIdle"] = "Ready to play",
            ["Play"] = "Play",
            ["Cancel"] = "Cancel",
            ["View Details"] = "View details",
            ["MapPinTapHint"] = "Tap to view details",
            ["Info"] = "Info"
        },
        ["zh"] = new()
        {
            ["Map"] = "地图",
            ["Explore"] = "探索",
            ["Settings"] = "设置",
            ["FeaturedPoiWidgetTitle"] = "精选热门 POI",
            ["FeaturedPoiWidgetSubtitle"] = "快速查看值得听、值得逛、值得扫码体验的地点。",
            ["NearbyPoiListTitle"] = "附近全部 POI",
            ["Language"] = "语言",
            ["AudioLocalizationTitle"] = "音频与语言",
            ["AudioLocalizationSubtitle"] = "讲解与界面偏好",
            ["SettingsTitle"] = "设置",
            ["Select Language"] = "选择语言",
            ["Vietnamese"] = "越南语",
            ["English"] = "英语",
            ["Chinese"] = "中文",
            ["Version Info:"] = "版本：",
            ["VersionValue"] = "v1.0.0 - 应用版本",
            ["Details"] = "详情",
            ["Audio Guide"] = "语音导览",
            ["AudioPlaying"] = "正在播放讲解",
            ["AudioIdle"] = "准备播放",
            ["Play"] = "播放",
            ["Cancel"] = "取消",
            ["View Details"] = "查看详情",
            ["MapPinTapHint"] = "点击查看详情",
            ["Info"] = "提示"
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
