using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using XMeter2.Annotations;

namespace XMeter2
{
    public partial class Tooltip : INotifyPropertyChanged
    {
        public static readonly DependencyProperty UpLabelProperty =
                  DependencyProperty.Register("UpLabel", typeof(string), typeof(Tooltip));

        public static readonly DependencyProperty DownLabelProperty =
                  DependencyProperty.Register("DownLabel", typeof(string), typeof(Tooltip));

        public string UpLabel
        {
            get { return GetValue(UpLabelProperty) as string; }
            set
            {
                SetValue(UpLabelProperty, value);
                OnPropertyChanged();
            }
        }

        public string DownLabel
        {
            get { return GetValue(DownLabelProperty) as string; }
            set
            {
                SetValue(DownLabelProperty, value);
                OnPropertyChanged();
            }
        }

        public Tooltip()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
