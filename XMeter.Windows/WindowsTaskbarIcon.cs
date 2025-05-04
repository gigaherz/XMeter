using H.NotifyIcon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using XMeter.Common;

namespace XMeter.Windows
{
    public class WindowsTaskbarIcon : INotificationIcon
    {
        public static INotificationIcon Create()
        {
            return new WindowsTaskbarIcon();
        }

        private readonly TaskbarIcon notifyIcon;

        private WindowsTaskbarIcon()
        {
            notifyIcon = new TaskbarIcon()
            {
                MenuActivation = H.NotifyIcon.Core.PopupActivationMode.RightClick,
                NoLeftClickDelay = true,
                PopupActivation = H.NotifyIcon.Core.PopupActivationMode.None
            };
        }

        public void ForceCreate()
        {
            notifyIcon.ForceCreate();
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
        }

        public object? DataContext
        {
            get => notifyIcon.DataContext;
            set => notifyIcon.DataContext = value;
        }

        public ICommand? LeftClickCommand
        {
            get => notifyIcon.LeftClickCommand;
            set => notifyIcon.LeftClickCommand = value;
        }

        public ContextMenu ContextMenu
        {
            get => notifyIcon.ContextMenu;
            set => notifyIcon.ContextMenu = value;
        }
        public string ToolTipText {
            get => notifyIcon.ToolTipText;
            set => notifyIcon.ToolTipText = value;
        }
        public Icon? Icon
        {
            get => notifyIcon.Icon;
            set => notifyIcon.Icon = value;
        }
    }
}
