﻿using System.Windows.Forms;
namespace GitUI.Statistics
{
    partial class FormCommitCount
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
            this.CommitCount = new System.Windows.Forms.RichTextBox();
            this.Loading = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).BeginInit();
            this.SuspendLayout();
            // 
            // CommitCount
            // 
            this.CommitCount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommitCount.Location = new System.Drawing.Point(0, 0);
            this.CommitCount.Name = "CommitCount";
            this.CommitCount.ReadOnly = true;
            this.CommitCount.Size = new System.Drawing.Size(367, 317);
            this.CommitCount.TabIndex = 0;
            this.CommitCount.Text = "";
            // 
            // Loading
            // 
            this.Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Loading.Location = new System.Drawing.Point(0, 0);
            this.Loading.Name = "Loading";
            this.Loading.Size = new System.Drawing.Size(367, 317);
            this.Loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Loading.TabIndex = 1;
            this.Loading.TabStop = false;
            // 
            // FormCommitCount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 317);
            this.Controls.Add(this.Loading);
            this.Controls.Add(this.CommitCount);
            this.Name = "FormCommitCount";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commit count";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCommitCountFormClosing);
            this.Load += new System.EventHandler(this.FormCommitCountLoad);
            ((System.ComponentModel.ISupportInitialize)(this.Loading)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox CommitCount;
        private System.Windows.Forms.PictureBox Loading;
    }
}