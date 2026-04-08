# Localization System - VinhKhanhFood

This document describes the language localization (i18n) system implemented for the VinhKhanhFood application, supporting Vietnamese, English, and Chinese (Simplified).

## Overview

The application now supports three languages:
- **Vietnamese** (Tiếng Việt) - `vi`
- **English** - `en`
- **Simplified Chinese** (中文) - `zh`

## Implementation

### MAUI App (VinhKhanhFood.App)

#### LocalizationService

Located in: `VinhKhanhFood.App/Services/LocalizationService.cs`

The service provides:
- **GetString(key, language)** - Retrieves translated text by key
- **SetLanguage(language)** - Sets the current language
- **GetAvailableLanguages()** - Returns list of supported languages
- **GetCultureCode(language)** - Maps language codes to culture codes

Usage in code:
```csharp
// Get translated text
string text = LocalizationService.GetString("Map");

// Change language
LocalizationService.SetLanguage("en");
```

#### LocalizationConverter

Located in: `VinhKhanhFood.App/Converters/LocalizationConverter.cs`

XAML value converter for binding UI text to translations.

Usage in XAML:
```xaml
<Label Text="{Binding ., Converter={StaticResource LocalizationConverter}}" />
```

#### Settings Page

The SettingsPage allows users to change the app language:
- Three language options: Vietnamese, English, Chinese
- Selected language is highlighted
- Language preference is stored in `App.CurrentLanguage`

#### AppShell Navigation Tabs

Tab titles in the navigation shell are localized:
- "Map" → "Bản đồ" (Vietnamese), "Map" (English), "地图" (Chinese)
- "Explore" → "Khám phá" (Vietnamese), "Explore" (English), "探索" (Chinese)
- "Settings" → "Cài đặt" (Vietnamese), "Settings" (English), "设置" (Chinese)

### Admin App (VinhKhanhFood.Admin)

#### LocalizationService

Located in: `VinhKhanhFood.Admin/Services/LocalizationService.cs`

Similar to MAUI version with server-side localization.

#### LocalizationTagHelper

Located in: `VinhKhanhFood.Admin/TagHelpers/LocalizationTagHelper.cs`

Razor tag helper for retrieving translations in `.cshtml` files.

Usage in Razor views:
```html
<a href="#"><loc key="Home" /></a>
```

#### Language Selector

Located in: `VinhKhanhFood.Admin/Views/Shared/_Layout.cshtml`

Dropdown menu in navbar allows language selection:
- Calls `Home/SetLanguage` action with language code
- Displays current language code (VI, EN, ZH)
- Stores language preference in session

#### Updated Views

- **_Layout.cshtml** - Navigation and language selector
- **Home/Index.cshtml** - Welcome page with localized content
- **Home/Privacy.cshtml** - Privacy page with localized content

#### HomeController Updates

Added `SetLanguage(string lang)` action:
```csharp
public IActionResult SetLanguage(string lang)
{
    if (!string.IsNullOrEmpty(lang))
    {
        LocalizationService.SetLanguage(lang);
        HttpContext.Session.SetString("CurrentLanguage", lang);
    }
    
    var returnUrl = Request.Headers["Referer"].ToString();
    if (string.IsNullOrEmpty(returnUrl))
    {
        returnUrl = Url.Action("Index", "Home");
    }
    
    return Redirect(returnUrl);
}
```

## Supported Translations

### Common UI Elements
- Tab titles: Map, Explore, Settings
- Buttons: Save, Cancel, Delete, Edit, Add
- Messages: Loading, Error, Success
- Common: Yes, No, OK

### MAUI App Specific
- Main page: "Discover", "Find amazing local food experiences"
- Explore page: "Discover Places", category filters (Seafood, Snails, Soup)
- Detail page: Details, Location, Call, Direction, Share
- Settings: Language selection, Dark Mode, Version

### Admin App Specific
- Navigation: Home, Privacy, Admin
- Account: Login, Logout, Username, Password
- Messages: Welcome, Management options

## Adding New Translations

### For MAUI App

1. Open `VinhKhanhFood.App/Services/LocalizationService.cs`
2. Add the key to all three language dictionaries:

```csharp
"vi", new Dictionary<string, string>
{
    { "NewKey", "Tiếng Việt translation" },
    ...
}

"en", new Dictionary<string, string>
{
    { "NewKey", "English translation" },
    ...
}

"zh", new Dictionary<string, string>
{
    { "NewKey", "中文 translation" },
    ...
}
```

3. Use in code:
```csharp
string text = LocalizationService.GetString("NewKey");
```

Or in XAML (first register the converter in Resources):
```xaml
<Label Text="{Binding Source=NewKey, Converter={StaticResource LocalizationConverter}}" />
```

### For Admin App

1. Open `VinhKhanhFood.Admin/Services/LocalizationService.cs`
2. Add translations to all three language dictionaries
3. Use in Razor views:
```html
<loc key="NewKey" />
```

## Language Persistence

### MAUI App
- Language preference is stored in `App.CurrentLanguage` static property
- Could be enhanced to persist using `SecureStorage` or `Preferences`

### Admin App
- Language is stored in `HttpContext.Session`
- Could be enhanced to store in cookies or database for persistent storage

## Culture Codes

The system maps language codes to culture codes:

| Language Code | Culture Code |
|---|---|
| vi | vi-VN |
| en | en-US |
| zh | zh-CN |

## Future Enhancements

1. **Persistence Layer**
   - Save language preference to local storage (MAUI)
   - Save to cookies or database (Admin)

2. **Additional Languages**
   - French, Japanese, Korean, Spanish

3. **Pluralization Support**
   - Handle plural forms for different languages

4. **Date/Time Localization**
   - Localize date formats, times, and calendars

5. **RTL Language Support**
   - Arabic, Hebrew support

6. **Translation Management**
   - Admin interface to manage translations
   - Integration with translation services (i18n platforms)

## Testing

To test the localization:

### MAUI App
1. Run the app
2. Navigate to Settings
3. Select different languages
4. Verify UI text changes immediately

### Admin App
1. Run the web application
2. Click language selector in navbar
3. Select different languages
4. Verify page content and navigation update

## Troubleshooting

### Translation Not Appearing
- Verify the key exists in LocalizationService
- Check spelling and capitalization
- Ensure language is properly initialized

### Language Not Persisting
- MAUI: Consider implementing local storage (Preferences)
- Admin: Verify session is enabled in Startup
- Add cookie-based persistence for Admin app

### Missing Translations
- Use fallback system: defaults to Vietnamese if translation missing
- Check console logs for errors
- Verify all three language dictionaries have matching keys

## Language Files Location

- **MAUI**: `VinhKhanhFood.App/Services/LocalizationService.cs`
- **Admin**: `VinhKhanhFood.Admin/Services/LocalizationService.cs`
- **MAUI Converter**: `VinhKhanhFood.App/Converters/LocalizationConverter.cs`
- **Admin Tag Helper**: `VinhKhanhFood.Admin/TagHelpers/LocalizationTagHelper.cs`
