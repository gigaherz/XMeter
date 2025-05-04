using XMeter.Common;

namespace XMeter.Windows
{
    public class WindowsImplementation : IImplementation
    {
        public IDataSource CreateDataSource()
        {
            return WMIDataSource.Create();
        }

        public INotificationIcon CreateNotificationIcon()
        {
            return WindowsTaskbarIcon.Create();
        }

        public ISettings CreateSettings()
        {
            return WindowsRegistrySettings.Create();
        }
    }
}
