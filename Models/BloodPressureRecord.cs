using System;

namespace BloodPressureTracker.Models
{
    public class BloodPressureRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SystolicMorning { get; set; }      // Верхнее давление утро
        public int DiastolicMorning { get; set; }     // Нижнее давление утро
        public int PulseMorning { get; set; }         // Пульс утро

        public int SystolicAfternoon { get; set; }    // Верхнее давление обед
        public int DiastolicAfternoon { get; set; }   // Нижнее давление обед
        public int PulseAfternoon { get; set; }       // Пульс обед

        public int SystolicEvening { get; set; }      // Верхнее давление вечер
        public int DiastolicEvening { get; set; }     // Нижнее давление вечер
        public int PulseEvening { get; set; }         // Пульс вечер

        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}