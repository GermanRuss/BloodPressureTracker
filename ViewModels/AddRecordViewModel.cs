using System;
using System.Linq;
using System.Threading.Tasks;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using BloodPressureTracker.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;  // Для Shell и DisplayAlert
using Microsoft.Maui.Graphics;  // ДЛЯ Color!

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

        [ObservableProperty]
        private string _morningValidationStatus = "";

        [ObservableProperty]
        private Color _morningValidationColor = Colors.Transparent;

        // Обеденные показания
        [ObservableProperty]
        private string _afternoonSystolic;

        [ObservableProperty]
        private string _afternoonDiastolic;

        [ObservableProperty]
        private string _afternoonPulse;

        [ObservableProperty]
        private string _afternoonValidationStatus = "";

        [ObservableProperty]
        private Color _afternoonValidationColor = Colors.Transparent;

        // Вечерние показания
        [ObservableProperty]
        private string _eveningSystolic;

        [ObservableProperty]
        private string _eveningDiastolic;

        [ObservableProperty]
        private string _eveningPulse;

        [ObservableProperty]
        private string _eveningValidationStatus = "";

        [ObservableProperty]
        private Color _eveningValidationColor = Colors.Transparent;

        [ObservableProperty]
        private string _notes;

        public AddRecordViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Title = "Добавить запись";
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
                    MorningSystolic = existingRecord.SystolicMorning > 0 ? existingRecord.SystolicMorning.ToString() : "";
                    MorningDiastolic = existingRecord.DiastolicMorning > 0 ? existingRecord.DiastolicMorning.ToString() : "";
                    MorningPulse = existingRecord.PulseMorning > 0 ? existingRecord.PulseMorning.ToString() : "";

                    AfternoonSystolic = existingRecord.SystolicAfternoon > 0 ? existingRecord.SystolicAfternoon.ToString() : "";
                    AfternoonDiastolic = existingRecord.DiastolicAfternoon > 0 ? existingRecord.DiastolicAfternoon.ToString() : "";
                    AfternoonPulse = existingRecord.PulseAfternoon > 0 ? existingRecord.PulseAfternoon.ToString() : "";

                    EveningSystolic = existingRecord.SystolicEvening > 0 ? existingRecord.SystolicEvening.ToString() : "";
                    EveningDiastolic = existingRecord.DiastolicEvening > 0 ? existingRecord.DiastolicEvening.ToString() : "";
                    EveningPulse = existingRecord.PulseEvening > 0 ? existingRecord.PulseEvening.ToString() : "";

                    Notes = existingRecord.Notes;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить запись: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaveRecordAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                if (!ValidateInputs())
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Проверьте правильность введенных данных", "OK");
                    return;
                }

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
                    Notes = Notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var existingRecords = await _databaseService.GetBloodPressureRecordsAsync(
                    SelectedDate, SelectedDate.AddDays(1));

                if (existingRecords.Any())
                {
                    var existingRecord = existingRecords.First();
                    record.Id = existingRecord.Id;
                    record.CreatedAt = existingRecord.CreatedAt;

                    if (await _databaseService.UpdateBloodPressureRecordAsync(record))
                    {
                        await Shell.Current.DisplayAlert("Успех", "Запись обновлена", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else
                {
                    if (await _databaseService.AddBloodPressureRecordAsync(record) > 0)
                    {
                        await Shell.Current.DisplayAlert("Успех", "Запись сохранена", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить запись: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool ValidateInputs()
        {
            bool isValid = true;

            MorningValidationStatus = "";
            MorningValidationColor = Colors.Transparent;
            AfternoonValidationStatus = "";
            AfternoonValidationColor = Colors.Transparent;
            EveningValidationStatus = "";
            EveningValidationColor = Colors.Transparent;

            // Проверка утренних показаний
            if (!string.IsNullOrEmpty(MorningSystolic) || !string.IsNullOrEmpty(MorningDiastolic))
            {
                if (!int.TryParse(MorningSystolic, out int systolic) || !int.TryParse(MorningDiastolic, out int diastolic))
                {
                    MorningValidationStatus = "Некорректные значения";
                    MorningValidationColor = Colors.Red;
                    isValid = false;
                }
                else if (!Validators.IsValidPressure(systolic, diastolic))
                {
                    MorningValidationStatus = "Недопустимые значения давления";
                    MorningValidationColor = Colors.Orange;
                    isValid = false;
                }
            }

            // Проверка обеденных показаний
            if (!string.IsNullOrEmpty(AfternoonSystolic) || !string.IsNullOrEmpty(AfternoonDiastolic))
            {
                if (!int.TryParse(AfternoonSystolic, out int systolic) || !int.TryParse(AfternoonDiastolic, out int diastolic))
                {
                    AfternoonValidationStatus = "Некорректные значения";
                    AfternoonValidationColor = Colors.Red;
                    isValid = false;
                }
                else if (!Validators.IsValidPressure(systolic, diastolic))
                {
                    AfternoonValidationStatus = "Недопустимые значения давления";
                    AfternoonValidationColor = Colors.Orange;
                    isValid = false;
                }
            }

            // Проверка вечерних показаний
            if (!string.IsNullOrEmpty(EveningSystolic) || !string.IsNullOrEmpty(EveningDiastolic))
            {
                if (!int.TryParse(EveningSystolic, out int systolic) || !int.TryParse(EveningDiastolic, out int diastolic))
                {
                    EveningValidationStatus = "Некорректные значения";
                    EveningValidationColor = Colors.Red;
                    isValid = false;
                }
                else if (!Validators.IsValidPressure(systolic, diastolic))
                {
                    EveningValidationStatus = "Недопустимые значения давления";
                    EveningValidationColor = Colors.Orange;
                    isValid = false;
                }
            }

            return isValid;
        }

        private int ParseInt(string value) =>
            string.IsNullOrEmpty(value) ? 0 : (int.TryParse(value, out int result) ? result : 0);

        [RelayCommand]
        private async Task CancelAsync() => await Shell.Current.GoToAsync("..");

        [RelayCommand]
        private void ClearMorning()
        {
            MorningSystolic = "";
            MorningDiastolic = "";
            MorningPulse = "";
            MorningValidationStatus = "";
            MorningValidationColor = Colors.Transparent;
        }

        [RelayCommand]
        private void ClearAfternoon()
        {
            AfternoonSystolic = "";
            AfternoonDiastolic = "";
            AfternoonPulse = "";
            AfternoonValidationStatus = "";
            AfternoonValidationColor = Colors.Transparent;
        }

        [RelayCommand]
        private void ClearEvening()
        {
            EveningSystolic = "";
            EveningDiastolic = "";
            EveningPulse = "";
            EveningValidationStatus = "";
            EveningValidationColor = Colors.Transparent;
        }
    }
}
