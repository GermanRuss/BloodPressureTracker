using System;

namespace BloodPressureTracker.Models
{
    public class MedicationReminder
    {
        public int Id { get; set; }
        public string MedicationName { get; set; }
        public TimeSpan MorningTime { get; set; }
        public TimeSpan AfternoonTime { get; set; }
        public TimeSpan EveningTime { get; set; }
        public bool IsEnabled { get; set; }
        public string Dosage { get; set; }
    }
}