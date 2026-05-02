# Localization Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    VinhKhanhFood Application                      │
├──────────────────────────────────┬──────────────────────────────┤
│         MAUI Mobile App           │    Admin Web Application      │
├──────────────────────────────────┼──────────────────────────────┤
│                                   │                               │
│  LocalizationService             │   LocalizationService        │
│  ┌──────────────────────┐        │   ┌──────────────────────┐  │
│  │ Translation Keys:    │        │   │ Translation Keys:    │  │
│  │ - Map                │        │   │ - Home               │  │
│  │ - Explore            │        │   │ - Privacy            │  │
│  │ - Settings           │        │   │ - Login              │  │
│  │ - 40+ more...        │        │   │ - 25+ more...        │  │
│  └──────────────────────┘        │   └──────────────────────┘  │
│                                   │                               │
│  Supported Languages:             │   Supported Languages:       │
│  - vi (Vietnamese)                │   - vi (Vietnamese)          │
│  - en (English)                   │   - en (English)             │
│  - zh (Chinese)                   │   - zh (Chinese)             │
│                                   │                               │
├───────────────────────────────────┼──────────────────────────────┤
│                                   │                               │
│  LocalizationConverter            │   LocalizationTagHelper      │
│  (XAML Value Converter)           │   (<loc> Razor Tag)          │
│                                   │                               │
│  Usage:                           │   Usage:                     │
│  Text="{.., Converter=...}"       │   <loc key="Home" />         │
│                                   │                               │
├───────────────────────────────────┼──────────────────────────────┤
│                                   │                               │
│  UI Components:                   │   UI Components:             │
│  - SettingsPage                   │   - _Layout.cshtml           │
│  - Language Selector              │   - Language Dropdown        │
│  - MainPage                       │   - Home/Index.cshtml        │
│  - ExplorePage                    │   - Home/Privacy.cshtml      │
│                                   │                               │
└───────────────────────────────────┴──────────────────────────────┘

                            Shared Translation Keys
                           ┌─────────────────────┐
                           │  Common Strings     │
                           │ - Map, Home, etc.   │
                           │ - Save, Delete      │
                           │ - Loading, Error    │
                           └─────────────────────┘
```

## Data Flow Diagram

### MAUI App Language Selection Flow

```
User Action (Tap Language Option)
         │
         ▼
SettingsPage.OnLanguageSelected()
         │
         ├─► LocalizationService.SetLanguage("en")
         │
         ├─► App.CurrentLanguage = "en"
         │
         └─► Update UI (Border colors, selected state)
         
         │
         ▼
Future Access to Translations:
LocalizationService.GetString("Home")
         │
         ├─► Check CurrentLanguage ("en")
         │
         ├─► Look up in Translations["en"]
         │
         └─► Return "Home" (or fallback to Vietnamese)
```

### Admin App Language Selection Flow

```
User Action (Click Language in Dropdown)
         │
         ▼
HomeController.SetLanguage("zh")
         │
         ├─► LocalizationService.SetLanguage("zh")
         │
         ├─► HttpContext.Session["CurrentLanguage"] = "zh"
         │
         └─► Redirect to Referrer
         
         │
         ▼
View Rendering:
<loc key="Home" />
         │
         ├─► LocalizationTagHelper.Process()
         │
         ├─► LocalizationService.GetString("Home", "zh")
         │
         ├─► Look up in Translations["zh"]["Home"]
         │
         └─► Render "主页"
```

## Translation Dictionary Structure

```csharp
Translations Dictionary
├── "vi" (Vietnamese)
│   ├── { "Map", "Bản đồ" }
│   ├── { "Home", "Trang chủ" }
│   ├── { "Save", "Lưu" }
│   └── ... (40+ keys for MAUI, 25+ for Admin)
│
├── "en" (English)
│   ├── { "Map", "Map" }
│   ├── { "Home", "Home" }
│   ├── { "Save", "Save" }
│   └── ... (same keys as above)
│
└── "zh" (Chinese)
    ├── { "Map", "地图" }
    ├── { "Home", "主页" }
    ├── { "Save", "保存" }
    └── ... (same keys as above)
```

## File Structure

### MAUI Application

```
VinhKhanhFood.App/
│
├── Services/
│   └── LocalizationService.cs ⭐
│       └── Manages translations
│           - Dictionaries for vi, en, zh
│           - GetString() method
│           - SetLanguage() method
│
├── Converters/
│   └── LocalizationConverter.cs ⭐
│       └── XAML value converter
│           - Convert() method
│           - Enables data binding
│
├── SettingsPage.xaml ✏️
│   └── Enhanced with:
│       - Language selector
│       - Chinese option
│
├── SettingsPage.xaml.cs ✏️
│   └── Updated:
│       - OnLanguageSelected()
│       - Language switching logic
│
└── AppShell.xaml
    └── Navigation shell
        - Map tab
        - Explore tab
        - Settings tab

Legend: ⭐ = New File, ✏️ = Modified File
```

### Admin Application

```
VinhKhanhFood.Admin/
│
├── Services/
│   └── LocalizationService.cs ⭐
│       └── Server-side translations
│           - Dictionaries for vi, en, zh
│           - GetString() method
│
├── TagHelpers/
│   └── LocalizationTagHelper.cs ⭐
│       └── Razor tag helper
│           - <loc> tag support
│           - Process() method
│
├── Controllers/
│   └── HomeController.cs ✏️
│       └── Updated:
│           - SetLanguage() action
│           - Session management
│
├── Views/
│   ├── _ViewImports.cshtml ✏️
│   │   └── Tag helper registration
│   │
│   ├── Shared/_Layout.cshtml ✏️
│   │   └── Enhanced with:
│   │       - Language dropdown
│   │       - Localized navigation
│   │
│   └── Home/
│       ├── Index.cshtml ✏️
│       │   └── Localized welcome
│       │
│       └── Privacy.cshtml ✏️
│           └── Localized content
│
└── Program.cs
    └── No changes needed (session built-in)

Legend: ⭐ = New File, ✏️ = Modified File
```

## Integration Points

### MAUI Integration

```
LocalizationService
        │
        ├─► SettingsPage
        │   └─► Language Selection UI
        │
        ├─► MainPage
        │   └─► Future: Localize content
        │
        ├─► ExplorePage
        │   └─► Future: Localize content
        │
        └─► LocalizationConverter
            └─► XAML Data Binding
```

### Admin Integration

```
LocalizationService
        │
        ├─► HomeController
        │   └─► SetLanguage Action
        │
        ├─► LocalizationTagHelper
        │   └─► <loc> Tag Processing
        │
        ├─► _Layout.cshtml
        │   ├─► Navigation Links
        │   └─► Language Dropdown
        │
        └─► Views
            ├─► Index.cshtml
            └─► Privacy.cshtml
```

## API Reference

### MAUI - LocalizationService

```csharp
// Static methods
public static string CurrentLanguage { get; set; }

public static string GetString(
    string key, 
    string? language = null
)
// Returns: Translated text or fallback to Vietnamese

public static void SetLanguage(string language)
// Sets CurrentLanguage and updates CultureInfo

public static string GetCultureCode(string language)
// Returns: Culture code (e.g., "en-US" for "en")

public static List<(string Code, string Name)> GetAvailableLanguages()
// Returns: List of (code, name) tuples

// Available Languages:
// ("vi", "Tiếng Việt")
// ("en", "English")
// ("zh", "中文")
```

### Admin - LocalizationService

```csharp
// Static methods
public static string CurrentLanguage { get; set; }

public static string GetString(
    string key, 
    string? language = null
)
// Returns: Translated text or fallback to Vietnamese

public static void SetLanguage(string language)
// Sets CurrentLanguage

public static List<(string Code, string Name)> GetAvailableLanguages()
// Returns: List of (code, name) tuples
```

### Admin - LocalizationTagHelper

```csharp
// Razor Tag Helper Usage
[HtmlTargetElement("loc")]
public class LocalizationTagHelper : TagHelper
{
    [HtmlAttributeName("key")]
    public string Key { get; set; }
    
    [HtmlAttributeName("lang")]
    public string? Language { get; set; }
}

// Usage in .cshtml:
// <loc key="Home" />
// <loc key="Home" lang="en" />
```

## Translation Coverage

### MAUI App (40+ Translations)

| Category | Count | Examples |
|----------|-------|----------|
| Navigation | 3 | Map, Explore, Settings |
| Main Page | 4 | Discover, Search, Location |
| Explore | 5 | Categories, Filters |
| Detail | 5 | Location, Call, Direction |
| Settings | 4 | Language, Version, About |
| Common | 15+ | Save, Delete, Error, etc. |

### Admin App (25+ Translations)

| Category | Count | Examples |
|----------|-------|----------|
| Navigation | 3 | Home, Privacy, Admin |
| Account | 5 | Login, Username, Password |
| Actions | 5 | Save, Delete, Edit, Add |
| Messages | 5 | Loading, Error, Success |
| Common | 7+ | Cancel, OK, Yes, No |

## Fallback Mechanism

```
Request: GetString("Home", "es") // Spanish (not supported)
         │
         ├─► Dictionary["es"] → Not found
         │
         └─► Dictionary["vi"] → Found! Return Vietnamese
         
         // Result: Falls back to Vietnamese
         // Output: "Trang chủ" (Vietnamese)
```

## Culture Code Mapping

```csharp
switch (language)
{
    case "vi" → "vi-VN"   // Vietnam
    case "en" → "en-US"   // United States
    case "zh" → "zh-CN"   // China (Simplified)
    default  → "vi-VN"    // Fallback
}
```

---

This architecture provides:
✅ Easy to extend with new languages
✅ Consistent across MAUI and Admin
✅ Simple API for developers
✅ Fallback for missing translations
✅ Session persistence (Admin)
✅ Immediate UI updates (MAUI)
