# Quick Reference - Language Support

## Supported Languages

| Language | Code | Culture | Example |
|----------|------|---------|---------|
| Vietnamese | `vi` | vi-VN | Tiếng Việt |
| English | `en` | en-US | English |
| Chinese (Simplified) | `zh` | zh-CN | 中文 |

## Key Translations

### App Navigation

| Key | Vietnamese | English | Chinese |
|-----|-----------|---------|---------|
| Map | Bản đồ | Map | 地图 |
| Explore | Khám phá | Explore | 探索 |
| Settings | Cài đặt | Settings | 设置 |

### Common UI

| Key | Vietnamese | English | Chinese |
|-----|-----------|---------|---------|
| Home | Trang chủ | Home | 主页 |
| Privacy | Chính sách riêng tư | Privacy | 隐私 |
| Language | Ngôn ngữ | Language | 语言 |
| Loading... | Đang tải... | Loading... | 加载中... |
| Error | Lỗi | Error | 错误 |
| Success | Thành công | Success | 成功 |
| Save | Lưu | Save | 保存 |
| Cancel | Hủy | Cancel | 取消 |
| Delete | Xóa | Delete | 删除 |

## Using Localization

### MAUI App

```csharp
// Get translation
string mapTitle = LocalizationService.GetString("Map");

// Change language
LocalizationService.SetLanguage("en");

// Get available languages
var languages = LocalizationService.GetAvailableLanguages();
```

### Admin App

```razor
<!-- In .cshtml files -->
<a href="#"><loc key="Home" /></a>
<button><loc key="Save" /></button>
```

```csharp
// In C# code
string text = LocalizationService.GetString("Home");
```

## Language Files

### MAUI App
- **Service**: `VinhKhanhFood.App/Services/LocalizationService.cs`
- **Converter**: `VinhKhanhFood.App/Converters/LocalizationConverter.cs`

### Admin App
- **Service**: `VinhKhanhFood.Admin/Services/LocalizationService.cs`
- **Tag Helper**: `VinhKhanhFood.Admin/TagHelpers/LocalizationTagHelper.cs`

## Adding New Translation

1. Open the LocalizationService file
2. Add your key to all three language dictionaries:

```csharp
"vi", new Dictionary<string, string>
{
    { "YourNewKey", "Vietnamese text" },
    { "AnotherKey", "Another Vietnamese text" }
}

"en", new Dictionary<string, string>
{
    { "YourNewKey", "English text" },
    { "AnotherKey", "Another English text" }
}

"zh", new Dictionary<string, string>
{
    { "YourNewKey", "中文文本" },
    { "AnotherKey", "另一个中文文本" }
}
```

3. Use in your code/markup:
   - MAUI: `LocalizationService.GetString("YourNewKey")`
   - Admin: `<loc key="YourNewKey" />`

## Changing Language

### MAUI App
Settings → Language → Select Vietnamese/English/中文

### Admin App
Click 🌐 dropdown in navbar → Select language

## File Organization

```
VinhKhanhFood.App/
├── Services/
│   └── LocalizationService.cs (translations)
├── Converters/
│   └── LocalizationConverter.cs (XAML binding)
├── SettingsPage.xaml* (language selector)
└── AppShell.xaml

VinhKhanhFood.Admin/
├── Services/
│   └── LocalizationService.cs (translations)
├── TagHelpers/
│   └── LocalizationTagHelper.cs (<loc> tag)
├── Controllers/
│   └── HomeController.cs (SetLanguage action)
└── Views/
    ├── _ViewImports.cshtml (tag helper register)
    ├── Shared/_Layout.cshtml (navbar selector)
    └── Home/*.cshtml (localized views)
```

## Troubleshooting

**Translation not showing?**
- Verify key exists in LocalizationService
- Check spelling (case-sensitive)
- Use fallback (missing translations default to Vietnamese)

**Language not changing?**
- MAUI: Verify `App.CurrentLanguage` is updated
- Admin: Check session is enabled
- Try clearing browser cache (Admin app)

**Missing Chinese characters?**
- Ensure UTF-8 encoding in files
- Check font supports Chinese characters
- Verify database supports Unicode (if persisting)

## Contact & Support

For issues or improvements, refer to:
- `LOCALIZATION.md` - Full documentation
- `LOCALIZATION_IMPLEMENTATION.md` - Implementation details
- Source code comments in LocalizationService files
