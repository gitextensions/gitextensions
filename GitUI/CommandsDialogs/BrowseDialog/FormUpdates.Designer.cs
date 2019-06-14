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
            this.UpdateLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.linkChangeLog = new System.Windows.Forms.LinkLabel();
            this.btnUpdateNow = new System.Windows.Forms.Button();
            this.linkDirectDownload = new System.Windows.Forms.LinkLabel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // UpdateLabel
            // 
            this.UpdateLabel.AutoSize = true;
            this.UpdateLabel.Location = new System.Drawing.Point(13, 13);
            this.UpdateLabel.Name = "UpdateLabel";
            this.UpdateLabel.Size = new System.Drawing.Size(111, 13);
            this.UpdateLabel.TabIndex = 0;
            this.UpdateLabel.Text = "Searching for updates";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 35);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(424, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 1;
            // 
            // linkChangeLog
            // 
            this.linkChangeLog.AutoSize = true;
            this.linkChangeLog.Location = new System.Drawing.Point(13, 35);
            this.linkChangeLog.Name = "linkChangeLog";
            this.linkChangeLog.Size = new System.Drawing.Size(92, 13);
            this.linkChangeLog.TabIndex = 2;
            this.linkChangeLog.TabStop = true;
            this.linkChangeLog.Text = "Show Change&Log";
            this.linkChangeLog.Visible = false;
            this.linkChangeLog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkChangeLog_LinkClicked);
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.AutoSize = true;
            this.btnUpdateNow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdateNow.Location = new System.Drawing.Point(253, 3);
            this.btnUpdateNow.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(100, 23);
            this.btnUpdateNow.TabIndex = 0;
            this.btnUpdateNow.Text = "&Update Now";
            this.btnUpdateNow.Visible = false;
            this.btnUpdateNow.UseVisualStyleBackColor = true;
            this.btnUpdateNow.Click += new System.EventHandler(this.btnUpdateNow_Click);
            // 
            // linkDirectDownload
            // 
            this.linkDirectDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.linkDirectDownload.AutoSize = true;
            this.linkDirectDownload.Location = new System.Drawing.Point(359, 0);
            this.linkDirectDownload.Name = "linkDirectDownload";
            this.linkDirectDownload.Size = new System.Drawing.Size(86, 29);
            this.linkDirectDownload.TabIndex = 1;
            this.linkDirectDownload.TabStop = true;
            this.linkDirectDownload.Text = "&Direct Download";
            this.linkDirectDownload.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkDirectDownload.Visible = false;
            this.linkDirectDownload.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkDirectDownload_LinkClicked);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.linkDirectDownload);
            this.flowLayoutPanel1.Controls.Add(this.btnUpdateNow);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 69);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 2);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(452, 31);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // FormUpdates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(452, 100);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.linkChangeLog);
            this.Controls.Add(this.UpdateLabel);
            this.Controls.Add(this.progressBar1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormUpdates";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Check for update";
            this.TopMost = true;
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label UpdateLabel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.LinkLabel linkChangeLog;
        private System.Windows.Forms.Button btnUpdateNow;
        private System.Windows.Forms.LinkLabel linkDirectDownload;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}