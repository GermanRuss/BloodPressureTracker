using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BloodPressureTracker.Services
{
    public class DatabaseService : Interfaces.IDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        public DatabaseService()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        private async Task InitializeAsync()
        {
            try
            {
                if (_isInitialized && _database != null)
                    return;

                // Получаем путь для базы данных
                var databasePath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    Constants.DATABASE_NAME);

                _database = new SQLiteAsyncConnection(databasePath);

                // Создаем таблицы
                await _database.CreateTableAsync<BloodPressureRecord>();
                await _database.CreateTableAsync<MedicationReminder>();
                await _database.CreateTableAsync<AppSettings>();

                // Проверяем, есть ли настройки по умолчанию
                var settings = await GetAppSettingsAsync();
                if (settings == null)
                {
                    var defaultSettings = new AppSettings();
                    await SaveAppSettingsAsync(defaultSettings);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (!_isInitialized || _database == null)
            {
                await InitializeAsync();
            }
        }

        // ============ Методы для записей давления ============
        public async Task<int> AddBloodPressureRecordAsync(BloodPressureRecord record)
        {
            await EnsureInitializedAsync();
            record.CreatedAt = DateTime.Now;
            record.UpdatedAt = DateTime.Now;
            return await _database.InsertAsync(record);
        }

        public async Task<bool> UpdateBloodPressureRecordAsync(BloodPressureRecord record)
        {
            await EnsureInitializedAsync();
            record.UpdatedAt = DateTime.Now;
            var result = await _database.UpdateAsync(record);
            return result > 0;
        }

        public async Task<bool> DeleteBloodPressureRecordAsync(int id)
        {
            await EnsureInitializedAsync();
            var record = await GetBloodPressureRecordAsync(id);
            if (record != null)
            {
                var result = await _database.DeleteAsync(record);
                return result > 0;
            }
            return false;
        }

        public async Task<BloodPressureRecord> GetBloodPressureRecordAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _database.Table<BloodPressureRecord>()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<BloodPressureRecord>> GetBloodPressureRecordsAsync(
            DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();
            return await _database.Table<BloodPressureRecord>()
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task<List<BloodPressureRecord>> GetAllBloodPressureRecordsAsync()
        {
            await EnsureInitializedAsync();
            return await _database.Table<BloodPressureRecord>()
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        // ============ Методы для настроек ============
        public async Task<AppSettings> GetAppSettingsAsync()
        {
            await EnsureInitializedAsync();
            return await _database.Table<AppSettings>().FirstOrDefaultAsync();
        }

        public async Task<bool> SaveAppSettingsAsync(AppSettings settings)
        {
            await EnsureInitializedAsync();

            var existing = await GetAppSettingsAsync();
            if (existing == null)
            {
                var result = await _database.InsertAsync(settings);
                return result > 0;
            }
            else
            {
                settings.Id = existing.Id;
                var result = await _database.UpdateAsync(settings);
                return result > 0;
            }
        }

        // ============ Методы для напоминаний о лекарствах ============
        public async Task<List<MedicationReminder>> GetMedicationRemindersAsync()
        {
            await EnsureInitializedAsync();
            return await _database.Table<MedicationReminder>()
                .OrderBy(r => r.MedicationName)
                .ToListAsync();
        }

        public async Task<int> AddMedicationReminderAsync(MedicationReminder reminder)
        {
            await EnsureInitializedAsync();
            return await _database.InsertAsync(reminder);
        }

        public async Task<bool> UpdateMedicationReminderAsync(MedicationReminder reminder)
        {
            await EnsureInitializedAsync();
            var result = await _database.UpdateAsync(reminder);
            return result > 0;
        }

        public async Task<bool> DeleteMedicationReminderAsync(int id)
        {
            await EnsureInitializedAsync();
            var reminder = await _database.Table<MedicationReminder>()
                .FirstOrDefaultAsync(r => r.Id == id);
            if (reminder != null)
            {
                var result = await _database.DeleteAsync(reminder);
                return result > 0;
            }
            return false;
        }

        public async Task InitializeDatabaseAsync()
        {
            await InitializeAsync(); // Просто вызывает уже существующий приватный метод
        }

        // ============ Методы для статистики ============
        public async Task<Statistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            await EnsureInitializedAsync();

            var records = await GetBloodPressureRecordsAsync(startDate, endDate);
            if (records == null || records.Count == 0)
                return new Statistics();

            var stats = new Statistics
            {
                AverageSystolic = Math.Round(records
                    .Where(r => r.SystolicMorning > 0 && r.SystolicAfternoon > 0 && r.SystolicEvening > 0)
                    .SelectMany(r => new[] { r.SystolicMorning, r.SystolicAfternoon, r.SystolicEvening })
                    .Average(), 1),

                AverageDiastolic = Math.Round(records
                    .Where(r => r.DiastolicMorning > 0 && r.DiastolicAfternoon > 0 && r.DiastolicEvening > 0)
                    .SelectMany(r => new[] { r.DiastolicMorning, r.DiastolicAfternoon, r.DiastolicEvening })
                    .Average(), 1),

                AveragePulse = Math.Round(records
                    .Where(r => r.PulseMorning > 0 && r.PulseAfternoon > 0 && r.PulseEvening > 0)
                    .SelectMany(r => new[] { r.PulseMorning, r.PulseAfternoon, r.PulseEvening })
                    .Average(), 1),

                HypertensiveCrises = records.Count(r =>
                    r.SystolicMorning >= 180 || r.SystolicAfternoon >= 180 || r.SystolicEvening >= 180 ||
                    r.DiastolicMorning >= 120 || r.DiastolicAfternoon >= 120 || r.DiastolicEvening >= 120)
            };

            // Анализ уровней давления
            stats.PressureLevels = new Dictionary<string, int>
            {
                ["Нормальное"] = 0,
                ["Повышенное"] = 0,
                ["Гипертония 1"] = 0,
                ["Гипертония 2"] = 0,
                ["Криз"] = 0
            };

            foreach (var record in records)
            {
                var pressures = new[]
                {
                    (record.SystolicMorning, record.DiastolicMorning),
                    (record.SystolicAfternoon, record.DiastolicAfternoon),
                    (record.SystolicEvening, record.DiastolicEvening)
                };

                foreach (var (systolic, diastolic) in pressures)
                {
                    if (systolic > 0 && diastolic > 0)
                    {
                        var category = GetPressureCategory(systolic, diastolic);
                        stats.PressureLevels[category]++;
                    }
                }
            }

            return stats;
        }

        private string GetPressureCategory(int systolic, int diastolic)
        {
            if (systolic < 120 && diastolic < 80) return "Нормальное";
            if (systolic <= 129 && diastolic <= 84) return "Повышенное";
            if (systolic <= 139 || diastolic <= 89) return "Гипертония 1";
            if (systolic <= 159 || diastolic <= 99) return "Гипертония 2";
            return "Криз";
        }
    }
}