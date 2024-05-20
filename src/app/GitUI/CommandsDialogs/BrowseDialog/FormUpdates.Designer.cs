namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormUpdates
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
            if (disposing && (components is not null))
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
            UpdateLabel = new Label();
            progressBar1 = new ProgressBar();
            linkChangeLog = new LinkLabel();
            btnUpdateNow = new Button();
            linkDirectDownload = new LinkLabel();
            tlpnlContent = new TableLayoutPanel();
            MainPanel.SuspendLayout();
            ControlsPanel.SuspendLayout();
            tlpnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Controls.Add(tlpnlContent);
            MainPanel.Size = new Size(456, 73);
            // 
            // ControlsPanel
            // 
            ControlsPanel.Controls.Add(linkDirectDownload);
            ControlsPanel.Controls.Add(btnUpdateNow);
            // 
            // UpdateLabel
            // 
            UpdateLabel.AutoSize = true;
            UpdateLabel.Dock = DockStyle.Fill;
            UpdateLabel.Location = new Point(3, 0);
            UpdateLabel.Name = "UpdateLabel";
            UpdateLabel.Size = new Size(426, 15);
            UpdateLabel.TabIndex = 0;
            UpdateLabel.Text = "Searching for updates";
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Fill;
            progressBar1.Location = new Point(3, 18);
            progressBar1.MinimumSize = new Size(0, 20);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(426, 20);
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.TabIndex = 1;
            // 
            // linkChangeLog
            // 
            linkChangeLog.AutoSize = true;
            linkChangeLog.Dock = DockStyle.Fill;
            linkChangeLog.Location = new Point(3, 41);
            linkChangeLog.Name = "linkChangeLog";
            linkChangeLog.Size = new Size(426, 15);
            linkChangeLog.TabIndex = 2;
            linkChangeLog.TabStop = true;
            linkChangeLog.Text = "Show Change&Log";
            linkChangeLog.Visible = false;
            linkChangeLog.LinkClicked += linkChangeLog_LinkClicked;
            // 
            // btnUpdateNow
            // 
            btnUpdateNow.AutoSize = true;
            btnUpdateNow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnUpdateNow.Location = new Point(242, 8);
            btnUpdateNow.MinimumSize = new Size(100, 0);
            btnUpdateNow.Name = "btnUpdateNow";
            btnUpdateNow.Size = new Size(100, 25);
            btnUpdateNow.TabIndex = 0;
            btnUpdateNow.Text = "&Update Now";
            btnUpdateNow.UseVisualStyleBackColor = true;
            btnUpdateNow.Visible = false;
            btnUpdateNow.Click += btnUpdateNow_Click;
            // 
            // linkDirectDownload
            // 
            linkDirectDownload.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            linkDirectDownload.AutoSize = true;
            linkDirectDownload.Location = new Point(348, 5);
            linkDirectDownload.Name = "linkDirectDownload";
            linkDirectDownload.Size = new Size(95, 31);
            linkDirectDownload.TabIndex = 1;
            linkDirectDownload.TabStop = true;
            linkDirectDownload.Text = "&Direct Download";
            linkDirectDownload.TextAlign = ContentAlignment.MiddleCenter;
            linkDirectDownload.Visible = false;
            linkDirectDownload.LinkClicked += linkDirectDownload_LinkClicked;
            // 
            // tlpnlContent
            // 
            tlpnlContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpnlContent.ColumnStyles.Add(new ColumnStyle());
            tlpnlContent.Controls.Add(UpdateLabel, 0, 0);
            tlpnlContent.Controls.Add(progressBar1, 0, 1);
            tlpnlContent.Controls.Add(linkChangeLog, 0, 2);
            tlpnlContent.Dock = DockStyle.Fill;
            tlpnlContent.Location = new Point(12, 12);
            tlpnlContent.Margin = new Padding(0);
            tlpnlContent.Name = "tlpnlContent";
            tlpnlContent.RowCount = 4;
            tlpnlContent.RowStyles.Add(new RowStyle());
            tlpnlContent.RowStyles.Add(new RowStyle());
            tlpnlContent.RowStyles.Add(new RowStyle());
            tlpnlContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpnlContent.Size = new Size(432, 49);
            tlpnlContent.TabIndex = 3;
            // 
            // FormUpdates
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(456, 114);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormUpdates";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Check for update";
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            ControlsPanel.ResumeLayout(false);
            ControlsPanel.PerformLayout();
            tlpnlContent.ResumeLayout(false);
            tlpnlContent.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label UpdateLabel;
        private ProgressBar progressBar1;
        private LinkLabel linkChangeLog;
        private Button btnUpdateNow;
        private LinkLabel linkDirectDownload;
        private TableLayoutPanel tlpnlContent;
    }
}
