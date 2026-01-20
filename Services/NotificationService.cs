using System;
using System.Threading.Tasks;
using BloodPressureTracker.Models;
using BloodPressureTracker.Services.Interfaces;

namespace BloodPressureTracker.Services
{
    public class NotificationService : INotificationService
    {
        public Task InitializeNotificationsAsync()
        {
            Console.WriteLine("[NotificationService] InitializeNotificationsAsync called");
            return Task.CompletedTask;
        }

        public Task ScheduleMeasurementReminderAsync(TimeSpan time, string timeSlot)
        {
            Console.WriteLine($"[NotificationService] Would schedule measurement for {timeSlot} at {time}");
            return Task.CompletedTask;
        }

        public Task ScheduleMedicationReminderAsync(MedicationReminder reminder)
        {
            if (reminder != null)
            {
                Console.WriteLine($"[NotificationService] Would schedule medication: {reminder.MedicationName}");
            }
            return Task.CompletedTask;
        }

        public Task CancelAllNotificationsAsync()
        {
            Console.WriteLine("[NotificationService] Would cancel all notifications");
            return Task.CompletedTask;
        }

        public Task<bool> RequestNotificationPermissionAsync()
        {
            Console.WriteLine("[NotificationService] Would request notification permission");
            return Task.FromResult(false); // Возвращаем false, так как уведомления отключены
        }
    }
}