using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace XMeter2
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private readonly LinkedList<TimeEntry> _upPoints = new LinkedList<TimeEntry>();
        private readonly LinkedList<TimeEntry> _downPoints = new LinkedList<TimeEntry>();

        private ulong _lastMaxUp;
        private ulong _lastMaxDown;
        private Icon _icon;

        private string _upSpeed = "0 B/s";
        private string _downSpeed = "0 B/s";
        private string _toolTipText = "Initializing...";
        private string _startTime = DateTime.Now.AddSeconds(-1).ToString("HH:mm:ss");
        private string _endTime = DateTime.Now.ToString("HH:mm:ss");

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

        public string UpSpeed
        {
            get => _upSpeed;
            set
            {
                if (value == _upSpeed) return;
                _upSpeed = value;
                OnPropertyChanged();
            }
        }

        public string DownSpeed
        {
            get => _downSpeed;
            set
            {
                if (value == _downSpeed) return;
                _downSpeed = value;
                OnPropertyChanged();
            }
        }

        public string ToolTipText
        {
            get => _toolTipText;
            private set
            {
                if (value == _toolTipText) return;
                _toolTipText = value;
                OnPropertyChanged();
            }
        }

        public Icon TrayIcon
        {
            get => _icon;
            set {
                if (ReferenceEquals(_icon, value)) return;
                _icon = value;
                NotificationIcon.Icon = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            
            SettingsManager.ReadSettings();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.IsEnabled = true;

            UpdateIcon(false, false);

            UpdateSpeeds();

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(Hide));
        }

        bool preventReshow = true;
        private void NotificationIcon_OnMouseLeftButtonDown(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("DOWN!");
            if (IsVisible)
                preventReshow = true;
        }

        private void NotificationIcon_OnMouseLeftButtonUp(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("UP!");
            if (preventReshow)
            {
                preventReshow = false;
                return;
            }
            Left = SystemParameters.WorkArea.Width - Width - 8;
            Top = SystemParameters.WorkArea.Height - Height - 8;
            Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("CLOSING!");
            e.Cancel = true;
            Hide();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Debug.WriteLine("DEACTIVATED!");
            Hide();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SettingsManager.WriteSettings();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private void Timer_Tick(object o, EventArgs e)
        {
            if (IsVisible && !Natives.ApplicationIsActivated())
            {
                Hide();
                return;
            }

            UpdateSpeeds();

            var sendActivity = _upPoints.Last.Value.Bytes > 0;
            var recvActivity = _downPoints.Last.Value.Bytes > 0;
            UpdateIcon(sendActivity, recvActivity);

            UpSpeed = Util.FormatUSize(_upPoints.Last.Value.Bytes);
            DownSpeed = Util.FormatUSize(_downPoints.Last.Value.Bytes);

            ToolTipText = $"Send: {Util.FormatUSize(_upPoints.Last.Value.Bytes)}; Receive: {Util.FormatUSize(_downPoints.Last.Value.Bytes)}";

            if (IsVisible)
            {
                var upTime = (_upPoints.Last.Value.TimeStamp - _upPoints.First.Value.TimeStamp).TotalSeconds;
                var downTime = (_downPoints.Last.Value.TimeStamp - _downPoints.First.Value.TimeStamp).TotalSeconds;
                var spanSeconds = Math.Max(upTime,downTime);

                var currentCheck = DateTime.Now;
                StartTime = currentCheck.AddSeconds(-spanSeconds).ToString("HH:mm:ss");
                EndTime = currentCheck.ToString("HH:mm:ss");

                UpdateGraph2();
            }
        }

        private void UpdateIcon(bool sendActivity, bool recvActivity)
        {
            if (sendActivity && recvActivity)
            {
                TrayIcon = Properties.Resources.U1D1;
            }
            else if (sendActivity)
            {
                TrayIcon = Properties.Resources.U1D0;
            }
            else if (recvActivity)
            {
                TrayIcon = Properties.Resources.U0D1;
            }
            else
            {
                TrayIcon = Properties.Resources.U0D0;
            }
        }

        private void UpdateSpeeds()
        {
            var maxStamp = NetTracker.UpdateNetwork(out ulong bytesReceivedPerSec, out ulong bytesSentPerSec);

            AddData(_upPoints, maxStamp, bytesSentPerSec);
            AddData(_downPoints, maxStamp, bytesReceivedPerSec);

            _lastMaxDown = _downPoints.Select(s => s.Bytes).Max();
            _lastMaxUp = _upPoints.Select(s => s.Bytes).Max();
        }

        private static void AddData(LinkedList<TimeEntry> points, DateTime maxStamp, ulong bytesSentPerSec)
        {
            points.AddLast(new TimeEntry(maxStamp, bytesSentPerSec));

            var totalSpan = points.Last.Value.TimeStamp - points.First.Value.TimeStamp;
            while (totalSpan.TotalSeconds > NetTracker.MaxSecondSpan && points.Count > 1)
            {
                points.RemoveFirst();
                totalSpan = points.Last.Value.TimeStamp - points.First.Value.TimeStamp;
            }
        }

        private void UpdateGraph2()
        {
            picGraph.Children.Clear();

            var max = Math.Max(_lastMaxDown, _lastMaxUp);

            BuildPolygon(_upPoints, max, 255, 24, 32, true);
            BuildPolygon(_downPoints, max, 48, 48, 255,  false);
        }

        private void BuildPolygon(LinkedList<TimeEntry> points, ulong max, byte r, byte g, byte b, bool up)
        {
            if (points.Count == 0)
                return;

            var bottom = picGraph.ActualHeight;
            var right = picGraph.ActualWidth;

            var lastTime = points.Last.Value.TimeStamp;

            var elapsed = (lastTime - points.First.Value.TimeStamp).TotalSeconds;

            var scale = 1.0;
            if (elapsed > 0 && elapsed < picGraph.ActualWidth)
                scale = picGraph.ActualWidth / elapsed;

            var polygon = new Polygon();
            for (var current = points.Last; current != null; current = current.Previous)
            {
                var td = (lastTime - current.Value.TimeStamp).TotalSeconds;

                var xx = right - td * scale;
                var yy = current.Value.Bytes * picGraph.ActualHeight / max;

                polygon.Points.Add(new Point(xx, up ? bottom - yy : yy));
            }

            polygon.Points.Add(new Point(right, up ? bottom : 0));

            polygon.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));
            picGraph.Children.Add(polygon);
        }

        private class TimeEntry
        {
            public readonly DateTime TimeStamp;
            public readonly ulong Bytes;

            public TimeEntry(DateTime t, ulong b)
            {
                TimeStamp = t;
                Bytes = b;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
