using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using XMeter.Annotations;
using XMeter.Common;
using XMeter.Windows;

namespace XMeter
{
    public partial class MainWindow : INotifyPropertyChanged
    {
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

        private Brush _popupBackground;
        private Brush _accentBackground;
        private Color _popupBackgroundColor;
        private bool _isPopupOpen;
        private bool _opening;
        private bool _shown;
        private Brush _mainText;
        private bool _separateFromTaskbar;

        public Brush PopupBackground
        {
            get => _popupBackground;
            set
            {
                if (Equals(value, _popupBackground)) return;
                _popupBackground = value;
                OnPropertyChanged();
            }
        }

        public Brush AccentBackground
        {
            get => _accentBackground;
            set
            {
                if (Equals(value, _accentBackground)) return;
                _accentBackground = value;
                OnPropertyChanged();
            }
        }

        public Color TextShadow
        {
            get => _popupBackgroundColor;
            set
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

        public bool SeparateFromTaskbar
        {
            get => _separateFromTaskbar;
            set => _separateFromTaskbar = value;
        }
        public SpeedViewModel Model => (Application.Current as App).Model;

        public MainWindow()
        {
            Visibility = Visibility.Hidden;

            InitializeComponent();

            SettingsManager.Settings.ReadSettings();

            if (OperatingSystem.IsWindows())
                WindowsAccentColors.SetupAccentsUpdate(this);

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => {

                if (OperatingSystem.IsWindows())
                    WindowsAccentColors.UpdateAccentColor(this);

                Hide();
            }));


            (Application.Current as App).Model.Update += () =>
            {
                if (!IsVisible || _opening)
                    return;

                UpdateGraphUI();
            };
        }

        public void Popup()
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
            SettingsManager.Settings.WriteSettings();

            (Application.Current as App).Model.UpdateTime();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UpdateGraphUI()
        {
            Graph.Children.Clear();

            var data = DataTracker.Instance;

            double timePerInterval = 0.5;
            double stepWidth = 2;
            int intervals = (int)(Graph.ActualWidth / stepWidth);

            var time1 = data.FirstTime;

            var time2 = data.LastTime;
            //time2 = time2.Date.AddSeconds(Math.Floor(time2.TimeOfDay.TotalSeconds/timePerInterval)*timePerInterval);

            if (time1 > time2)
            {
                return;
            }

            if ((time2 - time1).TotalSeconds > (intervals * timePerInterval))
            {
                time1 = time2.AddSeconds(-intervals * timePerInterval);
            }
            else
            {
                intervals = (int)Math.Floor((time2 - time1).TotalSeconds / timePerInterval);
            }

            double yy = 0.5 * Graph.ActualHeight;

            var (sendSpeedMax, recvSpeedMax, _, _) 
                = data.GetMaxMinSpeedBetween(time1, time2);
            if (sendSpeedMax > 0 || recvSpeedMax > 0)
            {
                var sqUp = Math.Max(32, Math.Sqrt(sendSpeedMax));
                var sqDown = Math.Max(32, Math.Sqrt(recvSpeedMax));
                var max2 = sqDown + sqUp;
                var maxUp = max2 * sendSpeedMax / sqUp;
                var maxDown = max2 * recvSpeedMax / sqDown;
                yy = sqDown * Graph.ActualHeight / max2;

                BuildPolygon(data, time1, time2, intervals, maxUp, 255, 24, 32, true, p => (p.Item1, p.Item3));
                BuildPolygon(data, time1, time2, intervals, maxDown, 48, 48, 255, false, p => (p.Item2, p.Item4));
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


            (Application.Current as App).Model.GraphWidth = (time2 - time1).TotalSeconds;
        }

        private void BuildPolygon(DataTracker data, DateTime time1, DateTime time2, int intervals, double max, byte r, byte g, byte b, bool up,
            Func<(double,double,double,double), (double,double)> fieldGetter)
        {
            if (intervals == 0 || time1 >= time2)
                return;

            var bottom = Graph.ActualHeight;
            var right = Graph.ActualWidth;

            var polygonMax = new Polyline();
            var polygonMin = new Polyline();
            polygonMax.Points.Add(new Point(0, up ? bottom : 0));
            polygonMin.Points.Add(new Point(0, up ? bottom : 0));
            for (int i=0;i<intervals;i++)
            {
                double dt1 = i / (double)intervals;
                var t1 = Lerp(time1, time2, dt1);

                double dt2 = (i+1) / (double)intervals;
                var t2 = Lerp(time1, time2, dt2);

                var x1 = dt1 * right;
                var x2 = dt2 * right;

                var values = data.GetMaxMinSpeedBetween(t1, t2);
                var speeds = fieldGetter(values);
                var maxSpeed = speeds.Item1;
                var minSpeed = speeds.Item2;
                var yyMax = maxSpeed * bottom / max;
                var yyMin = minSpeed * bottom / max;

                polygonMax.Points.Add(new Point(x1, up ? bottom - yyMax : yyMax));
                polygonMax.Points.Add(new Point(x2, up ? bottom - yyMax : yyMax));

                polygonMin.Points.Add(new Point(x1, up ? bottom - yyMin : yyMin));
                polygonMin.Points.Add(new Point(x2, up ? bottom - yyMin : yyMin));
            }
            polygonMax.Points.Add(new Point(right, up ? bottom : 0));
            polygonMin.Points.Add(new Point(right, up ? bottom : 0));

            polygonMax.Stroke = new SolidColorBrush(Color.FromArgb(160, r, g, b));
            polygonMax.StrokeThickness = 2;
            polygonMax.Fill = new SolidColorBrush(Color.FromArgb(64, r, g, b));
            Graph.Children.Add(polygonMax);

            polygonMin.Stroke = new SolidColorBrush(Color.FromArgb(224, r, g, b));
            polygonMin.StrokeThickness = 2;
            polygonMin.Fill = new SolidColorBrush(Color.FromArgb(160, r, g, b));
            Graph.Children.Add(polygonMin);
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
