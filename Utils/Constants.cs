

namespace BloodPressureTracker.Utils
{
    // ВАЖНО: Добавьте 'public' перед 'static class'
    public static class Constants
    {
        // Пути к файлам
        public const string DATABASE_NAME = "bloodpressure.db3";

        // Целевые значения давления
        public const int NORMAL_SYSTOLIC_MIN = 90;
        public const int NORMAL_SYSTOLIC_MAX = 120;
        public const int NORMAL_DIASTOLIC_MIN = 60;
        public const int NORMAL_DIASTOLIC_MAX = 80;

        // Время напоминаний по умолчанию
        public static readonly TimeSpan DEFAULT_MORNING_TIME = new TimeSpan(7, 0, 0);
        public static readonly TimeSpan DEFAULT_AFTERNOON_TIME = new TimeSpan(13, 0, 0);
        public static readonly TimeSpan DEFAULT_EVENING_TIME = new TimeSpan(19, 0, 0);
    }
}