using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace BloodPressureTracker.Platforms.Android
{
    [Service]
    public class NotificationForegroundService : Service
    {
        private const int NOTIFICATION_ID = 1001;
        private const string CHANNEL_ID = "bloodpressure_reminders";

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();

            // Используем стандартную системную иконку
            // Android.Resource.Drawable доступен через глобальное пространство имен
            var smallIconId = global::Android.Resource.Drawable.StatNotifyMore;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notification = new Notification.Builder(this, CHANNEL_ID)
                    .SetContentTitle("Напоминания о давлении")
                    .SetContentText("Сервис напоминаний активен")
                    .SetSmallIcon(smallIconId)  // ИСПРАВЛЕНО
                    .Build();

                StartForeground(NOTIFICATION_ID, notification);
            }
            else
            {
                // Для старых версий Android
                var notification = new Notification.Builder(this)
                    .SetContentTitle("Напоминания о давлении")
                    .SetContentText("Сервис напоминаний активен")
                    .SetSmallIcon(smallIconId)  // ИСПРАВЛЕНО
                    .Build();

                StartForeground(NOTIFICATION_ID, notification);
            }

            return StartCommandResult.Sticky;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Напоминания о давлении",
                    NotificationImportance.High);

                channel.Description = "Уведомления о замерах давления и приеме лекарств";

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                StopForeground(StopForegroundFlags.Remove);
            }
            else
            {
#pragma warning disable CA1422
                StopForeground(true);
#pragma warning restore CA1422
            }
        }
    }
}