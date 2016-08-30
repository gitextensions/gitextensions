﻿using System.Windows.Forms;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    partial class FormGitLog
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
            this.TabControl = new System.Windows.Forms.TabControl();
            this.tabPageCommandLog = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.LogItems = new System.Windows.Forms.ListBox();
            this.LogOutput = new System.Windows.Forms.RichTextBox();
            this.tabPageCommandCache = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CommandCacheItems = new System.Windows.Forms.ListBox();
            this.commandCacheOutput = new System.Windows.Forms.RichTextBox();
            this.alwaysOnTopCheckBox = new System.Windows.Forms.CheckBox();
            this.TabControl.SuspendLayout();
            this.tabPageCommandLog.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();

            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabPageCommandCache.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();

            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl
            // 
            this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl.Controls.Add(this.tabPageCommandLog);
            this.TabControl.Controls.Add(this.tabPageCommandCache);
            this.TabControl.Location = new System.Drawing.Point(0, 3);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(659, 442);
            this.TabControl.TabIndex = 1;
            this.TabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            // 
            // tabPageCommandLog
            // 
            this.tabPageCommandLog.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tabPageCommandLog.Controls.Add(this.splitContainer2);
            this.tabPageCommandLog.Location = new System.Drawing.Point(4, 24);
            this.tabPageCommandLog.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageCommandLog.Name = "tabPageCommandLog";
            this.tabPageCommandLog.Size = new System.Drawing.Size(651, 414);
            this.tabPageCommandLog.TabIndex = 0;
            this.tabPageCommandLog.Text = "Command log";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.LogItems);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.LogOutput);
            this.splitContainer2.Size = new System.Drawing.Size(651, 414);
            this.splitContainer2.SplitterDistance = 332;
            this.splitContainer2.TabIndex = 1;
            // 
            // LogItems
            // 
            this.LogItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogItems.FormattingEnabled = true;
            this.LogItems.ItemHeight = 15;
            this.LogItems.Location = new System.Drawing.Point(0, 0);
            this.LogItems.Margin = new System.Windows.Forms.Padding(0);
            this.LogItems.Name = "LogItems";
            this.LogItems.Size = new System.Drawing.Size(651, 332);
            this.LogItems.TabIndex = 0;
            this.LogItems.SelectedIndexChanged += new System.EventHandler(this.LogItems_SelectedIndexChanged);
            // 
            // LogOutput
            // 
            this.LogOutput.BackColor = System.Drawing.SystemColors.ControlLight;
            this.LogOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogOutput.Location = new System.Drawing.Point(0, 0);
            this.LogOutput.Margin = new System.Windows.Forms.Padding(0);
            this.LogOutput.Name = "LogOutput";
            this.LogOutput.Size = new System.Drawing.Size(651, 78);
            this.LogOutput.TabIndex = 0;
            this.LogOutput.Text = "";
            // 
            // tabPageCommandCache
            // 
            this.tabPageCommandCache.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tabPageCommandCache.Controls.Add(this.splitContainer1);
            this.tabPageCommandCache.Location = new System.Drawing.Point(4, 24);
            this.tabPageCommandCache.Margin = new System.Windows.Forms.Padding(0);
            this.tabPageCommandCache.Name = "tabPageCommandCache";
            this.tabPageCommandCache.Size = new System.Drawing.Size(651, 414);
            this.tabPageCommandCache.TabIndex = 1;
            this.tabPageCommandCache.Text = "Command cache";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CommandCacheItems);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.commandCacheOutput);
            this.splitContainer1.Size = new System.Drawing.Size(651, 414);
            this.splitContainer1.SplitterDistance = 183;
            this.splitContainer1.TabIndex = 0;
            // 
            // CommandCacheItems
            // 
            this.CommandCacheItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CommandCacheItems.FormattingEnabled = true;
            this.CommandCacheItems.ItemHeight = 15;
            this.CommandCacheItems.Location = new System.Drawing.Point(0, 0);
            this.CommandCacheItems.Margin = new System.Windows.Forms.Padding(0);
            this.CommandCacheItems.Name = "CommandCacheItems";
            this.CommandCacheItems.Size = new System.Drawing.Size(651, 183);
            this.CommandCacheItems.TabIndex = 0;
            this.CommandCacheItems.SelectedIndexChanged += new System.EventHandler(this.CommandCacheItems_SelectedIndexChanged);
            // 
            // commandCacheOutput
            // 
            this.commandCacheOutput.BackColor = System.Drawing.SystemColors.ControlLight;
            this.commandCacheOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.commandCacheOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandCacheOutput.Location = new System.Drawing.Point(0, 0);
            this.commandCacheOutput.Margin = new System.Windows.Forms.Padding(0);
            this.commandCacheOutput.Name = "commandCacheOutput";
            this.commandCacheOutput.Size = new System.Drawing.Size(651, 227);
            this.commandCacheOutput.TabIndex = 0;
            this.commandCacheOutput.Text = "";
            // 
            // alwaysOnTopCheckBox
            // 
            this.alwaysOnTopCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.alwaysOnTopCheckBox.AutoSize = true;
            this.alwaysOnTopCheckBox.Location = new System.Drawing.Point(4, 449);
            this.alwaysOnTopCheckBox.Name = "alwaysOnTopCheckBox";
            this.alwaysOnTopCheckBox.Size = new System.Drawing.Size(101, 19);
            this.alwaysOnTopCheckBox.TabIndex = 2;
            this.alwaysOnTopCheckBox.Text = "Always on top";
            this.alwaysOnTopCheckBox.UseVisualStyleBackColor = true;
            this.alwaysOnTopCheckBox.CheckedChanged += new System.EventHandler(this.alwaysOnTopCheckBox_CheckedChanged);
            // 
            // FormGitLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(659, 470);
            this.Controls.Add(this.alwaysOnTopCheckBox);
            this.Controls.Add(this.TabControl);
            this.Name = "FormGitLog";
            this.Text = "Log";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GitLogForm_FormClosed);
            this.Load += new System.EventHandler(this.GitLogFormLoad);
            this.TabControl.ResumeLayout(false);
            this.tabPageCommandLog.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();

            this.splitContainer2.ResumeLayout(false);
            this.tabPageCommandCache.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();

            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage tabPageCommandLog;
        private System.Windows.Forms.TabPage tabPageCommandCache;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox commandCacheOutput;
        private System.Windows.Forms.ListBox CommandCacheItems;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox LogItems;
        private System.Windows.Forms.RichTextBox LogOutput;
        private CheckBox alwaysOnTopCheckBox;
    }
}