﻿namespace AutoCheckForUpdates
{
    partial class Updates
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Updates));
            this.closeButton = new System.Windows.Forms.Button();
            this.UpdateLabel = new System.Windows.Forms.Label();
            this.link = new System.Windows.Forms.LinkLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.linkChangeLog = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(487, 81);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 28);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.CloseButtonClick);
            // 
            // UpdateLabel
            // 
            this.UpdateLabel.AutoSize = true;
            this.UpdateLabel.Location = new System.Drawing.Point(17, 16);
            this.UpdateLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.UpdateLabel.Name = "UpdateLabel";
            this.UpdateLabel.Size = new System.Drawing.Size(152, 17);
            this.UpdateLabel.TabIndex = 1;
            this.UpdateLabel.Text = "Searching for updates.";
            // 
            // link
            // 
            this.link.AutoSize = true;
            this.link.Location = new System.Drawing.Point(17, 46);
            this.link.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.link.Name = "link";
            this.link.Size = new System.Drawing.Size(54, 17);
            this.link.TabIndex = 2;
            this.link.TabStop = true;
            this.link.Text = "version";
            this.link.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClicked);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(21, 46);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(565, 28);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 3;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // linkChangeLog
            // 
            this.linkChangeLog.AutoSize = true;
            this.linkChangeLog.Location = new System.Drawing.Point(18, 87);
            this.linkChangeLog.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkChangeLog.Name = "linkChangeLog";
            this.linkChangeLog.Size = new System.Drawing.Size(119, 17);
            this.linkChangeLog.TabIndex = 2;
            this.linkChangeLog.TabStop = true;
            this.linkChangeLog.Text = "Show ChangeLog";
            this.linkChangeLog.Visible = false;
            this.linkChangeLog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkChangeLog_LinkClicked);
            // 
            // Updates
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 123);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.linkChangeLog);
            this.Controls.Add(this.link);
            this.Controls.Add(this.UpdateLabel);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Updates";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Check for update";
            this.Shown += new System.EventHandler(this.UpdatesShown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label UpdateLabel;
        private System.Windows.Forms.LinkLabel link;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.LinkLabel linkChangeLog;
    }
}