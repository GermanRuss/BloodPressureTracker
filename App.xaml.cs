using BloodPressureTracker.Services;
using BloodPressureTracker.Services.Interfaces;

namespace BloodPressureTracker
{
    public partial class App : Application
    {
        private readonly INotificationService _notificationService;
        private readonly IDatabaseService _databaseService;

        public App(
            INotificationService notificationService,
            IDatabaseService databaseService)
        {
            InitializeComponent();

            _notificationService = notificationService;
            _databaseService = databaseService;

            MainPage = new AppShell();

            // Инициализируем при запуске
            InitializeAsync().SafeFireAndForget(false);
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Инициализируем базу данных
                await _databaseService.InitializeDatabaseAsync();

                // Инициализируем уведомления
                await _notificationService.InitializeNotificationsAsync();

                // Проверяем первичный запуск
                var settings = await _databaseService.GetAppSettingsAsync();
                if (settings == null)
                {
                    await ShowWelcomeMessage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации приложения: {ex.Message}");
            }
        }

        private async Task ShowWelcomeMessage()
        {
            if (MainPage != null)
            {
                await MainPage.DisplayAlert(
                    "Добро пожаловать!",
                    "Привет! Это приложение для отслеживания артериального давления.\n\n" +
                    "Рекомендуется настроить время напоминаний в разделе 'Настройки'.",
                    "Начать");
            }
        }

        public static void HandleAppActions(AppAction appAction)
        {
            Current.Dispatcher.Dispatch(async () =>
            {
                var page = appAction.Id switch
                {
                    "add_record" => nameof(AddRecordPage),
                    "view_history" => nameof(HistoryPage),
                    "view_stats" => nameof(StatisticsPage),
                    _ => nameof(MainPage)
                };

                await Shell.Current.GoToAsync($"//{page}");
            });
        }

        protected override void OnStart()
        {
            base.OnStart();

            // Обновляем данные при запуске приложения
            UpdateDataAsync().SafeFireAndForget(false);
        }

        private async Task UpdateDataAsync()
        {
            // Можно добавить синхронизацию с облаком или другие фоновые задачи
            await Task.Delay(1000);
        }
    }
}