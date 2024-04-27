using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using XMeter.Annotations;
using XMeter.Common;

namespace XMeter.Windows
{
    [SupportedOSPlatform("windows")]
    internal class WindowsRegistrySettings : ISettings

    {
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

        public void ReadSettings()
        {
            Width = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredWidth", SettingsManager.DefaultPreferredWidth);
            Height = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredHeight", SettingsManager.DefaultPreferredHeight);
        }

        public void WriteSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredWidth", Width);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "PreferredHeight", Height);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal static ISettings Create()
        {
            return new WindowsRegistrySettings();
        }
    }
}
