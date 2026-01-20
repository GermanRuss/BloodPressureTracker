using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace BloodPressureTracker.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private BloodPressureRecord _todayRecord;

        [ObservableProperty]
        private string _todayStatus = "Нет данных за сегодня";

        [ObservableProperty]
        private Color _statusColor = Colors.Gray;

        public ObservableCollection<BloodPressureRecord> RecentRecords { get; } = new();

        public MainViewModel(IDatabaseService databaseService, INotificationService notificationService)
        {
            _databaseService = databaseService;
            _notificationService = notificationService;

            Title = "Главная";

            // Загружаем данные при создании ViewModel
            LoadDataCommand.ExecuteAsync(null);
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
                "Гипотония" or "Нормальное" => Colors.Green,
                "Предгипертония" => Colors.Yellow,
                "Гипертония 1 ст." => Colors.Orange,
                "Гипертония 2 ст." or "Гипертонический криз" => Colors.Red,
                _ => Colors.Gray
            };
        }

        private (int systolic, int diastolic) GetLatestMeasurement(BloodPressureRecord record)
        {
            var now = DateTime.Now.TimeOfDay;

            // Проверяем вечерние показания (после 18:00)
            if (now.Hours >= 18 && record.SystolicEvening > 0)
                return (record.SystolicEvening, record.DiastolicEvening);

            // Проверяем обеденные показания (после 12:00)
            if (now.Hours >= 12 && record.SystolicAfternoon > 0)
                return (record.SystolicAfternoon, record.DiastolicAfternoon);

            // Утренние показания
            if (record.SystolicMorning > 0)
                return (record.SystolicMorning, record.DiastolicMorning);

            return (0, 0);
        }

        [RelayCommand]
        private async Task AddRecordAsync()
        {
            await Shell.Current.GoToAsync("AddRecordPage");
        }

        [RelayCommand]
        private async Task ViewHistoryAsync()
        {
            await Shell.Current.GoToAsync("HistoryPage");
        }

        [RelayCommand]
        private async Task ViewStatisticsAsync()
        {
            await Shell.Current.GoToAsync("StatisticsPage");
        }

        [RelayCommand]
        private async Task ViewSettingsAsync()
        {
            await Shell.Current.GoToAsync("SettingsPage");
        }

        [RelayCommand]
        private async Task ExportDataAsync()
        {
            await Shell.Current.GoToAsync("ExportPage");
        }

        [RelayCommand]
        private async Task EditRecordAsync(BloodPressureRecord record)
        {
            if (record == null)
                return;

            // Переходим на страницу редактирования с параметром
            await Shell.Current.GoToAsync($"AddRecordPage?recordId={record.Id}");
        }

        [RelayCommand]
        private async Task DeleteRecordAsync(BloodPressureRecord record)
        {
            if (record == null)
                return;

            var confirm = await Shell.Current.DisplayAlert(
                "Подтверждение",
                $"Удалить запись от {record.Date:dd.MM.yyyy}?",
                "Удалить",
                "Отмена");

            if (confirm)
            {
                var success = await _databaseService.DeleteBloodPressureRecordAsync(record.Id);
                if (success)
                {
                    await LoadDataAsync();
                    await Shell.Current.DisplayAlert("Успех", "Запись удалена", "OK");
                }
            }
        }
    }
}