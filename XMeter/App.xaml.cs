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
        private TaskbarIcon notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            var contextMenu = new ContextMenu()
            {
                DataContext = SpeedViewModel.Instance,
            };
            contextMenu.Items.Add(new MenuItem() { Header = "Exit", Command = new RelayCommand(QuitCommand) });

            notifyIcon = new TaskbarIcon()
            {
                DataContext = SpeedViewModel.Instance,
                MenuActivation = H.NotifyIcon.Core.PopupActivationMode.RightClick,
                NoLeftClickDelay = true,
                PopupActivation = H.NotifyIcon.Core.PopupActivationMode.None,
                LeftClickCommand = new RelayCommand(OpenMainWindow),
                ContextMenu = contextMenu
            };

            SpeedViewModel.Instance.NotifyIcon = notifyIcon;

            notifyIcon.ForceCreate();

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
            notifyIcon?.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}
