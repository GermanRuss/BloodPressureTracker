using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BloodPressureTracker.ViewModels
{
    public partial class AddRecordViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        // Утренние показания
        [ObservableProperty]
        private string _morningSystolic;

        [ObservableProperty]
        private string _morningDiastolic;

        [ObservableProperty]
        private string _morningPulse;

        // Обеденные показания
        [ObservableProperty]
        private string _afternoonSystolic;

        [ObservableProperty]
        private string _afternoonDiastolic;

        [ObservableProperty]
        private string _afternoonPulse;

        // Вечерние показания
        [ObservableProperty]
        private string _eveningSystolic;

        [ObservableProperty]
        private string _eveningDiastolic;

        [ObservableProperty]
        private string _eveningPulse;

        [ObservableProperty]
        private string _notes;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRecordViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Добавить запись";

            SaveCommand = new AsyncRelayCommand(SaveRecordAsync);
            CancelCommand = new AsyncRelayCommand(CancelAsync);

            // Загружаем существующую запись на сегодня, если есть
            LoadExistingRecordAsync().SafeFireAndForget(false);
        }

        private async Task LoadExistingRecordAsync()
        {
            try
            {
                var records = await _databaseService.GetBloodPressureRecordsAsync(
                    SelectedDate, SelectedDate.AddDays(1));

                var existingRecord = records.FirstOrDefault();
                if (existingRecord != null)
                {
                    // Заполняем поля существующей записью
                    MorningSystolic = existingRecord.SystolicMorning > 0
                        ? existingRecord.SystolicMorning.ToString() : "";
                    MorningDiastolic = existingRecord.DiastolicMorning > 0
                        ? existingRecord.DiastolicMorning.ToString() : "";
                    MorningPulse = existingRecord.PulseMorning > 0
                        ? existingRecord.PulseMorning.ToString() : "";

                    AfternoonSystolic = existingRecord.SystolicAfternoon > 0
                        ? existingRecord.SystolicAfternoon.ToString() : "";
                    AfternoonDiastolic = existingRecord.DiastolicAfternoon > 0
                        ? existingRecord.DiastolicAfternoon.ToString() : "";
                    AfternoonPulse = existingRecord.PulseAfternoon > 0
                        ? existingRecord.PulseAfternoon.ToString() : "";

                    EveningSystolic = existingRecord.SystolicEvening > 0
                        ? existingRecord.SystolicEvening.ToString() : "";
                    EveningDiastolic = existingRecord.DiastolicEvening > 0
                        ? existingRecord.DiastolicEvening.ToString() : "";
                    EveningPulse = existingRecord.PulseEvening > 0
                        ? existingRecord.PulseEvening.ToString() : "";

                    Notes = existingRecord.Notes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки записи: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveRecordAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                // Валидация данных
                if (!ValidateInputs())
                {
                    await Shell.Current.DisplayAlert("Ошибка",
                        "Проверьте правильность введенных данных", "OK");
                    return;
                }

                // Создаем запись
                var record = new BloodPressureRecord
                {
                    Date = SelectedDate,

                    SystolicMorning = ParseInt(MorningSystolic),
                    DiastolicMorning = ParseInt(MorningDiastolic),
                    PulseMorning = ParseInt(MorningPulse),

                    SystolicAfternoon = ParseInt(AfternoonSystolic),
                    DiastolicAfternoon = ParseInt(AfternoonDiastolic),
                    PulseAfternoon = ParseInt(AfternoonPulse),

                    SystolicEvening = ParseInt(EveningSystolic),
                    DiastolicEvening = ParseInt(EveningDiastolic),
                    PulseEvening = ParseInt(EveningPulse),

                    Notes = Notes
                };

                // Проверяем, существует ли уже запись на эту дату
                var existingRecords = await _databaseService.GetBloodPressureRecordsAsync(
                    SelectedDate, SelectedDate.AddDays(1));

                if (existingRecords.Count > 0)
                {
                    // Обновляем существующую запись
                    var existingRecord = existingRecords.First();
                    record.Id = existingRecord.Id;
                    record.CreatedAt = existingRecord.CreatedAt;

                    var success = await _databaseService.UpdateBloodPressureRecordAsync(record);

                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Успех",
                            "Запись обновлена", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else
                {
                    // Создаем новую запись
                    var id = await _databaseService.AddBloodPressureRecordAsync(record);

                    if (id > 0)
                    {
                        await Shell.Current.DisplayAlert("Успех",
                            "Запись сохранена", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка",
                    $"Не удалось сохранить запись: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateInputs()
        {
            // Проверяем утренние показания
            if (!string.IsNullOrEmpty(MorningSystolic) ||
                !string.IsNullOrEmpty(MorningDiastolic))
            {
                if (!int.TryParse(MorningSystolic, out int systolic) ||
                    !int.TryParse(MorningDiastolic, out int diastolic))
                    return false;

                if (!Validators.IsValidPressure(systolic, diastolic))
                    return false;
            }

            // Проверяем обеденные показания
            if (!string.IsNullOrEmpty(AfternoonSystolic) ||
                !string.IsNullOrEmpty(AfternoonDiastolic))
            {
                if (!int.TryParse(AfternoonSystolic, out int systolic) ||
                    !int.TryParse(AfternoonDiastolic, out int diastolic))
                    return false;

                if (!Validators.IsValidPressure(systolic, diastolic))
                    return false;
            }

            // Проверяем вечерние показания
            if (!string.IsNullOrEmpty(EveningSystolic) ||
                !string.IsNullOrEmpty(EveningDiastolic))
            {
                if (!int.TryParse(EveningSystolic, out int systolic) ||
                    !int.TryParse(EveningDiastolic, out int diastolic))
                    return false;

                if (!Validators.IsValidPressure(systolic, diastolic))
                    return false;
            }

            return true;
        }

        private int ParseInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (int.TryParse(value, out int result))
                return result;

            return 0;
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private void ClearMorning()
        {
            MorningSystolic = "";
            MorningDiastolic = "";
            MorningPulse = "";
        }

        [RelayCommand]
        private void ClearAfternoon()
        {
            AfternoonSystolic = "";
            AfternoonDiastolic = "";
            AfternoonPulse = "";
        }

        [RelayCommand]
        private void ClearEvening()
        {
            EveningSystolic = "";
            EveningDiastolic = "";
            EveningPulse = "";
        }
    }
}