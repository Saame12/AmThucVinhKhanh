using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using VinhKhanhFood.App.Services;
using VinhKhanhFood.App.ViewModels;

namespace VinhKhanhFood.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiMaps() // 1. Kích hoạt tính năng Bản đồ
                .UseMauiCommunityToolkit() // 2. Kích hoạt CommunityToolkit
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Services
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddSingleton<LocalizationService>();

            // Register ViewModels
            builder.Services.AddSingleton<MapViewModel>();
            builder.Services.AddSingleton<ExploreViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}