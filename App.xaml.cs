using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;

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