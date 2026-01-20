using System;
using System.Threading.Tasks;

namespace BloodPressureTracker.Utils
{
    public static class Extensions
    {
        public static async void SafeFireAndForget(
            this Task task,
            bool returnToCallingContext = true,
            Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeFireAndForget error: {ex.Message}");
            }
        }

        public static string ToPressureString(this int systolic, int diastolic)
        {
            return $"{systolic}/{diastolic}";
        }

        public static string GetPressureDisplay(this BloodPressureRecord record, string timeSlot)
        {
            return timeSlot.ToLower() switch
            {
                "morning" => record.SystolicMorning > 0 && record.DiastolicMorning > 0
                    ? $"{record.SystolicMorning}/{record.DiastolicMorning}"
                    : "--/--",
                "afternoon" => record.SystolicAfternoon > 0 && record.DiastolicAfternoon > 0
                    ? $"{record.SystolicAfternoon}/{record.DiastolicAfternoon}"
                    : "--/--",
                "evening" => record.SystolicEvening > 0 && record.DiastolicEvening > 0
                    ? $"{record.SystolicEvening}/{record.DiastolicEvening}"
                    : "--/--",
                _ => "--/--"
            };
        }

        public static string ToLocalDateString(this DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }
    }
}