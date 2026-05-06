using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace VinhKhanhFood.App;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "vinhkhanhfood",
    DataHost = "poi")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleQrIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleQrIntent(intent);
    }

    private static void HandleQrIntent(Intent? intent)
    {
        var data = intent?.DataString;
        if (!string.IsNullOrWhiteSpace(data))
        {
            App.ReceiveQrUri(data);
        }
    }
}
