using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace XMeter2
{
    public partial class MainWindow : Window
    {
        TaskbarIcon notificationIcon = new TaskbarIcon();

        class TimeEntry
        {
            public readonly DateTime TimeStamp;
            public readonly ulong Bytes;

            public TimeEntry(DateTime t, ulong b)
            {
                TimeStamp = t;
                Bytes = b;
            }
        }

        readonly LinkedList<TimeEntry> UpPoints = new LinkedList<TimeEntry>();
        readonly LinkedList<TimeEntry> DownPoints = new LinkedList<TimeEntry>();

        ulong lastMaxUp;
        ulong lastMaxDown;

        DateTime lastCheck;
        
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            
            SettingsManager.ReadSettings();

            lbStartTime.Content = DateTime.Now.AddSeconds(-1).ToString("HH:mm:ss");
            lbEndTime.Content = DateTime.Now.ToString("HH:mm:ss");

            notificationIcon.Icon = Properties.Resources.U0D0;
            notificationIcon.ToolTipText = "Initializing...";
            notificationIcon.ContextMenu = ContextMenu;
            notificationIcon.LeftClickCommand = new RelayCommand(NotificationIcon_LeftClick);
            
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;
            
            Visibility = Visibility.Hidden;

            UpdateSpeeds();
        }

        private void NotificationIcon_LeftClick(object obj)
        {
            Visibility = Visibility.Visible;
            Left = SystemParameters.WorkArea.Width - Width - 8;
            Top = SystemParameters.WorkArea.Height - Height - 8;

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => Activate()));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private void Timer_Tick(object o, EventArgs e)
        {
            DateTime currentCheck = DateTime.Now;
            
            UpdateSpeeds();

            double spanSeconds =Math.Max(
                (UpPoints.Last.Value.TimeStamp - UpPoints.First.Value.TimeStamp).TotalSeconds,
                (DownPoints.Last.Value.TimeStamp - DownPoints.First.Value.TimeStamp).TotalSeconds);

            string up = Util.FormatUSize(UpPoints.Last.Value.Bytes);
            string down = Util.FormatUSize(DownPoints.Last.Value.Bytes);
            
            lbStartTime.Content = currentCheck.AddSeconds(-spanSeconds).ToString("HH:mm:ss");
            lbEndTime.Content = currentCheck.ToString("HH:mm:ss");
            lbUpSpeed.Text = up;
            lbDownSpeed.Text = down;

            notificationIcon.ToolTipText = string.Format("Send: {0}; Receive: {1}", up, down);

            bool sendActivity = (UpPoints.Last.Value.Bytes > 0);
            bool recvActivity = (DownPoints.Last.Value.Bytes > 0);

            if (sendActivity && recvActivity)
            {
                notificationIcon.Icon = Properties.Resources.U1D1;
            }
            else if (sendActivity)
            {
                notificationIcon.Icon = Properties.Resources.U1D0;
            }
            else if (recvActivity)
            {
                notificationIcon.Icon = Properties.Resources.U0D1;
            }
            else
            {
                notificationIcon.Icon = Properties.Resources.U0D0;
            }

            lastCheck = currentCheck;

            if (IsVisible)
            {
                UpdateGraph2();
            }
        }

        private void UpdateSpeeds()
        {
            ulong bytesSentPerSec;
            ulong bytesReceivedPerSec;
            DateTime maxStamp = NetTracker.UpdateNetwork(out bytesReceivedPerSec, out bytesSentPerSec);

            AddData(UpPoints, maxStamp, bytesSentPerSec);
            AddData(DownPoints, maxStamp, bytesReceivedPerSec);

            lastMaxDown = DownPoints.Select(s => s.Bytes).Max();
            lastMaxUp = UpPoints.Select(s => s.Bytes).Max();
        }

        private void AddData(LinkedList<TimeEntry> points, DateTime maxStamp, ulong bytesSentPerSec)
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

            var max = Math.Max(lastMaxDown, lastMaxUp);

            BuildPolygon(UpPoints, max, 255, 24, 32, true);
            BuildPolygon(DownPoints, max, 48, 48, 255,  false);
        }

        private void BuildPolygon(LinkedList<TimeEntry> points, ulong max, byte r, byte g, byte b, bool up)
        {
            if (points.Count == 0)
                return;

            var p = new Polygon();
                        
            double bottom = picGraph.ActualHeight;
            double right = picGraph.ActualWidth;

            var lastTime = points.Last.Value.TimeStamp;

            double elapsed = (lastTime - points.First.Value.TimeStamp).TotalSeconds;

            double scale = 1.0;
            if (elapsed > 0 && elapsed < picGraph.ActualWidth)
                scale = picGraph.ActualWidth / elapsed;
            
            for (var current = points.Last; current != null; current = current.Previous)
            {
                double td = (lastTime - current.Value.TimeStamp).TotalSeconds;

                double xx = (right - td * scale);

                var y = current.Value.Bytes;
                
                var yy = (y * picGraph.ActualHeight / max);

                if(up)
                    yy = bottom - yy;

                p.Points.Add(new Point(xx, yy));
            }

            if(up)
                p.Points.Add(new Point(right, bottom));
            else
                p.Points.Add(new Point(right, 0));

            p.Fill = new SolidColorBrush(Color.FromArgb(255, r, g, b));
            picGraph.Children.Add(p);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SettingsManager.WriteSettings();
        }
    }
}
