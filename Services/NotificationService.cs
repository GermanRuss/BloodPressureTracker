using Android.Webkit;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;
using Java.Security;
using Plugin.LocalNotification;
using System;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace BloodPressureTracker.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDatabaseService _databaseService;
        private bool _isInitialized = false;

        public NotificationService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task InitializeNotificationsAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                // Запрашиваем разрешение на уведомления
                await RequestNotificationPermissionAsync();

                // Очищаем старые уведомления
                await CancelAllNotificationsAsync();

                // Загружаем настройки
                var settings = await _databaseService.GetAppSettingsAsync();
                if (settings == null)
                    return;

                // Создаем каналы уведомлений
                CreateNotificationChannels();

                // Планируем уведомления о замерах
                if (settings.MorningMeasurementEnabled)
                    await ScheduleMeasurementReminderAsync(settings.MorningMeasurementTime, "Утро");

                if (settings.AfternoonMeasurementEnabled)
                    await ScheduleMeasurementReminderAsync(settings.AfternoonMeasurementTime, "Обед");

                if (settings.EveningMeasurementEnabled)
                    await ScheduleMeasurementReminderAsync(settings.EveningMeasurementTime, "Вечер");

                // Планируем уведомления о лекарствах
                if (settings.MedicationRemindersEnabled)
                {
                    var reminders = await _databaseService.GetMedicationRemindersAsync();
                    foreach (var reminder in reminders.Where(r => r.IsEnabled))
                    {
                        await ScheduleMedicationReminderAsync(reminder);
                    }
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации уведомлений: {ex.Message}");
            }
        }

        private void CreateNotificationChannels()
        {
            try
            {
                // Канал для напоминаний о замерах
                var measurementChannel = new NotificationChannelRequest
                {
                    Id = Constants.MEASUREMENT_CHANNEL_ID,
                    Name = "Напоминания о замерах давления",
                    Description = "Уведомления о необходимости измерить давление",
                    Importance = Plugin.LocalNotification.Android.NotificationImportance.High
                };

                // Канал для напоминаний о лекарствах
                var medicationChannel = new NotificationChannelRequest
                {
                    Id = Constants.MEDICATION_CHANNEL_ID,
                    Name = "Напоминания о лекарствах",
                    Description = "Уведомления о приёме лекарств",
                    Importance = Plugin.LocalNotification.Android.NotificationImportance.High
                };

                // Создаем каналы
                if (LocalNotificationCenter.Current.AreNotificationsEnabled())
                {
                    LocalNotificationCenter.Current.ClearNotificationChannel(measurementChannel.Id);
                    LocalNotificationCenter.Current.ClearNotificationChannel(medicationChannel.Id);

                    LocalNotificationCenter.Current.RegisterNotificationChannel(measurementChannel);
                    LocalNotificationCenter.Current.RegisterNotificationChannel(medicationChannel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания каналов уведомлений: {ex.Message}");
            }
        }

        public async Task ScheduleMeasurementReminderAsync(TimeSpan time, string timeSlot)
        {
            try
            {
                var notificationId = GetMeasurementNotificationId(timeSlot);

                var request = new NotificationRequest
                {
                    NotificationId = notificationId,
                    Title = "Время измерить давление",
                    Description = $"Не забудьте измерить давление ({timeSlot.ToLower()})",
                    CategoryType = NotificationCategoryType.Reminder,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = DateTime.Today.Add(time),
                        RepeatType = NotificationRepeat.Daily
                    },
                    Android = new AndroidOptions
                    {
                        ChannelId = Constants.MEASUREMENT_CHANNEL_ID,
                        AutoCancel = false,
                        Ongoing = false
                    }
                };

                await LocalNotificationCenter.Current.Show(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка планирования уведомления: {ex.Message}");
            }
        }

        public async Task ScheduleMedicationReminderAsync(MedicationReminder reminder)
        {
            try
            {
                // Утреннее уведомление
                if (reminder.MorningTime.TotalSeconds > 0)
                {
                    await ScheduleSingleMedicationReminder(
                        reminder,
                        reminder.MorningTime,
                        "Утро",
                        1000 + reminder.Id);
                }

                // Обеденное уведомление
                if (reminder.AfternoonTime.TotalSeconds > 0)
                {
                    await ScheduleSingleMedicationReminder(
                        reminder,
                        reminder.AfternoonTime,
                        "Обед",
                        2000 + reminder.Id);
                }

                // Вечернее уведомление
                if (reminder.EveningTime.TotalSeconds > 0)
                {
                    await ScheduleSingleMedicationReminder(
                        reminder,
                        reminder.EveningTime,
                        "Вечер",
                        3000 + reminder.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка планирования уведомления о лекарстве: {ex.Message}");
            }
        }

        private async Task ScheduleSingleMedicationReminder(
            MedicationReminder reminder,
            TimeSpan time,
            string timeOfDay,
            int notificationId)
        {
            var request = new NotificationRequest
            {
                NotificationId = notificationId,
                Title = $"Приём лекарства ({timeOfDay})",
                Description = $"{reminder.MedicationName} - {reminder.Dosage}",
                CategoryType = NotificationCategoryType.Alarm,
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.Today.Add(time),
                    RepeatType = NotificationRepeat.Daily
                },
                Android = new AndroidOptions
                {
                    ChannelId = Constants.MEDICATION_CHANNEL_ID,
                    AutoCancel = false,
                    Ongoing = false
                }
            };

            await LocalNotificationCenter.Current.Show(request);
        }

        public async Task CancelAllNotificationsAsync()
        {
            try
            {
                await LocalNotificationCenter.Current.CancelAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отмены уведомлений: {ex.Message}");
            }
        }

        public async Task<bool> RequestNotificationPermissionAsync()
        {
            try
            {
                var permission = await Permissions.RequestAsync<Permissions.Notifications>();
                return permission == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка запроса разрешений: {ex.Message}");
                return false;
            }
        }

        private int GetMeasurementNotificationId(string timeSlot)
        {
            return timeSlot switch
            {
                "Утро" => 1,
                "Обед" => 2,
                "Вечер" => 3,
                _ => 0
            };
        }
    }
}