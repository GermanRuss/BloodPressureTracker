using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;

namespace BloodPressureTracker.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class AlarmReceiver : BroadcastReceiver
    {
        private const string CHANNEL_ID = "bloodpressure_reminders";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
                RestartReminders(context);
                return;
            }

            CreateNotificationChannel(context);

            // Используем глобальное пространство имен для ресурсов Android
            var smallIconId = global::Android.Resource.Drawable.StatNotifyMore;

            var notification = new NotificationCompat.Builder(context, CHANNEL_ID)
                .SetContentTitle("Время измерить давление")
                .SetContentText("Не забудьте сделать замер")
                .SetSmallIcon(smallIconId)  // ИСПРАВЛЕНО
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetAutoCancel(true)
                .Build();

            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Notify(GetNotificationId(), notification);
        }

        private void CreateNotificationChannel(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Напоминания о давлении",
                    NotificationImportance.High);

                channel.Description = "Уведомления о замерах давления";

                var notificationManager = context.GetSystemService(Context.NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        private int GetNotificationId()
        {
            return (int)DateTime.Now.Ticks % int.MaxValue;
        }

        private void RestartReminders(Context context)
        {
            // Логика перезапуска напоминаний
        }
    }
}