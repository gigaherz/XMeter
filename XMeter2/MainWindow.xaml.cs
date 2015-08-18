using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace XMeter2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MaxSecondSpan = 3600;

        TaskbarIcon notificationIcon = new TaskbarIcon();

        class TimeEntry
        {
            public readonly DateTime TimeStamp;
            public readonly ulong UpBytes;
            public readonly ulong DownBytes;

            public TimeEntry(DateTime t, ulong u, ulong d)
            {
                TimeStamp = t;
                UpBytes = u;
                DownBytes = d;
            }
        }

        readonly LinkedList<TimeEntry> timeStamps = new LinkedList<TimeEntry>();

        readonly Dictionary<string, ulong> prevLastSend = new Dictionary<string, ulong>();
        readonly Dictionary<string, ulong> prevLastRecv = new Dictionary<string, ulong>();
        readonly Dictionary<string, DateTime> prevLastStamp = new Dictionary<string, DateTime>();
        readonly ManagementObjectSearcher searcher =
            new ManagementObjectSearcher(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS" +
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");

        ulong lastMinSpeed;
        ulong lastMaxSpeed;

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

            mnuStartOnLogon.IsChecked = SettingsManager.StartOnLogon;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.IsEnabled = true;
            
            Visibility = Visibility.Hidden;

            UpdateSpeeds();
        }

        private void NotificationIcon_LeftClick(object obj)
        {
            Visibility = Visibility.Visible;
            Left = SystemParameters.WorkArea.Width - Width;
            Top = SystemParameters.WorkArea.Height - Height;

            Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGraph();
        }
        
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void StartOnLogon_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.StartOnLogon = !SettingsManager.StartOnLogon;
            mnuStartOnLogon.IsChecked = SettingsManager.StartOnLogon;
            SettingsManager.WriteSettings();
        }
        
        private void Opacity10_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.10;
            SettingsManager.WriteSettings();
        }

        private void Opacity30_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.30;
            SettingsManager.WriteSettings();
        }

        private void Opacity50_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.50;
            SettingsManager.WriteSettings();
        }

        private void Opacity90_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.90;
            SettingsManager.WriteSettings();
        }

        private void Opaque_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 1.00;
            SettingsManager.WriteSettings();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private static string FormatUSize(ulong bytes)
        {
            double dbytes = bytes;

            if (bytes < 1024)
                return bytes.ToString() + " B/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return dbytes.ToString("#0.00") + " KB/s";

            dbytes /= 1024.0;

            if (dbytes < 1024)
                return dbytes.ToString("#0.00") + " MBs/s";

            dbytes /= 1024.0;

            // Maybe... someday...
            return dbytes.ToString("#0.00") + " GBs/s";
        }

        private void Timer_Tick(object o, EventArgs e)
        {
            DateTime currentCheck = DateTime.Now;
            
            UpdateSpeeds();

            double spanSeconds = (timeStamps.Last.Value.TimeStamp - timeStamps.First.Value.TimeStamp).TotalSeconds;

            lbMinSpeed.Content = FormatUSize(lastMinSpeed);
            lbMaxSpeed.Content = FormatUSize(lastMaxSpeed);
            lbStartTime.Content = currentCheck.AddSeconds(-spanSeconds).ToString("HH:mm:ss");
            lbEndTime.Content = currentCheck.ToString("HH:mm:ss");
            lbUpSpeed.Content = FormatUSize(timeStamps.Last.Value.UpBytes);
            lbDownSpeed.Content = FormatUSize(timeStamps.Last.Value.DownBytes);
            
            bool sendActivity = (timeStamps.Last.Value.UpBytes > 0);
            bool recvActivity = (timeStamps.Last.Value.DownBytes > 0);

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

            notificationIcon.ToolTipText = "Send: " + lbUpSpeed.Content + "; Receive: " + lbDownSpeed.Content;

            lastCheck = currentCheck;

            if (IsVisible)
            {
                UpdateGraph();
            }
        }

        private void UpdateSpeeds()
        {
            DateTime maxStamp = DateTime.MinValue;

            ulong bytesReceivedPerSec = 0;
            ulong bytesSentPerSec = 0;

            foreach (ManagementObject adapter in searcher.Get())
            {
                var name = (string)adapter["Name"];
                var sent = adapter["BytesReceivedPerSec"];
                var recv = adapter["BytesSentPerSec"];
                var curStamp = DateTime.FromBinary((long)(ulong)adapter["Timestamp_Sys100NS"]).AddYears(1600);

                if (curStamp > maxStamp)
                    maxStamp = curStamp;

                // XP seems to have uint32's there, but win7 has uint64's
                var curRecv = recv is uint ? (uint)recv : (ulong)recv;
                var curSend = sent is uint ? (uint)sent : (ulong)sent;

                var lstRecv = curRecv;
                var lstSend = curSend;
                var lstStamp = curStamp;

                if (prevLastRecv.ContainsKey(name)) lstRecv = prevLastRecv[name];
                if (prevLastSend.ContainsKey(name)) lstSend = prevLastSend[name];
                if (prevLastStamp.ContainsKey(name)) lstStamp = prevLastStamp[name];

                var diffRecv = (curRecv - lstRecv);
                var diffSend = (curSend - lstSend);
                var diffStamp = (curStamp - lstStamp);

                prevLastRecv[name] = curRecv;
                prevLastSend[name] = curSend;
                prevLastStamp[name] = curStamp;

                if (diffStamp <= TimeSpan.Zero)
                    continue;

                double diffSeconds = diffStamp.TotalSeconds;

                if (diffSeconds > MaxSecondSpan)
                    continue;

                bytesReceivedPerSec += (ulong)(diffRecv / diffSeconds);
                bytesSentPerSec += (ulong)(diffSend / diffSeconds);
            }

            timeStamps.AddLast(new TimeEntry(maxStamp, bytesSentPerSec, bytesReceivedPerSec));

            var totalSpan = timeStamps.Last.Value.TimeStamp - timeStamps.First.Value.TimeStamp;
            while (totalSpan.TotalSeconds > MaxSecondSpan && timeStamps.Count > 1)
            {
                timeStamps.RemoveFirst();
                totalSpan = timeStamps.Last.Value.TimeStamp - timeStamps.First.Value.TimeStamp;
            }

            var minSpeed = Math.Min(timeStamps.First.Value.UpBytes, timeStamps.First.Value.DownBytes);
            var maxSpeed = Math.Max(timeStamps.First.Value.UpBytes, timeStamps.First.Value.DownBytes);

            foreach (var ts in timeStamps)
            {
                minSpeed = Math.Min(minSpeed, Math.Min(ts.UpBytes, ts.DownBytes));
                maxSpeed = Math.Max(maxSpeed, Math.Max(ts.UpBytes, ts.DownBytes));
            }

            lastMaxSpeed = maxSpeed;
            lastMinSpeed = minSpeed;
        }

        private void UpdateGraph()
        {
            var gSize = new Size(picGraph.ActualWidth, picGraph.ActualHeight);

            if (lastMaxSpeed <= lastMinSpeed)
                return;

            if (timeStamps.Count == 0)
                return;

            var pb = new SolidColorBrush(Color.FromArgb(255, 48, 48, 255));
            var pg = new SolidColorBrush(Color.FromArgb(255, 32, 255, 64));
            var pr = new SolidColorBrush(Color.FromArgb(255, 255, 24, 32));

            var tt = (lastMaxSpeed - lastMinSpeed);

            const double top = 0;
            double bottom = gSize.Height - 1;

            double xStart = gSize.Width - 1;
            var tStart = timeStamps.Last.Value.TimeStamp;
            double xLast = xStart + 1;
            ulong iMaxSend = timeStamps.Last.Value.UpBytes;
            ulong iMaxRecv = timeStamps.Last.Value.DownBytes;

            picGraph.Children.Clear();

            for (var current = timeStamps.Last; current != null; current = current.Previous)
            {
                double td = Math.Round((tStart - current.Value.TimeStamp).TotalSeconds);
                double xCurrent = (xStart - td);
                if (xCurrent < 0)
                    break;

                iMaxSend = Math.Max(current.Value.UpBytes, iMaxSend);
                iMaxRecv = Math.Max(current.Value.DownBytes, iMaxRecv);

                if (xCurrent == xLast)
                    continue;

                var midBottom = bottom - (iMaxSend * gSize.Height / tt);
                var midTop = top + (iMaxRecv * gSize.Height / tt);

                if (midBottom < midTop)
                {
                    MakeCanvasRectangle(xCurrent, midTop,
                        xLast - xCurrent, midBottom - midTop, pg);

                    double t = midBottom;
                    midBottom = midTop;
                    midTop = t;
                }

               MakeCanvasRectangle(xCurrent, top,
                    xLast - xCurrent, midTop - top, pr);

                MakeCanvasRectangle(xCurrent, midBottom,
                    xLast - xCurrent, bottom - midBottom, pb);

                iMaxSend = current.Value.UpBytes;
                iMaxRecv = current.Value.DownBytes;

                xLast = xCurrent;
            }
        }

        void MakeCanvasRectangle(double x, double y, double w, double h, Brush b)
        {
            if (w < 0 || h < 0)
                return;

            var r = new Rectangle();
            Canvas.SetLeft(r, x);
            Canvas.SetTop(r, y);
            r.Width = w;
            r.Height = h;
            r.Fill = b;
            picGraph.Children.Add(r);
        }
    }
}
