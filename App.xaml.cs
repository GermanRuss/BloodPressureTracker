using BloodPressureTracker.Services;
using BloodPressureTracker.Services.Interfaces;

namespace BloodPressureTracker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}