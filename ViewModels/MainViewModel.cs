using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace BloodPressureTracker.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        public ObservableCollection<BloodPressureRecord> RecentRecords { get; } = new();

        private BloodPressureRecord _todayRecord;
        public BloodPressureRecord TodayRecord
        {
            get => _todayRecord;
            set => SetProperty(ref _todayRecord, value);
        }

        public ICommand AddRecordCommand { get; }
        public ICommand ViewHistoryCommand { get; }
        public ICommand ViewStatisticsCommand { get; }
        public ICommand ViewSettingsCommand { get; }
        public ICommand ExportDataCommand { get; }

        public MainViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            AddRecordCommand = new AsyncRelayCommand(AddRecordAsync);
            ViewHistoryCommand = new AsyncRelayCommand(ViewHistoryAsync);
            ViewStatisticsCommand = new AsyncRelayCommand(ViewStatisticsAsync);
            ViewSettingsCommand = new AsyncRelayCommand(ViewSettingsAsync);
            ExportDataCommand = new AsyncRelayCommand(ExportDataAsync);

            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                var today = DateTime.Today;
                var records = await _databaseService.GetBloodPressureRecordsAsync(
                    today.AddDays(-7), today);

                RecentRecords.Clear();
                foreach (var record in records.OrderByDescending(r => r.Date))
                {
                    RecentRecords.Add(record);
                }

                TodayRecord = records.FirstOrDefault(r => r.Date.Date == today);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddRecordAsync()
        {
            // Навигация на страницу добавления записи
            await Shell.Current.GoToAsync(nameof(AddRecordPage));
        }

        // Другие методы навигации...
    }
}