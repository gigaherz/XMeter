using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using XMeter.Annotations;
using Icon = System.Drawing.Icon;

namespace XMeter
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private static readonly TimeSpan ShowAnimationDelay = TimeSpan.FromMilliseconds(150);
        private static readonly TimeSpan ShowAnimationDuration = TimeSpan.FromMilliseconds(50);
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

        private double _upSpeed;
        private double _downSpeed;
        private double _downSpeedMax;
        private double _upSpeedMax;
        private string _startTime;
        private string _endTime;
        private Brush _popupBackground;
        private Brush _accentBackground;
        private Color _popupBackgroundColor;
        private bool _isPopupOpen;
        private bool _opening;
        private bool _shown;
        private Icon _icon;
        private Brush _mainText;
        private bool _separateFromTaskbar;

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
                OnPropertyChanged(nameof(UpDownToolTip));
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
                OnPropertyChanged(nameof(UpDownToolTip));
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

        public string UpDownToolTip => USizeConverter.FormatUSize(_upSpeed) + " ◤◢ " + USizeConverter.FormatUSize(_downSpeed);

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

        public Brush AccentBackground
        {
            get => _accentBackground;
            private set
            {
                if (Equals(value, _accentBackground)) return;
                _accentBackground = value;
                OnPropertyChanged();
            }
        }

        public Color TextShadow
        {
            get => _popupBackgroundColor;
            private set
            {
                if (Equals(value, _popupBackgroundColor)) return;
                _popupBackgroundColor = value;
                OnPropertyChanged();
            }
        }

        public Brush TextColor
        {
            get => _mainText;
            set
            {
                if (Equals(value, _mainText)) return;
                _mainText = value;
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

        public string MenuTitle { get; }

        public MainWindow()
        {
            Visibility = Visibility.Hidden;

            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Select(x => x.InformationalVersion).FirstOrDefault();
            MenuTitle = $"XMeter v{version}";

            InitializeComponent();


            SettingsManager.ReadSettings();

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.IsEnabled = true;

            SystemEvents.UserPreferenceChanging += SystemEvents_UserPreferenceChanging;

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {

                UpdateAccentColor();

                PerformUpdate();
                Hide();
            }));
        }

        private void UpdateAccentColor()
        {
#if false
            File.WriteAllLines(@"F:\Accents.txt", AccentColorSet.ActiveSet.GetAllColorNames().Select(s => {
                var c = AccentColorSet.ActiveSet[s];
                return $"{s}: {c}";
            }));
#endif
            var background = AccentColorSet.ActiveSet["SystemBackground"];
            var backgroundDark = AccentColorSet.ActiveSet["SystemBackgroundDarkTheme"];
            var shadow = background;
            var text = AccentColorSet.ActiveSet["SystemText"];
            var accent = AccentColorSet.ActiveSet["SystemAccentLight3"];
            //if (background != backgroundDark)
            //{
            //    accent = AccentColorSet.ActiveSet["SystemAccentDark3"];
            //}
            accent.A = 128;
            background.A = 160;

            if (Natives.MakeEdgesRounded(this))
            {
                _separateFromTaskbar = true;
            }

            if (Natives.EnableBlur(this, background))
            {
                background = Colors.Transparent;
            }
            PopupBackground = new SolidColorBrush(background);
            AccentBackground = new SolidColorBrush(accent);
            TextColor = new SolidColorBrush(text);
            TextShadow = shadow;
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
            Left = SystemParameters.WorkArea.Width - Width - (_separateFromTaskbar ? 12 : 0);
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
            _showTopAnimation.To = SystemParameters.WorkArea.Height - Height - (_separateFromTaskbar ? 12 : 0);

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

            UpdateTime();
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
            DataTracker.Instance.FetchData();

            var (sendSpeed, recvSpeed) = DataTracker.Instance.GetMaxSpeedBetween(DateTime.Now.AddSeconds(-1), DateTime.Now);
            UpSpeed = sendSpeed;
            DownSpeed = recvSpeed;

            var (sendMax, recvMax) = DataTracker.Instance.GetMaxSpeed();
            UpSpeedMax = sendMax;
            DownSpeedMax = recvMax;

            UpdateIcon();

            if (!IsVisible || _opening)
                return;

            UpdateTime();

            UpdateGraphUI();
        }

        private void UpdateTime()
        {
            var timeLast = DateTime.Now;
            var timeFirst = DataTracker.Instance.FirstTime;

            var time = (timeLast - timeFirst).TotalSeconds;
            var spanSeconds = Math.Min(Graph.ActualWidth, time);

            var currentCheck = DateTime.Now;
            StartTime = currentCheck.AddSeconds(-spanSeconds).ToString("HH:mm:ss", CultureInfo.CurrentUICulture);
            EndTime = currentCheck.ToString("HH:mm:ss", CultureInfo.CurrentUICulture);
        }

        private void UpdateIcon()
        {
            var (sendSpeed, recvSpeed) = DataTracker.Instance.GetMaxSpeedBetween(DateTime.Now.AddSeconds(-1), DateTime.Now);
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

            var data = DataTracker.Instance;

            int intervals = (int)(Graph.ActualWidth / 5);
            double timePerInterval = 2;

            var time1 = data.FirstTime;
            var time2 = data.LastTime;

            if ((time2-time1).TotalSeconds > (intervals * timePerInterval))
            {
                time1 = time2.AddSeconds(-intervals * timePerInterval);
            }

            double yy = 0.5 * Graph.ActualHeight;

            var (sendSpeedMax, recvSpeedMax) = data.GetMaxSpeedBetween(time1, time2);
            if (sendSpeedMax > 0 || recvSpeedMax > 0)
            {
                var sqUp = Math.Max(32, Math.Sqrt(sendSpeedMax));
                var sqDown = Math.Max(32, Math.Sqrt(recvSpeedMax));
                var max2 = sqDown + sqUp;
                var maxUp = max2 * sendSpeedMax / sqUp;
                var maxDown = max2 * recvSpeedMax / sqDown;
                yy = sqDown * Graph.ActualHeight / max2;

                BuildPolygon(data, time1, time2, intervals, maxUp, 255, 24, 32, true, p => p.Item1);
                BuildPolygon(data, time1, time2, intervals, maxDown, 48, 48, 255, false, p => p.Item2);
            }

            var line = new Line
            {
                X1 = 0,
                X2 = Graph.ActualWidth,
                Y1 = yy,
                Y2 = yy,
                Stroke = TextColor,
                Opacity = .6,
                StrokeDashArray = new DoubleCollection(new[] {1.0, 2.0}),
                StrokeDashCap = PenLineCap.Flat
            };
            Graph.Children.Add(line);

            GraphDown.Margin = new Thickness(0, 0, 0, Graph.ActualHeight - yy);
            GraphUp.Margin = new Thickness(0, yy, 0, 0);
        }

        private void BuildPolygon(DataTracker data, DateTime time1, DateTime time2, int intervals, double max, byte r, byte g, byte b, bool up,
            Func<(double,double), double> fieldGetter)
        {
            if (intervals == 0 || time1 >= time2)
                return;

            var bottom = Graph.ActualHeight;
            var right = Graph.ActualWidth;

            var polygon = new Polyline();
            polygon.Points.Add(new Point(0, up ? bottom : 0));
            for (int i=0;i<intervals;i++)
            {
                double dt1 = i / (double)intervals;
                var t1 = Lerp(time1, time2, dt1);

                double dt2 = (i+1) / (double)intervals;
                var t2 = Lerp(time1, time2, dt2);

                var x1 = dt1 * right;
                var x2 = dt2 * right;

                var values = data.GetMaxSpeedBetween(t1, t2);
                var speed = fieldGetter(values);
                var yy = speed * bottom / max;

                polygon.Points.Add(new Point(x1, up ? bottom - yy : yy));
                polygon.Points.Add(new Point(x2, up ? bottom - yy : yy));
            }
            polygon.Points.Add(new Point(right, up ? bottom : 0));

            polygon.Stroke = new SolidColorBrush(Color.FromArgb(160, r, g, b));
            polygon.StrokeThickness = 2;
            polygon.Fill = new SolidColorBrush(Color.FromArgb(64, r, g, b));
            Graph.Children.Add(polygon);
        }

        private DateTime Lerp(DateTime time1, DateTime time2, double dt)
        {
            return time1.AddSeconds(dt * (time2 - time1).TotalSeconds);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
