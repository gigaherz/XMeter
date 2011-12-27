//#define SHRINK

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Management;
using Microsoft.Win32;

namespace XMeter
{
    public partial class XMeterDisplay : Form
    {
        private const int MaxSecondSpan = 3600;

        readonly List<Tuple<ulong,ulong,ulong>> timeStamps = new List<Tuple<ulong, ulong, ulong>>();

        ulong lastMinSpeed;
        ulong lastMaxSpeed;

        readonly Dictionary<string, ulong> prevLastSend = new Dictionary<string, ulong>();
        readonly Dictionary<string, ulong> prevLastRecv = new Dictionary<string, ulong>();
        readonly ManagementObjectSearcher searcher =
            new ManagementObjectSearcher(
                "SELECT Name, BytesReceivedPerSec, BytesSentPerSec, Timestamp_Sys100NS"+
                " FROM Win32_PerfRawData_Tcpip_NetworkInterface");

        int currentSeconds;

        bool firstUpdate = true;

        DateTime lastCheck;


        bool startMinimized;
        bool startOnLogon;
        bool realClosing;

        public XMeterDisplay()
        {
            InitializeComponent();

            UpdateSpeeds(TimeSpan.FromSeconds(1));

            currentSeconds = 0;

            ReadSettings();
        }

        private void ReadSettings()
        {
            try
            {
                var value = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "StartMinimized", -1);
                if (value < 0)
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "StartMinimized", 0);
                startMinimized = value > 0;
            }
            catch (Exception)
            {
                startMinimized = false;
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "StartMinimized", 0);
            }

            try
            {
                var value = (int)Registry.GetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", -1);
                if (value < 0)
                {
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", 100);
                    value = 100;
                }
                Opacity = value / 100.0;
            }
            catch (Exception)
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", 100);
            }

            //object path = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Current Version\\Run","XMeter");

            //if (path != null)
            //{
            //    if(Application.ExecutablePath == (string)path)
            //        startOnLogon = true;
            //}
        }

        private void WriteSettings()
        {
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "StartMinimized", startMinimized ? 1 : 0);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\XMeter", "WindowOpacity", (int)(Opacity*100));

            //object path = Registry.CurrentUser.GetValue("Software\\Microsoft\\Windows\\Current Version\\Run","XMeter");

            //if ((path != null) && !startOnLogon)
            //{
            //    Registry.DeleteValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Current Version\\Run","XMeter");
            //}

            //if(startOnLogon)
            //    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\Current Version\\Run","XMeter", Application.ExecutablePath);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbMinSpeed.Text = "0 Bytes/s";
            lbMaxSpeed.Text = "0 Bytes/s";
            lbStartTime.Text = DateTime.Now.AddSeconds(-1).ToString("HH:mm:ss");
            lbEndTime.Text = DateTime.Now.ToString("HH:mm:ss");
            lbUpSpeed.Text = "0 Bytes/s";
            lbDownSpeed.Text = "0 Bytes/s";
            UpdateLayout();
            trayIcon.Icon = Properties.Resources.U0D0;
            trayIcon.Text = "Initializing...";
            startMinimizedToolStripMenuItem.Checked = startMinimized;
            startOnLogonToolStripMenuItem.Checked = startOnLogon;
            if (startMinimized)
                Visible = false;
        }

        private void UpdateLayout()
        {
            int lMargin = Math.Max(lbMaxSpeed.Width, lbMinSpeed.Width);
            int bMargin = Math.Max(Math.Max(lbUpSpeed.Height, lbDownSpeed.Height), Math.Max(lbStartTime.Height, lbEndTime.Height));

            int tSpace = ClientSize.Height - bMargin;

            lbMaxSpeed.Location = new Point(lMargin - lbMaxSpeed.Width, 0);
            lbMinSpeed.Location = new Point(lMargin - lbMinSpeed.Width, tSpace - lbMinSpeed.Height);

            lbStartTime.Location = new Point(lMargin, tSpace);
            lbEndTime.Location = new Point(ClientSize.Width - lbEndTime.Width, tSpace);

            int rSpace = ClientSize.Width - lMargin;

            picGraph.SetBounds(lMargin, 0, rSpace, tSpace);

            int middle = lMargin + rSpace / 2;

            lbUpSpeed.Location = new Point(middle - lbUpSpeed.Width, tSpace);
            lbDownSpeed.Location = new Point(middle + 1, tSpace);
        }

        private void UpdateGraph()
        {
            picGraph.Refresh();
        }

        private string FormatUSize(ulong bytes)
        {
            double dbytes = bytes;

            if (bytes < 1024)
                return bytes.ToString() + " Bytes/s";

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

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            DateTime currentCheck = DateTime.Now;
            TimeSpan timeDiff = (currentCheck - lastCheck);

            if (firstUpdate)
            {
                firstUpdate = false;

                if (startMinimized)
                    Visible = false;
            }

            if (timeDiff < TimeSpan.FromSeconds(1))
                return;

            UpdateSpeeds(timeDiff);

            double spanSeconds = (timeStamps[currentSeconds - 1].Item1 - timeStamps[0].Item1) / 10000000.0;

            lbMinSpeed.Text = FormatUSize(lastMinSpeed);
            lbMaxSpeed.Text = FormatUSize(lastMaxSpeed);
            lbStartTime.Text = currentCheck.AddSeconds(-spanSeconds).ToString("HH:mm:ss");
            lbEndTime.Text = currentCheck.ToString("HH:mm:ss");
            lbUpSpeed.Text = FormatUSize(timeStamps[currentSeconds - 1].Item3);
            lbDownSpeed.Text = FormatUSize(timeStamps[currentSeconds - 1].Item2);

            UpdateLayout();
            UpdateGraph();

            bool sendActivity = (timeStamps[currentSeconds - 1].Item3 > 0);
            bool recvActivity = (timeStamps[currentSeconds - 1].Item2 > 0);

            if (sendActivity && recvActivity)
            {
                trayIcon.Icon = Properties.Resources.U1D1;
            }
            else if (sendActivity)
            {
                trayIcon.Icon = Properties.Resources.U1D0;
            }
            else if (recvActivity)
            {
                trayIcon.Icon = Properties.Resources.U0D1;
            }
            else
            {
                trayIcon.Icon = Properties.Resources.U0D0;
            }

            trayIcon.Text = "Send: " + lbUpSpeed.Text + "; Receive: " + lbDownSpeed.Text;

            lastCheck = currentCheck;
        }

        private void UpdateSpeeds(TimeSpan timeDiff)
        {
            ulong bytesReceivedTotal = 0;
            ulong bytesSentTotal = 0;

            ulong maxStamp = 0;

            foreach (ManagementObject adapter in searcher.Get())
            {
                var name = (string)adapter["Name"];
                var sent = adapter["BytesReceivedPerSec"];
                var recv = adapter["BytesSentPerSec"];
                var stamp = adapter["Timestamp_Sys100NS"];

                maxStamp = Math.Max(maxStamp,(ulong)stamp);

                // XP seems to have uint32's there, but win7 has uint64's
                ulong curRecv;
                if (recv is uint)
                    curRecv = (uint)recv;
                else
                    curRecv = (ulong)recv;

                ulong lstRecv = curRecv;

                if (prevLastRecv.ContainsKey(name))
                    lstRecv = (uint)prevLastRecv[name];

                bytesReceivedTotal += (curRecv - lstRecv);
                prevLastRecv[name] = curRecv;

                // XP seems to have uint32's there, but win7 has uint64's
                ulong curSend;
                if (recv is uint)
                    curSend = (uint) sent;
                else
                    curSend = (ulong) sent;

                ulong lstSend = curSend;

                if (prevLastSend.ContainsKey(name))
                    lstSend = (uint)prevLastSend[name];

                bytesSentTotal += (curSend - lstSend);
                prevLastSend[name] = curSend;
            }

            var bytesReceivedPerSec = (ulong)(bytesReceivedTotal / timeDiff.TotalSeconds);
            var bytesSentPerSec = (ulong)(bytesSentTotal / timeDiff.TotalSeconds);

            if (currentSeconds == MaxSecondSpan)
            {
                currentSeconds--;
                for (int i = 0; i < currentSeconds; i++)
                {
                    timeStamps[i] = timeStamps[i + 1];
                }
            }

            timeStamps[currentSeconds] = new Tuple<ulong, ulong, ulong>(maxStamp, bytesReceivedPerSec, bytesSentPerSec);
            currentSeconds++;

            ulong minSpeed = Math.Min(timeStamps[0].Item2, timeStamps[0].Item3);
            ulong maxSpeed = Math.Max(timeStamps[0].Item2, timeStamps[0].Item3);

            for (int i = 0; i < currentSeconds; i++)
            {
                minSpeed = Math.Min(minSpeed, Math.Min(timeStamps[i].Item2, timeStamps[i].Item3));
                maxSpeed = Math.Max(maxSpeed, Math.Max(timeStamps[i].Item2, timeStamps[i].Item3));
            }

            lastMaxSpeed = maxSpeed;
            lastMinSpeed = minSpeed;
        }

        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var gSize = picGraph.ClientSize;

            int xOffset = 0;

#if !SHRINK
            int iOffset = 0;
#endif

            if (lastMaxSpeed <= lastMinSpeed)
                return;

            if (gSize.Width > currentSeconds)
            {
                xOffset = gSize.Width - currentSeconds;
                gSize.Width = currentSeconds;
            }

#if !SHRINK
            if (gSize.Width < currentSeconds)
            {
                iOffset = currentSeconds - gSize.Width;
            }
#endif

            int i = 0;
            for (int x = 0; x < gSize.Width; x++)
            {
#if SHRINK
                uint iTarget = (uint)Math.Min(currentSeconds, x * currentSeconds / GSize.Width);
#endif

#if SHRINK
                ulong iMaxSend = lastMinSpeed;
                ulong iMaxRecv = lastMinSpeed;
                while ((i+iOffset) < iTarget)
                {
                    iMaxSend = Math.Max(iMaxSend, sendSpeeds[i]);
                    iMaxRecv = Math.Max(iMaxRecv, recvSpeeds[i]);
                    i++;
                }
#else
                ulong iMaxSend = timeStamps[i + iOffset].Item3;
                ulong iMaxRecv = timeStamps[i + iOffset].Item2;
                i++;
#endif

                const int top = 0;
                int bottom = gSize.Height;

                var midBottom = bottom - (int)(iMaxSend * (uint)gSize.Height / (lastMaxSpeed - lastMinSpeed));
                var midTop = top + (int)(iMaxRecv * (uint)gSize.Height / (lastMaxSpeed - lastMinSpeed));

                var pb = new Pen(Color.FromArgb(255, 48, 48, 255));
                var pg = new Pen(Color.FromArgb(255, 32, 255, 64));
                var pr = new Pen(Color.FromArgb(255, 255, 24, 32));

                if (midBottom < midTop)
                {
                    g.DrawLine(pg, xOffset + x, midTop, xOffset + x, midBottom);
                    int t = midBottom;
                    midBottom = midTop;
                    midTop = t;
                }

                g.DrawLine(pb, xOffset + x, top, xOffset + x, midTop);
                g.DrawLine(pr, xOffset + x, bottom, xOffset + x, midBottom);

                pr.Dispose();
                pg.Dispose();
                pb.Dispose();
            }
        }

        private void XMeterDisplay_Resize(object sender, EventArgs e)
        {
            UpdateLayout();
            UpdateGraph();
        }

        private void toggleDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = !Visible;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            realClosing = true;
            Close();
        }

        private void startMinimizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startMinimized = !startMinimized;
            startMinimizedToolStripMenuItem.Checked = startMinimized;
            WriteSettings();
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            Visible = !Visible;
        }

        private void startOnLogonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startOnLogon = !startOnLogon;
            startOnLogonToolStripMenuItem.Checked = startOnLogon;
            WriteSettings();
        }

        private void XMeterDisplay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason != CloseReason.UserClosing) || realClosing) 
                return;

            e.Cancel = true;
            Visible = false;
        }

        private void nearlyInvisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.10;
            WriteSettings();
        }

        private void seethroughToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.50;
            WriteSettings();
        }

        private void overlayedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.90;
            WriteSettings();
        }

        private void opaqueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 1.00;
            WriteSettings();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Opacity = 0.30;
            WriteSettings();
        }
    }
}
