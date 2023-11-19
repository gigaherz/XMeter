using Microsoft.Win32;
using System;
using System.Windows;

namespace XMeter
{
    class SettingsManager
    {
        public static void ReadSettings()
        {
            if (OperatingSystem.IsWindows())
            {
                Application.Current.MainWindow.Width = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredWidth", 384);
                Application.Current.MainWindow.Height = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredHeight", 240);
            }
        }

        public static void WriteSettings()
        {
            if (OperatingSystem.IsWindows())
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredWidth", (int)Application.Current.MainWindow.ActualWidth);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredHeight", (int)Application.Current.MainWindow.ActualHeight);
            }
        }
    }
}
