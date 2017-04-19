using System.Diagnostics;
using System.Windows;

namespace XMeter2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            base.OnStartup(e);
        }
    }
}
