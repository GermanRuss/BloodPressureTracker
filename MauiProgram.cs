using BloodPressureTracker.Services;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.ViewModels;
using BloodPressureTracker.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using static Android.Telephony.CarrierConfigManager;

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
            })
            .ConfigureEssentials(essentials =>
            {
                essentials.UseVersionTracking();
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Регистрация сервисов
        builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IPdfService, PdfExportService>();
        builder.Services.AddSingleton<IExportService, DataExportService>();

        // Регистрация ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<AddRecordViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();

        // Регистрация страниц
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<AddRecordPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<ExportPage>();

        return builder.Build();
    }
}