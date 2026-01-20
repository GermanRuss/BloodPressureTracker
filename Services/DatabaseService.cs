using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace BloodPressureTracker.Services
{
    public class DatabaseService : IDatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _initialized = false;
        private readonly object _lock = new object();

        public DatabaseService()
        {
        }

        // Инициализация базы данных
        private async Task InitializeDatabase()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                var databasePath = Path.Combine(FileSystem.AppDataDirectory, "bloodpressure.db3");
                _database = new SQLiteAsyncConnection(databasePath);

                // Создаем таблицы
                _database.CreateTableAsync<BloodPressureRecord>().Wait();
                _database.CreateTableAsync<AppSettings>().Wait();
                _database.CreateTableAsync<MedicationReminder>().Wait();

                _initialized = true;
            }
        }

        // Реализация методов интерфейса

        public Task InitializeDatabaseAsync()
        {
            return InitializeDatabase();
        }

        public async Task<int> AddBloodPressureRecordAsync(BloodPressureRecord record)
        {
            await InitializeDatabase();
            return await _database.InsertAsync(record);
        }

        public async Task<bool> UpdateBloodPressureRecordAsync(BloodPressureRecord record)
        {
            await InitializeDatabase();
            record.UpdatedAt = DateTime.Now;
            var result = await _database.UpdateAsync(record);
            return result > 0;
        }

        public async Task<bool> DeleteBloodPressureRecordAsync(int id)
        {
            await InitializeDatabase();
            var result = await _database.DeleteAsync<BloodPressureRecord>(id);
            return result > 0;
        }

        public async Task<BloodPressureRecord> GetBloodPressureRecordAsync(int id)
        {
            await InitializeDatabase();
            return await _database.Table<BloodPressureRecord>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BloodPressureRecord>> GetBloodPressureRecordsAsync(DateTime startDate, DateTime endDate)
        {
            await InitializeDatabase();
            return await _database.Table<BloodPressureRecord>()
                .Where(r => r.Date >= startDate && r.Date <= endDate)
                .ToListAsync();
        }

        public async Task<List<BloodPressureRecord>> GetAllBloodPressureRecordsAsync()
        {
            await InitializeDatabase();
            return await _database.Table<BloodPressureRecord>().ToListAsync();
        }

        public async Task<AppSettings> GetAppSettingsAsync()
        {
            await InitializeDatabase();
            var settings = await _database.Table<AppSettings>().FirstOrDefaultAsync();
            return settings ?? new AppSettings();
        }

        public async Task<bool> SaveAppSettingsAsync(AppSettings settings)
        {
            await InitializeDatabase();
            if (settings.Id == 0)
            {
                var result = await _database.InsertAsync(settings);
                return result > 0;
            }
            else
            {
                var result = await _database.UpdateAsync(settings);
                return result > 0;
            }
        }

        public async Task<List<MedicationReminder>> GetMedicationRemindersAsync()
        {
            await InitializeDatabase();
            return await _database.Table<MedicationReminder>().ToListAsync();
        }

        public async Task<int> AddMedicationReminderAsync(MedicationReminder reminder)
        {
            await InitializeDatabase();
            return await _database.InsertAsync(reminder);
        }

        public async Task<bool> UpdateMedicationReminderAsync(MedicationReminder reminder)
        {
            await InitializeDatabase();
            var result = await _database.UpdateAsync(reminder);
            return result > 0;
        }

        public async Task<bool> DeleteMedicationReminderAsync(int id)
        {
            await InitializeDatabase();
            var result = await _database.DeleteAsync<MedicationReminder>(id);
            return result > 0;
        }

        public async Task<Statistics> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            await InitializeDatabase();
            var records = await GetBloodPressureRecordsAsync(startDate, endDate);

            var statistics = new Statistics();

            if (!records.Any())
                return statistics;

            // Собираем все значения давления
            var systolicValues = new List<int>();
            var diastolicValues = new List<int>();
            var pulseValues = new List<int>();

            foreach (var record in records)
            {
                if (record.SystolicMorning > 0 && record.DiastolicMorning > 0)
                {
                    systolicValues.Add(record.SystolicMorning);
                    diastolicValues.Add(record.DiastolicMorning);
                    pulseValues.Add(record.PulseMorning);
                }

                if (record.SystolicAfternoon > 0 && record.DiastolicAfternoon > 0)
                {
                    systolicValues.Add(record.SystolicAfternoon);
                    diastolicValues.Add(record.DiastolicAfternoon);
                    pulseValues.Add(record.PulseAfternoon);
                }

                if (record.SystolicEvening > 0 && record.DiastolicEvening > 0)
                {
                    systolicValues.Add(record.SystolicEvening);
                    diastolicValues.Add(record.DiastolicEvening);
                    pulseValues.Add(record.PulseEvening);
                }
            }

            if (systolicValues.Any())
            {
                statistics.AverageSystolic = systolicValues.Average();
                statistics.AverageDiastolic = diastolicValues.Average();
                statistics.AveragePulse = pulseValues.Any(p => p > 0) ? pulseValues.Where(p => p > 0).Average() : 0;
            }

            // Подсчет гипертонических кризов
            statistics.HypertensiveCrises = records.Count(r =>
                r.SystolicMorning >= 180 || r.DiastolicMorning >= 110 ||
                r.SystolicAfternoon >= 180 || r.DiastolicAfternoon >= 110 ||
                r.SystolicEvening >= 180 || r.DiastolicEvening >= 110);

            statistics.PressureLevels = new Dictionary<string, int>();

            return statistics;
        }
    }
}