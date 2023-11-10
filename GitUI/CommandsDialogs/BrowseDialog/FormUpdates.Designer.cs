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
            flowLayoutPanel1 = new FlowLayoutPanel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // UpdateLabel
            // 
            UpdateLabel.AutoSize = true;
            UpdateLabel.Location = new Point(13, 13);
            UpdateLabel.Name = "UpdateLabel";
            UpdateLabel.Size = new Size(111, 13);
            UpdateLabel.TabIndex = 0;
            UpdateLabel.Text = "Searching for updates";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(16, 35);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(424, 23);
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.TabIndex = 1;
            // 
            // linkChangeLog
            // 
            linkChangeLog.AutoSize = true;
            linkChangeLog.Location = new Point(13, 35);
            linkChangeLog.Name = "linkChangeLog";
            linkChangeLog.Size = new Size(92, 13);
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
            btnUpdateNow.Location = new Point(253, 3);
            btnUpdateNow.MinimumSize = new Size(100, 0);
            btnUpdateNow.Name = "btnUpdateNow";
            btnUpdateNow.Size = new Size(100, 23);
            btnUpdateNow.TabIndex = 0;
            btnUpdateNow.Text = "&Update Now";
            btnUpdateNow.Visible = false;
            btnUpdateNow.UseVisualStyleBackColor = true;
            btnUpdateNow.Click += btnUpdateNow_Click;
            // 
            // linkDirectDownload
            // 
            linkDirectDownload.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            linkDirectDownload.AutoSize = true;
            linkDirectDownload.Location = new Point(359, 0);
            linkDirectDownload.Name = "linkDirectDownload";
            linkDirectDownload.Size = new Size(86, 29);
            linkDirectDownload.TabIndex = 1;
            linkDirectDownload.TabStop = true;
            linkDirectDownload.Text = "&Direct Download";
            linkDirectDownload.TextAlign = ContentAlignment.MiddleCenter;
            linkDirectDownload.Visible = false;
            linkDirectDownload.LinkClicked += linkDirectDownload_LinkClicked;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(linkDirectDownload);
            flowLayoutPanel1.Controls.Add(btnUpdateNow);
            flowLayoutPanel1.Dock = DockStyle.Bottom;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new Point(0, 69);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(0, 0, 4, 2);
            flowLayoutPanel1.Size = new Size(452, 31);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // FormUpdates
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(452, 100);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(linkChangeLog);
            Controls.Add(UpdateLabel);
            Controls.Add(progressBar1);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormUpdates";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Check for update";
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label UpdateLabel;
        private ProgressBar progressBar1;
        private LinkLabel linkChangeLog;
        private Button btnUpdateNow;
        private LinkLabel linkDirectDownload;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}