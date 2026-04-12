using Foundation;
using UIKit;

namespace VinhKhanhFood.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        if (url.AbsoluteString is { Length: > 0 } value)
        {
            App.ReceiveQrUri(value);
        }

        return base.OpenUrl(app, url, options);
    }
}
