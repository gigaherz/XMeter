using H.NotifyIcon;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using XMeter.Util;

namespace XMeter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        private SpeedViewModel viewModel;

        public SpeedViewModel Model => viewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;

            viewModel = new SpeedViewModel();

            var contextMenu = new ContextMenu()
            {
                DataContext = viewModel,
            };
            contextMenu.Items.Add(new MenuItem() { Header = "Exit", Command = new RelayCommand(QuitCommand) });

            notifyIcon = new TaskbarIcon()
            {
                DataContext = viewModel,
                MenuActivation = H.NotifyIcon.Core.PopupActivationMode.RightClick,
                NoLeftClickDelay = true,
                PopupActivation = H.NotifyIcon.Core.PopupActivationMode.None,
                LeftClickCommand = new RelayCommand(OpenMainWindow),
                ContextMenu = contextMenu
            };

            viewModel.NotifyIcon = notifyIcon;

            notifyIcon.ForceCreate();

            viewModel.UpdateIcon();
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
