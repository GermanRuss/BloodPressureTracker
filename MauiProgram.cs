using BloodPressureTracker.Services;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.ViewModels;
using BloodPressureTracker.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace BloodPressureTracker
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Регистрация сервисов
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();

            // Регистрация ViewModels
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<AddRecordViewModel>();

            // Регистрация Views
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<AddRecordPage>();

            return builder.Build();
        }
    }
}