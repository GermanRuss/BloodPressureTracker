using Microsoft.Extensions.Logging;
using BloodPressureTracker.Services;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.ViewModels;
using BloodPressureTracker.Views;
using CommunityToolkit.Maui;

namespace BloodPressureTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ВАЖНО: УБЕРИТЕ UseLocalNotification и AddDebug пока не установлены пакеты

        // Регистрация СУЩЕСТВУЮЩИХ сервисов
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();

        // Регистрация СУЩЕСТВУЮЩИХ ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AddRecordViewModel>();
        // StatisticsViewModel, SettingsViewModel и другие пока НЕ регистрируем

        // Регистрация СУЩЕСТВУЮЩИХ страниц
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AddRecordPage>();
        // StatisticsPage, SettingsPage и другие пока НЕ регистрируем

        return builder.Build();
    }
}