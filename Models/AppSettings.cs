using System;

namespace BloodPressureTracker.Models
{
    public class AppSettings
    {
        public TimeSpan MorningMeasurementTime { get; set; } = new TimeSpan(7, 0, 0);
        public TimeSpan AfternoonMeasurementTime { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan EveningMeasurementTime { get; set; } = new TimeSpan(19, 0, 0);

        public bool MorningMeasurementEnabled { get; set; } = true;
        public bool AfternoonMeasurementEnabled { get; set; } = true;
        public bool EveningMeasurementEnabled { get; set; } = true;

        public bool MedicationRemindersEnabled { get; set; } = true;

        public string MeasurementUnit { get; set; } = "mmHg";
        public int NormalSystolicMin { get; set; } = 90;
        public int NormalSystolicMax { get; set; } = 120;
        public int NormalDiastolicMin { get; set; } = 60;
        public int NormalDiastolicMax { get; set; } = 80;

        public bool AutoBackupEnabled { get; set; } = false;
        public int BackupIntervalDays { get; set; } = 7;
    }
}