
using H.NotifyIcon.Core;
using System;
using System.Runtime.InteropServices;
using XMeter.Common;
using XMeter.Windows;

namespace XMeter
{
    public class PlatformImplementations
    {
        public static IDataSource DataSource { get; private set; }

        public static INotificationIcon NotificationIcon { get; private set; }

        public static ISettings Settings { get; set; }

        private static IDataSource CreateDataSource()
        {
            if (OperatingSystem.IsWindows())
            {
                return WMIDataSource.Construct();
            }
            else
            {
                throw new NotImplementedException("Data parsing not implemented for platform " + RuntimeInformation.OSDescription);
            }
        }

        private static INotificationIcon CreateNotificationIcon()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsTaskbarIcon.Construct();
            }
            else
            {
                throw new NotImplementedException("Data parsing not implemented for platform " + RuntimeInformation.OSDescription);
            }
        }

        private static ISettings CreateSettings()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsRegistrySettings.Construct();
            }
            else
            {
                throw new NotImplementedException("Settings not implemented for platform " + RuntimeInformation.OSDescription);
            }
        }

        internal static void Initialize()
        {
            DataSource = CreateDataSource();
            NotificationIcon = CreateNotificationIcon();
            Settings = CreateSettings();
        }
    }
}