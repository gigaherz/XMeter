using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using XMeter2.Annotations;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace XMeter2
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private static readonly TimeSpan ShowAnimationDelay = TimeSpan.FromMilliseconds(250);
        private static readonly TimeSpan ShowAnimationDuration = TimeSpan.FromMilliseconds(200);
        private readonly DoubleAnimation _showOpacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = ShowAnimationDuration,
            DecelerationRatio = 1
        };
        private readonly DoubleAnimation _showTopAnimation = new DoubleAnimation
        {
            Duration = ShowAnimationDuration,
            DecelerationRatio = 1
        };

        private ulong _upSpeed;
        private ulong _downSpeed;
        private ulong _downSpeedMax;
        private ulong _upSpeedMax;
        private DateTime _startTime = DateTime.Now.AddSeconds(-1);
        private DateTime _endTime = DateTime.Now;
        private Brush _popupBackground;
        private Brush _popupBorder;
        private bool _isPopupOpen;
        private Brush _popupPanel;
        private bool _opening;
        private bool _shown;
        private Icon _icon;

        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (value == _startTime) return;
                _startTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                if (value == _endTime) return;
                _endTime = value;
                OnPropertyChanged();
            }
        }

        public ulong UpSpeed
        {
            get => _upSpeed;
            set
            {
                if (value == _upSpeed) return;
                _upSpeed = value;
                OnPropertyChanged();
            }
        }

        public ulong UpSpeedMax
        {
            get => _upSpeedMax;
            set
            {
                if (value == _upSpeedMax) return;
                _upSpeedMax = value;
                OnPropertyChanged();
            }
        }

        public ulong DownSpeed
        {
            get => _downSpeed;
            set
            {
                if (value == _downSpeed) return;
                _downSpeed = value;
                OnPropertyChanged();
            }
        }

        public ulong DownSpeedMax
        {
            get => _downSpeedMax;
            set
            {
                if (value == _downSpeedMax) return;
                _downSpeedMax = value;
                OnPropertyChanged();
            }
        }

        public Icon TrayIcon
        {
            get => _icon;
            set
            {
                if (ReferenceEquals(_icon, value)) return;
                _icon = value;
                NotificationIcon.Icon = value;
                OnPropertyChanged();
            }
        }

        public Brush PopupBackground
        {
            get => _popupBackground;
            private set
            {
                if (Equals(value, _popupBackground)) return;
                _popupBackground = value;
                OnPropertyChanged();
            }
        }

        public Brush PopupBorder
        {
            get => _popupBorder;
            set
            {
                if (Equals(value, _popupBorder)) return;
                _popupBorder = value;
                OnPropertyChanged();
            }
        }

        public Brush PopupPanel
        {
            get => _popupPanel;
            set
            {
                if (Equals(value, _popupPanel)) return;
                _popupPanel = value;
                OnPropertyChanged();
            }
        }

        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (value == _isPopupOpen) return;
                _isPopupOpen = value;
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

            SystemEvents.UserPreferenceChanging += SystemEvents_UserPreferenceChanging;

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {

                UpdateAccentColor();

                Natives.EnableBlur(this);

                PerformUpdate();
                Hide();
            }));
        }

        private void UpdateAccentColor()
        {
            File.WriteAllLines(@"F:\Accents.txt", AccentColorSet.ActiveSet.GetAllColorNames().Select(s => {
                var c = AccentColorSet.ActiveSet[s];
                return $"{s}: {c}";
            }));
            var c1 = AccentColorSet.ActiveSet["SystemAccent"];
            var c2 = AccentColorSet.ActiveSet["SystemAccentDark2"];
            var c3 = Color.FromArgb(160,255,255,255); //AccentColorSet.ActiveSet["SystemTextDarkTheme"];
            c2.A = 192;
            PopupBackground = new SolidColorBrush(c2);
            PopupBorder = new SolidColorBrush(c1);
            PopupPanel = new SolidColorBrush(c3);
        }

        private void SystemEvents_UserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            UpdateAccentColor();
        }

        private void NotificationIcon_OnMouseLeftButtonUp(object sender, RoutedEventArgs routedEventArgs)
        {
            Popup();
        }

        private void Popup()
        {
            UpdateGraphUI();
            _opening = true;
            DelayInvoke(250, () => {
                _opening = false;
                UpdateGraphUI();
            });

            BeginAnimation(OpacityProperty, null);
            BeginAnimation(TopProperty, null);
            Left = SystemParameters.WorkArea.Width - Width;
            Top = SystemParameters.WorkArea.Height;
            Opacity = 0;

            _shown = true;
            Dispatcher.BeginInvoke(new Action(Show));
        }

        private void MainWindow_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible || !_shown)
            {
                Opacity = 0;
                return;
            }

            _shown = false;
            Dispatcher.BeginInvoke(new Action(() => Activate()));

            _showTopAnimation.From = SystemParameters.WorkArea.Height;
            _showTopAnimation.To = SystemParameters.WorkArea.Height - Height;

            DelayInvoke(ShowAnimationDelay, () => {
                BeginAnimation(OpacityProperty, _showOpacityAnimation);
                BeginAnimation(TopProperty, _showTopAnimation);
            });
        }

        private void DelayInvoke(uint ms, Action callback)
        {
            DelayInvoke(TimeSpan.FromMilliseconds(ms), callback);
        }

        private void DelayInvoke(TimeSpan time, Action callback)
        {
            if (time.TotalSeconds < float.Epsilon)
            {
                Dispatcher.BeginInvoke(callback);
                return;
            }

            var timer = new DispatcherTimer
            {
                Interval = time
            };
            timer.Tick += (_, __) =>
            {
                timer.Stop();
                Dispatcher.Invoke(callback);
            };
            timer.Start();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
            Opacity = 0;
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
            PerformUpdate();
        }

        private void PerformUpdate()
        {
            DataTracker.FetchData();

            var (sendSpeed, recvSpeed) = DataTracker.CurrentSpeed;
            UpSpeed = sendSpeed;
            DownSpeed = recvSpeed;

            var (sendMax, recvMax) = DataTracker.MaxSpeed;
            UpSpeedMax = sendMax;
            DownSpeedMax = recvMax;

            UpdateIcon();

            if (!IsVisible || _opening)
                return;

            var (sendTimeLast, recvTimeLast) = DataTracker.CurrentTime;
            var (sendTimeFirst, recvTimeFirst) = DataTracker.FirstTime;

            var upTime =   (sendTimeLast - sendTimeFirst).TotalSeconds;
            var downTime = (recvTimeLast - recvTimeFirst).TotalSeconds;
            var spanSeconds = Math.Max(upTime, downTime);

            var currentCheck = DateTime.Now;
            StartTime = currentCheck.AddSeconds(-spanSeconds);
            EndTime = currentCheck;

            UpdateGraphUI();
        }

        private void UpdateIcon()
        {
            var (sendSpeed, recvSpeed) = DataTracker.CurrentSpeed;
            var sendActivity = sendSpeed > 0;
            var recvActivity = recvSpeed > 0;

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

        private void UpdateGraphUI()
        {
            Graph.Children.Clear();

            (var sendSpeedMax, var recvSpeedMax) = DataTracker.MaxSpeed;

            var sqUp = Math.Max(32, Math.Sqrt(sendSpeedMax));
            var sqDown = Math.Max(32, Math.Sqrt(recvSpeedMax));
            var max2 = sqDown + sqUp;
            var maxUp = max2 * sendSpeedMax / sqUp;
            var maxDown = max2 * recvSpeedMax / sqDown;

            BuildPolygon(DataTracker.SendPoints, (ulong) maxUp, 255, 24, 32, true);
            BuildPolygon(DataTracker.RecvPoints, (ulong) maxDown, 48, 48, 255,  false);

            var yy = sqDown * Graph.ActualHeight / max2;
            var line = new Line
            {
                X1 = 0,
                X2 = Graph.ActualWidth,
                Y1 = yy,
                Y2 = yy,
                Stroke = Brushes.White,
                Opacity = .6,
                StrokeDashArray = new DoubleCollection(new[] {1.0, 2.0}),
                StrokeDashCap = PenLineCap.Flat
            };
            Graph.Children.Add(line);

            GraphDown.Margin = new Thickness(0, 0, 0, Graph.ActualHeight - yy);
            GraphUp.Margin = new Thickness(0, yy, 0, 0);
        }

        private void BuildPolygon(LinkedList<(DateTime TimeStamp, ulong Bytes)> points, ulong max, byte r, byte g, byte b, bool up)
        {
            if (points.Count == 0)
                return;

            var bottom = Graph.ActualHeight;
            var right = Graph.ActualWidth;

            var lastTime = points.Last.Value.TimeStamp;

            var elapsed = (lastTime - points.First.Value.TimeStamp).TotalSeconds;

            var scale = 1.0;
            if (elapsed > 0 && elapsed < Graph.ActualWidth)
                scale = Graph.ActualWidth / elapsed;

            var polygon = new Polygon();
            for (var current = points.Last; current != null; current = current.Previous)
            {
                var td = (lastTime - current.Value.TimeStamp).TotalSeconds;

                var xx = right - td * scale;
                var yy = current.Value.Bytes * Graph.ActualHeight / max;

                polygon.Points.Add(new Point(xx, up ? bottom - yy : yy));
            }

            polygon.Points.Add(new Point(right, up ? bottom : 0));

            polygon.Fill = new SolidColorBrush(Color.FromArgb(160, r, g, b));
            Graph.Children.Add(polygon);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
