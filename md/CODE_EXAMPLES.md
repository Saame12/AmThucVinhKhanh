# Code Examples - Localization Usage

## MAUI App Examples

### Getting Translated Text

```csharp
// In code-behind (.xaml.cs)
using VinhKhanhFood.App.Services;

public partial class MainPage : ContentPage
{
    public void DisplayMessage()
    {
        // Get text in current language
        string text = LocalizationService.GetString("Discover");
        
        // Get text in specific language
        string enText = LocalizationService.GetString("Discover", "en");
        string viText = LocalizationService.GetString("Discover", "vi");
        string zhText = LocalizationService.GetString("Discover", "zh");
    }
}
```

### Changing Language

```csharp
// In SettingsPage.xaml.cs
public void OnLanguageSelected(string language)
{
    if (language == "en")
    {
        LocalizationService.SetLanguage("en");
        App.CurrentLanguage = "en";
    }
    else if (language == "zh")
    {
        LocalizationService.SetLanguage("zh");
        App.CurrentLanguage = "zh";
    }
    else if (language == "vi")
    {
        LocalizationService.SetLanguage("vi");
        App.CurrentLanguage = "vi";
    }
    
    // UI updates
    RefreshUIDisplay();
}
```

### Using LocalizationConverter in XAML

```xaml
<!-- Register the converter -->
<ContentPage.Resources>
    <ResourceDictionary>
        <local:LocalizationConverter x:Key="LocalizationConverter" />
    </ResourceDictionary>
</ContentPage.Resources>

<!-- Use in data bindings -->
<Label Text="{Binding SomeText, Converter={StaticResource LocalizationConverter}}" />

<!-- Alternative: Direct binding to property -->
<Label x:Name="WelcomeLabel" />
```

```csharp
// In code-behind
protected override void OnAppearing()
{
    base.OnAppearing();
    WelcomeLabel.Text = LocalizationService.GetString("Welcome");
}
```

### Getting Available Languages

```csharp
public void PopulateLanguageOptions()
{
    var languages = LocalizationService.GetAvailableLanguages();
    
    foreach (var (code, name) in languages)
    {
        // code: "vi", "en", "zh"
        // name: "Tiếng Việt", "English", "中文"
        AddLanguageOption(code, name);
    }
}
```

### Programmatic Language Setting

```csharp
public class App : Application
{
    public static string CurrentLanguage { get; set; } = "vi";
    
    public App()
    {
        InitializeComponent();
        
        // Load saved language preference
        var savedLang = Preferences.Default.Get("language", "vi");
        CurrentLanguage = savedLang;
        LocalizationService.SetLanguage(savedLang);
    }
    
    public static void SaveLanguagePreference(string language)
    {
        CurrentLanguage = language;
        LocalizationService.SetLanguage(language);
        Preferences.Default.Set("language", language);
    }
}
```

## Admin App Examples

### Using LocalizationTagHelper in Razor Views

```html
<!-- Simple translation -->
<h1><loc key="Welcome" /></h1>

<!-- In links -->
<a href="@Url.Action("Index", "Home")"><loc key="Home" /></a>

<!-- In buttons -->
<button class="btn btn-primary">
    <loc key="Save" />
</button>

<!-- In labels -->
<label for="username"><loc key="Username" /></label>

<!-- In forms -->
<form>
    <div>
        <label><loc key="Username" /></label>
        <input type="text" name="username" />
    </div>
    <div>
        <label><loc key="Password" /></label>
        <input type="password" name="password" />
    </div>
    <button type="submit"><loc key="Login" /></button>
</form>
```

### Language Selector in Navigation

```html
<!-- In _Layout.cshtml -->
<div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
    <ul class="navbar-nav flex-grow-1">
        <li class="nav-item">
            <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">
                <loc key="Home" />
            </a>
        </li>
    </ul>
    
    <!-- Language Selector -->
    <ul class="navbar-nav">
        <li class="nav-item dropdown">
            <a class="nav-link dropdown-toggle" href="#" id="languageDropdown" role="button" data-bs-toggle="dropdown">
                🌐 @VinhKhanhFood.Admin.Services.LocalizationService.CurrentLanguage.ToUpper()
            </a>
            <ul class="dropdown-menu dropdown-menu-end">
                <li>
                    <a class="dropdown-item" 
                       asp-controller="Home" 
                       asp-action="SetLanguage" 
                       asp-route-lang="vi">
                        Tiếng Việt
                    </a>
                </li>
                <li>
                    <a class="dropdown-item" 
                       asp-controller="Home" 
                       asp-action="SetLanguage" 
                       asp-route-lang="en">
                        English
                    </a>
                </li>
                <li>
                    <a class="dropdown-item" 
                       asp-controller="Home" 
                       asp-action="SetLanguage" 
                       asp-route-lang="zh">
                        中文
                    </a>
                </li>
            </ul>
        </li>
    </ul>
</div>
```

### Getting Translations in C# Code

```csharp
using VinhKhanhFood.Admin.Services;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Get translated text
        string welcomeText = LocalizationService.GetString("Welcome");
        
        // Pass to view
        ViewData["Title"] = welcomeText;
        
        return View();
    }
    
    public IActionResult SetLanguage(string lang)
    {
        // Change language
        LocalizationService.SetLanguage(lang);
        
        // Store in session
        HttpContext.Session.SetString("CurrentLanguage", lang);
        
        // Get available languages
        var languages = LocalizationService.GetAvailableLanguages();
        
        // Redirect back
        var returnUrl = Request.Headers["Referer"].ToString();
        return Redirect(returnUrl ?? Url.Action("Index", "Home"));
    }
}
```

### Localized View Example

```html
@{
    ViewData["Title"] = VinhKhanhFood.Admin.Services.LocalizationService
        .GetString("Welcome to Vinh Khanh Food Admin");
}

<div class="container">
    <div class="row">
        <div class="col-md-12">
            <h1>
                <loc key="Welcome to Vinh Khanh Food Admin" />
            </h1>
            
            <p>
                <loc key="Manage your food locations and content" />
            </p>
            
            <div class="alert alert-info">
                <strong><loc key="Loading" /></strong>
                <span id="loadingMessage">
                    <loc key="Loading..." />
                </span>
            </div>
            
            <table class="table">
                <thead>
                    <tr>
                        <th><loc key="Name" /></th>
                        <th><loc key="Location" /></th>
                        <th><loc key="Action" /></th>
                    </tr>
                </thead>
                <tbody>
                    <!-- Content here -->
                </tbody>
            </table>
            
            <div>
                <a href="#" class="btn btn-primary"><loc key="Add" /></a>
                <a href="#" class="btn btn-secondary"><loc key="Cancel" /></a>
            </div>
        </div>
    </div>
</div>
```

### Middleware for Session-Based Language

```csharp
// In Program.cs (if needed)
public class LanguageMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get language from session
        var language = context.Session.GetString("CurrentLanguage") ?? "vi";
        
        // Set in LocalizationService
        LocalizationService.SetLanguage(language);
        
        // Continue
        await _next(context);
    }
}

// Register in Program.cs:
// app.UseMiddleware<LanguageMiddleware>();
```

## Adding New Translations

### MAUI App Example

```csharp
// In VinhKhanhFood.App/Services/LocalizationService.cs

private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
{
    {
        "vi", new Dictionary<string, string>
        {
            // Add your new key
            { "NewFeatureName", "Tên tính năng mới" },
            { "NewFeatureDescription", "Mô tả tính năng mới" },
        }
    },
    {
        "en", new Dictionary<string, string>
        {
            // Add your new key
            { "NewFeatureName", "New Feature Name" },
            { "NewFeatureDescription", "New Feature Description" },
        }
    },
    {
        "zh", new Dictionary<string, string>
        {
            // Add your new key
            { "NewFeatureName", "新功能名称" },
            { "NewFeatureDescription", "新功能描述" },
        }
    }
};

// Usage in code:
string featureName = LocalizationService.GetString("NewFeatureName");
```

### Admin App Example

```csharp
// In VinhKhanhFood.Admin/Services/LocalizationService.cs
// Add translations following the same pattern as MAUI

// Then use in views:
<h2><loc key="NewFeatureName" /></h2>
<p><loc key="NewFeatureDescription" /></p>
```

## Common Patterns

### Pattern 1: Dynamic Content Display

```csharp
// MAUI
public class ExploreViewModel : INotifyPropertyChanged
{
    private string _searchPlaceholder;
    
    public string SearchPlaceholder
    {
        get => _searchPlaceholder;
        set
        {
            if (_searchPlaceholder != value)
            {
                _searchPlaceholder = value;
                OnPropertyChanged();
            }
        }
    }
    
    public void ChangeLanguage(string language)
    {
        LocalizationService.SetLanguage(language);
        SearchPlaceholder = LocalizationService.GetString("Search places...");
    }
}
```

### Pattern 2: Error Messages

```csharp
// MAUI
public async Task LoadDataAsync()
{
    try
    {
        // Load data
    }
    catch (Exception ex)
    {
        string errorTitle = LocalizationService.GetString("Error");
        string errorMsg = LocalizationService.GetString("Failed to load data");
        await DisplayAlert(errorTitle, errorMsg, "OK");
    }
}

// Admin
public IActionResult SomeAction()
{
    try
    {
        // Do something
    }
    catch (Exception ex)
    {
        string errorMessage = LocalizationService.GetString("Error");
        return BadRequest(new { error = errorMessage });
    }
}
```

### Pattern 3: Conditional Localization

```csharp
// MAUI
public string GetLocationTypeDisplay(string type)
{
    return type switch
    {
        "seafood" => LocalizationService.GetString("Seafood"),
        "snails" => LocalizationService.GetString("Snails"),
        "soup" => LocalizationService.GetString("Soup"),
        _ => type
    };
}

// Admin
public string GetStatusDisplay(string status)
{
    return status switch
    {
        "active" => LocalizationService.GetString("Active"),
        "inactive" => LocalizationService.GetString("Inactive"),
        "pending" => LocalizationService.GetString("Pending"),
        _ => status
    };
}
```

## Testing Examples

```csharp
// Unit Tests for LocalizationService
[TestFixture]
public class LocalizationServiceTests
{
    [Test]
    public void GetString_WithValidKey_ReturnsTranslation()
    {
        var result = LocalizationService.GetString("Map", "en");
        Assert.AreEqual("Map", result);
    }
    
    [Test]
    public void GetString_WithInvalidLanguage_FallsbackToVietnamese()
    {
        var result = LocalizationService.GetString("Map", "es");
        Assert.AreEqual("Bản đồ", result);
    }
    
    [Test]
    public void SetLanguage_ChangesCurrentLanguage()
    {
        LocalizationService.SetLanguage("en");
        Assert.AreEqual("en", LocalizationService.CurrentLanguage);
    }
    
    [Test]
    public void GetAvailableLanguages_ReturnsThreeLanguages()
    {
        var languages = LocalizationService.GetAvailableLanguages();
        Assert.AreEqual(3, languages.Count);
    }
}
```

---

These examples show common patterns for using the localization system in both MAUI and Admin applications. Adapt them to your specific use cases!
