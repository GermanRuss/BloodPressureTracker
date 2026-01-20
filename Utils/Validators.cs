namespace BloodPressureTracker.Utils
{
    public static class Validators
    {
        public static bool IsValidPressure(int systolic, int diastolic)
        {
            return systolic >= 50 && systolic <= 300 &&
                   diastolic >= 30 && diastolic <= 200 &&
                   systolic > diastolic;
        }

        public static bool IsValidPulse(int pulse)
        {
            return pulse >= 30 && pulse <= 250;
        }

        public static string GetPressureCategory(int systolic, int diastolic)
        {
            if (systolic < 90 || diastolic < 60)
                return "Гипотония";

            if (systolic <= 120 && diastolic <= 80)
                return "Нормальное";

            if (systolic <= 139 || diastolic <= 89)
                return "Предгипертония";

            if (systolic <= 159 || diastolic <= 99)
                return "Гипертония 1 ст.";

            if (systolic <= 179 || diastolic <= 109)
                return "Гипертония 2 ст.";

            return "Гипертонический криз";
        }
    }
}