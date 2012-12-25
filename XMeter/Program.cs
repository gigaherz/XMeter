using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace XMeter
{
    static class Program
    {
        public static readonly string AuthorEmail = "gigaherz@gmail.com";
        public static readonly string ProgramUrl = "http://gigaherz.github.com/XNetMeter/#download";
        public static readonly string UpdateUrl = "https://github.com/downloads/gigaherz/XNetMeter/version.txt";

        public static readonly RegistryKey BaseKey = Registry.CurrentUser;
        public static readonly string AppKey = "SOFTWARE\\XNetMeter";
        public static readonly string RunKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new XMeterDisplay());
        }
    }
}
