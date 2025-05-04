using H.NotifyIcon;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using XMeter.Util;

namespace XMeter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            PlatformImplementations.Initialize();

            var contextMenu = new ContextMenu()
            {
                DataContext = SpeedViewModel.Instance,
            };
            contextMenu.Items.Add(new MenuItem() { Header = "Exit", Command = new RelayCommand(QuitCommand) });

            SpeedViewModel.Instance.NotifyIcon = PlatformImplementations.NotificationIcon;

            PlatformImplementations.NotificationIcon.DataContext = SpeedViewModel.Instance;
            PlatformImplementations.NotificationIcon.LeftClickCommand = new RelayCommand(OpenMainWindow);
            PlatformImplementations.NotificationIcon.ContextMenu = contextMenu;
            PlatformImplementations.NotificationIcon.ForceCreate();

            SpeedViewModel.Instance.UpdateIcon();
        }

        private void QuitCommand(object obj)
        {
           this.MainWindow.Close();
        }

        private void OpenMainWindow(object obj)
        {
            (this.MainWindow as MainWindow).Popup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            PlatformImplementations.NotificationIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
