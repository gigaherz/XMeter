using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XMeter2
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


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}
