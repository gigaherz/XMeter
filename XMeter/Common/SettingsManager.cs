using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using XMeter.Windows;

namespace XMeter.Common
{
    public class SettingsManager
    {
        public static readonly int DefaultPreferredWidth = 384;
        public static readonly int DefaultPreferredHeight = 240;

        public static readonly ISettings Settings = CreateSettings();

        private static ISettings CreateSettings()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsRegistrySettings.Create();
            }
            else
            {
                throw new NotImplementedException("Settings not implemented for platform " + RuntimeInformation.OSDescription);
            }
        }

    }

    public interface ISettings : INotifyPropertyChanged
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public void ReadSettings();

        public void WriteSettings();
    }
}
