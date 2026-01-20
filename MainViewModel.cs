using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Android.Provider.CalendarContract;

namespace BloodPressureTracker.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private BloodPressureRecord _todayRecord;

        [ObservableProperty]
        private string _todayStatus;

        [ObservableProperty]
        private Color _statusColor;

        public ObservableCollection<BloodPressureRecord> RecentRecords { get; } = new();

        public ICommand AddRecordCommand { get; }
        public ICommand ViewHistoryCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand ViewSettingsCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand RefreshCommand { get; }

        public MainViewModel(
            IDatabaseService databaseService,
            INotificationService notificationService)
        {
            _databaseService = databaseService;
            _notificationService = notificationService;

            Title = "Главная";

            AddRecordCommand = new AsyncRelayCommand(AddRecordAsync);
            ViewHistoryCommand = new AsyncRelayCommand(ViewHistoryAsync);
            ViewStatisticsCommand = new AsyncRelayCommand(ViewStatisticsAsync);
            ViewSettingsCommand = new AsyncRelayCommand(ViewSettingsAsync);
            ExportDataCommand = new AsyncRelayCommand(ExportDataAsync);
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);

            // Загружаем данные при создании ViewModel
            LoadDataAsync().SafeFireAndForget(false);
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                RecentRecords.Clear();

                // Загружаем записи за последние 7 дней
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-7);

                var records = await _databaseService.GetBloodPressureRecordsAsync(startDate, endDate);

                foreach (var record in records.OrderByDescending(r => r.Date))
                {
                    RecentRecords.Add(record);
                }

                // Получаем сегодняшнюю запись
                TodayRecord = records.FirstOrDefault(r => r.Date.Date == DateTime.Today.Date);

                // Обновляем статус
                UpdateTodayStatus();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка",
                    $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateTodayStatus()
        {
            if (TodayRecord == null)
            {
                TodayStatus = "Нет данных за сегодня";
                StatusColor = Colors.Gray;
                return;
            }

            // Получаем последнее измерение за сегодня
            var (systolic, diastolic) = GetLatestMeasurement(TodayRecord);

            if (systolic == 0 || diastolic == 0)
            {
                TodayStatus = "Не все замеры выполнены";
                StatusColor = Colors.Orange;
                return;
            }

            // Анализируем давление
            var category = Validators.GetPressureCategory(systolic, diastolic);

            TodayStatus = $"{systolic}/{diastolic} - {category}";

            StatusColor = category switch
            {
                "Нормальное" => Colors.Green,
                "Предгипертония" => Colors.Yellow,
                "Гипертония 1 ст." => Colors.Orange,
                "Гипертония 2 ст." => Colors.OrangeRed,
                "Гипертонический криз" => Colors.Red,
                _ => Colors.Gray
            };
        }

        private (int systolic, int diastolic) GetLatestMeasurement(BloodPressureRecord record)
        {
            var now = DateTime.Now.TimeOfDay;

            if (now >= new TimeSpan(19, 0, 0) && record.SystolicEvening > 0)
                return (record.SystolicEvening, record.DiastolicEvening);

            if (now >= new TimeSpan(13, 0, 0) && record.SystolicAfternoon > 0)
                return (record.SystolicAfternoon, record.DiastolicAfternoon);

            if (record.SystolicMorning > 0)
                return (record.SystolicMorning, record.DiastolicMorning);

            return (0, 0);
        }

        private async Task AddRecordAsync()
        {
            await Shell.Current.GoToAsync("AddRecordPage");
        }

        private async Task ViewHistoryAsync()
        {
            await Shell.Current.GoToAsync("HistoryPage");
        }

        private async Task ViewStatisticsAsync()
        {
            await Shell.Current.GoToAsync("StatisticsPage");
        }

        private async Task ViewSettingsAsync()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }

        private async Task ExportDataAsync()
        {
            await Shell.Current.GoToAsync("ExportPage");
        }
    }
}