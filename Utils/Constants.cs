namespace BloodPressureTracker.Utils
{
    public static class Constants
    {
        // Названия уведомлений
        public const string MEASUREMENT_CHANNEL_ID = "measurement_reminders";
        public const string MEDICATION_CHANNEL_ID = "medication_reminders";

        // Ключи настроек
        public const string SETTINGS_KEY = "app_settings";
        public const string FIRST_RUN_KEY = "first_run";

        // Пути к файлам
        public const string DATABASE_NAME = "bloodpressure.db3";
        public const string EXPORT_FOLDER = "BloodPressureExports";

        // Целевые значения давления
        public const int NORMAL_SYSTOLIC_MIN = 90;
        public const int NORMAL_SYSTOLIC_MAX = 120;
        public const int NORMAL_DIASTOLIC_MIN = 60;
        public const int NORMAL_DIASTOLIC_MAX = 80;
        public const int HYPERTENSION_STAGE1_SYSTOLIC = 140;
        public const int HYPERTENSION_STAGE2_SYSTOLIC = 160;
        public const int HYPERTENSION_CRISIS_SYSTOLIC = 180;

        // Время напоминаний по умолчанию
        public static readonly TimeSpan DEFAULT_MORNING_TIME = new(7, 0, 0);
        public static readonly TimeSpan DEFAULT_AFTERNOON_TIME = new(13, 0, 0);
        public static readonly TimeSpan DEFAULT_EVENING_TIME = new(19, 0, 0);
    }
}