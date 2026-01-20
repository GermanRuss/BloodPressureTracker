using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BloodPressureTracker.Models;

namespace BloodPressureTracker.Services.Interfaces
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync();

        // Записи давления
        Task<int> AddBloodPressureRecordAsync(BloodPressureRecord record);
        Task<bool> UpdateBloodPressureRecordAsync(BloodPressureRecord record);
        Task<bool> DeleteBloodPressureRecordAsync(int id);
        Task<BloodPressureRecord> GetBloodPressureRecordAsync(int id);
        Task<List<BloodPressureRecord>> GetBloodPressureRecordsAsync(DateTime startDate, DateTime endDate);
        Task<List<BloodPressureRecord>> GetAllBloodPressureRecordsAsync();

        // Настройки
        Task<AppSettings> GetAppSettingsAsync();
        Task<bool> SaveAppSettingsAsync(AppSettings settings);

        // Напоминания о лекарствах
        Task<List<MedicationReminder>> GetMedicationRemindersAsync();
        Task<int> AddMedicationReminderAsync(MedicationReminder reminder);
        Task<bool> UpdateMedicationReminderAsync(MedicationReminder reminder);
        Task<bool> DeleteMedicationReminderAsync(int id);

        // Статистика
        Task<Statistics> GetStatisticsAsync(DateTime startDate, DateTime endDate);
    }

    public class Statistics
    {
        public double AverageSystolic { get; set; }
        public double AverageDiastolic { get; set; }
        public double AveragePulse { get; set; }
        public int HypertensiveCrises { get; set; }
        public Dictionary<string, int> PressureLevels { get; set; }
    }
}