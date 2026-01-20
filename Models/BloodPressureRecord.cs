using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BloodPressureTracker.Models
{
    public class BloodPressureRecord : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int SystolicMorning { get; set; }
        public int DiastolicMorning { get; set; }
        public int PulseMorning { get; set; }
        public int SystolicAfternoon { get; set; }
        public int DiastolicAfternoon { get; set; }
        public int PulseAfternoon { get; set; }
        public int SystolicEvening { get; set; }
        public int DiastolicEvening { get; set; }
        public int PulseEvening { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Вычисляемые свойства для отображения
        public string MorningPressureDisplay =>
            SystolicMorning > 0 && DiastolicMorning > 0
            ? $"{SystolicMorning}/{DiastolicMorning}" : "--";

        public string AfternoonPressureDisplay =>
            SystolicAfternoon > 0 && DiastolicAfternoon > 0
            ? $"{SystolicAfternoon}/{DiastolicAfternoon}" : "--";

        public string EveningPressureDisplay =>
            SystolicEvening > 0 && DiastolicEvening > 0
            ? $"{SystolicEvening}/{DiastolicEvening}" : "--";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}