using System;
using System.Threading.Tasks;
using BloodPressureTracker.Models;

namespace BloodPressureTracker.Services.Interfaces
{
    public interface INotificationService
    {
        Task InitializeNotificationsAsync();
        Task ScheduleMeasurementReminderAsync(TimeSpan time, string timeSlot);
        Task ScheduleMedicationReminderAsync(MedicationReminder reminder);
        Task CancelAllNotificationsAsync();
        Task<bool> RequestNotificationPermissionAsync();
    }
}