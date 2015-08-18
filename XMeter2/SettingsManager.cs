using Microsoft.Win32;
using System;
using System.Windows;

namespace XMeter2
{
    class SettingsManager
    {
        public static bool StartOnLogon;

        public static void ReadSettings()
        {
            try
            {
                var value = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", -1);
                if (value < 0)
                {
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", 100);
                    value = 100;
                }
                Application.Current.MainWindow.Opacity = value / 100.0;
            }
            catch (Exception)
            {
                Application.Current.MainWindow.Opacity = 1.0;
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", 100);
            }

            //object path = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Current Version\\Run","XMeter");

            //if (path != null)
            //{
            //    if(Application.ExecutablePath == (string)path)
            //        startOnLogon = true;
            //}
        }

        public static void WriteSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", (int)(Application.Current.MainWindow.Opacity * 100));

            //object path = Registry.CurrentUser.GetValue("Software\\Microsoft\\Windows\\Current Version\\Run","XMeter");

            //if ((path != null) && !startOnLogon)
            //{
            //    Registry.DeleteValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Current Version\\Run","XMeter");
            //}

            //if(startOnLogon)
            //    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Current Version\\Run","XMeter", Application.ExecutablePath);
        }
    }
}
