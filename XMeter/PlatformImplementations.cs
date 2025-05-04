using System;
using System.Reflection;
using System.Runtime.InteropServices;
using XMeter.Common;
using XMeter.Windows;

namespace XMeter
{
    public class PlatformImplementations
    {
        private static readonly string WindowsAssemblyName = "XMeter.Windows";
        private static readonly string WindowsImplementationClassName = "WindowsImplementation";

        public static IDataSource DataSource { get; private set; }

        public static INotificationIcon NotificationIcon { get; private set; }

        public static ISettings Settings { get; set; }

        private static readonly IImplementation implementation = CreateImplementation();

        private static IImplementation CreateImplementation()
        {
            if (OperatingSystem.IsWindows())
            {
                //return LoadImplementation(WindowsAssemblyName, WindowsImplementationClassName);
                return new WindowsImplementation();
            }
            else
            {
                throw new NotImplementedException("Backend implemented for platform " + RuntimeInformation.OSDescription);
            }
        }

        private static IImplementation LoadImplementation(string assemblyName, string implementationClassName)
        {
            var implementationAssembly = Assembly.Load(assemblyName);
            var implementationClass = implementationAssembly.GetType(implementationClassName) ?? throw new Exception($"Implementation {implementationClassName} not found in {assemblyName}");
            var implementation = (IImplementation)Activator.CreateInstance(implementationClass) ?? throw new Exception($"Implementation {implementationClassName} failed to construct."); ;
            return implementation;
        }

        internal static void Initialize()
        {
            DataSource = implementation.CreateDataSource();
            NotificationIcon = implementation.CreateNotificationIcon();
            Settings = implementation.CreateSettings();
        }
    }
}