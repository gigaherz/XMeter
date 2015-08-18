namespace XMeter
{
    partial class XMeterDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.picGraph = new System.Windows.Forms.PictureBox();
            this.lbStartTime = new System.Windows.Forms.Label();
            this.lbEndTime = new System.Windows.Forms.Label();
            this.lbMinSpeed = new System.Windows.Forms.Label();
            this.lbMaxSpeed = new System.Windows.Forms.Label();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toggleDisplayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.startOnLogonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startMinimizedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.nearlyInvisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.seethroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlayedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opaqueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lbUpSpeed = new System.Windows.Forms.Label();
            this.lbDownSpeed = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picGraph)).BeginInit();
            this.trayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // picGraph
            // 
            this.picGraph.BackColor = System.Drawing.Color.Black;
            this.picGraph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picGraph.Location = new System.Drawing.Point(35, 0);
            this.picGraph.Name = "picGraph";
            this.picGraph.Size = new System.Drawing.Size(271, 99);
            this.picGraph.TabIndex = 0;
            this.picGraph.TabStop = false;
            this.picGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.picGraph_Paint);
            // 
            // lbStartTime
            // 
            this.lbStartTime.AutoSize = true;
            this.lbStartTime.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbStartTime.Location = new System.Drawing.Point(33, 102);
            this.lbStartTime.Name = "lbStartTime";
            this.lbStartTime.Size = new System.Drawing.Size(43, 12);
            this.lbStartTime.TabIndex = 1;
            this.lbStartTime.Text = "start time";
            // 
            // lbEndTime
            // 
            this.lbEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbEndTime.AutoSize = true;
            this.lbEndTime.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbEndTime.Location = new System.Drawing.Point(254, 103);
            this.lbEndTime.Name = "lbEndTime";
            this.lbEndTime.Size = new System.Drawing.Size(40, 12);
            this.lbEndTime.TabIndex = 2;
            this.lbEndTime.Text = "end time";
            this.lbEndTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lbMinSpeed
            // 
            this.lbMinSpeed.AutoSize = true;
            this.lbMinSpeed.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMinSpeed.Location = new System.Drawing.Point(19, 78);
            this.lbMinSpeed.Name = "lbMinSpeed";
            this.lbMinSpeed.Size = new System.Drawing.Size(10, 12);
            this.lbMinSpeed.TabIndex = 3;
            this.lbMinSpeed.Text = "0";
            this.lbMinSpeed.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // lbMaxSpeed
            // 
            this.lbMaxSpeed.AutoSize = true;
            this.lbMaxSpeed.Font = new System.Drawing.Font("Segoe UI", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMaxSpeed.Location = new System.Drawing.Point(12, 0);
            this.lbMaxSpeed.Name = "lbMaxSpeed";
            this.lbMaxSpeed.Size = new System.Drawing.Size(10, 12);
            this.lbMaxSpeed.TabIndex = 4;
            this.lbMaxSpeed.Text = "0";
            this.lbMaxSpeed.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // tmrUpdate
            // 
            this.tmrUpdate.Enabled = true;
            this.tmrUpdate.Tick += new System.EventHandler(this.tmrUpdate_Tick);
            // 
            // trayIcon
            // 
            this.trayIcon.ContextMenuStrip = this.trayMenu;
            this.trayIcon.Text = "notifyIcon1";
            this.trayIcon.Visible = true;
            this.trayIcon.DoubleClick += new System.EventHandler(this.trayIcon_DoubleClick);
            // 
            // trayMenu
            // 
            this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toggleDisplayToolStripMenuItem,
            this.toolStripSeparator1,
            this.startOnLogonToolStripMenuItem,
            this.startMinimizedToolStripMenuItem,
            this.toolStripMenuItem2,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.trayMenu.Name = "trayMenu";
            this.trayMenu.Size = new System.Drawing.Size(158, 148);
            // 
            // toggleDisplayToolStripMenuItem
            // 
            this.toggleDisplayToolStripMenuItem.Name = "toggleDisplayToolStripMenuItem";
            this.toggleDisplayToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.toggleDisplayToolStripMenuItem.Text = "&Toggle Display";
            this.toggleDisplayToolStripMenuItem.Click += new System.EventHandler(this.toggleDisplayToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(154, 6);
            // 
            // startOnLogonToolStripMenuItem
            // 
            this.startOnLogonToolStripMenuItem.Name = "startOnLogonToolStripMenuItem";
            this.startOnLogonToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.startOnLogonToolStripMenuItem.Text = "Start on &Logon";
            this.startOnLogonToolStripMenuItem.Visible = false;
            this.startOnLogonToolStripMenuItem.Click += new System.EventHandler(this.startOnLogonToolStripMenuItem_Click);
            // 
            // startMinimizedToolStripMenuItem
            // 
            this.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem";
            this.startMinimizedToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.startMinimizedToolStripMenuItem.Text = "Start &Minimized";
            this.startMinimizedToolStripMenuItem.Click += new System.EventHandler(this.startMinimizedToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nearlyInvisibleToolStripMenuItem,
            this.toolStripMenuItem3,
            this.seethroughToolStripMenuItem,
            this.overlayedToolStripMenuItem,
            this.opaqueToolStripMenuItem});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(157, 22);
            this.toolStripMenuItem2.Text = "Transparency";
            // 
            // nearlyInvisibleToolStripMenuItem
            // 
            this.nearlyInvisibleToolStripMenuItem.Name = "nearlyInvisibleToolStripMenuItem";
            this.nearlyInvisibleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.nearlyInvisibleToolStripMenuItem.Text = "10%";
            this.nearlyInvisibleToolStripMenuItem.Click += new System.EventHandler(this.nearlyInvisibleToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem3.Text = "30%";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // seethroughToolStripMenuItem
            // 
            this.seethroughToolStripMenuItem.Name = "seethroughToolStripMenuItem";
            this.seethroughToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.seethroughToolStripMenuItem.Text = "50%";
            this.seethroughToolStripMenuItem.Click += new System.EventHandler(this.seethroughToolStripMenuItem_Click);
            // 
            // overlayedToolStripMenuItem
            // 
            this.overlayedToolStripMenuItem.Name = "overlayedToolStripMenuItem";
            this.overlayedToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.overlayedToolStripMenuItem.Text = "90%";
            this.overlayedToolStripMenuItem.Click += new System.EventHandler(this.overlayedToolStripMenuItem_Click);
            // 
            // opaqueToolStripMenuItem
            // 
            this.opaqueToolStripMenuItem.Name = "opaqueToolStripMenuItem";
            this.opaqueToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.opaqueToolStripMenuItem.Text = "Opaque";
            this.opaqueToolStripMenuItem.Click += new System.EventHandler(this.opaqueToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(154, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lbUpSpeed
            // 
            this.lbUpSpeed.AutoSize = true;
            this.lbUpSpeed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbUpSpeed.Location = new System.Drawing.Point(106, 101);
            this.lbUpSpeed.Name = "lbUpSpeed";
            this.lbUpSpeed.Size = new System.Drawing.Size(37, 15);
            this.lbUpSpeed.TabIndex = 5;
            this.lbUpSpeed.Text = "label1";
            // 
            // lbDownSpeed
            // 
            this.lbDownSpeed.AutoSize = true;
            this.lbDownSpeed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbDownSpeed.Location = new System.Drawing.Point(149, 101);
            this.lbDownSpeed.Name = "lbDownSpeed";
            this.lbDownSpeed.Size = new System.Drawing.Size(37, 15);
            this.lbDownSpeed.TabIndex = 6;
            this.lbDownSpeed.Text = "label2";
            // 
            // XMeterDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(306, 124);
            this.Controls.Add(this.lbDownSpeed);
            this.Controls.Add(this.lbUpSpeed);
            this.Controls.Add(this.lbMaxSpeed);
            this.Controls.Add(this.lbMinSpeed);
            this.Controls.Add(this.lbEndTime);
            this.Controls.Add(this.lbStartTime);
            this.Controls.Add(this.picGraph);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "XMeterDisplay";
            this.ShowInTaskbar = false;
            this.Text = "XMeter Status display";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.XMeterDisplay_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.XMeterDisplay_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picGraph)).EndInit();
            this.trayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picGraph;
        private System.Windows.Forms.Label lbStartTime;
        private System.Windows.Forms.Label lbEndTime;
        private System.Windows.Forms.Label lbMinSpeed;
        private System.Windows.Forms.Label lbMaxSpeed;
        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayMenu;
        private System.Windows.Forms.ToolStripMenuItem toggleDisplayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem startMinimizedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startOnLogonToolStripMenuItem;
        private System.Windows.Forms.Label lbUpSpeed;
        private System.Windows.Forms.Label lbDownSpeed;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem nearlyInvisibleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem seethroughToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overlayedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem opaqueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
    }
}

