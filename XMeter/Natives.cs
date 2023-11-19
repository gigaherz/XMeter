using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace XMeter
{
    internal static class Natives
    {
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
                return false;

            var procId = Process.GetCurrentProcess().Id;
            GetWindowThreadProcessId(activatedHandle, out var activeProcId);

            return activeProcId == procId;
        }

        public static bool IsWindowForeground(IntPtr handle)
        {
            return GetForegroundWindow() != handle;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }
        
        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4, // RS4 1803
            ACCENT_ENABLE_HOSTBACKDROP = 5, // RS5 1809
            ACCENT_INVALID_STATE = 6
        }

        [Flags]
        internal enum AccentEdges
        {
            Left = 0x20,
            Top = 0x40,
            Right = 0x80,
            Bottom = 0x100
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWA_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3,
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long DwmSetWindowAttribute(IntPtr hwnd,
                                                    DWMWINDOWATTRIBUTE attribute,
                                                    ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                    uint cbAttribute);

        public static bool MakeEdgesRounded(MainWindow window)
        {
            var windowHelper = new WindowInteropHelper(window);
            windowHelper.EnsureHandle();

            return MakeEdgesRounded(windowHelper.Handle);
        }

        public static bool MakeEdgesRounded(IntPtr handle)
        {
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;     //change as you want
            var ret = DwmSetWindowAttribute(handle,
                                  DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                                  ref preference,
                                  sizeof(uint));
            return ret >= 0;
        }


        internal static bool EnableBlur(MainWindow window, Color background)
        {
            return EnableBlur(window, ((uint)background.A << 24) | ((uint)background.B << 16) | ((uint)background.G << 8) | ((uint)background.R));
        }

        internal static bool EnableBlur(MainWindow window, uint backgroundColor)
        {
            var windowHelper = new WindowInteropHelper(window);
            windowHelper.EnsureHandle();

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            accent.AccentFlags = (int)(AccentEdges.Left | AccentEdges.Top | AccentEdges.Right | AccentEdges.Bottom);
            accent.GradientColor = unchecked((int)backgroundColor);

            IntPtr accentPtr = IntPtr.Zero;
            try
            {
                accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = accentStructSize,
                    Data = accentPtr
                };

                int ret = SetWindowCompositionAttribute(windowHelper.Handle, ref data);
                return ret != 0;
            }
            finally
            {
                if (accentPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(accentPtr);
            }
        }
    }
}
