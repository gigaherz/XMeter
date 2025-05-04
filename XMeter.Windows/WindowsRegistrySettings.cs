using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using XMeter.Common;

namespace XMeter.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsRegistrySettings : ISettings, INotifyPropertyChanged

    {
        public static ISettings Construct()
        {
            return new WindowsRegistrySettings();
        }

        private int _width;
        private int _height;

        public int Width
        {
            get => _width;

            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        private WindowsRegistrySettings() { }

        public void ReadSettings()
        {
            Width = (int)(Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredWidth", ISettings.DefaultPreferredWidth) ?? ISettings.DefaultPreferredWidth);
            Height = (int)(Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredHeight", ISettings.DefaultPreferredHeight) ?? ISettings.DefaultPreferredHeight);
        }

        public void WriteSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredWidth", Width);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredHeight", Height);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
