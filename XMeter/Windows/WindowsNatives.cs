using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media;

namespace XMeter.Windows
{
    [SupportedOSPlatform("windows")]
    internal static partial class WindowsNatives
    {
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == nint.Zero)
                return false;

            var procId = Environment.ProcessId;
            var result = GetWindowThreadProcessId(activatedHandle, out var activeProcId);

            if (result == nint.Zero) return false;

            return activeProcId == procId;
        }

        public static bool IsWindowForeground(nint handle)
        {
            return GetForegroundWindow() != handle;
        }

        [LibraryImport("user32.dll")]
        private static partial nint GetForegroundWindow();

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial int GetWindowThreadProcessId(nint handle, out int processId);

        [LibraryImport("user32.dll")]
        internal static partial int SetWindowCompositionAttribute(nint hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public nint Data;
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

        [LibraryImport("dwmapi.dll", SetLastError = true)]
        private static partial long DwmSetWindowAttribute(nint hwnd,
                                                    DWMWINDOWATTRIBUTE attribute,
                                                    ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                    uint cbAttribute);

        public static bool MakeEdgesRounded(MainWindow window)
        {
            var windowHelper = new WindowInteropHelper(window);
            windowHelper.EnsureHandle();

            return MakeEdgesRounded(windowHelper.Handle);
        }

        public static bool MakeEdgesRounded(nint handle)
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
            return EnableBlur(window, (uint)background.A << 24 | (uint)background.B << 16 | (uint)background.G << 8 | background.R);
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

            nint accentPtr = nint.Zero;
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
                if (accentPtr != nint.Zero)
                    Marshal.FreeHGlobal(accentPtr);
            }
        }

        #region uxtheme
        [LibraryImport("uxtheme.dll", EntryPoint = "#98")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        public static partial uint GetImmersiveUserColorSetPreference([MarshalAs(UnmanagedType.Bool)] bool forceCheckRegistry, [MarshalAs(UnmanagedType.Bool)] bool skipCheckOnFail);

        [LibraryImport("uxtheme.dll", EntryPoint = "#94")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        public static partial uint GetImmersiveColorSetCount();

        [LibraryImport("uxtheme.dll", EntryPoint = "#95")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        public static partial uint GetImmersiveColorFromColorSetEx(uint immersiveColorSet, uint immersiveColorType,
            [MarshalAs(UnmanagedType.Bool)] bool ignoreHighContrast, uint highContrastCacheMode);

        [LibraryImport("uxtheme.dll", EntryPoint = "#96")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        public static partial uint GetImmersiveColorTypeFromName(nint name);

        [LibraryImport("uxtheme.dll", EntryPoint = "#100")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
        public static partial nint GetImmersiveColorNamedTypeByIndex(uint index);
        #endregion
    }
}
