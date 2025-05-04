using System.ComponentModel;

namespace XMeter.Common
{
    public interface ISettings : INotifyPropertyChanged
    {
        public static readonly int DefaultPreferredWidth = 384;
        public static readonly int DefaultPreferredHeight = 240;

        public int Width { get; set; }
        public int Height { get; set; }

        public void ReadSettings();

        public void WriteSettings();
    }
}
