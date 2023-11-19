using Microsoft.Win32;
using System;
using System.Runtime.Versioning;
using System.Windows.Media;

namespace XMeter
{
    [SupportedOSPlatform("windows")]
    internal class AccentColorUtil
    {
        internal static void SetupAccentsUpdate(MainWindow mainWindow)
        {
            [SupportedOSPlatform("windows")]
            void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
            {
                UpdateAccentColor(mainWindow);
            }

            SystemEvents.UserPreferenceChanging += SystemEvents_UserPreferenceChanging;
        }

        public static void UpdateAccentColor(MainWindow mainWindow)
        {
#if false
            File.WriteAllLines(@"F:\Accents.txt", AccentColorSet.ActiveSet.GetAllColorNames().Select(s => {
                var c = AccentColorSet.ActiveSet[s];
                return $"{s}: {c}";
            }));
#endif
            var background = AccentColorSet.ActiveSet["SystemBackground"];
            var backgroundDark = AccentColorSet.ActiveSet["SystemBackgroundDarkTheme"];
            var shadow = background;
            var text = AccentColorSet.ActiveSet["SystemText"];
            var accent = AccentColorSet.ActiveSet["SystemAccentLight3"];
            //if (background != backgroundDark)
            //{
            //    accent = AccentColorSet.ActiveSet["SystemAccentDark3"];
            //}
            accent.A = 128;
            background.A = 160;

            if (WindowsNatives.MakeEdgesRounded(mainWindow))
            {
                mainWindow.SeparateFromTaskbar = true;
            }

            if (WindowsNatives.EnableBlur(mainWindow, background))
            {
                background = Colors.Transparent;
            }
            mainWindow.PopupBackground = new SolidColorBrush(background);
            mainWindow.AccentBackground = new SolidColorBrush(accent);
            mainWindow.TextColor = new SolidColorBrush(text);
            mainWindow.TextShadow = shadow;
        }
    }
}