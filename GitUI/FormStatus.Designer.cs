﻿using System.Windows.Forms;
namespace GitUI
{
    partial class FormStatus
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
            this.Ok = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.Output = new System.Windows.Forms.RichTextBox();
            this.SuccessImage = new System.Windows.Forms.PictureBox();
            this.ErrorImage = new System.Windows.Forms.PictureBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.KeepDialogOpen = new System.Windows.Forms.CheckBox();
            this.Abort = new System.Windows.Forms.Button();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SuccessImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorImage)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(3, 2);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 0;
            this.Ok.Text = "OK";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(10, 10);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(607, 241);
            this.splitContainer1.SplitterDistance = 212;
            this.splitContainer1.TabIndex = 2;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.Output);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.SuccessImage);
            this.splitContainer5.Panel2.Controls.Add(this.ErrorImage);
            this.splitContainer5.Panel2Collapsed = true;
            this.splitContainer5.Size = new System.Drawing.Size(607, 212);
            this.splitContainer5.SplitterDistance = 548;
            this.splitContainer5.TabIndex = 3;
            // 
            // Output
            // 
            this.Output.BackColor = System.Drawing.SystemColors.Window;
            this.Output.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Output.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Output.Location = new System.Drawing.Point(0, 0);
            this.Output.Name = "Output";
            this.Output.ReadOnly = true;
            this.Output.Size = new System.Drawing.Size(607, 212);
            this.Output.TabIndex = 0;
            this.Output.Text = "";
            // 
            // SuccessImage
            // 
            this.SuccessImage.ErrorImage = global::GitUI.Properties.Resources.error;
            this.SuccessImage.Image = global::GitUI.Properties.Resources.success1;
            this.SuccessImage.InitialImage = global::GitUI.Properties.Resources.success1;
            this.SuccessImage.Location = new System.Drawing.Point(3, 3);
            this.SuccessImage.Name = "SuccessImage";
            this.SuccessImage.Size = new System.Drawing.Size(47, 46);
            this.SuccessImage.TabIndex = 2;
            this.SuccessImage.TabStop = false;
            this.SuccessImage.Visible = false;
            // 
            // ErrorImage
            // 
            this.ErrorImage.Image = global::GitUI.Properties.Resources.error;
            this.ErrorImage.Location = new System.Drawing.Point(2, 3);
            this.ErrorImage.Name = "ErrorImage";
            this.ErrorImage.Size = new System.Drawing.Size(45, 44);
            this.ErrorImage.TabIndex = 1;
            this.ErrorImage.TabStop = false;
            this.ErrorImage.Visible = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.Ok);
            this.splitContainer2.Size = new System.Drawing.Size(607, 25);
            this.splitContainer2.SplitterDistance = 523;
            this.splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.ProgressBar);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(523, 25);
            this.splitContainer3.SplitterDistance = 303;
            this.splitContainer3.TabIndex = 0;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProgressBar.Location = new System.Drawing.Point(0, 0);
            this.ProgressBar.MarqueeAnimationSpeed = 1;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(303, 25);
            this.ProgressBar.Step = 50;
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ProgressBar.TabIndex = 3;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.KeepDialogOpen);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.Abort);
            this.splitContainer4.Size = new System.Drawing.Size(216, 25);
            this.splitContainer4.SplitterDistance = 134;
            this.splitContainer4.TabIndex = 0;
            // 
            // KeepDialogOpen
            // 
            this.KeepDialogOpen.AutoSize = true;
            this.KeepDialogOpen.Location = new System.Drawing.Point(3, 5);
            this.KeepDialogOpen.Name = "KeepDialogOpen";
            this.KeepDialogOpen.Size = new System.Drawing.Size(108, 17);
            this.KeepDialogOpen.TabIndex = 3;
            this.KeepDialogOpen.Text = "Keep dialog open";
            this.KeepDialogOpen.ThreeState = true;
            this.KeepDialogOpen.UseVisualStyleBackColor = true;
            this.KeepDialogOpen.CheckStateChanged += new System.EventHandler(this.KeepDialogOpen_CheckStateChanged);
            // 
            // Abort
            // 
            this.Abort.Location = new System.Drawing.Point(3, 2);
            this.Abort.Name = "Abort";
            this.Abort.Size = new System.Drawing.Size(75, 23);
            this.Abort.TabIndex = 3;
            this.Abort.Text = "Abort";
            this.Abort.UseVisualStyleBackColor = true;
            this.Abort.Click += new System.EventHandler(this.Abort_Click);
            // 
            // FormStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(627, 261);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormStatus";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Process";
            this.Load += new System.EventHandler(this.FormStatus_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormStatus_FormClosed);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SuccessImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorImage)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.RichTextBox Output;
        private System.Windows.Forms.PictureBox ErrorImage;
        private System.Windows.Forms.PictureBox SuccessImage;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        protected System.Windows.Forms.Button Ok;
        protected System.Windows.Forms.CheckBox KeepDialogOpen;
        protected System.Windows.Forms.Button Abort;
    }
}