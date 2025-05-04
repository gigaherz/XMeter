using H.NotifyIcon;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using XMeter.Annotations;
using XMeter.Common;
using XMeter.Util;

namespace XMeter
{
    public class SpeedViewModel : DependencyObject, INotifyPropertyChanged
    {
        public static SpeedViewModel Instance { get; } = new();

        private readonly DispatcherTimer _timer = new();

        private string _startTime;
        private string _endTime;
        private double _upSpeed;
        private double _upSpeedMax;
        private double _downSpeed;
        private double _downSpeedMax;

        public string StartTime
        {
            get => _startTime;
            set
            {
                if (value == _startTime) return;
                _startTime = value;
                OnPropertyChanged();
            }
        }

        public string EndTime
        {
            get => _endTime;
            set
            {
                if (value == _endTime) return;
                _endTime = value;
                OnPropertyChanged();
            }
        }

        public double UpSpeed
        {
            get => _upSpeed;
            set
            {
                if (value == _upSpeed) return;
                _upSpeed = value;
                OnPropertyChanged();
            }
        }

        public double UpSpeedMax
        {
            get => _upSpeedMax;
            set
            {
                if (value == _upSpeedMax) return;
                _upSpeedMax = value;
                OnPropertyChanged();
            }
        }

        public double DownSpeed
        {
            get => _downSpeed;
            set
            {
                if (value == _downSpeed) return;
                _downSpeed = value;
                OnPropertyChanged();
            }
        }

        public double DownSpeedMax
        {
            get => _downSpeedMax;
            set
            {
                if (value == _downSpeedMax) return;
                _downSpeedMax = value;
                OnPropertyChanged();
            }
        }

        public string MenuTitle { get; }

        public double GraphWidth { get; set; }

        public TaskbarIcon NotifyIcon { get; internal set; }

        public SpeedViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Select(x => x.InformationalVersion).FirstOrDefault();
            MenuTitle = $"XMeter v{version}";

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.IsEnabled = true;

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {

                PerformUpdate();
            }));
        }

        private void Timer_Tick(object o, EventArgs e)
        {
            PerformUpdate();
        }

        private void PerformUpdate()
        {
            UpdateSpeed();

            UpdateTime();

            UpdateIcon();

            OnUpdate();
        }

        private void UpdateSpeed()
        {
            DataTracker.Instance.FetchData();

            var end = DateTime.Now;

            var start = end.AddSeconds(-1);

            var (sendMax, recvMax, _, _) = DataTracker.Instance.GetMaxMinSpeedBetween(start, end);
            UpSpeed = sendMax;
            DownSpeed = recvMax;

            var (sendMaxTotal, recvMaxTotal) = DataTracker.Instance.GetMaxSpeed();
            UpSpeedMax = sendMaxTotal;
            DownSpeedMax = recvMaxTotal;

            NotifyIcon.ToolTipText = USizeConverter.FormatUSize(_upSpeed) + " ◤◢ " + USizeConverter.FormatUSize(_downSpeed);
        }

        public void UpdateTime()
        {
            var timeLast = DateTime.Now;
            var timeFirst = DataTracker.Instance.FirstTime;

            var time = (timeLast - timeFirst).TotalSeconds;
            var spanSeconds = Math.Min(GraphWidth, time);

            var currentCheck = DateTime.Now;
            StartTime = currentCheck.AddSeconds(-spanSeconds).ToString("HH:mm:ss", CultureInfo.CurrentUICulture);
            EndTime = currentCheck.ToString("HH:mm:ss", CultureInfo.CurrentUICulture);
        }

        public void UpdateIcon()
        {
            var (sendMax, recvMax, _, _) = DataTracker.Instance.GetMaxMinSpeedBetween(DateTime.Now.AddSeconds(-1), DateTime.Now);
            var sendActivity = sendMax > 0;
            var recvActivity = recvMax > 0;

            if (sendActivity && recvActivity)
            {
                NotifyIcon.Icon = Properties.Resources.U1D1;
            }
            else if (sendActivity)
            {
                NotifyIcon.Icon = Properties.Resources.U1D0;
            }
            else if (recvActivity)
            {
                NotifyIcon.Icon = Properties.Resources.U0D1;
            }
            else
            {
                NotifyIcon.Icon = Properties.Resources.U0D0;
            }
        }


        protected void OnUpdate()
        {
            Update?.Invoke();
        }

        public event Action Update;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}