using Microsoft.Extensions.Logging;
using MedicineReminder.Services;

namespace MedicineReminder;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Регистрация сервисов
        builder.Services.AddSingleton<DataService>();
        
#if ANDROID
        builder.Services.AddSingleton<INotificationService, Platforms.Android.NotificationService>();
#else
        builder.Services.AddSingleton<INotificationService, Services.INotificationService>();
#endif

        return builder.Build();
    }
}
