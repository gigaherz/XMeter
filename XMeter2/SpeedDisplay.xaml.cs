using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XMeter2.Annotations;

namespace XMeter2
{
    /// <summary>
    /// Interaction logic for SpeedDisplay.xaml
    /// </summary>
    public partial class SpeedDisplay : INotifyPropertyChanged
    {
        public static readonly DependencyProperty UpSpeedProperty =
            DependencyProperty.Register("UpSpeed", typeof(string), typeof(SpeedDisplay));

        public static readonly DependencyProperty DownSpeedProperty =
            DependencyProperty.Register("DownSpeed", typeof(string), typeof(SpeedDisplay));


        public string UpSpeed
        {
            get => GetValue(UpSpeedProperty) as string;
            set
            {
                SetValue(UpSpeedProperty, value);
                OnPropertyChanged();
            }
        }

        public string DownSpeed
        {
            get => GetValue(DownSpeedProperty) as string;
            set
            {
                SetValue(DownSpeedProperty, value);
                OnPropertyChanged();
            }
        }


        public SpeedDisplay()
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
