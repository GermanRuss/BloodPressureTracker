using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;  // ДЛЯ Color!

namespace BloodPressureTracker.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private string _date = DateTime.Today.ToString("dd.MM.yyyy");

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
            LoadDataAsync().SafeFireAndForget(false);
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                RecentRecords.Clear();
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-7);

                var records = await _databaseService.GetBloodPressureRecordsAsync(startDate, endDate);

                foreach (var record in records.OrderByDescending(r => r.Date))
                    RecentRecords.Add(record);

                TodayRecord = records.FirstOrDefault(r => r.Date.Date == DateTime.Today.Date);
                UpdateTodayStatus();
            }
            catch (Exception ex)
            {
                await Microsoft.Maui.Controls.Shell.Current.DisplayAlert("Ошибка",
                    $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync() => await LoadDataAsync();

        private void UpdateTodayStatus()
        {
            if (TodayRecord == null)
            {
                TodayStatus = "Нет данных за сегодня";
                StatusColor = Colors.Gray;
                return;
            }

            var (systolic, diastolic) = GetLatestMeasurement(TodayRecord);

            if (systolic == 0 || diastolic == 0)
            {
                TodayStatus = "Не все замеры выполнены";
                StatusColor = Colors.Orange;
                return;
            }

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

            if (now.Hours >= 18 && record.SystolicEvening > 0)
                return (record.SystolicEvening, record.DiastolicEvening);

            if (now.Hours >= 12 && record.SystolicAfternoon > 0)
                return (record.SystolicAfternoon, record.DiastolicAfternoon);

            if (record.SystolicMorning > 0)
                return (record.SystolicMorning, record.DiastolicMorning);

            return (0, 0);
        }

        [RelayCommand]
        private async Task AddRecordAsync() =>
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("AddRecordPage");

        [RelayCommand]
        private async Task ViewHistoryAsync() =>
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("HistoryPage");

        [RelayCommand]
        private async Task ViewStatisticsAsync() =>
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("StatisticsPage");

        [RelayCommand]
        private async Task ViewSettingsAsync() =>
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("SettingsPage");

        [RelayCommand]
        private async Task ExportDataAsync() =>
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync("ExportPage");

        [RelayCommand]
        private async Task EditRecordAsync(BloodPressureRecord record)
        {
            if (record == null) return;
            await Microsoft.Maui.Controls.Shell.Current.GoToAsync($"AddRecordPage?recordId={record.Id}");
        }

        [RelayCommand]
        private async Task DeleteRecordAsync(BloodPressureRecord record)
        {
            if (record == null) return;

            var confirm = await Microsoft.Maui.Controls.Shell.Current.DisplayAlert(
                "Подтверждение", $"Удалить запись от {record.Date:dd.MM.yyyy}?", "Удалить", "Отмена");

            if (confirm && await _databaseService.DeleteBloodPressureRecordAsync(record.Id))
            {
                await LoadDataAsync();
                await Microsoft.Maui.Controls.Shell.Current.DisplayAlert("Успех", "Запись удалена", "OK");
            }
        }
    }
}