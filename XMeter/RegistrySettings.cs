using System.Windows.Forms;
using Microsoft.Win32;

namespace XMeter
{
    class RegistrySettings
    {
        public enum ConfigKey
        {
            StartMinimized,
            WindowOpacity
        }

        public static bool TryGetConfig<T>(ConfigKey key, out T value)
        {
            var updatesKey = Program.BaseKey.OpenSubKey(Program.AppKey);
            if (updatesKey == null)
            {
                value = default(T);
                return false;
            }

            var v = updatesKey.GetValue(key.ToString());
            updatesKey.Close();

            if (v == null)
            {
                value = default(T);
                return false;
            }

            if (typeof(T) == typeof(bool))
            {
                value = (T)(object)bool.Parse((string)v);
                return true;
            }

            if (typeof(T) == typeof(double))
            {
                value = (T)(object)double.Parse((string)v);
                return true;
            }

            if (typeof(T) != v.GetType())
            {
                value = default(T);
                return false;
            }

            value = (T)v;
            return true;
        }

        public static T GetConfig<T>(ConfigKey key, T defaultValue = default(T))
        {
            T value;

            if(!TryGetConfig(key, out value))
            {
                if (typeof (T) == typeof (long))
                    SetConfig(key, defaultValue, RegistryValueKind.QWord);
                else
                    SetConfig(key, defaultValue);
                return defaultValue;
            }

            return value;
        }

        public static void SetConfig(ConfigKey key, long value)
        {
            SetConfig(key, value, RegistryValueKind.QWord);
        }

        public static void SetConfig(ConfigKey key, int value)
        {
            SetConfig(key, value, RegistryValueKind.DWord);
        }

        public static void SetConfig(ConfigKey key, byte[] value)
        {
            SetConfig(key, value, RegistryValueKind.Binary);
        }

        public static void SetConfig<T>(ConfigKey key, T value, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            T oldValue;

            if (TryGetConfig(key, out oldValue))
            {
                if(oldValue.Equals(value))
                    return;
            }

            var updatesKey = Program.BaseKey.CreateSubKey(Program.AppKey);
            if (updatesKey == null) return;

            var val = updatesKey.GetValue(key.ToString());
            if (val != null && updatesKey.GetValueKind(key.ToString()) != valueKind)
                updatesKey.DeleteValue(key.ToString());

            updatesKey.SetValue(key.ToString(), value, valueKind);
            updatesKey.Close();
        }

        public static bool StartupState
        {
            get
            {
                var startupKey = Program.BaseKey.OpenSubKey(Program.RunKey);

                if (startupKey == null) return false;

                var text = (string)startupKey.GetValue(Application.ProductName);

                bool value = string.CompareOrdinal(text, Application.ExecutablePath) == 0;

                startupKey.Close();

                return value;
            }
            set
            {
                var startupKey = Program.BaseKey.OpenSubKey(Program.RunKey, true);

                if (startupKey == null) return;

                var old = (string)startupKey.GetValue(Application.ProductName);
                var oldSame = old != null && string.CompareOrdinal(old, Application.ExecutablePath) == 0;

                if (value && oldSame)
                {
                    startupKey.Close();
                    return;
                }

                if (old != null)
                    startupKey.DeleteValue(Application.ProductName, false);
                else if (value)
                    startupKey.SetValue(Application.ProductName, Application.ExecutablePath);

                startupKey.Close();
            }
        }

    }
}

