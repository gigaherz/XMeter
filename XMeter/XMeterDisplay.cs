using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace XMeter
{
    public partial class XMeterDisplay : Form
    {
        bool firstUpdate = true;

        private bool startMinimized = true;
        private bool startOnLogon = true;
        bool realClosing;

        private readonly Meter meter = new Meter();

        public XMeterDisplay()
        {
            InitializeComponent();

            meter.UpdateSpeeds();
            
            ReadSettings();
        }

        private void ReadSettings()
        {
            startMinimized = RegistrySettings.GetConfig(RegistrySettings.ConfigKey.StartMinimized, startMinimized);
            Opacity = RegistrySettings.GetConfig(RegistrySettings.ConfigKey.WindowOpacity, (int)(Opacity*255)) / 255.0;
            startOnLogon = RegistrySettings.StartupState;
        }

        private void WriteSettings()
        {
            RegistrySettings.SetConfig(RegistrySettings.ConfigKey.StartMinimized, startMinimized);
            RegistrySettings.SetConfig(RegistrySettings.ConfigKey.WindowOpacity, (int)(Opacity * 255));
            RegistrySettings.StartupState = startOnLogon;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbMinSpeed.Text = "0 Bytes/s";
            lbMaxSpeed.Text = "0 Bytes/s";
            lbStartTime.Text = DateTime.Now.AddSeconds(-1).ToString("HH:mm:ss");
            lbEndTime.Text = DateTime.Now.ToString("HH:mm:ss");

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
            int bMargin = Math.Max(lbStartTime.Height, lbEndTime.Height);

            int tSpace = ClientSize.Height - bMargin;

            lbMaxSpeed.Location = new Point(lMargin - lbMaxSpeed.Width, 0);
            lbMinSpeed.Location = new Point(lMargin - lbMinSpeed.Width, tSpace - lbMinSpeed.Height);

            lbStartTime.Location = new Point(lMargin, tSpace);
            lbEndTime.Location = new Point(ClientSize.Width - lbEndTime.Width, tSpace);

            int rSpace = ClientSize.Width - lMargin;

            picGraph.SetBounds(lMargin, 0, rSpace, tSpace);
        }

        private void UpdateGraph()
        {
            picGraph.Refresh();
        }
        
        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (firstUpdate)
            {
                firstUpdate = false;

                if (startMinimized)
                    Visible = false;
            }

            if(!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            meter.UpdateSpeeds();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var last = meter.DataPoints.Last();
            var first = meter.DataPoints.First();

            lbMinSpeed.Text = meter.LastMinSpeed.ToString();
            lbMaxSpeed.Text = meter.LastMaxSpeed.ToString();
            lbStartTime.Text = first.TimeStamp.ToString("HH:mm:ss");
            lbEndTime.Text = last.TimeStamp.ToString("HH:mm:ss");

            UpdateLayout();
            UpdateGraph();

            bool sendActivity = (last.UploadSpeed.Bytes > 0);
            bool recvActivity = (last.DownloadSpeed.Bytes > 0);

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

            string title = string.Format("Up: {0}; Down: {1}", last.UploadSpeed, last.DownloadSpeed);

            Text = string.Format("XMeter - {0}", title);
            trayIcon.Text = title;
        }
        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var gSize = picGraph.ClientSize;

            if (meter.LastMaxSpeed <= meter.LastMinSpeed)
                return;

            if (meter.DataPoints.Count == 0)
                return;

            var pb = new SolidBrush(Color.FromArgb(255, 48, 48, 255));
            var pg = new SolidBrush(Color.FromArgb(255, 32, 255, 64));
            var pr = new SolidBrush(Color.FromArgb(255, 255, 24, 32));

            var tt = (meter.LastMaxSpeed - meter.LastMinSpeed).Bytes;

            const int top = 0;
            int bottom = gSize.Height - 1;

            var last = meter.DataPoints.Last();

            var xLast = gSize.Width;
            ulong iMaxSend = last.UploadSpeed.Bytes;
            ulong iMaxRecv = last.DownloadSpeed.Bytes;

            foreach (var current in meter.DataPoints.Reverse())
            {
                var td = Math.Round((last.TimeStamp - current.TimeStamp).TotalSeconds) + 1;
                var xCurrent = (int) Math.Round(gSize.Width - td, 0);
                if (xCurrent < 0)
                    break;

                iMaxSend = Math.Max(current.UploadSpeed.Bytes, iMaxSend);
                iMaxRecv = Math.Max(current.DownloadSpeed.Bytes, iMaxRecv);

                if(xCurrent == xLast)
                    continue;

                var midBottom = bottom - (int)(iMaxSend * (uint)gSize.Height / tt);
                var midTop = top + (int)(iMaxRecv * (uint)gSize.Height / tt);

                if (midBottom < midTop)
                {
                    g.FillRectangle(pg, new Rectangle(
                                   new Point(xCurrent, midTop),
                                   new Size(xLast - xCurrent, midBottom - midTop)));
                    
                    int t = midBottom;
                    midBottom = midTop;
                    midTop = t;
                }
                
                g.FillRectangle(pb, new Rectangle(
                    new Point(xCurrent, top),
                    new Size(xLast - xCurrent, midTop - top)));

                g.FillRectangle(pr, new Rectangle(
                    new Point(xCurrent, midBottom),
                    new Size(xLast - xCurrent, bottom - midBottom)));

                iMaxSend = current.UploadSpeed.Bytes;
                iMaxRecv = current.DownloadSpeed.Bytes;

                xLast = xCurrent;
            }

            pr.Dispose();
            pg.Dispose();
            pb.Dispose();
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
            {
                while (backgroundWorker1.IsBusy)
                {
                    Thread.Sleep(100);
                }

                meter.Dispose();

                return;
            }

            e.Cancel = true;
            Visible = false;
        }

        private void nearlyInvisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.10;
            WriteSettings();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            Opacity = 0.30;
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

    }
}
